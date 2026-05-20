using System;
using WorkflowLibrary;
using Process = WorkflowLibrary.Process;

namespace WorkflowTest
{
    /// <summary>
    /// WCP-1: Sequence - Activity B starts after Activity A completes
    /// Pattern: job_1 --> job_2
    /// </summary>
    public static class SequencePattern
    {
        public static void Run()
        {
            Console.WriteLine("Testing: WCP-1 Sequence");
            Process process = new Process("process_1");
            process.Name = "Sequence Pattern";
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
            Link link = new Link("link_1");
            link.From = "job_1";
            link.To = "job_2";
            Node throwNode = new Node("node_1");
            throwNode.Link = link;
            job1.AddThrow(throwNode);
            Node catchNode = new Node("node_2");
            catchNode.Link = link;
            job2.AddCatch(catchNode);
            process.Add(job1);
            process.Add(job2);
            process.Add(link);
            process.Update();
            process.Start();
            Console.WriteLine("✓ Sequence pattern completed\n");
        }
    }
}
