using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace WorkflowLibrary
{
    public class IndexCollection<K,V>:IEnumerable
    {
        #region Fields
            private Collection<V> values;
            private ArrayList keys;
        #endregion       
        #region Constructors
        public IndexCollection()
        {
            values = new Collection<V>();
            keys = new ArrayList();
        }
        #endregion
        #region Properites

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }


        public int Count
        {
            get
            {
                return (values.Count);
            }
        }

        #endregion
        #region Methods

        public void Add(K key, V value)
        {
            values.Add(value);
            keys.Add(key);
        }

        public void Clear()
        {
            values.Clear();
            keys.Clear();
        }

        public bool Contains(V value)
        {
            return(values.Contains(value));
        }

        /// <summary>
        /// Get the index of a particular key.
        /// </summary>
        /// <param name="key">The key to find the index of.</param>
        /// <returns>The index of the key, or -1 if not found.</returns>
        public int IndexOf(K key)
        {
            int ret = -1;

            for (int i = 0; i < values.Count; i++)
            {
                if (keys[i].Equals(key))
                {
                    ret = i;
                    break;
                }
            }

            return ret;
        }

        public V GetValue(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            return (values[index]);
        }


        public void Remove(V value)
        {
            values.Remove(value);
        }

        public IEnumerator<V> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();  // Calls IEnumerator<T> GetEnumerator()
        }

        #endregion

    }
}
