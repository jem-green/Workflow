using System;
using System.Diagnostics;
using WorkflowLibrary;

namespace WorkflowTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Sequence();
        }

        public static void Sequence()
        {
            /*
             * Pattern WCP-1 (Sequence)
             * Description An activity in a work°ow process is enabled after the completion of a
             * preceding activity in the same process.
             * Synonyms Sequential routing, serial routing.
             */

            Job j = new Job();
            Job k = new Job();
            Link l = new Link();

            Node n = new Node();
            Node m = new Node();

            n.Link = l;
            m.Link = l;
            l.AddCatch(m);
            l.AddThrow(n);
            j.AddThrow(m);
            k.AddCatch(n);
            j.Start();
            k.Start();

        }

        public static void ParallelSplit()
        {
            /*
             * Pattern WCP-2 (Parallel Split)
             * Description The divergence of a branch into two or more parallel branches
             * each of which execute concurrently.
             * Synonyms AND-split, parallel routing, parallel split, fork.
             */

            Job j = new Job();
            Job k = new Job();
            Link l = new Link();

            Node n = new Node();
            Node m = new Node();
            n.Link = l;
            m.Link = l;
            l.AddCatch(m);
            l.AddThrow(n);
            j.AddThrow(m);
            k.AddCatch(n);
            j.Start();
            k.Start();

        }

        public static void Synchronization()
        {
            /*
             * Pattern WCP-3 (Synchronization)
             * Description The convergence of two or more branches into a single subsequent
             * branch such that the thread of control is passed to the subsequent branch when
             * all input branches have been enabled.
             * Synonyms AND-join, rendezvous, synchronizer.
             */
        }

        public static void ExclusiveChoice()
        {
            /*
             * Pattern WCP-4 (Exclusive Choice)
             * Description The divergence of a branch into two or more branches. When the
             * incoming branch is enabled, the thread of control is immediately passed to precisely
             * one of the outgoing branches based on the outcome of a logical expression associated
             * with the branch.
             * Synonyms XOR-split, exclusive OR-split, conditional routing, switch, decision, case statement.
             */
        }

        public static void SimpleMerge()
        {
            /*
             * Pattern WCP-5 (Simple Merge)
             * Description The convergence of two or more branches into a single subsequent
             * branch. Each enablement of an incoming branch results in the thread of control
             * being passed to the subsequent branch.
             * Synonyms XOR-join, exclusive OR-join, asynchronous join, merge.
             */
        }

        public void a()
        {
            /*
             * 
             */
        }

    }
}
