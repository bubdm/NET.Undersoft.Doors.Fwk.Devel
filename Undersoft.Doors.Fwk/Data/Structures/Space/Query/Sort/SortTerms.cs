using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Doors;

namespace System.Doors.Data
{
    [JsonArray]
    [Serializable]
    public class SortTerms : Collection<SortTerm>, IBindingList, ITypedList, IListSource, ICollection, IDataGridBinder, 
                                                   INoid, IDataDescriptor
    {
        private static int HASHES_CAPACITY_STANDARD_VALUE = 51;                          // probably there is no need for more then 500 values in hashes, +1 cause 0based count
        private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;     // established max count for access requests to hashes

        public DataConfig Config
        { get; set; }

        public SortTerms()
        {
            Config = new DataConfig(this, DataStore.Space);
 
        }
        public SortTerms(DataTrellis nTable)
        {          
            Trell = nTable;
            Config = new DataConfig(this, DataStore.Space);

        }

        [NonSerialized]
        private DataTrellis trell;
        public DataTrellis Trell
        { get { return trell; } set { trell = value; } }


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
            return (Config.Path != null) ? Config.Path : (Trell != null) ? Trell.Config.Path : "FilterTerms";
        }
        public string GetMapName()
        {
            return Config.Path + "/" + Trell.TrellName + ".strm";
        }

        public List<SortTerm> Get(List<string> PylonNames)
        {
            return this.AsEnumerable().Where(c => PylonNames.Contains(c.PylonName)).ToList();
        }
        public List<SortTerm> Get()
        {
            return this.AsEnumerable().Select(c => c).ToList();
        }
        public bool Have(string PylonName)
        {
            return this.AsEnumerable().Where(c => c.PylonName == PylonName).Any();
        }
        public SortTerm[] GetTerms(string PylonName)
        {
            return this.AsEnumerable().Where(c => c.PylonName == PylonName).ToArray();
        }

        public SortTerms Clone()
        {
            SortTerms mx = (SortTerms)this.MemberwiseClone();
            return mx;
        }

        #region ICollection
        public new int Add(SortTerm value)
        {
            value.Trell = Trell;
            value.Index = ((IList)this).Add(value);
            return value.Index;
        }
        public void AddRange(ICollection<SortTerm> terms)
        {
            foreach (SortTerm term in terms)
            {
                term.Trell = Trell;
                term.Index = ((IList)this).Add(term);
            }
        }
        public void AddNewRange(ICollection<SortTerm> terms)
        {
            bool diffs = false;
            if (Count != terms.Count)
            {
                diffs = true;
            }
            else
            {
                foreach (SortTerm term in terms)
                {
                    if (Have(term.PylonName))
                    {
                        int same = 0;
                        foreach (SortTerm myterm in GetTerms(term.PylonName))
                        {
                            if (myterm.Compare(term))
                                same++;
                        }
                        if (same == 0)
                        {
                            diffs = true;
                            break;
                        }
                    }
                    else
                    {
                        diffs = true;
                        break;
                    }
                }
            }
            if (diffs)
            {
                Clear();
                foreach (SortTerm term in terms)
                    term.Index = ((IList)this).Add(term);
            }

        }
        public void RemoveRange(ICollection<SortTerm> value)
        {
            foreach (SortTerm term in value)
                Remove(term);
        }
        public object AddNew()
        {
            return (object)((IBindingList)this).AddNew();
        }
      
        public void SetRange(SortTerm[] data)
        {
            for (int i = 0; i < data.Length; i++)
                this[i] = data[i];
        }
        public int IndexOf(object value)
        {
            for (int i = 0; i < Count; i++)
                if (this[i] == value)    // Found it
                    return i;
            return -1;
        }
     
        public SortTerm Find(SortTerm data)
        {
            foreach (SortTerm lDetailValue in this)
                if (lDetailValue == data)    // Found it
                    return lDetailValue;
            return null;    // Not found
        }
   
        #endregion

        #region IBindingList

        public object BindingSource { get { return this; } }
        public IDepotSync DepotSync { get { return BoundedGrid.DepotSync; }  set { BoundedGrid.DepotSync = value; } }
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
        //protected override void OnClear()
        //{
        //    foreach (SortTerm c in List)
        //    {
        //        c.SortedPylon = null;
        //    }
        //}
        //protected override void OnClearComplete()
        //{
        //    OnListChanged(resetEvent);
        //}
        //protected override void OnInsertComplete(int index, object value)
        //{
        //    SortTerm c = (SortTerm)value;
        //    c.SortedPylon = null;
        //    c.Trell = Trell;
        //    if (!c.Trell.State.Quered && c.Trell.State.Synced)
        //    {
        //        c.Trell.State.Quered = true;
        //        foreach (DataRelay ch in c.Trell.Relays)
        //        {
        //            ch.Child.Trell.State.Quered = true;
        //        }
        //    }
        //    OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        //}
        //protected override void OnRemoveComplete(int index, object value)
        //{
        //    SortTerm c = (SortTerm)value;
        //    if (!c.Trell.State.Quered && c.Trell.State.Synced)
        //    {
        //        c.Trell.State.Quered = true;
        //    }
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
        //        SortTerm oldItem = (SortTerm)oldValue;
        //        SortTerm newItem = (SortTerm)newValue;
        //        newItem.Trell = oldItem.Trell;
        //        oldItem = null;
        //        if (!newItem.Trell.State.Quered && newItem.Trell.State.Synced)
        //        {
        //            newItem.Trell.State.Quered = true;
        //        }
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
            SortTerm c = new SortTerm(Trell);
            c.Index = ((IList)this).Add(c);
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
                return CreateDescriptors(trell.TrellName + " Sort Terms", Config.DataIdx);
            }
            set
            {
                CreateDescriptors(trell.TrellName + " Sort Terms", Config.DataIdx, value);
            }
        }

        private PropertyDescriptorCollection createDescriptors()
        {
            PropertyDescriptorCollection origProperties = TypeDescriptor.GetProperties(typeof(SortTerm));

            ArrayList properties = new ArrayList();
            HashSet<string> handledNames = new HashSet<string>();

            List<string> propnames = new List<string>() { "PylonName", "Direction", "Ordinal" };
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
                listName = trell.TrellName + " Sort Terms";
            return listName;
        }
        #endregion
    }

    [Serializable]
    public class SortTerm
    {
        public string dataTypeName;
        [NonSerialized] private Type dataType;
        [NonSerialized] private DataTrellis trell;

        public DataState State = new DataState();


        public DataTrellis Trell
        {
            get { return trell; }
            set
            {
                if (value != null)
                {
                    trell = value;
                    if (pylonName != null)
                        if (value.Pylons.Have(pylonName))
                        {
                            DataPylon pyl = value.Pylons.AsEnumerable().Where(c => c.PylonName == pylonName).First();
                            if (pyl != null)
                            {
                                if (sortedPylon == null)
                                    sortedPylon = pyl;
                                if (DataType == null)
                                    DataType = pyl.DataType;
                                if (TypeString == null)
                                    TypeString = GetTypeString(pyl.DataType);
                            }
                        }                    
                }
            }
        }

        public SortTerm()
        {
        }
        public SortTerm(DataTrellis nTable)
        {
            Trell = nTable;
        }
        public SortTerm(string pylonName, string direction = "ASC", int ordinal = 0)
        {
                PylonName = pylonName;            
                SortDirection sortDirection;
                Enum.TryParse(direction, true, out sortDirection);
                Direction = sortDirection;
                Ordinal = ordinal;
        }

        public SortTerm(DataPylon sortedPylon, SortDirection direction = SortDirection.ASC, int ordinal = 0)
        {
            Direction = direction;
            SortedPylon = sortedPylon;
            Ordinal = ordinal;
        }

        public SortDirection Direction { get; set; }
        private DataPylon sortedPylon;
        public DataPylon SortedPylon
        {
            get { return sortedPylon; }
            set
            {
                if (value != null)
                {
                    sortedPylon = value;
                    pylonName = sortedPylon.PylonName;
                    DataType = sortedPylon.DataType;
                    TypeString = GetTypeString(DataType);
                }
            }
        }
        public Type DataType
        {
            get
            {
                if (dataType == null && dataTypeName != null)
                    dataType = Type.GetType(dataTypeName);
                return dataType;
            }
            set
            {
                dataType = value;
                dataTypeName = value.FullName;
            }
        }
        public string TypeString { get; set; }
        public int Ordinal { get; set; }
        private string pylonName;
        public string PylonName
        {
            get
            {
                return pylonName;
            }
            set
            {
                pylonName = value;
                if (Trell != null)
                {
                    if (Trell.Pylons.Have(pylonName))
                    {
                        if (sortedPylon == null)
                            SortedPylon = Trell.Pylons.AsEnumerable().Where(c => c.PylonName == PylonName).First();
                        if (DataType == null)
                            DataType = SortedPylon.DataType;
                        if (TypeString == null)
                            TypeString = GetTypeString(DataType);
                    }
                }
            }
        }
        public int Index { get; set; }
        private string GetTypeString(Type DataType)
        {
            Type dataType = DataType;
            string type = "string";
            if (dataType == typeof(string))
                type = "string";
            else if (dataType == typeof(int))
                type = "int";
            else if (dataType == typeof(decimal))
                type = "decimal";
            else if (dataType == typeof(DateTime))
                type = "DateTime";
            else if (dataType == typeof(Single))
                type = "Single";
            else if (dataType == typeof(float))
                type = "float";
            else
                type = "string";
            return type;
        }
        private string GetTypeString(DataPylon column)
        {
            Type dataType = column.DataType;
            string type = "string";
            if (dataType == typeof(string))
                type = "string";
            else if (dataType == typeof(int))
                type = "int";
            else if (dataType == typeof(decimal))
                type = "decimal";
            else if (dataType == typeof(DateTime))
                type = "DateTime";
            else if (dataType == typeof(Single))
                type = "Single";
            else if (dataType == typeof(float))
                type = "float";
            else
                type = "string";
            return type;
        }

        public bool Compare(SortTerm term)
        {
            if (PylonName != term.PylonName || Direction != term.Direction)
                return false;

            return true;
        }

    }
}
