using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO.MemoryMappedFiles;
using System.Security.Permissions;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Dors;

namespace System.Dors.Drive
{
    public class RawDrive
    {
        public Type typeStruct = null;
        public int sizeStruct = 0;

        public RawDrive(Type t)
        {
            typeStruct = t;          
            sizeStruct = Marshal.SizeOf(t);
        }

        public unsafe void* GetPtr(object[] structure)
        {
            Type gg = structure.GetType();
            GCHandle pinn =  GCHandle.Alloc(structure, GCHandleType.Pinned);
            IntPtr address = Marshal.UnsafeAddrOfPinnedArrayElement(structure, 0);
            return address.ToPointer();
        }
        public object PtrToStructure(IntPtr pointer)
        {
            return Marshal.PtrToStructure(pointer, typeStruct);
        }
        public void StructureToPtr(ref object s, IntPtr pointer)
        {
            Marshal.StructureToPtr(s, pointer, false);
        }

        public int SizeOf(object t)
        {
            return Marshal.SizeOf(t);
        }

        public unsafe void ReadArray(ref object[] buffer, IntPtr source, int index, int count)
        {
            if (index < 0) index = 0;
            if (buffer == null) buffer = new object[count];
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - index < count) count = buffer.Length - index;

            int elementSize = sizeStruct;
            int offset = index * elementSize;
            int length = count * elementSize;

            for (int i = 0; i < count; i++)
                Marshal.PtrToStructure(source + ((i * sizeStruct) + offset), buffer[i]);            
        }
        public unsafe void WriteArray(IntPtr destination, object[] buffer, int index, int count)
        {        
            if (index < 0) index = 0;
            if (buffer == null)throw new ArgumentNullException("buffer");
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - index < count) count = buffer.Length - index;

            int elementSize = sizeStruct;
            int offset = index * elementSize;
            int length = count * elementSize;

            byte[] sources = new byte[sizeStruct * count];

            GCHandle handler = GCHandle.Alloc(sources, GCHandleType.Pinned);
            IntPtr rawpointer = handler.AddrOfPinnedObject();
          
            for (int i = 0; i < buffer.Length; i++)
                Marshal.StructureToPtr(buffer[i], rawpointer + (i*sizeStruct), true);

            UnsafeNative.CopyMemory(destination, rawpointer, (uint)length);
        
            handler.Free();
        }
    }   
}