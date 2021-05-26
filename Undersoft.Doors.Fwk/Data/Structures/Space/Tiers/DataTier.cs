using System.Globalization;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Doors.Drive;
using System.IO;
using System.Doors;
using System.Runtime.InteropServices;

namespace System.Doors.Data
{    
    [JsonObject]
    public class DataTier : IDataCells, IDataTier, INoid, IDataGridBinder, IDataNative, IDataConfig
    {
        #region Private NonSerialized
        [NonSerialized] public object n;
        [NonSerialized] private IDataGridBinder boundedGrid;
        [NonSerialized] private DataTrellis trell;
        [NonSerialized] private DataTrellis subtrell;
        [NonSerialized] private DataPylons pylons;
        public Type Model { get { return trell.Model; } }
        [NonSerialized] private DataCells data;
        [NonSerialized] private DataTier prime;
        [NonSerialized] private DataTier subtier;
        [NonSerialized] private DataTier[] cubetierset;
        [NonSerialized] private DataTier devisor;
        [NonSerialized] private DataTiers tiers;
        [NonSerialized] private int primeindex = -1;
        [NonSerialized] private DataTiers[] childtiers;
        [NonSerialized] private DataTiers[] parenttiers;
        [NonSerialized] private List<Vessel<TierInherits>>[] childtiermaps;
        [NonSerialized] private List<Vessel<TierInherits>>[] parenttiermaps;
        [NonSerialized] private bool parentMapIsSet;
        [NonSerialized] private bool childMapIsSet;
        //[NonSerialized] private DataTier[][] childtierlists;
        //[NonSerialized] private DataTier[][] parenttierlists;
        //[NonSerialized] private JoinCube joincube;
        //[NonSerialized] private Quin[] chdnoids;
        //[NonSerialized] private Quin[] prtnoids;
        [NonSerialized] private bool isSingleton = false;
        public DataConfig Config
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        #endregion

        public IDataTrellis iTrell
        { get { return trell; } }
        public DataTrellis Trell
        {
            get { return trell; }
            set { trell = value; }
        }
        public DataPylons Pylons
        {
            get { return pylons; }
            set { pylons = value; }
        }
        public DataTrellis SubTrell
        {
            get { return subtrell; }
            set { subtrell = value; }
        }
        public DataTiers Tiers
        {
            get { return tiers; }
            set { tiers = value; }
        }
        public DataTier Prime
        {
            get { return prime; }
            set { prime = value; }
        }
        public DataTier Devisor
        {
            get { return devisor; }
            set { devisor = value; }
        }
        public DataTier SubTier
        {
            get { return subtier; }
            set { subtier = value; }
        }
        public DataTier[] CubeTierSet
        {
            get { return cubetierset; }
            set { cubetierset = value; }
        }

        public DataState State
        { get; set; }
      
        public IDataNative  iN
        { get { return (IDataNative)prime.n; } }
        public IDataTier    iTier
        { get { return this; } }
        public IDataGrating Grating
        { get; set; }
        //public IDataChanges Changes
        //{ get; set; }

        public object Write
        { get; set; }

        public ushort DriveId
        {
            get { return GetDriveId(); }
        }
        public ushort SectorId
        {
            get { return GetSectorId(); }
        }
        public ushort LineId
        {
            get { return GetLineId(); }
        }

        public bool IsPrime
        { get { return trell.IsPrime; } }
        public bool IsDevisor
        { get { return trell.IsDevisor; } }
        public bool IsSingleton
        { get { return isSingleton; } set { isSingleton = value; } }
        public bool IsEditable(int id)
        {
            return Pylons[id].Editable;
        }
        public bool IsEditable(string name)
        {
            return this.trell.Pylons[name].Editable;
        }

        public int Index
        { get; set; } = -1;
        public int ViewIndex
        { get { return (tiers.Mode != DataModes.Tiers && tiers.Mode != DataModes.TiersView) ? trell.Sims.TiersView.IndexOf(this) : trell.Tiers.TiersView.IndexOf(this); } set { } }
        public int PageIndex
        { get; set; } = -1;
        public int Page
        { get; set; } = -1;
        public string NoId
        {
            get
            {
                return ((Noid)iN[NoidOrdinal]).ToString();
            }
            set
            {
                iN[NoidOrdinal] = new Noid(value);
            }
        }

        public bool Checked
        { get { return State.Checked; } set { State.Checked = value; } }
        public bool Edited
        { get { return State.Edited; } set { State.Edited = value; } }
        public bool Deleted
        {
            get
            {
                return State.Deleted;
            }
            set
            {
                if (State.Deleted != value)
                {
                    State.Deleted = value;
                    if (value)
                        Trell.Tiers.AddToDeleteSet(Index);
                    else
                        Trell.Tiers.RemoveFromDeleteSet(Index);
                }
            }
        }
        public bool Added
        { get { return State.Added; } set { State.Added = value; } }
        public bool Synced
        { get { return State.Synced; } set { State.Synced = value; } }
        public bool Saved
        { get { return State.Saved; } set { State.Saved = value; } }
        public bool IsCube
        { get; set; }

        public int PrimeIndex
        { get { return (IsPrime || IsSingleton) ? primeindex : Prime.PrimeIndex; } set { if (IsPrime || IsSingleton) primeindex = value; } }
        public int DevIndex
        { get { return devisor.Index; } }

        public int NoidOrdinal
        { get { return (!IsCube) ? trell.NoidOrdinal : subtrell.NoidOrdinal; } }
        public int QuidOrdinal
        { get { return (!IsCube) ? trell.QuidOrdinal : subtrell.QuidOrdinal; } }
        public int[] QuidPylons
        { get { return (!IsCube) ? trell.QuidPylons : subtrell.QuidPylons; } }

        public int[] KeyId
        {
            get { return Trell.KeyId; }
        }

        public DataTier()
        {
            Grating = new EditGrating(this);
            State = new DataState();
        }
        public DataTier(DataTrellis _trell)
        {
            Trell = _trell;
            Tiers = Trell.Tiers;
            Pylons = Trell.Pylons;
            State = new DataState(Tiers);
            Grating = new EditGrating(this);

            n = NType.New(Model);

            if (IsPrime)
            {
                prime = this;
                devisor = this;
                data = new DataCells(this, _trell.Pylons.Count);
            }
            else
            {
                IsSingleton = true;
                prime = this;
                data = new DataCells(this, _trell.Pylons.Count);
            }
            SetDefaults();
        }
        public DataTier(DataTrellis _trell, ref object n)
        {
            Trell = _trell;
            Pylons = Trell.Pylons;
            Tiers = Trell.Tiers;
            State = new DataState(Tiers);
            Grating = new EditGrating(this);
            if (IsPrime)
            {
                prime = this;
                devisor = this;
                if (!IsCube)
                    data = new DataCells(this, ref n, _trell.Pylons.Count);
            }
            else
            {
                DataTier ptier = Trell.Devisor.Tiers.Put(ref n, false, true);
                data = ptier.Cells;
                prime = ptier.Prime;
                devisor = ptier;
            }
        }
        public DataTier(DataTrellis _trell, DataTrellis _topsubtrell, DataTier _cube)
        {
            Trell = _trell;
            Pylons = Trell.Pylons;
            Tiers = Trell.Tiers;          
            IsCube = true;
            State = new DataState(tiers);
            Grating = new EditGrating(this);
            SubTrell = _topsubtrell;
            SubTier = _cube;
            if (IsPrime)
            {
                if (_cube.IsPrime)
                    prime = _cube;
                else
                    prime = _cube.Prime;
                devisor = this;
                data = new DataCells(this, _cube, _trell.Pylons.Count);
            }
            else
            {
                if (_cube.IsPrime)
                    prime = _cube;
                else
                    prime = _cube.Prime;
                devisor = this;
                IsSingleton = true;
                data = new DataCells(this, _cube, _trell.Pylons.Count);
            }
          
        }
        public DataTier(DataTrellis _trell, DataTier _prime, DataModes _mode = DataModes.Tiers)
        {
            Trell = _trell;
            Pylons = Trell.Pylons;
            IsCube = _prime.IsCube;
            switch (_mode)
            {
                case DataModes.Tiers:
                    Grating = new EditGrating(this);                   
                    Tiers = _trell.Tiers;
                    break;
                case DataModes.Sims:
                    Grating = new SimGrating(_prime, _prime.Grating.changes);                    
                    Index = _prime.Index;
                    break;
            }
            State = new DataState(_trell.Tiers);
            if (IsCube)
            {
                SubTrell = _prime.SubTrell;
                SubTier = _prime.SubTier;
                data = _prime.Cells;
                prime = _prime.Prime;
                devisor = _prime;
            }
            else
            {
                data = _prime.Cells;
                prime = _prime.Prime;
                devisor = _prime;
            }
        }
        public DataTier(DataTier _devisor, Dictionary<long, object> _changer)
        {
            Trell = _devisor.Trell;
            Pylons = Trell.Pylons;
            Tiers = devisor.Tiers;
            prime = _devisor.Prime;
            devisor = _devisor.Devisor;
            State = _devisor.State;
            Grating = _devisor.Grating;
            Grating.PutRange(_changer);
            data = _devisor.Cells;
            childtiers = _devisor.childtiers;
            parenttiers = _devisor.parenttiers;
            Index = _devisor.Index;
            PrimeIndex = _devisor.PrimeIndex;
            Drive = _devisor.Drive;
        }
        public DataTier(DataTier _devisor, bool newState = true)
        {
            Trell = _devisor.Trell;
            Pylons = Trell.Pylons;
            Tiers = _devisor.Tiers;
            data = _devisor.Cells;
            prime = _devisor.Prime;
            devisor = _devisor.Devisor;
            if (newState)
            {
                State = new DataState(this);
                State.Impact(_devisor.State);
            }
            else
                State = _devisor.State;
            Grating = _devisor.Grating;
            childtiers = _devisor.childtiers;
            parenttiers = _devisor.parenttiers;
            Index = _devisor.Index;
            PrimeIndex = _devisor.PrimeIndex;
            ViewIndex = _devisor.ViewIndex;
            Page = _devisor.Page;
            PageIndex = _devisor.PageIndex;
            Drive = _devisor.Drive;
        }

        public object N
        {
            get
            {
                return n;
            }
            set
            {
                n = value;
            }
        }

        public DataCells Cells
        {
            get
            {
                return data;
            }
        }

        public object this[int index]
        {
            get
            {
                if (!IsCube)
                {
                    if (!IsPrime && !IsSingleton)
                    {
                        object result = Grating.ChangeOn(index);
                        if (result == null)
                        {
                            if (Tiers.Mode != DataModes.Sims)
                                return iN[index];
                            else
                                return devisor[index];
                        }
                        return result;
                    }
                    else
                        return iN[index];
                }
                else
                {
                    IDataCells datacell = data[index];
                    object result = Grating.ChangeOn(index);
                    if (result == null)
                    {
                        if (!datacell.IsCube)
                            return datacell[Pylons[index].CubeOrdinal];
                        else
                            return Pylons[index].Default;
                    }
                    return result;
                }
            }
            set
            {
                Cells[index, value] = this;
            }
        }
        public object this[string cellName]
        {
            get
            {
                DataPylon pyl = Pylons[cellName];
                if (pyl == null)
                    return null;
                return this[pyl.Ordinal];
            }
            set
            {
                Cells[cellName, value] = this;
            }
        }
       
        public object this[PropertyDescriptor cellProp]
        {
            get
            {
                return this[Cells.LineProperties().IndexOf(cellProp)];
                //if (index > -1)
                //    if (!IsCube)
                //    {
                //        if (!IsPrime && !IsSingleton)
                //            if (Tiers.Mode != DataModes.Sims)
                //                return Grating.ChangeOn(iN[index], index);
                //            else
                //                return Grating.ChangeOn(devisor[index], index);
                //        else
                //            return iN[index];
                //    }
                //    else
                //    {
                //        DataPylon cpyl = trell.Pylons[index];
                //        IDataCells datacell = data[index];
                //        if (!datacell.IsCube)
                //            return Grating.ChangeOn(datacell[cpyl.CubeOrdinal[0]], index);
                //        else
                //            return Grating.ChangeOn(cpyl.Default, index);
                //    }
                //else
                //    return null;
            }
            set
            {
                Write = value;
                Cells[cellProp] = this;
            }
        }           
     
        public object[] DataArray
        {
            get
            {
                return Cells.AsEnumerable().Select((x, y) => this[y]).ToArray();
            }
            set
            {
                value.Select((x, y) => this[y] = x).ToArray();
            }
        }
        public object[] PrimeArray
        {
            get
            {
                if (!IsCube)
                    return iN.PrimeArray;
                else
                    return Cells.AsEnumerable().Select((x, y) => this[y]).ToArray();
            }
            set
            {
                if (!IsCube)
                    value.Select((x, y) => (x != DBNull.Value) ? iN[y] = x : null).ToArray();
                else
                {
                    value.Select((x, y) => Cells.SetInner(y, this, x)).ToArray();
                }
            }
        }

        //public object ByteArray
        //{
        //    get
        //    {
        //        return iN.ByteArray;
        //    }
        //    //set
        //    //{
        //    //    iN.ByteArray = value;
        //    //}
        //}

        public byte[] Serialize()
        {
            return iN.Serialize();
        }

        public Dictionary<string, object> Fields
        {
            get
            {
                 return Trell.Pylons.VisiblePylons.Select(x =>
                                new KeyValuePair<string, object>(x.Ordinal.ToString(),
                                     this[x.Ordinal])).ToDictionary(k => k.Key, v => v.Value);
            }
            set
            {
                Type[] types = Trell.Pylons.TypeArray;
                foreach (KeyValuePair<string, object> field in value)
                {
                    try
                    {
                        int Id = int.Parse(field.Key);
                        if (types[Id] == typeof(Quid))
                            this[Id] = new Quid((string)field.Value);
                        else
                            this[Id] = Convert.ChangeType(field.Value, types[Id], CultureInfo.InvariantCulture);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
        }
        public Dictionary<string, object> JsonFields
        {
            get
            {
                return Trell.Pylons.VisiblePylons.Select(x =>
                                new KeyValuePair<string, object>(x.PylonName,
                                     this[x.Ordinal])).ToDictionary(k => k.Key, v => v.Value);
            }
            set
            {
                DataPylons pls = Trell.Pylons;
                foreach (KeyValuePair<string, object> field in value)
                {
                    try
                    {
                        int Id = pls[field.Key].Ordinal;
                        if (pls.TypeArray[Id] == typeof(Quid))
                            this[Id] = new Quid((string)field.Value);
                        else
                            this[Id] = Convert.ChangeType(field.Value, pls.TypeArray[Id], CultureInfo.InvariantCulture);
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
        }

        public Dictionary<string, object> ChildJoins
        {
            get
            {
                DataTiers[] dataArray = ChildTiers;
                if (dataArray != null && dataArray.Length > 0)
                {
                    Dictionary<string, object> preJson = dataArray.Select(data => new KeyValuePair<string, object>(data.Trell.TrellName,
                        data.AsEnumerable().Select(c => c.ViewIndex)
                        .ToArray())).ToDictionary(k => k.Key, v => v.Value);

                    return preJson;
                }
                else
                    return null;
            }
            set
            {

            }
        }
        public Dictionary<string, object> ParentJoins
        {
            get
            {
                DataTiers[] dataArray = ParentTiers;
                if (dataArray != null && dataArray.Length > 0)
                {
                    Dictionary<string, object> preJson = dataArray.Select(data => new KeyValuePair<string, object>(data.Trell.TrellName,
                            data.AsEnumerable().Select(c => c.ViewIndex)
                            .ToArray())).ToDictionary(k => k.Key, v => v.Value);

                    return preJson;
                }
                else
                    return null;
            }
            set
            {

            }
        }

        public object[] Keys
        {
            get
            {
                if (!IsCube)
                    return Trell.KeyId.Select(x => iN[x]).ToArray();
                else
                    return SubTrell.KeyId.Select(x => iN[x]).ToArray();
            }
        }

        public DataRelays ChdRelays
        {
            get
            {
                return Trell.ChildRelays;
            }
        }
        public DataRelays PrtRelays
        {
            get
            {
                return Trell.ParentRelays;
            }
        }

        public long[] ChdKeys
        {
            get
            {
                return Trell.ChildRelays.Select(k => k.Parent.KeysOrdinal.Select(x => this[x]).ToArray().GetShahCode64()).ToArray();
            }
        }
        public long[] PrtKeys
        {
            get
            {
                return Trell.ParentRelays.Select(k => k.Child.KeysOrdinal.Select(x => this[x]).ToArray().GetShahCode64()).ToArray();
            }
        }

        public HashSet<long> RelKeys
        {
            get
            {
                return new HashSet<long>(ChdKeys.Concat(PrtKeys), new Hash64Comparer());
            }
        }

        public T Field<T>(string cellName)
        {            
            return (T)this[cellName];
        }
        public T Field<T>(int cellIndex)
        {
            return (T)this[cellIndex];
        }
        public T Field<T>(DataPylon column)
        {
            return (T)this[column.Ordinal];
        }

        public DataTier SaveChanges(bool toDrive = true)
        {
            if (Edited)
            {
                if (!IsCube)
                {
                    foreach(Vessel<object> obj in Grating)
                    {
                        int id = (int)obj.Key;
                        iN[id] = obj.Value;
                        Cells.ClearBlame(id);
                    }
                }
                else
                {
                    if (Grating.changes.Count > 0)
                    {
                        DataCells datacells = null;
                        if (tiers.Mode != DataModes.Sims && tiers.Mode != DataModes.SimsView)
                            datacells = data;
                        else
                            datacells = devisor.Cells;

                        DataTier ctier = datacells.Tier.SubTier;
                        DataTiers[] _temp = ctier.GetCubeChildMap(Trell.Cube.CubeRelays);

                        foreach (Vessel<object> obj in Grating)
                        {
                            int index = (int)(obj.Key);
                            DataPylon cpyl = trell.Pylons[index];
                            IDataCells datacell = datacells[index];
                            if (!ReferenceEquals((DataTier)datacell, datacells.Tier))
                            {
                               datacell.iN[cpyl.CubeOrdinal] = obj.Value; 
                            }
                            else
                            {
                                int cl = cpyl.CubeLevel;
                                if (cl > 0)
                                {
                                    int c = 0;
                                    DataTiers temp = _temp[cpyl.CubeIndex[1]];
                                    for (c = 0; c < cl; c++)
                                    {
                                        if (c > 0)
                                            temp = ctier.GetCubeChildMap(Trell.Cube.CubeRelays)[cpyl.CubeIndex[c + 1]];
                                        if (temp != null && temp.Count > 0)
                                        {
                                            ctier = temp[0];
                                        }
                                        else
                                        {
                                            ctier = temp.AddNew();
                                            temp.AutoKeys.Select(k => ctier.iN[k.Key] = k.Value).ToArray();
                                        }
                                    }
                                    int ci = cpyl.Pylons[index].CubeIndex[c];
                                    ctier.iN[cpyl.CubeOrdinal] = obj.Value;
                                    trell.Pylons.Where(p => p.CubeIndex != null &&
                                                        p.CubeIndex.Length == (c + 1) &&
                                                            p.CubeIndex[c] == ci)
                                                                .Select(o => datacells.Replace(o.Ordinal, ctier)).ToArray();
                                }
                            }

                        }
                    }
                }
            }

            if (toDrive)
                WriteDrive();            

            Grating.Clear();
            Added = false;
            Edited = false;
            Saved = false;
            return this;
        }
        public DataTier ClearChanges()
        {
            Grating.Clear();
            Edited = false;
            Saved = false;
            State.Quered = false;
            return this;
        }

        public void SetDefaults()
        {
            this.Trell.Pylons.DefultPylons.Select(p => iN[p.Ordinal] = p.GetDefault()).ToArray();
        }
        
        public void SetAutoIds()
        {
            if (Trell.HasAutoId)
                this.Trell.Prime.Pylons.AutoIdPylons.Select(p => ((int)iN[p.Ordinal] == 0) ? iN[p.Ordinal] = p.GetAutoIds() : null).ToArray();          
        }

        public IDataCells[] AsArray()
        {
            return new DataTier[] { this };
        }

        public IEnumerable<IDataCells> AsEnumerable()
        {
            return Cells.AsEnumerable();
        }

        public DataTier[] GetCubeTierArray()
        {
            if (IsCube)
            {
                DataCells datacells = null;
                if (tiers.Mode != DataModes.Sims && tiers.Mode != DataModes.SimsView)
                    datacells = data;
                else
                    datacells = devisor.Cells;
                int[] ords = Pylons.FirstCubePylons.Select(p => p.CubeOrdinal).ToArray();
                return datacells.AsEnumerable().Where((x, y) => ords.Contains(y)).Where(d => !ReferenceEquals((DataTier)d, datacells.Tier)).Cast<DataTier>().ToArray();
            }
            return null;
        }

        #region Searching Cells       
        public int IndexOf(object value)
        {
            return Cells.IndexOf(value);
        }
        public bool IsEmpty(int CellId)
        {
            return Cells.IsEmpty(CellId);
        }
        public bool IsEmpty(string CellName)
        {
            return Cells.IsEmpty(CellName);
        }
        public bool Contains(object value)
        {
            return Cells.Contains(value);
        }
        public bool HasCell(string CellName)
        {
            return Cells.HasCell(CellName);
        }
        public bool HasPylon(string PylonName)
        {
            return Trell.Pylons.Have(PylonName);
        }

        public object Find(object value)
        {
            return Cells.Find(value);
        }
        public object[] Collect(string trellName, RelaySite Site)
        {
            switch (Site)
            {
                case RelaySite.Child:
                    return GetChild(trellName).AsArray();
                case RelaySite.Parent:
                    return GetParent(trellName).AsArray();
                default:
                    return null;
            }
        }
        #endregion

        #region Relay Joins
        public void ClearRelay()
        {
            childtiers = null;
            parenttiers = null;
        }

        public bool AllowReplic
        { get; set; } = true;

        public bool AnyChild(string ChildTrellName)
        {
            return (GetChild(ChildTrellName) != null) ? true : false;
        }

        public void Replicate(int id)
        {
            if (AllowReplic)
            {
                DataPylon rwpylon = null;
                if (Trell.ReplicPylons.TryGetValue(id, out rwpylon))
                {
                    if (rwpylon.JoinIndex != null)
                    {
                        int l = rwpylon.JoinIndex.Length;
                        DataTiers trs = null;
                        int vid = -1;
                        switch (Tiers.Mode)
                        {
                            case DataModes.Tiers:
                                vid = Index;
                                trs = Trell.Tiers;
                                break;
                            case DataModes.TiersView:
                                vid = ViewIndex;
                                trs = Trell.TiersView;
                                break;
                            case DataModes.Sims:
                                vid = Index;
                                trs = Trell.Sims;
                                break;
                            case DataModes.SimsView:
                                vid = ViewIndex;
                                trs = Trell.SimsView;
                                break;
                        }

                        for (int i = 0; i < l; i++)
                        {
                            IList<DataTier> t = trs.GetChildList()[rwpylon.JoinIndex[i]][vid].Child;
                            if (t.Count > 0)
                                for (int x = 0; x < t.Count; x++)
                                    t[x][rwpylon.JoinOrdinal[i]] = Write;
                        }
                    }
                }
            }
        }
        public void Replicate(DataPylon rwpylon)
        {
            if (AllowReplic)
            {
                int l = rwpylon.JoinIndex.Length;
                DataTiers trs = null;
                int vid = -1;
                switch (Tiers.Mode)
                {
                    case DataModes.Tiers:
                        vid = Index;
                        trs = Trell.Tiers;
                        break;
                    case DataModes.TiersView:
                        vid = ViewIndex;
                        trs = Trell.TiersView;
                        break;
                    case DataModes.Sims:
                        vid = Index;
                        trs = Trell.Sims;
                        break;
                    case DataModes.SimsView:
                        vid = ViewIndex;
                        trs = Trell.SimsView;
                        break;
                }
                for (int i = 0; i < l; i++)
                {
                    IList<DataTier> t = trs.GetChildList()[rwpylon.JoinIndex[i]][vid].Child;
                    if (t.Count > 0)
                        for (int x = 0; x < t.Count; x++)
                            t[x][rwpylon.JoinOrdinal[i]] = Write;
                }
            }
        }


        public DataTier[][] GetChildList(IList<DataRelay> relays = null)
        {

            return GetChildMaps(relays).Select((t, x) =>  (t != null) ?
                                                    childTrells[x].Filtering.Evaluator != null ?
                                                    t.Select(v => v.Value[Trell.ChildInherits[x]])
                                                    .Where(childTrells[x].Filtering.Evaluator).ToArray() :
                                                    t.Select(v => v.Value[Trell.ChildInherits[x]]).ToArray() : new DataTier[0]).ToArray();
        }
        public DataTier[][] GetChildList(int[] ids)
        {

            return GetChildMaps(ids).Select((t, x) => (t != null) ?
                                                    childTrells[x].Filtering.Evaluator != null ?
                                                    t.Select(v => v.Value[Trell.ChildInherits[x]])
                                                    .Where(childTrells[x].Filtering.Evaluator).ToArray() :
                                                    t.Select(v => v.Value[Trell.ChildInherits[x]]).ToArray() : new DataTier[0]).ToArray();
        }
        public DataTier[][] GetChildList()
        {
            return GetChildList();
        }
        public DataTier[] GetChildList(string trellName)
        {
            int id = ChdRelays.GetIdByChild(trellName);            
            if (id > -1)            
                return GetChildList(id);            
            else
                return null;
        }
        public DataTier[] GetChildList(int id)
        {  
            List<Vessel<TierInherits>> map = GetChildMap(id);
            if (map != null)
            {
                IEnumerable<DataTier> result = map.Select(v => v.Value[Trell.ChildInherits[id]]);
                Func<DataTier, bool> eval = childTrells[id].Filtering.Evaluator;
                if (eval != null)
                    return result.Where(eval).ToArray();
                return result.ToArray();
            }
            return null;
        }    

        public void SetChildMapParams()
        {
            if (!childMapIsSet)
            {
                if (childTrells == null)
                    childTrells = ChdRelays.Select(r => r.Child.Trell).ToArray();

                if (IsPrime)
                    if (childtiermaps == null)
                        childtiermaps = new List<Vessel<TierInherits>>[childTrells.Length];

                if (childtiers == null)
                    childtiers = new DataTiers[childTrells.Length];

                childMapIsSet = true;
            }
        }

        public List<Vessel<TierInherits>> GetChildMap(int id)
        {
            SetChildMapParams();

            if (!IsPrime)
            {
               return Devisor.GetChildMap(id);
            }
            else
            {
                DataRelays drs = ChdRelays;

                if (childtiermaps[id] == null)
                    childtiermaps[id] = childTrells[id].Tiers.Registry.Keymap.GetList(this.GetMapKey64(drs[id].Parent.KeysOrdinal));
            }
            return childtiermaps[id];
        }
        public List<Vessel<TierInherits>>[] GetChildMaps(IList<DataRelay> relays = null)
        {
            SetChildMapParams();

            if (!IsPrime)
            {
              return Devisor.GetChildMaps(relays);
            }
            else
            {
                DataRelays drs = Trell.ChildRelays;

                int[] ids = null;
                if (relays == null)
                    ids = childtiermaps.Select((o, y) => (o == null) ? y : -1).Where(z => z > -1).ToArray();
                else
                    ids = drs.GetIdByRelay(relays);// relays.Select(r => drs.GetIdByName(r.RelayName)).Where(i => i > -1).ToArray();

                ids.Select(i => (i > -1 && childtiermaps[i] == null) ?
                                      childtiermaps[i] = childTrells[i].Tiers.Registry.Keymap
                                                          .GetList(this.GetMapKey64(drs[i].Parent.KeysOrdinal)) : null).ToArray();
            }
            return childtiermaps;
        }
        public List<Vessel<TierInherits>>[] GetChildMaps(int[] ids)
        {
            SetChildMapParams();

            if (!IsPrime)
            {
                return Devisor.GetChildMaps(ids);
            }
            else
            {
                if (ids.Length > 0)
                {
                    DataRelays drs = Trell.ChildRelays;
                    ids.Select(i => (i > -1 && childtiermaps[i] == null) ?
                                        childtiermaps[i] = childTrells[i].Tiers.Registry.Keymap
                                                            .GetList(this.GetMapKey64(drs[i].Parent.KeysOrdinal)) : null).ToArray();
                }
            }
            return childtiermaps;
        }
        public List<Vessel<TierInherits>>[] GetChildMaps(JoinCubeRelay[] ids)
        {
            SetChildMapParams();

            if (!IsPrime)
            {
                return Devisor.GetChildMaps(ids);
            }
            else
            {
                if (ids.Length > 0)
                {
                    DataRelays drs = Trell.ChildRelays;

                    ids.Select(r => (childtiermaps[r.CubeRelayIndex] == null) ?
                                            childtiermaps[r.CubeRelayIndex] = r.childTiers.Registry.Keymap
                                                            .GetList(this.GetMapKey64(r.parentKeyOrdinal)) : null).ToArray();
                }
            }
            return childtiermaps;
        }

        public DataTiers[] GetCubeChildMap(IList<DataRelay> relays = null)
        {

            SetChildMapParams();

            DataRelays drs = Trell.ChildRelays;

            DataModes mode = DataModes.TiersView;
            if (Tiers.Mode == DataModes.Sims || Tiers.Mode == DataModes.SimsView)
                mode = DataModes.SimsView;

            int[] ids = new int[0];
            if (relays == null)
                ids = drs.Select((o, y) => y).ToArray();
            else
                ids = drs.GetIdByRelay(relays);

            DataTier[][] maps = GetChildList(ids);

            SortedList<int, object>[] autokeys = ids.Select(i =>
                                                new SortedList<int, object>(drs[i].Parent.KeysOrdinal.Select((o, z) =>
                                                    new KeyValuePair<int, object>(drs[i].Child.KeysOrdinal[z], this.iN[o]))
                                                        .ToDictionary(k => k.Key, v => v.Value))).ToArray();

            ids.Select((o, y) => childtiers[o] =
                                new DataTiers(childTrells[o], maps[o], autokeys[y], mode)).ToArray();
            return childtiers;
        }

        public DataTiers[] GetChildTiers(IList<DataRelay> relays = null)
        {           
            DataRelays drs = Trell.ChildRelays;

            SetChildMapParams();

            int[] ids = null;
            if (relays == null)
                ids = drs.Select((o, y) => y).ToArray();
            else
                ids = drs.GetIdByRelay(relays);

            return GetChildTiers(ids);         
        }
        public DataTiers[] GetChildTiers(int[] ids)
        {
            DataModes mode = DataModes.TiersView;
            if (!Tiers.Mode.IsTiersMode())
                mode = DataModes.SimsView;

            DataTier[][] maps = GetChildList(ids);

            if (ids.Length > 0)
            {
                ids.Select(r => childtiers[r] =
                          new DataTiers(childTrells[r], maps[r], mode)).ToArray();
            }
            return childtiers;
        }
        public DataTiers[] ChildTiers
        {
            get
            {
                return GetChildTiers();
            }
        }
        public DataTiers   GetChild(string trellName)
        {
            int id = ChdRelays.GetIdByChild(trellName);
            if (id > -1)
                return GetChildTiers(new int[] { id })[id];
            else
                return null;
        }

        private DataTrellis[] childTrells
        {
            get { return Tiers.ChildTrells; }
            set { Tiers.ChildTrells = value; }
        }

        public void SetParentMapParams()
        {
            if (!parentMapIsSet)
            {
                if (parentTrells == null)
                    parentTrells = PrtRelays.Select(r => r.Parent.Trell).ToArray();

                if (IsPrime)
                    if (parenttiermaps == null)
                        parenttiermaps = new List<Vessel<TierInherits>>[parentTrells.Length];

                if (parenttiers == null)
                    parenttiers = new DataTiers[parentTrells.Length];

                parentMapIsSet = true;
            }
        }

        public List<Vessel<TierInherits>> GetParentMap(int id)
        {
            SetParentMapParams();

            if (!IsPrime)
            {
               return Devisor.GetParentMap(id);
            }
            else
            {
                DataRelays drs = Trell.ParentRelays;

                if (parenttiermaps[id] == null)
                    parenttiermaps[id] = parentTrells[id].Tiers.Registry.Keymap.GetList(this.GetMapKey64(drs[id].Child.KeysOrdinal));
            }
            return parenttiermaps[id];
        }
        public List<Vessel<TierInherits>>[] GetParentMaps(IList<DataRelay> relays = null)
        {
            SetParentMapParams();

            if (!IsPrime)
            {
                return Devisor.GetParentMaps(relays);
            }
            else
            {
                DataRelays drs = Trell.ParentRelays;

                int[] ids = new int[0];
                if (relays == null)
                    ids = parenttiermaps.Select((o, y) => (o == null) ? y : -1).Where(z => z > -1).ToArray();
                else
                    ids = drs.GetIdByRelay(relays);

                ids.Select(i => (i > -1 && parenttiermaps[i] == null) ?
                                       parenttiermaps[i] = parentTrells[i].Tiers.Registry
                                           .Keymap.GetList(this.GetMapKey64(drs[i].Child.KeysOrdinal))
                                               : null).ToArray();
            }
            return parenttiermaps;
        }
        public List<Vessel<TierInherits>>[] GetParentMaps(int[] ids)
        {
            SetParentMapParams();

            if (!IsPrime)
            {
                return Devisor.GetParentMaps(ids);
            }
            else
            {
                if (ids.Length > 0)
                {
                    DataRelays drs = Trell.ParentRelays;

                    ids.Select(i => (i > -1 && parenttiermaps[i] == null) ?
                                        parenttiermaps[i] = parentTrells[i].Tiers.Registry
                                            .Keymap.GetList(this.GetMapKey64(drs[i].Child.KeysOrdinal))
                                                : null).ToArray();
                }
            }
            return parenttiermaps;
        }

        public DataTier[][] GetParentList(IList<DataRelay> relays = null)
        {
            
            return GetParentMaps(relays).Select((t, x) => t != null ?
                                                    parentTrells[x].Filtering.Evaluator != null ?
                                                    t.Select(v => v.Value[Trell.ParentInherits[x]])
                                                    .Where(parentTrells[x].Filtering.Evaluator).ToArray() :
                                                    t.Select(v => v.Value[Trell.ParentInherits[x]]).ToArray() : new DataTier[0]).ToArray();
        }
        public DataTier[][] GetParentList(int[] ids)
        {
            return GetParentMaps(ids).Select((t, x) => (t != null) ?
                                                    parentTrells[x].Filtering.Evaluator != null ?
                                                    t.Select(v => v.Value[Trell.ParentInherits[x]])
                                                    .Where(parentTrells[x].Filtering.Evaluator).ToArray() :
                                                    t.Select(v => v.Value[Trell.ParentInherits[x]]).ToArray() : new DataTier[0]).ToArray();
        }
        public DataTier[][] GetParentList()
        {
            return GetParentList();
        }
        public DataTier[] GetParentList(string trellName)
        {
            int id = PrtRelays.GetIdByParent(trellName);
            if (id > -1)
                return GetParentList(id);
            else
                return null;
        }
        public DataTier[] GetParentList(int id)
        {
            return GetParentMap(id).Select(v => v.Value[Trell.ParentInherits[id]]).ToArray();
        }

        public DataTiers[] GetParentTiers(IList<DataRelay> relays = null)
        {

            DataRelays drs = Trell.ParentRelays;

            SetParentMapParams();

            int[] ids = null;
            if (relays == null)
                ids = drs.Select((o, y) => y).ToArray();
            else
                ids = drs.GetIdByRelay(relays);

            return GetParentTiers(ids);
        }
        public DataTiers[] GetParentTiers(int[] ids)
        {
            DataModes mode = DataModes.TiersView;
            if (!Tiers.Mode.IsTiersMode())
                mode = DataModes.SimsView;

            DataTier[][] maps = GetParentList(ids);

            DataRelays drs = Trell.ParentRelays;          

            if (ids.Length > 0)
            {
                ids.Select(r => parenttiers[r] =
                          new DataTiers(drs[r].Parent.Trell, maps[r], mode)).ToArray();
            }
            return parenttiers;
        }
        public DataTiers[] ParentTiers
        {
            get
            {
                return GetParentTiers();
            }
        }
     
        public DataTiers   GetParent(string trellName)
        {
            int id = PrtRelays.GetIdByParent(trellName);
            if (id > -1)
                return GetParentTiers(new int[] { id })[id];
            else
                return null;
        }

        public bool AnyParent(string ParentTrellName)
        {
            return (GetParent(ParentTrellName) != null) ? true : false;
        }

        private DataTrellis[] parentTrells
        {
            get { return Tiers.ParentTrells; }
            set { Tiers.ParentTrells = value; }
        }

        public Dictionary<string, object> GetTierBag()
        {
            Dictionary<string, object> topbag = new Dictionary<string, object>()
            {
                { "Checked",    Checked    }, { "Index",       Index       }, { "Page",   Page },
                { "PageIndex",  PageIndex  }, { "ViewIndex",   ViewIndex   }, { "NoId",   NoId },
                { "Edited",     Edited     }, { "Deleted",     Deleted     }, { "Added",  Added },
                { "Synced",     Synced     }, { "Saved",       Saved       },  { "Fields", Fields }              
            };
            return topbag;
        }
        #endregion

        #region IDataGridBinder
        public object BindingSource
        { get { return this; } }

        public IDataGridStyle GridStyle
        { get; set; }
        public IDepotSync DepotSync
        { get { if (BoundedGrid != null) return BoundedGrid.DepotSync; return null; } set { if (BoundedGrid != null) BoundedGrid.DepotSync = value; } }
        public IDataGridBinder BoundedGrid
        { get { return boundedGrid; } set { boundedGrid = value; } }
        #endregion

        #region Serialization

        public int Serialize(Stream tostream, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        {
            if (Prime == null)
                return this.SetRaw(tostream);
            else
                return this.SetSim(tostream);
        }
        public int Serialize(ISerialContext buffor, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        {
            if (IsPrime)
                return this.SetRaw(buffor);
            else if (State.Edited && !State.Synced)
                return this.SetMod(buffor);
            else
                return this.SetSim(buffor);
        }

        public object Deserialize(Stream fromstream, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        {
                return this.GetRaw(fromstream);
        }
        public object Deserialize(ref object fromarray, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        {
            if (IsPrime)
                return this.GetRaw(ref fromarray);
            else if (State.Edited && !State.Synced)
                return this.GetMod(ref fromarray);
            else
                return this.GetRaw(ref fromarray);
        }

        public object[] GetMessage()
        {
            return this.AsArray();
        }
        public object   GetHeader()
        {
            return Trell;
        }

        public ushort   GetDriveId()
        {
            Noid inl = (Noid)iN[NoidOrdinal];
            if (IsPrime && !IsSingleton && (inl.IsEmpty || inl.DriveShortValue == 0))
                if (PrimeIndex <= 0)
                    return Convert.ToUInt16(0);
                else
                    return Convert.ToUInt16((PrimeIndex / Tiers.SectorSize) / Tiers.DriveSize);
            else
                return inl.DriveShortValue;
        }
        public ushort   GetSectorId()
        {
            Noid inl = (Noid)iN[NoidOrdinal];
            if (IsPrime && !IsSingleton && (inl.IsEmpty || inl.SectorShortValue == 0))
                if (PrimeIndex <= 0)
                    return Convert.ToUInt16(0);
                else
                    return Convert.ToUInt16((PrimeIndex / Tiers.SectorSize) % Tiers.DriveSize);
            else
                return inl.SectorShortValue;
        }
        public ushort   GetLineId()
        {
            Noid inl = (Noid)iN[NoidOrdinal];
            if (IsPrime && !IsSingleton && (inl.IsEmpty))
                if (PrimeIndex <= 0)
                    return Convert.ToUInt16(0);
                else
                    return Convert.ToUInt16(PrimeIndex % Tiers.SectorSize);
            else
                return inl.LineShortValue;
        }

        public int      GetPrimeId()
        {
            return (DriveId * Tiers.DriveSize * Tiers.SectorSize) + (SectorId * Tiers.SectorSize) + LineId;
        }

        public ushort   StateAsInt16()
        {
            if (!IsSingleton)
                return State.ToUInt16();
            else
                return ((Noid)iN[NoidOrdinal]).StateShortValue; //BitConverter.ToUInt16((byte[])iN[NoidOrdinal], 14);
        }
        public void     GetNoidState()
        {
            State.FromBits((Noid)iN[NoidOrdinal]);
        }
        public void     SetStateNoid()
        {
            State.ToBits((Noid)iN[NoidOrdinal]);
        }

        public void     GetNoidClock()
        {
            State.FromBitClock((Noid)iN[NoidOrdinal]);
        }
        public void     SetClockNoid()
        {
            State.ToBitClock((Noid)iN[NoidOrdinal]);
        }
        public void     NewClockNoid()
        {
            State.NewBitClock((Noid)iN[NoidOrdinal]);
        }

        public string   GetMapPath()
        {
            return Tiers.Config.Path ?? ((Trell.Sphere != null) ? Trell.Sphere.Config.Path + "/" + Trell.TrellName : Trell.TrellName);
        }
        public string   GetMapName()
        {
            return Trell.TrellName + ".tier";
        }

        public Noid GetNoid()
        {
            if (iN[NoidOrdinal] != null && ((Noid)iN[NoidOrdinal]).IsNotEmpty)
            {
                if (PrimeIndex < 0 && IsPrime)
                {
                    PrimeIndex = ((Noid)iN[NoidOrdinal]).GetPrimeId(Tiers.DriveSize, Tiers.SectorSize);
                }
                GetNoidState();

                return ((Noid)iN[NoidOrdinal]);
            }
            return SetNoid();
        }

        public byte[] GetShah()
        {
            return GetNoid()[0];
        }          

        public byte[] SetShah()
        {
            return Keys.GetShah();
        }
        public byte[] SetShah(int id)
        {
            return iN[id].GetShah();
        }

        public Noid SetNoid()
        {
            bool addnew = false;
            Noid inl;
            if (Noid().IsEmpty)
            {
                inl = new Noid(new byte[24]);
                addnew = true;
            }
            else
                inl = (Noid)iN[NoidOrdinal];

            if ((IsPrime && !IsCube) || addnew)
            {
                if (PrimeIndex < 0)
                {
                    PrimeIndex = Trell.Prime.Counter++;                  
                    inl.DriveShortValue = GetDriveId();
                    inl.SectorShortValue = GetSectorId();
                    inl.LineShortValue = GetLineId();
                }
                Quid qd = Quid.Empty;
                if (QuidOrdinal > -1 && inl.IsEmpty)
                {
                    qd = new Quid(DataBank.ClusterId, (short)Trell.Prime.DataId, PrimeIndex);
                    iN[QuidOrdinal] = qd;
                }

                if (Trell.QuidShah)
                    inl.KeyLongValue = qd.LongValue;
                else
                    inl[0] = SetShah();

                iN[NoidOrdinal] = inl;

                NewClockNoid();
            }

            return (Noid)iN[NoidOrdinal];
        }

        public Noid Noid()
        {
            return (Noid)iN[NoidOrdinal];
        }

        public byte[] NoidBytes()
        {
            return ((Noid)iN[NoidOrdinal]).GetBytes();
        }

        #endregion

        #region IDriveRecorder     

        public void AppendRegistryDrive()
        {
            Tiers.Registry.AppendDrive(Noid());
        }
        public void SetRegistryDrive()
        {
            Tiers.Registry.SetDrive(Index, Noid());
        }

        public IDrive Drive
        { get; set; } = null;       
        public void WriteDrive(int index = -1)
        {
            if (!IsCube)
            {
                if (!IsPrime)
                {
                    Prime.WriteDrive(index);
                }
                else
                {
                    if (index < 0)
                        Tiers.GetDrive(this)[LineId] = n;
                    else
                        Tiers.GetDrive(this)[LineId, Pylons[index].LineOffset, Pylons[index].DataType] = iN[index];
                }
            }
            else
            {
                DataTier[] cubeTiers = GetCubeTierArray();
                int l = cubeTiers.Length;
                for (int i = 0; i < l; i++)
                    cubeTiers[i].WriteDrive();
            }
        }
        public void ReadDrive(int index = -1)
        {
            if (!IsPrime)
            {
                Prime.ReadDrive(index);
            }
            else
            {
                if (index < 0)
                    n = Tiers.GetDrive(this)[LineId];
                else
                    iN[index] = Tiers.GetDrive(this)[LineId, Pylons[index].LineOffset, Pylons[index].DataType];
            }
        }
        public void OpenDrive()
        {
            if (IsPrime)
                if (Tiers.Mode == DataModes.Tiers)               
                        Tiers.GetDrive(this);
        }
        public Noid ReadNoid()
        {
            if (!IsPrime)
            {
                return Prime.ReadNoid();
            }
            else
            {
                DataPylon dp = Pylons[NoidOrdinal];
                return (Noid)Tiers.GetDrive(this)[LineId, dp.LineOffset , dp.DataType];
            }
        }
        public object ReadCell(int index)
        {
            if (!IsPrime)
            {
                return Prime.ReadCell(index);
            }
            else
            {
                DataPylon dp = Pylons[index];
                return Tiers.GetDrive(this)[LineId, dp.LineOffset, dp.DataType];
            }
        }
        #endregion
   
    }
}