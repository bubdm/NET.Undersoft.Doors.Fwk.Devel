using System.Collections.Generic;
using System.Collections.Concurrent;

namespace System.Dors
{
    public static class Dictionary
    {
        public static void AddRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
            }
        }
        public static void AddRange<T, S>(this Dictionary<T, S> source, IDictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
            }
        }
        public static void AddRange<T, S>(this IDictionary<T, S> source, IDictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
            }
        }
        public static Dictionary<T, S> AddRangeMsg<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            Dictionary<T, S> result = new Dictionary<T, S>();
            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
                else
                    result.Add(item.Key, item.Value);
            }
            return result;
        }
        public static void PutRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
                else
                    source[item.Key] = item.Value;
            }
        }
        public static void Put<T, S>(this Dictionary<T, S> source, T Key, S Value)
        {
            if (Key == null || Value == null)
                throw new ArgumentNullException("Collection is null");

            if (source.ContainsKey(Key))
                source[Key] = Value;
            else
                source.Add(Key, Value);

        }
        public static Dictionary<T, S> GetDict<T, S>(this Dictionary<T, S> source, T key)
        {
            if (key == null)
                throw new ArgumentNullException("Collection is null");
            Dictionary<T, S> result = new Dictionary<T, S>();
            if (source.ContainsKey(key))
                result.Add(key, source[key]);

            return result;
        }
        public static S Get<T, S>(this Dictionary<T, S> source, T key)
        {
            if (key == null)
                throw new ArgumentNullException("Collection is null");
            S result = default(S);
            if (source.ContainsKey(key))
                result = source[key];

            return result;
        }
        public static void Add<T, S>(this Dictionary<T, Dictionary<T, S>> source, T key, T key2, S item)
        {
            if (key == null || item == null)
                throw new ArgumentNullException("Collection is null");

            source.Add(key, new Dictionary<T, S>() { { key2, item } });
        }
        public static bool TryAdd<T, S>(this Dictionary<T, S> source, T Key, S Value)
        {
            if (Key == null || Value == null)
                throw new ArgumentNullException("Collection is null");

            if (source.ContainsKey(Key))
                return false;
            else
                source.Add(Key, Value);
            return true;
        }
        public static S AddOrUpdate<T, S>(this Dictionary<T, S> source, T Key, S Value, Func<T, S, S> func)
        {
            if (Key == null || Value == null)
                throw new ArgumentNullException("Collection is null");

            if (source.ContainsKey(Key))
                return source[Key] = func(Key, Value);
            else
            {
                source.Add(Key, Value);
                return Value;
            }

        }
        public static bool TryRemove<T, S>(this Dictionary<T, S> source, T Key, out S Value)
        {
            if (Key == null)
                throw new ArgumentNullException("Collection is null");
            
            if (source.TryGetValue(Key, out Value))
            {
                source.Remove(Key);
                return true;
            }                           
            return false;
        }
        public static void AddOrUpdateRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                source.AddOrUpdate(item.Key, item.Value, (k, v) => v = item.Value);
            }
        }

        public static void Put<T, S>(this Dictionary<T, Dictionary<T, S>> source, T key, T key2, S item)
        {
            if (key == null || item == null)
                throw new ArgumentNullException("Collection is null");

            if (!source.ContainsKey(key))
                source.Add(key, new Dictionary<T, S>() { { key2, item } });
            else if (!source[key].ContainsKey(key2))
                source[key].Add(key2, item);
            else
                source[key][key2] = item;
        }   
        public static void RemoveRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (source.ContainsKey(item.Key))
                    source.Remove(item.Key);
            }
        }
        public static void RemoveRangeList<T, S>(this Dictionary<T, S> source, List<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (source.ContainsKey(item))
                    source.Remove(item);
            }
        }
        public static List<S> GetRangeList<T, S>(this Dictionary<T, S> source, List<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            List<S> result = new List<S>();
            foreach (var item in collection)
            {
                if (source.ContainsKey(item))
                    result.Add(source[item]);
            }
            return result;
        }
        public static Dictionary<T, S> GetRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            Dictionary<T, S> result = new Dictionary<T, S>();
            foreach (var item in collection)
            {
                if (source.ContainsKey(item.Key))
                    result.Add(item.Key, source[item.Key]);
            }
            return result;
        }
        public static Dictionary<T, S>[] GetRangeMsg<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            Dictionary<T, S>[] result = new Dictionary<T, S>[2];
            foreach (var item in collection)
            {
                if (source.ContainsKey(item.Key))
                    result[0].Add(item.Key, source[item.Key]);
                else
                    result[1].Add(item.Key, item.Value);
            }
            return result;
        }
    }

    public static class SortedDictionary
    {
        public static void AddRange<T, S>(this SortedDictionary<T, S> source, SortedDictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
            }
        }
        public static void AddDict<T, S>(this SortedDictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
            }
        }
        public static SortedDictionary<T, S> AddRangeMsg<T, S>(this SortedDictionary<T, S> source, SortedDictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            SortedDictionary<T, S> result = new SortedDictionary<T, S>();
            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
                else
                    result.Add(item.Key, item.Value);
            }
            return result;
        }
        public static void PutRange<T, S>(this SortedDictionary<T, S> source, SortedDictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
                else
                    source[item.Key] = item.Value;
            }
        }
        public static SortedDictionary<T, S> GetDict<T, S>(this SortedDictionary<T, S> source, T key)
        {
            if (key == null)
                throw new ArgumentNullException("Collection is null");
            SortedDictionary<T, S> result = new SortedDictionary<T, S>();
            if (source.ContainsKey(key))
                result.Add(key, source[key]);

            return result;
        }
        public static S Get<T, S>(this SortedDictionary<T, S> source, T key)
        {
            if (key == null)
                throw new ArgumentNullException("Collection is null");
            S result = default(S);
            if (source.ContainsKey(key))
                result = source[key];

            return result;
        }

        public static bool TryAdd<T, S>(this SortedDictionary<T, S> source, T Key, S Value)
        {
            if (Key == null || Value == null)
                throw new ArgumentNullException("Collection is null");

            if (source.ContainsKey(Key))
                return false;
            else
                source.Add(Key, Value);
            return true;
        }
        public static S AddOrUpdate<T, S>(this SortedDictionary<T, S> source, T Key, S Value, Func<T, S, S> func)
        {
            if (Key == null || Value == null)
                throw new ArgumentNullException("Collection is null");

            if (source.ContainsKey(Key))
                return source[Key] = func(Key, Value);
            else
            {
                source.Add(Key, Value);
                return Value;
            }

        }
        public static bool TryRemove<T, S>(this SortedDictionary<T, S> source, T Key, out S Value)
        {
            if (Key == null)
                throw new ArgumentNullException("Collection is null");

            if (source.TryGetValue(Key, out Value))
            {
                source.Remove(Key);
                return true;
            }
            return false;
        }


        public static void AddOrUpdateRange<T, S>(this SortedDictionary<T, S> source, IDictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                source.AddOrUpdate(item.Key, item.Value, (k, v) => v = item.Value);
            }
        }
        public static void Put<T, S>(this SortedDictionary<T, S> source, T key, S item)
        {
            if (key == null || item == null)
                throw new ArgumentNullException("Collection is null");

            if (!source.ContainsKey(key))
                source.Add(key, item);
            else
                source[key] = item;

        }
        public static void Put<T, S>(this SortedDictionary<T, Dictionary<T, S>> source, T key, T key2, S item)
        {
            if (key == null || item == null)
                throw new ArgumentNullException("Collection is null");

            if (!source.ContainsKey(key))
                source.Add(key, new Dictionary<T, S>() { { key2, item } });
            else if (!source[key].ContainsKey(key2))
                source[key].Add(key2, item);
            else
                source[key][key2] = item;
        }
        public static void RemoveRange<T, S>(this SortedDictionary<T, S> source, SortedDictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (source.ContainsKey(item.Key))
                    source.Remove(item.Key);
            }
        }
        public static void RemoveRangeList<T, S>(this SortedDictionary<T, S> source, List<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (source.ContainsKey(item))
                    source.Remove(item);
            }
        }
        public static List<S> GetRangeList<T, S>(this SortedDictionary<T, S> source, List<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            List<S> result = new List<S>();
            foreach (var item in collection)
            {
                if (source.ContainsKey(item))
                    result.Add(source[item]);
            }
            return result;
        }
        public static SortedDictionary<T, S> GetRange<T, S>(this SortedDictionary<T, S> source, SortedDictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            SortedDictionary<T, S> result = new SortedDictionary<T, S>();
            foreach (var item in collection)
            {
                if (source.ContainsKey(item.Key))
                    result.Add(item.Key, source[item.Key]);
            }
            return result;
        }
        public static SortedDictionary<T, S>[] GetRangeMsg<T, S>(this SortedDictionary<T, S> source, SortedDictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            SortedDictionary<T, S>[] result = new SortedDictionary<T, S>[2];
            foreach (var item in collection)
            {
                if (source.ContainsKey(item.Key))
                    result[0].Add(item.Key, source[item.Key]);
                else
                    result[1].Add(item.Key, item.Value);
            }
            return result;
        }
    }

    public static class SortedList
    {
        public static void AddRange<T, S>(this SortedList<T, S> source, SortedList<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
            }
        }
        public static bool AddDict<T, S>(this SortedList<T, S> source, Dictionary<T, S> collection)
        {
            int added = 0;
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                {
                    source.Add(item.Key, item.Value);
                    added++;
                }
            }
            return added > 0;
        }
        public static SortedList<T, S> AddRangeMsg<T, S>(this SortedList<T, S> source, SortedList<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            SortedList<T, S> result = new SortedList<T, S>();
            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
                else
                    result.Add(item.Key, item.Value);
            }
            return result;
        }     
        public static void PutRange<T, S>(this SortedList<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
                else
                    source[item.Key] = item.Value;
            }
        }
        public static void PutRange<T, S>(this SortedList<T, S> source, SortedList<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
                else
                    source[item.Key] = item.Value;
            }
        }
        public static SortedList<T, S> GetDict<T, S>(this SortedList<T, S> source, T key)
        {
            if (key == null)
                throw new ArgumentNullException("Collection is null");
            SortedList<T, S> result = new SortedList<T, S>();
            if (source.ContainsKey(key))
                result.Add(key, source[key]);

            return result;
        }
        public static S Get<T, S>(this SortedList<T, S> source, T key)
        {
            if (key == null)
                throw new ArgumentNullException("Collection is null");
            S result = default(S);
            if (source.ContainsKey(key))
                result = source[key];

            return result;
        }
        public static void Put<T, S>(this SortedList<T, S> source, T key, S item)
        {           
            if (!source.ContainsKey(key))
                source.Add(key, item);
            else
                source[key] = item;
        }
        public static void RemoveRange<T, S>(this SortedList<T, S> source, SortedList<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (source.ContainsKey(item.Key))
                    source.Remove(item.Key);
            }
        }
        public static void RemoveRangeList<T, S>(this SortedList<T, S> source, List<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (source.ContainsKey(item))
                    source.Remove(item);
            }
        }
        public static List<S> GetRangeList<T, S>(this SortedList<T, S> source, List<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            List<S> result = new List<S>();
            foreach (var item in collection)
            {
                if (source.ContainsKey(item))
                    result.Add(source[item]);
            }
            return result;
        }
        public static SortedList<T, S> GetRange<T, S>(this SortedList<T, S> source, SortedList<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            SortedList<T, S> result = new SortedList<T, S>();
            foreach (var item in collection)
            {
                if (source.ContainsKey(item.Key))
                    result.Add(item.Key, source[item.Key]);
            }
            return result;
        }
        public static SortedList<T, S>[] GetRangeMsg<T, S>(this SortedList<T, S> source, SortedList<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            SortedList<T, S>[] result = new SortedList<T, S>[2];
            foreach (var item in collection)
            {
                if (source.ContainsKey(item.Key))
                    result[0].Add(item.Key, source[item.Key]);
                else
                    result[1].Add(item.Key, item.Value);
            }
            return result;
        }       
    }

    public static class ConcurrentDictionary
    {
        public static void Put<T, S>(this ConcurrentDictionary<T, Dictionary<T, S>> source, T key, T key2, S item)
        {
            if (key == null || item == null)
                throw new ArgumentNullException("Collection is null");

            if (!source.TryAdd(key, new Dictionary<T, S>() { { key2, item } }))
            {
                if (!source[key].ContainsKey(key2))
                    source[key].Add(key2, item);
                else
                    source[key][key2] = item;
            }
        }
        public static void AddOrUpdateRange<T, S>(this ConcurrentDictionary<T, S> source, ConcurrentDictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                 source.AddOrUpdate(item.Key, item.Value, (k,v) => v = item.Value);
            }
        }
        public static ConcurrentDictionary<T, S> AddRangeMsg<T, S>(this ConcurrentDictionary<T, S> source, ConcurrentDictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            ConcurrentDictionary<T, S> result = new ConcurrentDictionary<T, S>();
            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.AddOrUpdate(item.Key, item.Value, (k, v) => item.Value);
                else
                    result.AddOrUpdate(item.Key, item.Value, (k, v) => item.Value);
            }
            return result;
        }            
    }

    public static class NdhcDictionary
    {
        public static void AddRange<T, S>(this Dictionary<T, Dictionary<T, Dictionary<T, S>>> source, Dictionary<T, Dictionary<T, Dictionary<T, S>>> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
            }
        }     
        public static void PutRange<T, S>(this Dictionary<T, Dictionary<T, Dictionary<T, S>>> source, Dictionary<T, Dictionary<T, Dictionary<T, S>>> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (!source.ContainsKey(item.Key))
                    source.Add(item.Key, item.Value);
                else
                    source[item.Key] = item.Value;
            }
        }
        public static Dictionary<T, S> GetDict<T, S>(this Dictionary<T, S> source, T key)
        {
            if (key == null)
                throw new ArgumentNullException("Collection is null");
            Dictionary<T, S> result = new Dictionary<T, S>();
            if (source.ContainsKey(key))
                result.Add(key, source[key]);

            return result;
        }
        public static object Get<T, S>(this Dictionary<T, Dictionary<T, Dictionary<T, S>>> source, T key, T key2, T key3)
        {
            if (key == null)
                throw new ArgumentNullException("Collection is null");
            object result = default(S);
            if (source.ContainsKey(key))
                if (key2 == null)
                    result = source[key];
                else if (source.ContainsKey(key2))
                    if (key3 == null)
                        result = source[key][key2];
                    else if (source.ContainsKey(key3))
                        result = source[key][key2][key3];
                    else
                        result = source[key][key2];
                else
                    result = source[key];

            return result;
        }
        public static void Put<T, S>(this Dictionary<T, Dictionary<T, Dictionary<T, S>>> source, T key, T key2, T key3, S item)
        {
            if (key == null || item == null)
                throw new ArgumentNullException("Collection is null");

            if (!source.ContainsKey(key))
                source.Add(key, new Dictionary<T, Dictionary<T, S>>() { { key2, new Dictionary<T, S>() { { key3, item } } } });
            else if (!source[key].ContainsKey(key2))
                source[key].Add(key2, new Dictionary<T, S>() { { key3, item } });
            else if (!source[key][key2].ContainsKey(key3))
                source[key][key2].Add(key3, item);
            else
                source[key][key2][key3] = item;
        }
        public static void RemoveRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (source.ContainsKey(item.Key))
                    source.Remove(item.Key);
            }
        }
        public static void RemoveRangeList<T, S>(this Dictionary<T, S> source, List<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");

            foreach (var item in collection)
            {
                if (source.ContainsKey(item))
                    source.Remove(item);
            }
        }
        public static List<S> GetRangeList<T, S>(this Dictionary<T, S> source, List<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            List<S> result = new List<S>();
            foreach (var item in collection)
            {
                if (source.ContainsKey(item))
                    result.Add(source[item]);
            }
            return result;
        }
        public static Dictionary<T, S> GetRange<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            Dictionary<T, S> result = new Dictionary<T, S>();
            foreach (var item in collection)
            {
                if (source.ContainsKey(item.Key))
                    result.Add(item.Key, source[item.Key]);
            }
            return result;
        }
        public static Dictionary<T, S>[] GetRangeMsg<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("Collection is null");
            Dictionary<T, S>[] result = new Dictionary<T, S>[2];
            foreach (var item in collection)
            {
                if (source.ContainsKey(item.Key))
                    result[0].Add(item.Key, source[item.Key]);
                else
                    result[1].Add(item.Key, item.Value);
            }
            return result;
        }
    }

}
