using System;
using WorkflowLibrary;
using Process = WorkflowLibrary.Process;

namespace WorkflowTest
{
    /// <summary>
    /// WCP-5: Simple Merge - Multiple branches converge, fire on first arrival
    /// Pattern: job_1 --+
    ///                  |-- job_3 (fires when job_1 OR job_2 arrives)
    ///          job_2 --+
    /// </summary>
    public static class SimpleMergePattern
    {
        public static void Run()
        {
            Console.WriteLine("Testing: WCP-5 Simple Merge");
            Process process = new Process("process_1");
            process.Name = "Simple Merge Pattern";
            process.AddLocalData("takeBranch", "1");
            Job job1 = new Job("job_1");
            job1.Name = "Job 1 (Path 1)";
            Task task1 = new Task("task_1");

            Item item1 = new Item("item_1");
            item1.Application = "powershell.exe";
            item1.Command = "-NoLogo -NoProfile -Command \"Start-Sleep -Milliseconds 10000\"";
            task1.Add(item1);

            Item item2 = new Item("item_2");
            item2.Application = "cmd.exe";
            item2.Command = "/c echo Job 1 Completed";
            task1.Add(item2);
            job1.Add(task1);

            Job job2 = new Job("job_2");
            job2.Name = "Job 2 (Path 2)";
            Task task2 = new Task("task_2");

            Item item3 = new Item("item_3");
            item3.Application = "powershell.exe";
            item3.Command = "-NoLogo -NoProfile -Command \"Start-Sleep -Milliseconds 20000\"";
            task2.Add(item3);

            Item item4 = new Item("item_4");
            item4.Application = "cmd.exe";
            item4.Command = "/c echo Job 2 Completed";
            task2.Add(item4);
            job2.Add(task2);

            Job job3 = new Job("job_3");
            job3.Name = "Job 3 (Merge Point)";
            Task task3 = new Task("task_3");

            Item item5 = new Item("item_5");
            item5.Application = "cmd.exe";
            item5.Command = "/c echo Job 3: Merge completed";
            task3.Add(item5);
            
            job3.Add(task3);
            Link link13 = new Link("link_1");
            link13.From = "job_1";
            link13.To = "job_3";
            Link link23 = new Link("link_2");
            link23.From = "job_2";
            link23.To = "job_3";
            Node throw1 = new Node("node_1");
            throw1.Link = link13;
            job1.AddThrow(throw1);
            Node throw2 = new Node("node_2");
            throw2.Link = link23;
            job2.AddThrow(throw2);
            Node catchFrom1 = new Node("node_3");
            catchFrom1.Link = link13;
            job3.AddCatch(catchFrom1);
            Node catchFrom2 = new Node("node_4");
            catchFrom2.Link = link23;
            job3.AddCatch(catchFrom2);
            process.Add(job1);
            process.Add(job2);
            process.Add(job3);
            process.Add(link13);
            process.Add(link23);
            process.Update();
            process.Start();
            Console.WriteLine("✓ Simple merge completed\n");
        }
    }
}
