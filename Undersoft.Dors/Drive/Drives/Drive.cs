using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Diagnostics;
using System.Threading;
using System.Dors;

namespace System.Dors.Drive
{
    [SecurityPermission(SecurityAction.LinkDemand)] [SecurityPermission(SecurityAction.InheritanceDemand)]
    public abstract unsafe class Drive : IDisposable
    {
        public bool Exists
        { get; set; } = false;
        public bool FixSize
        { get; set; } = false;
        public Type ItemType
        { get; private set; }

        public string Path
        { get; set; }
        public string Name
        { get; private set; }

        public ushort DriveId
        { get; set; } = 0;
        public ushort SectorId
        { get; set; } = 0;

        public long BufferSize
        { get; set; } = 0;
        public long UsedSize
        { get; set; } = 0;
        public long FreeSize
        { get; set; } = 0;

        public int ItemSize
        { get; set; } = -1;
        public int ItemCount
        { get; set; } = -1;
        public long ItemCapacity
        { get; set; } = -1;

        public virtual long SharedMemorySize
        {
            get
            {
                return HeaderOffset + Marshal.SizeOf(typeof(DriveHeader)) + BufferSize;
            }
        }
        public virtual long UsedMemorySize
        {
            get
            {
                return HeaderOffset + Marshal.SizeOf(typeof(DriveHeader)) + UsedSize;
            }
        }
        public virtual long FreeMemorySize
        {
            get
            {
                return HeaderOffset + Marshal.SizeOf(typeof(DriveHeader)) + FreeSize;
            }
        }

        protected virtual long HeaderOffset
        {
            get
            {
                return 0;
            }
        }
        protected virtual long BufferOffset
        {
            get
            {
                return HeaderOffset + Marshal.SizeOf(typeof(DriveHeader));
            }
        }

        public bool IsOwnerOfSharedMemory
        { get; private set; }
        public bool ShuttingDown
        {
            get
            {
                if (Header == null || Header->Shutdown == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        protected MemoryMappedFile Mmf;                                /// Memory mapped file
        protected MemoryMappedViewAccessor View;                       /// Memory mapped view

        protected byte* ViewPtr = null;                                /// Pointer to the memory mapped view
        protected byte* BufferStartPtr = null;                         /// Pointer to the start of the buffer region of the memory mapped view
        protected DriveHeader* Header = null;                          /// Pointer to the header within shared memory        

        protected Drive(string file, string name, long bufferSize, bool ownsSharedMemory, bool fixSize = false, Type itemType = null)
        {
            #region Argument validation
            if (name == String.Empty || name == null)
                throw new ArgumentException("Cannot be String.Empty or null", "name");
            if (ownsSharedMemory && bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "Buffer size must be larger than zero when creating a new shared memory buffer.");
#if DEBUG
            else if (!ownsSharedMemory && bufferSize > 0)
                System.Diagnostics.Debug.Write("Buffer size is ignored when opening an existing shared memory buffer.", "Warning");
#endif
            #endregion

            IsOwnerOfSharedMemory = ownsSharedMemory;
            Name = name;
            Path = file;

            if (IsOwnerOfSharedMemory)
            {
                BufferSize = bufferSize;
                ItemType = itemType;
                ItemSize = (itemType != null) ? Marshal.SizeOf(ItemType) : -1;
                FixSize = fixSize;
            }
        }

        ~Drive()
        {
            Dispose(false);
        }

        #region Open / Close
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        protected bool Open()
        {
            Close();

            try
            {                
                if (!CheckMapFileOwnership(Path))
                {
                    IsOwnerOfSharedMemory = true;
                    bool exists = false;
                    Mmf = MemoryMappedFile.CreateNew(Path, CoreInfo.AssemblyGuid + "/" + Name, SharedMemorySize, out exists, FixSize);
                    if (exists && FixSize)
                    {
                        PreReadHeader();
                        CreateView();
                    }
                    else
                    {
                        CreateView();
                        InitHeader();
                    }
                    Exists = exists;                  
                }
                else
                {
                    IsOwnerOfSharedMemory = false;
                    Mmf = MemoryMappedFile.OpenExisting(CoreInfo.AssemblyGuid + "/" + Name);
                    PreReadHeader();
                    CreateView();
                }
            }
            catch
            {
                try
                {
                    IsOwnerOfSharedMemory = false;
                    Mmf = MemoryMappedFile.OpenExisting(CoreInfo.AssemblyGuid + "/" + Name);
                    PreReadHeader();
                    CreateView();
                }
                catch
                {
                    Close();
                    throw;
                }
            }

            try
            {
                if (!DoOpen())
                {
                    Close();
                    return false;
                }
                else
                    return true;
            }
            catch
            {
                Close();
                throw;
            }
        }
        protected virtual bool DoOpen()
        {
            return true;
        }

        public void EnumMemory(EnumMemoryDelegate enumMemoryCallback)
        {
            IntPtr address = IntPtr.Zero;
            MemoryBasicInformation mbi = new MemoryBasicInformation();
            int mbiSize = Marshal.SizeOf(mbi);

            while (Win32.VirtualQueryEx(Process.GetCurrentProcess().Handle, address, out mbi, mbiSize) != 0)
            {
                if (!enumMemoryCallback(mbi))
                    break;

                address = address.Increment(mbi.RegionSize);
            }
        }

        public bool CheckMapFileOwnership(string path)
        {
            bool hasOwner = false;
            string currentpath = !path.Contains(":") ? Directory.GetCurrentDirectory() + "/" + path : path;
            string unipath = currentpath.Replace("/", "\\");

            EnumMemory((region) =>
            {
                if (region.Type != MemoryType.Mapped)
                    return true;

                if (unipath.Equals(GetMappedFileName(region.BaseAddress)))
                    hasOwner = true;
                
                return true;
            });

            return hasOwner;
        }

        public string GetMappedFileName(IntPtr address)
        {
            NtStatus status;
            IntPtr retLength;

            using (var data = new MemoryAlloc(20))
            {
                if ((status = Win32.NtQueryVirtualMemory(
                    Process.GetCurrentProcess().Handle,
                    address,
                    MemoryInformationClass.MemoryMappedFilenameInformation,
                    data.Memory,
                    data.Size.ToIntPtr(),
                    out retLength
                    )) == NtStatus.BufferOverflow)
                {
                    data.ResizeNew(retLength.ToInt32());

                    status = Win32.NtQueryVirtualMemory(
                        Process.GetCurrentProcess().Handle,
                        address,
                        MemoryInformationClass.MemoryMappedFilenameInformation,
                        data.Memory,
                        data.Size.ToIntPtr(),
                        out retLength
                        );
                }

                if (status >= NtStatus.Error)
                       return null;

                return FileUtils.GetFileName(data.ReadStruct<UnicodeString>().Read());
            }
        }

        public IntPtr GetDrivePtr()
        {
            return (IntPtr)BufferStartPtr;
        }

        protected void CreateView()
        {
            View = Mmf.CreateViewAccessor(0, SharedMemorySize, MemoryMappedFileAccess.ReadWrite);
            View.SafeMemoryMappedViewHandle.AcquirePointer(ref ViewPtr);
            Header = (DriveHeader*)(ViewPtr + HeaderOffset);
            if (!IsOwnerOfSharedMemory)
                BufferStartPtr = ViewPtr + HeaderOffset + Marshal.SizeOf(typeof(DriveHeader));
            else
                BufferStartPtr = ViewPtr + BufferOffset;
        }

        public void PreReadHeader()
        {
            using (MemoryMappedViewAccessor headerView =
                     Mmf.CreateViewAccessor(0, HeaderOffset + Marshal.SizeOf(typeof(DriveHeader)), MemoryMappedFileAccess.Read))
            {
                byte* headerPtr = null;

                headerView.SafeMemoryMappedViewHandle.AcquirePointer(ref headerPtr);
                DriveHeader* _header = (DriveHeader*)(headerPtr + HeaderOffset);
                DriveHeader header = (DriveHeader)Marshal.PtrToStructure((IntPtr)_header, typeof(DriveHeader));
                int headerSize = Marshal.SizeOf(typeof(DriveHeader));
                BufferSize = header.SharedMemorySize - headerSize;
                UsedSize = header.UsedMemorySize - headerSize;
                FreeSize = header.FreeMemorySize - headerSize;
                ItemCapacity = header.ItemCapacity;
                ItemSize = header.ItemSize;
                ItemCount = header.ItemCount;
                DriveId = header.DriveId;
                SectorId = header.SectorId;
                headerView.SafeMemoryMappedViewHandle.ReleasePointer();
            }
        }
        public void ReadHeader()
        {
            object _header = new object();
            View.Read(HeaderOffset, out _header, typeof(DriveHeader));
            DriveHeader header = (DriveHeader)_header;
            int headerSize = Marshal.SizeOf(typeof(DriveHeader));
            BufferSize = header.SharedMemorySize - headerSize;
            UsedSize = header.UsedMemorySize - headerSize;
            FreeSize = header.FreeMemorySize - headerSize;
            ItemCapacity = header.ItemCapacity;
            ItemSize = header.ItemSize;
            ItemCount = header.ItemCount;
            DriveId = header.DriveId;
            SectorId = header.SectorId;
        }
        public void InitHeader()
        {
            if (!IsOwnerOfSharedMemory)
                return;

            DriveHeader header = new DriveHeader();
            header.SharedMemorySize = SharedMemorySize;
            header.UsedMemorySize = Marshal.SizeOf(typeof(DriveHeader));
            header.FreeMemorySize = SharedMemorySize - header.UsedMemorySize;
            header.ItemSize = (ItemType != null && ItemSize > 0) ? ItemSize : -1;
            header.ItemCapacity = (ItemType != null && ItemSize > 0) ? (header.FreeMemorySize / ItemSize) : -1;
            header.ItemCount = -1;
            header.Shutdown = 0;
            View.Write(HeaderOffset, header);
        }
        public void WriteHeader()
        {
            DriveHeader header = new DriveHeader();
            header.SharedMemorySize = SharedMemorySize;
            header.UsedMemorySize = UsedMemorySize;
            header.FreeMemorySize = FreeMemorySize;
            header.ItemCapacity = ItemCapacity;
            header.ItemCount = ItemCount;
            header.ItemSize = ItemSize;
            header.DriveId = DriveId;
            header.SectorId = SectorId;
            header.Shutdown = 0;
            View.Write(HeaderOffset, header);
        }

        public virtual void Close()
        {
            if (IsOwnerOfSharedMemory && View != null)
            {
                // Indicates to any open instances that the owner is no longer open
#pragma warning disable 0420 // ignore ref to volatile warning - Interlocked API
                Interlocked.Exchange(ref Header->Shutdown, 1);
#pragma warning restore 0420
            }

            DoClose();

            if (View != null)
            {
                View.SafeMemoryMappedViewHandle.ReleasePointer();
                View.Dispose();
            }
            if (Mmf != null)
                Mmf.Dispose();

            Header = null;
            ViewPtr = null;
            BufferStartPtr = null;
            View = null;
            Mmf = null;
        }
        protected virtual void DoClose()
        {
        }
        #endregion

        #region Writing
        protected virtual void Write(ref object source, long bufferPosition = 0, Type type = null, int timeout = 1000, int offset = 0)
        {
            View.Write(BufferOffset + bufferPosition + offset, source, type);
        }
        protected virtual void Write(object[] source, long bufferPosition = 0, Type type = null, int timeout = 1000)
        {
            Write(source, 0, bufferPosition, type);
        }
        protected virtual void Write(object[] source, int index, long bufferPosition = 0, Type type = null)
        {
            RawDrive fs = new RawDrive(type);
            fs.WriteArray((IntPtr)(BufferStartPtr + bufferPosition), source, index, source.Length - index);

        }
        protected virtual void Write(IntPtr source, long length, long bufferPosition = 0, Type t = null, int timeout = 1000)
        {
            UnsafeNative.CopyMemory(new IntPtr(BufferStartPtr + bufferPosition), source, (uint)length);
        }
        protected virtual void Write(byte* source, long length, long bufferPosition = 0, Type t = null, int timeout = 1000)
        {
            UnsafeNative.CopyMemory(BufferStartPtr + bufferPosition, source, (uint)length);
        }
        protected virtual void Write(Action<IntPtr> writeFunc, long bufferPosition = 0)
        {
            writeFunc(new IntPtr(BufferStartPtr + bufferPosition));
        }

        protected virtual void WriteArray(object[] source, int index, int count, long bufferPosition = 0, Type t = null)
        {
            RawDrive fs = new RawDrive(t);
            fs.WriteArray((IntPtr)(BufferStartPtr + bufferPosition), source, index, count);
        }
        #endregion

        #region Reading
        protected virtual void Read(out object data, long bufferPosition = 0, Type t = null, int timeout = 1000, int offset = 0)
        {
            View.Read(BufferOffset + bufferPosition + offset, out data, t);
        }
        protected virtual void Read(ref object[] destination, long bufferPosition = 0, Type t = null, int timeout = 1000)
        {
            RawDrive fs = new RawDrive(t);
            fs.ReadArray(ref destination, (IntPtr)(BufferStartPtr + bufferPosition), 0, destination.Length);
        }
        protected virtual void Read(IntPtr destination, long length, long bufferPosition = 0, Type t = null, int timeout = 1000)
        {
            UnsafeNative.CopyMemory(destination, new IntPtr(BufferStartPtr + bufferPosition), (uint)length);
        }
        protected virtual void Read(byte* destination, long length, long bufferPosition = 0, Type t = null, int timeout = 1000)
        {
            UnsafeNative.CopyMemory(destination, BufferStartPtr + bufferPosition, (uint)length);
        }
        protected virtual void Read(Action<IntPtr> readFunc, long bufferPosition = 0)
        {
            readFunc(new IntPtr(BufferStartPtr + bufferPosition));
        }

        protected virtual void ReadArray(ref object[] destination, int index, int count, long bufferPosition, Type t = null)
        {
            RawDrive fs = new RawDrive(t);
            fs.ReadArray(ref destination, (IntPtr)(BufferStartPtr + bufferPosition), index, count);
        }
#endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposeManagedResources)
        {
            if (disposeManagedResources)
            {
                this.Close();
            }
        }
#endregion
    }

    public delegate bool EnumMemoryDelegate(MemoryBasicInformation info);

    public interface IDrive
    {
        object this[int index] { get; set; }
        byte[] this[long index] { get; set; }
        object this[int index, int offset, Type t] { get; set; }

        bool Exists
        { get; set; } 
        bool FixSize
        { get;  set; }

        string Path
        { get; set; }

        ushort DriveId
        { get; set; }
        ushort SectorId
        { get; set; }

        long BufferSize
        { get; set; }
        long UsedSize
        { get; set; }
        long FreeSize
        { get; set; }

        int ItemSize
        { get; set; }
        int ItemCount
        { get; set; }
        long ItemCapacity
        { get; set; }

        long SharedMemorySize
        {
            get;            
        }
        long UsedMemorySize
        {
            get;
        }
        long FreeMemorySize
        {
            get;
        }

        void ReadHeader();

        void WriteHeader();

        IntPtr GetDrivePtr();

        void CopyTo(IDrive destination, uint length, int startIndex = 0);

        void Write(ref object data, long startIndex = 0, Type t = null, int timeout = 1000, int offset = 0);
        void Write(object[] buffer, long startIndex = 0, Type t = null, int timeout = 1000);
        void Write(IntPtr source, long length, long bufferPosition = 0, Type t = null, int timeout = 1000);
        unsafe void Write(byte* source, long length, long bufferPosition = 0, Type t = null, int timeout = 1000);

        void Read(out object data, long startIndex = 0, Type t = null, int timeout = 1000, int offset = 0);
        void Read(ref object[] buffer, long startIndex = 0, Type t = null, int timeout = 1000);
        void Read(IntPtr destination, long length, long startIndex = 0, Type t = null, int timeout = 1000);
        unsafe void Read(byte* destination, long length, long startIndex = 0, Type t = null, int timeout = 1000);

        void EnumMemory(EnumMemoryDelegate enumMemoryCallback);

        string GetMappedFileName(IntPtr address);

        void Close();

        void Dispose();
    }
}

