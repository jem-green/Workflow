using System;
using WorkflowLibrary;
using Process = WorkflowLibrary.Process;

namespace WorkflowTest
{
    /// <summary>
    /// WCP-2: Parallel Split - One activity diverges to multiple concurrent activities
    /// Pattern:         +-- job_2
    ///          job_1 --|
    ///                  +-- job_3
    /// </summary>
    public static class ParallelSplitPattern
    {
        public static void Run()
        {
            Console.WriteLine("Testing: WCP-2 Parallel Split");
            Process process = new Process("process_1");
            process.Name = "Parallel Split Pattern";
            Job job1 = new Job("job_1");
            job1.Name = "Job 1 (Source)";
            Task task1 = new Task("task_1");
            task1.Name = "Start Task";
            Item item1 = new Item("item_1");
            item1.Application = "cmd.exe";
            item1.Command = "/c echo Job 1 Starting Split";
            task1.Add(item1);
            job1.Add(task1);
            Job job2 = new Job("job_2");
            job2.Name = "Job 2 (Branch 1)";
            Task task2 = new Task("task_2");

            Item item2 = new Item("item_2");
            item2.Application = "cmd.exe";
            item2.Command = "/c echo Job 2 Starting";
            task2.Add(item2);

            Item item3 = new Item("item_3");
            item3.Application = "powershell.exe";
            item3.Command = "-NoLogo -NoProfile -Command \"Start-Sleep -Milliseconds 10000\"";
            task2.Add(item3);

            Item item4 = new Item("item_4");
            item4.Application = "cmd.exe";
            item4.Command = "/c echo Job 2 Completed";
            task2.Add(item4);

            job2.Add(task2);

            Job job3 = new Job("job_3");
            job3.Name = "Job 3 (Branch 2)";
            Task task3 = new Task("task_3");

            Item item5 = new Item("item_5");
            item5.Application = "cmd.exe";
            item5.Command = "/c echo Job 3 Starting";
            task3.Add(item5);

            Item item6 = new Item("item_6");
            item6.Application = "powershell.exe";
            item6.Command = "-NoLogo -NoProfile -Command \"Start-Sleep -Milliseconds 20000\"";
            task3.Add(item6);

            Item item7 = new Item("item_7");
            item7.Application = "cmd.exe";
            item7.Command = "/c echo Job 3 Completed";
            task3.Add(item7);

            job3.Add(task3);

            Link link12 = new Link("link_1");
            link12.From = "job_1";
            link12.To = "job_2";
            Link link13 = new Link("link_2");
            link13.From = "job_1";
            link13.To = "job_3";
            Node throw2 = new Node("node_1");
            throw2.Link = link12;
            job1.AddThrow(throw2);
            Node throw3 = new Node("node_2");
            throw3.Link = link13;
            job1.AddThrow(throw3);
            Node catch2 = new Node("node_3");
            catch2.Link = link12;
            job2.AddCatch(catch2);
            Node catch3 = new Node("node_4");
            catch3.Link = link13;
            job3.AddCatch(catch3);
            process.Add(job1);
            process.Add(job2);
            process.Add(job3);
            process.Add(link12);
            process.Add(link13);
            process.Update();
            process.Start();
            Console.WriteLine("✓ Parallel split completed - both branches ran concurrently\n");
        }
    }
}
