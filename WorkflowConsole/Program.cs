using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using TracerLibrary;
using WorkflowLibrary;

namespace WorkflowConsole
{
    class Program
    {
        #region Fields

        static WorkflowLibrary.Server config;
        static Collection<Object> processData = new Collection<Object>();
        public static bool isClosing = false;
        static private HandlerRoutine ctrlCHandler;

        #endregion
        #region Unmanaged

        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        #endregion
        #region Methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Debug.WriteLine("Enter Main()");

            ctrlCHandler = new HandlerRoutine(ConsoleCtrlCheck);
            SetConsoleCtrlHandler(ctrlCHandler, true);

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

            Parameter<string> logPath = new Parameter<string>("logPath","");
            Parameter<string> logName = new Parameter<string>("lagName","workflowconsole");
            logPath.Value = System.Reflection.Assembly.GetExecutingAssembly().Location;
            pos = logPath.Value.ToString().LastIndexOf(Path.DirectorySeparatorChar);
            if (pos > 0)
            {
                logPath.Value = logPath.Value.ToString().Substring(0, pos);
                logPath.Source = Parameter<string>.SourceType.App;
            }

            Parameter<SourceLevels> traceLevels = new Parameter<SourceLevels>("traceLavels");
            traceLevels.Value = TraceInternal.TraceLookup("VERBOSE");
            traceLevels.Source = Parameter<SourceLevels>.SourceType.App;

            // Configure tracer options

            string logFilenamePath = logPath.Value.ToString() + Path.DirectorySeparatorChar + logName.Value.ToString() + ".log";
            FileStreamWithRolling dailyRolling = new FileStreamWithRolling(logFilenamePath, new TimeSpan(1, 0, 0, 0), FileMode.Append);
            TextWriterTraceListenerWithTime listener = new TextWriterTraceListenerWithTime(dailyRolling);
            Trace.AutoFlush = true;
            TraceFilter fileTraceFilter = new System.Diagnostics.EventTypeFilter(SourceLevels.Verbose);
            listener.Filter = fileTraceFilter;
            Trace.Listeners.Clear();
            Trace.Listeners.Add(listener);

            ConsoleTraceListener console = new ConsoleTraceListener();
            TraceFilter consoleTraceFilter = new System.Diagnostics.EventTypeFilter(SourceLevels.Verbose);
            console.Filter = consoleTraceFilter;
            Trace.Listeners.Add(console);

            if (IsLinux == false)
            {

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

                // Get the name

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
            }


            // Check if the config file has been passed in and overwrite the registry

            string filenamePath = "";
            string extension = "";
            int items = args.Length;
            if (items == 1)
            {
                filenamePath = args[0].Trim('"');
                pos = filenamePath.LastIndexOf('.');
                if (pos > 0)
                {
                    extension = filenamePath.Substring(pos + 1, filenamePath.Length - pos - 1);
                    filenamePath = filenamePath.Substring(0, pos);
                }

                pos = filenamePath.LastIndexOf('\\');
                if (pos > 0)
                {
                    appPath.Value = filenamePath.Substring(0, pos);
                    appPath.Source = Parameter<string>.SourceType.Command;
                    appName.Value = filenamePath.Substring(pos + 1, filenamePath.Length - pos - 1);
                    appName.Value = appName.Value.ToString() + ".job";
                    appName.Source = Parameter<string>.SourceType.Command;

                }
                else
                {
                    appName.Value = filenamePath;
                    appName.Value = appName.Value.ToString() + ".job";
                    appName.Source = Parameter<string>.SourceType.Command;
                }
                TraceInternal.TraceVerbose("Use command value Path=" + appPath);
                TraceInternal.TraceVerbose("Use command value Name=" + appName);
            }
            else
            {
                for (int item = 0; item < items; item++)
                {
                    string lookup = args[item];
                    if (!lookup.StartsWith("/"))
                    {
                        lookup = lookup.ToLower();
                    }
                    switch (lookup)
                    {
                        case "/D":
                        case "--debug":
                            {
                                string traceName = args[item + 1];
                                traceName = traceName.TrimStart('"');
                                traceName = traceName.TrimEnd('"');
                                traceLevels.Value = TraceInternal.TraceLookup(traceName);
                                traceLevels.Source = Parameter<SourceLevels>.SourceType.Command;
                                TraceInternal.TraceVerbose("Use command value Debug=" + traceLevels);
                                break;
                            }
                        case "/N":
                        case "--name":
                            {
                                appName.Value = args[item + 1];
                                appName.Value = appName.Value.ToString().TrimStart('"');
                                appName.Value = appName.Value.ToString().TrimEnd('"');
                                appName.Source = Parameter<string>.SourceType.Command;
                                TraceInternal.TraceVerbose("Use command value Name=" + appName);
                                break;
                            }
                        case "/P":
                        case "--path":
                            {
                                appPath.Value = args[item + 1];
                                appPath.Value = appPath.Value.ToString().TrimStart('"');
                                appPath.Value = appPath.Value.ToString().TrimEnd('"');
                                appPath.Source = Parameter<string>.SourceType.Command;
                                TraceInternal.TraceVerbose("Use command value Path=" + appPath);
                                break;
                            }
                        case "/n":
                        case "--logname":
                            {
                                logName.Value = args[item + 1];
                                logName.Value = logName.Value.ToString().TrimStart('"');
                                logName.Value = logName.Value.ToString().TrimEnd('"');
                                logName.Source = Parameter<string>.SourceType.Command;
                                TraceInternal.TraceVerbose("Use command value logName=" + logName);
                                break;
                            }
                        case "/p":
                        case "--logpath":
                            {
                                logPath.Value = args[item + 1];
                                logPath.Value = logPath.Value.ToString().TrimStart('"');
                                logPath.Value = logPath.Value.ToString().TrimEnd('"');
                                logPath.Source = Parameter<string>.SourceType.Command;
                                TraceInternal.TraceVerbose("Use command value logPath=" + logPath);
                                break;
                            }
                        case "/I":
                        case "--id":
                            {
                                jobId = args[item + 1];
                                jobId = jobId.TrimStart('"');
                                jobId = jobId.TrimEnd('"');
                                TraceInternal.TraceVerbose("Use command value id=" + jobId);
                                break;
                            }
                    }
                }

                // Adjust the log location if it has been overridden in the command line

                if (logPath.Source == Parameter<string>.SourceType.Command)
                {
                    logFilenamePath = logPath.Value.ToString() + Path.DirectorySeparatorChar + logName.Value.ToString() + ".log";
                }
                if (logName.Source == Parameter<string>.SourceType.Command)
                {
                    logFilenamePath = logPath.Value.ToString() + Path.DirectorySeparatorChar + logName.Value.ToString() + ".log";
                }
            }

            // Redirect the output

            listener.Flush();
            Trace.Listeners.Remove(listener);
            listener.Close();
            listener.Dispose();

            dailyRolling = new FileStreamWithRolling(logFilenamePath, new TimeSpan(1, 0, 0, 0), FileMode.Append);
            listener = new TextWriterTraceListenerWithTime(dailyRolling);
            Trace.AutoFlush = true;
            SourceLevels sourceLevels = TraceInternal.TraceLookup(traceLevels.Value.ToString());
            fileTraceFilter = new System.Diagnostics.EventTypeFilter(sourceLevels);
            listener.Filter = fileTraceFilter;
            Trace.Listeners.Add(listener);

            TraceInternal.TraceInformation("Use Name=" + appName.Value);
            TraceInternal.TraceInformation("Use Path=" + appPath.Value);
            TraceInternal.TraceInformation("Use Log Name=" + logName.Value);
            TraceInternal.TraceInformation("Use Log Path=" + logPath.Value);


            // read in the XML config file and process the workflow

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
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            // Need to provide an interface to review what is happening to the workflow.tasks.items

            config = new WorkflowLibrary.Server();
            config.MessageReceived += new Server.MessageReceivedHandler(MessageReceived);
            config.PipeName = "\\\\.\\pipe\\workflow";
            config.Start();

            Debug.WriteLine("Out Main()");
        }

        #endregion
        #region Private

        private static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        private static void MessageReceived(Server.Client client, string message)
        {
            Debug.WriteLine("In MessageReceived()");
            string response;
            response = Messaging.Decoder(processData, message);

            // possibly need to check the processData for any newly added workflow that have a state of none.

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

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            Debug.WriteLine("In ConsoleCtrlCheck()");

            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                    {
                        isClosing = true;
                        TraceInternal.TraceInformation("CTRL+C received:");
                        break;
                    }

                case CtrlTypes.CTRL_BREAK_EVENT:
                    {
                        isClosing = true;
                        TraceInternal.TraceInformation("CTRL+BREAK received:");
                        break;
                    }
                case CtrlTypes.CTRL_CLOSE_EVENT:
                    {
                        isClosing = true;
                        TraceInternal.TraceInformation("Program being closed:");
                        break;
                    }
                case CtrlTypes.CTRL_LOGOFF_EVENT:
                case CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    {
                        isClosing = true;
                        TraceInternal.TraceInformation("User is logging off:");
                        break;
                    }
            }

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
                    TraceInternal.TraceVerbose("Other event type" + item);
                }
            }

            Debug.WriteLine("Exit ConsoleCtrlCheck()");

            Environment.Exit(0);

            return (true);

        }

        #endregion
    }
}
