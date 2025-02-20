using System;
using System.Collections;
using TracerLibrary;

namespace WorkflowLibrary 
{
    /// <summary>
    /// Event class derived from a standard Activity
    /// </summary>
    public class Event : Activity, IActivity, IEnumerable, ICloneable, IEvent
    {
        #region Fields
        private static int eventId;
        #endregion
        #region Constructors

        public Event() : base()
        {
            eventId = eventId + 1;
            _id = "event_" + eventId.ToString();
        }
        
        public Event(string id) : base(id)
        {
            this._id = id;
			if (id.StartsWith("event_"))
            {
                if (eventId < Convert.ToInt16(this._id.Substring(6)))
                {
                    eventId = Convert.ToInt16(this._id.Substring(6));
                }
            }
        }

        #endregion Constructors
        #region Properties

        #endregion Properties
        #region Methods

        #endregion Methods

    }
}
