using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace System.Doors
{
    [Serializable]
    public unsafe class NoidEqualer : IEqualityComparer
    {
        private byte pos = 0;

        public NoidEqualer(int position)
        {
            pos = (byte)(position * 4);
        }

        public new bool Equals(object x, object y)
        {
            return ((IQuid)x).Int32Equals((IQuid)y, pos);
        }
        public int GetHashCode(object obj)
        {
            unchecked
            {
                return ((IQuid)obj).GetIntValue(pos);
            }
        }

        public long GetHashCode64(object obj)
        {
            unchecked
            {
                return ((IQuid)obj).LongValue;
            }
        }
    }

    [Serializable]
    public unsafe class NoidComparer : IEqualityComparer<byte[]>
    {
        private byte pos = 0;

        public NoidComparer(int position)
        {
            pos = (byte)(position * 4);
        }

        public bool Equals(byte[] x, byte[] y)
        {
            fixed (byte* xbyte = &x[pos])
            {
                fixed (byte* ybyte = &y[pos])
                {
                    return *((int*)xbyte) == *((int*)ybyte);
                }
            }
        }
        public int GetHashCode(byte[] obj)
        {
            unchecked
            {
                fixed (byte* pbyte = &obj[pos])
                    return *((int*)pbyte);
            }
        }
    }

    [Serializable]
    public class NoixComparer : IEqualityComparer<int>
    {
     
        public bool Equals(int x, int y)
        {
            return x == y;            
        }
        public int GetHashCode(int obj)
        {
            return obj;
        }
    }

    [Serializable]
    public class IEnumerableGrouper<T> : IEquatable<IEnumerable<T>>
    {
        public IEnumerable<T> x { get; set; }

        public bool Equals(IEnumerable<T> other)
        {

            return ReferenceEquals(x, other) || (other != null && x.SequenceEqual(other));
        }

        public override int GetHashCode()
        {
            if (x == null)
                return 0;
            unchecked
            {
                return x.Select(e => e.GetHashCode()).Aggregate((a, b) => a + b);
            }
        }

    }

    [Serializable]
    public class IEnumerableComparer<T> : IEqualityComparer<IEnumerable<T>>
    {
        public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
        {
            return ReferenceEquals(x, y) || (x != null && y != null && x.SequenceEqual(y));
        }
        public int GetHashCode(IEnumerable<T> obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                return obj.Select(e => e.GetHashCode()).Aggregate((a, b) => a + b);
            }
        }
    }
  

    [Serializable]
    public class HashSortComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            return x - y;
        }

        public int GetHashCode(int obj)
        {
            return obj;
        }
    }

    [Serializable]
    public class LinearSortComparer : IComparer<IDataNative>
    {
        int[] index;

        public LinearSortComparer(int[] _index)
        {
            index = _index;
        }

        public int Compare(IDataNative x, IDataNative y)
        {
            return index.Aggregate((a,b) =>  (x[a].GetHashCode() - y[a].GetHashCode()) + 
                                             (x[b].GetHashCode() - y[b].GetHashCode()));
        }

        public int GetHashCode(IDataNative obj)
        {
            unchecked
            {
                return index.Aggregate(17, (a,b) => 23 * (obj[a].GetHashCode() + obj[b].GetHashCode()));
            }
        }
    }

    [Serializable]
    public unsafe class NoidSortComparer : IComparer<IDataNative>
    {
        int noidOrdinal;
        int offset;
        public NoidSortComparer(int _noidOrdinal, int _offset = 0)
        {
            noidOrdinal = _noidOrdinal;
            offset = _offset;
        }

        public int Compare(IDataNative x, IDataNative y)
        {
            fixed (byte* xbyte = &((byte[])x[noidOrdinal])[0])
            {
                fixed (byte* ybyte = &((byte[])y[noidOrdinal])[0])
                {
                    return *((int*)xbyte) - *((int*)ybyte);
                }
            }
        }

        public int GetHashCode(IDataNative obj)
        {
            unchecked
            {
                fixed (byte* ybyte = &((byte[])obj[noidOrdinal])[0])
                {
                    return *((int*)ybyte);
                }
            }
        }
    }

    [Serializable]
    public class IEnumerableAPIComparer<T> : IEqualityComparer<IEnumerable<T>>
    {
        public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
        {
            return ReferenceEquals(x, y) || (x != null && y != null && x.SequenceEqual(y));
        }

        public int GetHashCode(IEnumerable<T> obj)
        {
            if (obj == null)
                return 0;

            // Zapobiega Overflow Exception 
            unchecked
            {
                return obj.Select(e => e.GetHashCode()).Aggregate(17, (a, b) => 23 * a + b);
            }
        }
    }

    [Serializable]
    public class IEnumerableParallelComparer<T> : IEqualityComparer<ParallelQuery<IEnumerable<T>>>
    {
        public bool Equals(ParallelQuery<IEnumerable<T>> x, ParallelQuery<IEnumerable<T>> y)
        {
            return ReferenceEquals(x, y) || (x != null && y != null && x.SequenceEqual(y));
        }
        public int GetHashCode(ParallelQuery<IEnumerable<T>> obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                return obj.Select(e => e.GetHashCode()).Aggregate(17, (a, b) => 23 * (a + b));
            }
        }
    }

    [Serializable]
    public class OneArrayComparer<T> : IEqualityComparer<T[]>
    {
        public bool Equals(T[] x, T[] y)
        {
            return ReferenceEquals(x, y) || (x != null && y != null && x.SequenceEqual(y) || x[0].Equals(y[0]));
        }
        public int GetHashCode(T[] obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                //return obj.GetHashCode();
                return obj.Select(e => e.GetHashCode()).Aggregate(17, (a, b) => (a + b)); ;
            }
        }
        public int GetHashCode(T obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                //return obj.GetHashCode();
                return obj.GetHashCode() ;
            }
        }
    }   

    [Serializable]
    public class HashComparer : IEqualityComparer<int>
    {       
        public bool Equals(int x, int y)
        {
            return x == y;
        }

        public int GetHashCode(int obj)
        {
            return obj;
        }
    }

    [Serializable]
    public class ParellelHashComparer : IEqualityComparer<ParallelQuery<IEnumerable<int>>>
    {
        public bool Equals(ParallelQuery<IEnumerable<int>> x, ParallelQuery<IEnumerable<int>> y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(ParallelQuery<IEnumerable<int>> obj)
        {
            unchecked
            {
                return obj.GetHashCode();
            }
        }
    }


    [Serializable]
    public unsafe class HashComparer64 : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            fixed (byte* xbyte = &x[0])
            {
                fixed (byte* ybyte = &y[0])
                {
                    return *((long*)xbyte) == *((long*)ybyte);
                }
            }
        }

        public int GetHashCode(byte[] obj)
        {
            unchecked
            {
                fixed (byte* xbyte = &obj[0])
                {
                    fixed (byte* ybyte = &obj[4])
                    {
                        return new int[] { *((int*)xbyte), *((int*)ybyte) }.Aggregate(17, (a, b) => (a + b) * 23);
                    }
                }
            }
        }
    }

    [Serializable]
    public unsafe class HashComparer32 : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            fixed (byte* xbyte = &x[0])
            {
                fixed (byte* ybyte = &y[0])
                {
                    return *((int*)xbyte) == *((int*)ybyte);
                }
            }
        }

        public int GetHashCode(byte[] obj)
        {
            unchecked
            {
                fixed (byte* ybyte = &obj[0])
                {
                    return *((int*)ybyte);
                }
            }
        }
    }

    [Serializable]
    public class HAArrayComparer : IEqualityComparer<byte>
    {
        public bool Equals(byte x, byte y)
        {           
            return x == y;
        }

        public int GetHashCode(byte obj)
        {
            unchecked
            {
                return obj;
            }
        }
    }
    [Serializable]
    public class HAArraySortComparer : IComparer<byte>
    {
        public int Compare(byte x, byte y)
        {           
            return x - y;
        }

        public int GetHashCode(byte obj)
        {
            unchecked
            {
                return obj;
            }
        }
    }


    [Serializable]
    public unsafe class LinearHashComparer : IEqualityComparer<IDataNative>
    {
        int noidOrdinal;
        int offset;
        public LinearHashComparer(int _noidOrdinal, int _offset = 0)
        {
            noidOrdinal = _noidOrdinal;
            offset = _offset;
        }

        public bool Equals(IDataNative x, IDataNative y)
        {
            fixed (byte* xbyte = &((byte[])x[noidOrdinal])[0])
            {
                fixed (byte* ybyte = &((byte[])y[noidOrdinal])[0])
                {
                    return *((int*)xbyte) == *((int*)ybyte);
                }
            }
        }

        public int GetHashCode(IDataNative obj)
        {
            unchecked
            {
                fixed (byte* ybyte = &((byte[])obj[noidOrdinal])[0])
                {
                    return *((int*)ybyte);
                }
            }
        }
    }
    [Serializable]
    public class IndexHashComparer : IEqualityComparer<IDataNative>
    {
        int noidOrdinal;
        int offset;
        public IndexHashComparer(int _noidOrdinal)
        {
            noidOrdinal = _noidOrdinal;
        }

        public bool Equals(IDataNative x, IDataNative y)
        {
            return (0 != x[noidOrdinal].GetHashCode() - y[noidOrdinal].GetHashCode());
        }

        public int GetHashCode(IDataNative obj)
        {
            unchecked
            {
                return obj[noidOrdinal].GetHashCode();
            }
        }
    }

    [Serializable]
    public class Hash64Comparer : IEqualityComparer<long>
    {
        public bool Equals(long x, long y)
        {
            return x == y;
        }

        public unsafe int GetHashCode(long obj)
        {
            unchecked
            {
                byte* pkey = ((byte*)&obj);
                return (((17 + *(int*)(pkey + 4)) * 23) + *(int*)(pkey)) * 23;
            }
        }
    }
 
    [Serializable]
    public class IndexArrayComparer<T> : IEqualityComparer<T[]>
    {
        public bool Equals(T[] x, T[] y)
        {
            int l = x.Length - 1;
            return (x != null && y != null && x.Take(l).SequenceEqual(y.Take(l)));
        }

        public int GetHashCode(T[] obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                //return obj.GetHashCode();
                return obj.Take(obj.Length - 1).Select(e => e.GetHashCode()).Aggregate(17, (a, b) => (a + b)); ;
            }
        }
        public int GetHashCode(T obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                //return obj.GetHashCode();
                return obj.GetHashCode();
            }
        }
    }
  
    [Serializable]
    public class NArrayComparer<T> : IEqualityComparer<T[]>
    {
        public bool Equals(T[] x, T[] y)
        {
            return ReferenceEquals(x, y) || (x != null && y != null && x.SequenceEqual(y));
        }
        public int GetHashCode(T[] obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                //return obj.GetHashCode();
                return obj.Select(e => e.GetHashCode()).Aggregate(17, (a, b) => (a + b)); ;
            }
        }
    }

    [Serializable]
    public class IArrayComparer<T> : IComparer<T[]>
    {
        public int Compare(T[] x, T[] y)
        {
            return x.Select(e => e.GetHashCode()).Aggregate(17, (a, b) => (a + b)) - y.Select(e => e.GetHashCode()).Aggregate(17, (a, b) => (a + b));
        }
        public int GetHashCode(T[] obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                //return obj.GetHashCode();
                return obj.Select(e => e.GetHashCode()).Aggregate(17, (a, b) => (a + b)); ;
            }
        }
    }

    [Serializable]
    public class IntArrayComparer : IEqualityComparer<int[]>
    {
        public bool Equals(int[] x, int[] y)
        {
            return x.SequenceEqual(y);
        }
        public int GetHashCode(int[] obj)
        {
            unchecked
            {
                return obj.Select(o => o).Aggregate(17, (a, b) => (a + b) * 23); ;
            }
        }
    }

    [Serializable]
    public class NArrayParallelComparer<T> : IEqualityComparer<ParallelQuery<T[]>>
    {
        public bool Equals(ParallelQuery<T[]> x, ParallelQuery<T[]> y)
        {
            return (x != null && y != null && x.SequenceEqual(y));
        }
        public int GetHashCode(ParallelQuery<T[]> obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                //return obj.GetHashCode();
                return obj.Select(e => e.GetHashCode()).Aggregate(17, (a, b) => a + b);
            }
        }
    }

    public class NFalseArrayParallelComparer<T> : IEqualityComparer<ParallelQuery<T[]>>
    {
        public bool Equals(ParallelQuery<T[]> x, ParallelQuery<T[]> y)
        {
              return false;
        }
        public int GetHashCode(ParallelQuery<T[]> obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                return obj.Select(e => e.GetHashCode()).Aggregate(17, (a, b) => a + b);
            }
        }
    }

    public class NFalseArrayComparer<T> : IEqualityComparer<T[]>
    {
        public bool Equals(T[] x, T[] y)
        {
            return false;
        }
        public int GetHashCode(T[] obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                return obj.Select(e => e.GetHashCode()).Aggregate(17, (a, b) => a + b);
            }
        }
    }

    [Serializable]
    public class NSiftComparer<T> : IEqualityComparer<T[]>
    {
        public bool Equals(T[] x, T[] y)
        {
            if (x != null && y != null)
            {
                int allGood = y.Length;
                foreach (T ix in x)
                {
                    foreach (T iy in y)
                        if (ix.Equals(y))
                        {
                            allGood--;
                            if (allGood <= 0)
                                return true;
                        }
                }
            }
            return false;
        }
        public int GetHashCode(T[] obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                //return obj.GetHashCode();
                return obj.Select(e => e.GetHashCode()).Aggregate(17,(a, b) => a + b);
            }
  
        }
    }
}
