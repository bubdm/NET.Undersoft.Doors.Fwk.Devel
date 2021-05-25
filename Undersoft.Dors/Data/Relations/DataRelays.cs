using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Data;
using System.Dors;

namespace System.Dors.Data
{
    [JsonArray]
    [Serializable]
    public class DataRelays : Collection<DataRelay>, IBindingList, ITypedList, IListSource, ICollection, IDataGridBinder, INoid, IDataConfig, IList<DataRelay>
    {
        private static int HASHES_CAPACITY_STANDARD_VALUE = 51;                          // probably there is no need for more then 500 values in hashes, +1 cause 0based count
        private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;     // established max count for access requests to hashes

        public DataSphere SphereOn
        { get; set; }

        public DataConfig Config
        { get; set; }
        public DataState State
        { get; set; } = new DataState();

        public bool Checked
        {
            get { return State.Checked; }
            set { State.Checked = value; }
        }
        public bool Synced
        {
            get { return State.Synced; }
            set { State.Synced = value; }
        }
        public bool Edited
        {
            get { return State.Edited; }
            set { State.Edited = value; }
        }
        public bool Canceled
        {
            get { return State.Canceled; }
            set { State.Canceled = value; }
        }
        public bool Deleted
        {
            get { return State.Canceled; }
            set { State.Canceled = value; }
        }
        public bool Saved
        {
            get { return State.Saved; }
            set { State.Saved = value; }
        }

        public DataRelays()
        {
            Config = new DataConfig(this, DataStore.Space);            
        }
        public DataRelays(DataSphere sphere)
        {
            SphereOn = sphere;
            Config = new DataConfig(this, DataStore.Space);
        }
        public DataRelays(ICollection<DataRelay> relays)
        {
            Config = new DataConfig(this, DataStore.Space);
            AddRange(relays);
        }

        public DataRelay this[string relayName]
        {
            get
            {
                long k = relayName.GetShahCode64();
                if (RegistryIdByName.ContainsKey(k))
                    return this[RegistryIdByName.Get(k)];
                return null;
            }
        }

        [NonSerialized]
        public HashList<int> RegistryIdByName = new HashList<int>();

        public DataRelay Collect(string RelayName)
        {
            return this.Where(c => RelayName == c.RelayName).First();
        }
        public ICollection<DataRelay> Collect(ICollection<string> RelayNames = null)
        {
            if (RelayNames != null)
                return this.Where(c => RelayNames.Contains(c.RelayName)).ToList();
            else
                return this.ToList();
        }
        public ICollection<DataRelay> Collect(ICollection<DataRelay> relays)
        {
            if (relays != null)
                return this.Where(c => relays.Select(r => ReferenceEquals(r, c)).Any()).ToList();
            else
                return this.ToList();
        }

        public DataRelay GetByChild(string ChildName)
        {
            return this.Where(c => c.ChildName == ChildName).FirstOrDefault();           
        }
        public DataRelay GetByParent(string ParentName)
        {
            return this.Where(c => c.ParentName == ParentName).FirstOrDefault();
        }

        public int GetIdByChild(string ChildName)
        {
            DataRelay result = this.Where(c => c.ChildName == ChildName).FirstOrDefault();
            if (result != null)
                return RegistryIdByName.Get(result.RelayName.GetShahCode64());
            return -1;
        }
        public int GetIdByParent(string ParentName)
        {
            DataRelay result = this.Where(c => c.ParentName == ParentName).FirstOrDefault();
            if (result != null)
                return RegistryIdByName.Get(result.RelayName.GetShahCode64());
            return -1;
        }

        public int GetIdByName(string relayName)
        {
            long k = relayName.GetShahCode64();
            if(RegistryIdByName.ContainsKey(k))           
                return RegistryIdByName.Get(k);
            return -1;
        }
        public int[] GetIdByRelay(ICollection<DataRelay> relays)
        {
            if (relays != null)
                return relays.Select(r => RegistryIdByName.Get(r.RelayName.GetShahCode64())).ToArray();
            else
                return this.Select((x, y) => y).ToArray();
        }

        #region INoid     
        public byte[] GetShah()
        {
            return (Config.Place != null) ? Config.Place.GetShah() : null;
        }

        public ushort GetDriveId()
        {
            throw new NotImplementedException();
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
            return (Config.Path != null) ? Config.Path : (SphereOn != null) ? SphereOn.Config.Path : "Relays";
        }
        public string GetMapName()
        {
            return (SphereOn != null) ? SphereOn.Config.Path + "/" + SphereOn.SphereId + " .rlys" : Config.Path + "/Relays.rlys";
        }
        #endregion

        #region ICollection
        public bool Have(string relayName)
        {
            return RegistryIdByName.ContainsKey(relayName.GetShahCode64());
        }
        public bool IsReadOnly
        {
            get { return false; }
        }    
        public new int Add(DataRelay value)
        {
            if (value != null)
                if (!Have(value.RelayName))
                {
                    value.Relays = this;
                    int id = ((IList)this).Add(value);
                    RegistryIdByName.Put(value.RelayName.GetShahCode64(), id);
                    return id;
                }
            return -1;
        }
        public void AddRange(ICollection<DataRelay> _relaydata)
        {
            DataRelay[] _relays = _relaydata.Where(c => !this.AsEnumerable().Where(r => ReferenceEquals(r.Child, c.Child) &&
                                                                               ReferenceEquals(r.Parent, c.Parent)).Any()).ToArray();
            foreach (DataRelay _relay in _relays)
            {
                this.Add(_relay);
            }
        }
        public object AddNew()
        {
            return (object)((IBindingList)this).AddNew();
        }
        public int IndexOf(object value)
        {
            int l = this.Count;
            for (int i = 0; i < l; i++)
                if (ReferenceEquals(this[i], value) || this[i].Equals(value))    // Found it
                    return i;
            return -1;
        }
        public int IndexOfChild(string ChildName)
        {
            int l = this.Count;
            for (int i = 0; i < l; i++)
                if (this[i].ChildName == ChildName)    // Found it
                    return i;
            return -1;
        }
        public int IndexOfParent(string ParentName)
        {
            int l = this.Count;
            for (int i = 0; i < l; i++)
                if (this[i].ParentName.Equals(ParentName))    // Found it
                    return i;
            return -1;
        }
        public int IndexOfRelay(string RelayName)
        {
            int l = this.Count;
            for (int i = 0; i < l; i++)
                if (this[i].RelayName.Equals(RelayName))    // Found it
                    return i;
            return -1;
        }

        public DataRelay Find(DataRelay data)
        {
            foreach (DataRelay lDetailValue in this)
                if (ReferenceEquals(lDetailValue, data) || lDetailValue == data)    // Found it
                    return lDetailValue;
            return null;    // Not found
        }
        public void Reset()
        {           
            this.Clear();
        }
        #endregion

        #region IBindingList

        public object BindingSource { get { return this; } }
        public IDepotSync DepotSync { get { return BoundedGrid.DepotSync; } set { BoundedGrid.DepotSync = value; } }
        public IDataGridStyle GridStyle { get; set; }
        [NonSerialized] private IDataGridBinder boundedGrid;
        public IDataGridBinder BoundedGrid { get { return boundedGrid; } set { boundedGrid = value; } }

        IList IListSource.GetList()
        {
            return (IList)BindingSource;
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

        protected override void SetItem(int index, DataRelay value)
        {
            DataRelay t = base[index];
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
            base.SetItem(index, value);
        }
        protected override void InsertItem(int index, DataRelay item)
        {
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
            item.Relays = this;
            base.InsertItem(index, item);
        }
        protected override void RemoveItem(int index)
        {
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
            base.RemoveItem(index);
        }
        protected override void ClearItems()
        {          
            OnListChanged(resetEvent);
            RegistryIdByName.Clear();
            base.ClearItems();
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

        object IBindingList.AddNew()
        {
            DataRelay c = new DataRelay();
            Add(c);
            return c;
        }
        bool IBindingList.IsSorted
        {
            get { throw new NotSupportedException(); }
        }
        ListSortDirection IBindingList.SortDirection
        {
            get { throw new NotSupportedException(); }
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

        [NonSerialized] private ListChangedEventArgs resetEvent = new ListChangedEventArgs(ListChangedType.Reset, -1);
        [NonSerialized] private ListChangedEventHandler onListChanged;
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
                return CreateDescriptors("Relays", Config.DataIdx);
            }
            set
            {
                CreateDescriptors("Relays", Config.DataIdx, value);
            }
        }

        private PropertyDescriptorCollection createDescriptors()
        {
            PropertyDescriptorCollection origProperties = TypeDescriptor.GetProperties(typeof(DataRelay));

            ArrayList properties = new ArrayList();
            HashSet<string> handledNames = new HashSet<string>();

            List<string> propnames = new List<string>() { "ParentName", "ChildName" };
            foreach (string propname in propnames)
            {
                if (handledNames.Add(propname))
                    properties.Add(origProperties[propname]);
            }

            foreach (PropertyDescriptor desc in origProperties)
                if (typeof(ICollection).IsAssignableFrom(desc.PropertyType))
                {
                    if (desc.Name == "ParentKeys")
                    {
                        if (handledNames.Add("ParentKeys"))
                            properties.Add(new DataRelayPylonsDescriptor("ParentKeys", "ParentKeys", DataRelaySite.Parent));
                    }
                    else if (desc.Name == "ChildKeys")
                    {
                        if (handledNames.Add("ChildKeys"))
                            properties.Add(new DataRelayPylonsDescriptor("ChildKeys", "ChildKeys", DataRelaySite.Child));
                    }                 
                }

            return new PropertyDescriptorCollection((PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor)));
        }
        public PropertyDescriptorCollection CreateDescriptors(string name, string _dataId, PropertyDescriptorCollection _descriptors = null)
        {
            string dataId = _dataId;
            PropertyNaming.AddOrUpdate(name, _dataId, (k, v) => dataId);

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
                        descriptors = propertyDescriptors;
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
                listName = SphereOn.SphereId + " Relays";
            return listName;
        }


        #endregion

    }
    [JsonObject]
    [Serializable]
    public class DataRelayMember
    {
        public DataRelayMember()
        {
            RelayKeys = new DataPylons();
            RelayKeys.isKeys = true;
        }
        public DataRelayMember(DataTrellis trell)
        {
            Trell = trell;
            TrellName = trell.TrellName;
            RelayKeys = new DataPylons(trell, true);
        }

        public string TrellName { get; set; }
        public DataTrellis Trell { get; set; }

        public int JoinCount = 0;

        [NonSerialized] public DataPylons RelayPylons;
        public DataPylon[] PylonsArray
        {
            get
            {
                if (RelayPylons == null)
                    RelayPylons = new DataPylons(Trell, true);
                return RelayPylons.AsArray();
            }
            set
            {
                if (RelayPylons == null)
                    RelayPylons = new DataPylons(Trell, true);
                RelayPylons.AddRange(value);
            }
        }

        public DataPylons RelayKeys;
        public DataPylon[] PylonKeys
        {
            get
            {
                if (RelayKeys == null)
                    RelayKeys = new DataPylons(Trell, true);
                return RelayKeys.AsArray();
            }
            set
            {
                if (RelayKeys == null)
                    RelayKeys = new DataPylons(Trell, true);
                RelayKeys.AddRange(value);
            }
        }

        [NonSerialized] private int[] keysOrdinal;
        public int[] KeysOrdinal
        {
            get
            {
                return (keysOrdinal == null) ? keysOrdinal = RelayKeys.AsEnumerable().Select(o => o.Ordinal).ToArray() : keysOrdinal;
            }
        }            
    }
    [JsonObject]
    [Serializable]
    public class DataRelay
    {
        [NonSerialized] public DataRelays Relays;
      
        public DataRelay()
        {
            RelayName = "Relay#"+DateTime.Now.ToFileTime().ToString();
            Parent = new DataRelayMember();
            Child = new DataRelayMember();
        }
        public DataRelay(DataTrellis parent, DataTrellis child)
        {
            RelayName = parent.TrellName + "_" + child.TrellName;
            Parent = new DataRelayMember(parent);
            Child = new DataRelayMember(child);
        }        

        public string RelayName { get; set; }
        public DataRelayMember Parent { get; set; }
        public DataRelayMember Child { get; set; }

        public string ParentName
        {
            get { return Parent.TrellName; }
            set
            {
                if (Relays != null)
                    if (Relays.SphereOn != null && Relays.SphereOn.Trells.Have(value))
                    {
                        DataTrellis trell = Relays.SphereOn.Trells[value];
                        Parent.Trell = trell;
                        Parent.TrellName = trell.TrellName;
                        Parent.RelayPylons = new DataPylons(trell, true);
                        Parent.RelayKeys = new DataPylons(trell, true);
                    }
            }
        }
       
        public DataPylons ParentPylons
        {
            get
            {
                return Parent.RelayPylons;
            }
            set
            {
                Parent.RelayPylons = value;
            }
        }
        public DataPylons ParentKeys
        {
            get
            {
                return Parent.RelayKeys;
            }
            set
            {
                Parent.RelayKeys = value;
            }
        }

        public string ChildName
        {
            get { return Child.TrellName; }
            set
            {
                if (Relays != null)
                    if (Relays.SphereOn != null && Relays.SphereOn.Trells.Have(value))
                    {
                        DataTrellis trell = Relays.SphereOn.Trells[value];
                        Child.Trell = trell;
                        Child.TrellName = trell.TrellName;
                        Child.RelayPylons = new DataPylons(trell, true);
                        Child.RelayKeys = new DataPylons(trell, true);
                    }
            }
        }
      
        public DataPylons ChildPylons
        {
            get
            {
                return Child.RelayPylons;
            }
            set
            {
                Child.RelayPylons = value;
            }
        }
        public DataPylons ChildKeys
        {
            get
            {
                return Child.RelayKeys;
            }
            set
            {
                Child.RelayKeys = value;
            }
        }        
    }

    #region Relay Join Particles
    //[Serializable]
    //public struct DataJoinFull
    //{     
    //    public DataTier Parent { get; set; }
    //    public DataTier Child { get; set; }
    //}
    //[Serializable]
    //public class DataJoinFulls
    //{      
    //    public DataJoinFull[] Join { get; set; }
    //}
    //[Serializable]
    //public struct DataJoinChild
    //{
    //    public DataTier Parent { get; set; }
    //    public DataTier[] Child { get; set; }
    //}   
    //[Serializable]
    //public struct DataJoinParent
    //{
    //    public DataTier[] Parent { get; set; }
    //    public DataTier Child { get; set; }
    //}
    //[Serializable]
    //public class DataJoinChilds
    //{
    //    public DataTier Parent { get; set; }
    //    public List<DataTier[]> Childs { get; set; }
    //}
    //[Serializable]
    //public class DataJoinParents
    //{
    //    public DataTier Child { get; set; }
    //    public List<DataTier[]> Parents { get; set; }
    //}
    [Serializable]
    public class JoinFull
    {
        public DataTier Parent { get; set; }
        public DataTier Child { get; set; }
    }
    [Serializable]
    public class JoinChild
    {
        public DataTier Parent { get; set; }
        public DataTier[] Child { get; set; }
    }
    [Serializable]
    public class JoinParent
    {
        public DataTier Child { get; set; }
        public DataTier[] Parent { get; set; }
    }
    [Serializable]
    public class JoinCube
    {
        public JoinCube(DataTier subtier)
        {
            SubTier = subtier;
        }
        public JoinCube(int length)
        {
            SubTier = null;
            Child = new JoinCube[length];
        }
        public JoinCube(DataTier subtier, int length)
        {
            SubTier = subtier;
            Child = new JoinCube[length];
        }

        public DataTier SubTier; 
        public JoinCube[] Child;
    }
    [Serializable]
    public class JoinCubeList
    {
        public JoinCubeList()
        {
            List = new List<JoinCube>();
        }
        public JoinCubeList(int capacity)
        {
            List = new List<JoinCube>(capacity);
        }
        public JoinCubeList(IList<JoinCube> list)
        {
            List = list;
        }
        public IList<JoinCube> List;
        public JoinCube this[int id]
        {
            get { return List[id]; }
            set { List[id] = value; }
        }
    }
    [Serializable]
    public class JoinFullList
    {
        public JoinFullList(IList<JoinFull> list)
        {
            List = list;
        }
        public IList<JoinFull> List;
        public JoinFull this[int id]
        {
            get { return List[id]; }
            set { List[id] = value; }
        }
    }
    [Serializable]
    public class JoinChildList
    {
        public JoinChildList(IList<JoinChild> list)
        {
            List = list;
        }
        public IList<JoinChild> List;
        public JoinChild this[int id]
        {
            get { return List[id]; }
            set { List[id] = value; }
        }
    }
    [Serializable]
    public class JoinParentList
    {
        public JoinParentList(IList<JoinParent> list)
        {
            List = list;
        }
        public IList<JoinParent> List;
        public JoinParent this[int id]
        {
            get { return List[id]; }
            set { List[id] = value; }
        }
    }
    #endregion

    public enum DataRelaySite
    {
        Parent,
        Child
    }
}
