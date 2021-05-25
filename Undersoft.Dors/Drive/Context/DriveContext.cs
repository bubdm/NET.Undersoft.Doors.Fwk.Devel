using System;
using System.IO;
using System.Dors;
using System.Runtime.InteropServices;

namespace System.Dors.Drive
{
    public unsafe sealed class DriveContext : IDriveContext, IDisposable
    {     
        private MemoryStream msReceive = new MemoryStream();
        private MemoryStream msRead = new MemoryStream();
        private byte[] binReceive = new byte[0];
        private byte[] binSend = new byte[0];
        public IntPtr binReceiveHandler;
        public IntPtr binReceivePtr;

        public DriveContext()
        {
        }

        public DepotSite IdentitySite
        { get; set; } = DepotSite.Server;

        public string Place
        { get; set; }
        public string File
        { get; set; }

        public ushort DriveId
        { get; set; } = 0;
        public ushort SectorId
        { get; set; } = 0;

        public int NodeCount { get; set; } = 50;
        public int ServerCount { get; set; } = 1;
        public int ClientCount { get; set; } = 1;
        public int ClientWaitCount = 0;
        public int ServerWaitCount = 0;
        public int Elements { get; set; } = 1;
        public int WriteCount = 0;
        public int ReadCount = 0;

        public long BufferSize
        {
            get; set;
        } = 1048576;
      
        public long UsedSize
        {
            get; set;
        } = 0;
        public long FreeSize
        {
            get; set;
        } = 0;

        public int ItemSize
        { get; set; } = -1;
        public int ItemCount
        { get; set; } = -1;
        public long ItemCapacity
        { get; set; } = -1;

        public int ObjectPosition
        {
            get; set;
        } = 0;
        public int ObjectsLeft
        {
            get; set;
        } = 0;

        public byte[] SerialBlock
        {
            get
            {
                return binSend;
            }
            set
            {
                binSend = value;
                if (binSend != null && BlockOffset > 0) 
                {
                    long size = binSend.Length - BlockOffset;
                    new byte[] { (byte) 'D',
                                 (byte) 'P',
                                 (byte) 'O',
                                 (byte) 'T' }.CopyTo(binSend, 0);
                    BitConverter.GetBytes(size).CopyTo(binSend, 4);
                    BitConverter.GetBytes(ObjectPosition).CopyTo(binSend, 12);
                }
            }
        }
        public int SerialBlockId
        {
            get; set;
        } = 0;

        public long BlockSize
        { get; set; } = 0;
        public int  BlockOffset
        {
            get; set;
        } = 16;


        public byte[] DeserialBlock
        {
            get
            {
                byte[] result = null;
                lock (binReceive)
                {
                    BlockSize = 0;
                    result = binReceive;
                    binReceive = new byte[0];
                }
                return result;
            }
        }
        public int DeserialBlockId
        {
            get; set;
        } = 0;

        public NoiseType ReceiveBytes(byte[] buffer, long received)
        {

            NoiseType noiseKind = NoiseType.None;
            lock (binReceive)
            {
                int offset = 0, length = (int)received;
                bool inprogress = false;
                if (BlockSize == 0)
                {

                    BlockSize = BitConverter.ToInt64(buffer, 4);
                    DeserialBlockId = BitConverter.ToInt32(buffer, 12);
                    binReceive = new byte[BlockSize];
                    GCHandle gc = GCHandle.Alloc(binReceive, GCHandleType.Pinned);
                    binReceivePtr = GCHandle.ToIntPtr(gc);
                    offset = BlockOffset;
                    length -= BlockOffset;
                }

                if (BlockSize > 0)
                    inprogress = true;

                BlockSize -= length;

                if (BlockSize < 1)
                {
                    long endPosition = received;
                    noiseKind = buffer.SeekNoise(out endPosition, SeekDirection.Backward);
                }

                int destid = (binReceive.Length - ((int)BlockSize + length));
                if (inprogress)
                {
                    fixed (void* msgbuff = buffer)
                    {
                        MemoryNative.Copy(GCHandle.FromIntPtr(binReceivePtr).AddrOfPinnedObject() + destid, new IntPtr(msgbuff) + offset, (ulong)length);
                        //Marshal.Copy(HeaderBuffer, offset, GCHandle.FromIntPtr(binReceiveHandler).AddrOfPinnedObject() + destid, length);
                    }
                   // Marshal.Copy(buffer, offset, (GCHandle.FromIntPtr(binReceivePtr)).AddrOfPinnedObject() + destid, length);
                }
            }
            return noiseKind;
        }
        public NoiseType ReceiveBytes(byte[] buffer, int received)
        {
            NoiseType noiseKind = NoiseType.None;
            lock (binReceive)
            {
                int offset = 0, length = received;
                bool inprogress = false;

                if (BlockSize == 0)
                {
                    BlockSize = BitConverter.ToInt64(buffer, 4);
                    DeserialBlockId = BitConverter.ToInt32(buffer, 12);
                    binReceive = new byte[BlockSize];
                    GCHandle gc = GCHandle.Alloc(binReceive, GCHandleType.Pinned);
                    binReceivePtr = GCHandle.ToIntPtr(gc);
                    offset = BlockOffset;
                    length -= BlockOffset;

                }
                if (BlockSize > 0)
                    inprogress = true;

                BlockSize -= length;

                if (BlockSize < 1)
                {
                    long endPosition = received;
                    noiseKind = buffer.SeekNoise(out endPosition, SeekDirection.Backward);
                }
                int destid = (binReceive.Length - ((int)BlockSize + length));
                if (inprogress)
                {
                    fixed (void* msgbuff = buffer)
                    {
                        MemoryNative.Copy(GCHandle.FromIntPtr(binReceivePtr).AddrOfPinnedObject() + destid, new IntPtr(msgbuff) + offset, (ulong)length);
                        //Marshal.Copy(HeaderBuffer, offset, GCHandle.FromIntPtr(binReceiveHandler).AddrOfPinnedObject() + destid, length);
                    }
                    // Marshal.Copy(buffer, offset, (GCHandle.FromIntPtr(binReceivePtr)).AddrOfPinnedObject() + destid, length);
                }
            }
            return noiseKind;
        }

        public void WriteDrive(IDrive drive)
        {
            if (drive != null)
            {
                GCHandle handler = GCHandle.Alloc(SerialBlock, GCHandleType.Pinned);
                IntPtr rawpointer = handler.AddrOfPinnedObject();
                drive.BufferSize = SerialBlock.Length;
                drive.WriteHeader();
                drive.Write(rawpointer, SerialBlock.Length);
                handler.Free();
            }
        }
        public object ReadDrive(IDrive drive)
        {
            if (drive != null)
            {
                drive.ReadHeader();
                BufferSize = drive.BufferSize;
                byte[] bufferread = new byte[BufferSize];
                GCHandle handler = GCHandle.Alloc(bufferread, GCHandleType.Pinned);
                IntPtr rawpointer = handler.AddrOfPinnedObject();
                drive.Read(rawpointer, BufferSize, 0);
                ReceiveBytes(bufferread, BufferSize);
                handler.Free();
            }
            return DeserialBlock;
        }
      
        public void Dispose()
        {
            msRead.Dispose();
            msReceive.Dispose();
            if (!binReceivePtr.Equals(IntPtr.Zero))
            {
                GCHandle gc = GCHandle.FromIntPtr(binReceivePtr);
                gc.Free();
            }
            binReceive = null;
            binSend = null;
        }
    }
}
