using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace WorkflowLibrary
{
    interface IJob
    {
        string ID { get; }
        string Name { get; set; }
        bool Enabled { get; set; }
        string Description { get; set; }
        ArrayList Data {get; }
        List<Node> Throw {get; }
        List<Node> Catch {get; }
        State.StateType State { get; }
        void Activate();
        bool Add(Task task);
        bool AddCatch(Node value);
        bool AddData(string key, object value);
        bool AddThrow(Node value);
        void Cancel();
        int Perform();
        int Perform(int index);
        int Perform(string sessionId);
        int Perform(int index, string sessionId);
        bool Remove(Task task);
        bool RemoveData(string key);
        void Update(ref ArrayList Data, ArrayList Hierarchy);
        void Start();
        void Start(int index);
        void Start(string sessionId);
        void Start(int index, string sessionId);
        void Terminate();
    }
}

