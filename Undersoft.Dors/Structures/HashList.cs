using System.Collections.Generic;
using System.Collections;

/**
 ╔════════════════════════════════════════════════════════════╗ 
 ║  This class implements a Hash List with long type of hash key code     ║
 ║  Better performence from structure alghoritms like                     ║
 ║  HashMap, Dictionary. HashTable, HashSet.                              ║
 ║                                                                        ║
 ║  @author Dariusz Hanc                                                  ║
 ║  @version 1.1 (Nov 9, 2018)                                            ║
 ╚════════════════════════════════════════════════════════════╝
 */
namespace System.Dors
{
    public class Vessel<V>
    {
        public Vessel()
        { }
        public Vessel(IQuid key, V value)
        {
            Key = key.LongValue;
            Value = value;
        }
        public Vessel(int key, V value)
        {
            Key = key;
            Value = value;
        }
        public Vessel(long key, V value)
        {
            Key = key;
            Value = value;
        }
        public Vessel<V> Set(long key, V value)
        {
            Key = key;
            Value = value;
            Removed = false;
            return this;
        }

        public bool Removed;

        public long Key;
        public V    Value;

        public Vessel<V> Extent;
        public Vessel<V> Next;
    }

    public class HashList<V> : IEnumerable<Vessel<V>>
    {
        private int VesselSize = 64;
        public  int VesselFactor = (int)(64 * 0.83);
        public  int VesselTrim = 0;
        private int count = 0;
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
                if (value > VesselFactor)
                    rehash(VesselSize * 2);
                else if (value < VesselTrim)
                    rehash(VesselSize / 2);
            }
        }

        public Vessel<V> firstvessel;
        public Vessel<V> lastvessel;

        public Vessel<V>[] vessels;

        public HashList(int _vesselSize = 64)
        {
            VesselSize = _vesselSize;
            VesselFactor = (int)(_vesselSize * 0.83);
            vessels = new Vessel<V>[VesselSize];
            firstvessel = new Vessel<V>();
            lastvessel = firstvessel;            
        }

        public V this[long key]
        {
            get { return Get(key); }
            set { Put(key, value); }
        }
        public V this[IQuid key]
        {
            get { return Get(key); }
            set { Put(key, value); }
        }
        public unsafe V this[Object[] key]
        {
            get
            {
                fixed (byte* b = key.GetShah())
                    return Get(*(long*)b);
            }
            set
            {
                fixed (byte* b = key.GetShah())
                    Put(*(long*)b, value);
            }
        }
        public unsafe V this[Object key]
        {
            get
            {
                fixed (byte* b = key.GetShah())
                    return Get(*(long*)b);
            }
            set
            {
                fixed (byte* b = key.GetShah())
                    Put(*(long*)b, value);
            }
        }

        public unsafe bool ContainsKey(Object[] key, V value)
        {
            fixed (byte* b = key.GetShah())
                return ContainsKey(*(long*)b);
        }
        public unsafe bool ContainsKey(Object key, V value)
        {
            fixed (byte* b = key.GetShah())
                return ContainsKey(*(long*)b);
        }
        public bool ContainsKey(IQuid key)
        {
            long Long = key.LongValue;
            Vessel<V> mem = vessels[GetPosition(key, VesselSize)];

            while (mem != null)
            {
                if (!mem.Removed && mem.Key == Long)
                    return true;
                mem = mem.Extent;
            }
            return false;
        }
        public bool ContainsKey(long key)
        {
            Vessel<V> mem = vessels[GetPosition(key, VesselSize)];

            while (mem != null)
            {
                if (!mem.Removed && mem.Key == key)
                    return true;
                mem = mem.Extent;
            }
            return false;
        }
    
        public unsafe bool TryGet(Object[] key, out V value)
        {
            fixed (byte* b = key.GetShah())
                return TryGet(*(long*)b, out value);
        }
        public unsafe bool TryGet(Object key, out V value)
        {
            fixed (byte* b = key.GetShah())
                return TryGet(*(long*)b, out value);
        }
        public bool TryGet(IQuid key, out V entry)
        {
            long Long = key.LongValue;
            long pos = GetPosition(key, VesselSize);
            Vessel<V> mem = vessels[pos];

            while (mem != null)
            {
                if (!mem.Removed && mem.Key == Long)
                {
                    entry = mem.Value;
                    return true;
                }

                mem = mem.Extent;
            }

            entry = default(V);
            return false;
        }
        public bool TryGet(long key, out V entry)
        {
            Vessel<V> mem = vessels[GetPosition(key, VesselSize)];

            while (mem != null)
            {
                if (mem.Key == key && !mem.Removed)
                {
                    entry = mem.Value;
                    return true;
                }

                mem = mem.Extent;
            }

            entry = default(V);
            return false;
        }

        public unsafe V Get(Object[] key, V value)
        {
            fixed (byte* b = key.GetShah())
                return Get(*(long*)b);
        }
        public unsafe V Get(Object key, V value)
        {
            fixed (byte* b = key.GetShah())
                return Get(*(long*)b);
        }
        public V Get(IQuid key)
        {
            long Long = key.LongValue;
            Vessel<V> mem = vessels[GetPosition(Long, VesselSize)];

            while (mem != null)
            {
                if (!mem.Removed && mem.Key == key.LongValue)
                    return mem.Value;

                mem = mem.Extent;
            }

            return default(V);
        }
        public V Get(long key)
        {
            Vessel<V> mem = vessels[GetPosition(key, VesselSize)];

            while (mem != null)
            {
                if (mem.Key == key && !mem.Removed)
                    return mem.Value;

                mem = mem.Extent;
            }

            return default(V);
        }

        public Vessel<V> GetVessel(IQuid key)
        {
            return GetVessel(key.LongValue);
        }
        public Vessel<V> GetVessel(long key)
        {
            Vessel<V> mem = vessels[GetPosition(key, VesselSize)];

            while (mem != null)
            {
                if (mem.Key == key)
                {
                    if (!mem.Removed)
                        return mem;
                    return null;
                }
                mem = mem.Extent;
            }

            return null;
        }

        public bool TryGetVessel(long key, out Vessel<V> output)
        {
            Vessel<V> mem = vessels[GetPosition(key, VesselSize)];
           
            while (mem != null)
            {
                if (mem.Key == key)
                {
                    if (!mem.Removed)
                    {
                        output = mem;
                        return true;
                    }
                    output = null;
                    return false;
                }

                mem = mem.Extent;
            }

            output = null;
            return false;
        }

        public bool AddNew(long key, V value)
        {
            long pos = GetPosition(key, VesselSize);
            Vessel<V> mem = vessels[pos];

            if (mem == null)
            {
                vessels[pos] = lastvessel = lastvessel.Next = new Vessel<V>(key, value);
                Count++;
                return true;
            }

            for(; ; )
            {
                if (!mem.Removed)
                {
                    if (mem.Extent == null)
                    {
                        lastvessel = lastvessel.Next = mem.Extent = new Vessel<V>(key, value);
                        Count++;
                        return true;
                    }
                    mem = mem.Extent;
                }
                else
                {
                    mem.Set(key, value);
                    Count++;
                    return true;
                }
            }
        }

        public unsafe bool Add(Object[] key, V value)
        {
            fixed (byte* b = key.GetShah())
                return Add(*(long*)b, value);
        }
        public unsafe bool Add(Object key, V value)
        {
            fixed (byte* b = key.GetShah())
                return Add(*(long*)b, value);
        }
        public bool Add(IQuid key, V value)
        {
            long Long = key.LongValue;
            long pos = GetPosition(key, VesselSize);
            Vessel<V> mem = vessels[pos];

            if (mem == null)
            {
                vessels[pos] = lastvessel = lastvessel.Next = new Vessel<V>(Long, value);
                Count++;
                return true;
            }

            while (true)
            {
                if (!mem.Removed)
                {
                    if (mem.Key == Long)
                        return false;

                    if (mem.Extent == null)
                    {
                        lastvessel = lastvessel.Next = mem.Extent = new Vessel<V>(Long, value);
                        Count++;
                        return true;
                    }
                    mem = mem.Extent;
                }
                else
                {
                    mem.Set(Long, value);
                    Count++;
                    return true;
                }
            }
        }
        public bool Add(long key, V value)
        {
            long pos = GetPosition(key, VesselSize);
            Vessel<V> mem = vessels[pos];

            if (mem == null)
            {
                vessels[pos] = lastvessel = lastvessel.Next = new Vessel<V>(key, value);
                Count++;
                return true;
            }

            for(; ;)
            {
                if (!mem.Removed)
                {
                    if (mem.Key == key)
                        return false;

                    if (mem.Extent == null)
                    {
                        lastvessel = lastvessel.Next = mem.Extent = new Vessel<V>(key, value);
                        Count++;
                        return true;
                    }
                    mem = mem.Extent;
                }
                else
                {
                    mem.Set(key, value);
                    Count++;
                    return true;
                }
            }
        }
        public bool Add(Vessel<V> _vessel)
        {
            long _key = _vessel.Key;
            long pos = GetPosition(_key, VesselSize);
            Vessel<V> mem = vessels[pos];

            if (mem == null)
            {
                vessels[pos] = lastvessel = lastvessel.Next = _vessel;
                Count++;
                return true;
            }

            for (; ; )
            {
                if (!mem.Removed)
                {
                    if (mem.Key == _key)
                        return false;

                    if (mem.Extent == null)
                    {
                        lastvessel = lastvessel.Next = mem.Extent = _vessel;
                        Count++;
                        return true;
                    }
                    mem = mem.Extent;
                }
                else
                {
                    mem.Set(_vessel.Key, _vessel.Value);
                    Count++;
                    return true;
                }
            }
        }
        public void Add(IList<Vessel<V>> _vessel)
        {
            foreach (Vessel<V> v in _vessel)
                Add(v);
        }
        public void Add(Vessel<V>[] _vessel)
        {
            foreach (Vessel<V> v in _vessel)
                Add(v);
        }
        public void Add(Dictionary<long, V> _vessel)
        {
            foreach (KeyValuePair<long, V> i in _vessel)
            {
                long pos = GetPosition(i.Key, VesselSize);
                Vessel<V> mem = vessels[pos];

                if (mem == null)
                {
                    vessels[pos] = lastvessel = lastvessel.Next = new Vessel<V>(i.Key, i.Value);
                    Count++;
                }

                while (true)
                {
                    if (!mem.Removed)
                    {
                        if (mem.Key == i.Key)
                            break;

                        if (mem.Extent == null)
                        {
                            lastvessel = lastvessel.Next = mem.Extent = new Vessel<V>(i.Key, i.Value);
                            Count++;
                            break;
                        }
                        mem = mem.Extent;
                    }
                    else
                    {
                        mem.Set(i.Key, i.Value);
                        Count++;
                        break;
                    }
                }
            }
        }

        public unsafe bool TryRemove(Object[] key)
        {
            fixed (byte* b = key.GetShah())
                return TryRemove(*(long*)b);
        }
        public unsafe bool TryRemove(Object key)
        {
            fixed (byte* b = key.GetShah())
                return TryRemove(*(long*)b);
        }
        public unsafe bool TryRemove(IQuid key)
        {
            return TryRemove(key.LongValue);
        }
        public unsafe bool TryRemove(long key)
        {
            Vessel<V> mem = vessels[GetPosition(key, VesselSize)];

            while (mem != null)
            {
                if (mem.Key == key)
                {
                    mem.Removed = true;
                    Count--;
                    return true;
                }

                mem = mem.Extent;
            }

            return false;
        }

        public unsafe V Remove(Object[] key)
        {
            fixed (byte* b = key.GetShah())
                return Remove(*(long*)b);
        }
        public unsafe V Remove(Object key)
        {
            fixed (byte* b = key.GetShah())
                return Remove(*(long*)b);
        }
        public unsafe V Remove(IQuid key)
        {
            return Remove(key.LongValue);
        }
        public unsafe V Remove(long key)
        {
            Vessel<V> mem = vessels[GetPosition(key, VesselSize)];

            while (mem != null)
            {
                if (mem.Key == key)
                {
                    mem.Removed = true;
                    Count--;
                    return mem.Value;
                }

                mem = mem.Extent;
            }

            return default(V);       
        }

        public unsafe long GetPosition(IQuid key, int size)
        {
            long Long = key.LongValue;
            if (Long < 0)
                *(((byte*)&Long) + 7) &= 127;
            return (Long % size);
        }
        public unsafe long GetPosition(long key, long size)
        {
            if (key < 0)
                *(((byte*)&key) + 7) &= 127;
            return (key % size);
        }

        public unsafe bool Put(IQuid key, V value)
        {
            long Long = key.LongValue;
            long pos = GetPosition(key, VesselSize);
            Vessel<V> mem = vessels[pos];

            if (mem == null)
            {
                vessels[pos] = lastvessel = lastvessel.Next = new Vessel<V>(Long, value);
                Count++;
                return true;
            }

            for (; ; )
            {
                if (!mem.Removed)
                {
                    if (mem.Key == Long)
                    {
                        mem.Value = value;
                        return false;
                    }

                    if (mem.Extent == null)
                    {
                        lastvessel = lastvessel.Next = mem.Extent = new Vessel<V>(Long, value);
                        Count++;
                        return true;
                    }
                    mem = mem.Extent;
                }
                else
                {
                    mem.Set(Long, value);
                    Count++;
                    return true;
                }            
            }
        }
        public unsafe bool Put(Object[] key, V value)
        {
            fixed (byte* b = key.GetShah())
                return Put(*(long*)b, value);
        }
        public unsafe bool Put(Object key, V value)
        {
            fixed (byte* b = key.GetShah())
                return Put(*(long*)b, value);
        }
        public unsafe bool Put(long key, V value)
        {
            long pos = GetPosition(key, VesselSize);
            Vessel<V> mem = vessels[pos];

            if (mem == null)
            {
                vessels[pos] = lastvessel = lastvessel.Next = new Vessel<V>(key, value);
                Count++;
                return true;
            }

            for (; ; )
            {
                if (!mem.Removed)
                {
                    if (mem.Key == key)
                    {
                        mem.Value = value;
                        return false;
                    }

                    if (mem.Extent == null)
                    {
                        lastvessel = lastvessel.Next = mem.Extent = new Vessel<V>(key, value);
                        Count++;
                        return true;
                    }
                    mem = mem.Extent;
                }
                else
                {
                    mem.Set(key, value);
                    Count++;
                    return true;
                }
            }
        }

        public bool Put(Vessel<V> _vessel)
        {
            long _key = _vessel.Key;
            long pos = GetPosition(_key, VesselSize);
            Vessel<V> mem = vessels[pos];

            if (mem == null)
            {
                lastvessel.Next = _vessel;
                lastvessel = _vessel;
                vessels[pos] = _vessel;
                Count++;
                return true;
            }

            for (; ; )
            {
                if (!mem.Removed)
                {
                    if (mem.Key == _key)
                    {
                        mem.Value = _vessel.Value;
                        return false;
                    }

                    if (mem.Extent == null)
                    {
                        mem.Extent = _vessel;
                        lastvessel.Next = _vessel;
                        lastvessel = _vessel;
                        Count++;
                        return true;
                    }
                    mem = mem.Extent;
                }
                else
                {
                    mem.Set(_vessel.Key, _vessel.Value);
                    Count++;
                    return true;
                }               
            }
        }
        public void Put(IList<Vessel<V>> _vessel)
        {
            foreach (Vessel<V> v in _vessel)
                Put(v);
        }
        public void Put(Vessel<V>[] _vessel)
        {
            foreach (Vessel<V> v in _vessel)
                Put(v);
        }
        public void Put(Dictionary<long, V> _vessel)
        {
            foreach (KeyValuePair<long, V> i in _vessel)
            {
                long pos = GetPosition(i.Key, VesselSize);
                Vessel<V> mem = vessels[pos];

                if (mem == null)
                {
                    vessels[pos] = lastvessel = lastvessel.Next = new Vessel<V>(i.Key, i.Value);
                    Count++;
                }
                else
                    for (; ; )
                    {
                        if (!mem.Removed)
                        {
                            if (mem.Key == i.Key)
                            {
                                mem.Value = i.Value;
                                break;
                            }

                            if (mem.Extent == null)
                            {
                                lastvessel = lastvessel.Next = mem.Extent = new Vessel<V>(i.Key, i.Value);
                                Count++;
                                break;
                            }
                            mem = mem.Extent;
                        }
                        else
                        {
                            mem.Set(i.Key, i.Value);
                            Count++;
                            break;
                        }                     
                    }
            }
        }

        public void CopyTo(KeyValuePair<long, object>[] array, int index)
        {
            Vessel<V> vessel = firstvessel;
            int finish = Count;
            for (int i = 0; i < finish; i++)
            {
                vessel = vessel.Next;
                array[index + i] = new KeyValuePair<long, object>(vessel.Key, vessel.Value);
            }
        }
        public void CopyTo(KeyValuePair<long, V>[] array, int index)
        {
            Vessel<V> vessel = firstvessel;
            int finish = Count;
            for (int i = 0; i < finish; i++)
            {
                vessel = vessel.Next;
                array[index + i] = new KeyValuePair<long, V>(vessel.Key, vessel.Value);
            }
        }
        public void CopyTo(KeyValuePair<IQuid, V>[] array, int index)
        {
            Vessel<V> vessel = firstvessel;
            int finish = Count;
            for (int i = 0; i < finish; i++)
            {
                vessel = vessel.Next;
                array[index + i] = new KeyValuePair<IQuid, V>(new Quid(vessel.Key), vessel.Value);
            }
        }
        public void CopyTo(KeyValuePair<IQuid, object>[] array, int index)
        {
            int i = 0;
            foreach (Vessel<V> vessel in this)
            {            
                array[index + i++] = new KeyValuePair<IQuid, object>(new Quid(vessel.Key), vessel.Value);
            }
        }
        public void CopyTo(List<Vessel<V>> array)
        {
            foreach(Vessel<V> ves in this)
            {
                array.Add(ves);
            }
        }
        public void CopyTo(V[] array, int index)
        {
            int i = 0;
            foreach (Vessel<V> vessel in this)
            {
                array[index + i++] = vessel.Value;
            }
        }
        public void CopyTo(long[] array, int index)
        {
            int i = 0;
            foreach (Vessel<V> vessel in this)
            {
                array[index + i++] = vessel.Key;
            }
        }

        public Vessel<V> Next(Vessel<V> vessel)
        {
            Vessel<V> map = vessel.Next;
            if(map.Removed)
                return Next(map);
            return map;
        }

        public IEnumerator GetEnumerator()
        {
            return new HashListSeries<V>(this);
        }      

        public unsafe void Clear()
        {
            VesselSize = 64;
            VesselFactor = (int)(VesselSize * 0.83);
            VesselTrim = 0;
            vessels = new Vessel<V>[VesselSize];
            Count = 0;
        }

        private void rehash(int newsize)
        {
            int factor = (int)(newsize * 0.83);
            int trim = (newsize > 64) ? (factor / 3) : 0;
            int finish = Count;

            Vessel<V> _firstvessel = new Vessel<V>();
            Vessel<V> _lastvessel = _firstvessel;
            Vessel<V> vessel = firstvessel;
            Vessel<V>[] newvessels = new Vessel<V>[newsize];

            vessel = vessel.Next;
            do
            {
                if (!vessel.Removed)
                {
                    long pos = GetPosition(vessel.Key, newsize);

                    Vessel<V> mem = newvessels[pos];

                    if (mem == null)
                    {
                        vessel.Extent = null;
                        _lastvessel.Next = vessel;
                        _lastvessel = vessel;
                        newvessels[pos] = vessel;
                    }
                    else
                    {
                        for (; ; )
                        {
                            if (mem.Extent == null)
                            {
                                vessel.Extent = null;
                                mem.Extent = vessel;
                                _lastvessel.Next = vessel;
                                _lastvessel = vessel;
                                break;
                            }
                            else
                                mem = mem.Extent;
                        }
                    }
                }
                   
                vessel = vessel.Next;

            } while (vessel != null);

            firstvessel = _firstvessel;
            lastvessel = _lastvessel;
            vessels = newvessels;
            VesselFactor = factor;
            VesselSize = newsize;
            VesselTrim = trim;
        }

        public void Resize(int size)
        {
            rehash(size);
        }

        IEnumerator<Vessel<V>> IEnumerable<Vessel<V>>.GetEnumerator()
        {
            return new HashListSeries<V>(this);
        }
    }

    public class HashListSeries<V> : IEnumerator<Vessel<V>>
    {
        private HashList<V> map;
        private int size;

        public HashListSeries(HashList<V> Map)
        {
            map = Map;
            size = map.Count;            
            Entry = map.firstvessel;
        }

        public Vessel<V> Entry;

        public long Key { get { return Entry.Key; } }
        public V Value { get { return Entry.Value; } }

        public object Current => Entry;

        Vessel<V> IEnumerator<Vessel<V>>.Current => Entry;

        public bool MoveNext()
        {
            Entry = Entry.Next;
            if (Entry != null)
            {
                if (Entry.Removed)
                    return MoveNext();
                return true;
            }
            return false;
        }

        public void Reset()
        {
            Entry = map.firstvessel;
        }

        public void Dispose()
        {
            Entry = map.firstvessel;
        }
    }    
}
