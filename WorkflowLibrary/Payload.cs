using System;
using System.Collections.Generic;
using System.Text;

namespace WorkflowLibrary
{
	public class Payload : EventArgs
	{
		private string content;
		public string Content
		{
			set
			{
				content = value;
			}
			get
			{
				return content;
			}
		}
	}
}
