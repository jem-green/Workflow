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

        Queue<Token> _queue = new Queue<Token>();

        private static int _linkId;
        private string _from = "";
        private string _to = "";
        private string _expression = "";

        #endregion
        #region Constructors

        public Link()
        {
            _linkId = _linkId + 1;
            _localData = new ArrayList();
            _id = "link_" + _linkId.ToString();
        }        
        
        public Link(string Id)
        {
            _localData = new ArrayList();
            _id = Id;
            if (Id.StartsWith("link_"))
            {
                if (_linkId < Convert.ToInt16(Id.Substring(5)))
                {
                    _linkId = Convert.ToInt16(Id.Substring(5));
                }
            }
        }

        #endregion
        #region Properties

        static Link()
        {
            _linkId = -1;
        }

        public string From
        {
            get
            {
                return (_from);
            }
            set
            {
                _from = value;
            }
        }

        public string To
        {
            get
            {
                return (_to);
            }
            set
            {
                _to = value;
            }
        }

        public string Expression
        {
            get
            {
                return (_expression);
            }
            set
            {
                _expression = value;
            }
        }

        #endregion Properites
        #region Methods

        /// <summary>
        ///  
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public bool Evaluate(string expression)
        {
            bool evaluate = true;
            if (_expression.Length > 0)
            {
                try
                {
                    evaluate = Evaluator.EvaluateToBool(expression);
                }
                catch (Exception ex)
                {
                    // May not evaluate to true/false
                    TraceInternal.TraceVerbose(ex.ToString());
                }
            }
            return (evaluate);
        }

        /// <summary>
        /// Get the count of items on the queue
        /// </summary>
        /// <returns>Count of items on the queue</returns>
        public int Count()
        {
            return (_queue.Count);
        }

        /// <summary>
        /// Take a peek of messages on the queue
        /// </summary>
        /// <param name="maxWait"></param>
        /// <returns></returns>

        public Token PeekItem(int maxWait)
        {
            if (_queue.Count == 0)
            {
                if (maxWait == 0)
                {
                    return default(Token);
                }
                Monitor.Wait(_queue, maxWait);
                if (_queue.Count == 0)
                {
                    return default(Token);
                }
            }
            return _queue.Peek();
        }

        /// <summary>
        /// Post a message to the queue.
        /// </summary>
        public bool PutItem(Token item)
        {
           lock (_queue)
           {
               _queue.Enqueue(item);
               if (_queue.Count == 1)
               {
                   Monitor.Pulse(_queue);
               }
           }
           return (true);
        }

        /// <summary>
        /// Immediate message retrieve from the queue
        /// </summary>
        /// <returns>The next item in the queue, or default(T) if queue is empty</returns>
        public Token GetItem()
        {
            return (GetItem(0));
        }

        /// <summary>
        /// Retrieve a message from the queue.
        /// </summary>
        /// <param name="maxWait">Number of milliseconds to block if nothing is available. -1 means "block indefinitely"</param>
        /// <returns>The next item in the queue, or default(T) if queue is empty</returns>
        public Token GetItem(int maxWait)
        {
           lock (_queue)
           {
               if (_queue.Count == 0)
               {
                   if (maxWait == 0)
                   {
                       return default(Token);
                   }
                   Monitor.Wait(_queue, maxWait);
                   if (_queue.Count == 0)
                   {
                       return default(Token);
                   }
               }
               return _queue.Dequeue();
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
