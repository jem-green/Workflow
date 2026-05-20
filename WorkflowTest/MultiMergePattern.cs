using System;
using WorkflowLibrary;
using Process = WorkflowLibrary.Process;

namespace WorkflowTest
{
    /// <summary>
    /// WCP-8: Multi-Merge - Each incoming branch triggers downstream
    /// Pattern: job_1 --+
    ///                  |-- event_1 (fires for EACH arrival: job_1 then job_2)
    ///          job_2 --+
    /// </summary>
    public static class MultiMergePattern
    {
        public static void Run()
        {
            Console.WriteLine("Testing: WCP-8 Multi-Merge");
            Process process = new Process("process_1");
            process.Name = "Multi-Merge Pattern";
            Job job1 = new Job("job_1");
            job1.Name = "Job 1";
            Task task1 = new Task("task_1");
            Item item1 = new Item("item_1");
            item1.Application = "cmd.exe";
            item1.Command = "/c timeout /t 1 && echo Job 1 Completed";
            task1.Add(item1);
            job1.Add(task1);
            Job job2 = new Job("job_2");
            job2.Name = "Job 2";
            Task task2 = new Task("task_2");
            Item item2 = new Item("item_2");
            item2.Application = "cmd.exe";
            item2.Command = "/c timeout /t 2 && echo Job 2 Completed";
            task2.Add(item2);
            job2.Add(task2);
            Event event1 = new Event("event_1");
            event1.Name = "Multi-Merge Event 1";
            Link link1 = new Link("link_1");
            link1.From = "job_1";
            link1.To = "event_1";
            Link link2 = new Link("link_2");
            link2.From = "job_2";
            link2.To = "event_1";
            Node throw1 = new Node("node_1");
            throw1.Link = link1;
            job1.AddThrow(throw1);
            Node throw2 = new Node("node_2");
            throw2.Link = link2;
            job2.AddThrow(throw2);
            Node catchFrom1 = new Node("node_3");
            catchFrom1.Link = link1;
            event1.AddCatch(catchFrom1);
            Node catchFrom2 = new Node("node_4");
            catchFrom2.Link = link2;
            event1.AddCatch(catchFrom2);
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
