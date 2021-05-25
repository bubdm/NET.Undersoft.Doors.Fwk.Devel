using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Dors
{
    public class MemoryAlloc : IDisposable
    {
        private static Dictionary<Type, int> _sizeCache = new Dictionary<Type, int>();
#if SIZE_CACHE_USE_RESOURCE_LOCK
        private static FastResourceLock _sizeCacheLock = new FastResourceLock();
#endif

        private static int _allocatedCount = 0;
        private static int _freedCount = 0;
        private static int _reallocatedCount = 0;

        private IntPtr _memory;
        private int _size;

        // A private heap just for the client.
        private static Heap _privateHeap = new Heap(HeapFlags.Class1 | HeapFlags.Growable);
        private static Heap _processHeap = Heap.GetDefault();

        public static int AllocatedCount
        {
            get { return _allocatedCount; }
        }

        public static int FreedCount
        {
            get { return _freedCount; }
        }

        public static Heap PrivateHeap
        {
            get { return _privateHeap; }
        }

        public static int ReallocatedCount
        {
            get { return _reallocatedCount; }
        }

        /// <summary>
        /// Creates a new memory allocation with the specified size.
        /// </summary>
        /// <param name="size">The amount of memory, in bytes, to allocate.</param>
        public MemoryAlloc(int size)
            : this(size, 0)
        { }

        /// <summary>
        /// Creates a new memory allocation with the specified size.
        /// </summary>
        /// <param name="size">The amount of memory, in bytes, to allocate.</param>
        /// <param name="flags">Any flags to use.</param>
        public MemoryAlloc(int size, HeapFlags flags)
        {
            this._memory = _privateHeap.Allocate(flags, size);
            this._size = size;

#if ENABLE_STATISTICS
            System.Threading.Interlocked.Increment(ref _allocatedCount);
#endif
        }

        public MemoryAlloc(IntPtr ptr)
        {
            _privateHeap = Heap.FromHandle(ptr);
            this._memory = _privateHeap.Address;
            this._size = _privateHeap.GetBlockSize(0, _memory);

#if ENABLE_STATISTICS
            System.Threading.Interlocked.Increment(ref _allocatedCount);
#endif
        }

        protected void Free()
        {
            _privateHeap.Free(0, this.Memory);

#if ENABLE_STATISTICS
            System.Threading.Interlocked.Increment(ref _freedCount);
#endif
        }

        /// <summary>
        /// Resizes the memory allocation.
        /// </summary>
        /// <param name="newSize">The new size of the allocation.</param>
        public virtual void Resize(int newSize)
        {
            this._memory = _privateHeap.Reallocate(0, this._memory, newSize);
            this._size = newSize;

#if ENABLE_STATISTICS
            System.Threading.Interlocked.Increment(ref _reallocatedCount);
#endif
        }

        /// <summary>
        /// Resizes the memory allocation without retaining the contents 
        /// of the allocated memory.
        /// </summary>
        /// <param name="newSize">The new size of the allocation.</param>
        public virtual void ResizeNew(int newSize)
        {
            _privateHeap.Free(0, this._memory);
            this._memory = _privateHeap.Allocate(0, newSize);
            this._size = newSize;
        }

        public virtual int Size
        {
            get { return _size; }
            protected set { _size = value; }
        }

        public IntPtr Memory
        {
            get { return _memory; }
            protected set { _memory = value; }
        }

        private static int GetStructSize(Type structType)
        {
            int size;

#if SIZE_CACHE_USE_RESOURCE_LOCK
            _sizeCacheLock.AcquireShared();

            if (_sizeCache.ContainsKey(structType))
            {
                size = _sizeCache[structType];
                _sizeCacheLock.ReleaseShared();
            }
            else
            {
                _sizeCacheLock.ReleaseShared();

                size = Marshal.SizeOf(structType);
                _sizeCacheLock.AcquireExclusive();

                try
                {
                    if (!_sizeCache.ContainsKey(structType))
                        _sizeCache.Add(structType, size);
                }
                finally
                {
                    _sizeCacheLock.ReleaseExclusive();
                }
            }

            return size;
#else
            lock (_sizeCache)
            {
                if (_sizeCache.ContainsKey(structType))
                    size = _sizeCache[structType];
                else
                    _sizeCache.Add(structType, size = Marshal.SizeOf(structType));

                return size;
            }
#endif
        }

        public static T ReadStruct<T>(IntPtr ptr)
        {
            return (T)Marshal.PtrToStructure(ptr, typeof(T));
        }

        public void DestroyStruct<T>()
        {
            this.DestroyStruct<T>(0);
        }

        public void DestroyStruct<T>(int index)
        {
            this.DestroyStruct<T>(0, index);
        }

        public void DestroyStruct<T>(int offset, int index)
        {
            if (index == 0)
            {
                Marshal.DestroyStructure(_memory.Increment(offset), typeof(T));
            }
            else
            {
                Marshal.DestroyStructure(
                    _memory.Increment(offset + GetStructSize(typeof(T)) * index),
                    typeof(T)
                    );
            }
        }

        public void Fill(int offset, int length, byte value)
        {
            Win32.RtlFillMemory(
                _memory.Increment(offset),
                length.ToIntPtr(),
                value
                );
        }

        public string ReadAnsiString(int offset)
        {
            return Marshal.PtrToStringAnsi(_memory.Increment(offset));
        }

        public string ReadAnsiString(int offset, int length)
        {
            return Marshal.PtrToStringAnsi(_memory.Increment(offset), length);
        }

        public byte[] ReadBytes(int length)
        {
            return this.ReadBytes(0, length);
        }

        public byte[] ReadBytes(int offset, int length)
        {
            byte[] buffer = new byte[length];

            this.ReadBytes(offset, buffer, 0, length);

            return buffer;
        }

        public void ReadBytes(byte[] buffer, int startIndex, int length)
        {
            this.ReadBytes(0, buffer, startIndex, length);
        }

        public void ReadBytes(int offset, byte[] buffer, int startIndex, int length)
        {
            Marshal.Copy(_memory.Increment(offset), buffer, startIndex, length);
        }

        /// <summary>
        /// Reads a signed integer.
        /// </summary>
        /// <param name="offset">The offset at which to begin reading.</param>
        /// <returns>The integer.</returns>
        public int ReadInt32(int offset)
        {
            return this.ReadInt32(offset, 0);
        }

        /// <summary>
        /// Reads a signed integer.
        /// </summary>
        /// <param name="offset">The offset at which to begin reading.</param>
        /// <param name="index">The index at which to begin reading, after the offset is added.</param>
        /// <returns>The integer.</returns>
        public int ReadInt32(int offset, int index)
        {
            unsafe
            {
                return ((int*)((byte*)_memory + offset))[index];
            }
        }

        public int[] ReadInt32Array(int offset, int count)
        {
            int[] array = new int[count];

            Marshal.Copy(_memory.Increment(offset), array, 0, count);

            return array;
        }

        public IntPtr ReadIntPtr(int offset)
        {
            return this.ReadIntPtr(offset, 0);
        }

        public IntPtr ReadIntPtr(int offset, int index)
        {
            unsafe
            {
                return ((IntPtr*)((byte*)_memory + offset))[index];
            }
        }

        public void ReadMemory(IntPtr buffer, int destOffset, int srcOffset, int length)
        {
            Win32.RtlMoveMemory(
                buffer.Increment(destOffset),
                _memory.Increment(srcOffset),
                length.ToIntPtr()
                );
        }

        /// <summary>
        /// Reads an unsigned integer.
        /// </summary>
        /// <param name="offset">The offset at which to begin reading.</param>
        /// <returns>The integer.</returns>
        public uint ReadUInt32(int offset)
        {
            return this.ReadUInt32(offset, 0);
        }

        /// <summary>
        /// Reads an unsigned integer.
        /// </summary>
        /// <param name="offset">The offset at which to begin reading.</param>
        /// <param name="index">The index at which to begin reading, after the offset is added.</param>
        /// <returns>The integer.</returns>
        public uint ReadUInt32(int offset, int index)
        {
            unsafe
            {
                return ((uint*)((byte*)_memory + offset))[index];
            }
        }

        /// <summary>
        /// Creates a struct from the memory allocation.
        /// </summary>
        /// <typeparam name="T">The type of the struct.</typeparam>
        /// <returns>The new struct.</returns>
        public T ReadStruct<T>()
            where T : struct
        {
            return this.ReadStruct<T>(0);
        }

        /// <summary>
        /// Creates a struct from the memory allocation.
        /// </summary>
        /// <typeparam name="T">The type of the struct.</typeparam>
        /// <param name="index">The index at which to begin reading to the struct. This is multiplied by  
        /// the size of the struct.</param>
        /// <returns>The new struct.</returns>
        public T ReadStruct<T>(int index)
            where T : struct
        {
            return this.ReadStruct<T>(0, index);
        }

        /// <summary>
        /// Creates a struct from the memory allocation.
        /// </summary>
        /// <typeparam name="T">The type of the struct.</typeparam>
        /// <param name="offset">The offset to add before reading.</param>
        /// <param name="index">The index at which to begin reading to the struct. This is multiplied by  
        /// the size of the struct.</param>
        /// <returns>The new struct.</returns>
        public T ReadStruct<T>(int offset, int index)
            where T : struct
        {
            if (index == 0)
            {
                return (T)Marshal.PtrToStructure(_memory.Increment(offset), typeof(T));
            }
            else
            {
                return (T)Marshal.PtrToStructure(
                    _memory.Increment(offset + GetStructSize(typeof(T)) * index),
                    typeof(T)
                    );
            }
        }

        public string ReadUnicodeString(int offset)
        {
            return Marshal.PtrToStringUni(_memory.Increment(offset));
        }

        public string ReadUnicodeString(int offset, int length)
        {
            return Marshal.PtrToStringUni(_memory.Increment(offset), length);
        }

        /// <summary>
        /// Writes a single byte to the memory allocation.
        /// </summary>
        /// <param name="offset">The offset at which to write.</param>
        /// <param name="b">The value of the byte.</param>
        public void WriteByte(int offset, byte b)
        {
            unsafe
            {
                *((byte*)_memory + offset) = b;
            }
        }

        public void WriteBytes(int offset, byte[] b)
        {
            Marshal.Copy(b, 0, _memory.Increment(offset), b.Length);
        }

        public void WriteInt16(int offset, short i)
        {
            unsafe
            {
                *(short*)((byte*)_memory + offset) = i;
            }
        }

        public void WriteInt32(int offset, int i)
        {
            unsafe
            {
                *(int*)((byte*)_memory + offset) = i;
            }
        }

        public void WriteIntPtr(int offset, IntPtr i)
        {
            unsafe
            {
                *(IntPtr*)((byte*)_memory + offset) = i;
            }
        }

        public void WriteMemory(int offset, IntPtr buffer, int length)
        {
            Win32.RtlMoveMemory(
                _memory.Increment(offset),
                buffer,
                length.ToIntPtr()
                );
        }

        public void WriteStruct<T>(T s)
            where T : struct
        {
            this.WriteStruct<T>(0, s);
        }

        public void WriteStruct<T>(int index, T s)
            where T : struct
        {
            this.WriteStruct<T>(0, index, s);
        }

        public void WriteStruct<T>(int offset, int index, T s)
            where T : struct
        {
            if (index == 0)
            {
                Marshal.StructureToPtr(s, _memory.Increment(offset), false);
            }
            else
            {
                Marshal.StructureToPtr(
                    s,
                    _memory.Increment(offset + GetStructSize(typeof(T)) * index),
                    false
                    );
            }
        }

        /// <summary>
        /// Writes a Unicode string (without a null terminator) to the allocated memory.
        /// </summary>
        /// <param name="offset">The offset to add.</param>
        /// <param name="s">The string to write.</param>
        public void WriteUnicodeString(int offset, string s)
        {
            unsafe
            {
                fixed (char* ptr = s)
                {
                    this.WriteMemory(offset, (IntPtr)ptr, s.Length * 2);
                }
            }
        }

        public void Zero(int offset, int length)
        {
            Win32.RtlZeroMemory(
                _memory.Increment(offset),
                length.ToIntPtr()
                );
        }


        public void Dispose()
        {
            this.Free();
        }
    }
}
