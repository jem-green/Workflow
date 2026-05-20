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

        private string _tokenId = "";
        private Grouping _data = new Grouping();

        #endregion
        #region Constructors

        public Token(string tokenId)
        {
            _tokenId = tokenId;
        }

        #endregion
        #region Properties

        public Grouping Data
        {
            get
            {
                return (_data);
            }
        }

        public string TokenId
        {
            get
            {
                return (_tokenId);
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
                TraceInternal.TraceVerbose("[" + _tokenId + "] Add data: key=" + key + " value=" + value);
                KeyValuePair<string, object> item = new KeyValuePair<string, object>(key, value);
                for (int count = 0; count < _data.Count; count++)
                {
                    KeyValuePair<string, object> existing = _data[count];
                    if (existing.Key == item.Key)
                    {
                        _data[count] = item;
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
                TraceInternal.TraceVerbose("[" + _tokenId + "] Add data: key=" + key + " value=" + value);
                KeyValuePair<string, object> item = new KeyValuePair<string, object>(key, value);
                for (int count = 0; count < _data.Count; count++)
                {
                    if ((string)item.Key == key)
                    {
                        _data.RemoveAt(count);
                        add = true;
                        break;
                    }
                }
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
                TraceInternal.TraceVerbose("[" + _tokenId + "] Select data: key=" + key);
                for (int count = 0; count < _data.Count; count++)
                {
                    KeyValuePair<string, object> existing = _data[count];
                    if (existing.Key == key)
                    {
                        value = existing.Value;
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
                TraceInternal.TraceVerbose("[" + _tokenId + "] Remove data: key=" + key);
                for (int count = 0; count < _data.Count; count++)
                {
                    KeyValuePair<string, object> item = _data[count];
                    if ((string)item.Key == key)
                    {
                        _data.RemoveAt(count);
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
