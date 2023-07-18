using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace WorkflowLibrary
{
    public interface IProcess
    {
        string ID { get; }
        string Name { get; set; }
        bool Enabled { get; set; }
        string Description { get; set; }
        ArrayList Data { get; }
        State.StateType State { get; }
        bool AddData(string key, object value);
        bool RemoveData(string key);
        bool Add(IActivity activity);
        bool Remove(IActivity activity);
        void Activate();
        void Cancel();
        int Perform();
        int Perform(int elementIndex);
        int Perform(string sessionId);
        int Perform(int elementIndex, string sessionId);
        void Update();
        void Terminate();
        void Start();
        object Clone();
    }
}

