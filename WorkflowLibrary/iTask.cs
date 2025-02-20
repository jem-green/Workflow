using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace WorkflowLibrary
{
    interface ITask
    {
        #region Properties
        string ID { get; }
        string Name { get; set; }
        bool Enabled { get; set; }
        string Description { get; set; }
        string Next {get;set; }
        string Previous {get;set; }
        ArrayList LocalData {get; }
        State.StateType State { get; }
        #endregion
        #region Methods
        void Activate();
        bool Add(Item item);
        bool AddData(string key, object value);
        void Cancel();
        object Clone();
        int Perform();
        int Perform(int index);
        int Perform(string sessionId);
        int Perform(int index, string sessionId);
        bool Remove(Item item);
        bool RemoveData(string key);
        void Start();
        void Start(int index);
        void Start(string sessionId);
        void Start(int index, string sessionId);
        void Terminate();
        void Update(ref ArrayList data, ArrayList hierarchy);

        #endregion
    }
}
