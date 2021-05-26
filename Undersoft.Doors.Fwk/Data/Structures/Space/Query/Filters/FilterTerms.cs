using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Doors.Drive;
using System.Doors;
using System.Collections.ObjectModel;

namespace System.Doors.Data
{
    [JsonArray]
    [Serializable]
    public class FilterTerms : Collection<FilterTerm>, IBindingList, ITypedList, IListSource, ICollection, IDataGridBinder, 
                                                       INoid, IDataConfig, IDriveRecorder, IDataSerial, IDataDescriptor
    {
        private static int HASHES_CAPACITY_STANDARD_VALUE = 51;                          // probably there is no need for more then 500 values in hashes, +1 cause 0based count
        private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;     // established max count for access requests to hashes

        [NonSerialized] private IDrive drive;
        [NonSerialized] private DataTrellis trell;

        public FilterTerms()
        {
            Config = new DataConfig(this, DataStore.Space);          
        }
        public FilterTerms(DataTrellis nTrell)
        {
            Trell = nTrell;
            Config = new DataConfig(this, DataStore.Space);
        }

        public DataConfig Config
        { get; set; }
        public DataState State
        { get; set; } = new DataState();

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

        public DataTrellis Trell
        { get { return trell; } set { trell = value; } }
      
        public List<FilterTerm> Get(List<string> ColumnNames)
        {
            return this.AsEnumerable().Where(c => ColumnNames.Contains(c.FilterPylon.PylonName)).ToList();
        }
        public List<FilterTerm> Get(int stage)
        {
            FilterStage filterStage = (FilterStage)Enum.ToObject(typeof(FilterStage), stage);
            return this.AsEnumerable().Where(c => filterStage.Equals(c.Stage)).ToList();
        }
        public bool Have(string PylonName)
        {
            return this.AsEnumerable().Where(c => c.PylonName == PylonName).Any();
        }
        public FilterTerm[] GetTerms(string PylonName)
        {
            return this.AsEnumerable().Where(c => c.PylonName == PylonName).ToArray();
        }
        public FilterTerms Clone()
        {
            FilterTerms ft = new FilterTerms();
            foreach(FilterTerm t in this)
            {
                FilterTerm _t = new FilterTerm(t.PylonName, t.Operand, t.Value, t.Logic, t.Stage);
                ft.Add(_t);
            }            
            return ft;
        }

        #region INoid

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
            return Config.Path + "/" + Trell.TrellName + ".ftrm";
        }
        public ushort GetDriveId()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ICollection
        public new int Add(FilterTerm value)
        {
            value.Trell = Trell;
            value.Index = ((IList)this).Add(value);
            return value.Index;
        }        
        public void AddRange(ICollection<FilterTerm> terms)
        {
            foreach (FilterTerm term in terms)
            {
                term.Trell = Trell;
                term.Index = Add(term);
            }
        }
        public void AddNewRange(ICollection<FilterTerm> terms)
        {
            bool diffs = false;
            if (Count != terms.Count)
            {
                diffs = true;
            }
            else
            {             
                foreach (FilterTerm term in terms)
                {
                    if(Have(term.PylonName))
                    {
                        int same = 0;
                        foreach (FilterTerm myterm in GetTerms(term.PylonName))
                        {
                            if (!myterm.Compare(term))
                                same++;
                        }
                        if(same != 0)
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

            if(diffs)
            {
                Clear();
                foreach (FilterTerm term in terms)
                    Add(term);
            }
        }
        public object AddNew()
        {
            return (object)((IBindingList)this).AddNew();
        }          
        public void RemoveRange(ICollection<FilterTerm> value)
        {
            foreach (FilterTerm term in value)            
                Remove(term);
        }
        public void SetRange(FilterTerm[] data)
        {
            for (int i = 0; i < data.Length; i++)
                this[i] = data[i];
        }     
        public int IndexOf(object value)
        {
            for (int i = 0; i < Count; i++)
                if (ReferenceEquals(this[i], value))    // Found it
                    return i;
            return -1;
        }
        public FilterTerm Find(FilterTerm data)
        {
            foreach (FilterTerm lDetailValue in this)
                if (lDetailValue == data)    // Found it
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

        protected override void SetItem(int index, FilterTerm value)
        {
            FilterTerm oldItem = base[index];
            FilterTerm newItem = value;
            oldItem = null;

            newItem.Trell = Trell;
            if (newItem.Trell != null && !newItem.Trell.State.Quered)
            {
                newItem.Trell.State.Quered = true;
                foreach (DataRelay ch in newItem.Trell.ChildRelays)
                {
                    ch.Child.Trell.State.Quered = true;
                }
            }

            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
            base.SetItem(index, value);
        }
        protected override void InsertItem(int index, FilterTerm value)
        {
            FilterTerm c = (FilterTerm)value;
            c.FilterPylon = null;
            c.Trell = Trell;
            if (c.Trell != null && !c.Trell.State.Quered)
            {
                c.Trell.State.Quered = true;
                foreach (DataRelay ch in c.Trell.Relays)
                {
                    ch.Child.Trell.State.Quered = true;
                }
            }
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
            base.InsertItem(index, value);
        }
        protected override void RemoveItem(int index)
        {
            FilterTerm c = base[index];
            if (c.Trell != null)
                c.Trell.Quered = true;
            c.Trell = null;
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
            base.RemoveItem(index);
        }
        protected override void ClearItems()
        {
            bool first = true;
            foreach (FilterTerm c in Items)
            {
                if (first)
                {
                    if (c.Trell != null)
                        c.Trell.State.Quered = true;
                    first = false;
                }
            }
            OnListChanged(resetEvent);
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
            FilterTerm c = new FilterTerm(Trell);
            c.State.Added = true;
            c.Index = Add(c);
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
                return CreateDescriptors(trell.TrellName + " Filter Terms", Config.DataIdx);
            }
            set
            {
                CreateDescriptors(trell.TrellName + " Filter Terms", Config.DataIdx, value);
            }
        }

        private PropertyDescriptorCollection createDescriptors()
        {
            PropertyDescriptorCollection origProperties = TypeDescriptor.GetProperties(typeof(FilterTerm));

            ArrayList properties = new ArrayList();
            HashSet<string> handledNames = new HashSet<string>();

            List<string> propnames = new List<string>() { "Index", "PylonName", "Operand", "Value", "Logic", "Stage",  };
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
                listName = "Filter";//trell.TrellName + " Filter Terms":
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
                return this.AsEnumerable().ToArray();
        }
        public object GetHeader()
        {
            return this;
        }

        public byte[] GetShah()
        {
            return (Config.Place != null) ? Config.Place.GetShah() : null;
        }

        public int SerialCount { get; set; }
        public int DeserialCount { get; set; }
        public int ProgressCount { get; set; }
        public int ItemsCount { get { return Count; } }

        #endregion

        #region IDriveRecorder      
        public IDrive Drive
        { get { return drive; } set { drive = value; } }
        public void WriteDrive()
        {
            DriveContext dc = new DriveContext();
            Serialize(dc, 0, 0);
            dc.WriteDrive(Drive);
            dc.Dispose();
        }
        public void ReadDrive()
        {
            DriveContext dc = new DriveContext();
            object read = dc.ReadDrive(Drive);
            FilterTerms terms = (FilterTerms)Deserialize(ref read);
            this.AddRange(terms.AsEnumerable().ToArray());
            dc.Dispose();
        }
        public void OpenDrive()
        {
            if(Drive == null)
                Drive = new DriveBank(Config.File, Config.File, 1024 * 1024, typeof(FilterTerms));
        }
        public void CloseDrive()
        {
            if (Drive != null)
                Drive.Close();
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
    }

    [Serializable]
    public class FilterTerm : ICloneable
    {
        public string valueTypeName;
        [NonSerialized] private Type valueType;
        [NonSerialized] private DataTrellis trell;
        public DataTrellis Trell
        {
            get
            {
                return trell;
            }
            set
            {
                trell = value;
                if (FilterPylon == null && value != null)
                {
                    DataPylon[] filterPylon = trell.Pylons.AsEnumerable()
                             .Where(c => c.PylonName == PylonName).ToArray();
                    if (filterPylon.Length > 0)
                    {
                        FilterPylon = filterPylon[0];
                        ValueType = FilterPylon.DataType;
                    }
                }
            }
        }
        public DataPylon FilterPylon
        { get; set; }
       
        public DataState State
        { get; set; } = new DataState();

        public FilterTerm()
        {
            Stage = FilterStage.First;
        }
        public FilterTerm(DataTrellis nTrell)
        {
            Stage = FilterStage.First ;
            trell = nTrell;
        }
        public FilterTerm(string filterColumn, string operand, object value, string logic = "And", int stage = 1)
        {
            PylonName = filterColumn;
            OperandType tempOperand1;
            Enum.TryParse(operand, true, out tempOperand1);
            Operand = tempOperand1;
            Value = value;
            LogicType tempLogic;
            Enum.TryParse(logic, true, out tempLogic);
            Logic = tempLogic;          
            Stage = (FilterStage)Enum.ToObject(typeof(FilterStage), stage);

        }
        public FilterTerm(string filterColumn, OperandType operand, object value, LogicType logic = LogicType.And, FilterStage stage = FilterStage.First)
        {
            PylonName = filterColumn;
            Operand = operand;
            Value = value;
            Logic = logic;
            Stage = stage;

        }
        public FilterTerm(DataTrellis nTrell, string filterColumn, string operand, object value, string logic = "And", int stage = 1)
        {
            PylonName = filterColumn;
            OperandType tempOperand1;
            Enum.TryParse(operand, true, out tempOperand1);
            Operand = tempOperand1;
            Value = value;
            LogicType tempLogic;
            Enum.TryParse(logic, true, out tempLogic);
            Logic = tempLogic;
            Trell = nTrell;
            if (nTrell != null)
            {
                DataPylon[] filterPylon = trell.Pylons.AsEnumerable().Where(c => c.PylonName == PylonName).ToArray();
                if (filterPylon.Length > 0)
                {
                    FilterPylon = filterPylon[0]; ValueType = FilterPylon.DataType;
                }
            }
            Stage = (FilterStage)Enum.ToObject(typeof(FilterStage), stage);
        }
        public FilterTerm(DataPylon filterColumn, OperandType operand, object value, LogicType logic = LogicType.And, FilterStage stage = FilterStage.First)
        {
            Operand = operand;
            Value = value;
            Logic = logic;
            ValueType = filterColumn.DataType;
            PylonName = filterColumn.PylonName;
            FilterPylon = filterColumn;           
            Stage = stage;
        }

        [DisplayName("Pos")]
        public int Index { get; set; }
        public string PylonName { get; set; }
        public Type ValueType
        {
            get
            {
                if (valueType == null && valueTypeName != null)
                    valueType = Type.GetType(valueTypeName);
                return valueType;
            }
            set
            {
                valueType = value;
                valueTypeName = value.FullName;
            }
        }    
        public OperandType Operand { get; set; }
        public object Value { get; set; }
        public LogicType Logic { get; set; }
        public FilterStage Stage { get; set; } = FilterStage.First;

        public string OperandString(OperandType _operand)
        {
            string operandString = "";
            switch (_operand)
            {
                case OperandType.Equal:
                    operandString = "=";
                    break;
                case OperandType.EqualOrMore:
                    operandString = ">=";
                    break;
                case OperandType.More:
                    operandString = ">";
                    break;
                case OperandType.EqualOrLess:
                    operandString = "<=";
                    break;
                case OperandType.Less:
                    operandString = "<";
                    break;
                case OperandType.Like:
                    operandString = "like";
                    break;
                case OperandType.NotLike:
                    operandString = "!like";
                    break;
                default:
                    operandString = "=";
                    break;
            }
            return operandString;
        }
        public OperandType OperandEnum(string operandString)
        {
            OperandType _operand = OperandType.None;
            switch (operandString)
            {
                case "=":
                    _operand = OperandType.Equal;
                    break;
                case ">=":
                    _operand = OperandType.EqualOrMore;
                    break;
                case ">":
                    _operand = OperandType.More;
                    break;
                case "<=":
                    _operand = OperandType.EqualOrLess;
                    break;
                case "<":
                    _operand = OperandType.Less;
                    break;
                case "like":
                    _operand = OperandType.Like;
                    break;
                case "!like":
                    _operand = OperandType.NotLike;
                    break;
                default:
                    _operand = OperandType.None;
                    break;
            }
            return _operand;
        }

        public bool Compare(FilterTerm term)
        {
            if (PylonName != term.PylonName)
                return false;
            if (!Value.Equals(term.Value))
                return false;
            if (!Operand.Equals(term.Operand))
                return false;
            if (!Stage.Equals(term.Stage))
                return false;
            if (!Logic.Equals(term.Logic))
                return false;

            return true;
        }

        public object Clone()
        {
            FilterTerm clone = (FilterTerm)this.MemberwiseClone();
            clone.FilterPylon = FilterPylon;
            return clone;
        }
        public FilterTerm Clone(object value)
        {
            FilterTerm clone = (FilterTerm)this.MemberwiseClone();
            clone.FilterPylon = FilterPylon;
            clone.Value = value;
            return clone;
        }

    }

    [Serializable]
    public enum OperandType
    {
        Equal,
        EqualOrMore,
        EqualOrLess,
        More,
        Less,
        Like,
        NotLike,
        Contains,
        None
    }

    [Serializable]
    public enum LogicType
    {
        And,
        Or
    }

    [Serializable]
    public enum FilterStage
    {
        None,
        First,
        Second,
        Third
    }

    


}
