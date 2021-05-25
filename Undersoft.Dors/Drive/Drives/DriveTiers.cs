using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Dors;

namespace System.Dors.Drive
{
    [SecurityPermission(SecurityAction.LinkDemand)]
    [SecurityPermission(SecurityAction.InheritanceDemand)]
    public unsafe class DriveTiers : DriveRecorder, IList<object>, IDrive
    {
        public int Length
        { get; private set; }

        public object this[int index]
        {
            get
            {
                object item;
                Read(out item, index, type);
                return item;
            }
            set
            {
                Write(ref value, index);
            }
        }
        public byte[] this[long index]
        {
            get
            {
                byte[] b = new byte[_elementSize];
                GCHandle handle = GCHandle.Alloc(b, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Read(ptr, _elementSize, index, type);
                handle.Free();
                return b;
            }
            set
            {
                GCHandle handle = GCHandle.Alloc(value, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Write(ptr, _elementSize, index);
            }
        }
        public object this[int index, int offset, Type t]
        {
            get
            {
                object item;
                Read(out item, index, t, 1000, offset);
                return item;
            }
            set
            {
                Write(ref value, index, t, 1000, offset);
            }
        }

        private int _elementSize;
        private Type type;

        public DriveTiers(string file, string name, int length, Type t) :
                          base(file, name, Marshal.SizeOf(t) * length, true, true)
        {
            type = t;
            Length = length;
            _elementSize = Marshal.SizeOf(t);

            Open();
        }                   // Creates or Open as Prime
        public DriveTiers(string file, string name, int length, int size) :
                         base(file, name, size * length, true, true)
        {
            type = typeof(byte[]);
            Length = length;
            _elementSize = size;

            Open();
        }                   // Creates or Open as Prime
        public DriveTiers(string file, string name, Type t) :
                          base(file, name, 0, false, true)
        {
            type = t;
            _elementSize = Marshal.SizeOf(t);

            Open();
        }                               // Open as User

        protected override bool DoOpen()
        {
            if (!IsOwnerOfSharedMemory)
            {
                if (BufferSize % _elementSize != 0)
                    throw new ArgumentOutOfRangeException("name", "BufferSize is not evenly divisible by the size of " + type.Name);

                Length = (int)(BufferSize / _elementSize);
            }
            return true;
        }

        #region Writing

        new public void Write(ref object data, long startIndex = 0, Type t = null, int timeout = 1000, int offset = 0)
        {
            if (startIndex > Length - 1 || startIndex < 0)
                throw new ArgumentOutOfRangeException("index");

            base.Write(ref data, (startIndex * _elementSize), t, timeout, offset);
        }
        new public void Write(object[] buffer, long startIndex = 0, Type t = null, int timeout = 1000)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (buffer.Length + startIndex > Length || startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex");

            base.Write(buffer, startIndex * _elementSize, type);
        }
        new public void Write(IntPtr ptr, long length, long startIndex = 0, Type t = null, int timeout = 1000)
        {
            base.Write(ptr, length, (startIndex * _elementSize), t, timeout);
        }
        new public void Write(byte* ptr, long length, long startIndex = 0, Type t = null, int timeout = 1000)
        {
            base.Write(ptr, length, (startIndex * _elementSize), t, timeout);
        }

        #endregion

        #region Reading

        new public void Read(out object data, long startIndex = 0, Type t = null, int timeout = 1000, int offset = 0)
        {
            if (startIndex > Length - 1 || startIndex < 0)
                throw new ArgumentOutOfRangeException("index");

            base.Read(out data, (startIndex * _elementSize), t, timeout, offset);
        }
        new public void Read(ref object[] buffer, long startIndex = 0, Type t = null, int timeout = 1000)
        {
            if (buffer == null)
            {
                if (Length - startIndex < 0 || startIndex < 0)
                    startIndex = 0;
                buffer = new object[Length - startIndex];
            }
            if (buffer.Length + startIndex > Length || startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex");

            base.Read(ref buffer, startIndex * _elementSize);
        }
        new public void Read(IntPtr destination, long length, long startIndex = 0, Type t = null, int timeout = 1000)
        {
            base.Read(destination, length, (startIndex * _elementSize), t, timeout);
        }
        new public void Read(byte* destination, long length, long startIndex = 0, Type t = null, int timeout = 1000)
        {
            base.Read(destination, length, (startIndex * _elementSize), t, timeout);
        }

        public void CopyTo(object[] buffer, int startIndex = 0)
        {
            if (buffer == null)
            {
                if (Length - startIndex < 0 || startIndex < 0)
                    startIndex = 0;
                buffer = new object[Length - startIndex];
            }
            if (buffer.Length + startIndex > Length || startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex");

            base.Read(ref buffer, startIndex * _elementSize);
        }

        public void CopyTo(IDrive destination, uint length, int startIndex = 0)
        {
            UnsafeNative.CopyMemory(destination.GetDrivePtr() + startIndex, this.GetDrivePtr(), length);
        }
        #endregion

        #region IEnumerable<T>

        public IEnumerator<object> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return this[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IList<object>

        public void Add(object item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object item)
        {
            return IndexOf(item) >= 0;
        }

        public bool Remove(object item)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return Length; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public int IndexOf(object item)
        {
            for (var i = 0; i < Count; i++)
            {
                if (this[i].Equals(item)) return i;
            }
            return -1;
        }

        public void Insert(int index, object item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
