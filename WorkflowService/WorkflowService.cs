using System;
using System.ServiceProcess;
using TracerLibrary;
using WorkflowLibrary;
using System.Threading;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace WorkflowService
{
    public partial class WorkflowService : ServiceBase
    {
	    #region Fields
        
        WorkflowLibrary.Server config;
        Collection<object> processData = new Collection<object>();

        public enum Commands
        {
            None = 0,
            Log = 255
        }

        private System.Threading.Thread workerThread = null;

        #endregion
        public WorkflowService()
        {
            Debug.WriteLine("In WorkflowService()");
            InitializeComponent();
            eventLog.Source = "Workflow";
            Debug.WriteLine("Out WorkflowService()");

        }

        protected override void OnStart(string[] args)
        {

            eventLog.WriteEntry("In OnStart.");
            Debug.WriteLine("In OnStart()");

            if ((workerThread == null) ||
                ((workerThread.ThreadState &
                 (System.Threading.ThreadState.Unstarted | System.Threading.ThreadState.Stopped)) != 0))
            {
                workerThread = new Thread(new ThreadStart(ServiceWorkerMethod));
                workerThread.Start();
            }

            Debug.WriteLine("Out OnStart()");
            eventLog.WriteEntry("Out OnStart.");

        }

        protected override void OnStop()
        {
            eventLog.WriteEntry("In OnStop.");
            Debug.WriteLine("In OnStop()");

            if (workerThread != null)
            {
                foreach (object item in processData)
                {
                    if (item.GetType() == typeof(Job))
                    {
                        Job j = (Job)item;
                        j.Terminate();
                    }
                    else if (item.GetType() == typeof(WorkflowLibrary.Event))
                    {
                        Event e = (Event)item;
                        e.Terminate();
                    }
                    else
                    {
                        TraceInternal.TraceVerbose("Unknown Item");
                    }
                }

                try
                {
                    workerThread.Abort();
                }
                catch { }
                try
                {
                    workerThread.Join();
                }
                catch { }
            }
            Debug.WriteLine("Out OnStop()");
            eventLog.WriteEntry("Out OnStop.");

        }

        void ServiceWorkerMethod()
        {
            Debug.WriteLine("In ServiceWorkerMethod()");

            string jobId = "";

            Parameter<string> appPath = new Parameter<string>("appPath","");
            Parameter<string> appName = new Parameter<string>("appName","process.xml");
            appPath.Value = System.Reflection.Assembly.GetExecutingAssembly().Location;

            int pos = appPath.Value.ToString().LastIndexOf(Path.DirectorySeparatorChar);
            if (pos > 0)
            {
                appPath.Value = appPath.Value.ToString().Substring(0, pos);
                appPath.Source = Parameter<string>.SourceType.App;
            }

            Parameter<string> logPath = new Parameter<string>("logName", "");
            Parameter<string> logName = new Parameter<string>("logName","workflowservice");
            logPath.Value = System.Reflection.Assembly.GetExecutingAssembly().Location;
            pos = logPath.Value.ToString().LastIndexOf(Path.DirectorySeparatorChar);
            if (pos > 0)
            {
                logPath.Value = logPath.Value.ToString().Substring(0, pos);
                logPath.Source = Parameter<string>.SourceType.App;
            }

            Parameter<SourceLevels> traceLevels = new Parameter<SourceLevels>("traceLevels");
            traceLevels.Value = TraceInternal.TraceLookup("VERBOSE");
            traceLevels.Source = Parameter<SourceLevels>.SourceType.App;

            // Configure tracer options

            string logFilenamePath = logPath.Value.ToString() + Path.DirectorySeparatorChar + logName.Value.ToString() + ".log";
            FileStreamWithRolling dailyRolling = new FileStreamWithRolling(logFilenamePath, new TimeSpan(1, 0, 0, 0), FileMode.Append);
            TextWriterTraceListenerWithTime listener = new TextWriterTraceListenerWithTime(dailyRolling);
            Trace.AutoFlush = true;
            TraceFilter fileTraceFilter = new System.Diagnostics.EventTypeFilter(SourceLevels.Verbose);
            listener.Filter = fileTraceFilter;
            System.Diagnostics.Trace.Listeners.Clear();
            System.Diagnostics.Trace.Listeners.Add(listener);

   
            // Check if the registry has been set and overwrite the application defaults

            RegistryKey key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
            string keys = "software\\green\\workflow";
            foreach (string subkey in keys.Split('\\'))
            {
                key = key.OpenSubKey(subkey);
                if (key == null)
                {
                    TraceInternal.TraceVerbose("Failed to open" + subkey);
                    break;
                }
            }
			
            // Get the log path

            try
            {
                if (key.GetValue("logpath", "").ToString().Length > 0)
                {
                    logPath.Value = (string)key.GetValue("logpath", logPath);
                    logPath.Source = Parameter<string>.SourceType.Registry;
                    TraceInternal.TraceVerbose("Use registry value; logPath=" + logPath);
                }
            }
            catch (NullReferenceException)
            {
                TraceInternal.TraceVerbose("Registry error use default values; logPath=" + logPath.Value);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }

            // Get the log name

            try
            {
                if (key.GetValue("logname", "").ToString().Length > 0)
                {
                    logName.Value = (string)key.GetValue("logname", logName);
                    logName.Source = Parameter<string>.SourceType.Registry;
                    TraceInternal.TraceVerbose("Use registry value; LogName=" + logName);
                }
            }
            catch (NullReferenceException)
            {
                TraceInternal.TraceVerbose("Registry error use default values; LogName=" + logName.Value);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }

            // Get the Name
			
            try
            {
                if (key.GetValue("name", "").ToString().Length > 0)
                {
                    appName.Value = (string)key.GetValue("name", appName);
                    appName.Source = Parameter<string>.SourceType.Registry;
                    TraceInternal.TraceVerbose("Use registry value; Name=" + appName);
                }
            }
            catch (NullReferenceException)
            {
                TraceInternal.TraceVerbose("Registry error use default values; Name=" + appName.Value);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }

            // Get the path

            try
            {
                if (key.GetValue("path", "").ToString().Length > 0)
                {
                    appPath.Value = (string)key.GetValue("path", appPath);
                    appPath.Source = Parameter<string>.SourceType.Registry;
                    TraceInternal.TraceVerbose("Use registry value; Path=" + appPath);
                }
            }
            catch (NullReferenceException)
            {
                TraceInternal.TraceVerbose("Registry error use default values; Path=" + appPath.Value);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }

            // Get the traceLevels

            try
            {
                if (key.GetValue("debug", "").ToString().Length > 0)
                {
                    string traceName = (string)key.GetValue("debug", "Verbose");
                    traceName = traceName.TrimStart('"');
                    traceName = traceName.TrimEnd('"');
                    traceLevels.Value = TraceInternal.TraceLookup(traceName);
                    traceLevels.Source = Parameter<SourceLevels>.SourceType.Registry;
                    TraceInternal.TraceVerbose("Use command value Debug=" + traceLevels);
                }
            }
            catch (NullReferenceException)
            {
                Trace.TraceWarning("Registry error use default values; Debug=" + traceLevels.Value);
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
            }

            // Adjust the log location if it has been overridden in the registry

            if (logPath.Source == Parameter<string>.SourceType.Registry)
            {
                logFilenamePath = logPath.Value.ToString() + Path.DirectorySeparatorChar + logName.Value.ToString() + ".log";
            }
            if (logName.Source == Parameter<string>.SourceType.Registry)
            {
                logFilenamePath = logPath.Value.ToString() + Path.DirectorySeparatorChar + logName.Value.ToString() + ".log";
            }

            // Redirect the output

            listener.Flush();
            System.Diagnostics.Trace.Listeners.Remove(listener);
            listener.Close();
            listener.Dispose();

            dailyRolling = new FileStreamWithRolling(logFilenamePath, new TimeSpan(1, 0, 0, 0), FileMode.Append);
            listener = new TextWriterTraceListenerWithTime(dailyRolling);
            Trace.AutoFlush = true;
            SourceLevels sourceLevels = TraceInternal.TraceLookup(traceLevels.Value.ToString());
            fileTraceFilter = new System.Diagnostics.EventTypeFilter(sourceLevels);
            listener.Filter = fileTraceFilter;
            System.Diagnostics.Trace.Listeners.Add(listener);
        

            TraceInternal.TraceInformation("Use Name=" + appName.Value);
            TraceInternal.TraceInformation("Use Path=" + appPath.Value);
            TraceInternal.TraceInformation("Use Log Name=" + logName.Value);
            TraceInternal.TraceInformation("Use Log Path=" + logPath.Value);

            // read in the xml config file and process the workflow

            Serialise serialise = new Serialise();
            if (appPath.Value.ToString().Length > 0)
            {
                serialise.Path = appPath.Value.ToString();
            }

            if (appName.Value.ToString().Length > 0)
            {
                serialise.Filename = appName.Value.ToString();
            }
            processData = serialise.DeserialiseProcess();

            if (processData != null)
            {
                // link up the objects

                Serialise.LinkObjects(processData);

                // Link up the pipes

                Serialise.JoinPipes(processData);

                // Launch the job threads

                foreach (object item in processData)
	            {
	                if (item.GetType() == typeof(WorkflowLibrary.Process))
	                {
	                    WorkflowLibrary.Process p = (WorkflowLibrary.Process)item;
	                    p.Update();
	                    Thread processThread = new Thread(new ThreadStart(p.Start));
	                    processThread.Start();
	                }
                else if (item.GetType() == typeof(WorkflowLibrary.Job))
                    {
                        Job j = (Job)item;
	                    if ((jobId.Length == 0) || (j.ID == jobId))
	                    {
	                        //j.Update();
	                        Thread jobThread = new Thread(new ThreadStart(j.Start));
	                        TraceInternal.TraceInformation("Start job " + j.ID);
	                        jobThread.Start();
	                    }
	                    else
	                    {
	                        TraceInternal.TraceVerbose("Not starting job " + j.ID);
	                    }
                    }
                    else if (item.GetType() == typeof(WorkflowLibrary.Event))
                    {
                        Event e = (Event)item;
                        e.Update();
                        Thread eventThread = new Thread(new ThreadStart(e.Start));
                        TraceInternal.TraceInformation("Start event " + e.ID);
                        eventThread.Start();
                    }
                }
            }
            // Need to provide an interface to review what is happening to the jobs.tasks.items

            config = new WorkflowLibrary.Server();
            config.MessageReceived += new Server.MessageReceivedHandler(MessageReceived);
            config.PipeName = "\\\\.\\pipe\\workflow";
            config.Start();

            Debug.WriteLine("Out ServiceWorkerMethod()");
        }

        void MessageReceived(Server.Client client, string message)
        {
            Debug.WriteLine("In MessageReceived()");
            string response;
            response = Messaging.Decoder(processData, message);

            // possibly need to check the processData for any newly added jobs that have a state of none.

            foreach (object item in processData)
            {
                if (item.GetType() == typeof(Job))
                {
                    Job j = (Job)item;
                    if (j.State == Job.StateType.None)  // Job not started
                    {
                        j.Update();
                        Thread jobThread = new Thread(new ThreadStart(j.Start));
                        jobThread.Start();
                    }
                }
                else if (item.GetType() == typeof(WorkflowLibrary.Event))
                {
                    Event e = (Event)item;
                    if (e.State == Job.StateType.None)  // Job not started
                    {
                        e.Update();
                        Thread eventThread = new Thread(new ThreadStart(e.Start));
                        eventThread.Start();
                    }
                }
            }

            config.SendMessage(response);
            TraceInternal.TraceVerbose("response=" + response);
            Debug.WriteLine("Out MessageReceived()");
        }

        // Example of entry point for additional commands
        // Thus might be usefull for the GUI service controler to enable adding
        // adding new workflow, perhaps do a workflow.xml refreth, rather then stopping the service.

        protected override void OnCustomCommand(int command)
        {
            Debug.WriteLine("In OnCustomCommand()");
            base.OnCustomCommand(command);
            if (command == (int)Commands.Log)
            {
                eventLog.WriteEntry("Check Logging");
                TraceInternal.TraceVerbose("Check Logging");
            }
            Debug.WriteLine("Out OnCustomCommand()");
        }
    
    }
}
