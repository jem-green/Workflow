using Microsoft.VisualBasic;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using WorkflowLibrary;
using Process = WorkflowLibrary.Process;
using TracerLibrary;

namespace WorkflowTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Workflow Pattern Tests ===\n");

            string logFilePath = "workflow_test.log";
            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
            TextWriterTraceListenerWithTime fileListener = new TracerLibrary.TextWriterTraceListenerWithTime(logFilePath);
            TraceFilter traceFilter = new System.Diagnostics.EventTypeFilter(SourceLevels.Verbose);
            fileListener.Filter = traceFilter;
            Trace.Listeners.Add(fileListener);

            ConsoleTraceListener console = new ConsoleTraceListener();
            traceFilter = new System.Diagnostics.EventTypeFilter(SourceLevels.Verbose);
            console.Filter = traceFilter;
            System.Diagnostics.Trace.Listeners.Add(console);

            //SequencePattern.Run();            // WCP-1 
            //ParallelSplitPattern.Run();       // WCP-2
            //SynchronizationPattern.Run();     // WCP-3
            ExclusiveChoicePattern.Run();     // WCP-4
            //SimpleMergePattern.Run();         // WCP-5
            //MultiChoicePattern.Run();         // WCP-6
            //
            //MultiMergePattern.Run();          // WCP-8

            //GroupingTest.Run();


            Console.WriteLine("\n=== Tests Completed ===");
            Console.ReadKey();

            fileListener.Flush();

        }
    }
}
        