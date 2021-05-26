using System.Text;
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
    public class DataNoid : IDictionary<IQuid, DataTier>, IEnumerable<Vessel<TierInherits>>,  IEnumerable, INoid
    {
        private HashList<TierInherits> registry = new HashList<TierInherits>(256 * 256);

        private int resizeblock = 100 * 1000;
        private int counter = 0;
        private int capacity;

        public int InheritId;

        public DataTiers Tiers;
        public ArrayList Inheritors;
        public DataConfig Config
        { get; set; }
        public DataState State
        { get; set; }

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

        public DataNoid(DataTiers tiers, DataStore store = DataStore.Space)
        {
            Tiers = tiers;
            Config = new DataConfig(this, store);
            State = new DataState();
            Inheritors = new ArrayList(1);
            Inheritors.Insert(0, tiers);
            InheritId = 0;
            Keymap = new DataNork(tiers, this, store);
        }
        public DataNoid(DataTiers tiers, DataNoid devisor, DataStore store = DataStore.Space)
        {
            Tiers = tiers;
            Config = new DataConfig(this, store);
            State = new DataState();
            registry = devisor.registry;
            InheritId = devisor.Inheritors.Count;
            devisor.Inheritors.Capacity++;
            devisor.Inheritors.Add(tiers);
            Inheritors = devisor.Inheritors;
            Keymap = new DataNork(tiers, this, devisor.Keymap, store);
        }

        public DataNork Keymap { get; set; }

        public DataTier this[IQuid key]
        {
            get
            {
                Vessel<TierInherits> temp = registry.GetVessel(key);
                if (temp != null)
                {
                    if (!(temp.Value.Count > InheritId))
                        temp.Value.Resize(InheritId * 2);
                    return temp.Value[InheritId];
                }
                return null;
            }
            set
            {
                long _key = key.LongValue;
                Vessel<TierInherits> temp = registry.GetVessel(_key);
                if (temp == null)
                {
                    temp = new Vessel<TierInherits>(_key, new TierInherits(Inheritors.Count));
                    temp.Value[InheritId] = value;
                    registry.Put(temp);
                    if (IsPrime)
                        value.RelKeys.Select(k => Keymap[k] = temp).ToArray();
                }
                else
                {
                    if (!(temp.Value.Count > InheritId))
                        temp.Value.Resize(InheritId * 2);
                    temp.Value[InheritId] = value;
                }
            }
        }
        public Vessel<TierInherits> this[long key]
        {
            get
            {
                Vessel<TierInherits> temp = registry.GetVessel(key);
                if (temp != null)
                {
                    if (!(temp.Value.Count > InheritId))
                        temp.Value.Resize(InheritId * 2);
                    return temp;                   
                }
                return null;
            }
            set
            {
                long _key = key;
                Vessel<TierInherits> temp = registry.GetVessel(_key);
                if (temp == null)
                {
                    temp = new Vessel<TierInherits>(_key, value.Value);
                    registry.Put(temp);
                    if (IsPrime && value.Value.Count > 0)
                    {
                        value.Value[0].RelKeys.Select(k => Keymap[k] = temp).ToArray();
                    }
                }
                else
                {
                    if (!(temp.Value.Count > InheritId))
                        temp.Value.Resize(InheritId * 2);

                    value.Value.Select((c, y) => temp.Value[y] = c).ToArray();

                }
            }
        }

        public void RebuildMap()
        {
            if (IsDevisor)
            {
                Keymap.Clear();
                foreach (Vessel<List<DataTier>> al in (IEnumerable)Tiers.Registry)
                {
                    (al.Value[0]).RelKeys.Select(k => Keymap[k] = al.Value).ToArray();
                }
            }
        }

        #region IDictionary     

        public TierInherits GetList(IQuid key)
        {
            TierInherits temp = registry.Get(key.LongValue);
            if (temp != null)
            {

                if (!(temp.Count > InheritId))
                    temp.Resize(InheritId + 1);
                return temp;
            }
            return temp;
        }

        public Vessel<TierInherits> GetVessel(IQuid key)
        {
            return registry.GetVessel(key);          
        }
        public bool PutVessel(Vessel<TierInherits> vessel)
        {
            return registry.Put(vessel);
        }

        public void Resize(TierInherits array, int size)
        {
            array.Resize(size);
        }

        public void Add(IQuid key, DataTier value)
        {
            this[key] = value;
        }
        public void Add(KeyValuePair<IQuid, DataTier> item)
        {
            this[item.Key] = item.Value;
        }
        public void Add(KeyValuePair<IQuid, List<DataTier>> item)
        {
            foreach (DataTier t in item.Value)
                this[item.Key] = t;
        }

        public bool TryAdd(IQuid key, DataTier value)
        {
            long _key = key.LongValue;
            TierInherits temp = registry.Get(_key);
            if (temp == null)
            {
                temp = new TierInherits(Inheritors.Count);
                temp[InheritId] = value;
                registry.Add(_key, temp);
                if (IsPrime || IsDevisor)
                    (value).RelKeys.Select(k => Keymap[k] = temp).ToArray();
                return true;
            }
            else
            {
                int subtract = temp.Count - InheritId;

                if (subtract < 1)
                    temp.Resize(InheritId * 2);
                if (temp[InheritId] == null)
                {
                    temp[InheritId] = value;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void Put(IQuid key, DataTier value)
        {
            this[key] = value;
        }

        public bool ContainsKey(IQuid key)
        {
            TierInherits temp = registry.Get(key);
            if (temp != null)
            {
                if (temp.Count > InheritId)
                {
                    if (temp[InheritId] != null)
                        return true;
                }
                else
                    Resize(temp, InheritId + 1);
            }
            return false;
        }

        public bool Remove(IQuid key)
        {
           TierInherits list = registry.Remove(key);
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
                        DataNoid dn = this;
                        if (InheritId != x)
                            dn = ((DataTiers)Inheritors[x]).Registry;
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
        public bool Remove(DataTier value)
        {
            IQuid key = null;
            if (value is DataTier && ((DataTier)value).iN != null)
                key = ((DataTier)value).GetNoid();
            else if (value is IQuid)
                key = (IQuid)value;
            else if (value is IDataNative)
                key = ((IDataNative)value).GetNoid(Tiers.Trell.NoidOrdinal);
            if (key != null)
                return Remove(key);
            return false;
        }
        public bool Remove(KeyValuePair<IQuid, DataTier> item)
        {
            if (registry.ContainsKey(item.Key))
                return Remove(item.Key);
            return false;
        }

        public bool TryGetValue(IQuid key, out TierInherits value)
        {
            return registry.TryGet(key, out value);
        }
        public bool TryGetValue(IQuid key, out DataTier value)
        {
            TierInherits _value = registry.Get(key.LongValue);
            if (_value != null)
            {
                value = _value[InheritId];
                if (value != null)
                    return true;
                return false;
            }
            value = null;
            return false;
        }

        public void Clear()
        {
            registry.Clear();
            Counter = 0;
        }
        public bool Contains(KeyValuePair<IQuid, DataTier> item)
        {
            return registry.ContainsKey(item.Key);
        }
        public void CopyTo(KeyValuePair<IQuid, DataTier>[] array, int index)
        {
            int i = 0;
            foreach (Vessel<List<DataTier>> vessel in registry)
            {
                array[index + i++] = new KeyValuePair<IQuid, DataTier>(new Quid(vessel.Key), vessel.Value[InheritId]);
            }

        }

        public ICollection<IQuid> Keys
        {
            get
            {
                KeyValuePair<IQuid, object>[] keys = new KeyValuePair<IQuid, object>[registry.Count];
                registry.CopyTo(keys, 0);
                return keys.Select(k => k.Key).ToArray();
            }
        }
        public ICollection<DataTier> Values
        {
            get
            {
                KeyValuePair<IQuid, TierInherits>[] keys = new KeyValuePair<IQuid, TierInherits>[registry.Count];
                registry.CopyTo(keys, 0);
                return keys.Select(k => k.Value[InheritId]).ToArray();
            }
        }

        public int Count
        {
            get
            {
                return registry.Count;
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
        IEnumerator<Vessel<TierInherits>> IEnumerable<Vessel<TierInherits>>.GetEnumerator()
        {
            return ((IEnumerable<Vessel<TierInherits>>)registry).GetEnumerator();
        }
        public IEnumerator<KeyValuePair<IQuid, DataTier>> GetEnumerator()
        {
            throw new NotImplementedException();
            //return registry.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return registry.GetEnumerator();
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
            return Tiers.Config.Place.Replace("ndat", "noid");
        }

        #endregion

        #region IDriveRecorder        

        public void AppendDrive(IQuid key)
        {
            if (Drive == null)
                ReadDrive();

            Drive[Counter] = key;
            Counter++;
            Drive[Counter] = key.Empty;
            Counter++;
        }
        public void SetDrive(int id, IQuid key)
        {
            if (Drive == null)
                ReadDrive();

            if (id < Counter)
                Drive[id] = key;
            else if (id == Counter)
            {
                Drive[id] = key;
                Counter++;
                Drive[Counter] = key.Empty;
                Counter++;
            }
            else
            {
                Counter = id;
                Drive[id] = key;
                Counter++;
                Drive[Counter] = key.Empty;
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
                foreach (Vessel<TierInherits> _tr in registry)
                {
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
                    //bool readable = true;
                    Noid noid;
                    DataTier tier = null;
                    for(; ; )
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
                           break;

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
                    if (file.Contains(".noid"))
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
                    driveFile = driveFile.Replace(".noid", string.Format(".{0}.noid", mply));
                    driveName = driveName.Replace(".noid", string.Format(".{0}.noid", mply));
                }
                else
                {
                    driveFile = driveFile.Replace(".noid", string.Format(".{0}.noid", 0));
                    driveName = driveName.Replace(".noid", string.Format(".{0}.noid", 0));
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

    public class TierInherits: IEnumerable<DataTier>
    {
        private DataTier[] tierInherits;

        public TierInherits()
        {
            tierInherits = new DataTier[1];
        }
        public TierInherits(int capacity)
        {
            tierInherits = new DataTier[capacity];
        }
        public TierInherits(DataTier[] tiers)
        {
            tierInherits = tiers;
        }

        public DataTier this[int id]
        {
            get
            {
                if (id < Count)
                    return tierInherits[id];
                return tierInherits[0];
            }
            set
            {
                if (id < Count)
                    tierInherits[id] = value;
            }
        }

        public void Resize(int size)
        {
            DataTier[] temp = new DataTier[size];
            DataTier[] _tierInherits = tierInherits;
            _tierInherits.CopyTo(temp, 0);
            tierInherits = temp;
            _tierInherits = null; 
        }
        public int Count
        { get { return tierInherits.Length; } }

        public DataTier[] AsArray()
        {
            return tierInherits;
        }

        public IEnumerator<DataTier> GetEnumerator()
        {
            return ((IEnumerable<DataTier>)tierInherits).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<DataTier>)tierInherits).GetEnumerator();
        }
       
        public void Clear()
        {
            tierInherits = new DataTier[8];
        }

        public bool Contains(DataTier item)
        {
            return tierInherits.Contains(item);
        }

        public void CopyTo(DataTier[] array, int arrayIndex)
        {
            tierInherits.CopyTo(array, arrayIndex);
        }

        public int IndexOf(DataTier item)
        {
            return Array.IndexOf(tierInherits, item);
        }      
    }
}
