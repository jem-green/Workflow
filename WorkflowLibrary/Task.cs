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
    public class Task : Actions, ITask, IEnumerable, ICloneable
    {
        #region Fields

        private static int taskId;
        private IndexCollection<string, Item> items;
        protected string _next = "";
        protected string _previous = "";

        #endregion
        #region Constructors

        public Task() : base()
        {
            items = new IndexCollection<string, Item>();
            taskId = taskId + 1;
            _id = "task_" + taskId.ToString();
        }        
        
        public Task(string id) : base(id)
        {
            items = new IndexCollection<string, Item>();
            this._id = id;
            if (id.StartsWith("task_"))
            {
                if (taskId < Convert.ToInt16(this._id.Substring(5)))
                {
                    taskId = Convert.ToInt16(this._id.Substring(5));
                }
            }
        }

        #endregion Constructors
        #region Properties

        public string Next
        {
            get
            {
                return (_next);
            }
            set
            {
                _next = value;
            }

        }

        public string Previous
        {
            get
            {
                return (_previous);
            }
            set
            {
                _previous = value;
            }
        }

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
                items.Add(item.ID, item);
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
                items.Remove(item);
                remove = true;
            }
            catch { }
            return (remove);
        }

        public IEnumerator<Item> GetEnumerator()
        {
            return items.GetEnumerator();
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

        public override void Start(int index, string sessionId)
        {
            cancel = false;
            terminate = false;
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
            do
            {
                if ((cancel == false) || (terminate == false))
                {
                    this.Perform();
                }
                Thread.Sleep(1000);
            } while (terminate == false);
            _state = StateType.Terminated;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
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

        public override int Perform(int itemIndex, string sessionId)
        {
            Debug.WriteLine("[" + sessionId + "] In Perform() " + _id + "(" + _name + ")");

            this._sessionId = sessionId;
            int process = 0;
            _state = StateType.Active;
            cancel = false;

            // There are two modes of operation here the simplistic each item is synchronous and
            // this is expanding to a asynchronous solution where each item runs in its own thread and messaging could be used to collaborate
            // between the items. Is this the best place for this. It might be better to have the jobs in separate threads and still have item
            // collaboration.

            foreach (Item item in items)
            {
                TraceInternal.TraceVerbose("[" + sessionId + "] Process item:" + item.ID + "(" + item.Name + ")");
                if ((cancel == false) && (terminate == false))
                {
                    process = item.Perform(sessionId);
                    if (process == 0)
                    {
                        TraceInternal.TraceVerbose("[" + sessionId + "] OK (" + process + ")");
                    }
                    else
                    {
                        TraceInternal.TraceVerbose("[" + sessionId + "] Error (" + process + ")");
                        break;  // This will exit out on an error
                    }
                }
                else
                {
                    TraceInternal.TraceVerbose("[" + sessionId + "] Cancelled");
                    break;
                }
            }

            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
            Debug.WriteLine("[" + sessionId + "] Out Perform() " + _id + "(" + _name + ")");
            return (process);
        }

        public override void Update(ref ArrayList data, ArrayList parentHierarchy)
        {
            Debug.WriteLine("[" + _sessionId + "] In Update() " + _id + "(" + _name + ")");

            tempData = (ArrayList)_localData.Clone();           // Preserve the localdata and clone.
            _dataId = data.Add(tempData);                       // add the tempdata pointer to the data array list.
            _hierarchy = (ArrayList)parentHierarchy.Clone();    // Copy the parent hierarchy

            _hierarchy.Insert((int)StageType.Task, _dataId);    // Add the tempdata reference to the end
                        
            //if (Trace. log.IsDebugEnabled == true)
            //{
            //    for (int i = 0; i < hierarchy.Count; i++)
            //    {
            //        TraceInternal.TraceVerbose("[" + sessionId + "] hierarchy(" + i.ToString() + ")=" + hierarchy[i].ToString());
            //    }
            //}

            foreach (Item item in items)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Update item:" + item.ID + "(" + item.Name + ")");
                _data = data;
                item.Update(ref data, _hierarchy);
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

        public override void Cancel()
        {
            Debug.WriteLine("[" + _sessionId + "] In Cancel() " + _id + "(" + _name + ")");
            cancel = true;
            foreach (Item item in items)
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
            foreach (Item item in items)
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

            foreach (Item item in items)
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
