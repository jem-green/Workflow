using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using TracerLibrary;

namespace WorkflowLibrary
{
    public class Actions : Element
    {
        #region Fields

        #endregion
        #region Constructors
        public Actions()
        {
        }
        public Actions(string id)
        {
        }
        #endregion
        #region Methods

        public virtual void Start()
        { }

        public virtual void Start(string sessionId)
        {
            Start(0, sessionId);
        }

        public virtual void Start(int activityIndex)
        {
            Start(activityIndex, Id.UniqueCode());
        }

        public virtual void Start(int activityIndex, string sessionId)
        { }

        public virtual int Perform()
        {
            return (Perform(0));
        }

        public virtual int Perform(int taskIndex)
        {
            return (Perform(taskIndex, Id.UniqueCode()));
        }

        public virtual int Perform(string sessionId)
        {
            return (Perform(0, sessionId));
        }

        public virtual int Perform(int taskIndex, string sessionId)
        {
            int process = 0;
            return (process);
        }

        public virtual void Update()
        { }

        public virtual void Update(ref ArrayList data, ArrayList parentHierarchy)
        { }

        public virtual void Cancel()
        { }

        public virtual void Terminate()
        { }

        public virtual void Activate()
        { }

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion Methods
    }
}
