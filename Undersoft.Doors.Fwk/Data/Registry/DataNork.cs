using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Doors;
using System.Doors.Drive;

namespace System.Doors.Data
{
    public class DataNork : IDictionary<long, object>, IEnumerable, INoid
    {
        private HashList<List<Vessel<TierInherits>>> keymap = new HashList<List<Vessel<TierInherits>>>(16);

        private int resizeblock = 100 * 1000;
        private int counter = 0;
        private int capacity;

        public int InheritId;

        public DataTiers Tiers;
        public ArrayList Inheritors
        { get { return Registry.Inheritors; } }
        public DataNoid  Registry;
        public DataConfig Config
        { get; set; }
        public DataState State
        { get; set; }
        public HashSet<int[]> RelKeys
        {
            get; set;
        }

        public DataRelaying Relaying
        {
            get
            {
                return Tiers.Relaying;
            }
        }

        public bool IsPrime
        { get { return Tiers.Trell.IsPrime; } }
        public bool IsDevisor
        { get { return Tiers.Trell.IsDevisor; } }
        public bool DriveOn
        { get { return Tiers.DriveOn; } set { Tiers.DriveOn = value; } }
        public bool IsBatch
        { get { return Tiers.IsBatch; } set { Tiers.IsBatch = value; } }

        public int Counter
        {
            get
            {
                return counter;
            }
            set
            {
                counter = value;
                if (counter == capacity)
                {
                    if (DriveOn)
                    {
                        DriveTmp = Drive;
                        Drive = null;
                        OpenDrive();
                    }
                    else
                        capacity += resizeblock;
                }

            }
        }

        public DataNork(DataTiers tiers, DataNoid registry, DataStore store = DataStore.Space)
        {
            Tiers = tiers;
            Config = new DataConfig(this, store);
            State = new DataState();
            Registry = registry;            
            InheritId = registry.InheritId;
            RelKeys = new HashSet<int[]>(new IntArrayComparer());
        }
        public DataNork(DataTiers tiers, DataNoid registry, DataNork devisor, DataStore store = DataStore.Space)
        {
            Tiers = tiers;
            Config = new DataConfig(this, store);
            State = new DataState();
            RelKeys = devisor.RelKeys;
            keymap = devisor.keymap;
            Registry = registry;
            InheritId = registry.InheritId;
        }

        public DataTier this[long key, int index]
        {
            get
            {
                List<Vessel<TierInherits>> temp = keymap.Get(key);
                if (temp != null && temp.Count > index)                    
                    return temp[index].Value[InheritId];
                return null;
            }          
        }

        public object this[long key]
        {
            get
            {
                List<Vessel<TierInherits>> temp = keymap.Get(key);
                if (temp != null)
                    return temp.Select(t => t.Value[InheritId]).ToArray();
                return new DataTier[0];
            }
            set
            {

                List<Vessel<TierInherits>> temp = keymap.Get(key);
                if (temp == null)                
                    keymap.AddNew(key, new List<Vessel<TierInherits>>() { (Vessel<TierInherits>)value });                
                else
                    temp.Add((Vessel<TierInherits>)value);                
            }
        }

        public Vessel<TierInherits> this[long key, long noid]
        {
            get
            {
                List<Vessel<TierInherits>> temp = keymap.Get(key);
                if (temp != null)                
                    return temp.Where(t => t.Key == noid).FirstOrDefault();                
                return null;
            }
            set
            {

                List<Vessel<TierInherits>> temp = keymap.Get(key);
                if (temp == null)
                {
                    temp = new List<Vessel<TierInherits>>();
                    temp.Add(value);
                    keymap.Add(key, temp);
                }
                else
                {
                    for (int i = 0; i < temp.Count; i++)
                        if (temp[i].Key == noid)
                        {
                            temp[i] = value;
                            return;
                        }
                    temp.Add(value);
                }
            }
        }

        #region IDictionary     

        public DataTier GetFirst(long key)
        {
            List<Vessel<TierInherits>> temp = keymap.Get(key);
            if (temp != null && temp.Count > 0)
                return temp[0].Value[InheritId];
            return null;
        }
        public DataTier GetLast(long key)
        {
            List<Vessel<TierInherits>> temp = keymap.Get(key);
            if (temp != null && temp.Count > 0)
                return temp[temp.Count - 1].Value[InheritId];
            return null;
        }
        public DataTier[] GetTiers(long key)
        {
            List<Vessel<TierInherits>> temp = keymap.Get(key);
            if (temp != null)
                return temp.Select(t => t.Value[InheritId]).ToArray();
            return null;
        }
        public List<Vessel<TierInherits>> GetList(long key)
        {
            return keymap.Get(key);
        }
        public Vessel<List<Vessel<TierInherits>>> GetVessel(long key)
        {
            return keymap.GetVessel(key);
        }
        public bool PutVessel(Vessel<List<Vessel<TierInherits>>> vessel)
        {
            return keymap.Put(vessel);
        }

        public void Add(long key, object value)
        {
            this[key] = value;
        }
        public void Add(KeyValuePair<long, object> item)
        {
            this[item.Key] = item.Value;
        }
        public void Add(KeyValuePair<long, List<Vessel<TierInherits>>> item)
        {
            foreach (Vessel<TierInherits> t in item.Value)
                this[item.Key] = t;
        }

        public bool TryAdd(ref long key, object value)
        {
            long _key = key;
            List<Vessel<TierInherits>> temp = keymap.Get(_key);
            if (temp == null)
            {
                temp = new List<Vessel<TierInherits>>();
                temp.Add((Vessel<TierInherits>)value);
                keymap.Add(key, temp);
                return true;
            }
            else
            {
                temp.Add((Vessel<TierInherits>)value);
                return true;
            }
        }

        public void Put(long key, DataTier value)
        {
            this[key] = value;
        }

        public bool ContainsKey(long key)
        {
            List<Vessel<TierInherits>> temp = keymap.Get(key);
            if (temp != null)
            {
                return true;
            }
            return false;
        }

        public bool Remove(long key)
        {
            List<Vessel<TierInherits>> list = keymap.Remove(key);
            if (list != null)
            {
                object o = null;
                for (int x = 0; x < list.Count; x++)
                {
                    o = list[x];
                    if (o != null)
                    {
                        DataTier tier = ((DataTier)o);
                        Noid shah = tier.GetNoid();
                        shah[15, 1][0] = 15;
                        tier.Deleted = true;
                        tier.Synced = false;
                        DataNork dn = this;
                        if (InheritId != x)
                            dn = ((DataTiers)Inheritors[x]).Registry.Keymap;
                        if (dn.Drive == null) OpenDrive();
                        dn.Drive[tier.Index] = shah;
                        if (tier.IsPrime)
                        {
                            tier.SetStateNoid();
                            tier.SaveChanges(true);
                        }
                    }
                }
                return true;
            }
            return false;
        }
        public bool Remove(object value)
        {
            long key = -1;
            if (value is DataTier && ((DataTier)value).iN != null)
                key = ((DataTier)value).GetNoid().LongValue;
            else if (value is long)
                key = (long)value;
            else if (value is IDataNative)
                key = ((IDataNative)value).GetNoid(Tiers.Trell.NoidOrdinal).LongValue;
            if (key != -1)
                return Remove(key);
            return false;
        }
        public bool Remove(KeyValuePair<long, object> item)
        {
            if (keymap.ContainsKey(item.Key))
                return Remove(item.Key);
            return false;
        }

        public bool TryGetValue(long key, out List<Vessel<TierInherits>> value)
        {
            return keymap.TryGet(key, out value);
        }
        public bool TryGetValue(long key, out object value)
        {
            value = keymap.Get(key);
            if (value == null)
                return false;
            return true;
        }

        public void Clear()
        {
            keymap.Clear();
            Counter = 0;
        }

        public void RebuildMap(string relayName)
        {

            DataRelay rel = null;
            int[] keyodr = null;
            DataRelay[] cr = Tiers.Relaying.ChildRelays.Where(k => k.RelayName == relayName).ToArray();
            if (cr.Length > 0)
            {
                rel = cr[0];
                keyodr = rel.Parent.KeysOrdinal;
            }
            else
            {
                cr = Tiers.Relaying.ParentRelays.Where(k => k.RelayName == relayName).ToArray();
                if (cr.Length > 0)
                {
                    rel = cr[0];
                    keyodr = rel.Child.KeysOrdinal;
                }
            }
            if (rel != null)
            {
                if (RelKeys.Add(keyodr))
                {
                    if (!Tiers.IsCube)
                    {
                        foreach (Vessel<TierInherits> al in (IEnumerable)Tiers.Registry)
                        {
                            this[keyodr.Select(k => (al.Value[0]).iN[k]).ToArray().GetShahCode64()] = al;
                        }
                    }
                    else
                    {
                        foreach (Vessel<TierInherits> al in (IEnumerable)Tiers.Registry)
                        {
                            this[keyodr.Select(k => (al.Value[0])[k]).ToArray().GetShahCode64()] = al;
                        }
                    }
                }
            }

        }
        public bool Contains(KeyValuePair<long, object> item)
        {
            return keymap.ContainsKey(item.Key);
        }
        public void CopyTo(KeyValuePair<long, object>[] array, int arrayIndex)
        {
            keymap.CopyTo(array, arrayIndex);
        }

        public ICollection<long> Keys
        {
            get
            {
                KeyValuePair<long, object>[] keys = new KeyValuePair<long, object>[keymap.Count];
                keymap.CopyTo(keys, 0);
                return keys.Select(k => k.Key).ToArray();
            }
        }
        public ICollection<object> Values
        {
            get
            {
                KeyValuePair<long, List<Vessel<TierInherits>>>[] keys = new KeyValuePair<long, List<Vessel<TierInherits>>>[keymap.Count];
                keymap.CopyTo(keys, 0);
                return keys.Select(k => k.Value).ToArray();
            }
        }

        public int Count
        {
            get
            {
                return keymap.Count;
            }
        }
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region IEnumerable
        public IEnumerator<KeyValuePair<long, object>> GetEnumerator()
        {
            throw new NotImplementedException();
            //return keymap.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return keymap.GetEnumerator();
        }
        #endregion

        #region Serialization       

        //public int Serialize(Stream tostream, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        //{
        //    return this.SetRaw(tostream);
        //}
        //public int Serialize(ISerialContext buffor, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        //{
        //    return this.SetRaw(buffor);
        //}

        //public object Deserialize(Stream fromstream, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        //{
        //    return this.GetRaw(fromstream);
        //}
        //public object Deserialize(ref object fromarray, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        //{
        //    return this.GetRaw(ref fromarray);
        //}

        public object[] GetMessage()
        {
            return new object[] { this };
        }
        public object GetHeader()
        {
            return this;
        }

        public byte[] GetShah()
        {
            return (Config.Place != null) ? Config.Place.GetShah() : null;
        }

        public int SerialCount
        { get; set; }
        public int DeserialCount
        { get; set; }
        public int ProgressCount
        { get; set; }
        public int ItemsCount
        { get { return Count; } }

        public ushort GetDriveId()
        {
            return 0;
        }
        public ushort GetSectorId()
        {
            return 0;
        }
        public ushort GetLineId()
        {
            return 0;
        }

        public string GetMapPath()
        {
            return Tiers.Config.Path;
        }
        public string GetMapName()
        {
            return Tiers.Config.Place.Replace("ndat", "nork");
        }

        #endregion

        #region IDriveRecorder        

        public void AppendDrive(long key)
        {
            if (Drive == null)
                ReadDrive();

            Drive[Counter] = key;
            Counter++;
            Drive[Counter] = (long)0;
            Counter++;
        }
        public void SetDrive(int id, long key)
        {
            if (Drive == null)
                ReadDrive();

            if (id < Counter)
                Drive[id] = key;
            else if (id == Counter)
            {
                Drive[id] = key;
                Counter++;
                Drive[Counter] = (long)0;
                Counter++;
            }
            else
            {
                Counter = id;
                Drive[id] = key;
                Counter++;
                Drive[Counter] = (long)0;
                Counter++;
            }
        }

        [NonSerialized] public IDrive drivetmp;
        public IDrive DriveTmp
        { get { return drivetmp; } set { drivetmp = value; } }
        [NonSerialized] public IDrive drive;
        public IDrive Drive
        {
            get
            {
                return drive;
            }
            set
            {
                drive = value;
            }
        }
        private void WriteAll(object reg, int depth = 0)
        {
            if (Tiers.Trell.IsPrime || Tiers.Trell.IsDevisor)
            {
                foreach (List<Vessel<TierInherits>> _trs in keymap)
                {
                    foreach(Vessel<TierInherits> _tr in _trs)
                    if (_tr != null)
                    {
                        object tr = _tr.Value[InheritId];
                        if (tr != null)
                        {
                            if (((DataTier)tr).iN != null)
                            {
                                Drive[Counter] = ((DataTier)tr).GetNoid();
                                Counter++;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (DataTier idarray in Tiers)
                {
                    if (idarray.iN != null)
                    {
                        Drive[Counter] = idarray.GetNoid();
                        Counter++;
                    }
                }
            }
        }
        public void WriteDrive()
        {
            if (Tiers.Mode == DataModes.Tiers)
            {
                if (Drive == null)
                    OpenDrive();
                Counter = 0;
                WriteAll(null);
                Drive[Counter] = Noid.Empty;
                Counter++;
                Drive.ItemCapacity = capacity;
                Drive.ItemCount = Counter;
                Drive.ItemSize = 8 + 16;
                Drive.WriteHeader();
            }
        }
        public void ReadDrive()
        {
            if (Tiers.Mode == DataModes.Tiers)
            {
                if (!Tiers.Trell.IsPrime || Tiers.IsCube)
                {
                    if (Drive == null)
                        OpenDrive();

                    Counter = 0;
                    capacity = Convert.ToInt32(Drive.ItemCapacity);
                    bool readable = true;
                    Noid noid;
                    DataTier tier = null;
                    while (readable)
                    {
                        noid = (Noid)Drive[Counter];
                        if (noid.IsNotEmpty)
                        {
                            if (noid[15, 1][0] == 0)
                            {
                                tier = Tiers.PutInner(ref noid);
                                if (tier != null)
                                    Tiers.TiersView.AddView(tier);
                            }
                            Counter++;
                        }
                        else
                            readable = false;
                    }
                }
            }
        }
        public void OpenDrive()
        {
            if (Drive == null)
            {
                string driveFile = Config.File;
                string[] driveFiles = Directory.GetFiles((Config.Disk != "") ? Config.Disk + "/" + Config.Path : Config.Path);

                bool found = false;
                string fileFound = "";
                string driveName = "";

                string[] driveNames = Config.File.Split(':');

                if (driveNames.Length > 1)
                    driveName = driveNames[1];
                else
                    driveName = driveNames[0];

                int mply = 0;

                foreach (string file in driveFiles)
                    if (file.Contains(".nork"))
                    {
                        found = true;

                        string[] ext = file.Split('.');
                        int length = ext.Length;

                        int mplytemp = Convert.ToInt32(ext[length - 2]) + 1;
                        if (mplytemp >= mply + 1)
                        {
                            mply = mplytemp;
                            fileFound = file;
                        }
                    }

                if (found)
                {
                    if (mply * resizeblock <= Tiers.Count)
                    {
                        mply++;
                        capacity = mply * resizeblock;
                    }
                    else
                        capacity = mply * resizeblock;

                    mply--;
                    driveFile = driveFile.Replace(".nork", string.Format(".{0}.nork", mply));
                    driveName = driveName.Replace(".nork", string.Format(".{0}.nork", mply));
                }
                else
                {
                    driveFile = driveFile.Replace(".nork", string.Format(".{0}.nork", 0));
                    driveName = driveName.Replace(".nork", string.Format(".{0}.nork", 0));
                    capacity = resizeblock;
                }

                Drive = new DriveTiers(driveFile, driveName, capacity, typeof(Noid));
                DriveOn = true;

                if (DriveTmp != null)
                {
                    DriveTmp.CopyTo(Drive, (uint)(capacity - resizeblock), 0);
                    DriveTmp.Dispose();
                }
            }
        }
        public void CloseDrive()
        {
            if (Drive != null)
                Drive.Close();
            if (DriveTmp != null)
                DriveTmp.Close();
            DriveOn = false;
        }

        #endregion
    }
}
