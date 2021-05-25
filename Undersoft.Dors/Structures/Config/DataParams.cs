using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Dors
{
    [Serializable]
    public class DataParams : IDictionary<string, object>, IEnumerable<KeyValuePair<string, object>>
    {
        public object this[string key]
        {
            get
            {
                object result = null;
                if (parameters.TryGetValue(key, out result))
                    return result;
                return null;
            }
            set
            {
                if (parameters.ContainsKey(key))
                    parameters[key] = value;
                else
                    parameters.Add(key, value);
            }
        }

        #region IDictionary
        private Dictionary<string, object> parameters = new Dictionary<string, object>();

        public object Get(string key)
        {
            object result = null;

            if (parameters.ContainsKey(key))
            {
                result = parameters[key];
            }
            return result;
        }
        public void Set(string key, object value)
        {
            if (parameters.ContainsKey(key))
            {
                parameters[key] = value;
            }
            else
            {
                parameters.Add(key, value);
            }
        }
        public KeyValuePair<string, object> SetPair
        {
            set
            {
                Set(value.Key, value.Value);
            }
        }
        public Dictionary<string, object> Registry
        {
            get
            {
                return parameters;
            }
            set
            {
                parameters = value;
            }
        }

        public void Add(string key, object value)
        {
            this.Add(new KeyValuePair<string, object>(key, value));
        }
        public void Add(KeyValuePair<string, object> item)
        {
            object o = new object();
            if (parameters.TryGetValue(item.Key, out o))
            {
                ((IDataConfig)parameters[item.Key]).Config.SetMapConfig(this);
            }
        }
        public bool TryAdd(string key, object value)
        {
            object o = new object();
            if (parameters.TryGetValue(key, out o))
            {
                ((IDataConfig)o).Config.SetMapConfig(this);
                return true;
            }
            else
                return false;
        }
        public void Put(string key, object value)
        {
            parameters.Add(key, value);
            ((IDataConfig)parameters[key]).Config.SetMapConfig(this);
        }
        public bool ContainsKey(string key)
        {
            return parameters.ContainsKey(key);
        }
        public bool Remove(string key)
        {
            object outset = null;
            bool done = parameters.Remove(key);
            if (done)
            {
                object outreg = new object();
                DataCore.Space.Registry.TryRemove(((IDataConfig)outset).Config.GetDataId(), out outreg);
            }
            return done;
        }
        public bool TryGetValue(string key, out object value)
        {
            return parameters.TryGetValue(key, out value);
        }
        public void Clear()
        {
            parameters.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return parameters.Contains(item);
        }
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            parameters.ToArray().CopyTo(array, arrayIndex);
        }
        public bool Remove(KeyValuePair<string, object> item)
        {
            bool done = parameters.Remove(item.Key);
            if (done)
            {
                object outreg = new object();
                DataCore.Space.Registry.TryRemove(((IDataConfig)item.Value).Config.GetDataId(), out outreg);
            }
            return done;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return parameters.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return parameters.GetEnumerator();
        }

        public ICollection<string> Keys
        {
            get
            {
                return parameters.Keys;
            }
        }
        public ICollection<object> Values
        {
            get
            {
                return parameters.Values;
            }
        }
        public int Count
        {
            get
            {
                return parameters.Count;
            }
        }
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        #endregion
    }
}
