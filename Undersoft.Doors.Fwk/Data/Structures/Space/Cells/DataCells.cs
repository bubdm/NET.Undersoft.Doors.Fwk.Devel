using System;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Doors;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Doors.Data
{
    [JsonIgnore]
    public class DataCells : CollectionBase, IBindingList, IListSource, IDataGridBinder
    {
        public DataTier Tier
        { get; set; }
        public Type Model
        {
            get
            {
                return Tier.Model;
            }
        }
        public bool IsPrime
        { get { return Tier.IsPrime; } }
        public bool IsSingleton
        { get { return Tier.IsSingleton; } }

        public IDataNative iN
        { get { return Tier.iN; } }
        public IDataTier iTier
        { get { return Tier; } }

        public DataTier AssignCubeCell(JoinCube[] joincube, int[] cubeindex)
        {
            int length = cubeindex.Length;
            int i = 1;
            while (true)
            {
                JoinCube jc = joincube[cubeindex[i]];
                if (length == ++i)
                {
                    if (jc != null)
                        return jc.SubTier;
                    
                    return Tier;
                }                

                if (jc.Child != null)
                    joincube = jc.Child;
                else
                    return Tier;
            }
        }     

        public DataCells(DataTier tier, int capacity)
        {
            Tier = tier;
            if (Tier.IsPrime || Tier.IsSingleton)
            {               
                InnerList.Capacity = capacity;
                if (!tier.IsCube)
                {
                    for (int i = 0; i < capacity; i++)
                        InnerList.Add(tier);
                }
                else
                {
                    InnerList.Capacity = capacity;
                    InnerList.AddRange(new DataTier[capacity]);
                    DataTier sub = tier.SubTier;
                    DataCube cube = Tier.Trell.Cube;
                    cube.CubeSinglePylonIds.Select(c => InnerList[c] = tier).ToArray();
                    cube.CubeRootPylonIds.Select(c => InnerList[c] = sub).ToArray();
                    DataPylon[] cpyls = cube.CubeJoinPylonIds;
                    List<Vessel<TierInherits>>[] _temparr = sub.GetChildMaps(tier.Trell.Cube.JoinCubeRelays
                                                    .Select(j => j.CubeRelayIndex).ToArray());

                    DataTier ctier = null;

                    foreach (DataPylon cpyl in cube.CubeJoinPylonIds)
                    {
                        int[] cindex = cpyl.CubeIndex;
                        List<Vessel<TierInherits>> temp = _temparr[cindex[1]];
                        if (temp != null && temp.Count > 0)
                        {
                            if (cindex.Length > 2)
                            {
                                for (int c = 2; c < cindex.Length; c++)
                                {
                                    temp = temp[0].Value[cpyl.InheritorId].GetChildMap(cindex[c]);
                                    if (temp == null)
                                    {
                                        ctier = tier;
                                        break;
                                    }
                                    else
                                    {
                                        ctier = temp[0].Value[cpyl.InheritorId];
                                    }
                                }
                                InnerList[cpyl.Ordinal] = ctier;
                            }
                            else
                            {
                                InnerList[cpyl.Ordinal] = temp[0].Value[cpyl.InheritorId];
                            }
                        }
                        else
                        {
                            InnerList[cpyl.Ordinal] = tier;
                        }
                    }

                }
            }
        }
        //public DataCells(DataTier tier, int capacity)
        //{
        //    Tier = tier;
        //    if (Tier.IsPrime || Tier.IsSingleton)
        //    {
        //        InnerList.Capacity = capacity;
        //        if (!tier.IsCube)
        //        {
        //            for (int i = 0; i < capacity; i++)
        //                InnerList.Add(tier);
        //        }
        //        else
        //        {
        //            DataTier[] arr = new DataTier[capacity];
        //            InnerList.AddRange(arr);
        //            DataTier sub = tier.SubTier;
        //            DataCube cube = Tier.Trell.Cube;
        //            cube.CubeSinglePylonIds.Select(c => InnerList[c] = tier).ToArray();
        //            cube.CubeRootPylonIds.Select(c => InnerList[c] = sub).ToArray();
        //            cube.CubeJoinPylonIds.Select(c => InnerList[c] = AssignCubeCell(tier.GetCubeList(sub.Index).Child, tier.Pylons[c].CubeIndex)).ToArray();
        //        }
        //    }
        //}
        public DataCells(DataTier tier, ref object n, int capacity)
        {
            Tier = tier;
            if (tier.IsPrime)
            {
                InnerList.Capacity = capacity;
                if (!tier.IsCube)
                {
                    for (int i = 0; i < capacity; i++)
                        InnerList.Add(tier);

                    if (n != null)
                        if (Model == n.GetType())
                        {
                            Tier.n = n;
                        }
                        else
                        {
                            Tier.n = NType.New(Model);
                            iN.PrimeArray = ((IDataNative)n).PrimeArray;
                        }
                }
                n = null;
            }
        }
        public DataCells(DataTier tier, DataTier subTier, int capacity)
        {
            Tier = tier;
            if (tier.IsPrime || tier.IsSingleton)
            {
                InnerList.Capacity = capacity;
                if (tier.IsCube)
                {
                    InnerList.Capacity = capacity;
                    InnerList.AddRange(new DataTier[capacity]);
                    DataTier sub = subTier;
                    DataCube cube = Tier.Trell.Cube;
                    cube.CubeSinglePylonIds.Select(c => InnerList[c] = tier).ToArray();
                    cube.CubeRootPylonIds.Select(c => InnerList[c] = sub).ToArray();                    
                    DataPylon[] cpyls = cube.CubeJoinPylonIds;
                    List<Vessel<TierInherits>>[] _temparr = sub.GetChildMaps(tier.Trell.Cube.JoinCubeRelays
                                                        .Select(j => j.CubeRelayIndex).ToArray());

                    DataTier ctier = null;

                    foreach (DataPylon cpyl in cube.CubeJoinPylonIds)
                    {
                        int[] cindex = cpyl.CubeIndex;
                        List<Vessel<TierInherits>> temp = _temparr[cindex[1]];
                        if (temp != null && temp.Count > 0)
                        {
                            if (cindex.Length > 2)
                            {
                                for (int c = 2; c < cindex.Length; c++)
                                {
                                    temp = temp[0].Value[cpyl.InheritorId].GetChildMap(cindex[c]);
                                    if (temp == null)
                                    {
                                        ctier = tier;
                                        break;
                                    }
                                    else
                                    {
                                        ctier = temp[0].Value[cpyl.InheritorId];
                                    }
                                }
                                InnerList[cpyl.Ordinal] = ctier;
                            }
                            else
                            {
                                InnerList[cpyl.Ordinal] = temp[0].Value[cpyl.InheritorId];
                            }
                        }
                        else
                        {
                            InnerList[cpyl.Ordinal] = tier;
                        }
                    }
                }

            }
        }
        //public DataCells(DataTier tier, DataTier subTier, int capacity)
        //{
        //    Tier = tier;
        //    if (tier.IsPrime || tier.IsSingleton)
        //    {
        //        InnerList.Capacity = capacity;
        //        DataPylons cpyls = tier.Trell.Pylons;
        //        if (tier.IsCube)
        //        {
        //            DataTier[] arr = new DataTier[capacity];
        //            InnerList.AddRange(arr);
        //            DataTier sub = subTier;
        //            DataCube cube = Tier.Trell.Cube;
        //            cube.CubeSinglePylonIds.Select(c => InnerList[c] = tier).ToArray();
        //            cube.CubeRootPylonIds.Select(c => InnerList[c] = sub).ToArray();
        //            cube.CubeJoinPylonIds.Select(c => InnerList[c] = AssignCubeCell(tier.GetCubeList(sub.Index).Child, tier.Pylons[c].CubeIndex)).ToArray();
        //        }

        //    }
        //}

        //public IDataCells this[int index]
        //{
        //    get
        //    {
        //        return (IDataCells)List[index];
        //    }
        //    set
        //    {
        //        if (!value.IsCube)
        //        {
        //            bool single = value.IsSingleton;
        //            bool prime = value.IsPrime;
        //            if (!prime && !single)
        //            {
        //                if (value.iTier.Grating.Add(index, value.Write))
        //                {
        //                    value.Replicate(index);
        //                    List[index] = value;
        //                }
        //            }
        //            else
        //            {
        //                if (single)
        //                    iN[index] = value.Write;
        //                else if (!iN[index].IsSame(value.Write))
        //                {
        //                    iN[index] = value.Write;
        //                    value.Replicate(index);
        //                    value.iTier.Synced = false;
        //                    List[index] = value;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            IDataCells tr = (IDataCells)InnerList[index];
        //            if (!tr.IsCube)
        //            {
        //                if (value.iTier.Grating.Add(tr, Tier.Trell.Pylons[index].CubeOrdinal[0], index, value.Write))
        //                {
        //                    value.Replicate(index);
        //                    value.iTier.Synced = false;
        //                    List[index] = tr;

        //                }
        //            }
        //            else
        //            {
        //                if (value.iTier.Grating.Add((DataTier)value, index, index, value.Write))
        //                {
        //                    value.Replicate(index);
        //                    value.iTier.Synced = false;
        //                    List[index] = tr;
        //                }
        //            }
        //        }
        //    }
        //}
        //public IDataCells this[string name]
        //{
        //    get
        //    {
        //        int index = -1;
        //        Tier.Trell.PylonId.TryGetValue(name, out index);
        //        if (index < 0)
        //            Tier.Trell.DisplayId.TryGetValue(name, out index);
        //        if (index >= 0)
        //        {
        //            return ((IDataCells)List[index]);
        //        }
        //        return null;
        //    }
        //    set
        //    {
        //        int index = -1;
        //        Tier.Trell.PylonId.TryGetValue(name, out index);
        //        if (index < 0)
        //            Tier.Trell.DisplayId.TryGetValue(name, out index);
        //        if (index > -1)
        //        {
        //            if (!value.IsCube)
        //            {
        //                bool single = value.IsSingleton;
        //                bool prime = value.IsPrime;
        //                if (!prime && !single)
        //                {
        //                    if (value.iTier.Grating.Add(index, value.Write))
        //                    {
        //                        value.Replicate(index);
        //                        List[index] = value;
        //                    }
        //                }
        //                else
        //                {
        //                    if (single)
        //                        iN[index] = value.Write;
        //                    else if (!iN[index].IsSame(value.Write))
        //                    {
        //                        iN[index] = value.Write;
        //                        value.Replicate(index);
        //                        value.iTier.Synced = false;
        //                        List[index] = value;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                IDataCells tr = (IDataCells)InnerList[index];
        //                if (!tr.IsCube)
        //                {
        //                    if (value.iTier.Grating.Add(tr, Tier.Trell.Pylons[index].CubeOrdinal[0], index, value.Write))
        //                    {
        //                        value.Replicate(index);
        //                        value.iTier.Synced = false;
        //                        List[index] = tr;

        //                    }
        //                }
        //                else
        //                {
        //                    if (value.iTier.Grating.Add((DataTier)value, index, index, value.Write))
        //                    {
        //                        value.Replicate(index);
        //                        value.iTier.Synced = false;
        //                        List[index] = tr;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}     

        public IDataCells this[int index]
        {
            get
            {
                return (IDataCells)InnerList[index];
            }
            set
            {
                if (!value.IsCube)
                {
                    bool single = value.IsSingleton;
                    bool prime = value.IsPrime;
                    if (!prime && !single)
                    {
                        if (value.iTier.Grating.Add(index, value.Write))
                        {
                            value.Replicate(index);
                            List[index] = value;
                        }
                    }
                    else
                    {
                        if (single)
                            iN[index] = value.Write;
                        else if (!iN[index].IsSame(value.Write))
                        {
                            iN[index] = value.Write;
                            value.Replicate(index);
                            value.iTier.Synced = false;
                            List[index] = value;
                        }
                    }
                }
                else
                {
                    IDataCells tr = (IDataCells)InnerList[index];
                    if (!tr.IsCube)
                    {
                        if (!(value.iTier.Grating.Add(tr, Tier.Pylons[index].CubeOrdinal, index, value.Write)))
                            return;
                    }
                    else
                    {
                        if (!(value.iTier.Grating.Add(value, index, index, value.Write)))
                            return;
                    }

                    value.Replicate(index);
                    value.iTier.Synced = false;
                    List[index] = tr;
                }
            }
        }
        public IDataCells this[string name]
        {
            get
            {
                DataPylon pyl = Tier.Pylons[name];
                if (pyl != null)
                    return ((IDataCells)InnerList[pyl.Ordinal]);                
                return null;
            }
            set
            {
                DataPylon pyl = Tier.Pylons[name];
                if (pyl != null)
                {
                    this[pyl.Ordinal] = value;                   
                }
            }
        }
        public IDataCells this[PropertyDescriptor property]
        {
            get
            {
                if (property != null)
                {
                    PropertyDescriptorCollection pdc = LineProperties();
                    int index = pdc.IndexOf(property);
                    if (index < 0)
                        index = LineProperties().IndexOf(pdc.Find(property.Name, true));
                    return ((IDataCells)List[index]);
                }
                return null;
            }
            set
            {
                if (property != null)
                {
                    int index = LineProperties().IndexOf(property);
                    if (index > -1)
                    {
                        this[index] = value;
                    }
                }
            }
        }

        public IDataCells this[int index, object obj]
        {
            get
            {
                return (IDataCells)InnerList[index];
            }
            set
            {
                if (!value.IsCube)
                {
                    bool single = value.IsSingleton;
                    bool prime = value.IsPrime;
                    if (!prime && !single)
                    {
                        if (value.iTier.Grating.Add(index, obj))
                        {
                            value.Replicate(index);
                            List[index] = value;
                        }
                    }
                    else
                    {
                        if (single)
                        {
                            iN[index] = obj;
                            return;
                        }
                        if (!iN[index].IsSame(obj))
                        {
                            iN[index] = obj;
                            value.Replicate(index);
                            value.iTier.Synced = false;
                            List[index] = value;
                            return;
                        }
                    }
                }
                else
                {
                    IDataCells tr = (IDataCells)InnerList[index];
                    if (!tr.IsCube)
                    {
                        if (!(value.iTier.Grating.Add(tr, Tier.Pylons[index].CubeOrdinal, index, obj)))
                            return;
                    }
                    else
                    {
                        if (!(value.iTier.Grating.Add(value, index, index, obj)))
                            return;
                    }

                    value.Replicate(index);
                    value.iTier.Synced = false;
                    List[index] = tr;
                }
            }
        }
        public IDataCells this[string name, object obj]
        {
            get
            {
                DataPylon pyl = Tier.Pylons[name];
                if (pyl != null)
                    return ((IDataCells)InnerList[pyl.Ordinal]);
                return null;
            }
            set
            {
                DataPylon pyl = Tier.Pylons[name];
                if (pyl != null)
                {
                    this[pyl.Ordinal, obj] = value;
                    
                }
            }
        }

        public IDataCells[] AsArray()
        {
            return InnerList.Cast<IDataCells>().ToArray();
        }

        public byte[] GetShah(int id)
        {
            return iN[id].GetShah();
        }

        public void ClearBlames()
        {
            if (!Tier.IsCube)
                SetRange(this.AsEnumerable().Select(p => Tier.Prime).ToArray());
        }
        public void ClearBlame(int id)
        {
            InnerList[id] = Tier.Prime;
        }

        public PropertyDescriptorCollection LineProperties()
        {
            return TypeDescriptor.GetProperties(Tier.Model);
        }

        #region ICollection

        public void Set(int index, IDataCells value)
        {
            if (!value.IsCube)
            {
                bool single = value.IsSingleton;
                bool prime = value.IsPrime;
                if (!prime && !single)
                {
                    if (value.iTier.Grating.Add(index, value.Write))
                    {
                        value.Replicate(index);
                        List[index] = value;
                    }
                }
                else
                {
                    if (single)
                        iN[index] = value.Write;
                    else if (!iN[index].IsSame(value.Write))
                    {
                        iN[index] = value.Write;
                        value.Replicate(index);
                        value.iTier.Synced = false;
                        List[index] = value;
                    }
                }
            }
            else
            {
                IDataCells tr = (IDataCells)InnerList[index];
                if (!tr.IsCube)
                {
                    if (value.iTier.Grating.Add(tr, Tier.Pylons[index].CubeOrdinal, index, value.Write))
                    {
                        value.Replicate(index);
                        value.iTier.Synced = false;
                        List[index] = tr;

                    }
                }
                else
                {
                    if (value.iTier.Grating.Add((DataTier)value, index, index, value.Write))
                    {
                        value.Replicate(index);
                        value.iTier.Synced = false;
                        List[index] = tr;
                    }
                }
            }
        }

        public bool SetInner(int index, IDataCells value, object obj)
        {
            if (!value.IsCube)
            {
                bool single = value.IsSingleton;
                bool prime = value.IsPrime;
                if (!prime && !single)
                {
                    if (value.iTier.Grating.Add(index, obj))
                    {
                        value.Replicate(index);
                        return true;
                    }
                }
                else
                {
                    if (single)
                        iN[index] = value.Write;
                    else if (!iN[index].IsSame(obj))
                    {
                        iN[index] = obj;
                        value.Replicate(index);
                        value.iTier.Synced = false;
                        return true;
                    }
                }
            }
            else
            {
                IDataCells tr = (IDataCells)InnerList[index];
                if (!tr.IsCube)
                {
                    if (value.iTier.Grating.Add(tr, Tier.Pylons[index].CubeOrdinal, index, obj))
                    {
                        value.Replicate(index);
                        value.iTier.Synced = false;
                        return true;
                    }
                }
                else
                {
                    if (value.iTier.Grating.Add(value, index, index, obj))
                    {
                        value.Replicate(index);
                        value.iTier.Synced = false;
                        return true;
                    }
                }
            }
            return false;
        }
        public bool SetInner(string name, IDataCells value, object obj)
        {
            DataPylon pyl = Tier.Trell.Pylons[name];
            if (pyl != null)
            {
                SetInner(pyl.Ordinal, value, obj);
                return true;
            }
            return false;
        }
        public bool SetInner(PropertyDescriptor property, IDataCells value, object obj)
        {
            if (property != null)
            {
                int index = LineProperties().IndexOf(property);
                if (index > -1)
                    return SetInner(index, value, obj);
            }
            return false;
        }

        public int Add(IDataCells data)
        {
            return List.Add(data);
        }

        public void AddRange(IDataCells[] data)
        {
            InnerList.AddRange(data);
        }
        public void SetRange(IDataCells[] data)
        {
            InnerList.SetRange(0, data);
        }

        public int IndexOf(object value)
        {
            for (short i = 0; i < this.List.Count; i++)
                if (this[i][i].Equals(value))    // Found it
                    return i;
            return -1;
        }
        public int IndexOf(IDataCells value)
        {
            for (int i = 0; i < this.List.Count; i++)
            {
                if (this[i][i].Equals(value.Write))    // Found it
                    return i;
            }
            return -1;
        }

        public int Replace(int index, IDataCells data)
        {
            this.List[index] = data;
            return index;
        }

        public void Insert(int index, IDataCells data)
        {
            this.List.Insert(index, data);
        }
        public void Remove(IDataCells data)
        {
            this.List.Remove(data);
        }

        public object Find(object data)
        {
            for (int i = 0; i < this.Count; i++)
                if (this[i][i].Equals(data))    // Found it                
                    return data;
            return null;    // Not found
        }
        public IDataCells Find(IDataCells data)
        {
            for (int i = 0; i < this.Count; i++)
            {
                IDataCells _data = this[i];
                if (_data[i].Equals(data.Write))    // Found it
                {
                    _data.Write = data.Write;
                    return _data;
                }
            }
            return null;    // Not found
        }

        public bool Contains(object value)
        {
            return (IndexOf(value) >= 0);
        }
        public bool Contains(IDataCells data)
        {
            return (IndexOf(data) >= 0);
        }

        public bool HasCell(string CellName)
        {
            return Tier.Trell.Pylons.Have(CellName);
            //return (LineProperties().Find(CellName, true) != null) ? true : false;
        }
        public bool IsEmpty(string CellName)
        {
            return (this[CellName] != null && this[CellName].ToString() != string.Empty) ? false : true;
        }
        public bool IsEmpty(int CellId)
        {
            return (this[CellId] != null && this[CellId].ToString() != string.Empty) ? false : true;
        }

        public IEnumerable<IDataCells> AsEnumerable()
        {
            return InnerList.Cast<IDataCells>();
        }

        #endregion

        #region IBindingList

        public object BindingSource
        { get { return this.Tier.Trell; } }
        public IDepotSync DepotSync
        { get { return BoundedGrid.DepotSync; } set { BoundedGrid.DepotSync = value; } }
        public IDataGridStyle GridStyle
        { get; set; }
        public IDataGridBinder BoundedGrid
        { get; set; }

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

        protected override void OnInsertComplete(int index, object value)
        {
            // OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
        }

        protected override void OnRemoveComplete(int index, object value)
        {

            // OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
        }

        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            // OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
        }

        protected override void OnClearComplete()
        {
            // OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
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

        private ListChangedEventHandler onListChanged;

        object IBindingList.AddNew()
        {
            IDataCells c = (IDataCells)Tier;
            List.Add(c);
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

        #endregion

        public unsafe byte[] DynamicStructureToPtrBlk(object o)
        {
            //structure ptr
            int size = Marshal.SizeOf(o);
            byte[] barray = new byte[size];
            fixed (byte* b = &barray[0])
            {
                Marshal.StructureToPtr(o, new IntPtr(b), false);
            }
            return barray;
        }

        public unsafe void PtrToDynamicStructureBlk(byte[] pserial)
        {
            //structure ptr
            fixed (byte* b = &pserial[0])
            {
               Marshal.PtrToStructure(new IntPtr(b), this);
            }
        }
    }

}