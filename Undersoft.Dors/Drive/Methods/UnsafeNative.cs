using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Dors;
using Microsoft.Win32.SafeHandles;

namespace System.Dors.Drive
{
    [System.Security.SuppressUnmanagedCodeSecurity]
    public class UnsafeNative
    {
        public UnsafeNative() { }
       
        public static unsafe void CopyMemory(byte[] dest, byte[] src, uint count)
        {
            MemoryCopier.Copy(src, dest, 0, count);
            //byte* destptr = dest;
            //byte* srcptr = src;
            //uint lenght64 = count / 8;
            //uint lenght8 = count % 8;
            //for (uint i = 0; i < lenght64; i++)
            //{
            //    *((long*)destptr) = *((long*)srcptr);
            //    destptr += 8;
            //    srcptr += 8;
            //}
            //for (uint i = 0; i < lenght8; i++)
            //{
            //    *destptr = *srcptr;
            //    destptr++;
            //    srcptr++;
            //}
        }
        public static unsafe void CopyMemory(IntPtr dest, IntPtr src, uint count)
        {
            MemoryCopier.Copy((byte*)src.ToPointer(), (byte*)dest.ToPointer(), 0, count);           
        }
        public static unsafe void CopyMemory(byte* dest, byte* src, uint count)
        {
            MemoryCopier.Copy(src, dest, 0, count);
        }
        public static unsafe void CopyMemory(void* dest, void* src, uint count)
        {
            MemoryCopier.Copy((byte*)src, (byte*)dest, 0, count);
        }

        //public static unsafe byte[] StructureToBin(object structure)
        //{
            
        //        int count = Marshal.SizeOf(structure);
        //        byte[] barray = new byte[count];
        //        fixed (byte* dest = &barray[0])
        //        {
        //            int lenght64 = count / 8;
        //            int lenght8 = count % 8;
        //            byte* destptr = dest;
                
        //                GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
        //                byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());

        //                for (uint i = 0; i < lenght64; i++)
        //                {
        //                    *((long*)destptr) = *((long*)srcptr);
        //                    destptr += 8;
        //                    srcptr += 8;
        //                }
        //                for (uint i = 0; i < lenght8; i++)
        //                {
        //                    *destptr = *srcptr;
        //                    destptr++;
        //                    srcptr++;
        //                }
        //                handle.Free();                  
        //        }
        //        return barray;          
        //}
        //public static unsafe byte[] StructureToBin(object[] structures)
        //{
        //    if (structures.Length > 0)
        //    {
        //        int count = Marshal.SizeOf(structures[0]);
        //        byte[] barray = new byte[count * structures.Length];
        //        fixed (byte* dest = &barray[0])
        //        {
        //            int lenght64 = count / 8;
        //            int lenght8 = count % 8;
        //            byte* destptr = dest;
        //            foreach (object structure in structures)
        //            {
        //                GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
        //                byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());

        //                for (uint i = 0; i < lenght64; i++)
        //                {
        //                    *((long*)destptr) = *((long*)srcptr);
        //                    destptr += 8;
        //                    srcptr += 8;
        //                }
        //                for (uint i = 0; i < lenght8; i++)
        //                {
        //                    *destptr = *srcptr;
        //                    destptr++;
        //                    srcptr++;
        //                }
        //                handle.Free();
        //            }
        //        }
        //        return barray;
        //    }
        //    return null;
        //}

        //public static unsafe void StructureToBin(object structure, ref byte[] barray, int offset = 0)
        //{

        //    int count = Marshal.SizeOf(structure);           
        //    fixed (byte* dest = &barray[offset])
        //    {
        //        int lenght64 = count / 8;
        //        int lenght8 = count % 8;
        //        byte* destptr = dest;

        //        GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
        //        byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());

        //        for (uint i = 0; i < lenght64; i++)
        //        {
        //            *((long*)destptr) = *((long*)srcptr);
        //            destptr += 8;
        //            srcptr += 8;
        //        }
        //        for (uint i = 0; i < lenght8; i++)
        //        {
        //            *destptr = *srcptr;
        //            destptr++;
        //            srcptr++;
        //        }
        //        handle.Free();
        //    }           
        //}
        //public static unsafe void StructureToBin(object[] structures, ref byte[] barray, int offset = 0)
        //{
        //    if (structures.Length > 0)
        //    {
        //        int count = Marshal.SizeOf(structures[0]);

        //        fixed (byte* dest = &barray[offset])
        //        {
        //            int lenght64 = count / 8;
        //            int lenght8 = count % 8;
        //            byte* destptr = dest;
        //            foreach (object structure in structures)
        //            {
        //                GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
        //                byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());

        //                for (uint i = 0; i < lenght64; i++)
        //                {
        //                    *((long*)destptr) = *((long*)srcptr);
        //                    destptr += 8;
        //                    srcptr += 8;
        //                }
        //                for (uint i = 0; i < lenght8; i++)
        //                {
        //                    *destptr = *srcptr;
        //                    destptr++;
        //                    srcptr++;
        //                }
        //                handle.Free();
        //            }
        //        }
        //    }
        //}

        //public static unsafe void   StructureToPtr(object structure, IntPtr ptr)
        //{
        //    int count = Marshal.SizeOf(structure);
        //    GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
        //    byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());
        //    byte* destptr = (byte*)ptr.ToPointer();
        //    int lenght64 = count / 8;
        //    int lenght8 = count % 8;
        //    for (uint i = 0; i < lenght64; i++)
        //    {
        //        *((long*)destptr) = *((long*)srcptr);
        //        destptr += 8;
        //        srcptr += 8;
        //    }
        //    for (uint i = 0; i < lenght8; i++)
        //    {
        //        *destptr = *srcptr;
        //        destptr++;
        //        srcptr++;
        //    }
        //    handle.Free();
        //}
        //public static unsafe IntPtr StructureToPtr(object structure)
        //{
        //    int count = Marshal.SizeOf(structure);
        //    IntPtr ptr = Marshal.AllocHGlobal(count);         
        //    GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
        //    byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());
        //    byte* destptr = (byte*)ptr.ToPointer();
        //    int lenght64 = count / 8;
        //    int lenght8 = count % 8;
        //    for (uint i = 0; i < lenght64; i++)
        //    {
        //        *((long*)destptr) = *((long*)srcptr);
        //        destptr += 8;
        //        srcptr += 8;
        //    }
        //    for (uint i = 0; i < lenght8; i++)
        //    {
        //        *destptr = *srcptr;
        //        destptr++;
        //        srcptr++;
        //    }
        //    handle.Free();

        //    return ptr;
        //}
        //public static unsafe IntPtr StructureToPtr(object[] structures)
        //{
        //    if (structures.Length > 0)
        //    {
        //        int count = Marshal.SizeOf(structures[0]);
        //        IntPtr ptr = Marshal.AllocHGlobal(count * structures.Length);
        //        byte* destptr = (byte*)ptr.ToPointer();
        //        int lenght64 = count / 8;
        //        int lenght8 = count % 8;
        //        foreach (object structure in structures)
        //        {
        //            GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
        //            byte* srcptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());
                                  
        //            for (uint i = 0; i < lenght64; i++)
        //            {
        //                *((long*)destptr) = *((long*)srcptr);
        //                destptr += 8;
        //                srcptr += 8;
        //            }
        //            for (uint i = 0; i < lenght8; i++)
        //            {
        //                *destptr = *srcptr;
        //                destptr++;
        //                srcptr++;
        //            }
        //            handle.Free();
        //        }
        //        return ptr;
        //    }
        //    return IntPtr.Zero;
        //}

        //public static unsafe object PtrToStructure(IntPtr ptr, Type type)
        //{
        //    int count = Marshal.SizeOf(type);
        //    object structure = Activator.CreateInstance(type);
        //    GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
        //    byte* destptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());
        //    byte* srcptr = (byte*)ptr.ToPointer();
        //    int lenght64 = count / 8;
        //    int lenght8 = count % 8;
        //    for (uint i = 0; i < lenght64; i++)
        //    {
        //        *((long*)destptr) = *((long*)srcptr);
        //        destptr += 8;
        //        srcptr += 8;
        //    }
        //    for (uint i = 0; i < lenght8; i++)
        //    {
        //        *destptr = *srcptr;
        //        destptr++;
        //        srcptr++;
        //    }
        //    handle.Free();
        //    return structure;
        //}
        //public static unsafe object PtrToStructure(IntPtr ptr, object example)
        //{
        //    int count = Marshal.SizeOf(example);
        //    object structure = Activator.CreateInstance(example.GetType());
        //    GCHandle handle = GCHandle.Alloc(structure, GCHandleType.Pinned);
        //    byte* destptr = (byte*)(handle.AddrOfPinnedObject().ToPointer());
        //    byte* srcptr = (byte*)ptr.ToPointer();
        //    int lenght64 = count / 8;
        //    int lenght8 = count % 8;
        //    for (uint i = 0; i < lenght64; i++)
        //    {
        //        *((long*)destptr) = *((long*)srcptr);
        //        destptr += 8;
        //        srcptr += 8;
        //    }
        //    for (uint i = 0; i < lenght8; i++)
        //    {
        //        *destptr = *srcptr;
        //        destptr++;
        //        srcptr++;
        //    }
        //    handle.Free();
        //    return structure;
        //}

        ///// <summary>
        ///// Allow copying memory from one IntPtr to another. Required as the <see cref="System.Runtime.InteropServices.Marshal.Copy(System.IntPtr, System.IntPtr[], int, int)"/> implementation does not provide an appropriate override.
        ///// </summary>
        ///// <param name="dest"></param>
        ///// <param name="src"></param>
        ///// <param name="count"></param>
        //[DllImport("kernel32.dll", EntryPoint = "RtlCopyMemory", ExactSpelling = false, SetLastError = false)]
        //[SecurityCritical]
        //public static extern void MoveMemory(IntPtr dest, IntPtr src, uint count);

        //[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", ExactSpelling = false, SetLastError = false)]
        //[SecurityCritical]
        //public static extern unsafe void MoveMemoryPtr(void* dest, void* src, uint count);

#if !NET40Plus

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, ExactSpelling = false)]
        [SecurityCritical]
        internal static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr va_list_arguments);

        [SecurityCritical]
        internal static string GetMessage(int errorCode)
        {
            StringBuilder stringBuilder = new StringBuilder(512);
            if (UnsafeNative.FormatMessage(12800, IntPtr.Zero, errorCode, 0, stringBuilder, stringBuilder.Capacity, IntPtr.Zero) != 0)
            {
                return stringBuilder.ToString();
            }
            return string.Concat("UnknownError_Num ", errorCode);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SYSTEM_INFO
        {
            internal _PROCESSOR_INFO_UNION uProcessorInfo;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort dwProcessorLevel;
            public ushort dwProcessorRevision;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct _PROCESSOR_INFO_UNION
        {
            [FieldOffset(0)]
            internal uint dwOemId;
            [FieldOffset(0)]
            internal ushort wProcessorArchitecture;
            [FieldOffset(2)]
            internal ushort wReserved;
        }

        [Flags]
        public enum FileMapAccess : uint
        {
            FileMapCopy = 0x0001,
            FileMapWrite = 0x0002,
            FileMapRead = 0x0004,
            FileMapAllAccess = 0x001f,
            FileMapExecute = 0x0020,
        }

        [Flags]
        internal enum FileMapProtection : uint
        {
            PageReadonly = 0x02,
            PageReadWrite = 0x04,
            PageWriteCopy = 0x08,
            PageExecuteRead = 0x20,
            PageExecuteReadWrite = 0x40,
            SectionCommit = 0x8000000,
            SectionImage = 0x1000000,
            SectionNoCache = 0x10000000,
            SectionReserve = 0x4000000,
        }
        /// <summary>
        /// Cannot create a file when that file already exists.
        /// </summary>
        internal const int ERROR_ALREADY_EXISTS = 0xB7; // 183
        /// <summary>
        /// The system cannot open the file.
        /// </summary>
        internal const int ERROR_TOO_MANY_OPEN_FILES = 0x4; // 4
        /// <summary>
        /// Access is denied.
        /// </summary>
        internal const int ERROR_ACCESS_DENIED = 0x5; // 5
        /// <summary>
        /// The system cannot find the file specified.
        /// </summary>
        internal const int ERROR_FILE_NOT_FOUND = 0x2; // 2

        [DllImport("kernel32.dll", CharSet = CharSet.None, SetLastError = true)]
        [SecurityCritical]
        internal static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
        [SecurityCritical]
        internal static extern SafeMemoryMappedFileHandle CreateFileMapping(SafeFileHandle hFile, IntPtr lpAttributes, FileMapProtection fProtect, int dwMaxSizeHi, int dwMaxSizeLo, string lpName);
        internal static SafeMemoryMappedFileHandle CreateFileMapping(SafeFileHandle hFile, FileMapProtection flProtect, Int64 ddMaxSize, string lpName)
        {
            int hi = (Int32)(ddMaxSize / Int32.MaxValue);
            int lo = (Int32)(ddMaxSize % Int32.MaxValue);
            return CreateFileMapping(hFile, IntPtr.Zero, flProtect, hi, lo, lpName);
        }

        [DllImport("kernel32.dll")]
        internal static extern void GetSystemInfo([MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern SafeMemoryMappedViewHandle MapViewOfFile(
            SafeMemoryMappedFileHandle hFileMappingObject,
            FileMapAccess dwDesiredAccess,
            UInt32 dwFileOffsetHigh,
            UInt32 dwFileOffsetLow,
            UIntPtr dwNumberOfBytesToMap);
        internal static SafeMemoryMappedViewHandle MapViewOfFile(SafeMemoryMappedFileHandle hFileMappingObject, FileMapAccess dwDesiredAccess, ulong ddFileOffset, UIntPtr dwNumberofBytesToMap)
        {
            uint hi = (UInt32)(ddFileOffset / UInt32.MaxValue);
            uint lo = (UInt32)(ddFileOffset % UInt32.MaxValue);
            return MapViewOfFile(hFileMappingObject, dwDesiredAccess, hi, lo, dwNumberofBytesToMap);
        }

        [DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true, ThrowOnUnmappableChar = true)]
        internal static extern SafeMemoryMappedFileHandle OpenFileMapping(
             uint dwDesiredAccess,
             bool bInheritHandle,
             string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);

#endif
    }
}
