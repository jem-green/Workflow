using System;
using System.Collections;
using System.Collections.Generic;
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

        protected Grouping _localData;          // Container for KeyValuePair<string,object>
        protected Grouping _tempData;           // Container for KeyValuePair<string,object>
        protected List<Grouping> _data;
        protected List<int> _hierarchy;
        protected int _dataId;

        protected bool cancel = false;
        protected bool _enabled = true;
        protected bool terminate = false;

        #endregion
        #region Constructors
        public Element()
        {
            _dataId = _dataId + 1;
            _data = new List<Grouping>();
            _localData = new Grouping();
            _tempData = new Grouping();
            _hierarchy = new List<int>();
            terminate = false;
            cancel = false;
            _enabled = false;
        }

        public Element(string id)
        {
            _dataId = _dataId + 1;
            _data = new List<Grouping>();
            _localData = new Grouping();
            _tempData = new Grouping();
            _hierarchy = new List<int>();
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

        public List<int> Hierarchy
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

        public List<Grouping> Data
        {
            get
            {
                return (_data);
            }
        }

        public Grouping LocalData
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

        /// <summary>
        /// Update object by key to List<string,KeyValuePair<string,object>>
        /// Will add it doesn't exist, otherwise it will update the value for the key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool UpdateData(string key, object value)
        {
            bool update = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Add data: key=" + key + " value=" + value);
                KeyValuePair<string, object> item = new KeyValuePair<string, object>(key, value);
                for (int stage = 0; stage < _data.Count; stage++)
                {
                    Grouping grouping = _data[stage];
                    KeyValuePair<string, object> existing;
                    int count = 0;
                    do
                    {
                        existing = grouping[count];
                        if ((string)existing.Key == key)
                        {
                            TraceInternal.TraceVerbose("[" + _sessionId + "] Replace data: key=" + key);
                            grouping.RemoveAt(count);
                            break;
                        }
                        else
                        {
                            count = count + 1;
                        }
                    }
                    while (count < grouping.Count);
                    grouping.Add(item);
                }
            }
            catch { }
            return (update);
        }

        /// <summary>
        /// Add object by key to List<string,KeyValuePair<string,object>>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool AddData(string key, object value)
        {
            bool update = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Add data: key=" + key + " value=" + value);
                KeyValuePair<string, object> item = new KeyValuePair<string, object>(key, value);
                for (int stage = 0; stage < _data.Count; stage++)
                {
                    Grouping grouping = _data[stage];
                    KeyValuePair<string, object> existing;
                    int count = 0;
                    do
                    {
                        existing = grouping[count];
                        if ((string)existing.Key == key)
                        {
                            TraceInternal.TraceVerbose("[" + _sessionId + "] Replace data: key=" + key);
                            grouping.RemoveAt(count);
                            break;
                        }
                        else
                        {
                            count = count + 1;
                        }
                    }
                    while (count < grouping.Count);
                    grouping.Add(item);
                    update = true;
                }
            }
            catch { }
            return (update);
        }

        /// <summary>
        /// Select object by key from List<string,KeyValuePair<string,object>>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual object SelectData(string key)
        {
            object value = null;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Select data: key=" + key);
                for (int stage = 0; stage < _data.Count; stage++)
                {
                    Grouping grouping = _data[stage];
                    KeyValuePair<string, object> item;
                    for (int count = 0; count < grouping.Count; count++)
                    {
                        item = grouping[count];
                        if ((string)item.Key == key)
                        {
                            value = item.Value;
                            break;
                        }
                    }
                }
            }
            catch { }
            return (value);
        }

        /// <summary>
        /// Remove object by key from ArrayList of Dictionary objects
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool RemoveData(string key)
        {
            bool remove = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Select data: key=" + key);
                for (int stage = 0; stage < _data.Count; stage++)
                {
                    Grouping grouping = _data[stage];
                    for (int count = 0; count < grouping.Count; count++)
                    {
                        KeyValuePair<string, object> item = grouping[count];
                        if ((string)item.Key == key)
                        {
                            grouping.RemoveAt(count);
                            remove = true;
                            break;
                        }
                    }
                }
            }
            catch { }
            return (remove);
        }

        /// <summary>
        /// Select object by key from List<string,KeyValuePair<string,object>>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual object SelectLocalData(string key)
        {
            object value = null;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Select local data: key=" + key);
                for (int count = 0; count < _localData.Count; count++)
                {
                    KeyValuePair<string, object> item = _localData[count];
                    if ((string)item.Key == key)
                    {
                        value = item.Value;
                        break;
                    }
                }
            }
            catch { }
            return (value);
        }

        /// <summary>
        /// Add data to List<string,KeyValuePair<string,object>>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool AddLocalData(string key, object value)
        {
            bool add = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Add local data: key=" + key + " value=" + value);
                KeyValuePair<string, object> item = new KeyValuePair<string, object>(key, value);
                for(int count = 0; count < _localData.Count; count++)
                {
                    KeyValuePair<string, object> existing = _localData[count];
                    if ((string)existing.Key == item.Key)
                    {
                        TraceInternal.TraceVerbose("[" + _sessionId + "] Remove local data: key=" + key);
                        _localData.RemoveAt(count);
                        add = true;
                        break;
                    }
                }
                _localData.Add(item);
                add = true;
            }
            catch { }
            return (add);
        }

        /// <summary>
        /// Update data in List<string,KeyValuePair<string,object>>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool UpdateLocalData(string key, object value)
        {
            bool update = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Update local data: key=" + key + " value=" + value);
                KeyValuePair<string, object> item = new KeyValuePair<string, object>(key, value);
                for (int count = 0; count < _localData.Count; count++)
                {
                    KeyValuePair<string, object> existing = _localData[count];
                    if (existing.Key == item.Key)
                    {
                        TraceInternal.TraceVerbose("[" + _sessionId + "] Update local data: key=" + key);
                        _localData.RemoveAt(count);
                        break;
                    }
                }
                _localData.Add(item);
                update = true;
            }
            catch { }
            return (update);
        }

        /// <summary>
        /// Remove object by key from List<string,KeyValuePair<string,object>>  
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool RemoveLocalData(string key)
        {
            bool remove = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Remove local data: key=" + key);
                for (int count = 0; count < _localData.Count; count++)
                {
                    KeyValuePair<string, object> item = (KeyValuePair<string, object>)_localData[count];
                    if ((string)item.Key == key)
                    {
                        _localData.RemoveAt(count);
                        remove = true;
                        break;
                    }
                }
            }
            catch { }
            return (remove);
        }

        #endregion Methods
    }
}
