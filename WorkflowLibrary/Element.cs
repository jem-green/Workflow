using System;
using System.Collections;
using System.Diagnostics;
using TracerLibrary;

namespace WorkflowLibrary
{
    public class Element : State
    {
        #region Fields

        protected string _id = "";
        protected string _sessionId;
        protected string _name = "";
        protected string _description = "";

        protected ArrayList _localData;
        protected ArrayList tempData;
        protected ArrayList _data;
        protected ArrayList _hierarchy;
        protected int _dataId;

        protected bool cancel = false;
        protected bool _enabled = true;
        protected bool terminate = false;

        #endregion
        #region Constructors
        public Element()
        {
            _dataId = _dataId + 1;
            _data = new ArrayList();
            _localData = new ArrayList();
            tempData = new ArrayList();
            _hierarchy = new ArrayList(4);
            terminate = false;
            cancel = false;
            _enabled = false;
        }

        public Element(string id)
        {
            _dataId = _dataId + 1;
            _data = new ArrayList();
            _localData = new ArrayList();
            tempData = new ArrayList();
            _hierarchy = new ArrayList(4);
            terminate = false;
            cancel = false;
            _enabled = false;
        }
        #endregion
        #region Properties

        public string ID
        {
            get
            {
                return (_id);
            }
        }

        public string Name
        {
            get
            {
                return (_name);
            }
            set
            {
                _name = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return (_enabled);
            }
            set
            {
                _enabled = value;
            }
        }

        public string Description
        {
            get
            {
                return (_description);
            }
            set
            {
                _description = value;
            }
        }

        public ArrayList Hierarchy
        {
            get
            {
                return (_hierarchy);
            }
            set
            {
                _hierarchy = value;
            }
        }

        public StateType State
        {
            get
            {
                return (_state);
            }
        }

        public ArrayList Data
        {
            get
            {
                return (_data);
            }
        }

        public ArrayList LocalData
        {
            get
            { 
                return (_localData);
            }
            set
            {
                _localData = value;
            }
        }

        #endregion Properties
        #region Methods

        public virtual bool AddData(string key, object value)
        {
            bool add = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Add local data: key=" + key + " value=" + value);
                DictionaryEntry item = new DictionaryEntry(key, value);
                _localData.Add(item);
                add = true;
            }
            catch { }
            return (add);
        }

        public virtual bool RemoveData(string key)
        {

            bool remove = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Remove local data: key=" + key);
                _localData.Remove(key);
                remove = true;
            }
            catch { }
            return (remove);
        }
        #endregion Methods
    }
}
