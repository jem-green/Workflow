using System;
using System.Collections;
using System.Collections.Generic;

namespace WorkflowLibrary
{
    public interface IActivity
    {
        #region Properties

        State.StateType State { get; }
        string ID { get; }
        string Name { get; set; }
        bool Enabled { get; set; }
        string Description { get; set; }
        List<Grouping> Data { get; }
        List<Node> Throw { get; }
        List<Node> Catch { get; }

        #endregion
        #region Methods

        bool AddCatch(Node value);
        bool AddThrow(Node value);
        bool AddLocalData(string key, object value);
        bool RemoveData(string key);
        void Activate();
        void Cancel();
        int Perform();
        int Perform(int elementIndex);
        int Perform(string sessionId);
        int Perform(int elementIndex, string sessionId);
        void Update();
        void Update(ref List<Grouping> data, List<int> parentHierarchy);
        void Start();
        void Start(string sessionId);
        void Start(int activityIndex);
        void Start(int activityIndex, string sessionId);
        void Terminate();
        object Clone();

        #endregion
    }
}
