using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WorkflowLibrary;
using System.Xml;
using TracerLibrary;
using System.IO;
using System.Text;

namespace WorkflowApplication
{
   
    static class Program
    {
        public static Client pipeClient;
        public static XmlReader info;
        public static string pipeData;
        public static bool reading = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            pipeClient = new Client();
            pipeClient.MessageReceived += new Client.MessageReceivedHandler(PipeClient_MessageReceived);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        static void PipeClient_MessageReceived(string message)
        {
            try
            {
                if (reading == true)
                {
                    TraceInternal.TraceVerbose("Continue reading");
                    pipeData = pipeData + message;
                }
                else
                {
                    TraceInternal.TraceVerbose("Start reading");
                    pipeData = message;
                    reading = true;
                }

                if (message.EndsWith("\0") == true)
                {
                    TraceInternal.TraceVerbose("MesageReceived Complete");
                    pipeData = pipeData.Substring(0, pipeData.Length - 1);
                    TraceInternal.TraceVerbose("pipeData=" + pipeData);

                    Stream s = new MemoryStream(ASCIIEncoding.Default.GetBytes(pipeData));
                    info = System.Xml.XmlReader.Create(s);
                    pipeData = "";
                    reading = false;
                    
                }
                    else
                {
                    TraceInternal.TraceVerbose("MesageReceived Ongoing");
                }
            }
            catch
            {
                TraceInternal.TraceVerbose("error");
            }
            TraceInternal.TraceVerbose("message=" + message);
        }
    }
}