using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Collections.Concurrent;
using System.Collections;
using System.ComponentModel;
using System.Dors;
using System.Dors.Drive;

namespace System.Dors.Data
{
    [JsonObject]
    [Serializable]
    public class DataSpheres : IDictionary<string, DataSphere>, IEnumerable<KeyValuePair<string, DataSphere>>, IDriveRecorder, IDataDescriptor,
                               IDataSerial, IDataMorph, IListSource, ITypedList, IDataGridBinder, IDataTreeBinder, IBindingList, IList, INoid, IDataConfig
    {
        #region Private / NonSerialized
        private static int HASHES_CAPACITY_STANDARD_VALUE = 51;                           // probably there is no need for more then 50 values in Spheres, +1 cause 0based count
        private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;     // established max count for access requests to Spheres
        private DataSpheres spheresIn;
        [NonSerialized] private IDrive drive;
        [NonSerialized] private IDataTreeBinder boundedTree;
        [NonSerialized] private List<DataSpheres> spheresInAll;
        [NonSerialized] private List<DataSpheres> spheresUpAll;
        [NonSerialized] private DataArea areaOn;
        [NonSerialized] private DataSpheres spheresUp;
        [NonSerialized] private DataSphere sphereUp;
        #endregion

        public DataSpheres()
        {
            SpheresId = "Spheres#"+DateTime.Now.ToFileTime().ToString();
            DisplayName = SpheresId;
            Config = new DataConfig(this, DataStore.Space);
            State = new DataState();
            State.Expeled = true;
            Parameters = new DataParams();
            AreaOn = DataSpace.Area;
            spheresInAll = new List<DataSpheres>();
            spheresUpAll = new List<DataSpheres>();
            Spheres = new SortedDictionary<string, DataSphere>();
        }
        public DataSpheres(string spheresId)
        {
            SpheresId = (spheresId != null) ? spheresId : "Spheres#" + DateTime.Now.ToFileTime().ToString();
            DisplayName = SpheresId;
            Config = new DataConfig(this, DataStore.Space);
            State = new DataState();
            Parameters = new DataParams();
            AreaOn = DataSpace.Area;
            spheresInAll = new List<DataSpheres>();
            spheresUpAll = new List<DataSpheres>();
            Spheres = new SortedDictionary<string, DataSphere>();
        }

        public string SpheresId
        { get; set; }
        public string DisplayName
        { get; set; }

        public DataSpheres Devisor
        {
            get; set;        
        }     

        public List<DataSpheres> SpheresInAll
        {
            get
            {
                return spheresInAll;
            }
            set
            {
                spheresInAll = value;
            }
        }
        public List<DataSpheres> SpheresUpAll
        {
            get
            {
                return spheresUpAll;
            }
            set
            {
                spheresUpAll = value;
            }
        }

        public  DataArea AreaOn
        {
            get
            {
                return areaOn;
            }
            set
            {
                areaOn = value;
            }
        }

        public  DataSpheres SpheresIn
        {
            get
            {
                return spheresIn;
            }
            set
            {
                if (value != null)
                {
                    spheresIn = value;
                    value.SpheresUp = this;
                    value.Config.SetMapConfig(this);
                    SpheresInAll.Add(value);
                    if(SpheresUp != null)
                        SpheresUp.SpheresInAll.Add(value);
                    value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
                }
            }
        }
        public  DataSpheres SpheresUp
        {
            get
            {
                return spheresUp;
            }
            set
            {
                spheresUp = value;
                SpheresUpAll.Add(value);
            }
        }        
        public  DataSphere  SphereUp
        {
            get
            {
                return sphereUp;
            }
            set
            {
                sphereUp = value;
            }
        }     
       
        public SortedDictionary<string, DataSphere> Spheres
        { get; set; } = new SortedDictionary<string, DataSphere>();

        public DataSpheres NewSpheres(string iSpheresId = null)
        {
            string _SpheresId = (iSpheresId != null) ? iSpheresId : SpheresId + "_" + 1.ToString();
            DataSpheres value = new DataSpheres(_SpheresId);
            SpheresIn = value;
            return value;
        }
        public DataSphere  NewSphere(string sphereId = null)
        {
            string SphereId = (sphereId != null) ? sphereId : SpheresId + "_" + (Spheres.Count + 1).ToString();
            DataSphere value = new DataSphere(SphereId);
            this.Put(SphereId, value);
         //   value.WriteDrive();
            return value;
        }     

        public object      this[int index]
        {
            get
            {
                string key = null;
                DataSphere result = null;
                if (index <= Keys.Count - 1)
                {
                    key = Keys.ElementAt(index);
                    if (key != null)
                        if (Spheres.TryGetValue(key, out result))
                            if (result.Visible)
                                return result;
                            else
                                result = null;
                        else if (SpheresIn != null)
                            if (SpheresIn[key].Visible)
                                result = SpheresIn[key];
                }
                return result;
            }
            set
            {
                if (SphereUp != null)
                {
                    ((DataSphere)value).SphereUp = SphereUp;
                    DataParams p = new DataParams();
                    p.Registry = new Dictionary<string, object>(SphereUp.Parameters.Registry);
                    ((DataSphere)value).Parameters = p;
                }
                if (SpheresUp != null)
                    ((DataSphere)value).SpheresUp = SpheresUp;
                ((DataSphere)value).SpheresOn = this;

                string key = null;
                if (index <= Keys.Count - 1)
                {
                    key = Keys.ElementAt(index);
                    if (key != null)
                        if (Spheres.ContainsKey(key))
                        {
                            Spheres[key] = (DataSphere)value;
                            ((DataSphere)value).Config.SetMapConfig(this);
                            ((DataSphere)value).Trells.Config.SetMapConfig(((DataSphere)value));
                        }
                        else if (SpheresIn != null)
                        {
                            SpheresIn[key] = (DataSphere)value;
                            ((DataSphere)value).Config.SetMapConfig(SpheresIn);
                            ((DataSphere)value).Trells.Config.SetMapConfig(((DataSphere)value));
                        }
                }
            }
        }
        public DataSphere  this[string key]
        {
            get
            {
                DataSphere result = null;
                if (Spheres.TryGetValue(key, out result))
                    if(result.Visible)
                       return result;
                if (SpheresIn != null)
                    if (SpheresIn[key].Visible)
                        result = SpheresIn[key];
                return result;
            }
            set
            {
                if (SphereUp != null)
                {
                    value.SphereUp = SphereUp;
                    DataParams p = new DataParams();
                    p.Registry = new Dictionary<string, object>(SphereUp.Parameters.Registry);
                    value.Parameters = p;
                }
                if (SpheresUp != null)
                    value.SpheresUp = SpheresUp;
                value.SpheresOn = this;

                if (Spheres.ContainsKey(key))
                {
                    Spheres[key] = value;
                    ((DataSphere)value).Config.SetMapConfig(this);
                    ((DataSphere)value).Trells.Config.SetMapConfig(((DataSphere)value));
                }
                else if (SpheresIn != null)
                {
                    SpheresIn[key] = value;
                    ((DataSphere)value).Config.SetMapConfig(SpheresIn);
                    ((DataSphere)value).Trells.Config.SetMapConfig(((DataSphere)value));
                }
            }
        }
        public DataTrellis this[string key, string trellname]
        {
            get
            {
                DataSphere result = null;
                if (Spheres.TryGetValue(key, out result))
                    if (result.Trells.Have(trellname))
                        return result.Trells[trellname];
                    else if (SpheresIn != null)
                    {
                        result = SpheresIn[key];
                        if(result != null)
                            if (result.Trells.Have(trellname))
                                return result.Trells[trellname];
                    }
                return null;
            }
            set
            {
                if (SphereUp != null)
                {
                    DataParams p = new DataParams();
                    p.Registry = new Dictionary<string, object>(SphereUp.Parameters.Registry);
                    value.Parameters = p;
                }

                if (Spheres.ContainsKey(key))
                    Spheres[key].Trells[trellname] = value;
                else if (SpheresIn != null)
                    if(SpheresIn.ContainsKey(key))
                        if(SpheresIn[key].Trells.Have(trellname))
                            SpheresIn[key].Trells[trellname] = value;
            }
        }

        public DataConfig Config
        { get; set; }
        public DataParams Parameters
        { get; set; }
        public DataState  State
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
        public bool IsDivision
        { get; set; }

        public string DataPlace
        { get { return Config.Place; } }
        public string DataIdx
        { get { return Config.DataIdx; } }

        #region IEnumerable
        public IEnumerator<KeyValuePair<string, DataSphere>> GetEnumerator()
        {
            return Spheres.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Spheres.GetEnumerator();
        }
        #endregion

        #region IDictionary
        public DataSphere Get(string key)
        {
            DataSphere result = null;
            Spheres.TryGetValue(key, out result);
            return result;
        }
        public void Add(string key, DataSphere value)
        {
            if (SphereUp != null)
                value.SphereUp = SphereUp;
            value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
            if (SpheresUp != null)
                value.SpheresUp = SpheresUp;
            value.SpheresOn = this;
            this.Add(new KeyValuePair<string, DataSphere>(key, value));
        }
        public void Add(KeyValuePair<string, DataSphere> item)
        {
            if (this.TryAdd(item.Key, item.Value))
            {
                //item.Value.Config.SetMapConfig(this);               
                //item.Value.Trells.Config.SetMapConfig(item.Value);            
            }
        }
        public bool TryAdd(string key, DataSphere value)
        {
            if (SphereUp != null)
                value.SphereUp = SphereUp;
            value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
            if (SpheresUp != null)
                value.SpheresUp = SpheresUp;
            value.SpheresOn = this;
            if (Spheres.TryAdd(key, value))
            {
                value.Config.SetMapConfig(this);             
                value.Trells.Config.SetMapConfig(value);           
                return true;
            }
            else
                return false;
        }
        public void Put(string key, DataSphere value)
        {
            if (SphereUp != null)
                value.SphereUp = SphereUp;
            value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
            if (SpheresUp != null)
                value.SpheresUp = SpheresUp;
            value.SpheresOn = this;
            Spheres.AddOrUpdate(key, value, (k, v) => v = value);
            DataSphere _sphr = Spheres[key];
            _sphr.Config.SetMapConfig(this);           
            _sphr.Trells.Config.SetMapConfig(Spheres[key]);          
        }
        public bool ContainsKey(string key)
        {
            return Spheres.ContainsKey(key);
        }
        public bool Remove(string key)
        {
            DataSphere outset = null;
            bool done = Spheres.TryRemove(key, out outset);
            if (done)
            {
                object outreg = new object();
                DataSpace.Registry.TryRemove(outset.Config.GetDataId(), out outreg);
            }
            return done;
        }
        public bool TryGetValue(string key, out DataSphere value)
        {
            return Spheres.TryGetValue(key, out value);
        }
        public void Clear()
        {
            Spheres.Clear();
        }
        public bool Contains(KeyValuePair<string, DataSphere> item)
        {
            return Spheres.Contains(item);
        }
        public void CopyTo(KeyValuePair<string, DataSphere>[] array, int arrayIndex)
        {
            Spheres.ToArray().CopyTo(array, arrayIndex);
        }
        public bool Remove(KeyValuePair<string, DataSphere> item)
        {
            DataSphere outset = null;
            bool done = Spheres.TryRemove(item.Key, out outset);
            if (done)
            {
                object outreg = new object();
                DataSpace.Registry.TryRemove(outset.Config.GetDataId(), out outreg);
                foreach (DataTrellis trell in outset.Trells)
                {
                    DataSpace.Registry.TryRemove(trell.Config.GetDataId(), out outreg);
                    DataSpace.Registry.TryRemove(trell.Tiers.Config.GetDataId(), out outreg);
                }
                if (outset.SpheresIn != null)
                {
                    List<string> keys = outset.SpheresIn.Spheres.Keys.ToList();
                    foreach (string remset in keys)
                    {
                        outset.SpheresIn.Remove(remset);
                    }
                }
                DataSphere inset = null;
                if (outset.SphereIn != null)
                {
                    DataSpace.Registry.TryRemove(inset.Config.GetDataId(), out outreg);
                    foreach (DataTrellis trell in inset.Trells)
                    {
                        DataSpace.Registry.TryRemove(trell.Config.GetDataId(), out outreg);
                        DataSpace.Registry.TryRemove(trell.Tiers.Config.GetDataId(), out outreg);
                    }
                }

            }

            return done;
        }
        public ICollection<string> Keys
        {
            get
            {
                return Spheres.Keys;
            }
        }
        public ICollection<DataSphere> Values
        {
            get
            {
                return Spheres.Values;
            }
        }
        public int Count
        {
            get
            {
                return Spheres.Count;
            }
        }
        public int CountView
        {
            get
            {
                return Spheres.Where(s => s.Value.Visible).Count();
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
        public DataSphere AddNew()
        {
            DataSphere set = (DataSphere)((IBindingList)this).AddNew();
            return set;
        }
        public DataSphere Find(DataSphere data)
        {
            if (Spheres.Values.Contains(data))
                return data;
            else if (Spheres.ContainsKey(data.SphereId))
                return Spheres[data.SphereId];
            else
                return null;    // Not found
        }

        public int  Add(object _set)
        {
            int index = -1;
            DataSphere set = (DataSphere)_set;
            if (!this.Have(set.SphereId))
            {
                if(this.TryAdd(set.SphereId, set))
                    index = Count - 1;
            }
            return index;
        }
        public void AddRange(ICollection<DataSphere> _sets)
        {
            try
            {
                List<DataSphere> newSpheres = _sets.Where(c => !this.Have(c.SphereId)).ToList();
                foreach (DataSphere newSphere in newSpheres)
                {
                    newSphere.SpheresOn = this;
                    this.TryAdd(newSphere.SphereId, newSphere);
                }
            }
            catch (Exception ex)
            { }
        }
        public void Remove(object value)
        {
            string key = null;
            DataSphere tempset = (DataSphere)value;
            Spheres.Select((x, y) => (ReferenceEquals(x.Value, value) || x.Value.SphereId.Equals(((DataSphere)value).SphereId)) ? key = x.Key : null).ToArray();
            if (key != null)
                Spheres.TryRemove(key, out tempset);
        }
        public void RemoveAt(int index)
        {
            int count = Keys.Count;
            string key = null;
            DataSphere set = null;
            if (index >= count - 1)
            {
                key = Keys.ElementAt(index);
                Spheres.TryRemove(key, out set);
            }
        }
        public bool Have(string setId)
        {
            return this.ContainsKey(setId);
        }      
        public void SphereRange(DataSphere[] data)
        {
            Spheres.AddOrUpdateRange(new ConcurrentDictionary<string, DataSphere>(data.Select(d => new KeyValuePair<string, DataSphere>(d.SphereId, d)).ToArray()));
        }
        public int  IndexOf(object value)
        {
            int index = -1;
            Values.Select((x,y) => (ReferenceEquals(x, value) || x.SphereId.Equals(((DataSphere)value).SphereId)) ? index = y : 0).ToArray();
            return index;
        }
        public void Insert(int index, object data)
        {
            string key = null;
            if (index >= Keys.Count - 1)
            {
                key = Keys.ElementAt(index);
                if (key != null)
                    if (Spheres.ContainsKey(key))
                    {
                        DataSphere value = (DataSphere)data;
                        if (SphereUp != null)
                            value.SphereUp = SphereUp;
                        value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
                        if (SpheresUp != null)
                            value.SpheresUp = SpheresUp;
                        Spheres[key] = value;
                        value.Config.SetMapConfig(this);                  
                        value.Trells.Config.SetMapConfig(value);                    
                    }
            }
        }       
        public bool Contains(object data)
        {
            return (Find((DataSphere)data) != null);
        }
        public void CopyTo(DataSphere[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }
        public void CopyTo(Array array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
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

        public object SyncRoot
        {
            get
            {
                return null;
            }
        }
        #endregion

        #region IBindingList

        public object BindingSource { get { return this; } }
        [NonSerialized] private IDataGridStyle gridStyle;
        public IDataGridStyle GridStyle { get { return gridStyle; } set { gridStyle = value; } }
        [NonSerialized] private IDataGridBinder boundedGrid;
        public IDataGridBinder BoundedGrid { get { return boundedGrid; } set { boundedGrid = value; } }
        public IDepotSync DepotSync { get { return BoundedGrid.DepotSync; } set { BoundedGrid.DepotSync = value; } }
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
        }
        protected void OnClearComplete()
        {
            OnListChanged(resetEvent);
        }       
        protected void OnInsertComplete(int index, object value)
        {
            DataSphere c = (DataSphere)value;
            c.SpheresOn = this;
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        }
        protected void OnRemoveComplete(int index, object value)
        {
            DataSphere c = (DataSphere)value;
            c.Trells = null;

            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
        }
        protected void OnSetComplete(int index, object oldValue, object newValue)
        {
            //if (oldValue != newValue)
            //{
            //    object oldItem = oldValue;
            //    object newItem = newValue;

            //    oldItem = null;
            //    newItem = this;
            //}
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        }
        protected void OnValidate(object value)
        {

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
            DataSphere c = new DataSphere("Sphere#" + DateTime.Now.ToFileTime().ToString());
            Spheres.TryAdd(c.SphereId, c);           
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
                return CreateDescriptors(SpheresId, Config.DataIdx);
            }
            set
            {
                CreateDescriptors(SpheresId, Config.DataIdx, value);
            }
        }

        private PropertyDescriptorCollection createDescriptors()
        {
            PropertyDescriptorCollection origProperties = TypeDescriptor.GetProperties(typeof(DataSphere));

            ArrayList properties = new ArrayList();
            HashSet<string> handledNames = new HashSet<string>();

            foreach (PropertyDescriptor desc in origProperties)
                if (typeof(ICollection).IsAssignableFrom(desc.PropertyType))
                {
                    if (desc.Name == "Trells")
                    {
                        if (handledNames.Add("Trells"))
                            properties.Add(new TrellsDescriptor("Data Trellises", "Data Trellises"));
                    }
                    else if (desc.Name == "RootTrells")
                    {
                        if (handledNames.Add("RootTrells"))
                            properties.Add(new TrellsDescriptor("Root Trellises", "Root Trellises"));
                    }
                    else if (desc.Name == "ExpandTrells")
                    {
                        if (handledNames.Add("ExpandTrells"))
                            properties.Add(new TrellsDescriptor("Expand Trellises", "Expand Trellises"));
                    }
                    else if (desc.Name == "Relays")
                    {
                        if (handledNames.Add("Relays"))
                            properties.Add(new SphereRelaysDescriptor("Relation Trellises", "Relation Trellises"));
                    }
                    else if (desc.Name == "SpheresIn")
                    {
                        if (handledNames.Add("SpheresIn"))
                            properties.Add(new SpheresDescriptor("Sphere's Inside", "Sphere's Inside"));
                    }                 
                }

            List<string> propnames = new List<string>() { "SphereId", "DisplayName", "CountView", "Count",
                                                          "Synced", "Edited", "Saved", "Checked", "DataPlace", "DataIdx" };
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
                listName = SpheresId;
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
            return this.AreaOn;
        }

        public byte[] GetShah()
        {
            return (Config.Place != null) ? Config.Place.GetShah() : null;
        }

        public int    GetKeyHash()
        {
            return (Config.Place != null) ? Config.Place.GetHashCode() : 0;
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
            return (Config.Path != null) ? Config.Path : (AreaOn != null) ? AreaOn.Config.Path + "/" + SpheresId : SpheresId;
        }
        public string GetMapName()
        {
            return Config.Path + "/" + SpheresId + ".sphs";
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
        public int ItemsCount { get { return Spheres.Count; } }
        #endregion

        #region  ExpansionTools
        public object Emulator(object source, string name = null)
        {
            return this.Emulate((DataSpheres)source, name);
        }
        public object Imitator(object source, string name = null)
        {
            return this.Imitate((DataSpheres)source, name);
        }
        public object Impactor(object source, string name = null)
        {
            bool toDrive = false;
            DataSpheres srl = (DataSpheres)source;
            if (srl.State.Saved)
                toDrive = true;
            return this.Impact((DataSpheres)source, toDrive, name);
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
        }
        public void ReadDrive()
        {
            if (Drive == null) OpenDrive();
            DriveContext dc = new DriveContext();
            object read = dc.ReadDrive(Drive);
            this.Impact((DataSpheres)Deserialize(ref read));
            State.Saved = false;
            dc.Dispose();
        }
        public void OpenDrive()
        {
            if(Drive == null)
                Drive = new DriveBank(Config.File, Config.File, 5 * 1024 * 1024, typeof(DataSpheres));
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
            get { return SpheresId; }
        }
        public object TreeNodeTag
        {
            get { return this; }
        }
        public IDataTreeBinder[] TreeNodeChilds
        {
            get { return Spheres.Select(a => (IDataTreeBinder)a.Value).ToArray(); }
        }
        public IDataTreeBinder BoundedTree
        { get { return boundedTree; } set { boundedTree = value; } }

        #endregion
    }
}
