using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace WorkflowLibrary
{
    public class Grouping : IEnumerable<KeyValuePair<string, object>>
    {
        private readonly List<KeyValuePair<string, object>> _pairs = new();

        public Grouping() { }

        public Grouping(IEnumerable<KeyValuePair<string, object>> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            _pairs.AddRange(source);
        }

        public int Count
        {
            get
            {
                return _pairs.Count;
            }
        }

        public void Add(string key, object value) => _pairs.Add(new KeyValuePair<string, object>(key, value));

        public void Add(KeyValuePair<string, object> pair)
        {
            _pairs.Add(pair);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _pairs.GetEnumerator();

        public void Remove(string key)
        {
            _pairs.RemoveAll(pair => pair.Key == key);
        }

        internal void RemoveAt(int count)
        {
            _pairs.RemoveAt(count);
        }

        public KeyValuePair<string, object> this[int index]
        {
            get => _pairs[index];
            set => _pairs[index] = value;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}