using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using TracerLibrary;
using System.Collections;
using System.Collections.Generic;

namespace WorkflowLibrary
{
    /// <summary>
    /// This is the link endpoint
    /// </summary>
    public class Node
    {
        #region Fields

        private Link link;
        private string id;
        bool isLinked;

        #endregion
        #region Constructors
        public Node()
        {
        }

        public Node(string id)
        {
            this.id = id;
            isLinked = false;
        }

        #endregion
        #region Properties

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
                return (id);
            }
            set
            {
                id = value;
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
