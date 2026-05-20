using System;
using System.Collections;
using System.Collections.Generic;
using TracerLibrary;
using System.Threading;
using System.Diagnostics;

namespace WorkflowLibrary 
{
    public class Activity : Orchestration, IActivity, IEnumerable, ICloneable
    {
        #region Fields

        private IndexCollection<string,Task> tasks;

        #endregion
        #region Constructors

        public Activity() : base()
        {
            tasks = new IndexCollection<string, Task>();
        }
        
        public Activity(string id) : base(id)
        {
            tasks = new IndexCollection<string, Task>();
        }

        #endregion Constructors
        #region Properties

        #endregion Properties
        #region Methods

        public bool Add(Object task)
        {
            return (this.Add((Task)task));
        }

        public bool Add(Task task)
        {
            bool add = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Add task:" + task.Description);
                tasks.Add(task.ID, task);
                add = true;
            }
            catch { }
            return (add);
        }

        public bool Remove(Object task)
        {
            return (this.Remove((Task)task));
        }

        public bool Remove(Task task)
        {
            bool remove = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Remove task:" + task.Description);
                remove = true;
            }
            catch { }
            return (remove);
        }

        public IEnumerator<Task> GetEnumerator()
        {
            return tasks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();  // Calls IEnumerator<T> GetEnumerator()
        }

        public override void Start()
        {
            Start(0);
        }

        public override void Start(int activityIndex)
        {
            Start(activityIndex, Id.UniqueCode());
        }

        public override void Start(string sessionId)
        {
            Start(0, Id.UniqueCode());
        }

        public override void Start(int activityIndex, string sessionId)
        {
            Debug.WriteLine("[" + sessionId + "] In Start() " + _id + "(" + _name + ")");

            // Once the token has arrived then the state goes to ready

            bool thrown = false;
            Token tokenData = new Token(Id.UniqueCode());
            bool token = false;
            tokenData.AddData("token", token);
            string caught = "";
            int process = 0;
            cancel = false;
            terminate = false;
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));

            do
            {
                if ((@catch.Count > 0) && (cancel == false) && (terminate == false))
                {
                    do
                    {
                        foreach (Node node in @catch)
                        {
                            if (token == false)
                            {
                                tokenData = node.Link.GetItem();
                                if (tokenData != null)
                                {
                                    token = (bool)tokenData.SelectData("token");
                                    caught = node.Id;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (token == false)
                        {
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            TraceInternal.TraceInformation("[" + sessionId + "] Caught message (true) from " + caught);
                        }
                    } while (token == false);
                    _state = StateType.Ready;
                    TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
                }

                tokenData.UpdateData("token", false);

                TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(this._state));
                
                if (((cancel == false) && (terminate == false)) || (@catch.Count==0))
                {
                    TraceInternal.TraceInformation("[" + sessionId + "] Process:" + _id + "(" + _name + ")");

                    // Run perform

                    process = this.Perform(sessionId);

                    // Possibly send the throw event
                    // check if cancelled or terminated

                    if ((@throw.Count > 0) && (cancel == false) && (terminate == false))
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
                                TraceInternal.TraceInformation("[" + sessionId + "] Throw message (true) to " + node.Id);
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
            return(Perform(0));
        }

        public override int Perform(int taskIndex)
        {
            return (Perform(taskIndex, Id.UniqueCode()));
        }

        public override int Perform(string sessionId)
        {
            return (Perform(0, sessionId));
        }

        public override int Perform(int taskIndex, string sessionId)
        {
            Debug.WriteLine("[" + sessionId + "] In Perform() " + _id + "(" + _name + ")");

            // Provide some logic to the processing
            // by examining the error code and determining
            // the true / false routes

            this._sessionId = sessionId;
            Task task;
            string nextTask = "";
            int process = 0;
            int next = 0;

            // What do we do if the jobs is already active

            _state = StateType.Active;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
            cancel = false;

            //if (taskIndex == 0 && _dataId >= 0 && _dataId < _data.Count)
            //{
            //    TraceInternal.TraceVerbose("[" + sessionId + "] Reset local data slot");
            //    // Only replace THIS Job's data slot
            //    _data[_dataId] = (ArrayList)_localData.Clone();
            //    sessionId = Id.UniqueCode();
            //    // Don't call Update() - we don't have parent context
            //}

            if (tasks.Count > 0)
            {
                try
                {
                    do
                    {
                        if ((cancel == false) && (terminate == false))
                        {
                            task = tasks.GetValue(taskIndex);
                            TraceInternal.TraceInformation("[" + sessionId + "] Process Task:" + task.ID + "(" + task.Name + ")");
                            process = task.Perform(sessionId);
                            if ((cancel == false) && (terminate == false))
                            {
                                if (process == 0)
                                {
                                    nextTask = task.Next;
                                    TraceInternal.TraceVerbose("[" + sessionId + "] Next Task " + nextTask);
                                    if (nextTask.Length == 0)
                                    {
                                        taskIndex = taskIndex + 1;
                                    }
                                    else
                                    {
                                        // ideally want to support the legacy way for working so if its numeric
                                        // then jump to the task index.

                                        if (int.TryParse(nextTask, out next))
                                        {
                                            taskIndex = next;
                                        }
                                        else
                                        {
                                            taskIndex = tasks.IndexOf(nextTask);
                                        }
                                    }
                                    TraceInternal.TraceVerbose("[" + sessionId + "] Task Index " + taskIndex);

                                }
                                else
                                {
                                    nextTask = task.Previous;
                                    TraceInternal.TraceVerbose("[" + sessionId + "] Previous Task " + nextTask);
                                    if (nextTask.Length == 0)
                                    {
                                        taskIndex = taskIndex - 1;
                                    }
                                    else
                                    {
                                        if (int.TryParse(nextTask, out next))
                                        {
                                            taskIndex = next;
                                        }
                                        else
                                        {
                                            taskIndex = tasks.IndexOf(nextTask);
                                        }
                                    }
                                    TraceInternal.TraceVerbose("[" + sessionId + "] Task Index " + taskIndex);
                                }

                                if (taskIndex < 0)
                                {
                                    TraceInternal.TraceVerbose("[" + sessionId + "] Completed");
                                    _state = StateType.Completed;
                                }
                                else if (taskIndex == 0)
                                {
                                    TraceInternal.TraceVerbose("[" + sessionId + "] Reset data (loop restart)");
                                    // Only reset THIS Job's data slot, not entire hierarchy
                                    if (_dataId >= 0 && _dataId < _data.Count)
                                    {
                                        _data[_dataId] = _localData;
                                    }
                                    sessionId = Id.UniqueCode();
                                    // Don't clear _hierarchy - keep navigation path!
                                }
                                else if (taskIndex >= tasks.Count)
                                {
                                    TraceInternal.TraceVerbose("[" + sessionId + "] Completed");
                                    _state = StateType.Completed;
                                }
                            }
                            else
                            {
                                TraceInternal.TraceVerbose("[" + sessionId + "] Cancel Event");
                                break;
                            }
                        }
                        else
                        {
                            TraceInternal.TraceVerbose("[" + sessionId + "] Cancel Event");
                            break;
                        }
                    }
                    while ((_state == StateType.Active) && (cancel == false) && (terminate == false));

                    if (process == 0)
                    {
                        TraceInternal.TraceVerbose("[" + sessionId + "] OK (" + process + ")");
                    }
                    else
                    {
                        TraceInternal.TraceVerbose("[" + sessionId + "] Error (" + process + ")");
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError("[" + sessionId + "] Other (" + process + ")");
                    TraceInternal.TraceVerbose("[" + sessionId + "] Exception=" + e.ToString());
                }
            }
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
            Debug.WriteLine("[" + sessionId + "] Out Perform() " + _id + "(" + _name + ")");
            return (process);
        }

        public override void Update()
        {
            List<Grouping> data = new List<Grouping>();
            List<int> parentHierarchy = new List<int>();
            Update(ref data, parentHierarchy);
        }

        public override void Update(ref List<Grouping> data, List<int> parentHierarchy)
        {
            Debug.WriteLine("[" + _sessionId + "] In Update() " + _id + "(" + _name + ")");

            _tempData = new Grouping(_localData);                       // Preserve the localdata and clone.
            data.Add(_tempData);                                        // Add the tempdata to the data array list.
            _dataId = data.Count - 1;                                   // add the tempdata pointer to the data array list.
            if (parentHierarchy.Count == 0)
            {
                _hierarchy.Insert((int)StageType.Process, -1);          // fix issue where we don't have a process -1 means don't check now
            }
            else
            {
                _hierarchy = new List<int>(parentHierarchy);            // Copy the parent hierarchy
            }
            _hierarchy.Insert((int)StageType.Job, _dataId);             // update the local hierarchy.

            this._data = data;

            foreach (Task task in tasks)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Update task " + task.ID + "(" + task.Name + ") data");
                task.Update(ref data, _hierarchy);   // Propagate the data and hierarchy
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

        public override void Terminate()
        {
            Debug.WriteLine("[" + _sessionId + "] In Terminate() " + _id + "(" + _name + ")");
            _state = StateType.Terminating;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));

            foreach (Task task in tasks)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Task.State=" + StateDescription(task.State));
                if (task.State == Task.StateType.Active)
                {
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Terminate Task:" + task.ID + "(" + task.Name + ")");
                    task.Terminate();
                    break;
                }
                else
                {
                    Debug.WriteLine("[" + _sessionId + "] Inactive Task:" + task.ID + "(" + task.Name + ")");
                }
            }
            _state = StateType.Terminated;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            terminate = true;
            Debug.WriteLine("[" + _sessionId + "] Out Terminate() " + _id + "(" + _name + ")");
        }

        public override void Cancel()
        {
            Debug.WriteLine("[" + _sessionId + "] In cancel() " + _id + "(" + _name + ")");
            _state = StateType.Withdrawn;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            this.cancel = true;

            foreach (Task task in tasks)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Task.State=" + StateDescription(task.State));
                if (task.State == StateType.Active)
                {
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Cancel Task:" + task.ID + "(" + task.Name + ")");
                    task.Cancel();
                    break;
                }
                else
                {
                    Debug.WriteLine("[" + _sessionId + "] Inactive Task:" + task.ID + "(" + task.Name + ")");
                }

            }
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            Debug.WriteLine("[" + _sessionId + "] Out cancel() " + _id + "(" + _name + ")");
        }

        public override void Activate()
        {
            Debug.WriteLine("[" + _sessionId + "] In Activate() " + _id + "(" + _name + ")");
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));

            foreach (Task task in tasks)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Task.State=" + StateDescription(task.State));
                if (task.State != StateType.Active)
                {
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Activate Task:" + task.ID + "(" + task.Name + ")");
                    task.Activate();
                    break;
                }
                else
                {
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Active Task:" + task.ID + "(" + task.Name + ")");
                }

            }
            _state = StateType.Ready;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            cancel = false;
            Debug.WriteLine("[" + _sessionId + "] Out activate() " + _id + "(" + _name + ")");
        }

        public override object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion Methods
    }
}
