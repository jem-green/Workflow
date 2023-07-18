using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TracerLibrary;

namespace WorkflowLibrary
{
    /// <summary>
    /// pipe class implements a an event based conduit between items
    /// </summary>
    public class Pipe : Element
    {
        #region Fields
   
        private static int pipeId;
        private string content;    // This is a string buffer

        #endregion
        #region Constructors

        public Pipe() : base()
        {
            pipeId = pipeId + 1;
            _id = "pipe_" + pipeId.ToString();
        }        
        
        public Pipe(string Id) : base(Id)
        {
            _id = Id;
            if (Id.StartsWith("pipe_"))
            {
                if (pipeId < Convert.ToInt16(Id.Substring(5)))
                {
                    pipeId = Convert.ToInt16(Id.Substring(5));
                }
            }
        }

        #endregion
        #region Properties

        #endregion
        #region Methods
        /// <summary>
		/// Expose the Message event handler.
		/// </summary>
		public event MessageHandler Message;

		/// <summary>
		/// Define the class references for the event handler
		/// </summary>
		/// <param name="n"></param>
		/// <param name="p"></param>
		public delegate void MessageHandler(Payload pl);

		public void Join(Coupler connector)
		{
			// Create a Target Method to handle the Connector Event

			connector.Message += new Coupler.MessageHandler(Drain);
		}

		private void Fill()
		{
			// Raise a message event

			Payload payload = new Payload();
			payload.Content = content;
			Message(payload);

		}

		private void Drain(Payload payload)
		{
            // Store the string

			this.content = payload.Content;
			this.Fill();
        }
        #endregion Methods
    }
}
