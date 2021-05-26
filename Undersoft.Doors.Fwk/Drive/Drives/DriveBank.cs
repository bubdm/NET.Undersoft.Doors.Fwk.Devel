using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace System.Doors.Drive
{
    /// <summary>
    /// Read/Write buffer with support for simple inter-process read/write synchronisation.
    /// </summary>
    [SecurityPermission(SecurityAction.LinkDemand)]
    public unsafe class DriveBank : DriveRecorder, IDrive
    {
        public Type type;

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
                Write(ref value, index, type);
            }
        }
        public byte[] this[long index]
        {
            get
            {
                object item;
                Read(out item, index, type);
                return (byte[])item;
            }
            set
            {
                object o = value;
                Write(ref o, index, type);
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

        public DriveBank(string file, string name, int bufferSize, Type _type) : 
                         base(file, name, bufferSize, true, true)
        {
            type = _type;
            Open();
        }
        public DriveBank(string file, string name, Type _type) : 
                         base(file, name, 0, false, true)
        {
            type = _type;
            Open();
        }       

        #region Writing

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods")]
        new public void Write(ref object data, long startIndex = 0, Type t = null, int timeout = 1000, int offset = 0)
        {
            base.Write(ref data, startIndex, t, timeout, offset);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods")]
        new public void Write(object[] buffer, long startIndex = 0, Type t = null, int timeout = 1000)
        {
            base.Write(buffer, startIndex, t);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods")]
        new public void Write(IntPtr ptr, long length, long startIndex = 0, Type t = null, int timeout = 1000)
        {
            AcquireWriteLock();
            base.Write(ptr, length, startIndex);
            ReleaseWriteLock();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods")]
        new public void Write(Action<IntPtr> writeFunc, long bufferPosition = 0)
        {
            base.Write(writeFunc, bufferPosition);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods")]
        new public void Write(byte* ptr, long length, long startIndex = 0, Type t = null, int timeout = 1000)
        {
            AcquireWriteLock();
            base.Write(ptr, length, startIndex);
            ReleaseWriteLock();
        }

        #endregion

        #region Reading

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods")]
        new public void Read(out object data, long startIndex = 0, Type t = null, int timeout = 1000, int offset = 0)
        {
            base.Read(out data, startIndex, t, offset);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods")]
        new public void Read(ref object[] buffer, long startIndex = 0, Type t = null, int timeout = 1000)
        {
            base.Read(ref buffer, startIndex, t);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods")]
        new public void Read(IntPtr destination, long length, long startIndex = 0, Type t = null, int timeout = 1000)
        {
            base.Read(destination, length, startIndex);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods")]
        new public void Read(byte* destination, long length, long startIndex = 0, Type t = null, int timeout = 1000)
        {
            base.Read(destination, length, startIndex);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1061:DoNotHideBaseClassMethods")]
        new public void Read(Action<IntPtr> readFunc, long bufferPosition = 0)
        {
            base.Read(readFunc, bufferPosition);
        }

        #endregion

        public void CopyTo(IDrive destination, uint length, int startIndex = 0)
        {
            UnsafeNative.CopyMemory(destination.GetDrivePtr() + startIndex, this.GetDrivePtr(), length);
        }
    }
}
