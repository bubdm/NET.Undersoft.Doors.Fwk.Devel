using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Dors.Drive;
using System.IO;
using System.Dors;
using System.Dors.Mathtab;

namespace System.Dors.Data
{
    [JsonArray]
    public class DataTiers : CollectionBase, IBindingList, ITypedList, IListSource, IDataConfig, IDataTiers, IDataDescriptor,
                                             IDataSerial, IDataGridBinder, INoid, IDataTreatment, IDriveRecorder
    {
        #region Private NonSerilaized
        [NonSerialized] private static int HASHES_CAPACITY_STANDARD_VALUE = 51;                          // probably there is no need for more then 500 values in hashes, +1 cause 0based count
        [NonSerialized] private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;     // established max count for access requests to hashes
        [NonSerialized] private DataNoid registry;
        [NonSerialized] private DataTrellis trell;
        [NonSerialized] private DataPylons pylons;
        [NonSerialized] private DataTiers tiersview;
        [NonSerialized] private DataTiers tierstotal;
        [NonSerialized] private HashSet<int> deleteSet = new HashSet<int>(new HashComparer());
        [NonSerialized] private DataRelaying relaying;
        [NonSerialized] private IDataGridBinder boundedGrid;
        [NonSerialized] private IDrive drive;
        [NonSerialized] private SortedList<int, IDrive[]> driveArray;

        [NonSerialized] public Func<DataTier, bool> FilterEvaluator;

        private JoinParentList[] parentjoins
        { get { return Relaying.joinParentList; } }
        private JoinChildList[] childjoins
        { get { return Relaying.joinChildList; } }
        private JoinFullList[] fulljoins
        { get { return Relaying.joinFullList; } }

        #endregion
      
        public object access = new object();

        public DataTiers(DataModes _mode = DataModes.Tiers)
        {
            Mode = _mode;
            Trell = new DataTrellis();
            State = new DataState(Trell);
            dataLine = new MattabData(this);

            DataStore store = DataStore.Space;

            if (!Trell.IsPrime)
            {
                Config = new DataConfig(this, store);
            }
            else
            {
                store = DataStore.Bank;
                Config = new DataConfig(this, store);
            }

            switch (_mode)
            {
                case DataModes.Tiers:
                    if (Trell.IsPrime)
                        registry = new DataNoid(this, store);
                    //else if (Trell.Devisor != null)
                    //    registry = new DataNoid(this, Trell.Devisor.Tiers.Registry, store);
                    else if (Trell.Prime != null)
                        registry = new DataNoid(this, Trell.Prime.Tiers.Registry, store);
                    tiersview = new DataTiers(DataModes.TiersView);
                    tiersview.Data = dataLine;
                    break;
                case DataModes.TiersView:
                    registry = Trell.Tiers.Registry;
                    break;
                case DataModes.Sims:
                    registry = Trell.Tiers.Registry;
                    tiersview = new DataTiers(DataModes.SimsView);
                    tiersview.Data = dataLine;
                    break;
                case DataModes.SimsView:
                    registry = Trell.Tiers.Registry;
                    break;
            }

            SerialCount = 0;
            DeserialCount = 0;
        }
        public DataTiers(DataTrellis _trell, DataModes _mode = DataModes.Tiers)
        {
            Mode = _mode;
            Trell = _trell;            
            State = new DataState(Trell);
            DataStore store = DataStore.Space;
            //Changes = new TiersChanges(this);
            dataLine = new MattabData(this);

            if (!Trell.IsPrime)
            {
                Config = new DataConfig(this, store);
            }
            else
            {
                store = DataStore.Bank;
                Config = new DataConfig(this, store);
            }

            switch (_mode)
            {
                case DataModes.Tiers:
                    if (Trell.IsPrime)
                        registry = new DataNoid(this, store);
                    //else if (Trell.Devisor != null)
                    //    registry = new DataNoid(this, Trell.Devisor.Tiers.Registry, store);
                    else if (Trell.Prime != null)
                        registry = new DataNoid(this, Trell.Prime.Tiers.Registry, store);
                    tierstotal = new DataTiers(_trell, DataModes.TiersView);
                    tiersview = new DataTiers(_trell, DataModes.TiersView);
                    tiersview.Registry = registry;
                    tiersview.Data = dataLine;
                    break;
                case DataModes.Sims:
                    registry = Trell.Tiers.Registry;
                    tierstotal = new DataTiers(_trell, DataModes.SimsView);
                    tiersview = new DataTiers(_trell, DataModes.SimsView);
                    tiersview.Registry = registry;
                    tiersview.Data = dataLine;
                    break;
            }

            SerialCount = 0;
            DeserialCount = 0;
        }
        public DataTiers(DataTrellis _trell, DataTier[] tierArray, DataModes _mode = DataModes.Tiers)
        {
            Mode = _mode;
            Trell = _trell;
            State = new DataState(Trell);
            DataStore store = DataStore.Space;

            if (!Trell.IsPrime)
            {
                Config = new DataConfig(this, store);
            }
            else
            {
                store = DataStore.Bank;
                Config = new DataConfig(this, store);
            }

            switch (_mode)
            {
                case DataModes.Tiers:
                    if (Trell.IsPrime)
                        Registry = new DataNoid(this, store);
                    //else if (Trell.Devisor != null)
                    //    Registry = new DataNoid(this, Trell.Devisor.Tiers.Registry, store);
                    else if (Trell.Prime != null)
                        Registry = new DataNoid(this, Trell.Prime.Tiers.Registry, store);
                    if (Registry != null)
                        AddRange(tierArray);
                    else if (Trell.Prime != null)
                        AddRefRange(tierArray);
                    if (Trell.Tiers.Data == null)
                        Trell.Tiers.Data = new MattabData(this);
                    Data = Trell.Tiers.Data;
                    TiersView = new DataTiers(_trell, Tiers.AsArray(), DataModes.TiersView);
                    break;
                case DataModes.TiersView:
                    Registry = Trell.Tiers.Registry;
                    Data = Trell.Tiers.Data;
                    AddViewRange(tierArray);
                    break;
                case DataModes.Sims:
                    Registry = Trell.Tiers.Registry;
                    if (Trell.Sims.Data == null)
                        Trell.Sims.Data = new MattabData(this);
                    Data = Trell.Sims.Data;
                    AddSimRange(tierArray, DataModes.Sims);
                    TiersView = new DataTiers(_trell, Tiers.AsArray(), DataModes.SimsView);
                    break;
                case DataModes.SimsView:
                    Registry = Trell.Tiers.Registry;
                    Data = Trell.Sims.Data;
                    AddViewRange(tierArray);
                    break;
            }

            Trell.PagingDetails.ComputePageCount(Trell.CountView);
            SerialCount = 0;
            DeserialCount = 0;
        }
        public DataTiers(DataTrellis _trell, IList<DataTier> tierArray, DataModes _mode = DataModes.Tiers)
        {
            Mode = _mode;
            Trell = _trell;
            State = new DataState(Trell);
            DataStore store = DataStore.Space;

            if (!Trell.IsPrime)
            {
                Config = new DataConfig(this, store);
            }
            else
            {
                store = DataStore.Bank;
                Config = new DataConfig(this, store);
            }

            switch (_mode)
            {
                case DataModes.Tiers:
                    if (Trell.IsPrime)
                        Registry = new DataNoid(this, store);
                    //else if (Trell.Devisor != null)
                    //    Registry = new DataNoid(this, Trell.Devisor.Tiers.Registry, store);
                    else if (Trell.Prime != null)
                        Registry = new DataNoid(this, Trell.Prime.Tiers.Registry, store);
                    if (Registry != null)
                        AddRange(tierArray);
                    else if (Trell.Prime != null)
                        AddRefRange(tierArray);
                    if (Trell.Tiers.Data == null)
                        Trell.Tiers.Data = new MattabData(this);
                    Data = Trell.Tiers.Data;
                    TiersView = new DataTiers(_trell, Tiers.AsArray(), DataModes.TiersView);
                    break;
                case DataModes.TiersView:
                    Registry = Trell.Tiers.Registry;
                    Data = Trell.Tiers.Data;
                    AddViewRange(tierArray);
                    break;
                case DataModes.Sims:
                    Registry = Trell.Tiers.Registry;
                    if (Trell.Sims.Data == null)
                        Trell.Sims.Data = new MattabData(this);
                    Data = Trell.Sims.Data;
                    AddSimRange(tierArray, DataModes.Sims);
                    TiersView = new DataTiers(_trell, Tiers.AsArray(), DataModes.SimsView);
                    break;
                case DataModes.SimsView:
                    Registry = Trell.Tiers.Registry;
                    Data = Trell.Sims.Data;
                    AddViewRange(tierArray);
                    break;
            }

            Trell.PagingDetails.ComputePageCount(Trell.CountView);
            SerialCount = 0;
            DeserialCount = 0;
        }
        public DataTiers(DataTrellis _trell, IList<DataTier> tierArray, SortedList<int, object>autoKeys, DataModes _mode = DataModes.Tiers)
        {
            Mode = _mode;
            Trell = _trell;
            State = new DataState(Trell);
            DataStore store = DataStore.Space;
            AutoKeys = autoKeys;

            if (!Trell.IsPrime)
            {
                Config = new DataConfig(this, store);
            }
            else
            {
                store = DataStore.Bank;
                Config = new DataConfig(this, store);
            }

            switch (_mode)
            {
                case DataModes.Tiers:
                    if (Trell.IsPrime)
                        Registry = new DataNoid(this, store);
                    //else if (Trell.Devisor != null)
                    //    Registry = new DataNoid(this, Trell.Devisor.Tiers.Registry, store);
                    else if (Trell.Prime != null)
                        Registry = new DataNoid(this, Trell.Prime.Tiers.Registry, store);
                    if (Registry != null)
                        AddRange(tierArray);
                    else if (Trell.Prime != null)
                        AddRefRange(tierArray);
                    if (Trell.Tiers.Data == null)
                        Trell.Tiers.Data = new MattabData(this);
                    Data = Trell.Tiers.Data;
                    TiersView = new DataTiers(_trell, Tiers.AsArray(), DataModes.TiersView);
                    break;
                case DataModes.TiersView:
                    Registry = Trell.Tiers.Registry;
                    Data = Trell.Tiers.Data;
                    AddViewRange(tierArray);
                    break;
                case DataModes.Sims:
                    Registry = Trell.Tiers.Registry;
                    if (Trell.Sims.Data == null)
                        Trell.Sims.Data = new MattabData(this);
                    Data = Trell.Sims.Data;
                    AddSimRange(tierArray, DataModes.Sims);
                    TiersView = new DataTiers(_trell, Tiers.AsArray(), DataModes.SimsView);
                    break;
                case DataModes.SimsView:
                    Registry = Trell.Tiers.Registry;
                    Data = Trell.Sims.Data;
                    AddViewRange(tierArray);
                    break;
            }

            Trell.PagingDetails.ComputePageCount(Trell.CountView);
            SerialCount = 0;
            DeserialCount = 0;
        }

        public DataState State
        { get; set; }
        public DataModes Mode
        {
            get;
            set;
        }
        public DataConfig Config
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
        public bool Emulated
        { get; set; }
        public bool IsCube
        {
            get
            {
                return trell.IsCube;
            }
        }

        public SortedList<int, IDrive[]> DriveArray
        { get { return driveArray; } set { driveArray = value; } }

        public ushort SectorSize
        { get; set; } = 10000;
        public ushort DriveSize
        { get; set; } = 10;

        public DataTrellis Trell
        { get { return trell; } set { trell = value; } }
        public DataPylons Pylons
        {
            get { return pylons; }
            set { pylons = value; }
        }
        public DataTiers Self
        { get { return this; } }
        public DataTiers Tiers
        {
            get { if (Trell != null) { if (Mode.IsTiersMode()) return Trell.Tiers; return Trell.Sims; } return this; }
            set { if (Trell != null) { if (Mode.IsTiersMode()) Trell.Tiers = value; else Trell.Tiers = value; } }
        }
        public DataTiers TiersView
        {
            get { return tiersview; }
            set { tiersview = value; }
        }
        public DataTiers TiersTotal
        {
            get { return tierstotal; }
            set { tierstotal = value; }
        }

        public DataRelaying Relaying
        { get { if (relaying == null) relaying = new DataRelaying(this, Mode); return relaying; } set { relaying = value; } }        

        public DataTier this[long index]
        {
            get
            {
                return ((DataTier)InnerList[(int)index]);
            }
            set
            {
                List[(int)index] = value;
            }
        }
        public DataTier this[int index]
        {
            get
            {
                //if (index < Count)
                    return ((DataTier)InnerList[index]);
                //else
                //    return null;
            }
            set
            {
                List[index] = value;
            }
        }
        public DataTier this[Noid noid]
        {
            get
            {
                DataTier res = Registry[noid];
                if (res != null)
                    return res;
                else
                    return null;
            }
            set
            {
                DataTier temp = null;
                if (Registry.TryGetValue(noid, out temp))
                {
                    if (temp.IsPrime)
                        temp.PrimeArray = value.DataArray;
                    else
                        temp.DataArray = value.DataArray;
                }
            }
        }
        public object this[int index, int field]
        {
            get
            {
                return ((DataTier)List[index])[field];
            }
            set
            {
                ((DataTier)List[index])[field] = value;
            }
        }

        public void CheckAll()
        {
            this.AsEnumerable().Select(t => t.Checked = true).ToArray();
        }
        public void UncheckAll()
        {
            this.AsEnumerable().Select(t => t.Checked = false).ToArray();
        }

        public DataTier[] SaveChanges(bool toDrive = true, bool toDeposit = false)
        {
            List<DataTier> result = new List<DataTier>();
            try
            {
                for (int i = 0; i < Count; i++)
                {
                    DataTier p = this[i];
                    if (p.Saved || p.Edited || p.Added)
                        result.Add(p.SaveChanges(toDrive));
                }
                if (toDeposit)
                    SaveDeposit(result.ToArray());
            }
            catch (Exception ex)
            {
                DataBank.ToDataLog(ex);
            }
            return result.ToArray();
        }
        public void SaveChangesAsync(bool toDrive = true, bool toDeposit = false)
        {
            try
            {
                InvokeOnSaveAsync(this, toDrive, toDeposit);
            }
            catch (Exception ex)
            {
                DataBank.ToDataLog(ex);
            }
        }

        public DataTier[] ClearChanges()
        {
            List<DataTier> result = new List<DataTier>();
            for (int i = 0; i < Count; i++)
            {
                DataTier p = this[i];
                if (p.State.Canceled)
                    result.Add(p.ClearChanges());
            }
            return result.ToArray();
        }
        public DataTier[] RemoveDeleted()
        {
            lock (access)
            {
                int l = deleteSet.Count;
                if (l > 0)
                {
                    DataTier[] dtr = new DataTier[l];
                    int i = 0;
                    int min = -1;
                    foreach (int di in deleteSet)
                    {
                        dtr[i++] = Tiers[di];
                        if (min < 0 || di < min)
                            min = di;
                    }

                    for (i = 0; i < l; i++)
                        Trell.Tiers.Remove(dtr[i]);

                    deleteSet.Clear();

                    Reindex(min);

                    return dtr;
                }
                return null;
            }
        }
        public DataTier[] SaveDeposit(DataTier[] tiers)
        {
            try
            {
                if (Trell.Deposit.Box.GetMutatorDelegate(Trell.Deposit.SqlSource.Name) != null)
                    Trell.Deposit.Box.SetPrime(Trell.Deposit.SqlSource.AuthId,
                                               Trell.Deposit.SqlSource.ConnectionString,
                                               tiers,
                                               true,
                                               Trell.IsCube);
            }
            catch (Exception ex)
            {
                DataBank.ToDataLog(ex);
            }
            return tiers;
        }

        public void SetAutoIds(ref object n)
        {
            if (Trell.HasAutoId)
                foreach (DataPylon pyl in this.Trell.Prime.Pylons.AutoIdPylons)
                    if ((int)((IDataNative)n)[pyl.Ordinal] == 0) ((IDataNative)n)[pyl.Ordinal] = pyl.GetAutoIds();
            if (AutoKeys != null)
                foreach (KeyValuePair<int, object> ak in AutoKeys)
                    ((IDataNative)n)[ak.Key] = ak.Value;
        }
        public SortedList<int, object> AutoKeys
        {
            get; set;
        }

        public DataTier[] AsArray()
        {
            return InnerList.Cast<DataTier>().ToArray();
        }

        public DataTiers Clone()
        {
            return (DataTiers)this.MemberwiseClone();
        }

        #region NoId Registry

        public DataNoid Registry
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

        public bool ReNoid = false;

        public void RebuildNoid()
        {
            Registry.Clear();
            int noidord = Trell.NoidOrdinal;
            foreach (DataTier tier in this)
            {
                tier.iN[noidord] = null;
                Registry[tier.GetNoid()] = tier;
            }
            ReNoid = false;
        }

        #endregion

        #region Serialization

        public int Serialize(ISerialContext buffor, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        {
            DepotSite depotSite = buffor.IdentitySite;

            if (serialFormat == SerialFormat.Raw)
            {
                if (depotSite.Equals(DepotSite.Server))
                {
                    if (!Trell.State.Synced && Trell.State.Edited && !Trell.State.Quered)
                        return SerialCount = this.SetMod(buffor, offset, batchSize);
                }
                else
                {
                    if (!Trell.State.Synced && Trell.State.Edited)
                        return SerialCount = this.SetMod(buffor, offset, batchSize);
                }
                if (Trell.IsPrime && !Trell.IsCube)
                    return SerialCount = this.SetRaw(buffor, offset, batchSize);
                else
                    return SerialCount = this.SetSim(buffor, offset, batchSize);
            }
            else if (serialFormat == SerialFormat.Json)
                return SerialCount = this.SetJson(buffor, offset, batchSize);
            else
                return -1;
        }

        public object Deserialize(ref object fromarray, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        {
            int progresscount = 0;
            object obj = new object();
            bool _driveon = DriveOn;
            bool _isbatch = IsBatch;
            DriveOn = driveon;
            if (!DriveOn)
                IsBatch = true;

            DepotSite depotSite = (Config.ServerIdentity != null) ?
                                   Config.ServerIdentity.Site :
                                   DepotSite.Client;

            if (serialFormat == SerialFormat.Raw)
            {
                if (depotSite.Equals(DepotSite.Server))
                {
                    if (!Trell.State.Synced && Trell.State.Edited)
                        obj = this.GetMod(ref fromarray, out progresscount);
                    else
                        obj = this.GetRaw(ref fromarray, out progresscount);
                }
                else
                {
                    if ((!Trell.Quered && !Trell.Edited && !Trell.Saved && !Trell.Canceled) || Trell.Quered)
                        obj = this.GetView(ref fromarray, out progresscount);
                    else if (!Trell.State.Synced && Trell.State.Edited && !Trell.State.Quered)
                        obj = this.GetMod(ref fromarray, out progresscount);
                    else
                        obj = this.GetRaw(ref fromarray, out progresscount);
                }
            }
            else if (serialFormat == SerialFormat.Json)
                obj = this.GetJson(ref fromarray, out progresscount);

            DriveOn = _driveon;
            IsBatch = _isbatch;
            return obj;
        }

        public object[] GetMessage()
        {
            return new DataTiers[] { this };
        }
        public object GetHeader()
        {
            return Trell;
        }

        public byte[] GetShah()
        {
            return (Config.Place != null) ? Config.Place.GetShah() : null;
        }

        public ushort GetSectorId()
        {
            return 0;
        }
        public ushort GetLineId()
        {
            return 0;
        }
        public ushort GetDriveId()
        {
            return 0;
        }

        public string GetMapPath()
        {
            if (!Trell.IsPrime)
                return Trell.Config.Path;
            else
            {
                if (Trell.Deposit != null)
                    return Trell.Deposit.Config.Path;
                else
                    return Trell.Config.Path;
            }
        }
        public string GetMapName()
        {
            if (!Trell.IsPrime)
            {
                if (Mode != DataModes.Sims)
                    return Config.Path + "/" + Trell.TrellName + ".noid";
                else
                    return Config.Path + "/" + Trell.TrellName + ".sims.noid";
            }
            else
            {
                if (Trell.Deposit != null)
                    return Trell.Deposit.Config.Path + "/" + Trell.TrellName + ".ndat";
                else
                    return Config.Path + "/" + Trell.TrellName + ".ndat";
            }

        }

        public int SerialCount { get; set; }
        public int DeserialCount { get; set; }
        public int ProgressCount { get; set; }
        public int ItemsCount { get { return this.Count; } }

        #region Stream Serialization
        public int Serialize(Stream tostream, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        {
            if (serialFormat == SerialFormat.Raw)
                if (Trell.IsPrime)
                    return SerialCount = this.SetRaw(tostream, offset, batchSize);
                else
                    return SerialCount = this.SetSim(tostream, offset, batchSize);
            else if (serialFormat == SerialFormat.Json)
                return SerialCount = this.SetJson(tostream, offset, batchSize);
            else
                return -1;
        }
        public object Deserialize(Stream fromstream, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        {
            int progresscount = 0;
            object obj = new object();
            bool _driveon = DriveOn;
            bool _isbatch = IsBatch;
            DriveOn = driveon;
            if (!DriveOn)
                IsBatch = true;

            if (serialFormat == SerialFormat.Raw)
                obj = this.GetRaw(fromstream, out progresscount);
            else if (serialFormat == SerialFormat.Json)
                obj = this.GetJson(fromstream, out progresscount);

            ProgressCount += progresscount;
            DriveOn = _driveon;
            IsBatch = _isbatch;
            return obj;
        }
        #endregion

        #endregion

        #region ICollection  

        public void AddGrating(DataTier value)
        {
            value.Tiers = this;
            value.State.propagate = this.State;

            if (!Mode.IsTiersMode())
                value.Grating = new SimGrating(value, value.Grating.changes);
        }

        private DataTier add(DataTier value, bool save = false)
        {
            value.SetAutoIds();
            DataTier tier = null;
            if (!Trell.IsCube)
            {
                if (!Trell.IsPrime)
                {
                    DataTier ptier = null;
                    if (Trell.Devisor != null)
                        ptier = Trell.Devisor.Tiers.Put(value, save, true, false);
                    else if (Trell.Deposit != null)
                        ptier = Trell.Deposit.Trell.Tiers.Put(value, save, true, false);

                    if (ptier != null)
                    {
                        if (Trell.EmulateMode == DataEmulate.Sleeves)
                        {
                            tier = new DataTier(Trell, ptier);
                            tier.Index = List.Add(tier);
                            AddGrating(tier);
                        }
                        else
                        {
                            tier = ptier;
                            List.Add(tier);
                        }
                    }
                }
                else
                {
                    tier = new DataTier(Trell, ref value.n);
                    tier.Index = (List.Add(tier));
                    tier.PrimeIndex = value.PrimeIndex;
                    tier.Tiers = this;
                    tier.State.propagate = this.State;

                    if (save)
                        tier.WriteDrive();
                }
            }
            else
            {
                tier = value;
                if (Trell.EmulateMode == DataEmulate.Sleeves)
                {
                    value.Index = List.Add(tier);
                    AddGrating(tier);
                }
                else
                {
                    List.Add(tier);
                }
            }

            return tier;
        }
        private DataTier add(ref object n, bool save = false)
        {
            DataTier tier = null;
            if (!Trell.IsPrime)
            {
                DataTier ptier = null;
                if (Trell.Devisor != null)
                    ptier = Trell.Devisor.Tiers.Put(ref n, save, true, false);
                else if (Trell.Deposit != null)
                    ptier = Trell.Deposit.Trell.Tiers.Put(ref n, save, true, false);

                if (ptier != null)
                    if (Trell.EmulateMode == DataEmulate.Sleeves)
                    {
                        tier = new DataTier(Trell, ptier);
                        tier.Index = List.Add(tier);
                        AddGrating(tier);
                    }
                    else
                    {
                        tier = ptier;
                        List.Add(tier);
                    }
            }
            else
            {
                tier = new DataTier(Trell, ref n);
                tier.Index = (List.Add(tier));
                tier.PrimeIndex = ((Noid)((IDataNative)n)[Trell.NoidOrdinal]).GetPrimeId(DriveSize, SectorSize);
                tier.Tiers = this;
                tier.State.propagate = this.State;

                if (save)
                    tier.WriteDrive();
            }


            return tier;
        }
        private DataTier add(ref Noid keys, bool save = false)
        {
            DataTier tier = null;
            DataTier ptier = null;
            if (!Trell.IsPrime)
            {
                if (Trell.Devisor != null)
                    ptier = Trell.Devisor.Tiers.Put(ref keys);
                else if (Trell.Deposit != null)
                    ptier = Trell.Deposit.Trell.Tiers.Put(ref keys);
                if (ptier != null)
                {
                    if (Trell.EmulateMode == DataEmulate.Sleeves)
                    {
                        tier = new DataTier(Trell, ptier);
                        tier.Index = List.Add(tier);
                        AddGrating(tier);
                    }
                    else
                    {
                        tier = ptier;
                        List.Add(tier);
                    }
                }
            }
            else if (IsCube)
            {
                DataCube cube = Trell.Cube;
                if (cube.SubTrell != null)
                {
                    ptier = cube.SubTiers.Put(ref keys);
                    tier = new DataTier(Trell, cube.SubTrell, ptier);
                    tier.Index = List.Add(tier);
                    AddGrating(tier);
                }
            }
            //else
            //{
            //    object n = GetByNoid(ref keys);
            //    tier = new DataTier(Trell, ref n);
            //    tier.Index = (List.Add(tier));
            //    tier.Tiers = this;
            //    tier.State.propagate = this.State;
            //}

            return tier;
        }

        private DataTier addInner(DataTier value, bool save = false)
        {
            value.SetAutoIds();
            DataTier tier = null;
            if (!Trell.IsCube)
            {
                if (!Trell.IsPrime)
                {
                    DataTier ptier = null;
                    if (Trell.Devisor != null)
                        ptier = Trell.Devisor.Tiers.PutInner(value, save, true, false);
                    else if (Trell.Deposit != null)
                        ptier = Trell.Deposit.Trell.Tiers.PutInner(value, save, true, false);

                    if (ptier != null)
                        if (Trell.EmulateMode == DataEmulate.Sleeves)
                        {
                            tier = new DataTier(Trell, ptier);
                            tier.Index = InnerList.Add(tier);
                            AddGrating(tier);
                        }
                        else
                        {
                            tier = ptier;
                            InnerList.Add(tier);
                        }
                }
                else
                {
                    tier = new DataTier(Trell, ref value.n);
                    tier.Index = InnerList.Add(tier);
                    tier.PrimeIndex = value.PrimeIndex;
                    tier.Tiers = this;
                    tier.State.propagate = this.State;

                    if (save)
                        tier.WriteDrive();
                }
            }
            else
            {
                tier = value;
                if (Trell.EmulateMode == DataEmulate.Sleeves)
                {
                    tier.Index = InnerList.Add(tier);
                    AddGrating(tier);
                }
                else
                {
                    List.Add(tier);
                }
            }


            return tier;
        }
        private DataTier addInner(ref object n, bool save = false)
        {
            DataTier tier = null;
            if (!Trell.IsPrime)
            {
                DataTier ptier = null;
                if (Trell.Devisor != null)
                    ptier = Trell.Devisor.Tiers.PutInner(ref n, save, true, false);
                else if (Trell.Deposit != null)
                    ptier = Trell.Deposit.Trell.Tiers.PutInner(ref n, save, true, false);

                if (ptier != null)
                    if (Trell.EmulateMode == DataEmulate.Sleeves)
                    {
                        tier = new DataTier(Trell, ptier);
                        tier.Index = InnerList.Add(tier);
                        AddGrating(tier);
                    }
                    else
                    {
                        tier = ptier;
                        InnerList.Add(tier);
                    }
            }
            else
            {
                tier = new DataTier(Trell, ref n);
                tier.Index = InnerList.Add(tier);
                tier.Tiers = this;
                tier.State.propagate = this.State;

                if (save)
                    tier.WriteDrive();
            }

            return tier;
        }
        private DataTier addInner(ref Noid keys, bool save = false)
        {
            DataTier tier = null;
            DataTier ptier = null;
            if (!Trell.IsPrime)
            {
                if (Trell.Devisor != null)
                    ptier = Trell.Devisor.Tiers.PutInner(ref keys);
                else if (Trell.Deposit != null)
                    ptier = Trell.Deposit.Trell.Tiers.PutInner(ref keys);
                if (ptier != null)
                {
                    if (Trell.EmulateMode == DataEmulate.Sleeves)
                    {
                        tier = new DataTier(Trell, ptier);
                        tier.Index = InnerList.Add(tier);
                        AddGrating(tier);
                    }
                    else
                    {
                        tier = ptier;
                        InnerList.Add(tier);
                    }
                }
            }
            else if (IsCube)
            {
                DataCube cube = Trell.Cube;
                if (cube.SubTrell != null)
                {
                    ptier = cube.SubTiers.PutInner(ref keys);
                    tier = new DataTier(Trell, cube.SubTrell, ptier);
                    tier.Index = InnerList.Add(tier);
                    AddGrating(tier);
                }
            }

            return tier;
        }

        public bool AddToDeleteSet(int index)
        {
            return deleteSet.Add(index);
        }
        public bool RemoveFromDeleteSet(int index)
        {
            return deleteSet.Remove(index);
        }

        public DataTier AddNew()
        {
            return (DataTier)((IBindingList)this).AddNew();
        }
        public DataTier AddNew(DataTier value, bool save = false)
        {
            DataTier tier = add(value);
            if (tier != null)
            {
                Noid keys = tier.GetNoid();
                Registry[keys] = tier;

                if (save)
                    tier.AppendRegistryDrive();
            }
            return tier;
        }

        public int      Add(DataTier value, bool save = false)
        {
            value.SetAutoIds();
            DataTier tier = add(value);
            if (tier != null)
            {
                Noid keys = tier.GetNoid();
                Registry[keys] = tier;

                if (save)
                    tier.AppendRegistryDrive();

                return tier.Index;
            }
            return -1;
        }
        public DataTier Add(ref object n, bool save = false)
        {
            DataTier tier = add(ref n, save);
            if (tier != null)
            {
                Noid keys = tier.GetNoid();
                Registry[keys] = tier;

                if (save)
                    tier.AppendRegistryDrive();

            }
            return tier;
        }

        public int AddInner(DataTier value, bool save = false)
        {
            value.SetAutoIds();
            DataTier tier = addInner(value, save);
            if (tier != null)
            {
                Noid keys = tier.GetNoid();
                Registry[keys] = tier;

                if (save)
                    tier.AppendRegistryDrive();

                return tier.Index;
            }
            return -1;
        }
        public DataTier AddInner(ref object n, bool save = false)
        {
            DataTier tier = addInner(ref n, save);
            if (tier != null)
            {
                Noid keys = tier.GetNoid();
                Registry[keys] = tier;

                if (save)
                    tier.AppendRegistryDrive();
            }
            return tier;
        }

        public bool TryAdd(DataTier value, bool save = false)
        {
            value.SetAutoIds();
            IQuid keys = value.GetNoid();
            if (!Registry.ContainsKey(keys))
            {
                DataTier result = add(value, save);
                Registry[keys] = result;

                if (save)
                    result.AppendRegistryDrive();

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryAddInner(DataTier value, bool save = false)
        {            
            value.SetAutoIds();
            IQuid keys = value.GetNoid();
            if (!Registry.ContainsKey(keys))
            {
                DataTier result = addInner(value, save);
                Registry[keys] = result;

                if (save)
                    result.AppendRegistryDrive();

                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddRange(IList<DataTier> tiers)
        {
            int count = tiers.Count;
            for (int i = 0; i < count; i++)
                Add(tiers[i]);
        }

        public int AddRef(DataTier value, bool setPageIndex = true)
        {
            if (setPageIndex)
                value.PageIndex = (List.Add(value));
            else
                List.Add(value);
            return value.PageIndex;
        }
        public void AddRefRange(IList<DataTier> tiers, bool setPageIndex = true)
        {
            int count = tiers.Count;
            for (int i = 0; i < count; i++)
                AddRef(tiers[i], setPageIndex);
        }

        public DataTier Put(DataTier value, bool save = false, bool replicate = false, bool propagate = true)
        {
            bool addnew = true;
            bool inherit = false;
            value.SetAutoIds();
            Noid keys = value.GetNoid();
            DataTier tier = null;
            TierInherits otier = Registry.GetList(keys);
            if (otier != null)
            {
                inherit = true;
                tier = otier[Registry.InheritId];
                if (tier != null)
                {
                    addnew = false;
                    if (propagate)
                    {
                        tier.AllowReplic = replicate;
                        int length = tier.Cells.Count;
                        for (int i = 0; i < length; i++)
                            tier[i] = value[i];
                        if (save)
                        {
                            tier.SaveChanges(true);
                            tier.SetRegistryDrive();
                        }
                        tier.AllowReplic = true;
                    }
                }
            }

            if (addnew)
            {
                tier = add(value, save);

                if (tier != null)
                {
                    if (inherit)
                        otier[Registry.InheritId] = tier;
                    else
                        Registry[keys] = tier;

                    if (save)
                        tier.AppendRegistryDrive();
                }
            }
            return tier;
        }
        public DataTier Put(ref object n, bool save, bool replicate = false, bool propagate = true)
        {
            bool addnew = true;
            bool inherit = false;

            Noid keys = ((IDataNative)n).GetNoid(Trell.NoidOrdinal);
            if (keys == null)
                keys[0] = (Trell.QuidShah) ? ((IDataNative)n).GetShah(Trell.QuidOrdinal) : ((IDataNative)n).GetShah(Trell.KeyId);

            DataTier tier = null;
            TierInherits otier = Registry.GetList(keys);
            if (otier != null)
            {
                inherit = true;
                tier = otier[Registry.InheritId];
                if (tier != null)
                {
                    addnew = false;
                    if (propagate)
                    {
                        tier.AllowReplic = replicate;
                        int length = tier.Cells.Count;
                        for (int i = 0; i < length; i++)
                            tier[i] = ((IDataNative)n)[i];
                        if (save)
                        {
                            tier.SaveChanges(true);
                            tier.SetRegistryDrive();
                        }
                        tier.AllowReplic = true;
                    }
                }
            }
            if (addnew)
            {
                tier = add(ref n, save);

                if (tier != null)
                {
                    if (inherit)
                        otier[Registry.InheritId] = tier;
                    else
                        Registry[keys] = tier;

                    if (save)
                        tier.AppendRegistryDrive();
                }
            }
            n = null;
            return tier;
        }
        public DataTier Put(ref Noid keys, bool save = false)
        {
            bool addnew = true;
            bool inherit = false;
            DataTier tier = null;
            TierInherits otier = Registry.GetList(keys);
            if (otier != null)
            {
                inherit = true;
                tier = otier[Registry.InheritId];
                if (tier != null)
                    addnew = false;
            }
            if (addnew)
            {
                tier = add(ref keys);

                if (tier != null)
                {
                    if (inherit)
                        otier[Registry.InheritId] = tier;
                    else
                        Registry[keys] = tier;

                    if (save)
                        tier.AppendRegistryDrive();
                }
            }
            return tier;
        }

        public DataTier PutInner(DataTier value, bool save = false, bool replicate = false, bool propagate = true)
        {
            bool addnew = true;
            bool inherit = false;
            Noid keys = value.GetNoid();
            DataTier tier = null;
            TierInherits otier = Registry.GetList(keys);
            if (otier != null)
            {
                inherit = true;
                tier = otier[Registry.InheritId];
                if (tier != null)
                {
                    addnew = false;
                    if (propagate)
                    {
                        tier.AllowReplic = replicate;
                        DataCells cells = tier.Cells;
                        int length = tier.Cells.Count;
                        for (int i = 0; i < length; i++)
                            cells.SetInner(i, tier, value[i]);
                        if (save)
                        {
                            tier.SaveChanges(true);
                            tier.SetRegistryDrive();
                        }
                        tier.AllowReplic = true;
                    }
                }
            }

            if (addnew)
            {
                tier = addInner(value, save);

                if (tier != null)
                {
                    if (inherit)
                        otier[Registry.InheritId] = tier;
                    else
                        Registry[keys] = tier;

                    if (save)
                        tier.AppendRegistryDrive();
                }
            }
            return tier;
        }
        public DataTier PutInner(ref object _n, bool save = false, bool replicate = false, bool propagate = true)
        {
            bool addnew = true;
            bool inherit = false;
            object n = _n;
            Noid keys = ((IDataNative)n).GetNoid(Trell.NoidOrdinal);
            if (keys == null)
                keys[0] = (Trell.QuidShah) ? ((IDataNative)n).GetShah(Trell.QuidOrdinal) : ((IDataNative)n).GetShah(Trell.KeyId);
            DataTier tier = null;
            TierInherits otier = Registry.GetList(keys);
            if (otier != null)
            {
                inherit = true;
                tier = otier[Registry.InheritId];
                if (tier != null)
                {
                    addnew = false;
                    if (propagate)
                    {
                        tier.AllowReplic = replicate;
                        DataCells cells = tier.Cells;
                        int length = tier.Cells.Count;
                        for (int i = 0; i < length; i++)
                            cells.SetInner(i, tier, ((IDataNative)n)[i]);
                        if (save)
                        {
                            tier.SaveChanges(true);
                            tier.SetRegistryDrive();
                        }
                        tier.AllowReplic = true;
                    }
                }
            }
            if (addnew)
            {
                tier = addInner(ref n, save);

                if (tier != null)
                {
                    if (inherit)
                        otier[Registry.InheritId] = tier;
                    else
                        Registry[keys] = tier;

                    if (save)
                        tier.AppendRegistryDrive();
                }
            }
            n = null;
            return tier;
        }
        public DataTier PutInner(ref Noid keys, bool save = false)
        {
            bool addnew = true;
            bool inherit = false;
            DataTier tier = null;
            TierInherits otier = Registry.GetList(keys);
            if (otier != null)
            {
                inherit = true;
                tier = otier[Registry.InheritId];
                if (tier != null)
                    addnew = false;
            }
            if (addnew)
            {
                tier = addInner(ref keys, save);

                if (tier != null)
                {
                    if (inherit)
                        otier[Registry.InheritId] = tier;
                    else
                        Registry[keys] = tier;

                    if (save)
                        tier.AppendRegistryDrive();
                }
            }
            return tier;
        }

        public IList<DataTier>
                        PutRange(IList<DataTier> tiers, bool save = false, bool replicate = false, bool propagate = true)
        {
            List<DataTier> result = new List<DataTier>();
            int count = tiers.Count;
            for (int i = 0; i < count; i++)
                result.Add(Put(tiers[i], save, replicate, propagate));
            return result;
        }

        public void AddSim(DataTier tier, DataModes _mode, bool synced = false)
        {
            if (!tier.Deleted)
            {
                DataTier _tier = new DataTier(Trell, tier, _mode);
                InnerList.Add(_tier);
                TiersView.AddView(_tier);
                _tier.State.Synced = synced;
            }
        }
        public void AddSimRange(IList<DataTier> tiers, DataModes _mode)
        {
            if (InnerList.Count != 0)
                InnerList.Clear();
            int count = tiers.Count;
            for (int i = 0; i < count; i++)
            {
                if (!tiers[i].Deleted)
                {
                    DataTier _tier = new DataTier(Trell, tiers[i], _mode);
                    InnerList.Add(_tier);
                    TiersView.AddView(_tier);
                }
            }
        }

        public int AddView(DataTier data)
        {
            if (data != null && !data.Deleted)
                return InnerList.Add(data);
            else
                return -1;
        }
        public void AddViewRange(IList<DataTier> data)
        {
            if (InnerList.Count != 0)
                InnerList.Clear();
            if (deleteSet.Count > 0)
                InnerList.AddRange(data.Where(d => !d.Deleted).ToArray());
            else
                InnerList.AddRange(data.ToArray());
        }
        public void AddViewRange(DataTier[] data)
        {
            if (InnerList.Count != 0)
                InnerList.Clear();
            if (deleteSet.Count > 0)
                InnerList.AddRange(data.Where(d => !d.Deleted).ToArray());
            else
                InnerList.AddRange(data);
        }
        public void AddViewRange(DataTiers data)
        {
            if (InnerList.Count != 0)
                InnerList.Clear();
            if (deleteSet.Count > 0)
                InnerList.AddRange(data.AsEnumerable().Where(d => !d.Deleted).ToArray());
            else
                InnerList.AddRange(data);
        }

        public void PutView(DataTier data)
        {
            if (data != null && !data.Deleted)
            {
                int viewId = data.ViewIndex;
                if (viewId < 0)
                {
                    InnerList.Add(data);
                }
                else
                {
                    object loctier = InnerList[viewId];
                    if (!ReferenceEquals(loctier, data))
                    {
                        InnerList.Add(data);
                    }
                }
            }
        }

        public DataTier[]
                    AppendView(DataTier[] data)
        {
            if (deleteSet.Count > 0)
            {
                DataTier[] result = data.Where(d => !d.Deleted).ToArray();
                InnerList.AddRange(result);
                return result;
            }
            else
            {
                InnerList.AddRange(data);
                return data;
            }
        }

        public void Delete(DataTier value)
        {
            if (!IsCube)
                Delete(value.GetNoid());
            if (Mode.IsViewMode())
                List.Remove(value);
        }
        public void Delete(ref object n)
        {
            Noid keys = ((IDataNative)n).GetNoid(Trell.NoidOrdinal);
            object otier = Registry[keys];
            if (otier != null)
            {
                if (!IsCube)
                    Delete(keys);
                if (Mode.IsViewMode())
                    List.Remove(otier);
            }
        }
        public void Delete(Noid noid)
        {
            if (!Trell.IsPrime)
                Trell.Devisor.Tiers.Delete(noid);
            if (Trell.IsDevisor)
                Trell.Tiers.Registry.Remove(noid);
        }
        public void Delete(int index)
        {
            DataTier value = this[index];
            if (!IsCube)
                Delete(value.GetNoid());
            if (Mode.IsViewMode())
                List.Remove(value);
        }

        public void DeleteRange(IList<DataTier> data)
        {
            int count = data.Count;
            for (int i = 0; i < count; i++)
                Delete(data[i]);
        }
        public void DeleteRange(IList<object> data)
        {
            object n = null;
            int count = data.Count;
            for (int i = 0; i < count; i++)
            {
                n = data[i];
                Delete(ref n);
            }
        }

        public void Remove(DataTier value)
        {
            List.Remove(value);
        }
        public void Remove(int index)
        {
            List.RemoveAt(index);
        }

        public int IndexOf(DataTier value)
        {
            return InnerList.IndexOf(value);
        }
        public int IndexOf(object n)
        {
            int index = -1;
            if (n.GetType() == typeof(DataTier))
                return InnerList.IndexOf((DataTier)n);
            else
            {
                Noid keys = ((IDataNative)n).GetNoid(Trell.NoidOrdinal);
                object otier = Registry[keys];
                if (otier != null)
                {
                    DataTier tier = (DataTier)otier;
                    return InnerList.IndexOf(tier);
                }
            }
            return index;
        }
        public int IndexOf(Noid keys)
        {
            int index = -1;
            object otier = Registry[keys];
            if (otier != null)
            {
                DataTier tier = (DataTier)otier;
                return InnerList.IndexOf(tier);
            }
            return index;
        }

        public void Insert(int index, DataTier data)
        {
            data.Tiers = this;
            List.Insert(index, data);
            data.Index = index;
        }
        public void Insert(int index, object n)
        {
            DataTier tier = Find(n);
            if (tier == null)
            {
                tier = new DataTier(Trell, ref n);
                tier.Tiers = this;
            }
            List.Insert(index, tier);
            tier.Index = index;
        }

        public DataTier Find(DataTier data)
        {
            object otier = Registry[data.GetNoid()];
            if (otier != null)
            {
                DataTier tier = (DataTier)otier;
                return tier;
            }
            return null;
        }
        public DataTier Find(object n)
        {
            Noid keys = ((IDataNative)n).GetNoid(Trell.NoidOrdinal);
            object otier = Registry[keys];
            if (otier != null)
            {
                DataTier tier = (DataTier)otier;
                return tier;
            }
            return null;
        }
        public DataTier Find(Noid keys)
        {
            object otier = Registry[keys];
            if (otier != null)
            {
                DataTier tier = (DataTier)otier;
                return tier;
            }
            return null;
        }

        public bool Contains(DataTier data)
        {
            return InnerList.Contains(data);
        }
        public bool Contains(ref object n)
        {
            object otier = Registry[((IDataNative)n).GetNoid(Trell.NoidOrdinal)];
            if (otier != null)
            {
                DataTier tier = (DataTier)otier;
                if (InnerList.Contains(tier))
                    return true;
            }
            return false;
        }

        public void Resize(int size)
        {
            InnerList.Resize(size);
        }
        public void Reindex(int offset = 0)
        {
            for (int i = offset; i < Count; i++)
                this[i].Index = i;
        }

        #endregion

        #region IBindingList

        public object BindingSource
        { get { return this; } }
        public IDepotSync DepotSync
        { get { if (BoundedGrid != null) return BoundedGrid.DepotSync; return null; } set { if (BoundedGrid != null) BoundedGrid.DepotSync = value; } }
        public IDataGridStyle GridStyle
        { get; set; }
        public IDataGridBinder BoundedGrid
        { get { return boundedGrid; } set { boundedGrid = value; } }

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

        public IDorsEvent OnSetEvents
        {
            get; set;
        }
        public IDorsEvent OnInsertEvents
        {
            get; set;
        }
        public IDorsEvent OnDeleteEvents
        {
            get; set;
        }
        public IDorsEvent OnClearEvents
        {
            get; set;
        }
        public IDorsEvent OnSaveAsyncEvents
        {
            get; set;
        }

        private void InvokeOnSaveAsync(params object[] input)
        {
            if (OnSaveAsyncEvents != null)
                OnSaveAsyncEvents.Execute(input);
        }

        protected virtual void OnListChanged(ListChangedEventArgs ev)
        {
            if (onListChanged != null)
            {
                onListChanged(this, ev);
            }
        }

        protected override void OnClearComplete()
        {
            //OnListChanged(resetEvent);
        }
        protected override void OnInsertComplete(int index, object value)
        {
            DataTier c = (DataTier)value;
            if (Mode == DataModes.Tiers)
                c.State.Added = true;
            c.State.Synced = false;
            ListChangedEventArgs ev = new ListChangedEventArgs(ListChangedType.ItemAdded, index);
            OnListChanged(ev);
        }
        protected override void OnRemoveComplete(int index, object value)
        {
            DataTier c = (DataTier)value;
            if (Mode == DataModes.Tiers)
                c.State.Deleted = true;
            c.State.Synced = false;
            //ListChangedEventArgs ev = new ListChangedEventArgs(ListChangedType.ItemDeleted, index);
            //OnListChanged(ev);
        }
        protected override void OnSetComplete(int index, object oldValue, object newValue)
        {
            //ListChangedEventArgs ev = new ListChangedEventArgs(ListChangedType.ItemChanged, index);
            OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
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
            get { return true; }
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
            DataTier newtier = null;
            if (!Trell.IsCube)
            {
                DataTier primetier = new DataTier(Trell.Prime);
                newtier = Tiers.Put(primetier, false);
            }
            else
            {
                newtier = Trell.Cube.AddCubeTier(false);
            }

            if (Mode.IsViewMode())
                List.Add(newtier);
            else
                this.TiersView.AddView(newtier);

            return newtier;
        }       

        void IList.RemoveAt(int index)
        {
            Delete(index);
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
            //DataTiers tiers = this.Collect(property.Name, key);
            //if (tiers.Count > 0) return this.IndexOf(tiers[0]); else 
            return -1;
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
        private ListChangedEventArgs resetEvent = new
                                ListChangedEventArgs(ListChangedType.Reset, -1);
        [NonSerialized] private ListChangedEventHandler onListChanged;
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
                return CreateDescriptors(Trell.TrellName + " Tiers", Config.DataIdx);
            }
            set
            {
                CreateDescriptors(Trell.TrellName + " Tiers", Config.DataIdx, value);
            }
        }

        private PropertyDescriptorCollection createDescriptors()
        {
            PropertyDescriptorCollection origProperties = TypeDescriptor.GetProperties(typeof(DataTier));

            ArrayList properties = new ArrayList();
            HashSet<string> handledNames = new HashSet<string>();

            foreach (PropertyDescriptor desc in origProperties)
                if (typeof(ICollection).IsAssignableFrom(desc.PropertyType))
                    if (desc.Name == "Tiers")
                    {
                        if (Count > 0)
                            if (Trell.Count != Count)
                                if (handledNames.Add("Tiers"))
                                    properties.Add(new TierJoinDescriptor("All", 0, "Tiers", RelaySite.All));
                    }
                    else if (desc.Name == "ChildTiers")
                    {
                        DataRelays childRelays = this.Trell.ChildRelays;
                        if (childRelays != null)
                            for (int i = 0; i < childRelays.Count; i++)
                            {
                                DataRelay relay = childRelays[i];
                                if (handledNames.Add(relay.Child.Trell.TrellName))
                                    properties.Add(new TierJoinDescriptor(relay.Child.Trell.TrellName, i, relay.Child.Trell.TrellName, RelaySite.Child));
                            }
                    }
                    else if (desc.Name == "ParentTiers")
                    {
                        DataRelays parentRelays = this.Trell.ParentRelays;
                        if (parentRelays != null)
                            for (int i = 0; i < parentRelays.Count; i++)
                            {
                                DataRelay relay = parentRelays[i];
                                if (handledNames.Add(relay.Parent.Trell.TrellName))
                                    properties.Add(new TierJoinDescriptor(relay.Parent.Trell.TrellName, i, relay.Parent.Trell.TrellName, RelaySite.Parent));
                            }
                    }

            foreach (DataPylon dv in this.Trell.Pylons.AsEnumerable().Where(p => p.Visible))
                if (handledNames.Add(dv.PylonName))
                    properties.Add(new DataCellsDescriptor(dv.PylonName, dv));

            List<string> propnames = new List<string>() { "Synced", "Edited", "Added", "Deleted", "Saved", "Checked" };
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
                listName = Trell.TrellName + "_Tiers";
            return listName;
        }

        #endregion

        #region Relay Joins

        public void ClearJoins()
        {
            Relaying.ClearJoins();
            ChildTrells = null;
            ParentTrells = null;

            if (Relaying == null)
                Relaying = new DataRelaying(this, Mode);
            if (Relaying.ParentRelays != null)
                foreach (DataRelay rly in Relaying.ParentRelays)
                {
                    DataTiers trs = rly.Parent.Trell.TiersView;
                    trs.Relaying.ClearJoins();
                    trs.ChildTrells = null;
                    trs.ParentTrells = null;
                }
        }
     
        public DataTrellis[] ChildTrells;
        public DataTrellis[] ParentTrells;       

        public JoinChildList[] GetChildList(IList<DataRelay> relays = null)
        {
            DataRelays drs = Trell.ChildRelays;

            if (Relaying == null)
                Relaying = new DataRelaying(this, Mode);

            int[] ids = null;
            if (relays != null)
                ids = relays.Select(r => drs.GetIdByName(r.RelayName)).Where(i => i > -1).ToArray();
            else            
                ids = Trell.ChildRelays.Select((x, y) => y).ToArray();

            Relaying.SetChildJointParams();
            ids = ids.Where(x => childjoins[x] == null || drs[x].Child.Trell.Quered).ToArray();
            if (ids.Any())
                Relaying.GetJoinChilds(relays, ids);

            return childjoins;
        }
        public JoinChildList[] GetChildList(DataRelays relays)
        {
            return GetChildList((IList<DataRelay>)relays);
        }
        public JoinChildList   GetChildList(DataRelay relay)
        {
            int id = Trell.ChildRelays.GetIdByChild(relay.ChildName);
            if(id > -1)
                return GetChildList(id);
            return null;
        }
        public JoinChildList[] GetChildList(int[] ids)
        {
            DataRelays drs = Trell.ChildRelays;

            if (Relaying == null)
                Relaying = new DataRelaying(this, Mode);

            Relaying.SetChildJointParams();

            ids = ids.Where(x => childjoins[x] == null || drs[x].Child.Trell.Quered).ToArray();
            if (ids.Any())
                Relaying.GetJoinChilds(null, ids);

            return childjoins;
        }
        public JoinChildList   GetChildList(int id)
        {
            if (id > -1)
                return GetChildList(new int[] { id })[id];
            return null;
        }

        //public DataTrellis[] CubeTrells;
        //public JoinCubeList GetCubeList()
        //{
        //    if (Relaying == null)
        //        Relaying = new DataRelaying(this, Mode);

        //    if (cubejoins == null || cubejoins.List.Count != Trell.Cube.SubTiers.Count)
        //        cubejoins = Relaying.GetJoinCubes();

        //    return cubejoins;
        //}

        public JoinParentList[] GetParentList(IList<DataRelay> relays = null)
        {
            DataRelays drs = Trell.ParentRelays;

            if (Relaying == null)
                Relaying = new DataRelaying(this, Mode);
            int[] ids = null;
            if (relays != null)
                ids = relays.Select(r => Trell.ParentRelays.GetIdByName(r.RelayName)).Where(i => i > -1).ToArray();
            else
                ids = Trell.ParentRelays.Select((x, y) => y).ToArray();

            ids = ids.Where(x => parentjoins[x] == null || drs[x].Parent.Trell.Quered).ToArray();
            if (ids.Any())
                Relaying.GetJoinParents(null, ids);

            return parentjoins;
        }
        public JoinParentList[] GetParentList(DataRelays relays)
        {
            return GetParentList((IList<DataRelay>)relays);
        }
        public JoinParentList   GetParentList(DataRelay relay)
        {
            int id = Trell.ParentRelays.GetIdByParent(relay.ParentName);
            if (id > -1)
                return GetParentList(id);
            return null;
        }
        public JoinParentList[] GetParentList(int[] ids)
        {
            DataRelays drs = Trell.ParentRelays;

            if (Relaying == null)
                Relaying = new DataRelaying(this, Mode);

            Relaying.SetParentJointParams();

            ids = ids.Where(x => parentjoins[x] == null || drs[x].Parent.Trell.Quered).ToArray();
            if (ids.Any())
                Relaying.GetJoinParents(null, ids);
            return parentjoins;
        }
        public JoinParentList   GetParentList(int id)
        {
            if (id > -1)
                return GetParentList(new int[] { id })[id];
            return null;
        }

        public SortedList<int, DataPylon> ReplicPylons
        { get { return Trell.ReplicPylons; } }
        public Dictionary<string, object>[] GetTiersBag()
        {
            Dictionary<string, object>[] tiersbag = this.AsEnumerable().Select(t => t.GetTierBag()).ToArray();
            return tiersbag;
        }

        #endregion

        #region IDriveRecorder            
        public IDrive Drive
        { get { return drive; } set { drive = value; } }

        public bool ByNoid = false;
        public bool IsBatch = false;
        public bool DriveOn = false;

        public void WriteDrive()
        {
            State.Saved = false;
            if (Mode == DataModes.Tiers)
            {
                Registry.WriteDrive();

                if (Trell.IsPrime && !Trell.IsCube)
                {
                    if (DriveArray == null)
                        OpenDrive();

                    for (int i = 0; i < Count; i++)
                        this[i].WriteDrive();
                }
            }
        }
        public void ReadDrive()
        {
            int noidord = Trell.NoidOrdinal;
            if (Mode == DataModes.Tiers)
                if (Count == 0)
                {
                    Registry.Counter = 0;
                    if (Trell.IsPrime && !Trell.IsCube)
                    {
                        if (DriveArray == null)
                            OpenDrive();

                        Noid lastnoid = Noid.Empty;
                        if (!ByNoid)
                        {
                            foreach (IDrive[] drvarray in DriveArray.Values)
                                for (int z = 0; z < drvarray.Length; z++)
                                    if (drvarray[z] != null)
                                    {
                                        IDrive drv = drvarray[z];
                                        object n = null;
                                        for (int i = 0; i < SectorSize; i++)
                                        {
                                            n = drv[i];
                                            Noid noid = ((Noid)((IDataNative)n)[noidord]);
                                            if (noid.IsNotEmpty)
                                            {
                                                if (noid[15, 1][0] == 0)
                                                {
                                                    AddInner(ref n);
                                                    Registry.Counter++;
                                                }
                                                lastnoid = noid;
                                            }
                                            else
                                                break;
                                        }
                                        n = null;
                                    }
                                    else
                                        break;
                        }
                        else
                        {
                            int i = 0;
                            bool readable = true;

                            Registry.OpenDrive();

                            object n = null;
                            while (readable)
                            {
                                Noid noid = (Noid)Registry.Drive[i];
                                if (noid.IsNotEmpty)
                                {
                                    if (noid[15, 1][0] == 0)
                                    {
                                        n = GetByNoid(ref noid);

                                        AddInner(ref n);
                                        Registry.Counter++;
                                    }
                                    lastnoid = noid;
                                }
                                else
                                {
                                    readable = false;
                                }
                                i++;
                            }
                        }
                        if (lastnoid.IsNotEmpty)
                        {
                            Trell.Counter = lastnoid.GetPrimeId(DriveSize, SectorSize) + 1;
                            Trell.Pylons.SetAutoIdValues();
                        }
                    }
                    else
                    {
                        if (Registry != null)
                        {
                            if (Registry.Drive == null)
                                Registry.OpenDrive();

                            Registry.ReadDrive();
                        }
                    }
                }
        }
        public void OpenDrive()
        {
            if (Mode == DataModes.Tiers)
            {
                if (Trell.IsPrime && !Trell.IsCube)
                {
                    if (DriveArray == null)
                        DriveArray = new SortedList<int, IDrive[]>(new HashSortComparer());

                    if (Count == 0)
                    {
                        string[] driveFiles = Directory.GetFiles((Config.Disk != "") ? Config.Disk + "/" + Config.Path : Config.Path);

                        foreach (string driveFile in driveFiles)
                            if (driveFile.Contains(".ndat"))
                            {
                                string[] ids = driveFile.Split('.');
                                string name = Config.Place.Replace(".ndat", string.Format(".{0}.{1}.ndat", ids[ids.Length - 3], ids[ids.Length - 2]));
                                IDrive drv = new DriveTiers(driveFile, name, SectorSize, Trell.NType);
                                IDrive[] drvarray = SetDriveArray(drv.DriveId);
                                drvarray[drv.SectorId] = drv;
                            }
                    }
                    else
                        foreach (DataTier tier in this)
                            SetDriveLine(tier.DriveId, tier.SectorId, tier.LineId);
                }
            }
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
        public void CloseDrive()
        {
            if (DriveArray != null)
                foreach (IDrive[] da in DriveArray.Values)
                    foreach (IDrive d in da)
                        if (d != null)
                            d.Close();
            DriveArray = null;
        }

        public object GetByNoid(ref Noid noid)
        {
            if (noid[15, 1][0] == 0)
            {
                return DriveArray[noid.DriveShortValue]
                                     [noid.SectorShortValue]
                                     [noid.LineShortValue];
            }
            return null;
        }

        public IDrive GetDrive(DataTier tier)
        {
            if (tier.Drive == null)
                tier.Drive = SetDriveSector(tier.DriveId, tier.SectorId);
            return tier.Drive;
        }
        public IDrive GetDrive(object n, bool tryFind = true)
        {
            IDataNative _n = (IDataNative)n;
            IDrive _drive = null;
            if (tryFind)
            {
                Noid keys = _n.GetNoid(Trell.NoidOrdinal);
                if (keys == null)
                    keys[0] = Trell.KeyId.Select((x, y) => _n[x]).ToArray().GetShah();
                object tiers = Registry[keys];
                if (tiers != null)
                    _drive = GetDrive(tiers);
            }
            else
            {
                ushort[] dsl = _n.GetDrive(Trell.NoidOrdinal);
                _drive = SetDriveSector(dsl[0], dsl[1]);
            }
            return _drive;
        }

        private IDrive[] SetDriveArray(ushort driveId)
        {
            if (DriveArray == null)
                OpenDrive();

            ushort d = driveId;
            IDrive[] _drive = null;
            if (!DriveArray.TryGetValue(d, out _drive))
            {
                _drive = new IDrive[DriveSize];
                DriveArray.Add(d, _drive);
            }
            return _drive;
        }
        private IDrive SetDriveSector(ushort driveId, ushort sectorId)
        {
            ushort d = driveId, s = sectorId;
            IDrive[] driveArray = SetDriveArray(d);
            IDrive _drive = (driveArray != null) ? driveArray[s] : null;
            if (_drive == null)
            {
                string file = Config.File.Replace(".ndat", string.Format(".{0}.{1}.ndat", d, s));
                string name = Config.Place.Replace(".ndat", string.Format(".{0}.{1}.ndat", d, s));
                _drive = new DriveTiers(file, name, SectorSize, Trell.NType);
                if (!_drive.Exists)
                {
                    _drive.DriveId = d;
                    _drive.SectorId = s;
                    _drive.ItemSize = Trell.Size;
                    _drive.ItemCapacity = SectorSize;
                    _drive.WriteHeader();
                }
                driveArray[s] = _drive;
            }
            return _drive;
        }
        private IDrive SetDriveLine(ushort driveId, ushort sectorId, ushort lineId)
        {
            ushort d = driveId, s = sectorId, l = lineId;
            DataTier tr = this[(d * DriveSize * SectorSize) + (s * SectorSize) + l];
            if (tr != null)
            {
                if (tr.Drive == null)
                    tr.Drive = SetDriveSector(d, s);
                return tr.Drive;
            }
            return null;
        }

        public IEnumerable<DataTier> AsEnumerable()
        {
            return InnerList.Cast<DataTier>();
        }

        //public void Add(DataTier item)
        //{
        //    Add(item);
        //}

        //public void CopyTo(DataTier[] array, int arrayIndex)
        //{
        //    InnerList.CopyTo(array, arrayIndex);
        //}

        //bool ICollection<DataTier>.Remove(DataTier item)
        //{
        //    Remove(item);
        //    return true;
        //}

        //IEnumerator<DataTier> IEnumerable<DataTier>.GetEnumerator()
        //{
        //    return this.AsEnumerable().GetEnumerator();
        //}
        #endregion

        #region Mattab Formula
        [NonSerialized] private MattabData dataLine;
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

        /*ublic bool IsReadOnly => ((IList<DataTier>)Self).IsReadOnly;*/
        #endregion

    }
}