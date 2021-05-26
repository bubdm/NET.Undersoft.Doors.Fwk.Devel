using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Doors;
using System.Doors.Drive;

namespace System.Doors.Data
{
    public static class DataBank
    {
        private static int HASHES_CAPACITY_STANDARD_VALUE = 201;                                 // probably there is no need for more then 100 values in hashes, +1 cause 0based count
        private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;             // established max count for access requests to hashes        
        
        public static DepotIdentity ServerIdentity
        { get; set; }
        public static DepotIdentity ServiceIdentity
        { get; set; }

        public static IDataLog DataLog
        { get; set; }
        public static IDataLog NetLog
        { get; set; }

        public static bool DriveClient = false;

        private static string bankName = "Bank";
        public static string BankName
        {
            get
            {
                return bankName;
            }
            set
            {
                bankName = value;
                Vault.BankName = value;
                Vault.Config.SetMapFile();
            }
        }

        private static string drivePath = "QDFS";
        public static string DrivePath
        {
            get { return drivePath; }
            set { drivePath = value; Vault.Config.SetMapFile(); }
        }

        private static ushort clusterId = 0x00;
        public static ushort ClusterId
        {
            get { return clusterId; }
            set { clusterId = value; }
        }

        public static ConcurrentDictionary<int, object> Registry
        { get; set; } = new ConcurrentDictionary<int, object>(
                      MAX_ACCESS_REQUEST_VALUE, HASHES_CAPACITY_STANDARD_VALUE, new HashComparer());

        public static DataVaults Vault
        { get; set; } = new DataVaults(BankName);

        public static object PathFinder(string path, out Type type)
        {
            string[] items = path.Split('/');
            int length = items.Length;
            int last = length - 1;
            bool isTrellis = (items[last].Split('.').Length > 1) ? true : false;
            DataVault sets = Vault[items[0]];
            DataDeposit set = null;
            DataTrellis trell = null;
            if (length > 1 && sets != null)
            {
                if (isTrellis)
                {
                    if ((last) == 2)
                        trell = sets[items[1], items[2].Split('.')[0]].Trell;              
                    type = typeof(DataTrellis);
                    return trell;
                }
                else
                {
                    type = typeof(DataDeposit);
                    set = sets[items[1]];                 
                    return set;
                }
            }
            else
            {
                type = typeof(DataVault);
                return sets;
            }
        }      

        public static void ToDataLog(this Exception message)
        {
            string _message = 1.ToString() + "#Exception#" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "#" + DateTime.Now.Millisecond.ToString()
                                                                       + "#" + message.ToString();
            if (DataLog != null)
                DataLog.WriteLog(_message);

        }
        public static void ToDataLog(this String message)
        {
            string _message = 2.ToString() + "#Information#" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "#" + DateTime.Now.Millisecond.ToString()
                                                                       + "#" + message.ToString();
            if (DataLog != null)
                DataLog.WriteLog(_message);

        }
        public static void ToNetLog(this Exception message)
        {
            string _message = 1.ToString() + "#Exception#" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "#" + DateTime.Now.Millisecond.ToString()
                                                                 + "#" + message.ToString();
            if(NetLog != null)
                NetLog.WriteLog(_message);
        }
        public static void ToNetLog(this String message)
        {
            string _message = 2.ToString() + "#Information#" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "#" + DateTime.Now.Millisecond.ToString()
                                                                 + "#" + message.ToString();
            if (NetLog != null)
                NetLog.WriteLog(_message);
        }
    }
    [Serializable]
    public class DataVaults : IDictionary<string, DataVault>, IEnumerable<KeyValuePair<string, DataVault>>, IDriveRecorder, 
                              IDataBank, IDataSerial, IDataMorph, IListSource, ITypedList, IDataGridBinder, IDataTreeBinder, 
                              IBindingList, IList, INoid, IDataConfig
    {
        [NonSerialized] private IDataTreeBinder boundedTree;
        [NonSerialized] private DataVaults bankOn;
        [NonSerialized] private DataVault vaultUp;
        [NonSerialized] private DataVaults vaultsUp;
        [NonSerialized] private ConcurrentDictionary<int, object> registry;

        private static int HASHES_CAPACITY_STANDARD_VALUE = 501;                             
        private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;         // established max count for access requests to Areas

        public string BankName
        { get; set; }
        public bool IsMain = false;

        public DataConfig Config
        { get; set; }
        public DataParams Parameters
        { get; set; }
        public DataState State
        { get; set; }

        public bool Checked
        {
            get
            {
                return State.Checked;
            }
            set
            {
                State.Checked = value;
            }
        }
        public bool Synced
        { get { return State.Synced; } set { State.Synced = value; } }
        public bool Saved
        { get { return State.Saved; } set { State.Saved = value; } }

        public DataVaults()
        {
            BankName = "Vaults#" + DateTime.Now.ToFileTimeUtc().ToString();
            IsMain = false;

            Config = new DataConfig(this, DataStore.Bank);

            State = new DataState();
            Parameters = new DataParams();
            SerialCount = 0;
            DeserialCount = 0;
        }
        public DataVaults(string bankName, bool isMain = true)
        {
            BankName = bankName;
            IsMain = isMain;
            registry = DataBank.Registry;

            if (isMain)
                DataCore.Bank = this;

            Config = new DataConfig(this, DataStore.Bank);

            BankOn = DataBank.Vault;
            
            if (isMain)
                Config.SetMapFile();                         
           
            State = new DataState();
            Parameters = new DataParams();
            SerialCount = 0;
            DeserialCount = 0;
        }

        public SortedDictionary<string, DataVault> Vaults
        { get; set; } = new SortedDictionary<string, DataVault>();

        public DataVault this[string key]
        {
            get
            {
                DataVault result = null;
                Vaults.TryGetValue(key, out result);
                return result;
            }
            set
            {
                Vaults.AddOrUpdate(key, value, (k, v) => v = value);
            }
        }
        public object this[int index]
        {
            get
            {
                string key = null;
                DataVault result = null;
                if (index <= Keys.Count - 1)
                {
                    key = Keys.ElementAt(index);
                    if (key != null)
                        if (Vaults.TryGetValue(key, out result))
                            return result;
                }
                return result;
            }
            set
            {
                string key = null;
                if (index <= Keys.Count - 1)
                {
                    key = Keys.ElementAt(index);
                    if (key != null)
                        if (Vaults.ContainsKey(key))
                            Vaults[key] = (DataVault)value;
                }
            }
        }
        public DataTrellis this[string vault, string deposit]
        {
            get
            {
                DataVault nVault = null;
                DataDeposit nDeposit = null;
                if (Vaults.TryGetValue(vault, out nVault))
                    if (nVault.Have(deposit))
                    {
                        nDeposit = nVault[deposit];
                        return ((DataTrellis)nDeposit.Box.Prime);
                    }
                return null;
            }
            set
            {

                if (Vaults.ContainsKey(vault))
                    if (Vaults[vault].ContainsKey(deposit))                    
                        Vaults[vault][deposit].Box.Prime = value;                    
            }
        }

        public DataVault Create(string vaultId = null)
        {
            string DataVaultId = (vaultId != null) ? vaultId : BankName + "_" + (Vaults.Count + 1).ToString();
            DataVault value = new DataVault(DataVaultId);
            value.VaultsOn = this;            
            value.Config.SetMapConfig(this);       
            value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
            Vaults.AddOrUpdate(DataVaultId, value, (k, v) => v = value);

            return value;
        }

        public DataVaults BankOn
        {
            get { return bankOn; }
            set { bankOn = value; }
        }

        public DataVault  VaultUp
        {
            get { return vaultUp; }
            set { vaultUp = value; }
        }
        public DataVaults VaultsUp
        {
            get { return vaultsUp; }
            set { vaultsUp = value; }
        }

        #region IDictionary
        public DataVault Get(string key)
        {
            DataVault result = null;
            Vaults.TryGetValue(key, out result);
            return result;
        }
        public void Add(string key, DataVault value)
        {
            this.Add(new KeyValuePair<string, DataVault>(key, value));
        }
        public void Add(KeyValuePair<string, DataVault> item)
        {
            if (this.TryAdd(item.Key, item.Value))
            {
            }
        }
        public void Put(string key, DataVault value)
        {
            Vaults.AddOrUpdate(key, value, (k, v) => v = value);
            value.Config.SetMapConfig(this);         
        }
        public bool TryAdd(string key, DataVault value)
        {
            if (Vaults.TryAdd(key, value))
            {
                value.Config.SetMapConfig(this);               
                return true;
            }
            else
                return false;
        }
        public bool ContainsKey(string key)
        {
            return Vaults.ContainsKey(key);
        }
        public bool Remove(string key)
        {
            DataVault outset = null;
            bool done = Vaults.TryRemove(key, out outset);
            if (done)
            {
                object outreg = new object();
                DataBank.Registry.TryRemove(outset.Config.GetDataId(), out outreg);
            }
            return done;
        }
        public bool TryGetValue(string key, out DataVault value)
        {
            return Vaults.TryGetValue(key, out value);
        }
        public void Clear()
        {
            Vaults.Clear();
        }
        public bool Contains(KeyValuePair<string, DataVault> item)
        {
            return Vaults.Contains(item);
        }
        public void CopyTo(KeyValuePair<string, DataVault>[] array, int arrayIndex)
        {
            Vaults.ToArray().CopyTo(array, arrayIndex);
        }
        public bool Remove(KeyValuePair<string, DataVault> item)
        {
            DataVault outset = null;
            bool done = Vaults.TryRemove(item.Key, out outset);
            if (done)
            {
                object outreg = new object();
                DataBank.Registry.TryRemove(outset.Config.GetDataId(), out outreg);
            }
            return done;
        }

        public IEnumerator<KeyValuePair<string, DataVault>> GetEnumerator()
        {
            return Vaults.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Vaults.GetEnumerator();
        }

        public KeyValuePair<string, DataVault> SetPair
        {
            set
            {
                Add(value.Key, value.Value);
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return Vaults.Keys;
            }
        }
        public ICollection<DataVault> Values
        {
            get
            {
                return Vaults.Values;
            }
        }
        public int Count
        {
            get
            {
                return Vaults.Count;
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

        #region ICollection
        public int Add(object _set)
        {
            int index = -1;
            DataVault set = (DataVault)_set;
            if (!this.Have(set.VaultId))
            {
                if (this.TryAdd(set.VaultId, set))
                    index = Count - 1;
            }
            return index;
        }
        public void AddRange(List<DataVault> _sets)
        {           
                List<DataVault> newVault = _sets.Where(c => !this.Have(c.VaultId)).ToList();
                foreach (DataVault newDeposit in newVault)
                    this.TryAdd(newDeposit.VaultId, newDeposit);           
        }
        public void Remove(object value)
        {
            string key = null;
            DataVault tempset = (DataVault)value;
            Vaults.Select((x, y) => (ReferenceEquals(x.Value, value) ||
                           x.Value.VaultId.Equals(((DataVault)value).VaultId)) ?
                           key = x.Key : null).ToArray();
            if (key != null)
                Vaults.TryRemove(key, out tempset);
        }
        public void RemoveAt(int index)
        {
            int count = Keys.Count;
            string key = null;
            DataVault set = null;
            if (index >= count - 1)
            {
                key = Keys.ElementAt(index);
                Vaults.TryRemove(key, out set);
            }
        }
        public bool Have(string setId)
        {
            return this.ContainsKey(setId);
        }
        public DataVault AddNew()
        {
            DataVault set = (DataVault)((IBindingList)this).AddNew();
            return set;
        }
        public void DepositRange(DataVault[] data)
        {
            Vaults.AddOrUpdateRange(new ConcurrentDictionary<string, DataVault>(data.Select(d => new KeyValuePair<string, DataVault>(d.VaultId, d)).ToArray()));
        }
        public int IndexOf(object value)
        {
            int index = -1;
            Values.Select((x, y) => (ReferenceEquals(x, value) || x.VaultId.Equals(((DataDeposit)value).DepositId)) ? index = y : 0).ToArray();
            return index;
        }
        public void Insert(int index, object data)
        {
            string key = null;
            if (index >= Keys.Count - 1)
            {
                key = Keys.ElementAt(index);
                if (key != null)
                    if (Vaults.ContainsKey(key))
                    {
                        Vaults[key] = ((DataVault)data);
                        ((DataVault)data).Config.SetMapConfig(this);
                    }
            }
        }
        public DataVault Find(DataVault data)
        {
            if (Vaults.Values.Contains(data))
                return data;
            else if (Vaults.ContainsKey(data.VaultId))
                return Vaults[data.VaultId];
            else
                return null;    // Not found
        }
        public bool Contains(object data)
        {
            return (Find((DataVault)data) != null);
        }
        public void CopyTo(DataVault[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }
        public void CopyTo(Array array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }
        public object SyncRoot
        {
            get
            {
                return null;
            }
        }
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }
        public bool IsFixedSize
        { get { return false; } }
        #endregion

        #region IBindingList

        public object BindingSource { get { return this; } }
        public IDepotSync DepotSync { get { return (BoundedGrid != null) ? BoundedGrid.DepotSync : null; } set { if(BoundedGrid != null) BoundedGrid.DepotSync = value; } }
        [NonSerialized] private IDataGridStyle gridStyle;
        public IDataGridStyle GridStyle { get { return gridStyle; }  set { gridStyle = value; } }
        [NonSerialized] private IDataGridBinder boundedGrid;
        public IDataGridBinder BoundedGrid { get { return boundedGrid; } set { boundedGrid = value; } }


        IList IListSource.GetList()
        {
            return (IBindingList)BindingSource;
        }
        bool IListSource.ContainsListCollection
        {
            get
            {
                if (this.GetType().GetInterfaces().Contains(typeof(IDataGridBinder)))
                    if (BindingSource != null)
                        return true;
                return false;
            }
        }

        protected virtual void OnListChanged(ListChangedEventArgs ev)
        {
            if (onListChanged != null)
            {
                onListChanged(this, ev);
            }
        }
        protected void OnClear()
        {
            foreach (DataVault c in Values)
            {
                c.Deposit = null;
            }
        }
        protected void OnClearComplete()
        {
            OnListChanged(resetEvent);
        }
        protected void OnInsertComplete(int index, object value)
        {
            DataVault c = (DataVault)value;
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        }
        protected void OnRemoveComplete(int index, object value)
        {
            DataDeposit c = (DataDeposit)value;
            c.Box = null;

            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
        }
        protected void OnSet(int index, object oldValue, object newValue)
        {

        }
        protected void OnSetComplete(int index, object oldValue, object newValue)
        {
            if (oldValue != newValue)
            {
                object oldItem = oldValue;
                object newItem = newValue;

                oldItem = null;
                newItem = this;

                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
            }
        }

        bool IBindingList.AllowEdit
        {
            get { return true; }
        }
        bool IBindingList.AllowNew
        {
            get { return true; }
        }
        bool IBindingList.AllowRemove
        {
            get { return true; }
        }
        bool IBindingList.SupportsChangeNotification
        {
            get { return true; }
        }
        bool IBindingList.SupportsSearching
        {
            get { return false; }
        }
        bool IBindingList.SupportsSorting
        {
            get { return false; }
        }

        public event ListChangedEventHandler ListChanged
        {
            add
            {
                onListChanged += value;
            }
            remove
            {
                onListChanged -= value;
            }
        }

        event ListChangedEventHandler IBindingList.ListChanged
        {
            add
            {
                onListChanged += value;
            }

            remove
            {
                onListChanged -= value;
            }
        }

        object IBindingList.AddNew()
        {
            DataVault c = null;
            int number = 0;
            string numstr = "";
            bool found = false;
            while (!found)
            {
                if (ContainsKey("Deposit" + number))
                {
                    number++;
                    numstr = number.ToString();
                    if (number > 100)
                        found = true;
                }
                else
                {
                    c = new DataVault("Deposit" + numstr);
                    Vaults.TryAdd(c.VaultId, c);
                    found = true;
                }
            }
            return c;
        }
        bool IBindingList.IsSorted
        {
            get { return false; }
        }
        ListSortDirection IBindingList.SortDirection
        {
            get { return ListSortDirection.Ascending; }
        }

        PropertyDescriptor IBindingList.SortProperty
        {
            get { throw new NotSupportedException(); }
        }

        void IBindingList.AddIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }
        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {

            throw new NotSupportedException();
        }
        int IBindingList.Find(PropertyDescriptor property, object key)
        {
            throw new NotSupportedException();
        }
        void IBindingList.RemoveIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }
        void IBindingList.RemoveSort()
        {
            throw new NotSupportedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, e);
            }
        }

        [NonSerialized]
        private ListChangedEventArgs resetEvent = new ListChangedEventArgs(ListChangedType.Reset, -1);
        [NonSerialized]
        private ListChangedEventHandler onListChanged;
        #endregion

        #region ITypedList Members

        [NonSerialized]
        public ConcurrentDictionary<string, string> PropertyNaming =
           new ConcurrentDictionary<string, string>(
                 MAX_ACCESS_REQUEST_VALUE, HASHES_CAPACITY_STANDARD_VALUE);
        [NonSerialized]
        public static ConcurrentDictionary<string, PropertyDescriptorCollection> DescriptorRegistry =
                  new ConcurrentDictionary<string, PropertyDescriptorCollection>(
                      MAX_ACCESS_REQUEST_VALUE, HASHES_CAPACITY_STANDARD_VALUE);

        public PropertyDescriptorCollection propertyDescriptors
        {
            get
            {
                return CreateDescriptors(BankName, Config.DataIdx);
            }
            set
            {
                CreateDescriptors(BankName, Config.DataIdx, value);
            }
        }

        private PropertyDescriptorCollection createDescriptors()
        {
            PropertyDescriptorCollection origProperties = TypeDescriptor.GetProperties(typeof(DataVault));

            ArrayList properties = new ArrayList();
            HashSet<string> handledNames = new HashSet<string>();

            foreach (PropertyDescriptor desc in origProperties)
                if (typeof(ICollection).IsAssignableFrom(desc.PropertyType))
                {
                    if (desc.Name == "Deposit")
                    {
                        if (handledNames.Add("Deposit"))
                            properties.Add(new DataVaultDescriptor("Deposit's Inside", "Deposit's Inside"));
                    }
                    else if (desc.Name == "VaultsIn")
                    {
                        if (handledNames.Add("VaultsIn"))
                            properties.Add(new DataVaultsInDescriptor("Vault's Inside", "Vault's Inside"));
                    }
                }

            List<string> propnames = new List<string>() { "VaultId", "DisplayName" };
            foreach (string propname in propnames)
            {
                if (handledNames.Add(propname))
                    properties.Add(origProperties[propname]);
            }
            return new PropertyDescriptorCollection((PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor)));
        }
        public PropertyDescriptorCollection CreateDescriptors(string name, string _dataId, PropertyDescriptorCollection _descriptors = null)
        {
            string dataId = _dataId;
            PropertyNaming.AddOrUpdate(name, _dataId, (k, v) => v = dataId);

            if (_descriptors != null)
            {
                if (!DescriptorRegistry.ContainsKey(dataId))
                    return DescriptorRegistry.GetOrAdd(dataId, _descriptors);
                else
                    return DescriptorRegistry[dataId];
            }
            else if (!DescriptorRegistry.ContainsKey(Config.DataIdx))
                return DescriptorRegistry.GetOrAdd(Config.DataIdx, createDescriptors());
            else
                return DescriptorRegistry[Config.DataIdx];
        }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            PropertyDescriptorCollection descriptors = null;
            if (listAccessors != null)
            {
                int position = listAccessors.Length - 1;
                string dataId = null;
                if (PropertyNaming.TryGetValue(listAccessors[position].Name, out dataId))
                    if (!DescriptorRegistry.TryGetValue(dataId, out descriptors))
                        descriptors = CreateDescriptors(listAccessors[position].Name, dataId);
            }
            else
                descriptors = propertyDescriptors;

            return descriptors;
        }

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            string listName = "";
            if (listAccessors != null)
            {
                int position = listAccessors.Length - 1;
                listName = listAccessors[position].Name;
            }
            else
                listName = BankName;
            return listName;
        }

        #endregion

        #region Serialization

        public int Serialize(Stream tostream, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        {
            return this.SetRaw(tostream);
        }
        public int Serialize(ISerialContext buffor, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        {
            return this.SetRaw(buffor);
        }

        public object Deserialize(Stream fromstream, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        {
            return this.GetRaw(fromstream);
        }
        public object Deserialize(ref object fromarray, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        {
            return this.GetRaw(ref fromarray);
        }

        public object[] GetMessage()
        {
            return null;
        }
        public object   GetHeader()
        {
            return this;
        }

        public byte[] GetShah()
        {
            return (Config.Place != null) ? Config.Place.GetShah() : null;
        }

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
            return Config.Path ?? BankName;
        }
        public string GetMapName()
        {
            return Config.Path + "/" + BankName + ".ndb";
        }
       
        public int SerialCount { get; set; }
        public int DeserialCount { get; set; }
        public int ProgressCount { get; set; }
        public int ItemsCount { get { return Count; } }

        #endregion

        #region  ExpansionTools

        public object Emulator(object source, string name = null)
        {
            throw new NotImplementedException();
        }
        public object Imitator(object source, string name = null)
        {
            throw new NotImplementedException();
        }
        public object Impactor(object source, string name = null)
        {
            bool save = ((IDataConfig)source).State.Saved;
            return this.Impact((DataVaults)source, save);
        }
        public object Locator(string path = null)
        {
            return this.Locate(path);
        }

        #endregion

        #region IDriveRecorder
        [NonSerialized]
        private IDrive drive;
        public IDrive Drive
        { get { return drive; } set { drive = value; } }
        public void WriteDrive()
        {
            State.Saved = false;
            if (Drive == null) OpenDrive();
            DriveContext dc = new DriveContext();
            Serialize(dc, 0, 0);
            dc.WriteDrive(Drive);
            dc.Dispose();           

            foreach (object obj in DataBank.Registry.Values)
            {
                Type[] ifaces = obj.GetType().GetInterfaces();
                if (ifaces.Contains(typeof(IDriveRecorder)) && ifaces.Contains(typeof(IDataConfig)))
                    if (!ReferenceEquals(obj, this) && obj.GetType() == typeof(DataTiers))
                    {
                        if (((IDataConfig)obj).State.Saved)
                        {
                            IDriveRecorder recobj = (IDriveRecorder)obj;
                            if (recobj.Drive == null)
                                recobj.OpenDrive();
                            recobj.WriteDrive();
                        }
                    }
            }
        }
        public void ReadDrive()
        {
            if (Drive == null) OpenDrive();
            DriveContext dc = new DriveContext();
            object read = dc.ReadDrive(Drive);
            DataBank.Vault.Config.DepotId = Quid.Empty;
            DataBank.Vault.Impact((DataVaults)Deserialize(ref read));           
            dc.Dispose();

            foreach (object obj in DataBank.Registry.Values)
            {
                if (obj.GetType().GetInterfaces().Contains(typeof(IDriveRecorder)))
                    if (!ReferenceEquals(obj, this) && obj.GetType() == typeof(DataTiers))
                    {
                        if (!((DataTiers)obj).Trell.IsCube)
                        {
                            IDriveRecorder recobj = (IDriveRecorder)obj;
                            if (recobj.Drive == null)
                                recobj.OpenDrive();
                            recobj.ReadDrive();
                        }
                    }
            }
        }
        public bool TryReadDrive()
        {
            if (TryOpenDrive())
            {
                ReadDrive();
                return true;
            }
            return false;
        }
        public void OpenDrive()
        {
            if(Drive == null)
                Drive = new DriveBank(Config.File, Config.File, 20 * 1024 * 1024, typeof(DataVaults));
        }
        public bool TryOpenDrive()
        {
            if (Drive == null)
                Drive = new DriveBank(Config.File, Config.File, 20 * 1024 * 1024, typeof(DataVaults));
            return Drive.Exists;
        }
        public void CloseDrive()
        {
            if (Drive != null)
                Drive.Close();
            Drive = null;
        }
        #endregion

        #region IDataBank
        public IDataLog DataLog
        {
            get { return DataBank.DataLog; }
            set { DataBank.DataLog = value; }
        }
        public IDataLog NetLog
        {
            get { return DataBank.NetLog; }
            set { DataBank.NetLog = value; }
        }

        public DepotIdentity ServiceIdentity
        {
            get
            {
                return DataBank.ServiceIdentity;

            }
            set { DataBank.ServiceIdentity = value; }
        }
        public DepotIdentity ServerIdentity
        {
            get
            {
                return DataBank.ServerIdentity;

            }
            set
            {
                DataBank.ServerIdentity = value;
               
            }
        }

        public string DrivePath
        {
            get
            {
                return DataBank.DrivePath;

            }
            set
            {
                DataBank.DrivePath = value;

            }
        }
        public ConcurrentDictionary<int, object> Registry
        {
            get
            {
                return registry;

            }
            set
            {
                registry = value;

            }
        }
        #endregion

        #region IDataTreeBinder

        public string TreeNodeName
        {
            get { return BankName; }
        }
        public object TreeNodeTag
        {
            get { return this; }
        }
        public IDataTreeBinder[] TreeNodeChilds
        {
            get { return Vaults.Select(a => (IDataTreeBinder)a.Value).ToArray(); }
        }
        public IDataTreeBinder BoundedTree
        { get { return boundedTree; } set { boundedTree = value; } }

        #endregion
    }

    public class DataVaultDescriptor : PropertyDescriptor
    {
        public DataVaultDescriptor(string name, string display) : base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name) })
        {
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataVault); } }
        public override bool IsReadOnly { get { return true; } }
        public override Type PropertyType { get { return typeof(DataVault); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
        }
        public override object GetValue(object component)
        {
            
            DataVault child = ((DataVault)component);
            if (child != null)
            {
                PropertyDescriptorCollection pdc = child.CreateDescriptors(Name, child.Config.DataIdx);

                DataVaults vaults = child.VaultsOn;
                if (vaults != null)
                {
                    DataVaults tempvaults = vaults;
                    while (vaults != null)
                    {
                        pdc = vaults.CreateDescriptors(Name, child.Config.DataIdx, pdc);
                        tempvaults = vaults;
                        vaults = vaults.VaultsUp;
                    }
                    vaults = tempvaults;
                    if (vaults.BankOn != null)
                        pdc = vaults.BankOn.CreateDescriptors(Name, child.Config.DataIdx, pdc);
                }

                if (((IDataGridBinder)component).BoundedGrid != null)
                    ((IDataGridBinder)child).BoundedGrid = ((IDataGridBinder)component).BoundedGrid;

            }
            return child;
        }
    }

    public class DataVaultsInDescriptor : PropertyDescriptor
    {
        public DataVaultsInDescriptor(string name, string display) : base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name) })
        {

        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataVault); } }
        public override bool IsReadOnly { get { return true; } }
        public override Type PropertyType { get { return typeof(DataVaults); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
        }
        public override object GetValue(object component)
        {
            DataVaults child = ((DataVault)component).VaultsIn;
            if (child == null)
                child = ((DataVault)component).VaultsOn;

            if (child != null)
            {
                DataVault vaultOn = child.VaultUp;
                PropertyDescriptorCollection pdc = child.CreateDescriptors(Name, child.Config.DataIdx);
                DataVaults vaults = child;

                if (vaultOn != null)
                    vaultOn.CreateDescriptors(Name, child.Config.DataIdx, pdc);

                DataVaults tempvaults = vaults;
                while (vaults != null)
                {
                    pdc = vaults.CreateDescriptors(Name, child.Config.DataIdx, pdc);
                    tempvaults = vaults;
                    vaults = vaults.VaultsUp;
                }
                vaults = tempvaults;
                if (vaults.BankOn != null)
                    pdc = vaults.BankOn.CreateDescriptors(Name, child.Config.DataIdx, pdc);

                if (((IDataGridBinder)component).BoundedGrid != null)
                    ((IDataGridBinder)child).BoundedGrid = ((IDataGridBinder)component).BoundedGrid;

            }
            return child;
        }
    }
}