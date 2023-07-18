using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using JobsLibrary;
using TracerLibrary;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.ObjectModel;

namespace JobsTerminal
{
    class Program
    {
	    #region Fields
	
        static Collection<Object> processData = new Collection<Object>();

        #endregion
        #region Constructor
        #endregion
        #region Methods
        static void Main(string[] args)
        {
            Debug.WriteLine("In Main()");
			
			// Read in specific configuration

            string jobId = "";

            Parameter appPath = new Parameter("");
            Parameter appName = new Parameter("process.xml");
            appPath.Value = System.Reflection.Assembly.GetExecutingAssembly().Location;

            int pos = appPath.Value.ToString().LastIndexOf(Path.DirectorySeparatorChar);
            if (pos > 0)
            {
                appPath.Value = appPath.Value.ToString().Substring(0, pos);
                appPath.Source = Parameter.SourceType.App;
            }

            Parameter logPath = new Parameter("");
            Parameter logName = new Parameter("jobsterminal");
            logPath.Value = System.Reflection.Assembly.GetExecutingAssembly().Location;
            pos = logPath.Value.ToString().LastIndexOf(Path.DirectorySeparatorChar);
            if (pos > 0)
            {
                logPath.Value = logPath.Value.ToString().Substring(0, pos);
                logPath.Source = Parameter.SourceType.App;
            }

            // Configure tracer options

            string filenamePath = logPath.Value.ToString() + Path.DirectorySeparatorChar + logName.Value.ToString() + ".log";
            FileStreamWithRolling dailyRolling = new FileStreamWithRolling(filenamePath, new TimeSpan(1, 0, 0, 0), FileMode.Append);
            TextWriterTraceListenerWithTime listener = new TextWriterTraceListenerWithTime(dailyRolling);
            Trace.AutoFlush = true;
            TraceFilter fileTraceFilter = new System.Diagnostics.EventTypeFilter(SourceLevels.Verbose);
            listener.Filter = fileTraceFilter;
            Trace.Listeners.Clear();
            Trace.Listeners.Add(listener);

            ConsoleTraceListener console = new ConsoleTraceListener();
            TraceFilter consoleTraceFilter = new System.Diagnostics.EventTypeFilter(SourceLevels.Information);
            console.Filter = consoleTraceFilter;
            Trace.Listeners.Add(console);

            // Check if the config file has been paased in and overwrite the defaults
            filenamePath = "";
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
                    appPath.Source = Parameter.SourceType.Command;
                    appName.Value = filenamePath.Substring(pos + 1, filenamePath.Length - pos - 1);
                    appName.Value = appName.Value.ToString() + ".job";
                    appName.Source = Parameter.SourceType.Command;
                    TraceInternal.TraceVerbose("Use command value Path=" + appPath);
                    TraceInternal.TraceVerbose("Use command value Name=" + appName);
                }
                else
                {
                    appName.Value = filenamePath;
                    appName.Value = appName.Value.ToString() + ".job";
                    appName.Source = Parameter.SourceType.Command;
                    TraceInternal.TraceVerbose("Use command value Name=" + appName);
                }
            }
            else
            {
                for (int item = 0; item < items; item++)
                {
                    switch (args[item])
                    {
                        case "/N":
                        case "--name":
                            appName.Value = args[item + 1];
                            appName.Value = appName.Value.ToString().TrimStart('"');
                            appName.Value = appName.Value.ToString().TrimEnd('"');
                            appName.Source = Parameter.SourceType.Command;
                            TraceInternal.TraceVerbose("Use command value Name=" + appName);
                            break;
                        case "/P":
                        case "--path":
                            appPath.Value = args[item + 1];
                            appPath.Value = appPath.Value.ToString().TrimStart('"');
                            appPath.Value = appPath.Value.ToString().TrimEnd('"');
                            appPath.Source = Parameter.SourceType.Command;
                            TraceInternal.TraceVerbose("Use command value Path=" + appPath);
                            break;
                        case "/n":
                        case "--logname":
                            logName.Value = args[item + 1];
                            logName.Value = logName.Value.ToString().TrimStart('"');
                            logName.Value = logName.Value.ToString().TrimEnd('"');
                            logName.Source = Parameter.SourceType.Command;
                            TraceInternal.TraceVerbose("Use command value logName=" + logName);
                            break;
                        case "/p":
                        case "--logpath":
                            logPath.Value = args[item + 1];
                            logPath.Value = logPath.Value.ToString().TrimStart('"');
                            logPath.Value = logPath.Value.ToString().TrimEnd('"');
                            logPath.Source = Parameter.SourceType.Command;
                            TraceInternal.TraceVerbose("Use command value logPath=" + logPath);
                            break;
                        case "/I":
                        case "--id":
                            jobId = args[item + 1];
                            jobId = jobId.TrimStart('"');
                            jobId = jobId.TrimEnd('"');
                            TraceInternal.TraceVerbose("Use command value id=" + jobId);
                            break;
                    }
                }
            }

            if ((logPath.Source != Parameter.SourceType.App) || (logPath.Source != Parameter.SourceType.App))
            {
                // Adjust the log location if it has been overridden in the registry
                // This is an interim measure until can add in the naming mask

                Trace.Listeners.Remove(listener);
                filenamePath = logPath.Value.ToString() + Path.DirectorySeparatorChar + logName.Value.ToString() + ".log";
                dailyRolling = new FileStreamWithRolling(filenamePath, new TimeSpan(1, 0, 0, 0), FileMode.Append);
                listener = new TextWriterTraceListenerWithTime(dailyRolling);
                Trace.AutoFlush = true;
                fileTraceFilter = new System.Diagnostics.EventTypeFilter(SourceLevels.Verbose);
                listener.Filter = fileTraceFilter;
                Trace.Listeners.Clear();
                Trace.Listeners.Add(listener);
            }

            TraceInternal.TraceInformation("Use Name=" + appName.Value);
            TraceInternal.TraceInformation("Use Path =" + appPath.Value);
            TraceInternal.TraceInformation("Use Log Name=" + logName.Value);
            TraceInternal.TraceInformation("Use Log Path=" + logPath.Value);

            // read in the xml config file and process the jobs

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

            // link up the objects

            Serialise.LinkObjects(processData);

            // Link up the pipes

            Serialise.JoinPipes(processData);

            // Launch the job threads

            foreach (object item in processData)
            {
                if (item.GetType() == typeof(JobsLibrary.Process))
                {
                    JobsLibrary.Process p = (JobsLibrary.Process)item;
                    p.Update();
                    Thread processThread = new Thread(new ThreadStart(p.Start));
                    processThread.Start();
                }
                else if (item.GetType() == typeof(Job))
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
                else if (item.GetType() == typeof(Event))
                {
                    Event e = (Event)item;
                    e.Update();
                    Thread eventThread = new Thread(new ThreadStart(e.Start));
                    eventThread.Start();
                }
            }

            ManualResetEvent manualResetEvent = new ManualResetEvent(false);

            manualResetEvent.WaitOne();

            Debug.WriteLine("Out Main()");
        }

        #endregion
    }
}
