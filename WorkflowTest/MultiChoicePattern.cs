using System;
using WorkflowLibrary;
using Process = WorkflowLibrary.Process;

namespace WorkflowTest
{
    /// <summary>
    /// WCP-6: Multi-Choice - Select multiple branches based on conditions
    /// Pattern:         +-- job_2 (if condition1)
    ///          job_1 --|-- job_3 (if condition2)
    ///                  +-- job_4 (if condition3)
    /// </summary>
    public static class MultiChoicePattern
    {
        public static void Run()
        {
            Console.WriteLine("Testing: WCP-6 Multi-Choice");
            Process process = new Process("process_1");
            process.Name = "Multi-Choice Pattern";
            process.AddLocalData("processJob2", true);
            process.AddLocalData("processJob3", true);
            process.AddLocalData("processJob4", false);
            Job job1 = new Job("job_1");
            job1.Name = "Job 1 (Multi-Decision)";
            Task task1 = new Task("task_1");
            Item item1 = new Item("item_1");
            item1.Application = "cmd.exe";
            item1.Command = "/c echo Making multi-choice decision";
            task1.Add(item1);
            job1.Add(task1);
            Job job2 = new Job("job_2");
            job2.Name = "Job 2 (Optional 1)";
            Task task2 = new Task("task_2");
            Item item2 = new Item("item_2");
            item2.Application = "cmd.exe";
            item2.Command = "/c echo Branch 2 Executed";
            task2.Add(item2);
            job2.Add(task2);
            Job job3 = new Job("job_3");
            job3.Name = "Job 3 (Optional 2)";
            Task task3 = new Task("task_3");
            Item item3 = new Item("item_3");
            item3.Application = "cmd.exe";
            item3.Command = "/c echo Branch 3 Executed";
            task3.Add(item3);
            job3.Add(task3);
            Job job4 = new Job("job_4");
            job4.Name = "Job 4 (Optional 3)";
            Task task4 = new Task("task_4");
            Item item4 = new Item("item_4");
            item4.Application = "cmd.exe";
            item4.Command = "/c echo Branch 4 Executed";
            task4.Add(item4);
            job4.Add(task4);
            Link link12 = new Link("link_1");
            link12.From = "job_1";
            link12.To = "job_2";
            link12.Expression = "[processJob2] = true";
            Link link13 = new Link("link_2");
            link13.From = "job_1";
            link13.To = "job_3";
            link13.Expression = "[processJob3] = true";
            Link link14 = new Link("link_3");
            link14.From = "job_1";
            link14.To = "job_4";
            link14.Expression = "[processJob4] = true";
            Node throw2 = new Node("node_1");
            throw2.Link = link12;
            job1.AddThrow(throw2);
            Node throw3 = new Node("node_2");
            throw3.Link = link13;
            job1.AddThrow(throw3);
            Node throw4 = new Node("node_3");
            throw4.Link = link14;
            job1.AddThrow(throw4);
            Node catch2 = new Node("node_4");
            catch2.Link = link12;
            job2.AddCatch(catch2);
            Node catch3 = new Node("node_5");
            catch3.Link = link13;
            job3.AddCatch(catch3);
            Node catch4 = new Node("node_6");
            catch4.Link = link14;
            job4.AddCatch(catch4);
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
    }
}
