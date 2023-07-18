using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace WorkflowLibrary
{
    interface ILink
    {
        string From {get;set; }
        string To {get;set; }
        string Expression { get; set; }
        State.StateType State { get; }
    }
}

