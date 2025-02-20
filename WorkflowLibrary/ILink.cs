using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace WorkflowLibrary
{
    interface ILink
    {
        #region Properties

        string Expression { get; set; }
        string From {get;set; }
        string ID { get; }
        State.StateType State { get; }
        string To {get;set; }

        #endregion
    }
}

