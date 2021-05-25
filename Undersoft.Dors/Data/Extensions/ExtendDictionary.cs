using System.Dors;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace System.Dors.Data
{
    public static class Dictionary
    {

        public static void Add(this Dictionary<int, Dictionary<int, IDataTier>> source, IDataTier item)
        {
            byte[] keys = item.GetShah();
            int key = BitConverter.ToInt32(keys, 0);
            int key2 = BitConverter.ToInt32(keys, 4);
            source.Add(key, new Dictionary<int, IDataTier>(new HashComparer()) { { key2, item } });
        }
        public static void Put(this Dictionary<int, Dictionary<int, IDataTier>> source, IDataTier item)
        {
            byte[] keys = item.GetShah();
            int key = BitConverter.ToInt32(keys, 0);
            int key2 = BitConverter.ToInt32(keys, 4);
            Dictionary<int, IDataTier> tiers = null;
            IDataTier tier = null;
            if (!source.TryGetValue(key, out tiers))
                source.Add(key, new Dictionary<int, IDataTier>(new HashComparer()) { { key2, item } });
            else if (!tiers.TryGetValue(key2, out tier))
                tiers.Add(key2, item);
            else
                tier = item;
        }
        public static bool TryGetValue(this Dictionary<int, Dictionary<int, IDataTier>> source, IDataTier item, out IDataTier value)
        {
            value = null;
            byte[] keys = item.GetShah();
            int key = BitConverter.ToInt32(keys, 0);
            int key2 = BitConverter.ToInt32(keys, 4);
            Dictionary<int, IDataTier> tiers = null;
            value = null;
            if (source.TryGetValue(key, out tiers))
                if (tiers.TryGetValue(key2, out value))
                    return true;               
                else
                    return false;
            else
                return false;
        }     
        public static bool TryGetValue(this Dictionary<int, Dictionary<int, IDataTier>> source, object item, int id, out IDataTier value)
        {
            value = null;
            byte[] keys = (byte[])((IDataNative)item)[id];
            int key = BitConverter.ToInt32(keys, 0);
            int key2 = BitConverter.ToInt32(keys, 4);
            Dictionary<int, IDataTier> tiers = null;
            value = null;
            if (source.TryGetValue(key, out tiers))
                if (tiers.TryGetValue(key2, out value))                                   
                    return true;                
                else
                    return false;
            else
                return false;
        }
        public static bool Contains(this Dictionary<int, Dictionary<int, IDataTier>> source, IDataTier item)
        {
            byte[] keys = item.GetShah();
            int key = BitConverter.ToInt32(keys, 0);
            int key2 = BitConverter.ToInt32(keys, 4);
            Dictionary<int, IDataTier> tiers = null;
            if (source.TryGetValue(key, out tiers))
                if (tiers.ContainsKey(key2))                                    
                    return true;               
                else
                    return false;
            else
                return false;
        }
        public static bool Contains(this Dictionary<int, Dictionary<int, IDataTier>> source, object item, int noid, int id)
        {
            byte[] keys = ((IDataNative)item).GetShah(noid, id);
            int key = BitConverter.ToInt32(keys, 0);
            int key2 = BitConverter.ToInt32(keys, 4);
            Dictionary<int, IDataTier> tiers = null;
            if (source.TryGetValue(key, out tiers))
                if (tiers.ContainsKey(key2))
                    return true;
                else
                    return false;
            else
                return false;
        }
    }

    public static class SortedDictionary
    {

        public static void Add(this SortedDictionary<int, Dictionary<int, List<IDataTier>>> source, IDataTier item)
        {
            byte[] keys = item.GetShah();
            int key = BitConverter.ToInt32(keys, 0);
            int key2 = BitConverter.ToInt32(keys, 4);
            source.Add(key, new Dictionary<int, List<IDataTier>> (new HashComparer()) { { key2, new List<IDataTier>() { item } } });
        }
        public static void Put(this SortedDictionary<int, Dictionary<int, List<IDataTier>>> source, IDataTier item)
        {
            byte[] keys = item.GetShah();
            int key = BitConverter.ToInt32(keys, 0);
            int key2 = BitConverter.ToInt32(keys, 4);
            Dictionary<int, List<IDataTier>> tiers = null;
            List <IDataTier> tier = null;
            if (!source.TryGetValue(key, out tiers))
                source.Add(key, new Dictionary<int, List<IDataTier>>(new HashComparer()) { { key2, new List<IDataTier>() { item } } });
            else if (!tiers.TryGetValue(key2, out tier))
                tiers.Add(key2, new List<IDataTier>() { item });
            else
            {
                int id = tier.IndexOf(item);
                if (id >= 0)
                    tier[id] = item;
                else
                    tier.Add(item);
            }
        }
        public static bool TryGetValue(this SortedDictionary<int, Dictionary<int, List<IDataTier>>> source, IDataTier item, out List<IDataTier> value)
        {
            value = null;
            byte[] keys = item.GetShah();
            int key = BitConverter.ToInt32(keys, 0);
            int key2 = BitConverter.ToInt32(keys, 4);
            Dictionary<int, List<IDataTier>> tiers = null;
            value = null;
            if (source.TryGetValue(key, out tiers))
                if (tiers.TryGetValue(key2, out value))
                    return true;
                else
                    return false;
            else
                return false;
        }
        public static bool TryGetValue(this SortedDictionary<int, Dictionary<int, List<IDataTier>>> source, object item, int id, out List<IDataTier> value)
        {
            value = null;
            byte[] keys = (byte[])((IDataNative)item)[id];
            int key = BitConverter.ToInt32(keys, 0);
            int key2 = BitConverter.ToInt32(keys, 4);
            Dictionary<int, List<IDataTier>> tiers = null;
            value = null;
            if (source.TryGetValue(key, out tiers))
                if (tiers.TryGetValue(key2, out value))
                    return true;
                else
                    return false;
            else
                return false;
        }
        public static bool Contains(this SortedDictionary<int, Dictionary<int, List<IDataTier>>> source, IDataTier item)
        {
            byte[] keys = item.GetShah();
            int key = BitConverter.ToInt32(keys, 0);
            int key2 = BitConverter.ToInt32(keys, 4);
            Dictionary<int, List<IDataTier>> tiers = null;
            List<IDataTier> tier = null;
            if (source.TryGetValue(key, out tiers))
                if (tiers.TryGetValue(key2, out tier))
                {
                    int id = tier.IndexOf(item);
                    if (id >= 0)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            else
                return false;
        }
        public static bool Contains(this SortedDictionary<int, Dictionary<int, List<IDataTier>>> source, object item, int noid, int id)
        {
            byte[] keys = ((IDataNative)item).GetShah(noid, id);
            int key = BitConverter.ToInt32(keys, 0);
            int key2 = BitConverter.ToInt32(keys, 4);
            Dictionary<int, List<IDataTier>> tiers = null;
            List<IDataTier> tier = null;
            if (source.TryGetValue(key, out tiers))
                if (tiers.TryGetValue(key2, out tier))
                {
                    foreach(IDataTier t in tier)
                    {
                        if (t.GetShah().SequenceEqual(keys))
                            return true;                          
                    }
                    return false;
                }
                else
                    return false;
            else
                return false;
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

    }
}
