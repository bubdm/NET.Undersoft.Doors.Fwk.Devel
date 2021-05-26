using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Doors;
using System.Doors.Drive;

namespace System.Doors.Data
{
    [JsonObject]
    [Serializable]
    public class DataTrellis : IDisposable, IDataSerial, IDataMorph, IDataGridBinder, 
                               IListSource, INoid, IDataConfig, IDriveRecorder, IDataTrellis
    {
        #region Private / NonSerilaized        
        private IList<AfectMapping> afectMap;
        [NonSerialized] private DataPylons joinPylons;
        [NonSerialized] private DataPylons totalPylons;
        [NonSerialized] private SortedList<int, DataPylon> replicPylons;
        private DataPylon[] primeKey;
        private int noidOrdinal = -1;
        private int quikOrdinal = -1;
        private int[] quikPylons;
        private DataDeposit deposit;

        [NonSerialized] private DataTiers tiers;
        [NonSerialized] private DataTiers sims;
        [NonSerialized] private DataSphere sphere;
        [NonSerialized] private Type model;
        [NonSerialized] private Type nType;
        [NonSerialized] private DataModes mode;
        [NonSerialized] private IDrive drive;
        [NonSerialized] private Dictionary<string, int> pylonId;
        [NonSerialized] private Dictionary<string, int> displayId;
        [NonSerialized] private int[] childinherits;
        [NonSerialized] private int[] parentinherits;
        #endregion

        public DataDeposit Deposit
        {
            get
            {
                return (!IsPrime && Prime != null) ? Prime.Deposit : deposit;
            }
            set
            {
                deposit = value;
            }
        }
        public DataSphere  Sphere
        {
            get
            {
                return sphere;
            }
            set
            {
                if (value != null)
                {
                    sphere = value;
                    Relays.SphereOn = value;
                    State.propagate = sphere.State;
                    if (!State.Expeled)
                    {
                        Config.SetMapConfig(sphere, "Trls");

                        if (!IsPrime)
                        {
                            Tiers.Config.SetMapConfig(this);
                            Sims.Config.SetMapConfig(this);

                            Tiers.Registry.Config.Path = Tiers.Config.Path;
                            Tiers.Registry.Config.Place = Tiers.Config.Place;
                            Tiers.Registry.Config.SetMapConfig(this);
                            Tiers.Saved = true;
                            //Tiers.Registry.OpenDrive();
                        }
                        
                        //Sort.Config.SetMapConfig(this);
                        //Filter.Config.SetMapConfig(this);
                        //Relays.Config.SetMapConfig(this);
                        //Pylons.Config.SetMapConfig(this);
                    }
                }
            }
        }
        public DataTrellis Prime;
        public DataTrellis Devisor;                   
        public DataFilter  Filtering;
        public DataSort    Sorting;

        public FilterTerms Filter
        { get { return Filtering.Terms; } set { Filtering.Terms = value; } }       
        public SortTerms   Sort
        { get { return Sorting.Terms; } set { Sorting.Terms = value; } }   

        public DataPageDetails PagingDetails
        { get; set; }      

        public DataFavorites Favorites
        { get; set; }

        public string TrellName
        { get; set; }
        public string DisplayName
        { get; set; }
        public string MappingName
        { get; set; }

        public string DataPlace
        { get { return Config.Place; } }      
        public string DataIdx
        { get { return Config.DataIdx; } }
        public int    DataId
        { get { return Config.DataId; } }

        public bool Visible
        { get; set; } = true;
        public bool IsPrime
        { get; set; } = false;
        public bool IsDevisor
        { get; set; } = false;
        public bool IsCube
        { get; set; }
        public bool HasAutoId
        {
            get
            {
                return Prime.Pylons.hasAutoId;
            }
        }
        public bool QuidShah
        { get; set; }

        public DataConfig Config
        { get; set; }
        public DataParams Parameters
        { get; set; }
        public DataState State
        { get; set; } = new DataState();     
        public DataModes Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                if (value == DataModes.Sims)
                {
                    if (tiers.Count != sims.Count)
                        sims.AddSimRange(tiers.AsArray(), mode);                  
                }
            }
        }
        public DataGroup Group
        { get; set; }
        public DataEmulate EmulateMode
        { get; set; }

        public bool Added
        {
            get
            {
                return State.Added;
            }
            set
            {
                State.Added = value;
            }
        }
        public bool Checked
        {
            get
            {
                return State.Checked;
            }
            set
            {
                bool currstate = State.Checked;              
                if (currstate != value)
                {
                    DataTiers tiers = null;
                    if (Mode == DataModes.Tiers || Mode == DataModes.TiersView)
                        tiers = TiersView;
                    else
                        tiers = SimsView;

                    if (value)
                        tiers.CheckAll();
                    else
                        tiers.UncheckAll();
                }
                
            }
        }
        public bool Edited
        { get { return State.Edited; } set { State.Edited = value; } }
        public bool Synced
        { get { return State.Synced; } set { State.Synced = value; } }
        public bool Saved
        {
            get
            {
                return State.Saved;
            }
            set
            {
                State.Saved = value;
                if (Mode == DataModes.Tiers || Mode == DataModes.TiersView)
                    Tiers.AsEnumerable().Where(t => t.Edited).Select(r => r.Saved = value).ToArray();
                else
                    Sims.AsEnumerable().Where(t => t.Edited).Select(r => r.Saved = value).ToArray();
            }
        }
        public bool Quered
        { get { return State.Quered; } set { State.Quered = value; } }
        public bool Canceled
        { get { return State.Canceled; } set { State.Canceled = value; } }
        public bool Emulated
        { get; set; }

        public short EditLevel
        { get; set; } = 1;
        public short EditLength
        { get; set; } = 1;
        public short SimLevel
        { get; set; } = 1;
        public short SimLength
        { get; set; } = 4;

        public int CountView
        { get { return (Mode == DataModes.Tiers) ? (TiersView != null) ? TiersView.Count : 0 : (SimsView != null) ? SimsView.Count : 0;  } set { } }
        public int Counter
        { get; set; } = 0;
        public int Count
        { get { return (Tiers != null) ? Tiers.Count : 0; } set { } }
        public int Size
        { get; set; } = 0;

        public  Type Model
        {
            get
            {
                if (model == null)
                    return CreateModel();
                return model;
            }
            set
            {
                model = value;
            }
        }
        public  Type NType
        {
            get
            {
                if(nType == null)
                    CreateModel();
                return nType;
            }
            set
            {
                nType = value;
            }
        }
        public  Type CreateModel(bool recreate = true)
        {
            if (!recreate && model != null)
                return model;

            DataNative ndm = null;
            if ((IsPrime || Prime == null) && Pylons.Count > 0)
            {
                ndm = new DataNative(Pylons, TrellName);
            }
            else if (Prime != null && Prime.Model != null)
            {
                model = Prime.Model;
                nType = Prime.NType;
                Size = Prime.Size;
                pylonId = null;
                //CreateModelMethods();
                return model;
            }
            else
            {
                if (!Pylons.Have("NOID"))
                    Pylons.Add(new DataPylon(typeof(Noid), this, "NOID", "NOID")
                    {
                        Ordinal = 0,
                        isNoid = true,
                        isDBNull = false,
                        isIdentity = true,
                        PylonSize = 24
                    });
                ndm = new DataNative(Pylons, TrellName);
            }

            model = ndm.ModelType;
            nType = ndm.ObjectType;
            Size = ndm.ModelSize;
            pylonId = null;
            //CreateModelMethods();
            return model;
        }
        //public void CreateModelMethods(bool recreate = true)
        //{
        //    int i = 0;
        //    foreach (DataPylon pyl in Pylons)
        //    {
        //        pyl.PylonField = Pylons.PylonFields[i++];

        //        pyl.GetCellMethod = pyl.CreateGetCellMethod();
        //        pyl.SetCellMethod = pyl.CreateSetCellMethod();
        //    }
        //}

        public DataTrellis()
        {
            Config = new DataConfig(this, DataStore.Space);
            Parameters = new DataParams();
            State = new DataState();
            State.Expeled = true;
            TrellName = "Trls#" + DateTime.Now.ToFileTimeUtc().ToString();
            DisplayName = TrellName;
            Pylons = new DataPylons(this);
            PagingDetails = new DataPageDetails(true);
            tiers = new DataTiers(this, DataModes.Tiers);
            sims = new DataTiers(this, DataModes.Sims);
            Relays = new DataRelays();
            Relaying = new DataRelaying(this);
            Filtering = new DataFilter(this);
            Sorting = new DataSort(this);
            Favorites = new DataFavorites(this);
            afectMap = new List<AfectMapping>();
            SerialCount = 0;
            DeserialCount = 0;
            Group = DataGroup.None;
        }
        public DataTrellis(string trellName, bool isPrime = true, bool isCube = false)
        {
            IsPrime = isPrime;
            IsCube = isCube;

            if (isCube)
            {
                Cube = new DataCube(this);
                if(!isPrime)
                    Cube.FindRelays = true;
            }

            if (isPrime)
            {
                Prime = this;
                Devisor = this;
            }
            Config = new DataConfig(this, DataStore.Space);        
            Parameters = new DataParams();
            TrellName = (trellName != null) ? trellName : "Trls#" + DateTime.Now.ToFileTimeUtc().ToString();
            DisplayName = TrellName;  
            Pylons = new DataPylons(this);
            PagingDetails = new DataPageDetails(true);
            tiers = new DataTiers(this, DataModes.Tiers);
            sims = new DataTiers(this, DataModes.Sims);
            Relays = new DataRelays();
            Relaying = new DataRelaying(this);
            Filtering = new DataFilter(this);
            Sorting = new DataSort(this);
            Favorites = new DataFavorites(this);
            afectMap = new List<AfectMapping>();
            SerialCount = 0;
            DeserialCount = 0;
            Group = DataGroup.None;
        }
        public DataTrellis(string trellName, DataRelays cubeRelays, bool isPrime = true)
        {
            IsPrime = isPrime;
            IsCube = true;
            Cube = new DataCube(this, cubeRelays);
            if(!IsPrime)
                Cube.FindRelays = true;

            if (isPrime)
            {
                Prime = this;
                Devisor = this;
            }
            Config = new DataConfig(this, DataStore.Space);
            Parameters = new DataParams();
            TrellName = (trellName != null) ? trellName : "Trls#" + DateTime.Now.ToFileTimeUtc().ToString();
            DisplayName = TrellName;
            Pylons = new DataPylons(this);
            PagingDetails = new DataPageDetails(true);
            tiers = new DataTiers(this, DataModes.Tiers);
            sims = new DataTiers(this, DataModes.Sims);
            Relays = new DataRelays();
            Relaying = new DataRelaying(this);
            Filtering = new DataFilter(this);
            Sorting = new DataSort(this);
            Favorites = new DataFavorites(this);
            afectMap = new List<AfectMapping>();
            SerialCount = 0;
            DeserialCount = 0;
            Group = DataGroup.None;
        }
        public DataTrellis(string trellName, DataTrellis prime, bool locate = false)
        {
            IsPrime = false;
            IsCube = prime.IsCube;

            if (IsCube)
            {
                Cube = new DataCube(this, prime.Cube.CubeRelays);
                Cube.FindRelays = true;
            }

            Config = new DataConfig(this, DataStore.Space);
            Parameters = new DataParams();

            TrellName = trellName;
            DisplayName = trellName;
            if (!locate)
            {
                Prime = (prime.IsPrime) ? prime : prime.Prime;
                Devisor = prime;
                prime.IsDevisor = true;
            }
            else
            {
                IsPrime = prime.IsPrime;
                Prime = prime.Prime.Locate();
                Devisor = prime.Devisor.Locate();
                IsDevisor = prime.IsDevisor;
            }
            Pylons = new DataPylons(this);
            PagingDetails = new DataPageDetails(true);
            tiers = new DataTiers(this, DataModes.Tiers);
            sims = new DataTiers(this, DataModes.Sims);
            Relays = new DataRelays();
            Relaying = new DataRelaying(this);
            Filtering = new DataFilter(this);
            Sorting = new DataSort(this);
            Favorites = new DataFavorites(this);
            afectMap = new List<AfectMapping>();
            SerialCount = 0;
            DeserialCount = 0;
            Group = DataGroup.None;
        }

        public IDataPylons iPylons
        { get { return Pylons; } }
        public DataPylons Pylons
        { get; set; }

        public int NoidOrdinal
        {
            get
            {
                if (noidOrdinal < 0)
                {
                    DataPylon[] noidord = Pylons.AsEnumerable().Where(p => p.isNoid).ToArray();
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
                    DataPylon[] quikord = Pylons.AsEnumerable().Where(p => p.isQuid &&
                                                                            p.isIdentity)
                                                                            .ToArray();
                    if (quikord.Length > 0)
                        quikOrdinal = quikord[0].Ordinal;
                }
                return quikOrdinal;
            }
        }

        public int[] QuidPylons
        {
            get
            {
                if (quikPylons == null)
                {
                    quikPylons = Pylons.AsEnumerable().Where(p => p.isQuid).Select(o => o.Ordinal).ToArray();
                }
                return quikPylons;
            }
        }

        public short[] Ordinals
        { get { return Pylons.Ordinals; } }

        public DataPylon[] PrimeKey
        {
            get
            {
                return Pylons.AsEnumerable().Where(pk => pk.isKey).ToArray();
            }
            set
            {
                foreach (DataPylon p in PrimeKey)
                    p.isKey = false;

                primeKey = Pylons.AsEnumerable().Where(c => value.Where(p => p.PylonName == c.PylonName).Any()).ToArray();
                int length = primeKey.Length;
                if (length > 0)
                {
                    KeyId = new int[length];
                    for (int i = 0; i < length; i++)
                    {
                        primeKey[i].isKey = true;
                        KeyId[i] = primeKey[i].Ordinal;
                    }
                    if (KeyId.Contains(QuidOrdinal) && length == 1)
                        QuidShah = true;

                    Tiers.AsArray().Select(t => t.SetNoid()).ToArray();
                }
            }
        }
        private int[] keyId;
        public int[] KeyId
        {
            get
            {
                if(keyId == null)
                    keyId = PrimeKey.Select(x => x.Ordinal).ToArray();
                return keyId;
            }
            set
            {
                keyId = value;
            }
        }

        public Dictionary<string, int> PylonId
        {
            get
            {
                if (pylonId != null)
                    return pylonId;
                else
                    pylonId = new Dictionary<string, int>(this.Pylons.AsEnumerable().Select(p => new KeyValuePair<string, int>(p.PylonName, p.Ordinal)).ToDictionary(k => k.Key, v => v.Value));
                return pylonId;
            }
            set
            {
                pylonId = value;
            }
        }
        public Dictionary<string, int> DisplayId
        {
            get
            {
                if (displayId != null)
                    return displayId;
                else
                    displayId = new Dictionary<string, int>(this.Pylons.AsEnumerable().Select(p => new KeyValuePair<string, int>(p.DisplayName, p.Ordinal)).ToDictionary(k => k.Key, v => v.Value));
                return displayId;
            }
            set
            {
                displayId = value;
            }
        }
        
        public DataTiers Tiers
        {
            get
            {
                if (tiers == null)
                    tiers = new DataTiers(this, DataModes.Tiers);
                return tiers;
            }
            set
            {
                tiers = value;
            }
        }      
        public DataTiers Sims
        {
            get
            {
                if (sims == null)                
                    sims = new DataTiers(this, DataModes.Sims);
                return sims;
            }
            set
            {
                sims = value;
            }
        }

        public DataTiers TiersView
        {
            get
            {
                if (Tiers.Count > 0 && Tiers.TiersView.Count == 0)
                    Tiers.Query();
                return Tiers.TiersView;
            }
            set
            {
                Tiers.TiersView = value;
            }
        }
        public DataTiers SimsView
        {
            get
            {
                if (Sims.Count > 0 && Sims.TiersView.Count == 0)
                    Sims.Query();
                return Sims.TiersView;
            }
            set
            {
                Sims.TiersView = value;
            }
        }

        public DataTiers TiersTotal
        {
            get
            {
                return Tiers.TiersTotal;
            }
            set
            {
                Tiers.TiersTotal = value;
            }
        }
        public DataTiers SimsTotal
        {
            get
            {
                return Sims.TiersTotal;
            }
            set
            {
                Sims.TiersTotal = value;
            }
        }

        public DataTiers GridTotal
        {
            get { if (Mode != DataModes.Tiers) return SimsTotal; else return TiersTotal;  }
            set { if (Mode != DataModes.Tiers) SimsTotal = value; else TiersTotal = value; }
        }

        public HashSet<int> LambdaHash
        { get; set; } = new HashSet<int>(new HashComparer());
        public HashSet<int> MattabHash
        { get; set; } = new HashSet<int>(new HashComparer());

        public SortedList<int, DataPylon>
                          ReplicPylons
        {
            get
            {
                if (replicPylons == null)
                {
                    replicPylons = new SortedList<int, DataPylon>();
                    if (joinPylons == null)
                    {
                        int count = JoinPylons.Count;
                    }
                }
                return replicPylons;
            }
        }
        public DataPylons TotalPylons
        {
            get
            {
                AggregateOperand parsed = new AggregateOperand();
                if(totalPylons == null)
                    totalPylons = new DataPylons(this);
                totalPylons.AddRange(this.Pylons.AsEnumerable().Where(c =>
                                       (c.PylonName.Split('=').Length > 1 || (c.TotalOperand != AggregateOperand.None))).Select(c =>
                                       (new DataPylon(c.DataType)
                                       {
                                           PylonName = c.PylonName,
                                           TotalPattern = (c.TotalPattern != null) ? c.TotalPattern : (c.PylonName.Split('=').Length > 1) ? new DataPylon(c.DataType) { PylonName = c.PylonName.Split('=')[1] } : null,
                                           TotalOperand = (Enum.TryParse(c.PylonName.Split('=')[0], true, out parsed)) ? parsed : c.TotalOperand,
                                           Ordinal = c.Ordinal,
                                       })
                                        ).ToList());
                return totalPylons;
            }
        }

        public DataPylons JoinPylons
        {
            get
            {
                if (joinPylons == null)
                {
                    AssignJoinPylons();
                }
                return joinPylons;
            }
        }

        public DataPylons AssignJoinPylons()
        {
            AggregateOperand parsed = new AggregateOperand();
            if(joinPylons == null)
                joinPylons = new DataPylons(this);
            DataPylon[] _joinPylons = this.Pylons.AsEnumerable().Where(c => (c.PylonName.Split('#').Length > 1) || (c.JoinPattern != null && c.JoinOperand != AggregateOperand.None) || c.JoinOperand != AggregateOperand.None).ToArray();
            foreach (DataPylon c in _joinPylons)
            {
                c.JoinPattern = (c.JoinPattern != null) ? c.JoinPattern : (c.JoinOperand != AggregateOperand.None) ? new DataPylon(c.DataType) { PylonName = c.PylonName } : new DataPylon(c.DataType) { PylonName = c.PylonName.Split('#')[1] };
                c.JoinOperand = c.JoinOperand != AggregateOperand.None ? c.JoinOperand : (Enum.TryParse(c.PylonName.Split('#')[0], true, out parsed)) ? parsed : AggregateOperand.None;
                c.JoinIndex = (this.ChildRelays.Where(cr => cr.Child.Trell.Pylons.AsEnumerable()
                                              .Where(ct => ct.PylonName == ((c.JoinPattern != null) ?
                                              c.JoinPattern.PylonName :
                                              c.PylonName.Split('#')[1])).Any()).Any()) ?
                             this.ChildRelays.Where(cr => cr.Child.Trell.Pylons.AsEnumerable()
                                              .Where(ct => ct.PylonName == ((c.JoinPattern != null) ?
                                              c.JoinPattern.PylonName :
                                              c.PylonName.Split('#')[1])).Any()).ToArray().Select(ix => this.ChildRelays.ToList().IndexOf(ix)).ToArray()
                                              : null;
               c.JoinOrdinal = this.ChildRelays.Where(cr => cr.Child.Trell.Pylons.AsEnumerable()
                                               .Where(ct => ct.PylonName == ((c.JoinPattern != null) ?
                                                c.JoinPattern.PylonName :
                                                c.PylonName.Split('#')[1])).Any())
                                                .Select(cr => cr.Child.Trell.Pylons.AsEnumerable()
                                               .Where(ct => ct.PylonName == ((c.JoinPattern != null) ?
                                                c.JoinPattern.PylonName :
                                                c.PylonName.Split('#')[1]))
                                                .Select(o => o.PylonId).FirstOrDefault()).ToArray();
            }

            joinPylons.AddRange(_joinPylons);

            joinPylons.AsEnumerable().Where(j => j.JoinIndex != null).Select(p => p.JoinRelays = this.ChildRelays.Where((x, y) => p.JoinIndex.Contains(y)).ToArray());
            ReplicPylons.AddDict(this.joinPylons.AsEnumerable().Where(p => p.JoinOperand == AggregateOperand.Bind).ToDictionary(k => k.Ordinal, v => v));

            return joinPylons;
        }

        #region Searching Tier
        public int  IndexOf(object value)
        {
            switch(Mode)
            {
                case DataModes.Sims:
                    return Sims.IndexOf(value);
                case DataModes.SimsView:
                    return SimsView.IndexOf(value);
                case DataModes.Tiers:
                    return Tiers.IndexOf(value);
                case DataModes.TiersView:
                    return TiersView.IndexOf(value);
                default:
                    return -1;
            }
        }
        public bool Contains(ref object value)
        {
            switch (Mode)
            {
                case DataModes.Sims:
                    return Sims.Contains(ref value);
                case DataModes.SimsView:
                    return SimsView.Contains(ref value);
                case DataModes.Tiers:
                    return Tiers.Contains(ref value);
                case DataModes.TiersView:
                    return TiersView.Contains(ref value);
                default:
                    return false;
            }
        }        
        public bool HasPylon(string PylonName)
        {
            return this.Pylons.Have(PylonName);
        }
        public bool IsEditable(int id)
        {
            return this.Pylons[id].Editable;
        }
        public bool IsEditable(string name)
        {
            return this.Pylons[name].Editable;
        }
        #endregion

        #region IBindingList

        public object BindingSource
        {
            get
            {
                return TiersView;
            }
        }        
        public IDepotSync DepotSync { get { return (BoundedGrid != null) ? BoundedGrid.DepotSync : null; } set { BoundedGrid.DepotSync = value; } }
        [NonSerialized] private IDataGridStyle gridStyle;
        public IDataGridStyle GridStyle { get { return gridStyle; } set { gridStyle = value; } }
        public IDataGridBinder BoundedGrid
        {
            get { return Sphere != null ? Sphere.Trells.BoundedGrid : Deposit != null ? Deposit.BoundedGrid : null; }
            set { object setoobj = Sphere != null ? Sphere.Trells.BoundedGrid : Deposit != null ? Deposit.BoundedGrid : null; setoobj = value; }
        }

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
        #endregion

        #region RelayJoins

        public int[] ChildInherits
        {
            get
            {
                if (childinherits == null)
                    childinherits = ChildRelays.Select(t => t.Child.Trell.Tiers.Registry.InheritId).ToArray();
                return childinherits;
            }
        }

        public int[] ParentInherits
        {
            get
            {
                if (parentinherits == null)
                    parentinherits = ParentRelays.Select(t => t.Parent.Trell.Tiers.Registry.InheritId).ToArray();
                return parentinherits;
            }
        }

        public void ClearRelays()
        {
            if(joinPylons != null)
                joinPylons.Clear();
            if(replicPylons != null)
                replicPylons.Clear();
            Relays.Clear();
            Tiers.ClearJoins();
        }

        public DataRelays   Relays
        { get; set; }
        public DataRelaying Relaying
        { get; set; }

        public DataRelays ChildRelays
         { get { return Relaying.ChildRelays; } }
        public DataRelays ParentRelays
        { get { return Relaying.ParentRelays; } }

        #endregion

        #region CubeJoins

        public DataCube Cube
        {
            get; set;
        }

        #endregion

        #region AfectMapper

        public void AddMap(AfectMapping iafectMap)
        {
            AfectMapping[] afm = AfectMap.Where(m => m.DbTableName == iafectMap.DbTableName).ToArray();
            if (afm.Length > 0 && AfectMap.IndexOf(afm[0]) > -1)
                AfectMap[AfectMap.IndexOf(afm[0])] = iafectMap;
            else
                AfectMap.Add(iafectMap);

        }
        public void AddMaps(IList<AfectMapping> iafectMapList)
        {
            foreach (AfectMapping iafectMap in iafectMapList)
            {
                AfectMapping[] afm = AfectMap.Where(m => m.DbTableName == iafectMap.DbTableName).ToArray();
                if (afm.Length > 0 && AfectMap.IndexOf(afm[0]) > -1)
                    AfectMap[AfectMap.IndexOf(afm[0])] = iafectMap;
                else
                    AfectMap.Add(iafectMap);
            }
        }
        public void RemoveMap(AfectMapping iafectMap)
        {
            if (AfectMap.Where(m => m.DbTableName == iafectMap.DbTableName).Any())
                AfectMap.Remove(iafectMap);

        }
        public void ClearMaps()
        {
            AfectMap.Clear();
        }
     
        public IList<AfectMapping> AfectMap
        {
            get { return afectMap; }
            set { afectMap = value; }
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
            DataTiers msg = null;
            if (Mode == DataModes.Sims || Mode == DataModes.SimsView)
                msg = Sims;
            else
                msg = Tiers;

            if (msg.Count == 0)
            {
                if (msg.DriveArray == null)
                    msg.OpenDrive();
                msg.ReadDrive();
                msg.ClearJoins();
            }

            return new DataTiers[] { msg };
        }
        public object GetHeader()
        {
            if (this.Drive == null)
            {
                this.OpenDrive();
                if (this.Drive.Exists)
                    this.ReadDrive();
            }
            return this;
        }

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
            return (Config.Path != null) ? Config.Path : (this.Sphere != null) ? this.Sphere.Config.Path + "/" + this.TrellName : this.TrellName;
        }
        public string GetMapName()
        {
            return Config.Path + "/" + this.TrellName + ".trl";
        }      

         public int SerialCount { get; set; }
         public int DeserialCount { get; set; }
         public int ProgressCount { get; set; }
         public int ItemsCount { get { return Count; } }

        #endregion

        #region  IDataMorph
        public object Emulator(object source, string name = null)
        {
            return this.Emulate((DataTrellis)source);
        }
        public object Imitator(object source, string name = null)
        {
            return this.Imitate((DataTrellis)source);
        }
        public object Impactor(object source, string name = null)
        {
            DataTrellis trl = this;
            if (source != null)
            {
                bool save = ((IDataConfig)source).State.Saved;
                trl = this.Impact((DataTrellis)source, save);
                DataSpace.PrimeFinder(new DataTrellis[] { trl });
            }
            return trl;
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
            DataTrellis trell = (DataTrellis)Deserialize(ref read);
            this.Impactor(trell);
            State.Saved = false;
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
            if(Drive == null)
                if (!DataSpace.DriveClient)
                    Drive = new DriveBank(Config.File, Config.File, 10 * 1024 * 1024, typeof(DataTrellis));
                else
                    Drive = new DriveBank(Config.File, Config.File, typeof(DataTrellis));
        }
        public void CloseDrive()
        {
            if (Drive != null)
                Drive.Close();
            Drive = null;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            primeKey = null;
            pylonId = null;
            displayId = null;
            tiers = null;
            sims = null;
        }       
        #endregion
    }  
}