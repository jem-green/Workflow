using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using TracerLibrary;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Diagnostics;

namespace WorkflowLibrary
{
    public class Orchestration : Actions, ICloneable
    {
        #region Fields

        protected List<Node> @throw;
        protected List<Node> @catch;

        #endregion
        #region Constructors
        public Orchestration()
        {
            @throw = new List<Node>();
            @catch = new List<Node>();
        }
        public Orchestration(string id)
        {
            @throw = new List<Node>();
            @catch = new List<Node>();
        }
        #endregion
        #region Properties

        public List<Node> Throw
        {
            get
            {
                return (@throw);
            }
        }

        public List<Node> Catch
        {
            get
            {
                return (@catch);
            }
        }

        #endregion Properites
        #region Methods

        public bool AddThrow(Node node)
        {
            bool add = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Add throw: value=" + node.Id);
                @throw.Add(node);
                add = true;
            }
            catch { }
            return (add);
        }

        public bool AddCatch(Node node)
        {
            bool add = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Add catch: value=" + node.Id);
                @catch.Add(node);
                add = true;
            }
            catch { }
            return (add);
        }

        #endregion Methods
    }
}
