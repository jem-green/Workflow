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
            Token token = new Token(sessionId);
            token.AddData("token", true);
            string caught = "";
            int process = 0;
            cancel = false;
            terminate = false;
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));

            Groupings replace = new Groupings();

            do
            {
                if ((@catch.Count > 0) && (cancel == false) && (terminate == false))
                {
                    do
                    {
                        foreach (Node node in @catch)
                        {
                            if ((bool)token.SelectData("Token") == false)
                            {
                                tokenData = node.Link.GetItem();
                                token = (bool)tokenData.SelectData("token");
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
                            TraceInternal.TraceVerbose("[" + sessionId + "] Caught message (" + tokenData + ") from " + caught);
                        }
                    } while (token == false);
                    _state = StateType.Ready;
                    TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
                }

                token = false;

                TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(this._state));
                
                if (((cancel == false) && (terminate == false)) || (@catch.Count==0))
                {
                    TraceInternal.TraceVerbose("[" + sessionId + "] Process:" + _id + "(" + _name + ")");

                    // Run perform

                    process = this.Perform();

                    // Possibly send the throw event
                    // check if terminated

                    if ((@throw.Count > 0) && (cancel == false) && (terminate == false))
                    {
                        foreach (Node node in @throw)
                        {
                            bool result = true;
                            if (node.Link.Expression.Length > 0)
                            {
                                result = node.Link.Evaluate(replace.ReplaceGrouping(node.Link.Expression, _data, _hierarchy));
                            }
                            if (((result == true) && (process == 0)) || ((result == false) && (process > 0)))
                            {
                                thrown = node.Link.PutItem(true);
                                TraceInternal.TraceVerbose("[" + sessionId + "] Throw message (true) to " + node.Id);
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

            if (taskIndex == 0)
            {
                TraceInternal.TraceVerbose("[" + sessionId + "] Reset data");    // 
                sessionId = Id.UniqueCode();                    // Create a new session id assuming the job restarts
                _data.Clear();                                   // Clear out the data
                _data = (ArrayList)_localData.Clone();            // Clone the local data
                _hierarchy.Clear();                              // Clear out the hierarchy
                this.Update();                                  // Replicate the data
            }

            try
            {
                do
                {
                    task = tasks.GetValue(taskIndex);
                    if ((cancel == false) && (terminate == false))
                    {
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
                                TraceInternal.TraceVerbose("[" + sessionId + "] Reset data");    // 
                                _data.Clear();                                   // Clear out the data
                                _data = (ArrayList)_localData.Clone();            // Clone the local data
                                _hierarchy.Clear();                              // Clear out the hierarchy
                                this.Update();                                  // Replicate the data
                                sessionId = Id.UniqueCode();                    // Create a new session id assuming the job restarts
                            }
                            else if (taskIndex >= tasks.Count)
                            {
                                TraceInternal.TraceVerbose("[" + sessionId + "] Completed");
                                _state = StateType.Completed;
                            }
                        }
                    }
                    else
                    {
                        TraceInternal.TraceVerbose("[" + sessionId + "] Cancel Event");
                        break;
                    }
                }
                while ((_state == StateType.Active) && (cancel==false) && (terminate==false));
                if (process == 0)
                {
                    TraceInternal.TraceVerbose("[" + sessionId + "] Ok (" + process + ")");
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
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
            Debug.WriteLine("[" + sessionId + "] Out Perform() " + _id + "(" + _name + ")");
            return (process);
        }

        public override void Update()
        {
            ArrayList data = new ArrayList();
            ArrayList parentHierarchy = new ArrayList();
            Update(ref data, parentHierarchy);
        }

        public override void Update(ref ArrayList data, ArrayList parentHierarchy)
        {
            Debug.WriteLine("[" + _sessionId + "] In Update() " + _id + "(" + _name + ")");

            tempData = (ArrayList)_localData.Clone();                    // Preserve the localdata and clone.
            _dataId = data.Add(tempData);                                // add the tempdate pointer to the data array list.
            if (parentHierarchy.Count == 0)
            {
                _hierarchy.Insert((int)StageType.Process, -1);         // fix issue where we don't have a process -1 means don't check now
            }
            else
            {
                _hierarchy = (ArrayList)parentHierarchy.Clone();     // Copy the parent hierarchy
            }
            _hierarchy.Insert((int)StageType.Job, _dataId);     // update the local hierarchy.

            foreach (Task task in tasks)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Update task " + task.ID + "(" + task.Name + ") data");
                this._data = data;
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
