using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TracerLibrary;
using System.Threading;
using System.Diagnostics;

namespace WorkflowLibrary
{
    /// <summary>
    /// This is the queue between endpoints
    /// </summary>
    public class Link : Orchestration, IActivity, ILink, ICloneable
    {
        #region Fields

        /// <summary>
        /// link class implements a queue designed to be used by multiple threads to exchange messages.
        /// Any thread can post an item to the queue with PutItem(), and any thread can retrieve items with GetItem().Node
        /// GetItem has a maxWait parameter which specifies the amount of time the receiving thread will block
        /// waiting for an item.  If this value is -1, blocking is indefinite.
        /// </summary>

        System.Collections.Generic.Queue<bool> queue = new System.Collections.Generic.Queue<bool>();

        private static int linkId;
        private string from = "";
        private string to = "";
        private string expression = "";

        #endregion
        #region Constructors

        public Link()
        {
            linkId = linkId + 1;
            _localData = new ArrayList();
            _id = "link_" + linkId.ToString();
        }        
        
        public Link(string Id)
        {
            _localData = new ArrayList();
            _id = Id;
            if (Id.StartsWith("link_"))
            {
                if (linkId < Convert.ToInt16(Id.Substring(5)))
                {
                    linkId = Convert.ToInt16(Id.Substring(5));
                }
            }
        }

        #endregion
        #region Properties

        static Link()
        {
            linkId = -1;
        }

        public string From
        {
            get
            {
                return (from);
            }
            set
            {
                from = value;
            }
        }

        public string To
        {
            get
            {
                return (to);
            }
            set
            {
                to = value;
            }
        }

        public string Expression
        {
            get
            {
                return (expression);
            }
            set
            {
                expression = value;
            }
        }

        #endregion Properites
        #region Methods

        public bool Evaluate(string excpression)
        {
            bool evaluate = true;
            if (expression.Length > 0)
            {
                //evaluate = Evaluator.EvaluateToBool(expression);
            }
            return (evaluate);
        }

        /// <summary>
        /// Get the count of items on the queue
        /// </summary>
        /// <returns>Count of items on the queue</returns>
        public int Count()
        {
            return (queue.Count);
        }

        /// <summary>
        /// Take a peek of messages on the queue
        /// </summary>
        /// <param name="maxWait"></param>
        /// <returns></returns>

        public bool PeekItem(int maxWait)
        {
            if (queue.Count == 0)
            {
                if (maxWait == 0)
                {
                    return default(bool);
                }
                Monitor.Wait(queue, maxWait);
                if (queue.Count == 0)
                {
                    return default(bool);
                }
            }
            return queue.Peek();
        }

        /// <summary>
        /// Post a message to the queue.
        /// </summary>
        public bool PutItem(bool item)
        {
           lock (queue)
           {
               queue.Enqueue(item);
               if (queue.Count == 1)
               {
                   Monitor.Pulse(queue);
               }
           }
           return (true);
        }

        /// <summary>
        /// Immediate message retrieve from the queue
        /// </summary>
        /// <returns>The next item in the queue, or default(T) if queue is empty</returns>
        public bool GetItem()
        {
            return (GetItem(0));
        }

        /// <summary>
        /// Retrieve a message from the queue.
        /// </summary>
        /// <param name="maxWait">Number of milliseconds to block if nothing is available. -1 means "block indefinitely"</param>
        /// <returns>The next item in the queue, or default(T) if queue is empty</returns>
        public bool GetItem(int maxWait)
        {
           lock (queue)
           {
               if (queue.Count == 0)
               {
                   if (maxWait == 0)
                   {
                       return default(bool);
                   }
                   Monitor.Wait(queue, maxWait);
                   if (queue.Count == 0)
                   {
                       return default(bool);
                   }
               }
               return queue.Dequeue();
           }
       }

        public override void Update()
        {
            ArrayList data = new ArrayList();
            ArrayList parentHierarchy = new ArrayList();
            Update(ref data, parentHierarchy);
        }

        public override void Update(ref ArrayList data, ArrayList parentHierarchy)
        {
            Debug.WriteLine("[" + _sessionId + "] In Update() " + _id + "(" + _name + ")");
            
            Debug.WriteLine("[" + _sessionId + "] Out Update() " + _id + "(" + _name + ")");
        }

        #endregion Methods
    }
}
