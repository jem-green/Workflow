using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace WorkflowLibrary
{
    interface IItem
    {
        #region Properties

        string Application { get; set; }
        string Command { get; set; }
        string Description { get; set; }
        bool Enabled { get; set; }
        Content Error { get; set; }
        string ID { get; }
        Coupler Inlet { get; set; }
        Content Input { get; set; }
        string Name { get; set; }
        Coupler Outlet { get; set; }
        Content Output { get; set; }

        #endregion
        #region Methods

        void Activate();
        bool AddData(string key, object value);
        void Cancel();
        object Clone();
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

        #endregion
    }
}

