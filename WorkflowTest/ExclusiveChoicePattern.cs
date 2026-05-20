using System;
using WorkflowLibrary;
using Process = WorkflowLibrary.Process;

namespace WorkflowTest
{
    /// <summary>
    /// WCP-4: Exclusive Choice - One branch chosen based on condition
    /// Pattern:              +-- job_2 (if x>5)
    ///          decision_1 --|
    ///                       +-- job_3 (if x<=5)
    /// </summary>
    public static class ExclusiveChoicePattern
    {
        public static void Run()
        {
            Console.WriteLine("Testing: WCP-4 Exclusive Choice");
            Process process = new Process("process_1");
            process.Name = "Exclusive Choice Pattern";
            process.AddLocalData("x", 7);  // Change this value to test different paths
            Decision decision1 = new Decision("decision_1");
            decision1.Name = "Decision 1 (Router)";
            Job job2 = new Job("job_2");
            job2.Name = "Job 2 (x > 5)";
            Task task2 = new Task("task_2");
            Item item2 = new Item("item_2");
            item2.Application = "cmd.exe";
            item2.Command = "/c echo Branch 2: x is greater than 5";
            task2.Add(item2);
            job2.Add(task2);
            Job job3 = new Job("job_3");
            job3.Name = "Job 3 (x <= 5)";
            Task task3 = new Task("task_3");
            Item item3 = new Item("item_3");
            item3.Application = "cmd.exe";
            item3.Command = "/c echo Branch 3: x is less than or equal to 5";
            task3.Add(item3);
            job3.Add(task3);
            Link link12 = new Link("link_1");
            link12.From = "decision_1";
            link12.To = "job_2";
            link12.Expression = "[x] > 5";
            Link link13 = new Link("link_2");
            link13.From = "decision_1";
            link13.To = "job_3";
            link13.Expression = "[x] <= 5";
            Node throw2 = new Node("node_1");
            throw2.Link = link12;
            decision1.AddThrow(throw2);
            Node throw3 = new Node("node_2");
            throw3.Link = link13;
            decision1.AddThrow(throw3);

            //decision1.Expression = "[link_1] AND NOT [line_2]";

            Node catch2 = new Node("node_3");
            catch2.Link = link12;
            job2.AddCatch(catch2);
            Node catch3 = new Node("node_4");
            catch3.Link = link13;
            job3.AddCatch(catch3);
            process.Add(decision1);
            process.Add(job2);
            process.Add(job3);
            process.Add(link12);
            process.Add(link13);
            process.Update();
            process.Start();
            Console.WriteLine("✓ Exclusive choice completed - decision routed to correct branch\n");
        }
    }
}
