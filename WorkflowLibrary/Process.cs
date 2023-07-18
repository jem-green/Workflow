using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using TracerLibrary;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Diagnostics;

namespace WorkflowLibrary
{
    public class Process : Orchestration, IActivity, IProcess, IEnumerable, ICloneable
    {
        #region Fields

        private IndexCollection<string,IActivity> activities;
        private static int processId;

        #endregion
        #region Constructors

        public Process()
        {
            activities = new IndexCollection<string, IActivity>();
            processId = processId + 1;
            _id = "process_" + processId.ToString();
        }
        public Process(string Id)
        {
            activities = new IndexCollection<string, IActivity>();
            _id = Id;
            if (Id.StartsWith("process_"))
            {
                if (processId < Convert.ToInt16(Id.Substring(8)))
                {
                    processId = Convert.ToInt16(Id.Substring(8));
                }
            }
        }

        #endregion Constructors
        #region Properties

        #endregion Properites
        #region Methods

        public bool Add(Object activity)
        {
            return (this.Add((IActivity)activity));
        }

        public bool Add(IActivity activity)
        {
            bool add = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Add activity:" + activity.Description);
                activities.Add(activity.ID, activity);
                add = true;
            }
            catch { }
            return (add);
        }

        public bool Remove(Object activity)
        {
            return (this.Remove((IActivity)activity));
        }

        public bool Remove(IActivity activity)
        {
            bool remove = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Remove activity:" + activity.Description);
                remove = true;
            }
            catch { }
            return (remove);
        }

        public IEnumerator<IActivity> GetEnumerator()
        {
            return activities.GetEnumerator();
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

            // All the jobs run in separate threads so get lauched 
            // at the same time

            int perform = 0;
            cancel = false;
            terminate = false;
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));

            Groupings replace = new Groupings();
 
            // link up the objects

            linkObjects(activities);

            // Link up the pipes

            JoinPipes(activities);

            // Launch the job threads

            foreach (object item in activities)
            {
                if (item.GetType() == typeof(Job))
                {
                    Job job = (Job)item;
                    Thread jobThread = new Thread(new ThreadStart(job.Start));
                    jobThread.Start();
                }
                else if (item.GetType() == typeof(Event))
                {
                    Event @event = (Event)item;
                    Thread eventThread = new Thread(new ThreadStart(@event.Start));
                    eventThread.Start();
                }
            }

            do
            {
                perform = Perform();
                Thread.Sleep(1000);
            } while (terminate == false);

            Debug.WriteLine("[" + sessionId + "] Out Start() " + _id + "(" + _name + ")");
        }

        public override int Perform()
        {
            return(this.Perform(0));
        }

        public override int Perform(int jobIndex)
        {
            return (Perform(jobIndex, Id.UniqueCode()));
        }

        public override int Perform(string sessionId)
        {
            return (Perform(0, sessionId));
        }

        public override int Perform(int jobIndex, string sessionId)
        {

            // Provide some logic to the processing
            // by examining the errorcode and determining
            // the true / false routes

            this._sessionId = sessionId;
            int perform = 0;

            try
            {
                do
                {
                    Thread.Sleep(1000);
                }
                while ((cancel == false) && (terminate == false)) ;
                if (perform == 0)
                {
                    TraceInternal.TraceVerbose("[" + sessionId + "] Ok (" + perform + ")");
                }
                else
                {
                    TraceInternal.TraceVerbose("[" + sessionId + "] Error (" + perform + ")");
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("[" + sessionId + "] Other (" + perform + ")");
                TraceInternal.TraceVerbose("[" + sessionId + "] Exception=" + e.ToString());
            }
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + sessionId + "] State=" + StateDescription(_state));
            Debug.WriteLine("[" + sessionId + "] Out Perform() " + _id + "(" + _name + ")");
            return (perform);

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
            _hierarchy.Insert((int)StageType.Process, _dataId);     // update the local hierarchy.
            //if (log.IsDebugEnabled == true)
            //{
            //    for (int i = 0; i < hierarchy.Count; i++)
            //    {
            //        TraceInternal.TraceVerbose("[" + sessionId + "] hierarchy[" + i.ToString() + "]=" + hierarchy[i].ToString());
            //    }
            //}

            foreach (IActivity activity in activities)
            {
                if (activity.GetType() == typeof(Job))
                {
                    Job job = (Job)activity;
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Update task " + job.ID + "(" + job.Name + ") data");
                    this._data = data;
                    job.Update(ref data, _hierarchy);   // Propagate the data and hierarchy
                }
                else if (activity.GetType() == typeof(Event))
                {
                    Event @event = (Event)activity;
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Update event " + @event.ID + "(" + @event.Name + ") data");
                    this._data = data;
                    @event.Update(ref data, _hierarchy);   // Propagate the data and hierarchy
                }
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
            Debug.WriteLine("[" + _sessionId + "] In cancel() " + _id + "(" + _name + ")");
            _state = StateType.Withdrawn;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            this.cancel = true;

            foreach (Job job in activities)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] job.State=" + StateDescription(job.State));
                if (job.State == StateType.Active)
                {
                    TraceInternal.TraceVerbose("Cancel job:" + job.Description + "(" + job.ID + ")");
                    job.Cancel();
                    break;
                }
                else
                {
                    Debug.WriteLine("Inactive job:" + job.Description + "(" + job.ID + ")");
                }

            }
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            Debug.WriteLine("[" + _sessionId + "] Out cancel() " + _id + "(" + _name + ")");
        }

        public override void Terminate()
        {
            Debug.WriteLine("[" + _sessionId + "] In Terminate() " + _id + "(" + _name + ")");
            _state = StateType.Terminating;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));

            foreach (Job job in activities)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Job.State=" + StateDescription(job.State));
                if (job.State == Task.StateType.Active)
                {
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Terminate Task:" + job.ID + "(" + job.Name + ")");
                    job.Terminate();
                    break;
                }
                else
                {
                    Debug.WriteLine("[" + _sessionId + "] Inactive Task:" + job.ID + "(" + job.Name + ")");
                }
            }
            _state = StateType.Terminated;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));
            terminate = true;
            Debug.WriteLine("[" + _sessionId + "] Out Terminate() " + _id + "(" + _name + ")");
        }

        public override void Activate()
        {
            Debug.WriteLine("[" + _sessionId + "] In activate() " + _id + "(" + _name + ")");
            _state = StateType.Inactive;
            TraceInternal.TraceVerbose("[" + _sessionId + "] State=" + StateDescription(_state));

            foreach (Job job in activities)
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] JOb.State=" + StateDescription(job.State));
                if (job.State != Task.StateType.Active)
                {
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Activate Task:" + job.ID + "(" + job.Name + ")");
                    job.Activate();
                    break;
                }
                else
                {
                    TraceInternal.TraceVerbose("[" + _sessionId + "] Active Task:" + job.ID + "(" + job.Name + ")");
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

        private void linkObjects(IndexCollection<string, IActivity> container)
        {
            Debug.WriteLine("In LinkObjects()");
            // Link the objects correctly based on the throw/catch rules

            foreach (IActivity activity in container)
            {
                if (activity.GetType() == typeof(Link))
                {
                    Link link = (Link)activity;
                    foreach (object obj in container)
                    {
                        if (obj.GetType() == typeof(Job))
                        {
                            Job job = (Job)obj;

                            foreach (Node node in job.Throw)
                            {
                                if ((job.ID == link.From) && (node.IsLinked == false))
                                {
                                    node.Link = link;
                                    TraceInternal.TraceVerbose("Link " + link.ID + ", thrown from " + link.From + " to " + link.To);
                                    break;
                                }
                            }

                            foreach (Node node in job.Catch)
                            {
                                if ((job.ID == link.To) && (node.IsLinked == false))
                                {
                                    node.Link = link;
                                    TraceInternal.TraceVerbose("Link " + link.ID + ", caught from " + link.From + " to " + link.To);
                                    break;
                                }
                            }

                        }
                        else if (obj.GetType() == typeof(Event))
                        {
                            Event @event = (Event)obj;
                            foreach (Node node in @event.Throw)
                            {
                                if ((@event.ID == link.From) && (node.IsLinked == false))
                                {
                                    node.Link = link;
                                    TraceInternal.TraceVerbose("Link " + link.ID + ", thrown from " + link.From + " to " + link.To);
                                    break;
                                }
                            }

                            foreach (Node node in @event.Catch)
                            {
                                if ((@event.ID == link.To) && (node.IsLinked == false))
                                {
                                    node.Link = link;
                                    TraceInternal.TraceVerbose("Link " + link.ID + ", caught from " + link.From + " to " + link.To);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            Debug.WriteLine("Out LinkObjects()");
        }

        private void JoinPipes(IndexCollection<string, IActivity> container)
        {
            Debug.WriteLine("In JoinPipes()");
            // Link the pipes to the item connectors

            foreach (object j in container)
            {
                if (j.GetType() == typeof(Job))
                {
                    foreach (object t in (Job)j)
                    {
                        if (t.GetType() == typeof(Task))
                        {
                            foreach (object i in (Task)t)
                            {
                                if (i.GetType() == typeof(Item))
                                {
                                    Item item = (Item)i;

                                    if (item.Input.Kind == DataKind.pipe)
                                    {
                                        // Now check the input connetor with any pipe starts

                                        foreach (object p in container)
                                        {
                                            if (p.GetType() == typeof(Pipe))
                                            {
                                                Pipe pipe = (Pipe)p;
                                                if ((string)item.Input.Value == pipe.ID)
                                                {
                                                    item.Inlet.Join(pipe);
                                                    TraceInternal.TraceVerbose("Join pipe() '" + pipe.ID + "'(" + pipe.Name + ") to item() '" + item.ID + "'(" + item.Name + ")");
                                                }

                                            }
                                        }
                                    }

                                    if (item.Output.Kind == DataKind.pipe)
                                    {
                                        // Now check the output connetor with any pipe ends

                                        foreach (object p in container)
                                        {
                                            if (p.GetType() == typeof(Pipe))
                                            {
                                                Pipe pipe = (Pipe)p;
                                                if ((string)item.Output.Value == pipe.ID)
                                                {
                                                    pipe.Join(item.Outlet);
                                                    TraceInternal.TraceVerbose("Join item() '" + item.ID + "'(" + item.Name + ") to pipe() '" + pipe.ID + "'(" + pipe.Name + ")");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Debug.WriteLine("Out JoinPipes()");
        }

        #endregion Methods
    }
}
