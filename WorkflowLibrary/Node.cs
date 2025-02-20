using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using TracerLibrary;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace WorkflowLibrary
{
    /// <summary>
    /// This is the link endpoint
    /// </summary>
    public class Node
    {
        #region Fields

        private static int _nodeId;
        private Link link;
        private string _id;
        bool isLinked;

        #endregion
        #region Constructors
        public Node()
        {
            _nodeId = _nodeId + 1;
            _id = "node_" + _nodeId.ToString();
        }

        public Node(string Id)
        {
            _id = Id;
            isLinked = false;
            if (Id.StartsWith("node_"))
            {
                if (_nodeId < Convert.ToInt16(Id.Substring(5)))
                {
                    _nodeId = Convert.ToInt16(Id.Substring(5));

                }
            }
        }

        #endregion
        #region Properties

        static Node()
        {
            _nodeId = -1;
        }

        public Link Link
        {
            get
            {
                return (link);
            }
            set
            {
                link = value;
                isLinked = true;
            }
        }

        public string Id
        {
            get
            {
                return (_id);
            }
            set
            {
                _id = value;
            }
        }

        public bool IsLinked
        {
            get
            {
                return (isLinked);
            }
        }

        #endregion
        #region Methods


        #endregion
    }
}
