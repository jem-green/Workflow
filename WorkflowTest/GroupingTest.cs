using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowLibrary;

namespace WorkflowTest
{
    internal class GroupingTest
    {

        public enum StageType : int
        {
            Process = 0,
            Job = 1,
            Task = 2,
            Item = 3
        }

        public static void Run()
        {
            Console.WriteLine("Testing: Grouping Extraction and Replacement");

            List<Grouping> data = new List<Grouping>();
            List<int> hierarchy = new List<int>();

            // process data

            var processData = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Process", "Process_1")
            };
            Grouping processGrouping = new Grouping(processData);
            data.Add(processGrouping);


        
            string template = "[Process]";

            // We should have a List<List<DictionaryEntry>> for the process data, and the hierarchy
            // should have the reference to that data at the correct stage type index.


            //.Add(processData);
            int reference = data.Count - 1;
            hierarchy.Insert((int)StageType.Process, reference);
            string replacement = Replacer.ReplaceGrouping(template, data, hierarchy);

            Console.WriteLine($"Replacement Result: {replacement}");

        }
    }
}
    