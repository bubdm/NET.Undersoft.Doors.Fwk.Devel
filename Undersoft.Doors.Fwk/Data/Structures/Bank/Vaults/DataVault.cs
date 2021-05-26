using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Doors;
using System.Doors.Drive;

namespace System.Doors.Data
{
    [Serializable]
    public class DataVault : IDictionary<string, DataDeposit>, IEnumerable<KeyValuePair<string, DataDeposit>>, IDriveRecorder,
                             IDataSerial, IDataMorph, IListSource, ITypedList, IDataGridBinder, IDataTreeBinder, IBindingList, 
                             IList, INoid, IDataConfig
    {
        private static int HASHES_CAPACITY_STANDARD_VALUE = 51;                                   // probably there is no need for more then 100 values in hashes, +1 cause 0based count
        private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;             // established max count for access requests to hashes

        [NonSerialized] private IDataTreeBinder boundedTree;
        [NonSerialized] private DataVault vaultUp;
        [NonSerialized] private DataVaults vaultsOn;
        private DataVaults vaultsIn;

        public string VaultId
        { get; set; }
        public string DisplayName
        { get; set; }

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

        [NonSerialized]
        private IDrive drive;
        public IDrive Drive
        { get { return drive; } set { drive = value; } }

        public DataVault()
        {
            VaultId = "Vault#" + DateTime.Now.ToFileTimeUtc().ToString();
            DisplayName = VaultId;
            Config = new DataConfig(this, DataStore.Bank);
            State = new DataState();
            Parameters = new DataParams();
        }
        public DataVault(string _VaultName)
        {
            VaultId = _VaultName;
            DisplayName = _VaultName;
            Config = new DataConfig(this, DataStore.Bank);
            State = new DataState(); 
            Parameters = new DataParams();
        }
      
        public SortedDictionary<string, DataDeposit> Deposit
        { get; set; } = new SortedDictionary<string, DataDeposit>();

        public DataDeposit this[string key]
        {
            get
            {
                DataDeposit result = null;
                Deposit.TryGetValue(key, out result);
                return result;
            }
            set
            {                          
                if (Deposit.ContainsKey(key))
                {
                    if(!value.Vault.Equals(this))
                        value.Vault = this;
                    Deposit[key] = value;
                }
            }
        }
        public object this[int index]
        {
            get
            {
                string key = null;
                DataDeposit result = null;
                if (index <= Keys.Count - 1)
                {
                    key = Keys.ElementAt(index);
                    if (key != null)
                        if (Deposit.TryGetValue(key, out result))
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
                        if (Deposit.ContainsKey(key))
                        {
                            if(!((DataDeposit)value).Vault.Equals(this))
                                ((DataDeposit)value).Vault = this;
                            Deposit[key] = (DataDeposit)value;
                        }
                }
            }
        }
        public DataDeposit this[string vault, string deposit]
        {
            get
            {
                DataVault _vault = null;
                DataDeposit _deposit = null;
                if (VaultsIn.TryGetValue(vault, out _vault))
                    _vault.TryGetValue(deposit, out _deposit);
                return _deposit;
            }
            set
            {
                if (VaultUp != null)
                {
                    DataParams p = new DataParams();
                    p.Registry = new Dictionary<string, object>(VaultUp.Parameters.Registry);
                    value.Parameters = p;
                }

                if (VaultsIn.ContainsKey(vault))
                    if (VaultsIn[vault].ContainsKey(deposit))
                        VaultsIn[vault][deposit] = value;
            }
        }

        public DataDeposit Create(string dataName = null, DataDepositType depositType = DataDepositType.Bussines, 
                               DataConfig config = null, DataParams parameters = null)
        {
            string DataPlace = (dataName != null) ? dataName : 
                               VaultId + "_" + (Count + 1).ToString();
            DataDeposit value = new DataDeposit(DataPlace, this);

            if (config != null)
                value.Config = config;
            if(parameters != null)
                value.Parameters.Registry.AddRange(new Dictionary<string, object>(parameters.Registry));         
            value.Vault = this;
            this.Put(DataPlace, value);
            return value;
        }

        public DataVault CreateWithVlts(string vaultId = null)
        {
            DataVaults  vlts = NewVaults(vaultId + "_Vaults");

            string _vaultId = (vaultId != null) ? vaultId : VaultId + "_" + (vlts.Count + 1).ToString();
            return vlts.Create(vaultId);     
        }

        public DataVaults VaultsOn
        {
            get
            {
                return vaultsOn;
            }
            set
            {
                vaultsOn = value;          
            }
        }
        
        public DataVaults VaultsIn
        {
            get
            {
                return vaultsIn;
            }
            set
            {
                vaultsIn = value;
                value.VaultUp = this;
                value.VaultsUp = VaultsOn;
                value.Config.SetMapConfig(this);             
                value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
            }
        }

        public DataVault  VaultUp
        {
            get
            {
                return vaultUp;
            }
            set
            {
                vaultUp = value;
            }
        }
     
        public DataDeposit NewDeposit(string iDepositId = null, bool useSqlLocal = true)
        {
            string DepositId = (iDepositId != null) ? iDepositId : VaultId + "_" + (Deposit.Count + 1).ToString();
            DataDeposit value = new DataDeposit(DepositId, this);
            Put(DepositId, value);
            return value;
        }
        public DataVaults NewVaults(string vaults = null)
        {
            string vaultsId = (vaults != null) ? vaults : VaultId + "_" + (Deposit.Count + 1).ToString();
            DataVaults value = new DataVaults(vaultsId, false);
            value.VaultUp = this;
            value.VaultsUp = this.VaultsOn;
            VaultsIn = value;            
            return value;
        }

        #region IDictionary
        public DataDeposit Get(string key)
        {
            DataDeposit result = null;
            Deposit.TryGetValue(key, out result);
            return result;
        }
        public void Add(string key, DataDeposit value)
        {          
            value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));        
            value.Vault = this;
            this.Add(new KeyValuePair<string, DataDeposit>(key, value));
        }
        public void Add(KeyValuePair<string, DataDeposit> item)
        {
            if (this.TryAdd(item.Key, item.Value))
            {               
            }
        }
        public bool TryAdd(string key, DataDeposit value)
        {
            if (Deposit.TryAdd(key, value))
            {
                value.Config.SetMapConfig(this);
                value.Box.Config.SetMapConfig(value);           
                return true;
            }
            else
                return false;
        }
        public void Put(string key, DataDeposit value)
        {        
            value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));         
            value.Vault = this;
            Deposit.AddOrUpdate(key, value, (k, v) => v = value);
            value.Config.SetMapConfig(this);          
            value.Box.Config.SetMapConfig(value);          
        }
        public bool ContainsKey(string key)
        {
            return Deposit.ContainsKey(key);
        }
        public bool Remove(string key)
        {
            DataDeposit outset = null;
            bool done = Deposit.TryRemove(key, out outset);
            if (done)
            {
                object outreg = null;
                DataBank.Registry.TryRemove(outset.Config.GetDataId(), out outreg);
                DataBank.Registry.TryRemove(outset.Trell.Config.GetDataId(), out outreg);
                DataBank.Registry.TryRemove(outset.Trell.Tiers.Config.GetDataId(), out outreg);
                DataSpace.Registry.TryRemove(outset.Trell.Config.GetDataId(), out outreg);
                DataSpace.Registry.TryRemove(outset.Trell.Tiers.Config.GetDataId(), out outreg);
                outset.CloseDrive();
                outset.Trell.CloseDrive();
                outset.Trell.Tiers.Registry.CloseDrive();
                outset.Config.DeleteDirectory();
            }
            return done;
        }
        public bool TryGetValue(string key, out DataDeposit value)
        {
            return Deposit.TryGetValue(key, out value);
        }
        public void Clear()
        {
            Deposit.Clear();
        }
        public bool Contains(KeyValuePair<string, DataDeposit> item)
        {
            return Deposit.Contains(item);
        }
        public void CopyTo(KeyValuePair<string, DataDeposit>[] array, int arrayIndex)
        {
            Deposit.ToArray().CopyTo(array, arrayIndex);
        }
        public bool Remove(KeyValuePair<string, DataDeposit> item)
        {
            DataDeposit outset = null;
            bool done = Deposit.TryRemove(item.Key, out outset);
            if (done)
            {
                object outreg = new object();
                DataBank.Registry.TryRemove(outset.Config.GetDataId(), out outreg);
            }

            return done;
        }

        public IEnumerator<KeyValuePair<string, DataDeposit>> GetEnumerator()
        {
            return Deposit.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Deposit.GetEnumerator();
        }

        public KeyValuePair<string, DataDeposit> SpherePair
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
                return Deposit.Keys;
            }
        }
        public ICollection<DataDeposit> Values
        {
            get
            {
                return Deposit.Values;
            }
        }
        public int Count
        {
            get
            {
                return Deposit.Count;
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
            DataDeposit set = (DataDeposit)_set;
            if (!this.Have(set.DepositId))
            {
                if (this.TryAdd(set.DepositId, set))
                    index = Count - 1;
            }
            return index;
        }
        public void AddRange(List<DataDeposit> _sets)
        {
            try
            {
                List<DataDeposit> newDeposit = _sets.Where(c => !this.Have(c.DepositId)).ToList();
                foreach (DataDeposit newSphere in newDeposit)
                {
                    newSphere.Vault = this;
                    this.TryAdd(newSphere.DepositId, newSphere);
                }
            }
            catch (Exception ex)
            { }
        }
        public void Remove(object value)
        {
            string key = null;
            DataDeposit tempset = (DataDeposit)value;
            Deposit.Select((x, y) => (ReferenceEquals(x.Value, value) || x.Value.DepositId.Equals(((DataDeposit)value).DepositId)) ? key = x.Key : null).ToArray();
            if (key != null)
                Deposit.TryRemove(key, out tempset);
        }
        public void RemoveAt(int index)
        {
            int count = Keys.Count;
            string key = null;
            DataDeposit set = null;
            if (index >= count - 1)
            {
                key = Keys.ElementAt(index);
                Deposit.TryRemove(key, out set);
            }
        }
        public bool Have(string setId)
        {
            return this.ContainsKey(setId);
        }
        public DataDeposit AddNew()
        {
            DataDeposit set = (DataDeposit)((IBindingList)this).AddNew();
            return set;
        }
        public void SphereRange(DataDeposit[] data)
        {
            Deposit.AddOrUpdateRange(data.Select(d => new KeyValuePair<string, DataDeposit>(d.DepositId, d)).ToDictionary(k => k.Key, v => v.Value));
        }
        public int IndexOf(object value)
        {
            int index = -1;
            Values.Select((x, y) => (ReferenceEquals(x, value) || x.DepositId.Equals(((DataDeposit)value).DepositId)) ? index = y : 0).ToArray();
            return index;
        }
        public void Insert(int index, object data)
        {
            string key = null;
            if (index >= Keys.Count - 1)
            {
                key = Keys.ElementAt(index);
                if (key != null)
                    if (Deposit.ContainsKey(key))
                    {
                        Deposit[key] = (DataDeposit)data;
                        ((DataDeposit)data).Config.SetMapConfig(this);
                        ((DataDeposit)data).Box.Config.SetMapConfig(((DataDeposit)data));
                    }
            }
        }
        public DataDeposit Find(DataDeposit data)
        {
            if (Deposit.Values.Contains(data))
                return data;
            else if (Deposit.ContainsKey(data.DepositId))
                return Deposit[data.DepositId];
            else
                return null;    // Not found
        }
        public bool Contains(object data)
        {
            return (Find((DataDeposit)data) != null);
        }
        public void CopyTo(DataDeposit[] array, int arrayIndex)
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
        public bool IsFixedSize { get { return false; } }
        #endregion

        #region IBindingList

        public object BindingSource
        {
            get
            {
                if (VaultsIn != null)
                    return VaultsIn;
                else
                    return this;
            }
        }
        public IDepotSync DepotSync { get { return BoundedGrid.DepotSync; } set { BoundedGrid.DepotSync = value; } }
        public IDataGridStyle GridStyle { get; set; }       
        public IDataGridBinder BoundedGrid { get { return VaultsOn.BoundedGrid; } set { VaultsOn.BoundedGrid = value; } }

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
            foreach (DataDeposit c in Values)
            {
                c.Box = null;
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
            DataDeposit c = null;
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
                    c = new DataDeposit("Deposit" + numstr, this);
                    Deposit.TryAdd(c.DepositId, c);
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
                return CreateDescriptors(VaultId, Config.DataIdx);
            }
            set
            {
                CreateDescriptors(VaultId, Config.DataIdx, value);
            }
        }

        private PropertyDescriptorCollection createDescriptors()
        {
            PropertyDescriptorCollection origProperties = TypeDescriptor.GetProperties(typeof(DataDeposit));

            ArrayList properties = new ArrayList();
            HashSet<string> handledNames = new HashSet<string>();

            foreach (PropertyDescriptor desc in origProperties)
                if (desc.Name == "Box")
                {
                    if (handledNames.Add("Box"))
                        properties.Add(new DataBoxDescriptor("Prime Tiers", "Prime Tiers"));
                }              


            List<string> propnames = new List<string>() { "DepositId", "DisplayName", "DataIdx", "SqlQuery", "Count" };
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
            if(PropertyNaming == null)
                PropertyNaming =  new ConcurrentDictionary<string, string>(MAX_ACCESS_REQUEST_VALUE, HASHES_CAPACITY_STANDARD_VALUE);

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
                listName = VaultId;
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
            DataTiers[] tiers = Deposit.Where(b => b.Value.Box.Prime != null)
                                    .SelectMany(a => (DataTiers[])a.Value.Box.Prime.GetMessage()).ToArray();          
            return tiers;
        }
        public object GetHeader()
        {
            return this;
        }

        public int    GetKeyHash()
        {
            return (Config.Place != null) ? Config.Place.GetHashCode() : 0;
        }

        public byte[] GetShah()
        {
            return (Config.Place != null) ? Config.Place.GetShah() : null;
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
            return (Config.Path != null) ? Config.Path : Config.Path + "/" + VaultId ;
        }
        public string GetMapName()
        {
            return Config.Path + "/" + VaultId + ".vlt";
        }
        public int    GetKeyShah()
        {
            return (Config.Place != null) ? Config.Place.Reverse<char>().ToString().GetHashCode() : 0;
        }
        public ushort GetDriveId()
        {
            return 0;
        }
        public ushort GetStateBits()
        {
            return State.ToUInt16();
        }

        public int SerialCount { get; set; }
        public int DeserialCount { get; set; } public int ProgressCount { get; set; }
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
           return this.Impact((DataVault)source);
        }
        public object Locator(string path = null)
        {
            return this.Locate(path);
        }
        #endregion

        #region IDriveRecorder
        public void WriteDrive()
        {
            State.Saved = false;
            if (Drive == null) OpenDrive();
            DriveContext dc = new DriveContext();
            Serialize(dc, 0, 0);
            dc.WriteDrive(Drive);
            dc.Dispose();
        }
        public void ReadDrive()
        {
            if (Drive == null) OpenDrive();
            DriveContext dc = new DriveContext();
            object read = dc.ReadDrive(Drive);
            DataVault sphrs = (DataVault)Deserialize(ref read);
            this.Impact(sphrs);
            State.Saved = false;
            dc.Dispose();
        }
        public void OpenDrive()
        {
            if(Drive == null)
                Drive = new DriveBank(Config.File, Config.File, 10 * 1024 * 1024, typeof(DataSphere));
        }
        public void CloseDrive()
        {
            if (Drive != null)
                Drive.Close();
            Drive = null;
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
        public bool TryOpenDrive()
        {
            OpenDrive();
            return Drive.Exists;
        }
        #endregion

        #region IDataTreeBinder

        public string TreeNodeName
        {
            get { return VaultId; }
        }
        public object TreeNodeTag
        {
            get { return this; }
        }
        public IDataTreeBinder[] TreeNodeChilds
        {
            get { return (VaultsIn != null) ? VaultsIn.Select(a => (IDataTreeBinder)a.Value).ToArray() : null; }
        }
        public IDataTreeBinder BoundedTree
        { get { return boundedTree; } set { boundedTree = value; } }

        #endregion
    }

    public class DataBoxDescriptor : PropertyDescriptor
    {
        public DataBoxDescriptor(string name, string display) : base(name, new Attribute[] { new DataIdxAttribute((display != null) ? display : name) })
        {

        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataDeposit); } }
        public override bool IsReadOnly { get { return true; } }
        public override Type PropertyType { get { return typeof(DataTiers); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
        }
        public override object GetValue(object component)
        {
            DataBox child = ((DataDeposit)component).Box;
            if (child != null)
                if (child.Prime != null)
                {
                    DataVault vaultOn = child.DepositOn.Vault;
                    PropertyDescriptorCollection pdc = child.Prime.Tiers.CreateDescriptors(Name, child.Config.DataIdx);
                    if (vaultOn != null)
                    {
                        vaultOn.CreateDescriptors(Name, child.Config.DataIdx, pdc);

                        DataVaults vaults = child.DepositOn.Vault.VaultsOn;
                       
                        if (vaults != null)
                        {
                            DataVaults tempvaults = vaults;
                            while (vaults != null)
                            {
                                pdc = vaults.CreateDescriptors(Name, child.Prime.Config.DataIdx, pdc);
                                tempvaults = vaults;
                                vaults = vaults.VaultsUp;
                            }
                            vaults = tempvaults;
                            if (vaults.BankOn != null)
                                pdc = vaults.BankOn.CreateDescriptors(Name, child.Config.DataIdx, pdc);
                        }
                    }

                    return child.Prime.Tiers;
                }
            return null;
        }
    }

}
