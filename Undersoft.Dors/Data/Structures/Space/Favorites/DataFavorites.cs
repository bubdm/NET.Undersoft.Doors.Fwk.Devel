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
    public class DataFavorites : IDictionary<string, DataFavorite>, IEnumerable<KeyValuePair<string, DataFavorite>>, INoid,
                                 IListSource, ITypedList, IDataGridBinder, IBindingList, IList, IDataConfig, IDataDescriptor
    {
        #region Private / NonSerialized
        private static int HASHES_CAPACITY_STANDARD_VALUE = 11;                           // probably there is no need for more then 50 values in Spheres, +1 cause 0based count
        private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;     // established max count for access requests to Spheres
        [NonSerialized] private IDrive drive;
        #endregion

        public DataTrellis Trell;

        public DataFavorites(DataTrellis trell)
        {
            FavoritesId = "Favorites#" + DateTime.Now.ToFileTime().ToString();
            DisplayName = FavoritesId;
            Trell = trell;
            Config = new DataConfig(this, DataStore.Space);
            State = new DataState();
            Parameters = new DataParams();
            State.Expeled = true;
            Parameters = new DataParams();       
        }
        public DataFavorites(string favoritesId, DataTrellis trell)
        {
            FavoritesId = (favoritesId != null) ? favoritesId : "Favorites#" + DateTime.Now.ToFileTime().ToString();
            DisplayName = FavoritesId;
            Trell = trell;
            Config = new DataConfig(this, DataStore.Space);
            State = new DataState();
            Parameters = new DataParams();          
        }

        public string FavoritesId
        { get; set; }
        public string DisplayName
        { get; set; }      

        public SortedDictionary<string, DataFavorite> Favorites
        { get; set; } = new SortedDictionary<string, DataFavorite>();
       
        public DataFavorite NewFavorite(string favoriteId = null)
        {
            string FavoriteId = (favoriteId != null) ? favoriteId : "Favorite#" + DateTime.Now.ToFileTime().ToString();
            DataFavorite value = new DataFavorite(FavoriteId, this);
            this.Put(FavoriteId, value);
            return value;
        }

        public object this[int index]
        {
            get
            {
                string key = null;
                DataFavorite result = null;
                if (index <= Keys.Count - 1)
                {
                    key = Keys.ElementAt(index);
                    if (key != null)
                        if (Favorites.TryGetValue(key, out result))
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
                        if (Favorites.ContainsKey(key))
                        {
                            Favorites[key] = (DataFavorite)value;
                            ((DataFavorite)value).Config.SetMapConfig(this);
                        }                       
                }
            }
        }
        public DataFavorite this[string key]
        {
            get
            {
                DataFavorite result = null;
                if (Favorites.TryGetValue(key, out result))
                    return result;               
                return result;
            }
            set
            {                
                if (Favorites.ContainsKey(key))
                {
                    Favorites[key] = value;
                    ((DataFavorite)value).Config.SetMapConfig(this);                
                }          
            }
        }    

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

        public DataFavorites Clone()
        {
            return (DataFavorites)this.MemberwiseClone();
        }

        #region IEnumerable
        public IEnumerator<KeyValuePair<string, DataFavorite>> GetEnumerator()
        {
            return Favorites.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Favorites.GetEnumerator();
        }
        #endregion

        #region IDictionary
        public DataFavorite Get(string key)
        {
            DataFavorite result = null;
            Favorites.TryGetValue(key, out result);
            return result;
        }
        public void Add(string key, DataFavorite value)
        {
            value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
            this.Add(new KeyValuePair<string, DataFavorite>(key, value));
        }
        public void Add(KeyValuePair<string, DataFavorite> item)
        {
            if (this.TryAdd(item.Key, item.Value))
            {
                //item.Value.Config.SetMapConfig(this);               
                //item.Value.Trells.Config.SetMapConfig(item.Value);            
            }
        }
        public bool TryAdd(string key, DataFavorite value)
        { 
            value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
            if (Favorites.TryAdd(key, value))
            {
                value.Config.SetMapConfig(this);
                return true;
            }
            else
                return false;
        }
        public void Put(string key, DataFavorite value)
        {
            value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
            Favorites.AddOrUpdate(key, value, (k, v) => v = value);
            DataFavorite _sphr = Favorites[key];
            _sphr.Config.SetMapConfig(this);
        }
        public bool ContainsKey(string key)
        {
            return Favorites.ContainsKey(key);
        }
        public bool Remove(string key)
        {
            DataFavorite outset = null;
            bool done = Favorites.TryRemove(key, out outset);
            if (done)
            {
                object outreg = new object();
                DataSpace.Registry.TryRemove(outset.Config.GetDataId(), out outreg);
            }
            return done;
        }
        public bool TryGetValue(string key, out DataFavorite value)
        {
            return Favorites.TryGetValue(key, out value);
        }
        public void Clear()
        {
            Favorites.Clear();
        }
        public bool Contains(KeyValuePair<string, DataFavorite> item)
        {
            return Favorites.Contains(item);
        }
        public void CopyTo(KeyValuePair<string, DataFavorite>[] array, int arrayIndex)
        {
            Favorites.ToArray().CopyTo(array, arrayIndex);
        }
        public bool Remove(KeyValuePair<string, DataFavorite> item)
        {
            DataFavorite outset = null;
            bool done = Favorites.TryRemove(item.Key, out outset);
            if (done)
            {
                object outreg = new object();
                DataSpace.Registry.TryRemove(outset.Config.GetDataId(), out outreg);                                          
            }

            return done;
        }
        public ICollection<string> Keys
        {
            get
            {
                return Favorites.Keys;
            }
        }
        public ICollection<DataFavorite> Values
        {
            get
            {
                return Favorites.Values;
            }
        }
        public int Count
        {
            get
            {
                return Favorites.Count;
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
        public DataFavorite AddNew()
        {
            DataFavorite set = (DataFavorite)((IBindingList)this).AddNew();
            return set;
        }
        public DataFavorite Find(DataFavorite data)
        {
            if (Favorites.Values.Contains(data))
                return data;
            else if (Favorites.ContainsKey(data.FavoriteId))
                return Favorites[data.FavoriteId];
            else
                return null;    // Not found
        }

        public int  Add(object _set)
        {
            int index = -1;
            DataFavorite set = (DataFavorite)_set;
            if (!this.Have(set.FavoriteId))
            {
                if (this.TryAdd(set.FavoriteId, set))
                    index = Count - 1;
            }
            return index;
        }
        public void AddRange(ICollection<DataFavorite> _sets)
        {
            try
            {
                List<DataFavorite> newFavorites = _sets.Where(c => !this.Have(c.FavoriteId)).ToList();
                foreach (DataFavorite newSphere in newFavorites)
                {
                    newSphere.Favorites = this;
                    this.TryAdd(newSphere.FavoriteId, newSphere);
                }
            }
            catch (Exception ex)
            { }
        }
        public void Remove(object value)
        {
            string key = null;
            DataFavorite tempset = (DataFavorite)value;
            Favorites.Select((x, y) => (ReferenceEquals(x.Value, value) || x.Value.FavoriteId.Equals(((DataFavorite)value).FavoriteId)) ? key = x.Key : null).ToArray();
            if (key != null)
                Favorites.TryRemove(key, out tempset);
        }
        public void RemoveAt(int index)
        {
            int count = Keys.Count;
            string key = null;
            DataFavorite set = null;
            if (index >= count - 1)
            {
                key = Keys.ElementAt(index);
                Favorites.TryRemove(key, out set);
            }
        }
        public bool Have(string setId)
        {
            return this.ContainsKey(setId);
        }
        public void FavoriteRange(DataFavorite[] data)
        {
            Favorites.AddOrUpdateRange(new ConcurrentDictionary<string, DataFavorite>(data.Select(d => new KeyValuePair<string, DataFavorite>(d.FavoriteId, d)).ToArray()));
        }
        public void FavoriteRange(DataFavorites data)
        {
            Favorites.AddOrUpdateRange(data.Favorites);
        }
        public int  IndexOf(object value)
        {
            int index = -1;
            Values.Select((x, y) => (ReferenceEquals(x, value) || x.FavoriteId.Equals(((DataFavorite)value).FavoriteId)) ? index = y : 0).ToArray();
            return index;
        }
        public void Insert(int index, object data)
        {
            string key = null;
            if (index >= Keys.Count - 1)
            {
                key = Keys.ElementAt(index);
                if (key != null)
                    if (Favorites.ContainsKey(key))
                    {
                        DataFavorite value = (DataFavorite)data;
                        value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
                        Favorites[key] = value;
                        value.Config.SetMapConfig(this);
                    }
            }
        }
        public bool Contains(object data)
        {
            return (Find((DataFavorite)data) != null);
        }
        public void CopyTo(DataFavorite[] array, int arrayIndex)
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

        public IDataGridStyle GridStyle { get; set; }
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
            foreach (DataFavorite c in Values)
            {
                c.Favorites = null;
            }
        }
        protected void OnClearComplete()
        {
            OnListChanged(resetEvent);
        }
        protected void OnInsert(int index, object value)
        {
        }
        protected void OnInsertComplete(int index, object value)
        {
            DataFavorite c = (DataFavorite)value;
            c.Favorites = this;
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        }
        protected void OnRemove(int index, object value)
        {

        }
        protected void OnRemoveComplete(int index, object value)
        {
            DataFavorite c = (DataFavorite)value;
            c.Favorites = null;

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
            DataFavorite c = new DataFavorite("Favorite#" + DateTime.Now.ToFileTime().ToString(), this);
            Favorites.TryAdd(c.FavoriteId, c);            
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
                return CreateDescriptors(FavoritesId, Config.DataIdx);
            }
            set
            {
                CreateDescriptors(FavoritesId, Config.DataIdx, value);
            }
        }

        private PropertyDescriptorCollection createDescriptors()
        {
            PropertyDescriptorCollection origProperties = TypeDescriptor.GetProperties(typeof(DataFavorite));

            ArrayList properties = new ArrayList();
            HashSet<string> handledNames = new HashSet<string>();

            foreach (PropertyDescriptor desc in origProperties)
                if (typeof(ICollection).IsAssignableFrom(desc.PropertyType))
                {
                    if (desc.Name == "Favorite")
                    {
                        if (handledNames.Add("Favorite"))
                            properties.Add(new FavoriteDescriptor("Favorite", "Favorite"));
                    }
                }

            List<string> propnames = new List<string>() { "FavoriteId", "DisplayName", "FavoriteObject", "ObjectType" };
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
                listName = FavoritesId;
            return listName;
        }

        #endregion

        #region INoid
        public byte[] GetShah()
        {
            return (Config.Place != null) ? Config.Place.GetShah() : null;
        }

        public int GetKeyHash()
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
            return (Config.Path != null) ? Config.Path : (Trell != null) ? Trell.Config.Path + "/" + FavoritesId : FavoritesId;
        }
        public string GetMapName()
        {
            return Config.Path + "/" + FavoritesId + ".fvts";
        }
        public int GetKeyShah()
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
        public object GetHeader()
        {
            return this.Trell;
        }

        public int SerialCount { get; set; }
        public int DeserialCount { get; set; }
        public int ProgressCount { get; set; }
        public int ItemsCount { get { return Favorites.Count; } }
        #endregion

        //#region  ExpansionTools
        //public object Emulator(object source, string name = null)
        //{
        //    return this.Emulate((DataFavorites)source, name);
        //}
        //public object Imitator(object source, string name = null)
        //{
        //    return this.Imitate((DataFavorites)source, name);
        //}
        //public object Impactor(object source, string name = null)
        //{
        //    return this.Impact((DataFavorites)source, false, name);
        //}
        //public object Locator(string path = null)
        //{
        //    return this.Locate(path);
        //}
        //#endregion

        #region IDriveRecorder
        public IDrive Drive
        { get { return drive; } set { drive = value; } }
        public void WriteDrive()
        {
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
            //this.Impact((DataFavorites)Deserialize(ref read));
            dc.Dispose();
        }
        public void OpenDrive()
        {
            if (Drive == null)
                Drive = new DriveBank(Config.File, Config.File, 5 * 1024 * 1024, typeof(DataFavorites));
        }
        public void CloseDrive()
        {
            if (Drive != null)
                Drive.Close();
        }
        #endregion
    }
}
