using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Doors;
using System.Doors.Drive;

namespace System.Doors.Data
{
    [JsonArray]
    [Serializable]
    public class DataTrellises : Collection<DataTrellis>, IBindingList, ITypedList, IDataSerial, IDataMorph, IDataTrellises,
                                                          IListSource, ICollection, IDataGridBinder, INoid, IDataConfig, IDataDescriptor
    {
        #region Private / NonSerialized
        [NonSerialized] private static int HASHES_CAPACITY_STANDARD_VALUE = 51;                           // probably there is no need for more then 50 values in hashes
        [NonSerialized] private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;     // established max count for access requests to hashes
        [NonSerialized] private ListChangedEventArgs resetEvent = new ListChangedEventArgs(ListChangedType.Reset, -1);
        [NonSerialized] private ListChangedEventHandler onListChanged;
        [NonSerialized] private IDataGridBinder boundedGrid;
        [NonSerialized] private IDrive drive;
        [NonSerialized] private DataSphere sphereOn;

        #endregion

        public DataTrellises()
        {
            Config = new DataConfig(this, DataStore.Space);
            State = new DataState();
        }
        public DataTrellises(DataSphere sphereOn)
        {
            SphereOn = sphereOn;
            Config = new DataConfig(this, DataStore.Space);
            State = new DataState();          
        }

        public string SphereId
        { get; set; }

        public DataConfig Config
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

        public DataSphere SphereOn
        { get { return sphereOn; } set { sphereOn = value; } }    

        public DataTrellis this[string trellName]
        {
            get
            {
                DataTrellis[] trell = this.Where(c => trellName == c.TrellName).ToArray();
                if (trell.Length > 0)
                    return trell[0];
                else
                    return null;
            }
            set
            {
                DataTrellis[] trell = this.AsEnumerable().Where(c => trellName == c.TrellName).ToArray();
                if (trell.Length > 0)
                    base.SetItem(IndexOf(trell[0]), value);
            }
        }      

        public DataTrellis   Collect(string trellName)
        {
            List<DataTrellis> gtab = this.Where(c => trellName == c.TrellName).ToList();
            if (gtab.Count > 0)
                return gtab.First();
            else
                return null;
        }
        public DataTrellis[] Collect(string[] trellNames)
        {
            DataTrellis[] gtabs = this.Where(c => trellNames.Contains(c.TrellName)).ToArray();
            if (gtabs.Length > 0)
                return gtabs;
            else
                return null;
        }

        #region  ExpansionTools

        public object Emulator(object source, string name = null)
        {
            DataSphere ds = null;
            if (this.SphereOn != null)            
                ds = this.SphereOn.Emulate(((DataTrellises)source).SphereOn);
            if (ds != null)
                return ds.Trells;
            else
                return null;
        }
        public object Imitator(object source, string name = null)
        {
            DataSphere ds = null;
            if (this.SphereOn != null)
                ds = this.SphereOn.Imitate(((DataTrellises)source).SphereOn);
            if (ds != null)
                return ds.Trells;
            else
                return null;
        }
        public object Impactor(object source, string name = null)
        {
           return this.Impact((DataTrellises)source, true);
        }
        public object Locator(string path = null)
        {
            return this.Locate(path);
        }

        #endregion

        #region Serialization
        public int Serialize(Stream tostream, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        {
            if (SphereOn != null)
            {
                if (serialFormat == SerialFormat.Raw)
                    return SphereOn.Serialize(tostream, offset, batchSize);
                else if (serialFormat == SerialFormat.Json)
                    return SphereOn.Serialize(tostream, offset, batchSize, SerialFormat.Json);
                else
                    return -1;
            }
            else
            {
                if (serialFormat == SerialFormat.Raw)
                    return this.SetRaw(tostream);
                else if (serialFormat == SerialFormat.Json)
                    return this.SetJson(tostream);
                else
                    return -1;
            }
        }
        public int Serialize(ISerialContext buffor, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        {
            if (SphereOn != null)
            {
                if (serialFormat == SerialFormat.Raw)
                    return SphereOn.Serialize(buffor, offset, batchSize);
                else if (serialFormat == SerialFormat.Json)
                    return SphereOn.Serialize(buffor, offset, batchSize, SerialFormat.Json);
                else
                    return -1;
            }
            else
            {
                if (serialFormat == SerialFormat.Raw)
                    return this.SetRaw(buffor);
                else if (serialFormat == SerialFormat.Json)
                    return this.SetJson(buffor);
                else
                    return -1;
            }
        }

        public object Deserialize(Stream fromstream, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        {
            if (SphereOn != null)
            {
                if (serialFormat == SerialFormat.Raw)
                    return SphereOn.Deserialize(fromstream, driveon);
                else if (serialFormat == SerialFormat.Json)
                    return SphereOn.Deserialize(fromstream, driveon, SerialFormat.Json);
                else
                    return -1;
            }
            else
            {
                if (serialFormat == SerialFormat.Raw)
                    return this.GetRaw(fromstream);
                else if (serialFormat == SerialFormat.Json)
                    return this.GetJson(fromstream);
                else
                    return -1;
            }
        }
        public object Deserialize(ref object fromarray, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        {
            if (SphereOn != null)
            {
                if (serialFormat == SerialFormat.Raw)
                    return SphereOn.Deserialize(ref fromarray, driveon);
                else if (serialFormat == SerialFormat.Json)
                    return SphereOn.Deserialize(ref fromarray, driveon, SerialFormat.Json);
                else
                    return -1;
            }
            else
            {
                if (serialFormat == SerialFormat.Raw)
                    return this.GetRaw(ref fromarray);
                else if (serialFormat == SerialFormat.Json)
                    return this.GetJson(ref fromarray);
                else
                    return -1;
            }
        }

        public object[] GetMessage()
        {
            DataTiers[] result =  this.AsEnumerable().SelectMany(t => (DataTiers[])t.GetMessage()).ToArray();         
            return result;
        }
        public object GetHeader()
        {
            if (SphereOn != null)
                return SphereOn;
            else
                return this;
        }

        public int SerialCount { get; set; }
        public int DeserialCount { get; set; }
        public int ProgressCount { get; set; }
        public int ItemsCount { get { return this.Count; } }

        public int    GetKeyHash()
        {
            return (Config.Path != null) ? Config.Path.GetHashCode() : 0;
        }
        public int    GetKeyShah()
        {
            return 0;
        }

        public byte[] GetShah()
        {
            return (Config.Place != null) ? Config.Place.GetShah() : null;
        }

        public ushort GetDriveId()
        {
            return 0;
        }
        public ushort GetStateBits()
        {
            return State.ToUInt16();
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
            return (Config.Path != null) ? Config.Path : (SphereOn != null) ? SphereOn.Config.Path : SphereOn.SphereId;
        }
        public string GetMapName()
        {
            return Config.Path + "/" + SphereOn.SphereId + ".trls";
        }
        public IDrive Drive
        { get { return drive; } set { drive = value; } }
        #endregion

        #region ICollection
        public DataTrellis AddNew()
        {
            return (DataTrellis)((IBindingList)this).AddNew();
        }
        public new int Add(DataTrellis trell)
        {
            int index = -1;
            if (trell != null)
                if (!this.Have(trell.TrellName) || !this.Have(trell.Config))
                {
                    //if (trell.Deposit != null)
                        index = ((IList)this).Add(trell);
                }  
            return index;
        }
        public DataTrellis Put(DataTrellis trell)
        { 
            if (!this.Have(trell.TrellName) || !this.Have(trell.Config))
            {
                base.Add(trell);
                return trell;
            }
            return null;
        }
        public void AddRange(List<DataTrellis> _trells)
        {
            try
            {
                foreach (DataTrellis newTable in _trells)
                    this.Add(newTable);
            }
            catch (Exception ex)
            { }
        }
        public void RemoveRange(string[] trellNames)
        {
            ICollection<DataTrellis> trells = this.Collect(trellNames);
            foreach (DataTrellis trell in trells)
                Remove(trell);
        }
        public bool Have(string trellName)
        {
            return this.Where(c => c.TrellName == trellName).Any();
        }
        public bool Have(DataConfig TrellConfig)
        {
            return this.Where(c => c.Config.DataIdx.Equals(TrellConfig.DataIdx)).Any();
        }
        public void SetRange(DataTrellis[] data)
        {
            if (Count != data.Length || Count == 0)
            {
                Clear();
                ((List<DataTrellis>)Items).AddRange(data);
                return;
            }
            data.Select((x,y) => this[y] = x).ToArray();
        }
        public int IndexOf(string TrellName)
        {
            int index = -1;
            DataTrellis trell = this[TrellName];
            if(trell != null)
                index = IndexOf(trell);
            return index;
        }
        public DataTrellis Find(DataTrellis data)
        {
            int id = IndexOf(data);
            if (id > -1)
                return this[id];
            return null;    // Not found
        }
        public void CopyTo(Array array, int arrayIndex)
        {
            ((IList)this).CopyTo(array, arrayIndex);
        }
        public bool IsReadOnly
        {
            get { return false; }
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
        #endregion

        #region IBindingList

        public object BindingSource
        { get { return this; } }
        public IDataGridBinder BoundedGrid
        { get { return SphereOn.BoundedGrid; } set { SphereOn.BoundedGrid = value; } }
        public IDepotSync DepotSync
        { get { return BoundedGrid.DepotSync; } set { BoundedGrid.DepotSync = value; } }
        public IDataGridStyle GridStyle
        { get; set; }

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

        protected virtual void  OnListChanged(ListChangedEventArgs ev)
        {
            if (onListChanged != null)
            {
                onListChanged(this, ev);
            }
        }        

        protected override void InsertItem(int index, DataTrellis value)
        {
            if (this.SphereOn != null && !State.Expeled)
                value.Sphere = this.SphereOn;
            base.InsertItem(index, value);
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        }

        protected override void SetItem(int index, DataTrellis newValue)
        {
            DataTrellis oldValue = Items[index];
            if (!ReferenceEquals(oldValue, newValue))
            {
                if (this.SphereOn != null && !State.Expeled)
                    newValue.Sphere = this.SphereOn;
                base.SetItem(index, newValue);
                oldValue = null;

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
            DataTrellis c = new DataTrellis();
            base.Add(c);
            c.Sphere = SphereOn;
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
        int  IBindingList.Find(PropertyDescriptor property, object key)
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

        #endregion

        #region ITypedList 
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
                return CreateDescriptors(SphereOn.SphereId+ " Trells", Config.DataIdx);
            }
            set
            {
                CreateDescriptors(SphereOn.SphereId + " Trells", Config.DataIdx, value);
            }
        }

        private PropertyDescriptorCollection createDescriptors()
        {
            PropertyDescriptorCollection origProperties = TypeDescriptor.GetProperties(typeof(DataTrellis));

            ArrayList properties = new ArrayList();
            HashSet<string> handledNames = new HashSet<string>();

            foreach (PropertyDescriptor desc in origProperties)
                if (typeof(ICollection).IsAssignableFrom(desc.PropertyType))
                    if (desc.Name == "Tiers")
                    {
                        if (handledNames.Add("Tiers"))                        
                            properties.Add(new TiersDescriptor("Data Tiers", "Data Tiers"));                       
                    }                 
                    else if (desc.Name == "Sims")
                    {
                        if (handledNames.Add("Sims"))
                            properties.Add(new TiersDescriptor("Data Simulate", "Data Simulate"));
                    }
                    else if (desc.Name == "TiersTotals")
                    {
                        if (handledNames.Add("TiersTotals"))
                            properties.Add(new TiersDescriptor("Tiers Totals", "Tiers Totals"));
                    }
                    else if (desc.Name == "SimsTotals")
                    {
                        if (handledNames.Add("SimsTotals"))
                            properties.Add(new TiersDescriptor("Simulate Totals", "Simulate Totals"));
                    }
                    else if (desc.Name == "Filter")
                    {
                        if (handledNames.Add("Filter"))
                            properties.Add(new FilterDescriptor("Filtering", "Filtering"));
                    }
                    else if (desc.Name == "Sort")
                    {
                        if (handledNames.Add("Sort"))
                            properties.Add(new SortDescriptor("Sorting", "Sorting"));
                    }
                    else if (desc.Name == "Pylons")
                    {
                        if (handledNames.Add("Pylons"))
                            properties.Add(new DataPylonsDescriptor("Field Pylons", "Field Pylons"));
                    }
                    else if (desc.Name == "Relays")
                    {
                        if (handledNames.Add("Relays"))
                            properties.Add(new DataTrellRelaysDescriptor("Relations", "Relations"));
                    }
                    else if (desc.Name == "Favorites")
                    {
                        if (handledNames.Add("Favorites"))
                            properties.Add(new FavoritesDescriptor("Favorites", "Favorites"));
                    }


            List<string> propnames = new List<string>() { "TrellName", "DisplayName", "CountView", "Count", "Synced", "Edited", "Added",
                                                          "Saved", "Checked", "DataPlace", "DataIdx", "Quered", "IsCube", "IsPrime", "IsDevisor", "Visible", "Mode" };

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
            PropertyNaming.AddOrUpdate(name, dataId, (k, v) => v = dataId);

            if (_descriptors != null)
            {
                if (!DescriptorRegistry.ContainsKey(dataId))
                    return DescriptorRegistry.GetOrAdd(dataId, _descriptors);
                else
                    return DescriptorRegistry[dataId];
            }
            else if (!DescriptorRegistry.ContainsKey(SphereOn.Config.DataIdx))
                return DescriptorRegistry.GetOrAdd(SphereOn.Config.DataIdx, createDescriptors());
            else
                return DescriptorRegistry[SphereOn.Config.DataIdx];
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
                listName = SphereOn.SphereId + "_Trells";
            return listName;
        }
        #endregion
    }  
}