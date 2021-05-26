using Microsoft.Win32.SafeHandles;
using System.Doors.Drive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.IO.MemoryMappedFiles
{
#if !NET40Plus
    /// <summary>
    /// 
    /// </summary>
    [SecurityPermission(SecurityAction.LinkDemand)]
    public sealed class MemoryMappedViewAccessor : IDisposable
    {
        MemoryMappedView _view;

        internal MemoryMappedViewAccessor(MemoryMappedView memoryMappedView)
        {
            this._view = memoryMappedView;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public SafeMemoryMappedViewHandle SafeMemoryMappedViewHandle
        {
            [SecurityCritical]
            [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return this._view.SafeMemoryMappedViewHandle;
            }
        }

        /// <summary>
        /// Dispose pattern
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposeManagedResources)
        {
            if (_view != null)
                _view.Dispose();
            _view = null;
        }

        internal static unsafe void PtrToStructure(byte* ptr, out object structure, Type t)
        {
            structure = Marshal.PtrToStructure((IntPtr)ptr, t);
        }

        internal static unsafe void StructureToPtr(ref object structure, byte* ptr)
        {
            Marshal.StructureToPtr(structure, (IntPtr)ptr, false);
        }

        internal unsafe void Write(long position, object structure, Type t = null)
        {
            uint elementSize = (uint)Marshal.SizeOf(structure.GetType());
            if (position > this._view.Size - elementSize)
                throw new ArgumentOutOfRangeException("position", "");

            try
            {
                byte* ptr = null;
                _view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                ptr += +_view.ViewStartOffset + position;
                StructureToPtr(ref structure, ptr);
            }
            finally
            {
                _view.SafeMemoryMappedViewHandle.ReleasePointer();
            }
        }

        internal unsafe void WriteArray(long position, object[] buffer, int index, int count)
        {
            Type t = buffer[0].GetType();
            uint elementSize = (uint)Marshal.SizeOf(t);

            if (position > this._view.Size - (elementSize * count))
                throw new ArgumentOutOfRangeException("position");
            
            try
            {
                byte* ptr = null;
                _view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                ptr += _view.ViewStartOffset + position;

                for (var i = 0; i < count; i++)
                    StructureToPtr(ref buffer[index + i], ptr + (i * elementSize));
            }
            finally
            {
                _view.SafeMemoryMappedViewHandle.ReleasePointer();
            }
        }

        internal unsafe void Read(long position, out object structure, Type t)
        {
            uint size = (uint)Marshal.SizeOf(t);

            if (position > this._view.Size - size)
                throw new ArgumentOutOfRangeException("position", "");
            try
            {
                byte* ptr = null;
                _view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                ptr += _view.ViewStartOffset + position;
                PtrToStructure(ptr, out structure, t);
            }
            finally
            {
                _view.SafeMemoryMappedViewHandle.ReleasePointer();
            }
        }

        internal unsafe void ReadArray(long position, object[] buffer, int index, int count, Type t)
        {
            uint elementSize = (uint)Marshal.SizeOf(t);

            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (position > this._view.Size - (elementSize * count))
                throw new ArgumentOutOfRangeException("position");
            try
            {
                byte* ptr = null;
                _view.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                ptr += _view.ViewStartOffset + position;
                
                for (var i = 0; i < count; i++)
                    PtrToStructure(ptr + (i * elementSize), out buffer[index + i], t);
            }
            finally
            {
                _view.SafeMemoryMappedViewHandle.ReleasePointer();
            }
        }
    }
#endif
}
