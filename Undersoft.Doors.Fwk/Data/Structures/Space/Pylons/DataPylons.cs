using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Doors.Mathtab;
using System.Doors;

namespace System.Doors.Data
{ 
    [JsonArray]
    [Serializable]
    public class DataPylons : Collection<DataPylon>, IBindingList, ITypedList, IListSource, ICollection, IDataGridBinder, INoid, IDataPylons
    {
        #region Private NonSerilaized
        [NonSerialized] private IList<int> editablePylons = new List<int>();
        [NonSerialized] private IList<DataPylon> visiblePylons = new List<DataPylon>();
        [NonSerialized] private IList<DataPylon> defaultPylons = new List<DataPylon>();
        [NonSerialized] private IList<DataPylon> autoIdPylons = new List<DataPylon>();
        [NonSerialized] private IList<DataPylon> firstCubePylons = new List<DataPylon>();      
        [NonSerialized] private short[] ordinals;
        [NonSerialized] private Type[] typeArray;
        [NonSerialized] private MattabData dataLine;
        private static int HASHES_CAPACITY_STANDARD_VALUE = 51;                          
        private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;     // established max count for access requests to hashes       
        private int noidOrdinal = -1;
        private int quikOrdinal = -1;
        #endregion

        [NonSerialized] public DataTrellis Trell;

        [NonSerialized]
        public FieldInfo[] PylonFields;

        public bool isKeys { get; set; }
        public bool hasAutoId { get; set; }
        public bool newFormula { get; set; }

        public DataPylons()
        {
            Config = new DataConfig(this, DataStore.Space);
            dataLine = new MattabData();
        }
        public DataPylons(DataTrellis trell, bool _isKeys = false)
        {
            isKeys = _isKeys;
            Trell = trell;
            Config = new DataConfig(this, DataStore.Space);
            MattabPylons = new DataPylons();
            LeftMattabPylons = new DataPylons();
            dataLine = new MattabData(Trell.Tiers);
        }
        public DataPylons(DataTrellis trell, DataPylon[] pylonarray, bool _isKeys = false)
        {
            isKeys = _isKeys;
            Trell = trell;
            Config = new DataConfig(this, DataStore.Space);
            this.AddRange(pylonarray);
            MattabPylons = new DataPylons();
            LeftMattabPylons = new DataPylons();
            dataLine = new MattabData(Trell.Tiers);
        }

        public DataConfig Config { get; set; }
      
        public bool Have(string PylonName)
        {
            return RegistryByName.ContainsKey(PylonName.GetShahCode64());
        }
        public bool Have(string PylonName, int Ordinal)
        {
            DataPylon pyl = RegistryByName[PylonName.GetShahCode64()];
            if (pyl != null && pyl.Ordinal == Ordinal)
                return true;
            return false;
        }
        public bool Have(int Ordinal)
        {
            return this.Count > Ordinal;
        }

        IDataPylon IDataPylons.this[string pylonName]
        {
            get
            {
                return RegistryByName[pylonName.GetShahCode64()];
            }
        }
        IDataPylon IDataPylons.this[int id]
        {
            get
            {
                return this[id];
            }
        }

        public DataPylon this[string pylonName]
        {
            get
            {
                return RegistryByName[pylonName.GetShahCode64()];
            }
            set
            {
                Vessel<DataPylon> pyl = RegistryByName.GetVessel(pylonName.GetShahCode64());
                if (pyl != null)
                {
                    base.SetItem(pyl.Value.Ordinal, value);
                    pyl.Value = value;
                }
            }
        }

        public DataPylon   GetPylon(string PylonName)
        {
            return RegistryByName[PylonName.GetShahCode64()];
        }
        public DataPylon[] GetPylons(ICollection<string> PylonNames)
        {
            return PylonNames.Select(n => RegistryByName[n.GetShahCode64()]).Where(p => p != null).ToArray();
        }

        public DataPylon[] AsArray()
        {
            return Items.ToArray();
        }

        public  Type[]  TypeArray
        { get { return (typeArray != null && typeArray.Length == this.Count) ? typeArray : typeArray = this.AsEnumerable().Select(d => d.DataType).ToArray(); } set { typeArray = value; } }

        public  short[] Ordinals
        {
            get
            {
                if (ordinals == null || ordinals.Length != Count)
                    return ordinals = this.AsEnumerable().Select(o => (short)o.Ordinal).ToArray();
                else
                    return ordinals;
            }
        }

        public IList<int> EditablePylons
        {
            get
            {
                if (editablePylons.Count == 0)
                    editablePylons = this.AsEnumerable().Where(p => p.Editable).Select(p => p.Ordinal).ToList();
                return editablePylons;
            }
            set
            {
                editablePylons = value;
            }
        }
        public IList<DataPylon> VisiblePylons
        {
            get
            {
                if (visiblePylons.Count == 0)
                    visiblePylons = this.AsEnumerable().Where(p => p.Visible).ToList();
                return visiblePylons;
            }
            set
            {
                visiblePylons = value;
            }
        }
        public IList<DataPylon> DefultPylons
        {
            get
            {
                if (defaultPylons.Count == 0)
                    defaultPylons = this.AsEnumerable().Where(p => p.Default != null).ToList();
                return defaultPylons;
            }
            set
            {
                defaultPylons = value;
            }
        }
        public IList<DataPylon> AutoIdPylons
        {
            get
            {
                if (autoIdPylons.Count == 0)
                { 
                    autoIdPylons = this.AsEnumerable().Where(p => p.isAutoincrement).ToList();
                    if (autoIdPylons.Count > 0)
                        hasAutoId = true;
                    else
                        hasAutoId = false;
                }
                return autoIdPylons;
            }
            set
            {
                autoIdPylons = value;
            }
        }
        public IList<DataPylon> FirstCubePylons
        {
            get
            {
                if (firstCubePylons.Count == 0)
                {
                    int maxlevel = Trell.Pylons.AsEnumerable().Max(p => p.CubeLevel);
                    int[] cubeids = new int[] { -1 };
                    List<DataPylon> firstCubePylons = new List<DataPylon>();
                    DataPylon[] fp = Trell.Pylons.AsEnumerable().Where(p => p.CubeIndex != null && p.CubeIndex.SequenceEqual(cubeids) && p.CubeLevel == 0).ToArray();
                    if (fp.Length > 0)
                        firstCubePylons.Add(fp[0]);

                    List<DataSubCube> dsclist = new List<DataSubCube>() { Trell.Cube.SubCube };
                    for (int i = 0; i < maxlevel; i++)
                    {
                        List<DataSubCube> tempdsclist = new List<DataSubCube>();
                        foreach (DataSubCube dsc in dsclist)
                        {
                            foreach (DataSubCubeRelay dscr in dsc.SubCubeRelays)
                            {
                                int[] tempids = dsc.CubeIndex.Concat(new int[] { dscr.CubeIndex }).ToArray();
                             
                                DataPylon[] sfp = Trell.Pylons.AsEnumerable().Where(p => p.CubeIndex != null && p.CubeIndex.SequenceEqual(tempids) && p.CubeLevel == i + 1).ToArray();
                                if (sfp.Length > 0)
                                    firstCubePylons.Add(sfp[0]);
                                if (dscr.SubCube != null)
                                    tempdsclist.Add(dscr.SubCube);
                            }
                        }
                        dsclist = tempdsclist;
                    }
                }
                return firstCubePylons;
            }
        }

        public void SetAutoIdValues()
        {
            if (hasAutoId)
            {
                if (Trell.Count > 0)
                    AutoIdPylons.Select(p => p.AutoIndex = Trell.Tiers.AsEnumerable().Max(x => (int)x[p.Ordinal]) + p.AutoStep).ToArray();
            }
        }

        public bool IsEditable(int id)
        {
          return this[id].Editable;
        }
        public bool IsEditable(string name)
        {
            return this[name].Editable;
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public DataPylons Clone()
        {
            DataPylons mx = (DataPylons)this.MemberwiseClone();
            return mx;
        }

        public int NoidOrdinal
        {
            get
            {
                if (noidOrdinal < 0)
                {
                    DataPylon[] noidord = this.AsEnumerable().Where(p => p.isNoid).ToArray();
                    if (noidord.Length > 0)
                        noidOrdinal = noidord[0].Ordinal;
                }
                return noidOrdinal;
            }
        }
        public int QuidOrdinal
        {
            get
            {
                if (quikOrdinal < 0)
                {
                    DataPylon[] quikord = this.AsEnumerable().Where(p => p.isQuid && p.isIdentity).ToArray();
                    if (quikord.Length > 0)
                        quikOrdinal = quikord[0].Ordinal;
                }
                return quikOrdinal;
            }
        }

        [NonSerialized]
        public HashList<DataPylon> RegistryByName = new HashList<DataPylon>();
        [NonSerialized]
        public HashList<DataPylon> RegistryByDisplay = new HashList<DataPylon>();

        #region Noid
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
            return (Config.Path != null) ? Config.Path : Trell.Config.Path;
        }
        public string GetMapName()
        {
            return (Trell != null)? Trell.Config.Path + ".pyls": Config.Path + ".pyls";
        }
        #endregion

        #region ICollection
        public new int Add(DataPylon pylon)
        {
            if (!this.Have(pylon.PylonName))
            {
                if (Trell != null)
                {
                    pylon.Trell = Trell;
                    if (isKeys)
                        pylon = Trell.Pylons.GetPylon(pylon.PylonName);

                    pylon.isCube = Trell.IsCube;

                    if (pylon.isAutoincrement)
                        pylon.SetAutoIdHandler(pylon.isAutoincrement);
                }

                if (isKeys)
                    pylon.Keys = this;
                else if(pylon.Pylons == null)
                    pylon.Pylons = this;

                int index = ((IList)this).Add(pylon);

                if (pylon.Ordinal < 0)
                    pylon.Ordinal = index;

                RegistryByName.Add(pylon.PylonName.GetShahCode64(), pylon);
                RegistryByDisplay.Add(pylon.DisplayName.GetShahCode64(), pylon);

                return index; 
                
            }
            else if (this.Have(pylon.PylonName, pylon.Ordinal))
            {
                if(Trell != null)
                    if (isKeys)
                        pylon = Trell.Pylons.GetPylon(pylon.PylonName);

                base.Insert(pylon.Ordinal, pylon);
                return pylon.Ordinal;
            }
            return -1;
        }
        
        public int    AddRange(ICollection<DataPylon> cols)
        {
            int added = 0;
            foreach (DataPylon p in cols)
                if (!this.Have(p.PylonName))
                {
                    added++;
                    this.Add(p);
                }
            return added;
        }
        public int    AddRange(DataPylons cols)
        {
            int added = 0;
            foreach (DataPylon p in cols)
                if (!this.Have(p.PylonName))
                {
                    added++;
                    this.Add(p);
                }
            return added;
        }
        public object AddNew()
        {
            return (object)((IBindingList)this).AddNew();
        }
       
        //public void   SetRange(DataPylon[] data)
        //{
        //    if (InnerList.Count != data.Length || InnerList.Count == 0)
        //    {
        //        InnerList.Clear();
        //        if (isKeys)
        //            data = Trell.Pylons.GetPylons(data.Select(t => t.PylonName).ToList());
        //        InnerList.AddRange(data);
        //        Trell.Model = null;
        //        return;
        //    }
        //    InnerList.SetRange(0, data);
        //    Trell.Model = null;
        //}
        //public int    IndexOf(object value)
        //{
        //    for (int i = 0; i < this.List.Count; i++)
        //        if (ReferenceEquals(this[i], value) || this[i].Equals(value))    // Found it
        //            return i;
        //    return -1;
        //}4
        public new void Insert(int index, DataPylon pylon)
        {
            if (Trell != null)
                if (isKeys)
                    pylon = Trell.Pylons.GetPylon(pylon.PylonName);

            if (isKeys)
                pylon.Keys = this;
            else
                pylon.Pylons = this;

            base.Insert(index, pylon);
        }

        public void Reset()
        {
            this.Clear();
        }

        protected override void ClearItems()
        {           
            RegistryByDisplay.Clear();
            RegistryByName.Clear();
            if(MattabPylons != null)
                MattabPylons.Clear();
            if (LeftMattabPylons != null)
                LeftMattabPylons.Clear();
            base.ClearItems();
        }

        public DataPylon Find(DataPylon data)
        {
            DataPylon pyl = RegistryByName[data.PylonName.GetShahCode64()];
            if (pyl != null && ReferenceEquals(pyl, data))
                return pyl;
            return null;
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

        //protected virtual void OnListChanged(ListChangedEventArgs ev)
        //{
        //    if (onListChanged != null)
        //    {
        //        onListChanged(this, ev);
        //    }
        //}
        //protected override void OnClear()
        //{
        //    foreach (DataPylon c in List)
        //    {
        //        c.Trell = null;
        //    }
        //}
        //protected override void OnClearComplete()
        //{
        //    OnListChanged(resetEvent);
        //}
        //protected override void OnInsertComplete(int index, object value)
        //{
        //    DataPylon c = (DataPylon)value;
        //    if (Trell != null)
        //    {
        //        c.Trell = this.Trell;
        //        c.Trell.PylonId = null;
        //    }

        //    if (isKeys)
        //        c.Keys = this;
        //    else
        //        c.Pylons = this;
        //    OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        //}
        //protected override void OnRemoveComplete(int index, object value)
        //{
        //    DataPylon c = (DataPylon)value;
        //    c.Trell.Model = null;
        //    c.Trell.PylonId = null;
        //    c.Trell = null;
        //    OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
        //}
        //protected override void OnSet(int index, object oldValue, object newValue)
        //{

        //}
        //protected override void OnSetComplete(int index, object oldValue, object newValue)
        //{
        //    if (oldValue != newValue)
        //    {
        //        object oldItem = oldValue;
        //        object newItem = newValue;

        //        oldItem = null;
        //        newItem = this;               

        //        OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        //    }
        //}       

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
            DataPylon c = new DataPylon(Trell);
            if (isKeys)
                c.Keys = this;
            else
                c.Pylons = this;
            base.Add(c);
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
                return CreateDescriptors(Trell.TrellName + " Pylons", Config.DataIdx);
            }
            set
            {
                CreateDescriptors(Trell.TrellName + " Pylons", Config.DataIdx, value);
            }
        }

        private PropertyDescriptorCollection createDescriptors()
        {
            PropertyDescriptorCollection origProperties = TypeDescriptor.GetProperties(typeof(DataPylon));

            ArrayList properties = new ArrayList();
            HashSet<string> handledNames = new HashSet<string>();
            List<string> propnames = new List<string>();
            if (!isKeys)
                propnames = new List<string>() { "PylonName", "DisplayName", "Visible", "Editable", "Ordinal", "DataType", "Default", "PylonSize", "MaxLength",
                                                 "isDBNull", "isIdentity", "isKey", "isIndex", "isAutoincrement", "ArithmeticMode", "JoinOperand",
                                                 "TotalOperand", "RevalOperand", "RevalType" };
            else
                propnames = new List<string>() { "PylonName", "DisplayName", "Ordinal", "DataType", "PylonSize",
                                                 "isDBNull", "isIdentity", "isKey", "isIndex", "isAutoincrement" };
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
                listName = Trell.TrellName + " Pylons";
            return listName;
        }
        #endregion

        #region MattabData

        private DataTiers Tiers
        {
            get
            {
                switch (ComputeSource)
                {
                    case DataModes.Tiers:
                        return Trell.Tiers;
                    case DataModes.Sims:
                        return Trell.Sims;
                    default:
                        return Trell.Tiers;
                }
            }
        }

        public DataModes ComputeSource
        { get; set; } = DataModes.Tiers;
       
        public int MattabPylonsCount
        {
            get
            {
                return MattabPylons.Count;
            }
        }
        public int MattabTiersCount
        {
            get                          
            {
                return Tiers.Count;
            }
        }

        public DataPylons MattabPylons
        { get; set; }
        public DataPylons LeftMattabPylons
        { get; set; }
        
        public MattabData Data
        {
            get
            {
                return dataLine;
            }
            set
            {
                dataLine = value;
            }
        }

        public bool SetMattabData(DataTiers tiers)
        {
            if (!ReferenceEquals(dataLine.iTiers, tiers.Data.iTiers))
            {
                Data = tiers.Data;
                PreparedEvaluator[] evs = GetMattabEvaulators();
                bool[] b = evs.Select(e => e.SetParams(Data.iTiers, 0)).ToArray();
                return true;
            }
            GetMattabEvaulators();
            return false;

        }

        public PreparedEvaluator[] GetMattabEvaulators()
        {
            return LeftMattabPylons.Select(m => m.CompileMattab()).ToArray();          
        }   


        #endregion

    }
}
