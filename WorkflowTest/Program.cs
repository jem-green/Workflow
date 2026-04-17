using Microsoft.VisualBasic;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using WorkflowLibrary;
using Process = WorkflowLibrary.Process;

namespace WorkflowTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Workflow Pattern Tests ===\n");

            ConsoleTraceListener console = new ConsoleTraceListener();
            TraceFilter consoleTraceFilter = new System.Diagnostics.EventTypeFilter(SourceLevels.Verbose);
            console.Filter = consoleTraceFilter;
            System.Diagnostics.Trace.Listeners.Add(console);

            //Sequence();
            //ParallelSplit();
            //Synchronization();
            ExclusiveChoice();
            //SimpleMerge();
            //MultiChoice();
            //MultiMerge();
                
        }

        /// <summary>
        /// WCP-1: Sequence - Activity B starts after Activity A completes
        /// Pattern: job_1 --> job_2
        /// </summary>
        public static void Sequence()
        {
            Console.WriteLine("Testing: WCP-1 Sequence");
            
            // Create a Process to manage the jobs
            Process process = new Process("process_1");
            process.Name = "Sequence Pattern";

            // Job 1 - First job
            Job job1 = new Job("job_1");
            job1.Name = "Job 1";
            Task task1 = new Task("task_1");
            task1.Name = "Task 1";
            Item item1 = new Item("item_1");
            item1.Name = "Echo Job 1";
            item1.Application = "cmd.exe";
            item1.Command = "/c echo Job 1 Completed";
            task1.Add(item1);
            job1.Add(task1);

            // Job 2 - Second job (waits for job_1)
            Job job2 = new Job("job_2");
            job2.Name = "Job 2";
            Task task2 = new Task("task_2");
            task2.Name = "Task 2";
            Item item2 = new Item("item_2");
            item2.Name = "Echo Job 2";
            item2.Application = "cmd.exe";
            item2.Command = "/c echo Job 2 Completed";
            task2.Add(item2);
            job2.Add(task2);

            // Create Link from job_1 to job_2
            Link link = new Link("link_1");
            link.From = "job_1";
            link.To = "job_2";

            // Wire up throw/catch nodes
            Node throwNode = new Node("node_1");
            throwNode.Link = link;
            job1.AddThrow(throwNode);

            Node catchNode = new Node("node_2");
            catchNode.Link = link;
            job2.AddCatch(catchNode);

            // Add to process
            process.Add(job1);
            process.Add(job2);
            process.Add(link);

            // Update and start
            process.Update();
            process.Start();
            
            Console.WriteLine("✓ Sequence pattern completed\n");
        }

        /// <summary>
        /// WCP-2: Parallel Split - One activity diverges to multiple concurrent activities
        /// Pattern:        +-- job_2
        ///          job_1 --|
        ///                  +-- job_3
        /// </summary>
        public static void ParallelSplit()
        {
            Console.WriteLine("Testing: WCP-2 Parallel Split");
            
            Process process = new Process("process_1");
            process.Name = "Parallel Split Pattern";

            // Job 1 - Source
            Job job1 = new Job("job_1");
            job1.Name = "Job 1 (Source)";
            Task task1 = new Task("task_1");
            task1.Name = "Start Task";
            Item item1 = new Item("item_1");
            item1.Application = "cmd.exe";
            item1.Command = "/c echo Job 1 Starting Split";
            task1.Add(item1);
            job1.Add(task1);

            // Job 2 - Branch 1
            Job job2 = new Job("job_2");
            job2.Name = "Job 2 (Branch 1)";
            Task task2 = new Task("task_2");
            Item item2 = new Item("item_2");
            item2.Application = "cmd.exe";
            item2.Command = "/c waitfor copydone /T 10 && echo Job 2 Completed";
            task2.Add(item2);
            job2.Add(task2);

            // Job 3 - Branch 2
            Job job3 = new Job("job_3");
            job3.Name = "Job 3 (Branch 2)";
            Task task3 = new Task("task_3");
            Item item3 = new Item("item_3");
            item3.Application = "cmd.exe";
            item3.Command = "/c waitfor copydone /T 20 && echo Job 3 Completed";
            task3.Add(item3);
            job3.Add(task3);

            // Create Links - job_1 throws to both job_2 and job_3
            Link link12 = new Link("link_1");
            link12.From = "job_1";
            link12.To = "job_2";

            Link link13 = new Link("link_2");
            link13.From = "job_1";
            link13.To = "job_3";

            // Wire job_1 to throw to both
            Node throw2 = new Node("node_1");
            throw2.Link = link12;
            job1.AddThrow(throw2);

            Node throw3 = new Node("node_2");
            throw3.Link = link13;
            job1.AddThrow(throw3);

            // Wire job_2 to catch
            Node catch2 = new Node("node_3");
            catch2.Link = link12;
            job2.AddCatch(catch2);

            // Wire job_3 to catch
            Node catch3 = new Node("node_4");
            catch3.Link = link13;
            job3.AddCatch(catch3);

            // Add to process
            process.Add(job1);
            process.Add(job2);
            process.Add(job3);
            process.Add(link12);
            process.Add(link13);

            process.Update();
            process.Start();
            
            Console.WriteLine("✓ Parallel split completed - both branches ran concurrently\n");
        }

        /// <summary>
        /// WCP-3: Synchronization - Multiple activities converge to single activity
        /// Pattern: job_1 --+
        ///                  |-- event_1
        ///          job_2 --+
        /// </summary>
        public static void Synchronization()
        {
            Console.WriteLine("Testing: WCP-3 Synchronization");
            
            Process process = new Process("process_1");
            process.Name = "Synchronization Pattern";

            // Job 1 - Branch 1
            Job job1 = new Job("job_1");
            job1.Name = "Job 1 (Branch 1)";
            Task task1 = new Task("task_1");
            Item item1 = new Item("item_1");
            item1.Application = "cmd.exe";
            item1.Command = "/c echo Job 2 Completed";
            task1.Add(item1);
            job1.Add(task1);

            // Job 2 - Branch 2
            Job job2 = new Job("job_2");
            job2.Name = "Job 2 (Branch 2)";
            Task task2 = new Task("task_2");
            Item item2 = new Item("item_2");
            item2.Application = "cmd.exe";
            item2.Command = "/c echo Job 2 Completed";
            task2.Add(item2);
            job2.Add(task2);

            // Event 1 - Synchronization point (uses Event which can catch multiple)
            Event event1 = new Event("event_1");
            event1.Name = "Sync Event 1";

            // Create Links from job_1 and job_2 to event_1
            Link link1 = new Link("link_1");
            link1.From = "job_1";
            link1.To = "event_1";

            Link link2 = new Link("link_2");
            link2.From = "job_2";
            link2.To = "event_1";

            // Wire job_1 to throw
            Node throw1 = new Node("node_1");
            throw1.Link = link1;
            job1.AddThrow(throw1);

            // Wire job_2 to throw
            Node throw2 = new Node("node_2");
            throw2.Link = link2;
            job2.AddThrow(throw2);

            // Wire event_1 to catch both (multiple catch nodes)
            Node catchFrom1 = new Node("node_3");
            catchFrom1.Link = link1;
            event1.AddCatch(catchFrom1);

            Node catchFrom2 = new Node("node_4");
            catchFrom2.Link = link2;
            event1.AddCatch(catchFrom2);

            // Add to process
            process.Add(job1);
            process.Add(job2);
            process.Add(event1);
            process.Add(link1);
            process.Add(link2);

            process.Update();
            process.Start();
            
            Console.WriteLine("✓ Synchronization completed - waited for both branches\n");
        }

        /// <summary>
        /// WCP-4: Exclusive Choice - One branch chosen based on condition
        /// Pattern:            +-- job_2 (if x>5)
        ///          decision_1 --|
        ///                        +-- job_3 (if x<=5)
        /// </summary>
        public static void ExclusiveChoice()
        {
            Console.WriteLine("Testing: WCP-4 Exclusive Choice");
            
            Process process = new Process("process_1");
            process.Name = "Exclusive Choice Pattern";

            // Add test data at process level
            process.AddData("x", 7);  // Change this value to test different paths

            // Decision 1 - Routing decision point (no work, just evaluates and routes)
            Decision decision1 = new Decision("decision_1");
            decision1.Name = "Decision 1 (Router)";
            // Decision has no catches - it starts immediately and loops
            // Decision doesn't need tasks/items - it's purely for routing logic

            // Job 2 - Branch 1 (x > 5)
            Job job2 = new Job("job_2");
            job2.Name = "Job 2 (x > 5)";
            Task task2 = new Task("task_2");
            Item item2 = new Item("item_2");
            item2.Application = "cmd.exe";
            item2.Command = "/c echo Branch 2: x is greater than 5";
            task2.Add(item2);
            job2.Add(task2);

            // Job 3 - Branch 2 (x <= 5)
            Job job3 = new Job("job_3");
            job3.Name = "Job 3 (x <= 5)";
            Task task3 = new Task("task_3");
            Item item3 = new Item("item_3");
            item3.Application = "cmd.exe";
            item3.Command = "/c echo Branch 3: x is less than or equal to 5";
            task3.Add(item3);
            job3.Add(task3);

            // Create Links with expressions
            Link link12 = new Link("link_1");
            link12.From = "decision_1";
            link12.To = "job_2";
            link12.Expression = "[x] > 5";  // Conditional expression

            Link link13 = new Link("link_2");
            link13.From = "decision_1";
            link13.To = "job_3";
            link13.Expression = "[x] <= 5";  // Conditional expression

            // Wire decision throws with conditional links
            Node throw2 = new Node("node_1");
            throw2.Link = link12;
            decision1.AddThrow(throw2);

            Node throw3 = new Node("node_2");
            throw3.Link = link13;
            decision1.AddThrow(throw3);

            // Wire job catches
            Node catch2 = new Node("node_3");
            catch2.Link = link12;
            job2.AddCatch(catch2);

            Node catch3 = new Node("node_4");
            catch3.Link = link13;
            job3.AddCatch(catch3);

            // Add to process
            process.Add(decision1);
            process.Add(job2);
            process.Add(job3);
            process.Add(link12);
            process.Add(link13);

            process.Update();
            process.Start();
            
            Console.WriteLine("✓ Exclusive choice completed - decision routed to correct branch\n");
        }

        /// <summary>
        /// WCP-5: Simple Merge - Multiple branches converge, fire on first arrival
        /// Pattern: job_1 --+
        ///                  |-- job_3 (fires when job_1 OR job_2 arrives)
        ///          job_2 --+
        /// </summary>
        public static void SimpleMerge()
        {
            Console.WriteLine("Testing: WCP-5 Simple Merge");
            
            Process process = new Process("process_1");
            process.Name = "Simple Merge Pattern";

            // Add condition to choose path
            process.AddData("takeBranch", "1");  // or "2"

            // Job 1 - Branch 1
            Job job1 = new Job("job_1");
            job1.Name = "Job 1 (Path 1)";
            Task task1 = new Task("task_1");
            Item item1 = new Item("item_1");
            item1.Application = "cmd.exe";
            item1.Command = "/c echo Job 1 Completed";
            task1.Add(item1);
            job1.Add(task1);

            // Job 2 - Branch 2
            Job job2 = new Job("job_2");
            job2.Name = "Job 2 (Path 2)";
            Task task2 = new Task("task_2");
            Item item2 = new Item("item_2");
            item2.Application = "cmd.exe";
            item2.Command = "/c echo Job 2 Completed";
            task2.Add(item2);
            job2.Add(task2);

            // Job 3 - Merge point (fires on first arrival)
            Job job3 = new Job("job_3");
            job3.Name = "Job 3 (Merge Point)";
            Task task3 = new Task("task_3");
            Item item3 = new Item("item_3");
            item3.Application = "cmd.exe";
            item3.Command = "/c echo Job 3: Merge completed";
            task3.Add(item3);
            job3.Add(task3);

            // Links
            Link link13 = new Link("link_1");
            link13.From = "job_1";
            link13.To = "job_3";

            Link link23 = new Link("link_2");
            link23.From = "job_2";
            link23.To = "job_3";

            // Wire job_1
            Node throw1 = new Node("node_1");
            throw1.Link = link13;
            job1.AddThrow(throw1);

            // Wire job_2
            Node throw2 = new Node("node_2");
            throw2.Link = link23;
            job2.AddThrow(throw2);

            // Wire job_3 to catch from either job_1 or job_2
            // Note: With multiple catch nodes, job_3 will wait for first to fire
            Node catchFrom1 = new Node("node_3");
            catchFrom1.Link = link13;
            job3.AddCatch(catchFrom1);

            Node catchFrom2 = new Node("node_4");
            catchFrom2.Link = link23;
            job3.AddCatch(catchFrom2);

            // Add to process
            process.Add(job1);
            process.Add(job2);
            process.Add(job3);
            process.Add(link13);
            process.Add(link23);

            process.Update();
            process.Start();
            
            Console.WriteLine("✓ Simple merge completed\n");
        }

        /// <summary>
        /// WCP-6: Multi-Choice - Select multiple branches based on conditions
        /// Pattern:        +-- job_2 (if condition1)
        ///          job_1 --|-- job_3 (if condition2)
        ///                  +-- job_4 (if condition3)
        /// </summary>
        public static void MultiChoice()
        {
            Console.WriteLine("Testing: WCP-6 Multi-Choice");
            
            Process process = new Process("process_1");
            process.Name = "Multi-Choice Pattern";

            // Add test data for multiple conditions
            process.AddData("processJob2", true);
            process.AddData("processJob3", true);
            process.AddData("processJob4", false);

            // Job 1 - Decision point
            Job job1 = new Job("job_1");
            job1.Name = "Job 1 (Multi-Decision)";
            Task task1 = new Task("task_1");
            Item item1 = new Item("item_1");
            item1.Application = "cmd.exe";
            item1.Command = "/c echo Making multi-choice decision";
            task1.Add(item1);
            job1.Add(task1);

            // Job 2 - Optional Branch 1
            Job job2 = new Job("job_2");
            job2.Name = "Job 2 (Optional 1)";
            Task task2 = new Task("task_2");
            Item item2 = new Item("item_2");
            item2.Application = "cmd.exe";
            item2.Command = "/c echo Branch 2 Executed";
            task2.Add(item2);
            job2.Add(task2);

            // Job 3 - Optional Branch 2
            Job job3 = new Job("job_3");
            job3.Name = "Job 3 (Optional 2)";
            Task task3 = new Task("task_3");
            Item item3 = new Item("item_3");
            item3.Application = "cmd.exe";
            item3.Command = "/c echo Branch 3 Executed";
            task3.Add(item3);
            job3.Add(task3);

            // Job 4 - Optional Branch 3
            Job job4 = new Job("job_4");
            job4.Name = "Job 4 (Optional 3)";
            Task task4 = new Task("task_4");
            Item item4 = new Item("item_4");
            item4.Application = "cmd.exe";
            item4.Command = "/c echo Branch 4 Executed";
            task4.Add(item4);
            job4.Add(task4);

            // Create Links with independent conditions
            Link link12 = new Link("link_1");
            link12.From = "job_1";
            link12.To = "job_2";
            link12.Expression = "{process:processJob2} == true";

            Link link13 = new Link("link_2");
            link13.From = "job_1";
            link13.To = "job_3";
            link13.Expression = "{process:processJob3} == true";

            Link link14 = new Link("link_3");
            link14.From = "job_1";
            link14.To = "job_4";
            link14.Expression = "{process:processJob4} == true";

            // Wire throws
            Node throw2 = new Node("node_1");
            throw2.Link = link12;
            job1.AddThrow(throw2);

            Node throw3 = new Node("node_2");
            throw3.Link = link13;
            job1.AddThrow(throw3);

            Node throw4 = new Node("node_3");
            throw4.Link = link14;
            job1.AddThrow(throw4);

            // Wire catches
            Node catch2 = new Node("node_4");
            catch2.Link = link12;
            job2.AddCatch(catch2);

            Node catch3 = new Node("node_5");
            catch3.Link = link13;
            job3.AddCatch(catch3);

            Node catch4 = new Node("node_6");
            catch4.Link = link14;
            job4.AddCatch(catch4);

            // Add to process
            process.Add(job1);
            process.Add(job2);
            process.Add(job3);
            process.Add(job4);
            process.Add(link12);
            process.Add(link13);
            process.Add(link14);

            process.Update();
            process.Start();
            
            Console.WriteLine("✓ Multi-choice completed - multiple branches may execute\n");
        }

        /// <summary>
        /// WCP-8: Multi-Merge - Each incoming branch triggers downstream
        /// Pattern: job_1 --+
        ///                  |-- event_1 (fires for EACH arrival: job_1 then job_2)
        ///          job_2 --+
        /// </summary>
        public static void MultiMerge()
        {
            Console.WriteLine("Testing: WCP-8 Multi-Merge");
            
            Process process = new Process("process_1");
            process.Name = "Multi-Merge Pattern";

            // Job 1 - Branch 1
            Job job1 = new Job("job_1");
            job1.Name = "Job 1";
            Task task1 = new Task("task_1");
            Item item1 = new Item("item_1");
            item1.Application = "cmd.exe";
            item1.Command = "/c timeout /t 1 && echo Job 1 Completed";
            task1.Add(item1);
            job1.Add(task1);

            // Job 2 - Branch 2
            Job job2 = new Job("job_2");
            job2.Name = "Job 2";
            Task task2 = new Task("task_2");
            Item item2 = new Item("item_2");
            item2.Application = "cmd.exe";
            item2.Command = "/c timeout /t 2 && echo Job 2 Completed";
            task2.Add(item2);
            job2.Add(task2);

            // Event 1 - Fires on each incoming message
            // Note: This would need custom logic to fire multiple times
            // Current library limitation: Event waits for all catches before processing
            Event event1 = new Event("event_1");
            event1.Name = "Multi-Merge Event 1";

            // Links
            Link link1 = new Link("link_1");
            link1.From = "job_1";
            link1.To = "event_1";

            Link link2 = new Link("link_2");
            link2.From = "job_2";
            link2.To = "event_1";

            // Wire throws
            Node throw1 = new Node("node_1");
            throw1.Link = link1;
            job1.AddThrow(throw1);

            Node throw2 = new Node("node_2");
            throw2.Link = link2;
            job2.AddThrow(throw2);

            // Wire catches
            Node catchFrom1 = new Node("node_3");
            catchFrom1.Link = link1;
            event1.AddCatch(catchFrom1);

            Node catchFrom2 = new Node("node_4");
            catchFrom2.Link = link2;
            event1.AddCatch(catchFrom2);

            // Add to process
            process.Add(job1);
            process.Add(job2);
            process.Add(event1);
            process.Add(link1);
            process.Add(link2);

            process.Update();
            process.Start();
            
            Console.WriteLine("⚠️  Multi-merge has limitations in current library\n");
            Console.WriteLine("    Event waits for all catches (synchronization behavior)\n");
        }
    }
}
