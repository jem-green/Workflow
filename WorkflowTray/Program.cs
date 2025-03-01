using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TracerLibrary;
using Microsoft.Win32;
using System.Diagnostics;

namespace WorkflowTray
{
    static class Program
    {     
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string appPath = "";
            string appName = "process.xml";

            appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            try
            {
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

                if (key.GetValue("path", "").ToString() != "")
                {
                    appPath = (string)key.GetValue("path", appPath);
                    TraceInternal.TraceVerbose("Use registry value Name=" + appName);
                }
                if (key.GetValue("name", "").ToString() != "")
                {
                    appName = (string)key.GetValue("name", appName);
                    TraceInternal.TraceVerbose("Use registry value Path=" + appPath);
                }
            }
            catch
            {
                Trace.TraceError("Registry error use default values; Name=" + appName + " Path=" + appPath);
            }

            // Check if the config file has been paased in and overwrite the registry

            string[] args = Environment.GetCommandLineArgs();

            for (int item = 0; item < args.Length; item++)
            {
                switch (args[item])
                {
                    case "-N":
                    case "--name":
                        appName = args[item + 1];
                        appName = appName.TrimStart('"');
                        appName = appName.TrimEnd('"');
                        TraceInternal.TraceVerbose("Use command value Name=" + appName);
                        break;
                    case "-P":
                    case "--path":
                        appPath = args[item + 1];
                        appPath = appPath.TrimStart('"');
                        appPath = appPath.TrimEnd('"');
                        TraceInternal.TraceVerbose("Use command value Path=" + appPath);
                        break;
                }
            }

            TraceInternal.TraceVerbose("Use Name=" + appName);
            TraceInternal.TraceVerbose("Use Path=" + appPath);

            // Show the system tray icon.					
            using (ProcessIcon pi = new ProcessIcon())
            {
                pi.Display();
                pi.Start(appName,appPath);

                // Make sure the application runs!

                Application.Run();
            }

        }
    }
}
