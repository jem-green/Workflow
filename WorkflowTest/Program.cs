using Microsoft.VisualBasic;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using WorkflowLibrary;

namespace WorkflowTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Read();
            //Sequence();
            //Synchronization();
        }

        public static void Read()
        {
            Collection<object> c = new Collection<object>();
            Serialise s = new Serialise("workflow.job",".");
            c = s.FromXML(State.StageType.Job);

        }



        public static void Sequence()
        {
            /*
             * Pattern WCP-1 (Sequence)
             * Description An activity in a work°ow process is enabled after the completion of a
             * preceding activity in the same process.
             * Synonyms Sequential routing, serial routing.
             */

            // j+---+k

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

            //    +j
            //   /
            //  +
            //   \
            //    +k

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

            // j+n
            //   \l
            //    +pq- 
            //   /m
            // k+o

            Job j = new Job();
            Job k = new Job();

            Link l = new Link();
            Link m = new Link();

            Node n = new Node();
            Node o = new Node();
            Node p = new Node();
            Node q = new Node();

            n.Link = l;
            p.Link = l;
            o.Link = m;
            q.Link = m;

            j.AddThrow(n);
            k.AddThrow(o);

            l.AddCatch(n);
            m.AddCatch(o);

            j.Start();
            k.Start();


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
             * branch. Each entablement of an incoming branch results in the thread of control
             * being passed to the subsequent branch.
             * Synonyms XOR-join, exclusive OR-join, asynchronous join, merge.
             */
        }

        public static void MultiChoice()
        {
            /*
             * Pattern WCP-6 (Multi-Choice)
             * Description The divergence of a branch into two or more branches. When the
             * incoming branch is enabled, the thread of control is passed to one or more of the
             * outgoing branches based on the outcome of distinct logical expressions associated
             * with each of the branches.
             * Synonyms Conditional routing, selection, OR-split, multiple choice.
             */
        }

        public static void StructuredSynchronizingMerge()
        {
            /*
             * Pattern WCP-7 (Structured Synchronizing Merge)
             * Description The convergence of two or more branches (which diverged earlier in the
             * process at a uniquely identifiable point) into a single subsequent branch. The thread
             * of control is passed to the subsequent branch when each active incoming branch has
             * been enabled.
             */
        }

        public static void MultiMerge()
        {
            /*
             * Pattern WCP-8 (Multi-Merge)
             * Description The convergence of two or more branches into a single subsequent
             * branch. Each entablement of an incoming branch results in the thread of control
             * being passed to the subsequent branch.
            */
        }

        public static void StructuredDiscriminator()
        {
            /*
             * Pattern WCP-9 (Structured Discriminator)
             * Description The convergence of two or more branches into a single subsequent
             * branch following a corresponding divergence earlier in the process model. The thread
             * of control is passed to the subsequent branch when the First incoming branch has been
             * enabled. Subsequent entablement of incoming branches do not result in the thread
             * of control being passed on. The discriminator construct resets when all incoming
             * branches have been enabled.
             */
        }


    }
}
