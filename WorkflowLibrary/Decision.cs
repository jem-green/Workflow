using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TracerLibrary;
using System.Threading;
using System.Diagnostics;

namespace WorkflowLibrary 
{
    /// <summary>
    /// 
    /// </summary>
    public class Decision: Orchestration, IActivity, ICloneable
    {
        #region Fields

        //
        private static int decisionId;
        private IndexCollection<string, Item> _items;
        
        #endregion
        #region Constructors
         
        public Decision() : base()
        {
            _items = new IndexCollection<string, Item>();
            decisionId = decisionId + 1;
            _id = "decision_" + decisionId.ToString();
        }
        
        public Decision(string Id) : base(Id)
        {
            _items = new IndexCollection<string, Item>();
            _id = Id;
            if (Id.StartsWith("decision_"))
            {
                if (decisionId < Convert.ToInt16(Id.Substring(9)))
                {
                    decisionId = Convert.ToInt16(Id.Substring(9));
                }
            }
        }

        #endregion Constructors
        #region Properties

        #endregion Properties
        #region Methods

        public override void Start()
        {

            // Once the token has arrived then the state goes to ready

            bool thrown = false;
            Token token = new Token(_sessionId);
            token.AddData("Token", false);
            string caught = "";
            int process = 0;
            cancel = false;
            terminate = false;
            Groupings replace = new Groupings();

            Debug.WriteLine("In Start() " + this._description + "(" + this.ID + ")");

            // Does it seem sensible to 


            do
            {
                if (@catch.Count > 0)
                {
                    do
                    {
                        foreach (Node node in @catch)
                        {
                            if ((bool)token.SelectData("Token") == false)
                            {
                                token = node.Link.GetItem();
                                caught = node.Id;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if ((bool)token.SelectData("Token") == false)
                        {
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            TraceInternal.TraceVerbose("Caught message (" + token + ") from " + caught);
                        }
                    } while ((bool)token.SelectData("Token") == false);
                    this._state = StateType.Ready;
                }

                token.UpdateData("Token",false);

                TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(this._state));

                if (((cancel == false) && (terminate == false)) || (this._state == StateType.Ready) || (this._state == (int)StateType.None) || (@catch.Count==0))
                {
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Process message");

                    process = 1;  // nothing to process?

                    // This is where the descision is made to throw the message
                    // Possibly send the throw decision

                    if ((@throw.Count > 0) && (cancel == false) && (terminate == false))
                    {
                        foreach (Node node in @throw)
                        {
                            bool result = node.Link.Evaluate(replace.ReplaceGrouping(node.Link.Expression, _data, _hierarchy));
                            if (((result == true) && (process == 0)) || ((result == false) && (process > 0)))
                            {
                                token = new Token(_sessionId);
                                token.AddData("Token",true);
                                thrown = node.Link.PutItem(token);
                                TraceInternal.TraceVerbose("Throw message (true) to " + node.Id);
                            }
                        }
                    }
                }
                Thread.Sleep(1000);
            } while (this._state != StateType.Closed);

            TraceInternal.TraceVerbose("Exit Start() " + _id + "(" + _name + ")");

        }

        public override int Perform(int itemIndex, string sessionId)
        {
            Debug.WriteLine("[" + sessionId + "] In Perform() " + this._description + "(" + this.ID + ")");

            this._sessionId = sessionId;
            int process = 0;

            _state = StateType.Active;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
            cancel = false;

            foreach (Item item in _items)
            {
                TraceInternal.TraceVerbose("[" + sessionId + "] Process item:" + item.Description + "(" + item.ID + ")");
                if (cancel == false)
                {
                    process = item.Perform();
                    if (process == 0)
                    {
                        TraceInternal.TraceVerbose("[" + sessionId + "] Ok (" + process + ")");
                    }
                    else
                    {
                        Trace.TraceError("[" + sessionId + "] Error (" + process + ")");
                        break;
                    }
                }
                else
                {
                    TraceInternal.TraceVerbose("[" + sessionId + "] Cancel Event");
                    break;
                }
            }

            // This is where the token should be sent to the linked jobs.
            // Not sure how to link

            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
            Debug.WriteLine("[" + sessionId + "] Out Perform() " + this._description + "(" + this.ID + ")");
            return (process);
        }

        public override void Terminate()
        {
            Debug.WriteLine("[" + _sessionId + "] In Terminate() " + _id + "(" + _name + ")");
            _state = StateType.Terminating;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));

            foreach (Item item in _items)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Task.State=" + StateDescription(item.State));
                if (item.State == Task.StateType.Active)
                {
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Terminate Task:" + item.ID + "(" + item.Name + ")");
                    item.Terminate();
                    break;
                }
                else
                {
                    Debug.WriteLine("[" + _sessionId + "] Inactive Task:" + item.ID + "(" + item.Name + ")");
                }
            }
            _state = StateType.Terminated;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            terminate = true;
            Debug.WriteLine("[" + _sessionId + "] Out Terminate() " + _id + "(" + _name + ")");
        }

        public override void Cancel()
        {
            Debug.WriteLine("In Cancel()");
            cancel = true;
            foreach (Item item in _items)
            {
                if (item.State == Item.StateType.Active)
                {
                    TraceInternal.TraceVerbose("Cancel item:" + item.Description + "(" + item.ID + ")");
                    item.Cancel();
                    break;
                }
                else
                {
                    Debug.WriteLine("Inactive item:" + item.Description + "(" + item.ID + ")");
                }
            }
            _state = StateType.Inactive;
            Debug.WriteLine("Out Cancel()");
        }

        public override void Activate()
        {
            Debug.WriteLine("In Activate()");
            cancel = false;
            foreach (Item item in _items)
            {
                if (item.State != Item.StateType.Active)
                {
                    TraceInternal.TraceVerbose("Activate item:" + item.Description + "(" + item.ID + ")");
                    item.Activate();
                    break;
                }
                else
                {
                    TraceInternal.TraceVerbose("Active item:" + item.Description + "(" + item.ID + ")");
                }
            }
            _state = StateType.Ready;
            Debug.WriteLine("Out Activate()");
        }

        public override void Update(ref ArrayList data, ArrayList parentHierarchy)
        {
            Debug.WriteLine("[" + _sessionId + "] In Update() " + _id + "(" + _name + ")");

            tempData = (ArrayList)_localData.Clone();                    // Preserve the local data and clone.
            _dataId = data.Add(tempData);                                // add the temp date pointer to the data array list.
            if (_hierarchy.Count == 0)
            {
                _hierarchy.Insert((int)StageType.Process, -1);         // fix issue where we don't have a process -1 means don't check now
            }
            else
            {
                _hierarchy = (ArrayList)parentHierarchy.Clone();     // Copy the parent hierarchy
            }
            _hierarchy.Insert((int)StageType.Job, _dataId);     // update the local hierarchy.
            for (int i = 0; i < _hierarchy.Count; i++)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] hierarchy[" + i.ToString() + "]=" + _hierarchy[i].ToString());
            }

            foreach (Item item in _items)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Update item:" + item.ID + "(" + item.Name + ")");
                _data = data;
                item.Update(ref data, _hierarchy);   // Propagate the data and hierarchy
            }

            //if (log.IsDebugEnabled == true)
            //{
            //    int counter = 0;
            //    if (data.Count > 0)
            //    {
            //        do
            //        {
            //            ArrayList stageData = (ArrayList)data[counter];
            //            if (stageData.Count > 0)
            //            {
            //                int count = 0;
            //                DictionaryEntry existing;
            //                do
            //                {
            //                    existing = (DictionaryEntry)stageData[count];
            //                    TraceInternal.TraceVerbose("[" + sessionId + "] Data: counter=" + counter + " count=" + count + " key=" + existing.Key + " value=" + existing.Value);
            //                    count = count + 1;
            //                }
            //                while (count < stageData.Count);
            //            }
            //            counter = counter + 1;
            //        }
            //        while (counter < data.Count);
            //    }
            //}

            Debug.WriteLine("[" + _sessionId + "] Out Update() " + _id + "(" + _name + ")");
        }

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion Methods
    }
}
