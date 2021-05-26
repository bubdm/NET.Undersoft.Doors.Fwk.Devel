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
    public static class DataSpace
    {
        private static int HASHES_CAPACITY_STANDARD_VALUE = 201;
        private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;

        public static bool DriveClient = false;

        private static string spaceName = "Space";
        public static string SpaceName
        {
            get
            {
                return spaceName;
            }
            set
            {
                spaceName = value;
                Area.SpaceName = value;
                Area.Config.SetMapFile();
            }
        }     

        private static string drivePath = "QDFS";
        public static string DrivePath
        {
            get { return drivePath; }
            set { drivePath = value; Area.Config.SetMapFile(); }
        }

        public static ConcurrentDictionary<int, object> Registry
        { get; set; } = new ConcurrentDictionary<int, object>(
                        MAX_ACCESS_REQUEST_VALUE, HASHES_CAPACITY_STANDARD_VALUE, new HashComparer());

        public static DataArea Area
        { get; set; } = new DataArea(SpaceName);

        public static object PathFinder(string path)
        {
            string[] items = path.Split('/');
            int length = items.Length;
            int last = length - 1;
            bool isTrellis = (items[last].Split('.').Length > 1) ? true : false;
            DataSpheres sets = Area[items[0]];
            DataSphere set = null;
            DataTrellis trell = null;
            if (length > 1 && sets != null)
            {
                if (isTrellis)
                { 
                    if ((last) == 2)
                        trell = sets[items[1], items[2].Split('.')[0]];
                    else
                    {
                        set = sets[items[1]];
                        if (set != null && length > 3)
                            for (int i = 3; i < length; i++)
                            {
                                DataSphere tempset = null;
                                if ((last - 1) == i)
                                    trell = set[items[i], items[last].Split('.')[0]];
                                else
                                {
                                    tempset = set[items[i]];
                                    set = tempset;
                                }
                            }
                    }
                    return trell;
                }
                else
                {
                    set = sets[items[1]];
                    if (set != null && length > 2)
                    {
                        for (int i = 2; i < length; i++)
                        {
                            DataSphere tempset = set[items[i]];
                            set = tempset;
                        }
                    }
                    return set;
                }
            }
            else
            {
                return sets;
            }
        }

        public static void PrimeFinder()
        {
            foreach (object obj in Registry.Values)
                if (obj.GetType().GetInterfaces().Contains(typeof(IDataSerial)))
                    if (((IDataConfig)obj).State.Added)
                    {
                        if (obj.GetType() == typeof(DataTrellis))
                        {
                            DataTrellis trell = (DataTrellis)obj;
                            if (!trell.IsPrime && trell.Prime != null)
                            {
                                DataTrellis _prime = ((DataTrellis)obj).Prime.Locate();
                                if (_prime != null)
                                    trell.Prime = _prime;
                                if (trell.Devisor != null)
                                {
                                    DataTrellis _dev = trell.Devisor.Locate();
                                    if (_dev != null)
                                        trell.Devisor = _dev;
                                }
                            }
                        }
                        ((IDataConfig)obj).State.Added = false;
                    }
        }
        public static void PrimeFinder(DataSpheres spheres)
        {
            foreach (DataSphere sphere in spheres.Values)
                if (sphere.State.Added)
                {
                    PrimeFinder(sphere);
                    sphere.State.Added = false;
                }

            if (spheres.SpheresIn != null)
                PrimeFinder(spheres.SpheresIn);
        }
        public static void PrimeFinder(DataSphere spher)
        {
            PrimeFinder(spher.Trells.AsEnumerable().ToArray());

            if (spher.SpheresIn != null)
                if (spher.SpheresIn.State.Added)
                {
                    PrimeFinder(spher.SpheresIn);
                    spher.SpheresIn.State.Added = false;
                }
            if (spher.SphereIn != null)
                if (spher.SphereIn.State.Added)
                {
                    PrimeFinder(spher.SphereIn);
                    spher.SphereIn.State.Added = false;
                }
            spher.State.Added = false;
        }
        public static void PrimeFinder(ICollection<DataTrellis> trellArray)
        {
            foreach (DataTrellis trell in trellArray)
                if (trell.State.Added)
                {
                    if (!trell.IsPrime && trell.Prime != null)
                    {
                        DataTrellis _prime = trell.Prime.Locate();
                        if (_prime != null)
                            trell.Prime = _prime;
                        if (trell.Devisor != null)
                        {
                            DataTrellis _dev = trell.Devisor.Locate();
                            if (_dev != null)
                                trell.Devisor = _dev;
                        }
                    }
                    trell.State.Added = false;
                }
        }
    }

    [JsonObject]
    [Serializable]
    public class DataArea : IDictionary<string, DataSpheres>, IEnumerable<KeyValuePair<string, DataSpheres>>, IDriveRecorder, IDataSpace,
                            IDataSerial, IDataMorph, IListSource, ITypedList, IDataGridBinder, IDataTreeBinder, IBindingList, IList, INoid, IDataConfig
    {
        [NonSerialized] private static int HASHES_CAPACITY_STANDARD_VALUE = 51;                                      
        [NonSerialized] private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;       
        [NonSerialized] private IDrive drive;
        [NonSerialized] private ConcurrentDictionary<int, object> registry;

        public DepotIdentity ServiceIdentity
        {
            get { return DataBank.ServiceIdentity; }
            set { DataBank.ServiceIdentity = value; }
        }
        public DepotIdentity ServerIdentity
        {
            get { return DataBank.ServerIdentity;  }
            set { DataBank.ServerIdentity = value; }
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

        public string SpaceName
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

        public string DrivePath
        {
            get { return DataSpace.DrivePath; }
            set { DataSpace.DrivePath = value; }
        }

        public string DataPlace
        { get { return Config.Place; } }
        public string DataIdx
        { get { return Config.DataIdx; } }

        public DataArea()
        {
            SpaceName = DataSpace.SpaceName;
            DisplayName = DataSpace.SpaceName;
            DataCore.Space = this;
            Config = new DataConfig(this, DataStore.Space);
            Config.SetMapFile();
            State = new DataState();
            SerialCount = 0;
            DeserialCount = 0;
            Parameters = new DataParams();
        }
        public DataArea(string spaceName)
        {
            SpaceName = spaceName;
            DisplayName = spaceName;
            registry = DataSpace.Registry;
            DataCore.Space = this;
            Config = new DataConfig(this, DataStore.Space);
            Config.SetMapFile();
            State = new DataState();
            SerialCount = 0;
            DeserialCount = 0;
            Parameters = new DataParams();
        }

        public SortedDictionary<string, DataSpheres> Areas
        { get; set; } = new SortedDictionary<string, DataSpheres>();

        public DataSpheres this[string key]
        {
            get
            {
                DataSpheres result = null;
                Areas.TryGetValue(key, out result);
                return result;
            }
            set
            {
                Areas.AddOrUpdate(key, value, (k, v) => v = value);
                value.Config.SetMapConfig(this);
            }
        }
        public object this[int index]
        {
            get
            {
                string key = null;
                DataSpheres result = null;
                if (index <= Keys.Count - 1)
                {
                    key = Keys.ElementAt(index);
                    if (key != null)
                        if (Areas.TryGetValue(key, out result))
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
                        if (Areas.ContainsKey(key))
                        {
                            Areas[key] = (DataSpheres)value;
                            ((DataSpheres)value).Config.SetMapConfig(this);
                        }
                }
            }
        }
        public DataTrellis this[string spheres, string sphere, string trellname]
        {
            get
            {
                DataSpheres nSpheres = null;
                DataSphere nSphere = null;
                if (Areas.TryGetValue(sphere, out nSpheres))
                    if (nSpheres.TryGetValue(spheres, out nSphere))
                        if (nSphere.Trells.Have(trellname))
                            return nSphere.Trells[trellname];
                return null;
            }
            set
            {

                if (Areas.ContainsKey(spheres))
                    if (Areas[spheres].ContainsKey(sphere))
                    {
                        if (Areas[spheres][sphere].Trells.Have(trellname))
                            Areas[spheres][sphere].Trells[trellname] = value;
                    }
                    else if (Areas[spheres].SpheresIn != null)
                        if (Areas[spheres].SpheresIn.ContainsKey(sphere))
                            if (Areas[spheres].SpheresIn[sphere].Trells.Have(trellname))
                                Areas[spheres].SpheresIn[sphere].Trells[trellname] = value;
            }
        }

        public void SetPrimeRef()
        {
            foreach (object _trell in DataSpace.Registry.Values)
                if (_trell.GetType() == typeof(DataTrellis))
                    if (!((DataTrellis)_trell).IsPrime && ((DataTrellis)_trell).Prime != null)
                    {
                        ((DataTrellis)_trell).Prime = ((DataTrellis)_trell).Prime.Locate();
                        if (((DataTrellis)_trell).Devisor != null)
                            ((DataTrellis)_trell).Devisor = ((DataTrellis)_trell).Devisor.Locate();
                    }
        }

        #region IDictionary

        public DataSpheres Get(string key)
        {
            DataSpheres result = null;
            Areas.TryGetValue(key, out result);
            return result;
        }
        public void Add(string key, DataSpheres value)
        {           
            this.Add(new KeyValuePair<string, DataSpheres>(key, value));
        }
        public void Add(KeyValuePair<string, DataSpheres> item)
        {
            if (Areas.TryAdd(item.Key, item.Value))
            {
                Areas[item.Key].Config.SetMapConfig(this);             
            }
        }
        public bool TryAdd(string key, DataSpheres value)
        {
            value.AreaOn = this;
            if (Areas.TryAdd(key, value))
            {
                value.Config.SetMapConfig(this);               
                return true;
            }
            else
                return false;
        }
        public void Put(string key, DataSpheres value)
        {
            Areas.AddOrUpdate(key, value, (k, v) => v = value);
            Areas[key].Config.SetMapConfig(this);           
        }
        public DataSpheres Create(string iSetsId = null)
        {
            string DataSpheresId = (iSetsId != null) ? iSetsId : SpaceName + "_" + (Areas.Count + 1).ToString();
            DataSpheres value = new DataSpheres(DataSpheresId);
            value.Config.DepotId = Config.DepotId;
            value.Config.Path = Config.Path + "/" + DataSpheresId;
            value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
            Areas.AddOrUpdate(DataSpheresId, value, (k, v) => v = value);
            value.Config.SetMapConfig(this);
            return value;
        }
        public bool ContainsKey(string key)
        {
            return Areas.ContainsKey(key);
        }
        public bool Remove(string key)
        {
            DataSpheres outset = null;
            bool done = Areas.TryRemove(key, out outset);
            if (done)
            {
                object outreg = new object();
                int hash = outset.Config.GetDataId();
                DataSpace.Registry.TryRemove(hash, out outreg);
            }
            return done;
        }
        public bool TryGetValue(string key, out DataSpheres value)
        {
            return Areas.TryGetValue(key, out value);
        }
        public void Clear()
        {
            Areas.Clear();
        }

        public bool Contains(KeyValuePair<string, DataSpheres> item)
        {
            return Areas.Contains(item);
        }
        public void CopyTo(KeyValuePair<string, DataSpheres>[] array, int arrayIndex)
        {
            Areas.ToArray().CopyTo(array, arrayIndex);
        }
        public bool Remove(KeyValuePair<string, DataSpheres> item)
        {
            DataSpheres outset = null;
            bool done = Areas.TryRemove(item.Key, out outset);
            if (done)
            {
                object outreg = new object();
                DataSpace.Registry.TryRemove(outset.Config.GetDataId(), out outreg);
            }
            return done;
        }

        public IEnumerator<KeyValuePair<string, DataSpheres>> GetEnumerator()
        {
            return Areas.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Areas.GetEnumerator();
        }

        public KeyValuePair<string, DataSpheres> SetPair
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
                return Areas.Keys;
            }
        }
        public ICollection<DataSpheres> Values
        {
            get
            {
                return Areas.Values;
            }
        }
        public int Count
        {
            get
            {
                return Areas.Count;
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
            DataSpheres set = (DataSpheres)_set;
            if (!this.Have(set.SpheresId))
            {
                if (this.TryAdd(set.SpheresId, set))
                    index = Count - 1;
            }
            return index;
        }
        public void AddRange(List<DataSpheres> _sets)
        {
            try
            {
                List<DataSpheres> newSpheres = _sets.Where(c => !this.Have(c.SpheresId)).ToList();
                foreach (DataSpheres newSphere in newSpheres)
                    this.TryAdd(newSphere.SpheresId, newSphere);
            }
            catch (Exception ex)
            { }
        }
        public void Remove(object value)
        {
            string key = null;
            DataSpheres tempset = (DataSpheres)value;
            Areas.Select((x, y) => (ReferenceEquals(x.Value, value) ||
                           x.Value.SpheresId.Equals(((DataSpheres)value).SpheresId)) ?
                           key = x.Key : null).ToArray();
            if (key != null)
                Areas.TryRemove(key, out tempset);
        }
        public void RemoveAt(int index)
        {
            int count = Keys.Count;
            string key = null;
            DataSpheres set = null;
            if (index >= count - 1)
            {
                key = Keys.ElementAt(index);
                Areas.TryRemove(key, out set);
            }
        }
        public bool Have(string setId)
        {
            return this.ContainsKey(setId);
        }
        public DataSpheres AddNew()
        {
            DataSpheres set = (DataSpheres)((IBindingList)this).AddNew();
            return set;
        }
        public void SphereRange(DataSpheres[] data)
        {
            Areas.AddOrUpdateRange(new ConcurrentDictionary<string, DataSpheres>(data.Select(d => new KeyValuePair<string, DataSpheres>(d.SpheresId, d)).ToArray()));
        }
        public int IndexOf(object value)
        {
            int index = -1;
            Values.Select((x, y) => (ReferenceEquals(x, value) || x.SpheresId.Equals(((DataSphere)value).SphereId)) ? index = y : 0).ToArray();
            return index;
        }
        public void Insert(int index, object data)
        {
            string key = null;
            if (index >= Keys.Count - 1)
            {
                key = Keys.ElementAt(index);
                if (key != null)
                    if (Areas.ContainsKey(key))
                    {
                        DataSpheres sprs = (DataSpheres)data;
                        sprs.Config.SetMapConfig(this);
                        Areas[key] = sprs;
                    }
            }
        }
        public DataSpheres Find(DataSpheres data)
        {
            if (Areas.Values.Contains(data))
                return data;
            else if (Areas.ContainsKey(data.SpheresId))
                return Areas[data.SpheresId];
            else
                return null;    // Not found
        }
        public bool Contains(object data)
        {
            return (Find((DataSpheres)data) != null);
        }
        public void CopyTo(DataSpheres[] array, int arrayIndex)
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

        public object BindingSource { get { return this; } }
        public IDepotSync DepotSync { get { return (BoundedGrid != null) ? BoundedGrid.DepotSync : null; } set { if (BoundedGrid != null) BoundedGrid.DepotSync = value; } }
        [NonSerialized] private IDataGridStyle gridStyle;
        public IDataGridStyle GridStyle { get { return gridStyle; } set { gridStyle = value; } }
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
            foreach (DataSpheres c in Values)
            {
                c.Spheres = null;
            }
        }
        protected void OnClearComplete()
        {
            OnListChanged(resetEvent);
        }
        protected void OnInsertComplete(int index, object value)
        {
            DataSpheres c = (DataSpheres)value;
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        }
        protected void OnRemoveComplete(int index, object value)
        {
            DataSphere c = (DataSphere)value;
            c.Trells = null;

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
            DataSpheres c = null;
            int number = 0;
            string numstr = "";
            bool found = false;
            while (!found)
            {
                if (ContainsKey("Sphere" + number))
                {
                    number++;
                    numstr = number.ToString();
                    if (number > 100)
                        found = true;
                }
                else
                {
                    c = new DataSpheres("Sphere" + numstr);
                    Areas.TryAdd(c.SpheresId, c);
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
                return CreateDescriptors(SpaceName, Config.DataIdx);
            }
            set
            {
                CreateDescriptors(SpaceName, Config.DataIdx, value);
            }
        }

        private PropertyDescriptorCollection createDescriptors()
        {
            PropertyDescriptorCollection origProperties = TypeDescriptor.GetProperties(typeof(DataSpheres));

            ArrayList properties = new ArrayList();
            HashSet<string> handledNames = new HashSet<string>();

            foreach (PropertyDescriptor desc in origProperties)
                if (typeof(ICollection).IsAssignableFrom(desc.PropertyType))
                {                   
                    if (desc.Name == "SpheresIn")
                    {
                        if (handledNames.Add("SpheresIn"))
                            properties.Add(new DataAreaSphereDescriptor("Sphere's Inside", "Sphere's Inside"));
                    }
                }

            List<string> propnames = new List<string>() { "SpheresId", "DisplayName", "CountView", "Count", "Synced", "Edited",
                                                          "Saved", "Checked", "DataPlace", "DataIdx" };
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
                {
                    DescriptorRegistry.TryAdd(dataId, _descriptors);
                    return _descriptors;
                }
                else
                    return DescriptorRegistry[dataId];
            }
            else if (!DescriptorRegistry.ContainsKey(Config.DataIdx))
            {
                PropertyDescriptorCollection descriptors = createDescriptors();
                DescriptorRegistry.TryAdd(Config.DataIdx, descriptors);
                return descriptors;
            }
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
                listName = SpaceName;
            return listName;
        }

        #endregion

        #region Serialization
        public int Serialize(Stream tostream, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        {
            if (serialFormat == SerialFormat.Raw)
                return this.SetRaw(tostream);
            else if (serialFormat == SerialFormat.Json)
                return this.SetJson(tostream);
            else
                return -1;
        }
        public int Serialize(ISerialContext buffor, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        {
            if (serialFormat == SerialFormat.Raw)
                return this.SetRaw(buffor);
            else if (serialFormat == SerialFormat.Json)
                return this.SetJson(buffor);
            else
                return -1;
        }

        public object Deserialize(Stream fromstream, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        {
            if (serialFormat == SerialFormat.Raw)
                return this.GetRaw(fromstream);
            else if (serialFormat == SerialFormat.Json)
                return this.GetJson(fromstream);
            else
                return -1;
        }
        public object Deserialize(ref object fromarray, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        {
            if (serialFormat == SerialFormat.Raw)
                return this.GetRaw(ref fromarray);
            else if (serialFormat == SerialFormat.Json)
                return this.GetJson(ref fromarray);
            else
                return -1;
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

        public int    GetKeyHash()
        {
            return (Config.Place != null)? Config.Place.GetHashCode(): 0;
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
            return Config.Path ?? SpaceName;
        }
        public string GetMapName()
        {
            return Config.Path +"/"+ SpaceName + ".nds";
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

        public int SerialCount
        { get; set; }
        public int DeserialCount
        { get; set; }
        public int ProgressCount
        { get; set; }
        public int ItemsCount
        { get { return Count; } }
        #endregion

        #region INMutator

        public object Emulator(object source, string name = null)
        {
            return this.Emulate((DataArea)source, name);
        }
        public object Imitator(object source, string name = null)
        {
            return this.Imitate((DataArea)source, name);
        }
        public object Impactor(object source, string name = null)
        {
            if(source == null)
                throw new ArgumentException("DataSpace Source Is Empty");
            try
            {
                bool save = ((IDataConfig)source).State.Saved;
                DataArea area = this.Impact((DataArea)source, save, name);
                DataSpace.PrimeFinder();
                return area;
            }
            catch(Exception ex)
            {
                throw new ArgumentException("DataSpace Source Error");
            }
           
        }
        public object Locator(string path = null)
        {
            return this.Locate(path);
        }

        #endregion

        #region IDriveRecorder
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

            foreach (object obj in DataSpace.Registry.Values)
            {
                Type[] ifaces = obj.GetType().GetInterfaces();
                if (ifaces.Contains(typeof(IDriveRecorder)) && ifaces.Contains(typeof(IDataConfig)))
                    if (!ReferenceEquals(obj, this) && (obj.GetType() == typeof(DataTiers)))
                    {
                        DataTiers trs = null;
                        //if (obj is DataTrellis)
                        //    trs = ((DataTrellis)obj).Tiers;
                        //else
                            trs = (DataTiers)obj;

                        if (!((IDataConfig)trs).State.Saved)
                        {
                            IDriveRecorder recobj = (IDriveRecorder)trs;
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
            DataSpace.Area.Impactor((DataArea)Deserialize(ref read));

            DataVault ds = DataBank.Vault["Data"];

            foreach (object obj in DataSpace.Registry.Values)
            {                
                Type[] ifaces = obj.GetType().GetInterfaces();
                if (ifaces.Contains(typeof(IDriveRecorder)) && ifaces.Contains(typeof(IDataConfig)))
                    if (!ReferenceEquals(obj, this) && obj.GetType() == typeof(DataTrellis))
                    {
                        DataTrellis trs = (DataTrellis)obj;
                        if (trs.IsCube && trs.IsPrime)
                        {
                            trs.Cube.CreateCube();
                            trs.Cube.CubeTiers.Registry.ReadDrive();
                        }
                    }
            }
            dc.Dispose();

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
        public void OpenDrive()
        {
            if (Drive == null)
                if (!DataSpace.DriveClient)
                    Drive = new DriveBank(Config.File, Config.File, 20 * 1024 * 1024, typeof(DataArea));
                else
                    Drive = new DriveBank(Config.File, Config.File, typeof(DataArea));
        }
        public void CloseDrive()
        {
            if (Drive != null)
                Drive.Close();
            Drive = null;
        }
        #endregion

        #region IDataTreeBinder
     
        public string TreeNodeName
        {
            get { return SpaceName; }
        }
        public object TreeNodeTag
        {
            get { return this; }
        }
        public IDataTreeBinder[] TreeNodeChilds
        {
            get { return Areas.Select(a => (IDataTreeBinder)a.Value).ToArray(); }
        }
        public IDataTreeBinder BoundedTree
        { get; set; }

        #endregion

    }

    public class DataAreaSphereDescriptor : PropertyDescriptor
    {
        public DataAreaSphereDescriptor(string name, string display) : base(name, new Attribute[] { new DataIdxAttribute((display != null) ? display : name) })
        {

        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataSpheres); } }
        public override bool IsReadOnly { get { return true; } }
        public override Type PropertyType { get { return typeof(DataSpheres); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
        }
        public override object GetValue(object component)
        {
            DataSpheres child = ((DataSpheres)component);
            if (child != null)
            {
                PropertyDescriptorCollection descript = child.AreaOn.CreateDescriptors(Name, child.Config.DataIdx, 
                                                               child.CreateDescriptors(Name, child.Config.DataIdx));

              if (((DataSpheres)component).AreaOn.BoundedGrid != null)
                  ((IDataGridBinder)child).BoundedGrid = ((DataSpheres)component).AreaOn.BoundedGrid;
            }

            return child;
        }      
    }
}   
