using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Dors;
using System.Dors.Drive;

namespace System.Dors.Data
{
    #region Headers - BinaryFormatter
    public static class RawBank
    {
        public static int SetRaw(this DataVaults bank, ISerialContext tostream)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(new byte[16], 0, 16);
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(ms, bank);
            tostream.SerialBlock = ms.ToArray();
            ms.Dispose();
            return tostream.SerialBlock.Length;
        }
        public static DataVaults GetRaw(this DataVaults bank, ref object fromarray)
        {
            try
            {
                MemoryStream ms = new MemoryStream((byte[])fromarray);
                BinaryFormatter binform = new BinaryFormatter();
                DataVaults _bank = (DataVaults)binform.Deserialize(ms);
                ms.Dispose();             
                return _bank;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class RawVault
    {
        public static int SetRaw(this DataVault vault, ISerialContext tostream)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(new byte[16], 0, 16);
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(ms, vault);
            tostream.SerialBlock = ms.ToArray();
            ms.Dispose();
            return tostream.SerialBlock.Length;
        }
        public static DataVault GetRaw(this DataVault vault, ref object fromarray)
        {
            try
            {
                MemoryStream ms = new MemoryStream((byte[])fromarray);
                BinaryFormatter binform = new BinaryFormatter();
                DataVault _vault = (DataVault)binform.Deserialize(ms);
                ms.Dispose();
                return _vault;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class RawDeposit
    {
        public static int SetRaw(this DataDeposit vault, ISerialContext tostream)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(new byte[16], 0, 16);
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(ms, vault);
            tostream.SerialBlock = ms.ToArray();
            ms.Dispose();
            return tostream.SerialBlock.Length;
        }
        public static DataDeposit GetRaw(this DataDeposit vault, ref object fromarray)
        {
            try
            {
                MemoryStream ms = new MemoryStream((byte[])fromarray);
                BinaryFormatter binform = new BinaryFormatter();
                DataDeposit _vault = (DataDeposit)binform.Deserialize(ms);
                ms.Dispose();
                return _vault;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class RawBox
    {
        public static int SetRaw(this DataBox box, ISerialContext tostream)
        {
            box.Prime.SetRaw(tostream);
            return tostream.SerialBlock.Length;
        }
        public static DataTrellis GetRaw(this DataBox box, ref object fromarray)
        {
            try
            {
                if (box.Prime.GetType() == typeof(DataTrellis))
                {
                    MemoryStream ms = new MemoryStream((byte[])fromarray);
                    BinaryFormatter binform = new BinaryFormatter();
                    DataTrellis _vault = (DataTrellis)binform.Deserialize(ms);
                    ms.Dispose();
                    return _vault;
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

    public static class RawSpace
    {
        public static int SetRaw(this DataArea vault, ISerialContext tostream)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(new byte[16], 0, 16);
            BinaryFormatter binform = new BinaryFormatter();            
            binform.Serialize(ms, vault);
            tostream.SerialBlock = ms.ToArray();
            ms.Dispose();
            return tostream.SerialBlock.Length;
        }
        public static DataArea GetRaw(this DataArea vault, ref object fromarray)
        {
            try
            {
                MemoryStream ms = new MemoryStream((byte[])fromarray);
                BinaryFormatter binform = new BinaryFormatter();
                DataArea _vault = (DataArea)binform.Deserialize(ms);
                ms.Dispose();
                return _vault;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class RawSpheres
    {
        public static int SetRaw(this DataSpheres vault, ISerialContext tostream)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(new byte[16], 0, 16);
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(ms, vault);
            tostream.SerialBlock = ms.ToArray();
            ms.Dispose();
            return tostream.SerialBlock.Length;
        }
        public static DataSpheres GetRaw(this DataSpheres vault, ref object fromarray)
        {
            try
            {
                MemoryStream ms = new MemoryStream((byte[])fromarray);
                BinaryFormatter binform = new BinaryFormatter();
                DataSpheres _vault = (DataSpheres)binform.Deserialize(ms);
                ms.Dispose();
                return _vault;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class RawSphere
    {
        public static int SetRaw(this DataSphere vault, ISerialContext tostream)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(new byte[16], 0, 16);
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(ms, vault);
            tostream.SerialBlock = ms.ToArray();
            ms.Dispose();
            return tostream.SerialBlock.Length;
        }
        public static DataSphere GetRaw(this DataSphere vault, ref object fromarray)
        {
            try
            {
                MemoryStream ms = new MemoryStream((byte[])fromarray);
                BinaryFormatter binform = new BinaryFormatter();
                DataSphere _vault = (DataSphere)binform.Deserialize(ms);
                ms.Dispose();
                return _vault;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class RawTrellises
    {
        public static int SetRaw(this DataTrellises vault, ISerialContext tostream)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(new byte[16], 0, 16);
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(ms, vault);
            tostream.SerialBlock = ms.ToArray();
            ms.Dispose();
            return tostream.SerialBlock.Length;
        }
        public static DataTrellises GetRaw(this DataTrellises vault, ref object fromarray)
        {
            try
            {
                MemoryStream ms = new MemoryStream((byte[])fromarray);
                BinaryFormatter binform = new BinaryFormatter();
                DataTrellises _vault = (DataTrellises)binform.Deserialize(ms);
                ms.Dispose();
                return _vault;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class RawTrellis
    {
        public static int SetRaw(this DataTrellis vault, ISerialContext tostream)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(new byte[16], 0, 16);
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(ms, vault);
            tostream.SerialBlock = ms.ToArray();
            ms.Dispose();
            return tostream.SerialBlock.Length;
        }
        public static DataTrellis GetRaw(this DataTrellis vault, ref object fromarray)
        {
            try
            {
                MemoryStream ms = new MemoryStream((byte[])fromarray);
                BinaryFormatter binform = new BinaryFormatter();
                DataTrellis _vault = (DataTrellis)binform.Deserialize(ms);
                ms.Dispose();
                return _vault;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class RawFilter
    {
        public static int SetRaw(this FilterTerms filterTerms, ISerialContext tostream)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(new byte[16], 0, 16);
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(ms, filterTerms);
            tostream.SerialBlock = ms.ToArray();
            ms.Dispose();
            return tostream.SerialBlock.Length;
        }
        public static FilterTerms GetRaw(this FilterTerms filterTerms, ref object fromarray)
        {
            try
            {
                MemoryStream ms = new MemoryStream((byte[])fromarray);
                BinaryFormatter binform = new BinaryFormatter();
                FilterTerms _vault = (FilterTerms)binform.Deserialize(ms);
                ms.Dispose();
                return _vault;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class RawFavorites
    {
        public static int SetRaw(this DataFavorites filterTerms, ISerialContext tostream)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(new byte[16], 0, 16);
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(ms, filterTerms);
            tostream.SerialBlock = ms.ToArray();
            ms.Dispose();
            return tostream.SerialBlock.Length;
        }
        public static DataFavorites GetRaw(this DataFavorites filterTerms, ref object fromarray)
        {
            try
            {
                MemoryStream ms = new MemoryStream((byte[])fromarray);
                BinaryFormatter binform = new BinaryFormatter();
                DataFavorites _vault = (DataFavorites)binform.Deserialize(ms);
                ms.Dispose();
                return _vault;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
    #endregion

    #region Message - Tiers Fast Structures
    public static class SimTiers
    {
        public static int SetSim(this DataTiers tiers, ISerialContext buffor, int offset, int batchSize)
        {
            int Size = Marshal.SizeOf(tiers.Trell.Model);
            int length = tiers.Count;
            int yLeft = length - offset;
            int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
            int startbytes = 16;
            int processed = 0;
            int byteoffset = 0;
            int bytesize = Size;
            buffor.SerialBlock = new byte[(yLength * (int)Size) + startbytes];
            DataTier singleton = new DataTier(tiers.Trell);
            GCHandle handler = GCHandle.Alloc(buffor.SerialBlock, GCHandleType.Pinned);
            IntPtr ptr = handler.AddrOfPinnedObject();
            DepotSite site = buffor.IdentitySite;
            for (int y = offset; y < offset + yLength; y++)
            {
                byteoffset = (processed * bytesize) + startbytes;
                DataTier temp = tiers[y];
                temp.DataArray.Select((d, o) => singleton.iN[o] = d).ToArray();

                if (site == DepotSite.Server)
                    temp.State.Synced = true;

                singleton.State.Impact(temp.State);
                singleton.SetStateNoid();
                singleton.SetClockNoid();
                Marshal.StructureToPtr(singleton.n, ptr + byteoffset, true);

                if (site == DepotSite.Server)
                    temp.State.Synced = false;

                processed++;
            }
            if (site == DepotSite.Server)
                tiers.Trell.State.ClearState();
            singleton = null;
            handler.Free();
            int nextoffset = (processed < yLeft) ? processed + offset : -1;
            int noiseCount = (buffor.SerialBlock.Length > 4096) ? 1024 : 4096;
            if (nextoffset > 0)
                buffor.SerialBlock = buffor.SerialBlock.Noise(noiseCount, NoiseType.Block);
            else
                buffor.SerialBlock = buffor.SerialBlock.Noise(noiseCount, NoiseType.End);
            return nextoffset;
        }
        public static int SetSim(this DataTier[] tiers, ISerialContext buffor, int offset, int batchSize)
        {
            int Size = Marshal.SizeOf(tiers[0].Trell.Model);
            int length = tiers.Length;
            int yLeft = length - offset;
            int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
            int startbytes = 16;
            int processed = 0;
            int byteoffset = 0;
            int bytesize = Size;
            buffor.SerialBlock = new byte[(yLength * (int)Size) + startbytes];
            DataTier singleton = new DataTier(tiers[0].Trell);
            GCHandle handler = GCHandle.Alloc(buffor.SerialBlock, GCHandleType.Pinned);
            IntPtr ptr = handler.AddrOfPinnedObject();
            DepotSite site = buffor.IdentitySite;
            for (int y = offset; y < offset + yLength; y++)
            {
                byteoffset = (processed * bytesize) + startbytes;
                DataTier temp = tiers[y];
                temp.DataArray.Select((d,o) => singleton.iN[o] = d).ToArray();

                if (site == DepotSite.Server)
                    temp.State.Synced = true;
                
                singleton.State.Impact(temp.State);
                singleton.SetStateNoid();
                singleton.SetClockNoid();
                Marshal.StructureToPtr(singleton.n, ptr + byteoffset, true);

                if (site == DepotSite.Server)
                    temp.State.Synced = false;

                processed++;
            }
            singleton = null;
            handler.Free();
            int nextoffset = (processed < yLeft) ? processed + offset : -1;
            int noiseCount = (buffor.SerialBlock.Length > 4096) ? 1024 : 4096;
            if (nextoffset > 0)
                buffor.SerialBlock = buffor.SerialBlock.Noise(noiseCount, NoiseType.Block);
            else
                buffor.SerialBlock = buffor.SerialBlock.Noise(noiseCount, NoiseType.End);
            return nextoffset;
        }
      
        public static void SetRaw(ref object s, IntPtr ptr)
        {
            Marshal.StructureToPtr(s, ptr, true);
        }
    }   

    public static class RawTiers
    {
        public static int SetRaw(this DataTiers tiers, ISerialContext buffor, int offset, int batchSize)
        {         

            int Size = Marshal.SizeOf(tiers.Trell.Model);
            int length = tiers.Count;
            int yLeft = length - offset;
            int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
            int startbytes = 16;
            int processed = 0;
            int byteoffset = 0;
            int bytesize = Size;
            buffor.SerialBlock = new byte[(yLength * (int)Size) + startbytes];
            GCHandle handler = GCHandle.Alloc(buffor.SerialBlock, GCHandleType.Pinned);
            IntPtr ptr = handler.AddrOfPinnedObject();
            DepotSite site = buffor.IdentitySite;
            for (int y = offset; y < offset + yLength; y++)
            {
                DataTier temp = tiers[y];

                if (site == DepotSite.Server)
                    temp.State.Synced = true;

                temp.SetStateNoid();
                temp.SetClockNoid();
                byteoffset = (processed * bytesize) + startbytes;
                Marshal.StructureToPtr(temp.Prime.n, ptr + byteoffset, true);

                if (site == DepotSite.Server)
                    temp.State.Synced = false;

                processed++;
            }
            if (site == DepotSite.Server)
                tiers.Trell.State.ClearState();
            handler.Free();
            int nextoffset = (processed < yLeft) ? processed + offset : -1;
            int noiseCount = (buffor.SerialBlock.Length > 4096) ? 1024 : 4096;
            if (nextoffset > 0)
                buffor.SerialBlock = buffor.SerialBlock.Noise(noiseCount, NoiseType.Block);
            else
                buffor.SerialBlock = buffor.SerialBlock.Noise(noiseCount, NoiseType.End);
            return nextoffset;
        }
        public static int SetRaw(this DataTier[] tiers, ISerialContext buffor, int offset, int batchSize)
        {
            int Size = Marshal.SizeOf(tiers[0].Trell.Model);
            int length = tiers.Length;
            int yLeft = length - offset;
            int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
            int startbytes = 16;
            int processed = 0;
            int byteoffset = 0;
            int bytesize = Size;
            buffor.SerialBlock = new byte[(yLength * (int)Size) + startbytes];
            GCHandle handler = GCHandle.Alloc(buffor.SerialBlock, GCHandleType.Pinned);
            DepotSite site = buffor.IdentitySite;
            IntPtr ptr = handler.AddrOfPinnedObject();

            for (int y = offset; y < offset + yLength; y++)
            {
                DataTier temp = tiers[y];

                if (site == DepotSite.Server)                
                    temp.State.Synced = true;
                
                temp.SetStateNoid();
                temp.SetClockNoid();
                byteoffset = (processed * bytesize) + startbytes;
                Marshal.StructureToPtr(temp.Prime.n, ptr + byteoffset, true);

                if (site == DepotSite.Server)
                    temp.State.Synced = false;

                processed++;
            }
            handler.Free();
            int nextoffset = (processed < yLeft) ? processed + offset : -1;
            int noiseCount = (buffor.SerialBlock.Length > 4096) ? 1024 : 4096;
            if (nextoffset > 0)
                buffor.SerialBlock = buffor.SerialBlock.Noise(noiseCount, NoiseType.Block);
            else
                buffor.SerialBlock = buffor.SerialBlock.Noise(noiseCount, NoiseType.End);
            return nextoffset;
        }
      
        public static void SetRaw(ref object s, IntPtr ptr)
        {
            Marshal.StructureToPtr(s, ptr, true);
        }

        public static DataTiers GetRaw(this DataTiers tiers, ref object fromarray, out int rawcount)
        {
            byte[] _fromarray = (byte[])fromarray;
            int noiseCount = (_fromarray.Length > 4096) ? 1024 : 4096;

            Type Model = tiers.Trell.Model;
            int Size = Marshal.SizeOf(Model);

            bool save = false;
            if (tiers.State.Saved && !tiers.State.Synced && (tiers.State.Edited))
                save = true;

            long endPosition = _fromarray.Length;
            long endTempPosition = 0;

            NoiseType endNoise = _fromarray.SeekNoise(out endTempPosition, SeekDirection.Backward, 0, noiseCount);
            if (endNoise != NoiseType.None)
                endPosition = endTempPosition;

            long startPosition = 0;
            long startTempPosition = 0;

            NoiseType startNoise = _fromarray.SeekNoise(out startTempPosition, SeekDirection.Forward, 0, noiseCount);
            if (startNoise != NoiseType.None)
                startPosition = startTempPosition;

            int _rawcount = 0;
            long leftToRead = endPosition - startPosition;
            int length = (int)(leftToRead / Size);
            byte[] array = new byte[Size];

            GCHandle handler = GCHandle.Alloc(fromarray, GCHandleType.Pinned);
            IntPtr ptr = handler.AddrOfPinnedObject();
            int noidOrdinal = tiers.Trell.NoidOrdinal;
            lock (tiers.access)
                lock (tiers.TiersView.access)
                {
                    if (length > 0)
                    {
                        if (tiers.ProgressCount == 0)
                            tiers.TiersView.ClearJoins();

                        int noidOrd = tiers.Trell.NoidOrdinal;
                        object native = null;
                        for (int i = 0; i < length; i++)
                        {
                            int position = (int)(startPosition + (i * Size));
                            if (endPosition >= position + Size)
                            {
                                native = Marshal.PtrToStructure(ptr + position, Model);
                                Noid noid = (Noid)((IDataNative)native)[noidOrdinal];

                                DataTier tier = tiers.PutInner
                                      (
                                          ref native,
                                          save,
                                          true
                                      );

                                tier.State.FromBits(noid);
                                tier.State.FromBitClock(noid);
                                _rawcount++;
                            }
                        }
                        native = null;
                        tiers.ProgressCount += _rawcount;
                    }
                }

            handler.Free();
            fromarray = null;
            _fromarray = null;
            rawcount = _rawcount;
            return tiers;
        }

        public static DataTiers GetView(this DataTiers tiers, ref object fromarray, out int rawcount)
        {
            byte[] _fromarray = (byte[])fromarray;
            int noiseCount = (_fromarray.Length > 4096) ? 1024 : 4096;
            Type Model = tiers.Trell.Model;
            int Size = Marshal.SizeOf(Model);
            long endPosition = _fromarray.Length;
            long endTempPosition = 0;

            NoiseType endNoise = _fromarray.SeekNoise(out endTempPosition, SeekDirection.Backward, 0, noiseCount);
            if (endNoise != NoiseType.None)
                endPosition = endTempPosition;

            long startPosition = 0;
            long startTempPosition = 0;

            NoiseType startNoise = _fromarray.SeekNoise(out startTempPosition, SeekDirection.Forward, 0, noiseCount);
            if (startNoise != NoiseType.None)
                startPosition = startTempPosition;

            int _rawcount = 0;

            long leftToRead = endPosition - startPosition;
            int length = (int)(leftToRead / Size);
            byte[] array = new byte[Size];

            GCHandle handler = GCHandle.Alloc(fromarray, GCHandleType.Pinned);
            IntPtr ptr = handler.AddrOfPinnedObject();

            lock (tiers.access)
                lock (tiers.TiersView.access)
                {
                    if (length > 0)
                    {
                        if (tiers.ProgressCount == 0)
                        {
                            tiers.TiersView.Clear();
                            tiers.TiersView.ClearJoins();
                        }
                        object native = null;
                        Noid noid;
                        for (int i = 0; i < length; i++)
                        {
                            int position = (int)(startPosition + (i * Size));
                            if (endPosition >= position + Size)
                            {
                                native = Marshal.PtrToStructure(ptr + position, Model);
                                noid = (Noid)((IDataNative)native)[tiers.Trell.NoidOrdinal];

                                DataTier tier = tiers.PutInner
                                      (
                                          ref native,
                                          false,
                                          false
                                      );
                                tier.State.FromBits(noid);
                                tier.State.FromBitClock(noid);
                                tiers.TiersView.AddView(tier);
                                _rawcount++;
                            }
                        }
                        native = null;
                        tiers.ProgressCount += _rawcount;
                    }
                }

            handler.Free();
            fromarray = null;
            _fromarray = null;
            rawcount = _rawcount;
            return tiers;
        }
    }

    public static class ModTiers
    {      
        public static int SetMod(this DataTiers tiers, ISerialContext buffor, int offset = 0, int batchSize = 0, MemoryHandler memhandler = MemoryHandler.Marshal)
        {

            Type[] types = tiers.Trell.Pylons.TypeArray;
            DataTier[] objmatrix = tiers.AsArray().Where(t => t.State.Edited).ToArray();
            int noidord = tiers.Trell.NoidOrdinal;
            int length = objmatrix.Length;
            int yLeft = length - offset;
            int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
            ArrayList bytearray = new ArrayList();
            bytearray.AddRange(new byte[16]);
            int processed = 0;
            int written = 16;
            for (int y = offset; y < offset + yLength; y++)
            {
                if (buffor.IdentitySite == DepotSite.Server)
                    objmatrix[y].State.Synced = true;                

                //SortedList<int, object> mtxgrid = objmatrix[y].Grating.changes;
                HashList<object> mtxgrid = objmatrix[y].Grating.changes;
                Noid noid = objmatrix[y].Noid();
                objmatrix[y].State.ToBits(noid);
                objmatrix[y].State.ToBitClock(noid);
                bytearray.AddRange(noid.GetBytes());
                bytearray.AddRange(BitConverter.GetBytes(mtxgrid.Count));
                written += 28;
                foreach (Vessel<object> mtx in mtxgrid)
                {
                    object objvalue = mtx.Value;
                    long x = mtx.Key;
                    byte[] rawvalue = null;
                    if (memhandler != MemoryHandler.Marshal)  
                        rawvalue = SetGCPointer(objvalue, (int)x, types[x]);
                    else
                        rawvalue = SetMarshalPointer(objvalue, (int)x, types[x]);

                    if (rawvalue != null)
                    {
                        bytearray.AddRange(rawvalue);
                        written += rawvalue.Length;
                    }
                }

                if (buffor.IdentitySite == DepotSite.Server)
                    objmatrix[y].State.Synced = false;

                processed++;
            }
            if (buffor.IdentitySite == DepotSite.Server)
                tiers.Trell.State.ClearState();
            int nextoffset = (processed < yLeft) ? processed + offset : -1;
            int noiseCount = (bytearray.Count > 4096) ? 1024 : 4096;
            if (nextoffset > 0)
                buffor.SerialBlock = ((byte[])bytearray.ToArray(typeof(byte))).Noise(noiseCount, NoiseType.Block);
            else
                buffor.SerialBlock = ((byte[])bytearray.ToArray(typeof(byte))).Noise(noiseCount, NoiseType.End);
            return nextoffset;
        }
        public static int SetMod(this DataTier[] tiers, ISerialContext buffor, int offset = 0, int batchSize = 0, MemoryHandler memhandler = MemoryHandler.Marshal)
        {

            Type[] types = tiers[0].Trell.Pylons.TypeArray;
            DataTier[] objmatrix = tiers.Where(t => t.State.Edited).ToArray();
            int length = objmatrix.Length;
            int yLeft = length - offset;
            int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
            ArrayList bytearray = new ArrayList();
            bytearray.AddRange(new byte[16]);
            int processed = 0;
            int written = 16;
            for (int y = offset; y < offset + yLength; y++)
            {
                if (buffor.IdentitySite == DepotSite.Server)
                    objmatrix[y].State.Synced = true;

                // SortedList<int, object> mtxgrid = objmatrix[y].Grating.changes;
                HashList<object> mtxgrid = objmatrix[y].Grating.changes;
                Noid noid = objmatrix[y].Noid();
                objmatrix[y].State.ToBits(noid);
                objmatrix[y].State.ToBitClock(noid);
                bytearray.AddRange(noid.GetBytes());
                bytearray.AddRange(BitConverter.GetBytes(mtxgrid.Count));
                written += 28;
                foreach (Vessel<object> mtx in mtxgrid)
                {
                    object objvalue = mtx.Value;
                    long x = mtx.Key;
                    byte[] rawvalue = null;
                    if (memhandler != MemoryHandler.Marshal)
                        rawvalue = SetGCPointer(objvalue, (int)x, types[x]);
                    else
                        rawvalue = SetMarshalPointer(objvalue, (int)x, types[x]);

                    if (rawvalue != null)
                    {
                        bytearray.AddRange(rawvalue);
                        written += rawvalue.Length;
                    }
                }

                if (buffor.IdentitySite == DepotSite.Server)
                    objmatrix[y].State.Synced = false;

                processed++;
            }
            int nextoffset = (processed < yLeft) ? processed + offset : -1;
            int noiseCount = (bytearray.Count > 4096) ? 1024 : 4096;
            if (nextoffset > 0)
                buffor.SerialBlock = ((byte[])bytearray.ToArray(typeof(byte))).Noise(noiseCount, NoiseType.Block);
            else
                buffor.SerialBlock = ((byte[])bytearray.ToArray(typeof(byte))).Noise(noiseCount, NoiseType.End);
            return nextoffset;
        }

        public static byte[] SetGCPointer(object objvalue, int order, Type _type = null)
        {
            byte[] rawvalue = null;
            int rawsize = 0;
            Type type = (_type != null) ? _type : objvalue.GetType();
            if (objvalue.GetType() != type)
                objvalue = Convert.ChangeType(objvalue, type);

            IntPtr rawpointer = new IntPtr();

            if (type == typeof(string))
            {
                objvalue = (objvalue != null) ? objvalue : string.Empty;
                rawsize = objvalue.ToString().Length;
                short size = sizeof(short) * 2;                
                rawvalue = new byte[rawsize + size];
                BitConverter.GetBytes((short)order).CopyTo(rawvalue, 0);
                BitConverter.GetBytes((short)rawsize).CopyTo(rawvalue, 2);
                rawpointer = Marshal.StringToHGlobalAnsi(objvalue.ToString());
                Marshal.Copy(rawpointer, rawvalue, 4, rawsize);
                Marshal.FreeHGlobal(rawpointer);
            }
            else
            {
                if (type == typeof(DateTime))
                {
                    objvalue = (objvalue != null) ? ((DateTime)objvalue).Ticks : (long)0;
                    rawsize = sizeof(long) + 2;
                }
                else if (type == typeof(byte[]))
                {
                    if (objvalue == DBNull.Value) objvalue = new byte[24];
                    rawsize = 24 + 2;
                    rawvalue = new byte[rawsize];
                    BitConverter.GetBytes((short)order).CopyTo(rawvalue, 0);
                    ((byte[])objvalue).CopyTo(rawvalue, 2);
                    return rawvalue;
                }
                else
                {
                    if (objvalue == DBNull.Value) objvalue = 0;
                    rawsize = Marshal.SizeOf(type) + 2;
                }

                rawvalue = new byte[rawsize];
                BitConverter.GetBytes((short)order).CopyTo(rawvalue, 0);
                GCHandle handler = GCHandle.Alloc(rawvalue, GCHandleType.Pinned);
                rawpointer = handler.AddrOfPinnedObject();
                Marshal.StructureToPtr(objvalue, rawpointer + 2, false);
                handler.Free();
            }

            return rawvalue;
        }
        public static byte[] SetMarshalPointer(object objvalue, int order, Type _type = null)
        {
            byte[] rawvalue = null;
            int rawsize = 0;            
            Type type = (_type != null) ? _type : objvalue.GetType();
            if (objvalue.GetType() != type)
                objvalue = Convert.ChangeType(objvalue, type);

            IntPtr rawpointer = new IntPtr();

            if (type == typeof(string))
            {
                objvalue = (objvalue != DBNull.Value) ? objvalue : string.Empty;
                rawsize = objvalue.ToString().Length;
                short size = sizeof(short) * 2;
                rawvalue = new byte[rawsize + size];
                BitConverter.GetBytes((short)order).CopyTo(rawvalue, 0);
                BitConverter.GetBytes((short)rawsize).CopyTo(rawvalue, 2);
                rawpointer = Marshal.StringToHGlobalAnsi(objvalue.ToString());
                Marshal.Copy(rawpointer, rawvalue, 4, rawsize);
                Marshal.FreeHGlobal(rawpointer);
            }
            else
            {
                if (type == typeof(DateTime))
                {
                    objvalue = (objvalue != DBNull.Value) ? ((DateTime)objvalue).Ticks : (long)0;
                    rawsize = sizeof(long) + 2;
                }
                else if (type == typeof(byte[]))
                {
                    if (objvalue == DBNull.Value) objvalue = new byte[24];
                    rawsize = 24 + 2;
                    rawvalue = new byte[rawsize];
                    BitConverter.GetBytes((short)order).CopyTo(rawvalue, 0);
                    ((byte[])objvalue).CopyTo(rawvalue, 2);
                    return rawvalue;
                }
                else
                {
                    if (objvalue == DBNull.Value) objvalue = 0;
                    rawsize = Marshal.SizeOf(type) + 2;
                }

                rawvalue = new byte[rawsize];
                BitConverter.GetBytes((short)order).CopyTo(rawvalue, 0);
                rawpointer = Marshal.AllocHGlobal(rawsize);
                Marshal.StructureToPtr(objvalue, rawpointer, true);
                Marshal.Copy(rawpointer, rawvalue, 2, rawsize - 2);
                Marshal.FreeHGlobal(rawpointer);
            }

            return rawvalue;
        }

        public static DataTiers GetMod(this DataTiers tiers, ref object fromarray, out int rawcount, MemoryHandler memhandler = MemoryHandler.GC)
        {         
            byte[] _fromarray = (byte[])fromarray;       
            
            int noiseCount = (_fromarray.Length > 4096) ? 1024 : 4096;
            Type[] typearray = tiers.Trell.Pylons.AsEnumerable().Select(t => t.DataType).ToArray();
            int arraycount = typearray.Length;
            int int16Size = sizeof(Int16);
            int int64Size = sizeof(Int64);
            bool canRead = true;
            int _rawcount = 0;

            long endPosition = _fromarray.Length;
            long endTempPosition = 0;

            NoiseType endNoise = _fromarray.SeekNoise(out endTempPosition, SeekDirection.Backward, 0, noiseCount);
            if (endNoise != NoiseType.None)
                endPosition = endTempPosition;

            long startPosition = 0;
            long startTempPosition = 0;

            NoiseType startNoise = _fromarray.SeekNoise(out startTempPosition, SeekDirection.Forward, 0, noiseCount);
            if (startNoise != NoiseType.None)
                startPosition = startTempPosition;

            long position = (startPosition >= 0) ? startPosition : 0;

            long leftToRead = endPosition - startPosition;
            rawcount = _rawcount;
            Noid noid = Noid.Empty;

            while (canRead)
            {
                if (endPosition > position)
                {                 
                    noid[0] = _fromarray.Skip((int)position).Take(24).ToArray();

                    DataTier tier = null;
                    TierInherits otier = tiers.Registry.GetList(noid);
                    if (otier != null)
                    {
                        tier = otier[tiers.Registry.InheritId];
                        
                        position += 24;
                        int forlength = BitConverter.ToInt32(_fromarray, (int)position);
                        position += 4;
                        int bytecount = 0;
                        for (int c = 0; c < forlength; c++)
                        {
                            try
                            {
                                if (endPosition > position)
                                {
                                    int objsize = 0;
                                    bool isDate = false;
                                    if (leftToRead > 0)
                                    {
                                        int x = BitConverter.ToInt16(_fromarray, (int)position);
                                        position += 2;
                                        int addbytes = 0;
                                        if (typearray[x] == typeof(string))
                                        {
                                            objsize = BitConverter.ToInt16(_fromarray, (int)position);
                                            addbytes += int16Size;
                                            position += addbytes;
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
                                            Array.Copy(_fromarray, (int)position, rawvalue, 0, objsize);
                                            position += objsize;
                                            read = objsize + addbytes;
                                            leftToRead -= read;
                                            bytecount += read;
                                            if (tier != null)
                                            {
                                                if (memhandler != MemoryHandler.Marshal)
                                                    tier[x] = (!isDate) ? GetGCPointer(rawvalue, typearray[x]) :
                                                        new DateTime((long)GetGCPointer(rawvalue, typearray[x]));
                                                else
                                                    tier[x] = (!isDate) ? GetMarshalPointer(rawvalue, typearray[x]) :
                                                        new DateTime((long)GetMarshalPointer(rawvalue, typearray[x]));
                                            }
                                        }
                                        else canRead = false;
                                    }
                                    else canRead = false;
                                }
                                else canRead = false;

                                if (!canRead) c = forlength;
                            }
                            catch (Exception ex)
                            {
                                canRead = false;
                                c = forlength;  // 4U2DO
                            }
                        }
                        if (tier != null)
                            tier.State.FromBits(noid);
                    }                    
                }
                else canRead = false;

                if (canRead) _rawcount++;
            }
            rawcount = _rawcount;
            fromarray = null;
            _fromarray = null;
            return tiers;
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

    public static class MtxTiers
    {       
        public static int SetMtx(this DataTiers tiers, ISerialContext buffor, int offset = 0, int batchSize = 0, MemoryHandler memhandler = MemoryHandler.GC)
        {
            DataTiers objmatrix = tiers;
            int length = objmatrix.Count;
            Type[] types = tiers.Trell.Pylons.AsEnumerable().Select(t => t.DataType).ToArray();
            int Size = Marshal.SizeOf(tiers.Trell.Model);
            int yLeft = length - offset;
            int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
            int xLength = (yLength > 0) ? types.Length : 0;
            buffor.SerialBlock = new byte[yLength * (int)Size + 16];

            int processed = 0;
            int written = 16;
            if (xLength > 0)
            {
                for (int y = offset; y < offset + yLength; y++)
                {
                    for (short x = 0; x < xLength; x++)
                    {
                        object objvalue = objmatrix[y][x];

                        byte[] rawvalue = null;
                        if (memhandler != MemoryHandler.Marshal)
                            rawvalue = SetGCPointer(objvalue, types[x]);
                        else
                            rawvalue = SetMarshalPointer(objvalue, types[x]);

                        if (rawvalue != null)
                        {
                            rawvalue.CopyTo(buffor.SerialBlock, written);
                            written += rawvalue.Length;
                        }
                    }
                    processed++;
                }
                int nextoffset = (processed < yLeft) ? processed + offset : -1;

                if (nextoffset > 0)
                    buffor.SerialBlock = buffor.SerialBlock.Noise(1024, NoiseType.Block, written);
                else
                    buffor.SerialBlock = buffor.SerialBlock.Noise(1024, NoiseType.End, written);
                return nextoffset;
            }
            else
            {
                return -1;
            }
        }
        public static int SetMtx(this DataTier[] tiers, ISerialContext buffor, int offset = 0, int batchSize = 0, MemoryHandler memhandler = MemoryHandler.GC)
        {
            DataTier[] objmatrix = tiers;
            int length = objmatrix.Length;
            Type[] types = tiers[0].Trell.Pylons.AsEnumerable().Select(t => t.DataType).ToArray();
            int Size = Marshal.SizeOf(tiers[0].Trell.Model);
            int yLeft = length - offset;
            int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
            int xLength = (yLength > 0) ? types.Length : 0;
            buffor.SerialBlock = new byte[yLength * (int)Size + 16];

            int processed = 0;
            int written = 16;
            if (xLength > 0)
            {
                for (int y = offset; y < offset + yLength; y++)
                {
                    for (short x = 0; x < xLength; x++)
                    {
                        object objvalue = objmatrix[y][x];

                        byte[] rawvalue = null;
                        if (memhandler != MemoryHandler.Marshal)
                            rawvalue = SetGCPointer(objvalue, types[x]);
                        else
                            rawvalue = SetMarshalPointer(objvalue, types[x]);

                        if (rawvalue != null)
                        {
                            rawvalue.CopyTo(buffor.SerialBlock, written);
                            written += rawvalue.Length;
                        }
                    }
                    processed++;
                }
                int nextoffset = (processed < yLeft) ? processed + offset : -1;

                if (nextoffset > 0)
                    buffor.SerialBlock = buffor.SerialBlock.Noise(1024, NoiseType.Block, written);
                else
                    buffor.SerialBlock = buffor.SerialBlock.Noise(1024, NoiseType.End, written);
                return nextoffset;
            }
            else
            {
                return -1;
            }
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
     
        public static DataTiers GetMtx(this DataTiers tiers, ref object fromarray, out int rawcount, MemoryHandler memhandler = MemoryHandler.GC)
        {
            byte[] _fromarray = (byte[])fromarray;
            Type[] typearray = tiers.Trell.Pylons.AsEnumerable().Select(t => t.DataType).ToArray();
            int arraycount = typearray.Length;
            int int16Size = sizeof(Int16);
            int int64Size = sizeof(Int64);
            bool canRead = true;
            int _rawcount = 0;

            long endPosition = _fromarray.Length;
            long endTempPosition = 0;
            if (_fromarray.SeekNoise(out endTempPosition, SeekDirection.Backward, 0, 2048) != NoiseType.None)
                endPosition = endTempPosition;

            long startPosition = 0;
            long startTempPosition = 0;
            if (_fromarray.SeekNoise(out startTempPosition, SeekDirection.Forward, 0, 1024) != NoiseType.None)
                startPosition = startTempPosition;

            long position = startPosition;

            long leftToRead = endPosition - startPosition;
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
                            if (endPosition > position)
                            {
                                int objsize = 0;
                                bool isDate = false;
                                if (leftToRead > 0)
                                {
                                    int addbytes = 0;
                                    if (typearray[x] == typeof(string))
                                    {
                                        objsize = BitConverter.ToInt16(_fromarray, (int)position);
                                        addbytes = int16Size;
                                        position += addbytes;
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
                                        Array.ConstrainedCopy(_fromarray, (int)position, rawvalue, 0, objsize);
                                        position += objsize;
                                        read = objsize + addbytes;
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
                fromarray = null;
                _fromarray = null;
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
    #endregion
    
    #region Single Tier Fast Structures - TODO - Update from Tiers - not actual
    public static class RawTier
    {
        public static int SetRaw(this DataTier tier, ISerialContext buffor)
        {
            int Size = Marshal.SizeOf(tier.Model);
            byte[] array = new byte[Size + 16];
            byte[] item = new byte[Size];
            GCHandle handler = SetPointer(ref item);
            IntPtr ptr = handler.AddrOfPinnedObject();
            SetRaw(tier.n, ptr);
            handler.Free();
            item.CopyTo(array, 16);
            buffor.SerialBlock = array;
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

        public static DataTier GetRaw(this DataTier tier, ref object fromarray)
        {
            byte[] _fromarray = (byte[])fromarray;
            int Size = Marshal.SizeOf(tier.Model);
            Type Model = tier.Model;
            long endPosition = _fromarray.Length;
            long endTempPosition = 0;
            if (_fromarray.SeekNoise(out endTempPosition, SeekDirection.Backward, 0, 2048) != NoiseType.None)
                endPosition = endTempPosition;

            long startPosition = 0;
            long startTempPosition = 0;
            if (_fromarray.SeekNoise(out startTempPosition, SeekDirection.Forward, 0, 2048) != NoiseType.None)
                startPosition = startTempPosition;

            long leftToRead = endPosition - startPosition;

            List<DataTier> toUpdate = new List<DataTier>();
            int length = (int)(endPosition / Size);
            IntPtr objpointer = Marshal.AllocHGlobal(Size);
            GCHandle handler = GCHandle.Alloc(_fromarray);
            IntPtr rawpointer = handler.AddrOfPinnedObject();
            for (int i = 0; i < length; i++)
            {
                int position = (int)(startPosition + (i * Size));
                if (endPosition >= position + Size)
                {
                    UnsafeNative.CopyMemory(objpointer, rawpointer + position, (uint)Size);
                    object n = Marshal.PtrToStructure(rawpointer, Model);
                    tier = new DataTier(tier.Trell, ref n);
                }
            }
            handler.Free();
            Marshal.FreeHGlobal(objpointer);
            fromarray = null;
            _fromarray = null;
            return tier;
        }
    }

    public static class SimTier
    {
        public static int SetSim(this DataTier tier, ISerialContext buffor)
        {
            int Size = Marshal.SizeOf(tier.Model);
            byte[] array = new byte[Size + 16];
            byte[] item = new byte[Size];
            GCHandle handler = SetPointer(ref item);
            IntPtr ptr = handler.AddrOfPinnedObject();
            DataTier single = new DataTier(tier.Trell);
            single.PrimeArray = tier.DataArray;
            SetSim(single.n, ptr);
            handler.Free();
            item.CopyTo(array, 16);
            buffor.SerialBlock = array;
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
        public static void SetSim(object s, IntPtr ptr)
        {
            Marshal.StructureToPtr(s, ptr, true);
        }
    }

    public static class MtxTier
    {
        public static int SetMtx(this DataTier tier, ISerialContext buffor, MemoryHandler memhandler = MemoryHandler.GC)
        {
            DataTier objmatrix = tier;
            Type[] types = tier.Trell.Pylons.AsEnumerable().Select(t => t.DataType).ToArray();
            int Size = Marshal.SizeOf(tier.Trell.Model);
            int xLength = types.Length;
            byte[] array = new byte[(int)Size + 16];

            int written = 16;
            if (xLength > 0)
            {
                for (short x = 0; x < xLength; x++)
                {
                    object objvalue = objmatrix[x];

                    byte[] rawvalue = null;
                    if (memhandler != MemoryHandler.Marshal)
                        rawvalue = SetGCPointer(objvalue, types[x]);
                    else
                        rawvalue = SetMarshalPointer(objvalue, types[x]);

                    if (rawvalue != null)
                    {
                        rawvalue.CopyTo(array, written);
                        written += rawvalue.Length;
                    }
                }

                Array.Resize(ref array, written);

                buffor.SerialBlock = array;
                return written;
            }
            else
            {
                buffor.SerialBlock = array;
                return -1;
            }
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

        public static DataTier GetMtx(this DataTier tier, ref object fromarray, MemoryHandler memhandler = MemoryHandler.GC)
        {
            byte[] _fromarray = (byte[])fromarray;
            Type[] typearray = tier.Trell.Pylons.AsEnumerable().Select(t => t.DataType).ToArray();
            int arraycount = typearray.Length;
            int int16Size = sizeof(Int16);
            int int64Size = sizeof(Int64);
            bool canRead = true;
            List<DataTier> toUpdate = new List<DataTier>();

            long endPosition = _fromarray.Length;
            long endTempPosition = 0;
            if (_fromarray.SeekNoise(out endTempPosition, SeekDirection.Backward, 0, 2048) != NoiseType.None)
                endPosition = endTempPosition;

            long startPosition = 0;
            long startTempPosition = 0;
            if (_fromarray.SeekNoise(out startTempPosition, SeekDirection.Forward, 0, 2048) != NoiseType.None)
                startPosition = startTempPosition;

            long position = startPosition;
            long leftToRead = endPosition - startPosition;

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
                            if (endPosition > position)
                            {
                                int objsize = 0;
                                bool isDate = false;
                                if (leftToRead > 0)
                                {
                                    int addbytes = 0;
                                    if (typearray[x] == typeof(string))
                                    {
                                        objsize = BitConverter.ToInt16(_fromarray, (int)position);
                                        addbytes = int16Size;
                                        position += addbytes;
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
                                        Array.ConstrainedCopy(_fromarray, (int)position, rawvalue, 0, objsize);
                                        position += objsize;
                                        read = objsize + addbytes;
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
                fromarray = null;
                _fromarray = null;
                return tier;
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

    public static class ModTier
    {
        public static int SetMod(this DataTier tier, ISerialContext buffor, int offset = 0, int batchSize = 0, MemoryHandler memhandler = MemoryHandler.Marshal)
        {
            DataTier objmatrix = tier;
            Type[] types = tier.Trell.Pylons.TypeArray;
            int length = objmatrix.Cells.Count;
            int yLeft = length - offset;
            int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
            ArrayList bytearray = new ArrayList();
            bytearray.AddRange(new byte[16]);
            int processed = 0;
            int written = 16;
            //SortedList<int, object> mtxgrid = objmatrix.Grating.changes;
            HashList<object> mtxgrid = objmatrix.Grating.changes;
            bytearray.AddRange(BitConverter.GetBytes(objmatrix.DevIndex));
            bytearray.AddRange(BitConverter.GetBytes(mtxgrid.Count));
            written += 8;
            foreach (KeyValuePair<int, object> mtx in mtxgrid)
            {
                object objvalue = mtx.Value;
                int x = mtx.Key;
                byte[] rawvalue = null;
                if (memhandler != MemoryHandler.Marshal)
                    rawvalue = SetGCPointer(objvalue, x, types[x]);
                else
                    rawvalue = SetMarshalPointer(objvalue, x, types[x]);

                if (rawvalue != null)
                {
                    bytearray.AddRange(rawvalue);
                    written += rawvalue.Length;
                }
            }
            objmatrix.State.Synced = true;
            processed++;

            int nextoffset = (processed < yLeft) ? processed + offset : -1;
            int noiseCount = (bytearray.Count > 4096) ? 1024 : 4096;
            if (nextoffset > 0)
                buffor.SerialBlock = ((byte[])bytearray.ToArray(typeof(byte))).Noise(noiseCount, NoiseType.Block);
            else
                buffor.SerialBlock = ((byte[])bytearray.ToArray(typeof(byte))).Noise(noiseCount, NoiseType.End);
            return nextoffset;
        }

        public static byte[] SetGCPointer(object objvalue, int order, Type _type = null)
        {
            byte[] rawvalue = null;
            int rawsize = 0;
            Type type = (_type != null) ? _type : objvalue.GetType();
            IntPtr rawpointer = new IntPtr();

            if (type == typeof(string))
            {
                objvalue = (objvalue != null) ? objvalue : string.Empty;
                rawsize = objvalue.ToString().Length;
                short size = sizeof(short) * 2;
                rawvalue = new byte[rawsize + size];
                BitConverter.GetBytes((short)order).CopyTo(rawvalue, 0);
                BitConverter.GetBytes((short)rawsize).CopyTo(rawvalue, 2);
                rawpointer = Marshal.StringToHGlobalAnsi(objvalue.ToString());
                Marshal.Copy(rawpointer, rawvalue, 4, rawsize);
                Marshal.FreeHGlobal(rawpointer);
            }
            else
            {
                if (type == typeof(DateTime))
                {
                    objvalue = (objvalue != null) ? ((DateTime)objvalue).Ticks : (long)0;
                    rawsize = sizeof(long) + 2;
                }
                else
                {
                    if (objvalue == DBNull.Value) objvalue = 0;
                    rawsize = Marshal.SizeOf(type) + 2;
                }

                rawvalue = new byte[rawsize];
                BitConverter.GetBytes((short)order).CopyTo(rawvalue, 0);
                GCHandle handler = GCHandle.Alloc(rawvalue, GCHandleType.Pinned);
                rawpointer = handler.AddrOfPinnedObject();
                Marshal.StructureToPtr(objvalue, rawpointer + 2, false);
                handler.Free();
            }

            return rawvalue;
        }
        public static byte[] SetMarshalPointer(object objvalue, int order, Type _type = null)
        {
            byte[] rawvalue = null;
            int rawsize = 0;
            Type type = (_type != null) ? _type : objvalue.GetType();
            IntPtr rawpointer = new IntPtr();

            if (type == typeof(string))
            {
                objvalue = (objvalue != DBNull.Value) ? objvalue : string.Empty;
                rawsize = objvalue.ToString().Length;
                short size = sizeof(short) * 2;
                rawvalue = new byte[rawsize + size];
                BitConverter.GetBytes((short)order).CopyTo(rawvalue, 0);
                BitConverter.GetBytes((short)rawsize).CopyTo(rawvalue, 2);
                rawpointer = Marshal.StringToHGlobalAnsi(objvalue.ToString());
                Marshal.Copy(rawpointer, rawvalue, 4, rawsize);
                Marshal.FreeHGlobal(rawpointer);
            }
            else
            {
                if (type == typeof(DateTime))
                {
                    objvalue = (objvalue != DBNull.Value) ? ((DateTime)objvalue).Ticks : (long)0;
                    rawsize = sizeof(long) + 2;
                }
                else
                {
                    if (objvalue == DBNull.Value) objvalue = 0;
                    rawsize = Marshal.SizeOf(type) + 2;
                }

                rawvalue = new byte[rawsize];
                BitConverter.GetBytes((short)order).CopyTo(rawvalue, 0);
                rawpointer = Marshal.AllocHGlobal(rawsize);
                Marshal.StructureToPtr(objvalue, rawpointer, true);
                Marshal.Copy(rawpointer, rawvalue, 2, rawsize - 2);
                Marshal.FreeHGlobal(rawpointer);
            }

            return rawvalue;
        }

        public static DataTier GetMod(this DataTier tier, ref object fromarray, MemoryHandler memhandler = MemoryHandler.GC)
        {
            byte[] _fromarray = (byte[])fromarray;
            int noiseCount = (_fromarray.Length > 4096) ? 1024 : 4096;
            Type[] typearray = tier.Trell.Pylons.AsEnumerable().Select(t => t.DataType).ToArray();
            int arraycount = typearray.Length;
            int int16Size = sizeof(Int16);
            int int64Size = sizeof(Int64);
            bool canRead = true;
            int _rawcount = 0;

            long endPosition = _fromarray.Length;
            long endTempPosition = 0;
            if (_fromarray.SeekNoise(out endTempPosition, SeekDirection.Backward, 0, noiseCount) != NoiseType.None)
                endPosition = endTempPosition;

            long startPosition = 0;
            long startTempPosition = 0;
            if (_fromarray.SeekNoise(out startTempPosition, SeekDirection.Forward, 0, noiseCount) != NoiseType.None)
                startPosition = startTempPosition;

            long position = (startPosition >= 0) ? startPosition : 0;

            long leftToRead = endPosition - startPosition;
            while (canRead)
            {
                if (endPosition > position)
                {
                    position += 4;
                    int forlength = BitConverter.ToInt32(_fromarray, (int)position);
                    position += 4;
                    int bytecount = 0;
                    for (int c = 0; c < forlength; c++)
                    {
                        try
                        {
                            if (endPosition > position)
                            {
                                int objsize = 0;
                                bool isDate = false;
                                if (leftToRead > 0)
                                {
                                    int x = BitConverter.ToInt16(_fromarray, (int)position);
                                    position += 2;
                                    int addbytes = 0;
                                    if (typearray[x] == typeof(string))
                                    {
                                        objsize = BitConverter.ToInt16(_fromarray, (int)position);
                                        addbytes += int16Size;
                                        position += addbytes;
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
                                        Array.ConstrainedCopy(_fromarray, (int)position, rawvalue, 0, objsize);
                                        position += objsize;
                                        read = objsize + addbytes;
                                        leftToRead -= read;
                                        bytecount += read;

                                        if (memhandler != MemoryHandler.Marshal)
                                            tier[x] = (!isDate) ? GetGCPointer(rawvalue, typearray[x]) :
                                                new DateTime((long)GetGCPointer(rawvalue, typearray[x]));
                                        else
                                            tier[x] = (!isDate) ? GetMarshalPointer(rawvalue, typearray[x]) :
                                                new DateTime((long)GetMarshalPointer(rawvalue, typearray[x]));

                                    }
                                    else canRead = false;
                                }
                                else canRead = false;
                            }
                            else canRead = false;

                            if (!canRead) c = forlength;
                        }
                        catch (Exception ex)
                        {
                            canRead = false;
                            c = forlength;  // 4U2DO
                        }
                    }
                }
                else canRead = false;

                if (canRead) _rawcount++;
            }
            fromarray = null;
            _fromarray = null;
            return tier;       
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

    public enum MemoryHandler
    {
        Marshal,
        GC
    }
}