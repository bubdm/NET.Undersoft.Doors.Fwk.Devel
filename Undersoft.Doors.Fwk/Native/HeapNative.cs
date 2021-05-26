using System;
using System.Runtime.InteropServices;

namespace System.Doors
{
    public struct Heap
    {
        public static Heap FromHandle(IntPtr handle)
        {
            return new Heap(handle);
        }

        public static Heap GetDefault()
        {
            return new Heap(Win32.GetProcessHeap());
        }

        public static Heap[] GetHeaps()
        {
            IntPtr[] heapAddresses = new IntPtr[64];
            int retHeaps;

            retHeaps = Win32.RtlGetProcessHeaps(heapAddresses.Length, heapAddresses);

            // Reallocate the buffer if it wasn't large enough.
            if (retHeaps > heapAddresses.Length)
            {
                heapAddresses = new IntPtr[retHeaps];
                retHeaps = Win32.RtlGetProcessHeaps(heapAddresses.Length, heapAddresses);
            }

            int numberOfHeaps = Math.Min(heapAddresses.Length, retHeaps);
            Heap[] heaps = new Heap[numberOfHeaps];

            for (int i = 0; i < numberOfHeaps; i++)
                heaps[i] = new Heap(heapAddresses[i]);

            return heaps;
        }

        private IntPtr _heap;

        private Heap(IntPtr heap)
        {
            _heap = heap;
        }

        public Heap(HeapFlags flags)
            : this(flags, 0, 0)
        { }

        public Heap(HeapFlags flags, int reserveSize, int commitSize)
        {
            _heap = Win32.RtlCreateHeap(
                flags,
                IntPtr.Zero,
                reserveSize.ToIntPtr(),
                commitSize.ToIntPtr(),
                IntPtr.Zero,
                IntPtr.Zero
                );

            if (_heap == IntPtr.Zero)
                throw new OutOfMemoryException();
        }

        public IntPtr Address
        {
            get { return _heap; }
        }

        public IntPtr Allocate(HeapFlags flags, int size)
        {
            IntPtr memory = Win32.RtlAllocateHeap(_heap, flags, size.ToIntPtr());

            if (memory == IntPtr.Zero)
                throw new OutOfMemoryException();

            return memory;
        }

        public int Compact(HeapFlags flags)
        {
            return Win32.RtlCompactHeap(_heap, flags).ToInt32();
        }

        public void Destroy()
        {
            Win32.RtlDestroyHeap(_heap);
        }

        public void Free(HeapFlags flags, IntPtr memory)
        {
            Win32.RtlFreeHeap(_heap, flags, memory);
        }

        public int GetBlockSize(HeapFlags flags, IntPtr memory)
        {
            return Win32.RtlSizeHeap(_heap, flags, memory).ToInt32();
        }

        public IntPtr Reallocate(HeapFlags flags, IntPtr memory, int size)
        {
            IntPtr newMemory = Win32.RtlReAllocateHeap(_heap, flags, memory, size.ToIntPtr());

            if (newMemory == IntPtr.Zero)
                throw new OutOfMemoryException();

            return newMemory;
        }
    } 

    [Flags]
    public enum HeapFlags : uint
    {
        NoSerialize = 0x00000001,
        Growable = 0x00000002,
        GenerateExceptions = 0x00000004,
        ZeroMemory = 0x00000008,
        ReallocInPlaceOnly = 0x00000010,
        TailCheckingEnabled = 0x00000020,
        FreeCheckingEnabled = 0x00000040,
        DisableCoalesceOnFree = 0x00000080,

        CreateAlign16 = 0x00010000,
        CreateEnableTracing = 0x00020000,
        CreateEnableExecute = 0x00040000,

        SettableUserValue = 0x00000100,
        SettableUserFlag1 = 0x00000200,
        SettableUserFlag2 = 0x00000400,
        SettableUserFlag3 = 0x00000800,
        SettableUserFlags = 0x00000e00,

        Class0 = 0x00000000, // Process heap
        Class1 = 0x00001000, // Private heap
        Class2 = 0x00002000, // Kernel heap
        Class3 = 0x00003000, // GDI heap
        Class4 = 0x00004000, // User heap
        Class5 = 0x00005000, // Console heap
        Class6 = 0x00006000, // User desktop heap
        Class7 = 0x00007000, // CSRSS shared heap
        Class8 = 0x00008000, // CSR port heap
        ClassMask = 0x0000f000
    }
}
