using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text;
using System.Threading;
using TracerLibrary;
using static WorkflowLibrary.KVP;

namespace WorkflowLibrary 
{
    /// <summary>
    /// 
    /// </summary>
    public class Decision: Orchestration, IActivity, IEnumerable, ICloneable
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
		
		public bool Add(Object item)
        {
            return (Add((Item)item));
        }

        public bool Add(Item item)
        {
            bool add = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Add item:" + item.Description);
                _items.Add(item.ID, item);
                add = true;
            }
            catch { }
            return (add);
        }

        public bool Remove(Object item)
        {
            return (Remove((Item)item));
        }

        public bool Remove(Item item)
        {
            bool remove = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Remove item:" + item.Description);
                _items.Remove(item);
                remove = true;
            }
            catch { }
            return (remove);
        }

        public IEnumerator<Item> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();  // Calls IEnumerator<T> GetEnumerator()
        }

        public override void Start()
        {
            Start(0);
        }

        public override void Start(int index)
        {
            Start(index, Id.UniqueCode());
        }

        public override void Start(string sessionId)
        {
            Start(0, Id.UniqueCode());
        }

        public override void Start(int itemIndex, string sessionId)
        {
            Debug.WriteLine("[" + sessionId + "] In Start() " + _id + "(" + _name + ")");

            // Once the token has arrived then the state goes to ready

            bool thrown = false;
            Token tokenData = new Token(sessionId);
            //bool token = false;
            //tokenData.AddLocalData("token", token);
            string caught = "";
            int process = 0;
            cancel = false;
            terminate = false;
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));

            bool match;
            string catchType = "OR";

            do
            {
                if ((@catch.Count > 0) && (cancel == false) && (terminate == false))
                {
                    do   // Wait for one/some/all tokens to arrive
                    {
                        if (catchType == "AND")
                        {

                            // Test the AND logic

                            match = true;
                            foreach (Node node in @catch)
                            {
                                tokenData = node.Link.PeekItem(0);
                                // not sure what this returns as default(Token)
                                if (tokenData != null)
                                {
                                    if ((bool)tokenData.SelectData("token"))
                                    {
                                        match = match & true;
                                    }
                                }
                            }
                            // somehow need to remove the tokens from the queues if they are all true
                        }
                        else if (catchType == "OR")
                        {

                            // Test the OR logic

                            match = false;
                            foreach (Node node in @catch)
                            {
                                tokenData = node.Link.PeekItem(0);
                                // not sure what this returns as default(Token)
                                if (tokenData != null)
                                {
                                    if ((bool)tokenData.SelectData("token"))
                                    {
                                        node.Link.GetItem();  // remove the token from the queue
                                        match = match | true;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            match = true;
                        }

                        if (match == false)
                        {
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            TraceInternal.TraceVerbose("[" + sessionId + "] Caught message (" + tokenData + ") from " + caught);
                        }

                    } while (match == false);
                    _state = StateType.Ready;
                    TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
                }

                // From the identified documents it seems that new token(s)
                // need generating if there is a split. The data will need copying
                // to the new token(s) and the new token(s) sent to the relevant linked jobs.
                // This is where the decision logic will be applied to determine which path to take.

                tokenData.UpdateData("token", false);

                TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(this._state));
                
                if (((cancel == false) && (terminate == false)) || (@catch.Count==0))
                {
                    TraceInternal.TraceInformation("[" + sessionId + "] Process:" + _id + "(" + _name + ")");

                    // Run perform

                    process = 0;  // nothing to process?

                    // This is where the decisions is made to throw the message
                    // Possibly send the throw decision

                    string throwType = "OR";

                    if ((@throw.Count > 0) && (cancel == false) && (terminate == false))
                    {
                        if (throwType == "AND")
                        {
                            foreach (Node node in @throw)
                            {
                                bool result = true;
                                if (node.Link.Expression.Length > 0)
                                {
                                    result = node.Link.Evaluate(Replacer.ReplaceGrouping(node.Link.Expression, _data, _hierarchy));
                                }
                                if (((result == true) && (process == 0)) || ((result == false) && (process > 0)))
                                {
                                    tokenData = new Token(sessionId);
                                    tokenData.AddData("token", true);
                                    thrown = node.Link.PutItem(tokenData);
                                    TraceInternal.TraceVerbose("[" + sessionId + "] Throw message (true) to " + node.Id);
                                }
                            }
                        }
                        else if (throwType == "OR")
                        {
                            foreach (Node node in @throw)
                            {
                                bool result = true;
                                if (node.Link.Expression.Length > 0)
                                {
                                    result = node.Link.Evaluate(Replacer.ReplaceGrouping(node.Link.Expression, _data, _hierarchy));
                                }
                                if (((result == true) && (process == 0)) || ((result == false) && (process > 0)))
                                {
                                    tokenData = new Token(sessionId);
                                    tokenData.AddData("token", true);
                                    thrown = node.Link.PutItem(tokenData);
                                    TraceInternal.TraceVerbose("[" + sessionId + "] Throw message (true) to " + node.Id);
                                    break;
                                }
                            }
                        }

                    }

                    // Extra logic required here to identify that the start event has fired once.
                    // this could be achieved by overloading a base class bit of logic

                    terminate = true;
                }
                Thread.Sleep(1000);
            } while (terminate == false);

            Debug.WriteLine("[" + sessionId + "] Out Start() " + _id + "(" + _name + ")");

        }

        public override int Perform()
        {
            return (Perform(0));
        }

        public override int Perform(int itemIndex)
        {
            return (Perform(itemIndex, Id.UniqueCode()));
        }

        public override int Perform(string sessionId)
        {
            return (Perform(0, sessionId));
        }

        public override int Perform(int indexIndex, string sessionId)
        {
            Debug.WriteLine("[" + sessionId + "] In Perform() " + _id + "(" + _name + ")");

            this._sessionId = sessionId;
            int process = 0;
            _state = StateType.Active;
            cancel = false;

            foreach (Item item in _items)
            {
                TraceInternal.TraceInformation("[" + sessionId + "] Process item:" + item.ID + "(" + item.Name + ")");
                if ((cancel == false) && (terminate == false))
                {
                    process = item.Perform(sessionId);
                    if (process == 0)
                    {
                        TraceInternal.TraceInformation("[" + sessionId + "] OK (" + process + ")");
                    }
                    else
                    {
                        TraceInternal.TraceError("[" + sessionId + "] Error (" + process + ")");
                        break;  // This will exit out on an error
                    }
                }
                else
                {
                    TraceInternal.TraceVerbose("[" + sessionId + "] Cancelled");
                    break;
                }
            }

            // This is where the token should be sent to the linked jobs.
            // Not sure how to link

            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
            Debug.WriteLine("[" + sessionId + "] Out Perform() " + _id + "(" + _name + ")");
            return (process);
        }

        public override void Update(ref List<Grouping> data, List<int> parentHierarchy)
        {
            Debug.WriteLine("[" + _sessionId + "] In Update() " + _id + "(" + _name + ")");

            _tempData = new Grouping(_localData);            // Preserve the localdata and clone.
            data.Add(_tempData);                                 // add the tempdata pointer to the data array list.
            _dataId = data.Count - 1;                           // point to the end of the data array list.
            _hierarchy = new List<int>(parentHierarchy);        // Copy the parent hierarchy


            _hierarchy.Insert((int)StageType.Job, _dataId);     // Add the tempdata reference to the end


            _data = new List<Grouping>(data);

            // Only propagate to _items if they exist
            foreach (Item item in _items)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Update item:" + item.ID + "(" + item.Name + ")");
                item.Update(ref data, _hierarchy);
            }

            Debug.WriteLine("[" + _sessionId + "] Out Update() " + _id + "(" + _name + ")");
        }

        public override void Cancel()
        {
            Debug.WriteLine("[" + _sessionId + "] In Cancel() " + _id + "(" + _name + ")");
            cancel = true;
            foreach (Item item in _items)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Item.State=" + item.State);
                if (item.State == Item.StateType.Active)
                {
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Cancel item:" + item.ID + "(" + item.Name + ")");
                    item.Cancel();
                    break;
                }
                else
                {
                    Debug.WriteLine("[" + _sessionId + "] Inactive item:" + item.ID + "(" + item.Name + ")");
                }
            }
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            cancel = true;
            Debug.WriteLine("[" + _sessionId + "] Out cancel() " + _id + "(" + _name + ")");
        }

        public override void Terminate()
        {
            Debug.WriteLine("[" + _sessionId + "] In Terminate() " + _id + "(" + _name + ")");
            _state = StateType.Terminating;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));

            foreach (Item item in _items)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Item.State=" + StateDescription(item.State));
                if (item.State == Item.StateType.Active)
                {
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Terminate item:" + item.ID + "(" + item.Name + ")");
                    item.Terminate();
                    break;
                }
                else
                {
                    Debug.WriteLine("[" + _sessionId + "] Inactive item:" + item.ID + "(" + item.Name + ")");
                }
            }
            _state = StateType.Terminated;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            terminate = true;
            Debug.WriteLine("[" + _sessionId + "] Out Terminate() " + _id + "(" + _name + ")");
        }

        public override void Activate()
        {
            Debug.WriteLine("[" + _sessionId + "] In Activate() " + _id + "(" + _name + ")");
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));

            foreach (Item item in _items)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
                if (item.State != Item.StateType.Active)
                {
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Activate item:" + item.ID+ "(" + item.Name + ")");
                    item.Activate();
                    break;
                }
                else
                {
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Active item:" + item.ID + "(" + item.Name + ")");
                }
            }
            _state = StateType.Ready;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            cancel = false;
            Debug.WriteLine("[" + _sessionId + "] Out Activate() " + _id + "(" + _name + ")");
        }

        public override object Clone()
        {
            return this.MemberwiseClone();
        }
        
        #endregion Methods
    }
}
