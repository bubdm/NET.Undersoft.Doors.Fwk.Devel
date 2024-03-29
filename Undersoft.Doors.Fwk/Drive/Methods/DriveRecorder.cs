﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;

namespace System.Doors.Drive
{
    [SecurityPermission(SecurityAction.LinkDemand)]
    [SecurityPermission(SecurityAction.InheritanceDemand)]
    public abstract class DriveRecorder : Drive
    {
       // protected EventWaitHandle WriteWaitEvent { get; private set; }                    /// An event handle used for blocking write operations.
       // protected EventWaitHandle ReadWaitEvent { get; private set; }                     /// An event handle used for blocking read operations.

        protected ManualResetEvent WriteWaitEvent { get; private set; }
        protected ManualResetEvent ReadWaitEvent { get; private set; }

        protected DriveRecorder(string path, string name, long bufferSize, bool ownsSharedMemory, bool fixsize) :
                                base(path, name, bufferSize, ownsSharedMemory, fixsize)
        {
            WriteWaitEvent = new ManualResetEvent(true); //new EventWaitHandle(true, EventResetMode.ManualReset, Name + "_evt_write");
            ReadWaitEvent = new ManualResetEvent(true); // new EventWaitHandle(true, EventResetMode.ManualReset, Name + "_evt_read");
        }

        private int _readWriteTimeout = 100;

        public virtual int ReadWriteTimeout
        {
            get { return _readWriteTimeout; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("ReadWriteTimeout", "Must be larger than -1.");
                _readWriteTimeout = value;
            }
        }

        public bool AcquireReadLock(int millisecondsTimeout = System.Threading.Timeout.Infinite)
        {
            if (!ReadWaitEvent.WaitOne(millisecondsTimeout))
                return false;
            WriteWaitEvent.Reset();
            return true;
        }
        public void ReleaseReadLock()
        {
            WriteWaitEvent.Set();
        }

        public bool AcquireWriteLock(int millisecondsTimeout = System.Threading.Timeout.Infinite)
        {
            if (!WriteWaitEvent.WaitOne(millisecondsTimeout))
                return false;
            ReadWaitEvent.Reset();
            return true;
        }
        public void ReleaseWriteLock()
        {
            ReadWaitEvent.Set();
        }

        #region Writing

        private void WriteWait()
        {
            if (!WriteWaitEvent.WaitOne(ReadWriteTimeout))
                throw new TimeoutException("The write operation timed out waiting for the write lock WaitEvent. Check your usage of AcquireWriteLock/ReleaseWriteLock and AcquireReadLock/ReleaseReadLock.");
        }

        protected override void Write(ref object data, long bufferPosition = 0, Type t = null, int timeout = 1000, int offset = 0)
        {
            WriteWait();
            base.Write(ref data, bufferPosition, t, timeout, offset);
        }
        protected override void Write(object[] buffer, long bufferPosition = 0, Type t = null, int timeout = 1000)
        {
            WriteWait();
            base.Write(buffer, bufferPosition, t, timeout);
        }
        protected override void Write(IntPtr ptr, long length, long bufferPosition = 0, Type t = null, int timeout = 1000)
        {
            WriteWait();
            base.Write(ptr, length, bufferPosition);
        }
        protected override void Write(Action<IntPtr> writeFunc, long bufferPosition = 0)
        {
            WriteWait();
            base.Write(writeFunc, bufferPosition);
        }

        #endregion

        #region Reading

        private void ReadWait()
        {
            if (!ReadWaitEvent.WaitOne(ReadWriteTimeout))
                throw new TimeoutException("The read operation timed out waiting for the read lock WaitEvent. Check your usage of AcquireWriteLock/ReleaseWriteLock and AcquireReadLock/ReleaseReadLock.");
        }

        protected override void Read(out object data, long bufferPosition = 0, Type t = null, int timeout = 1000, int offset = 0)
        {
            ReadWait();
            base.Read(out data, bufferPosition, t, timeout, offset);
        }
        protected override void Read(ref object[] buffer, long bufferPosition = 0, Type t = null, int timeout = 1000)
        {
            ReadWait();
            base.Read(ref buffer, bufferPosition, t, timeout);
        }
        protected override void Read(IntPtr destination, long length, long bufferPosition = 0, Type t = null, int timeout = 1000)
        {
            ReadWait();
            base.Read(destination, length, bufferPosition);
        }
        protected override void Read(Action<IntPtr> readFunc, long bufferPosition = 0)
        {
            ReadWait();
            base.Read(readFunc, bufferPosition);
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                (WriteWaitEvent as IDisposable).Dispose();
                (ReadWaitEvent as IDisposable).Dispose();
            }
            base.Dispose(disposeManagedResources);
        }

        #endregion
    }
}
