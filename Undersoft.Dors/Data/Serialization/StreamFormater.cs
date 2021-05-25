using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Dors;

namespace System.Dors.Data
{
    #region Headers - BinaryFormatter
    public static class StreamBank
    {
        public static int SetRaw(this DataVaults bank, Stream tostream)
        {
            if (tostream == null) tostream = new MemoryStream();
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(tostream, bank);
            return (int)tostream.Length;
        }
        public static DataVaults GetRaw(this DataVaults bank, Stream fromstream)
        {
            try
            {
                BinaryFormatter binform = new BinaryFormatter();
                return (DataVaults)binform.Deserialize(fromstream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class StreamVault
    {
        public static int SetRaw(this DataVault vault, Stream tostream)
        {
            if (tostream == null) tostream = new MemoryStream();
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(tostream, vault);
            return (int)tostream.Length;
        }
        public static DataVault GetRaw(this DataVault vault, Stream fromstream)
        {
            try
            {
                BinaryFormatter binform = new BinaryFormatter();
                return (DataVault)binform.Deserialize(fromstream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
    public static class StreamDeposit
    {
        public static int SetRaw(this DataDeposit deposit, Stream tostream)
        {
            if (tostream == null) tostream = new MemoryStream();
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(tostream, deposit);
            return (int)tostream.Length;
        }
        public static DataDeposit GetRaw(this DataDeposit deposit, Stream fromstream)
        {
            try
            {
                BinaryFormatter binform = new BinaryFormatter();
                return (DataDeposit)binform.Deserialize(fromstream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class StreamBox
    {
        public static int SetRaw(this DataBox box, Stream tostream)
        {
            if (tostream == null) tostream = new MemoryStream();
            box.Prime.SetRaw(tostream);
            return (int)tostream.Length;
        }
        public static DataTrellis GetRaw(this DataBox box, Stream fromstream)
        {
            try
            {
                if (box.Prime.GetType() == typeof(DataTrellis))
                {
                    BinaryFormatter binform = new BinaryFormatter();
                    return (DataTrellis)binform.Deserialize(fromstream);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class StreamSpace
    {
        public static int SetRaw(this DataArea space, Stream tostream)
        {
            if (tostream == null) tostream = new MemoryStream();
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(tostream, space);
            return (int)tostream.Length;
        }
        public static DataArea GetRaw(this DataArea space, Stream fromstream)
        {
            try
            {
                BinaryFormatter binform = new BinaryFormatter();
                return (DataArea)binform.Deserialize(fromstream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class StreamSpheres
    {
        public static int SetRaw(this DataSpheres sets, Stream tostream)
        {
            if (tostream == null) tostream = new MemoryStream();
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(tostream, sets);
            return (int)tostream.Length;
        }
        public static DataSpheres GetRaw(this DataSpheres sets, Stream fromstream)
        {
            try
            {
                BinaryFormatter binform = new BinaryFormatter();
                return (DataSpheres)binform.Deserialize(fromstream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class StreamSphere
    {
        public static int SetRaw(this DataSphere set, Stream tostream)
        {
            if (tostream == null) tostream = new MemoryStream();
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(tostream, set);
            return (int)tostream.Length;
        }
        public static DataSphere GetRaw(this DataSphere set, Stream fromstream)
        {
            try
            {
                BinaryFormatter binform = new BinaryFormatter();
                return (DataSphere)binform.Deserialize(fromstream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class StreamTrellises
    {
        public static int SetRaw(this DataTrellises trell, Stream tostream)
        {
            if (tostream == null) tostream = new MemoryStream();
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(tostream, trell);
            return (int)tostream.Length;
        }
        public static DataTrellises GetRaw(this DataTrellises trell, Stream fromstream)
        {
            try
            {
                BinaryFormatter binform = new BinaryFormatter();
                return (DataTrellises)binform.Deserialize(fromstream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class StreamTrellis
    {
        public static int SetRaw(this DataTrellis trell, Stream tostream)
        {
            if (tostream == null) tostream = new MemoryStream();
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(tostream, trell);
            return (int)tostream.Length;
        }
        public static DataTrellis GetRaw(this DataTrellis trell, Stream fromstream)
        {
            try
            {
                BinaryFormatter binform = new BinaryFormatter();
                return (DataTrellis)binform.Deserialize(fromstream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
    public static class StreamFilter
    {
        public static int SetRaw(this FilterTerms filterTerms, Stream tostream)
        {
            if (tostream == null) tostream = new MemoryStream();
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(tostream, filterTerms);
            return (int)tostream.Length;
        }
        public static FilterTerms GetRaw(this FilterTerms filterTerms, Stream fromstream)
        {
            try
            {
                BinaryFormatter binform = new BinaryFormatter();
                return (FilterTerms)binform.Deserialize(fromstream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class StreamFavorites
    {
        public static int SetRaw(this DataFavorites filterTerms, Stream tostream)
        {
            if (tostream == null) tostream = new MemoryStream();
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(tostream, filterTerms);
            return (int)tostream.Length;
        }
        public static DataFavorites GetRaw(this DataFavorites filterTerms, Stream fromstream)
        {
            try
            {
                BinaryFormatter binform = new BinaryFormatter();
                return (DataFavorites)binform.Deserialize(fromstream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    #endregion

    #region Message - FastStructures
    public static class StreamSimTiers
    {
        public static int SetSim(this DataTiers tiers, Stream tostream, int offset, int batchSize)
        {
            BinaryWriter binwriter = new BinaryWriter(tostream);
            int Size = Marshal.SizeOf(tiers.Trell.Model);
            int length = tiers.Count;
            int yLeft = length - offset;
            int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
            int processed = 0;
            DataTier singleton = new DataTier(tiers.Trell);
            byte[] item = new byte[Size];
            GCHandle handler = GetRawHandler(ref item);
            IntPtr ptr = handler.AddrOfPinnedObject();
            for (int y = offset; y < offset + yLength; y++)
            {
                DataTier tier = tiers[y];
                singleton.State = tier.State;
                singleton.PrimeArray = tier.DataArray;
                singleton.State.Synced = true;
                singleton.SetStateNoid();
                singleton.SetClockNoid();
                SetRaw(ref singleton.n, ptr);
                binwriter.Write(item);
                processed++;
            }
            handler.Free();
            int nextoffset = (processed < yLeft) ? processed + offset : -1;
            int noiseCount = (tostream.Length > 4096) ? 1024 : 4096;
            if (nextoffset > 0)
                tostream.Noise(noiseCount, NoiseType.Block);
            else
                tostream.Noise(noiseCount, NoiseType.End);

            return nextoffset;
        }

        public static byte[] GetRawData(object s, int size)
        {
            byte[] array = new byte[size];
            /////////////// wersja GC handler ////////////////
            GCHandle handler = GCHandle.Alloc(array, GCHandleType.Pinned);
            // Get mem address pointer to pinned bytes 
            IntPtr rawpointer = handler.AddrOfPinnedObject();
            // Get bytes from structure located in mem address directed by pointer  
            Marshal.StructureToPtr(s, rawpointer, true);
            // Free used unallocated mem

            handler.Free();
            ////////////// werjsa Marshal ///////////////
            //IntPtr rawpointer = Marshal.AllocHGlobal(size);
            // Marshal.StructureToPtr(s, rawpointer, false);
            // Marshal.Copy(rawpointer, array, 0, size);
            //Marshal.FreeHGlobal(rawpointer);
            return array;
        }
        public static GCHandle GetRawHandler(ref byte[] array)
        {
            GCHandle handler = GCHandle.Alloc(array, GCHandleType.Pinned);
            return handler;
        }
        public static void SetRaw(ref object s, IntPtr ptr)
        {
            Marshal.StructureToPtr(s, ptr, true);
        }
    }

    public static class StreamTiers
    {
        public static int SetRaw(this DataTiers tiers, Stream tostream, int offset, int batchSize)
        {
            BinaryWriter binwriter = new BinaryWriter(tostream);
            int Size = Marshal.SizeOf(tiers.Trell.Model);
            int length = tiers.Count;
            int yLeft = length - offset;
            int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
            int processed = 0;
            byte[] item = new byte[Size];
            GCHandle handler = GetRawHandler(ref item);
            IntPtr ptr = handler.AddrOfPinnedObject();
            for (int y = offset; y < offset + yLength; y++)
            {
                DataTier temp = tiers[y];
                temp.SetStateNoid();
                temp.SetClockNoid();
                SetRaw(ref temp.n, ptr);
                binwriter.Write(item);
                processed++;
            }
            handler.Free();
            int nextoffset = (processed < yLeft) ? processed + offset : -1;
            int noiseCount = (tostream.Length > 4096) ? 1024 : 4096;
            if (nextoffset > 0)
                tostream.Noise(noiseCount, NoiseType.Block);
            else
                tostream.Noise(noiseCount, NoiseType.End);

            return nextoffset;
        }

        public static byte[] GetRawData(object s, int size)
        {
            byte[] array = new byte[size];
            /////////////// wersja GC handler ////////////////
            GCHandle handler = GCHandle.Alloc(array, GCHandleType.Pinned);
            // Get mem address pointer to pinned bytes 
            IntPtr rawpointer = handler.AddrOfPinnedObject();
            // Get bytes from structure located in mem address directed by pointer  
            Marshal.StructureToPtr(s, rawpointer, true);
            // Free used unallocated mem

            handler.Free();
            ////////////// werjsa Marshal ///////////////
            //IntPtr rawpointer = Marshal.AllocHGlobal(size);
            // Marshal.StructureToPtr(s, rawpointer, false);
            // Marshal.Copy(rawpointer, array, 0, size);
            //Marshal.FreeHGlobal(rawpointer);
            return array;
        }
        public static GCHandle GetRawHandler(ref byte[] array)
        {
            GCHandle handler = GCHandle.Alloc(array, GCHandleType.Pinned);
            return handler;
        }
        public static void SetRaw(ref object s, IntPtr ptr)
        {
            Marshal.StructureToPtr(s, ptr, true);
        }

        public static DataTiers GetRaw(this DataTiers tiers, Stream fromstream, out int rawcount)
        {
            int Size = Marshal.SizeOf(tiers.Trell.Model);
            Type Model = tiers.Trell.Model;
            int noiseCount = (fromstream.Length > 4096) ? 1024 : 4096;
            long endPosition = fromstream.Seek(0, SeekOrigin.End);
            if (fromstream.SeekNoise(SeekOrigin.End, SeekDirection.Backward, 0, noiseCount) != NoiseType.None)
                endPosition = fromstream.Position;

            fromstream.Position = 0;
            if (fromstream.SeekNoise(SeekOrigin.Begin, SeekDirection.Forward, 0, noiseCount) == NoiseType.None)
                fromstream.Position = 0;

            byte[] array = new byte[Size]; int read = 0;
            IntPtr rawpointer = Marshal.AllocHGlobal(Size);
            int _rawcount = 0;

            lock (tiers.access)
                lock (tiers.TiersView.access)
                {
                    if (tiers.ProgressCount == 0 && fromstream.Length > 0)
                    {
                        tiers.TiersView.Clear();
                        tiers.TiersView.ClearJoins();
                    }
                    object native = null;
                    while ((read = fromstream.Read(array, 0, array.Length)) > 0)
                    {
                        if (endPosition >= fromstream.Position)
                        {
                            Marshal.Copy(array, 0, rawpointer, Size);
                            native = Marshal.PtrToStructure(rawpointer, Model);
                            tiers.TiersView.AddView(tiers.Put(ref native, false, true));
                            _rawcount++;
                        }
                    }
                    native = null;

                    tiers.ProgressCount += _rawcount;
                }

            rawcount = _rawcount;
            fromstream.Dispose();
            Marshal.FreeHGlobal(rawpointer);
            return tiers;
        }
    }

    public static class StreamMtxTiers
    {
        public static int SetMtx(this DataTiers tiers, Stream tostream, int offset = 0, int batchSize = 0, MemoryHandler memhandler = MemoryHandler.GC)
        {
            DataTiers objmatrix = tiers;
            int length = objmatrix.Count;
            Type[] types = tiers.Trell.Pylons.AsEnumerable().Select(t => t.DataType).ToArray();

            BinaryWriter binwriter = new BinaryWriter(tostream);
            int yLeft = length - offset;
            int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
            int xLength = (yLength > 0) ? types.Length : 0;
            int processed = 0;

            if (xLength > 0)
            {
                for (int y = offset; y < offset + yLength; y++)
                {
                    for (short x = 0; x < (xLength - 1); x++)
                    {
                        object objvalue = objmatrix[y][x];

                        byte[] rawvalue = null;
                        if (memhandler != MemoryHandler.Marshal)
                            rawvalue = SetGCPointer(objvalue, types[x]);
                        else
                            rawvalue = SetMarshalPointer(objvalue, types[x]);

                        if (rawvalue != null)
                            binwriter.Write(rawvalue);
                    }
                    processed++;
                }

                int nextoffset = (processed < yLeft) ? processed + offset : -1;

                if (nextoffset > 0)
                    tostream.Noise(1024, NoiseType.Block);
                else
                    tostream.Noise(1024, NoiseType.End);

                return nextoffset;
            }
            else
                return -1;
        }

        public static byte[] SetGCPointer(object objvalue, Type _type = null)
        {
            byte[] rawvalue = null;
            int rawsize = 0;
            Type type = (_type != null) ? _type : objvalue.GetType();
            IntPtr rawpointer = new IntPtr();

            if (type == typeof(string))
            {
                objvalue = (objvalue != null) ? objvalue : string.Empty;
                rawsize = objvalue.ToString().Length;
                short size = sizeof(short);
                rawvalue = new byte[rawsize + size];
                BitConverter.GetBytes((short)rawsize).CopyTo(rawvalue, 0);
                rawpointer = Marshal.StringToHGlobalAnsi(objvalue.ToString());
                Marshal.Copy(rawpointer, rawvalue, 2, rawsize);
                Marshal.FreeHGlobal(rawpointer);
            }
            else
            {
                if (type == typeof(DateTime))
                {
                    objvalue = (objvalue != null) ? ((DateTime)objvalue).Ticks : (long)0;
                    rawsize = sizeof(long);
                }
                else
                {
                    if (objvalue == DBNull.Value) objvalue = 0;
                    rawsize = Marshal.SizeOf(type);
                }

                rawvalue = new byte[rawsize];
                GCHandle handler = GCHandle.Alloc(rawvalue, GCHandleType.Pinned);
                rawpointer = handler.AddrOfPinnedObject();
                Marshal.StructureToPtr(objvalue, rawpointer, false);
                handler.Free();
            }

            return rawvalue;
        }
        public static byte[] SetMarshalPointer(object objvalue, Type _type = null)
        {
            byte[] rawvalue = null;
            int rawsize = 0;
            Type type = (_type != null) ? _type : objvalue.GetType();
            IntPtr rawpointer = new IntPtr();

            if (type == typeof(string))
            {
                objvalue = (objvalue != DBNull.Value) ? objvalue : string.Empty;
                rawsize = objvalue.ToString().Length;
                short size = sizeof(short);
                rawvalue = new byte[rawsize + size];
                BitConverter.GetBytes((short)rawsize).CopyTo(rawvalue, 0);
                rawpointer = Marshal.StringToHGlobalAnsi(objvalue.ToString());
                Marshal.Copy(rawpointer, rawvalue, 2, rawsize);
                Marshal.FreeHGlobal(rawpointer);
            }
            else
            {
                if (type == typeof(DateTime))
                {
                    objvalue = (objvalue != DBNull.Value) ? ((DateTime)objvalue).Ticks : (long)0;
                    rawsize = sizeof(long);
                }
                else
                {
                    if (objvalue == DBNull.Value) objvalue = 0;
                    rawsize = Marshal.SizeOf(type);
                }

                rawvalue = new byte[rawsize];
                rawpointer = Marshal.AllocHGlobal(rawsize);
                Marshal.StructureToPtr(objvalue, rawpointer, true);
                Marshal.Copy(rawpointer, rawvalue, 0, rawsize);
                Marshal.FreeHGlobal(rawpointer);
            }

            return rawvalue;
        }

        public static DataTiers GetMtx(this DataTiers tiers, Stream fromstream, out int rawcount, MemoryHandler memhandler = MemoryHandler.GC)
        {
            Type[] typearray = tiers.Trell.Pylons.AsEnumerable().Select(t => t.DataType).ToArray();
            BinaryReader binread = new BinaryReader(fromstream);
            int arraycount = typearray.Length;
            int int16Size = sizeof(Int16);
            int int64Size = sizeof(Int64);
            bool canRead = true;
            int _rawcount = 0;

            long endPosition = fromstream.Seek(0, SeekOrigin.End);
            if (fromstream.SeekNoise(SeekOrigin.End, SeekDirection.Backward, 0, 2048) != NoiseType.None)
                endPosition = fromstream.Position;

            fromstream.Position = 0;
            if (fromstream.SeekNoise(SeekOrigin.Begin, SeekDirection.Forward, 0, 1024) == NoiseType.None)
                fromstream.Position = 0;

            long leftToRead = endPosition - fromstream.Position;
            rawcount = _rawcount;
            if (arraycount > 0)
            {
                List<DataTier> list = new List<DataTier>();
                while (canRead)
                {
                    int bytecount = 0;
                    //object[] objitem = new object[arraycount];
                    DataTier tier = new DataTier(tiers.Trell);
                    for (int x = 0; x < arraycount; x++)
                    {
                        try
                        {
                            if (endPosition > fromstream.Position)
                            {
                                int objsize = 0;
                                if (leftToRead > 0)
                                {
                                    int addbytes = 0;
                                    bool isDate = false;
                                    if (typearray[x] == typeof(string))
                                    {
                                        objsize = binread.ReadInt16();
                                        addbytes = int16Size;
                                    }
                                    else if (typearray[x] == typeof(DateTime))
                                    {
                                        objsize = int64Size;
                                        isDate = true;
                                    }
                                    else
                                        objsize = Marshal.SizeOf(typearray[x]);

                                    if (objsize <= leftToRead)
                                    {
                                        int read = 0;
                                        byte[] rawvalue = new byte[objsize];
                                        read = binread.Read(rawvalue, 0, objsize) + addbytes;

                                        leftToRead -= read;
                                        bytecount += read;

                                        if (memhandler != MemoryHandler.Marshal)
                                            tier[x] = (!isDate) ? GetGCPointer(rawvalue, typearray[x]) :
                                                new DateTime((long)GetGCPointer(rawvalue, typearray[x]));
                                        else
                                            tier[x] = (!isDate) ? GetMarshalPointer(rawvalue, typearray[x]) :
                                                new DateTime((long)GetMarshalPointer(rawvalue, typearray[x]));
                                    }
                                    else
                                        canRead = false;
                                }
                                else
                                    canRead = false;
                            }
                            else
                                canRead = false;

                            if (!canRead) x = arraycount;
                        }
                        catch (Exception ex)
                        {
                            canRead = false;
                            x = arraycount;  // 4U2DO
                        }
                    }
                    if (canRead)
                    {
                        list.Add(tier);
                        _rawcount++;
                    }
                }
                lock (tiers.access)
                    tiers.PutRange(list);
                rawcount = _rawcount;
                return tiers;
            }
            else
                return null;
        }

        public static object GetGCPointer(byte[] rawvalue, Type objtype)
        {
            if (objtype == typeof(DateTime))
                objtype = typeof(long);

            int size = rawvalue.Length;
            GCHandle handler = GCHandle.Alloc(rawvalue, GCHandleType.Pinned);
            IntPtr rawpointer = handler.AddrOfPinnedObject();
            object objresult = (objtype != typeof(string))
                                ? Marshal.PtrToStructure(rawpointer, objtype) :
                                  Marshal.PtrToStringAnsi(rawpointer, rawvalue.Length);

            handler.Free();
            if (objtype == typeof(DateTime))
                return new DateTime((long)objresult);
            else
                return objresult;
        }
        public static object GetMarshalPointer(byte[] rawvalue, Type objtype)
        {
            if (objtype == typeof(DateTime))
                objtype = typeof(long);

            int size = rawvalue.Length;
            IntPtr rawpointer = Marshal.AllocHGlobal(size);
            Marshal.Copy(rawvalue, 0, rawpointer, size);

            object objresult = (objtype != typeof(string))
                                ? Marshal.PtrToStructure(rawpointer, objtype) :
                                  Marshal.PtrToStringAnsi(rawpointer, rawvalue.Length);

            Marshal.FreeHGlobal(rawpointer);
            if (objtype == typeof(DateTime))
                return new DateTime((long)objresult);
            else
                return objresult;
        }
    }

    public static class StreamTier
    {
        public static int SetRaw(this DataTier tier, Stream tostream)
        {
            BinaryWriter binwriter = new BinaryWriter(tostream);
            int Size = Marshal.SizeOf(tier.Model);
            byte[] item = new byte[Size];
            GCHandle handler = SetPointer(ref item);
            IntPtr ptr = handler.AddrOfPinnedObject();
            SetRaw(tier.n, ptr);
            binwriter.Write(item);
            handler.Free();
            return Size;
        }

        public static byte[] SetRawPointer(object s, int size)
        {
            byte[] array = new byte[size];
            /////////////// wersja GC handler ////////////////
            GCHandle handler = GCHandle.Alloc(array, GCHandleType.Pinned);
            // Get mem address pointer to pinned bytes 
            IntPtr rawpointer = handler.AddrOfPinnedObject();
            // Get bytes from structure located in mem address directed by pointer  
            Marshal.StructureToPtr(s, rawpointer, true);
            // Free used unallocated mem

            handler.Free();
            ////////////// werjsa Marshal ///////////////
            //IntPtr rawpointer = Marshal.AllocHGlobal(size);
            // Marshal.StructureToPtr(s, rawpointer, false);
            // Marshal.Copy(rawpointer, array, 0, size);
            //Marshal.FreeHGlobal(rawpointer);
            return array;
        }

        public static GCHandle SetPointer(ref byte[] array)
        {
            GCHandle handler = GCHandle.Alloc(array, GCHandleType.Pinned);
            return handler;
        }
        public static void SetRaw(object s, IntPtr ptr)
        {
            Marshal.StructureToPtr(s, ptr, true);
        }

        public static DataTier GetRaw(this DataTier tier, Stream fromstream)
        {
            int Size = Marshal.SizeOf(tier.Model);
            Type Model = tier.Model;
            long endPosition = fromstream.Seek(0, SeekOrigin.End);
            if (fromstream.SeekNoise(SeekOrigin.End, SeekDirection.Backward, 0, 2048) != NoiseType.None)
                endPosition = fromstream.Position;

            fromstream.Position = 0;
            if (fromstream.SeekNoise(SeekOrigin.Begin, SeekDirection.Forward, 0, 1024) == NoiseType.None)
                fromstream.Position = 0;

            long leftToRead = endPosition;

            List<DataTier> toUpdate = new List<DataTier>();

            byte[] array = new byte[Size]; int read = 0;
            IntPtr rawpointer = Marshal.AllocHGlobal(Size);
            object n = null;
            while ((read = fromstream.Read(array, 0, array.Length)) > 0)
            {
                if (endPosition >= fromstream.Position)
                {
                    Marshal.Copy(array, 0, rawpointer, Size);
                    n = Marshal.PtrToStructure(rawpointer, Model);
                    tier = new DataTier(tier.Trell, ref n);                    
                }
            }
            n = null;
            fromstream.Dispose();
            return tier;
        }
    }

    public static class StreamSimTier
    {
        public static int SetSim(this DataTier tier, Stream tostream)
        {
            BinaryWriter binwriter = new BinaryWriter(tostream);
            int Size = Marshal.SizeOf(tier.Model);
            byte[] item = new byte[Size];
            GCHandle handler = SetPointer(ref item);
            IntPtr ptr = handler.AddrOfPinnedObject();
            DataTier single = new DataTier(tier.Trell);
            single.PrimeArray = tier.DataArray;
            SetRaw(single.n, ptr);
            binwriter.Write(item);
            handler.Free();
            return Size;
        }

        public static byte[] SetRawPointer(object s, int size)
        {
            byte[] array = new byte[size];
            /////////////// wersja GC handler ////////////////
            GCHandle handler = GCHandle.Alloc(array, GCHandleType.Pinned);
            // Get mem address pointer to pinned bytes 
            IntPtr rawpointer = handler.AddrOfPinnedObject();
            // Get bytes from structure located in mem address directed by pointer  
            Marshal.StructureToPtr(s, rawpointer, true);
            // Free used unallocated mem

            handler.Free();
            ////////////// werjsa Marshal ///////////////
            //IntPtr rawpointer = Marshal.AllocHGlobal(size);
            // Marshal.StructureToPtr(s, rawpointer, false);
            // Marshal.Copy(rawpointer, array, 0, size);
            //Marshal.FreeHGlobal(rawpointer);
            return array;
        }

        public static GCHandle SetPointer(ref byte[] array)
        {
            GCHandle handler = GCHandle.Alloc(array, GCHandleType.Pinned);
            return handler;
        }
        public static void SetRaw(object s, IntPtr ptr)
        {
            Marshal.StructureToPtr(s, ptr, true);
        }
    }

    public static class StreamMtxTier
    {
        public static int SetMtx(this DataTier tier, Stream tostream, MemoryHandler memhandler = MemoryHandler.GC)
        {
            DataTier objmatrix = tier;
            Type[] types = tier.Trell.Pylons.AsEnumerable().Select(t => t.DataType).ToArray();

            BinaryWriter binwriter = new BinaryWriter(tostream);
            int xLength = types.Length;
            int written = 0;

            if (xLength > 0)
            {
                for (short x = 0; x < (xLength - 1); x++)
                {
                    object objvalue = objmatrix[x];

                    byte[] rawvalue = null;
                    if (memhandler != MemoryHandler.Marshal)
                        rawvalue = SetGCPointer(objvalue, types[x]);
                    else
                        rawvalue = SetMarshalPointer(objvalue, types[x]);

                    if (rawvalue != null)
                    {
                        binwriter.Write(rawvalue);
                        written += rawvalue.Length;
                    }
                }

                return written;
            }
            else
                return -1;
        }

        public static byte[] SetGCPointer(object objvalue, Type _type = null)
        {
            byte[] rawvalue = null;
            int rawsize = 0;
            Type type = (_type != null) ? _type : objvalue.GetType();
            IntPtr rawpointer = new IntPtr();

            if (type == typeof(string))
            {
                objvalue = (objvalue != null) ? objvalue : string.Empty;
                rawsize = objvalue.ToString().Length;
                short size = sizeof(short);
                rawvalue = new byte[rawsize + size];
                BitConverter.GetBytes((short)rawsize).CopyTo(rawvalue, 0);
                rawpointer = Marshal.StringToHGlobalAnsi(objvalue.ToString());
                Marshal.Copy(rawpointer, rawvalue, 2, rawsize);
                Marshal.FreeHGlobal(rawpointer);
            }
            else
            {
                if (type == typeof(DateTime))
                {
                    objvalue = (objvalue != null) ? ((DateTime)objvalue).Ticks : (long)0;
                    rawsize = sizeof(long);
                }
                else
                {
                    if (objvalue == DBNull.Value) objvalue = 0;
                    rawsize = Marshal.SizeOf(type);
                }

                rawvalue = new byte[rawsize];
                GCHandle handler = GCHandle.Alloc(rawvalue, GCHandleType.Pinned);
                rawpointer = handler.AddrOfPinnedObject();
                Marshal.StructureToPtr(objvalue, rawpointer, false);
                handler.Free();
            }

            return rawvalue;
        }
        public static byte[] SetMarshalPointer(object objvalue, Type _type = null)
        {
            byte[] rawvalue = null;
            int rawsize = 0;
            Type type = (_type != null) ? _type : objvalue.GetType();
            IntPtr rawpointer = new IntPtr();

            if (type == typeof(string))
            {
                objvalue = (objvalue != DBNull.Value) ? objvalue : string.Empty;
                rawsize = objvalue.ToString().Length;
                short size = sizeof(short);
                rawvalue = new byte[rawsize + size];
                BitConverter.GetBytes((short)rawsize).CopyTo(rawvalue, 0);
                rawpointer = Marshal.StringToHGlobalAnsi(objvalue.ToString());
                Marshal.Copy(rawpointer, rawvalue, 2, rawsize);
                Marshal.FreeHGlobal(rawpointer);
            }
            else
            {
                if (type == typeof(DateTime))
                {
                    objvalue = (objvalue != DBNull.Value) ? ((DateTime)objvalue).Ticks : (long)0;
                    rawsize = sizeof(long);
                }
                else
                {
                    if (objvalue == DBNull.Value) objvalue = 0;
                    rawsize = Marshal.SizeOf(type);
                }

                rawvalue = new byte[rawsize];
                rawpointer = Marshal.AllocHGlobal(rawsize);
                Marshal.StructureToPtr(objvalue, rawpointer, true);
                Marshal.Copy(rawpointer, rawvalue, 0, rawsize);
                Marshal.FreeHGlobal(rawpointer);
            }

            return rawvalue;
        }

        public static DataTier GetMtx(this DataTier tier, Stream fromstream, MemoryHandler memhandler = MemoryHandler.GC)
        {
            Type[] typearray = tier.Trell.Pylons.AsEnumerable().Select(t => t.DataType).ToArray();
            BinaryReader binread = new BinaryReader(fromstream);
            int arraycount = typearray.Length;
            int int16Size = sizeof(Int16);
            int int64Size = sizeof(Int64);
            bool canRead = true;

            long endPosition = fromstream.Seek(0, SeekOrigin.End);
            long leftToRead = endPosition;
            fromstream.Position = 0;

            if (arraycount > 0)
            {
                while (canRead)
                {
                    int bytecount = 0;
                    object[] objitem = new object[arraycount];

                    for (int x = 0; x < arraycount; x++)
                    {
                        try
                        {
                            if (endPosition > fromstream.Position)
                            {
                                int objsize = 0;
                                if (leftToRead > 0)
                                {
                                    int addbytes = 0;
                                    bool isDate = false;
                                    if (typearray[x] == typeof(string))
                                    {
                                        objsize = binread.ReadInt16();
                                        addbytes = int16Size;
                                    }
                                    else if (typearray[x] == typeof(DateTime))
                                    {
                                        objsize = int64Size;
                                        isDate = true;
                                    }
                                    else
                                        objsize = Marshal.SizeOf(typearray[x]);

                                    if (objsize <= leftToRead)
                                    {
                                        int read = 0;
                                        byte[] rawvalue = new byte[objsize];
                                        read = binread.Read(rawvalue, 0, objsize) + addbytes;

                                        leftToRead -= read;
                                        bytecount += read;

                                        if (memhandler != MemoryHandler.Marshal)
                                            objitem[x] = (!isDate) ? GetGCPointer(rawvalue, typearray[x]) :
                                                new DateTime((long)GetGCPointer(rawvalue, typearray[x]));
                                        else
                                            objitem[x] = (!isDate) ? GetMarshalPointer(rawvalue, typearray[x]) :
                                                new DateTime((long)GetMarshalPointer(rawvalue, typearray[x]));
                                    }
                                    else
                                        canRead = false;
                                }
                                else
                                    canRead = false;
                            }
                            else
                                canRead = false;

                            if (!canRead) x = arraycount;
                        }
                        catch (Exception ex)
                        {
                            canRead = false;
                            x = arraycount;  // 4U2DO
                        }
                    }
                    if (canRead)
                    {
                        tier.DataArray = objitem;
                    }
                }
                return tier;
            }
            else
                return
                    null;
        }

        public static object GetGCPointer(byte[] rawvalue, Type objtype)
        {
            if (objtype == typeof(DateTime))
                objtype = typeof(long);

            int size = rawvalue.Length;
            GCHandle handler = GCHandle.Alloc(rawvalue, GCHandleType.Pinned);
            IntPtr rawpointer = handler.AddrOfPinnedObject();
            object objresult = (objtype != typeof(string))
                                ? Marshal.PtrToStructure(rawpointer, objtype) :
                                  Marshal.PtrToStringAnsi(rawpointer, rawvalue.Length);

            handler.Free();
            if (objtype == typeof(DateTime))
                return new DateTime((long)objresult);
            else
                return objresult;
        }
        public static object GetMarshalPointer(byte[] rawvalue, Type objtype)
        {
            if (objtype == typeof(DateTime))
                objtype = typeof(long);

            int size = rawvalue.Length;
            IntPtr rawpointer = Marshal.AllocHGlobal(size);
            Marshal.Copy(rawvalue, 0, rawpointer, size);

            object objresult = (objtype != typeof(string))
                                ? Marshal.PtrToStructure(rawpointer, objtype) :
                                  Marshal.PtrToStringAnsi(rawpointer, rawvalue.Length);

            Marshal.FreeHGlobal(rawpointer);
            if (objtype == typeof(DateTime))
                return new DateTime((long)objresult);
            else
                return objresult;
        }
    }
    #endregion
}





