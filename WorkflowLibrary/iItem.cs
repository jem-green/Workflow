using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace WorkflowLibrary
{
    interface IItem
    {
        string ID { get; }
        string Name { get; set; }
        bool Enabled { get; set; }
        string Description { get; set; }
        string Application { get; set; }
        string Command { get; set; }
        Content Error { get; set; }
        Content Input { get; set; }
        Content Output { get; set; }
        Coupler Inlet { get; set; }
        Coupler Outlet { get; set; }
        void Activate();
        bool AddData(string key, object value);
        void Cancel();
        int Perform();
        int Perform(int index);
        int Perform(string sessionId);
        int Perform(int index, string sessionId);
        bool RemoveData(string key);
        void Start();
        void Start(int index);
        void Start(string sessionId);
        void Start(int index, string sessionId);
        void Terminate();
        void Update(ref ArrayList Data, ArrayList Hierarchy);
    }
}

