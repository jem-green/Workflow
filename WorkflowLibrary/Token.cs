using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using TracerLibrary;

namespace WorkflowLibrary
{
    /// <summary>
    /// Token to store data passed between Elements
    /// </summary>
    public class Token
    {
        #region Fields

        private string _sessionId = "";
        private ArrayList _data = new ArrayList();

        #endregion
        #region Constructors

        public Token(string sessionId)
        {
            _sessionId = sessionId;
        }

        #endregion
        #region Properties

        public ArrayList Data
        {
            get
            {
                return (_data);
            }
        }

        #endregion
        #region Methods

        /// <summary>
        /// Update data in ArrayList of Dictionary objects
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool UpdateData(string key, object value)
        {
            bool add = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Add data: key=" + key + " value=" + value);
                for (int i = 0; i < _data.Count; i++)
                {
                    DictionaryEntry item = (DictionaryEntry)_data[i];
                    if ((string)item.Key == key)
                    {
                        _data[i] = new DictionaryEntry(key, value);
                        add = true;
                        break;
                    }
                }
            }
            catch { }
            return (add);
        }

        /// <summary>
        /// Add data to ArrayList of Dictionary objects
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddData(string key, object value)
        {
            bool add = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Add data: key=" + key + " value=" + value);
                DictionaryEntry item = new DictionaryEntry(key, value);
                _data.Add(item);
                add = true;
            }
            catch { }
            return (add);
        }

        /// <summary>
        /// Select object by key from ArrayList of Dictionary objects
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object SelectData(string key)
        {
            object value = null;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Select data: key=" + key);
                foreach (DictionaryEntry item in _data)
                {
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
        /// Remove object by key from ArrayList of Dictionary objects
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool RemoveData(string key)
        {
            bool remove = false;
            try
            {
                TraceInternal.TraceVerbose("[" + _sessionId + "] Remove data: key=" + key);
                for (int i = 0; i < _data.Count; i++)
                {
                    DictionaryEntry item = (DictionaryEntry)_data[i];
                    if ((string)item.Key == key)
                    {
                        _data.RemoveAt(i);
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
