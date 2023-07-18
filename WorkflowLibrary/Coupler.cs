using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using TracerLibrary;

namespace WorkflowLibrary
{
    /// <summary>
    /// Coupler joins pipe to items, delivering payloads
    /// </summary>
	public class Coupler
	{
        #region Fields

		private string content;    // string buffer
        private static int couplerID;
        private string id;

        #endregion
        #region Constructor
        public Coupler()
        {
            couplerID = couplerID + 1;
            id = "coupler_" + couplerID.ToString();
        }

        public Coupler(string Id)
        {
            id = Id;
            if (Id.StartsWith("coupler_"))
            {
                if (couplerID < Convert.ToInt16(Id.Substring(4)))
                {
                    couplerID = Convert.ToInt16(Id.Substring(4));
                }
            }
            else
            {
                couplerID = couplerID + 1;
            }
            id = "coupler_" + couplerID.ToString();
        }
        #endregion Constructor
        #region Properties
        string Content
        {
            get
            {
                return (this.content);
            }
        }
        #endregion Properties
        #region Methods
        /// <summary>
        /// Expose the Message event handler.
        /// </summary>
        public event MessageHandler Message;

		/// <summary>
		/// Define the class references for the event handler
		/// </summary>
		/// <param name="connector"></param>
		/// <param name="payload"></param>
		public delegate void MessageHandler(Payload payload);

		public void Join(Pipe pipe)
		{
			// Create a Target Method to handle the Node Event

			pipe.Message += new Pipe.MessageHandler(Receive);
		}

        public void Fill(string content)
        {
            // Raise a message event

            Payload payload = new Payload();
            payload.Content = content;
            TraceInternal.TraceVerbose("[" + id + "] Fill()");
            Message(payload);
        }

        public string Drain()
        {
            TraceInternal.TraceVerbose("[" + id + "] Drain()");
            return (this.content);
        }

		private void Send()
		{
			// Raise a message event

			Payload payload = new Payload();
			payload.Content = this.content;
            TraceInternal.TraceVerbose("[" + id + "] Send()");
			Message(payload);
		}

		private void Receive(Payload payload)
		{
			// Store the character

            TraceInternal.TraceVerbose("[" + id + "] Receive()");
            this.content = payload.Content;
			this.Send();
		}
#endregion
    }
}
