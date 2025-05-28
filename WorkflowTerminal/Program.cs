using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using TracerLibrary;
using WorkflowLibrary;

namespace WorkflowTerminal
{
    class Program
    {
        #region Fields

        static WorkflowLibrary.Server config;
        static Collection<Object> processData = new Collection<Object>();

        #endregion
        #region Constructor
        #endregion
        #region Methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            Debug.WriteLine("Enter Main()");

            // Read in specific configuration

            string jobId = "";

            Parameter<string> appPath = new Parameter<string>("appPath", "");
            Parameter<string> appName = new Parameter<string>("appName", "process.cfg");
            appPath.Value = System.Reflection.Assembly.GetExecutingAssembly().Location;

            int pos = appPath.Value.ToString().LastIndexOf(Path.DirectorySeparatorChar);
            if (pos > 0)
            {
                appPath.Value = appPath.Value.ToString().Substring(0, pos);
                appPath.Source = IParameter.SourceType.App;
            }

            Parameter<string> logPath = new Parameter<string>("logPath", "");
            Parameter<string> logName = new Parameter<string>("logName", "workflowterminal");
            logPath.Value = System.Reflection.Assembly.GetExecutingAssembly().Location;
            pos = logPath.Value.ToString().LastIndexOf(Path.DirectorySeparatorChar);
            if (pos > 0)
            {
                logPath.Value = logPath.Value.ToString().Substring(0, pos);
                logPath.Source = IParameter.SourceType.App;
            }

            Parameter<SourceLevels> traceLevels = new Parameter<SourceLevels>("traceLavels", SourceLevels.Verbose);
            traceLevels.Value = TraceInternal.TraceLookup("VERBOSE");
            traceLevels.Source = IParameter.SourceType.App;

            // Configure tracer options

            string logFilenamePath = logPath.Value.ToString() + Path.DirectorySeparatorChar + logName.Value.ToString() + ".log";
            FileStreamWithRolling dailyRolling = new FileStreamWithRolling(logFilenamePath, new TimeSpan(1, 0, 0, 0), FileMode.Append);
            TextWriterTraceListenerWithTime listener = new TextWriterTraceListenerWithTime(dailyRolling);
            Trace.AutoFlush = true;
            TraceFilter fileTraceFilter = new System.Diagnostics.EventTypeFilter(SourceLevels.Verbose);
            listener.Filter = fileTraceFilter;
            System.Diagnostics.Trace.Listeners.Clear();
            System.Diagnostics.Trace.Listeners.Add(listener);

            ConsoleTraceListener console = new ConsoleTraceListener();
            TraceFilter consoleTraceFilter = new System.Diagnostics.EventTypeFilter(SourceLevels.Information);
            console.Filter = consoleTraceFilter;
            System.Diagnostics.Trace.Listeners.Add(console);

            // Check if the config file has been passed in and overwrite the defaults
            string filenamePath = "";
            string extension = "";
            int items = args.Length;
            if (items == 1)
            {
                int index = 0;
                filenamePath = args[index].Trim('"');
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
                    appPath.Source = IParameter.SourceType.Command;
                    appName.Value = filenamePath.Substring(pos + 1, filenamePath.Length - pos - 1);
                    appName.Value = appName.Value.ToString() + ".job";
                    appName.Source = IParameter.SourceType.Command;
                }
                else
                {
                    appName.Value = filenamePath;
                    appName.Value = appName.Value.ToString() + ".job";
                    appName.Source = IParameter.SourceType.Command;
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
                                traceLevels.Source = IParameter.SourceType.Command;
                                TraceInternal.TraceVerbose("Use command value Debug=" + traceLevels);
                                break;
                            }
                        case "/N":
                        case "--name":
                            {
                                appName.Value = args[item + 1];
                                appName.Value = appName.Value.ToString().TrimStart('"');
                                appName.Value = appName.Value.ToString().TrimEnd('"');
                                appName.Source = IParameter.SourceType.Command;
                                TraceInternal.TraceVerbose("Use command value Name=" + appName);
                                break;
                            }
                        case "/P":
                        case "--path":
                            {
                                appPath.Value = args[item + 1];
                                appPath.Value = appPath.Value.ToString().TrimStart('"');
                                appPath.Value = appPath.Value.ToString().TrimEnd('"');
                                appPath.Source = IParameter.SourceType.Command;
                                TraceInternal.TraceVerbose("Use command value Path=" + appPath);
                                break;
                            }
                        case "/n":
                        case "--logname":
                            {
                                logName.Value = args[item + 1];
                                logName.Value = logName.Value.ToString().TrimStart('"');
                                logName.Value = logName.Value.ToString().TrimEnd('"');
                                logName.Source = IParameter.SourceType.Command;
                                TraceInternal.TraceVerbose("Use command value logName=" + logName);
                                break;
                            }
                        case "/p":
                        case "--logpath":
                            {
                                logPath.Value = args[item + 1];
                                logPath.Value = logPath.Value.ToString().TrimStart('"');
                                logPath.Value = logPath.Value.ToString().TrimEnd('"');
                                logPath.Source = IParameter.SourceType.Command;
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

                if (logPath.Source == IParameter.SourceType.Command)
                {
                    logFilenamePath = logPath.Value.ToString() + Path.DirectorySeparatorChar + logName.Value.ToString() + ".log";
                }
                if (logName.Source == IParameter.SourceType.Command)
                {
                    logFilenamePath = logPath.Value.ToString() + Path.DirectorySeparatorChar + logName.Value.ToString() + ".log";
                }
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
                        TraceInternal.TraceInformation("Cannot start item " + item.GetType());
                    }
                }


                // Need to provide an interface to review what is happening to the workflow.tasks.items

                config = new WorkflowLibrary.Server();
                config.MessageReceived += new Server.MessageReceivedHandler(MessageReceived);
                config.PipeName = "\\\\.\\pipe\\workflow";
                config.Start();

            }
            else
            {
                TraceInternal.TraceError("No data to process");
            }

            Debug.WriteLine("Exit Main()");
        }

        #endregion

        #region Private
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

        #endregion

    }
}
