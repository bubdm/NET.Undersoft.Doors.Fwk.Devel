using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Collections.Concurrent;
using System.Collections;
using System.ComponentModel;
using System.Doors;
using System.Doors.Drive;

namespace System.Doors.Data
{
    [JsonObject]
    [Serializable]
    public class DataSphere : IDataSerial, IDataMorph, IDataGridBinder, IDataTreeBinder, IListSource, 
                              INoid, IDataConfig, IDriveRecorder, IDataSphere
    {
        #region Private / NonSerialized

        [NonSerialized] private DataPages pages;
        [NonSerialized] private DataPagination pagination;
        [NonSerialized] private IDataTreeBinder boundedTree;
        [NonSerialized] private IDrive drive;
        [NonSerialized] private DataSpheres spheresUp;
        [NonSerialized] private DataSphere sphereUp;
        [NonSerialized] private DataSpheres spheresOn;
        private DataSpheres spheresIn;
        private DataSphere sphereIn;
        #endregion

        public DataSphere()
        {
            SphereId = "Sphere#" + DateTime.Now.ToFileTime().ToString();
            Config = new DataConfig(this, DataStore.Space);
            State = new DataState();
            State.Expeled = true;
            DisplayName = SphereId;
            Trells = new DataTrellises(this);
            Pages = new DataPages();
            Relays = new DataRelays();
            Relays.SphereOn = this;
            Parameters = new DataParams();
            Pagination = new DataPagination(this);
        }
        public DataSphere(string iSphereId)
        {
            SphereId = (iSphereId != null) ? iSphereId : "Sphere#" + DateTime.Now.ToFileTime().ToString();
            Config = new DataConfig(this, DataStore.Space);
            State = new DataState();
            DisplayName = SphereId;
            Trells = new DataTrellises(this);
            Pages = new DataPages();
            Relays = new DataRelays();
            Relays.SphereOn = this;
            Parameters = new DataParams();
            Pagination = new DataPagination(this);
        }
    
        public string SphereId
        { get; set; }
        public string DisplayName
        { get; set; }

        public string DataPlace
        { get { return Config.Place; } }    
        public string DataIdx
        { get { return Config.DataIdx; } }

        public DataSphere Devisor
        {
            get; set;
        }

        public DataConfig Config
        { get; set; }      
        public DataState State
        { get; set; }

        public int Count
        {
            get { return Trells.Count; }
        }
        public int CountView
        {
            get { return Trells.AsEnumerable().Where(t => t.Visible).Count(); }
        }

        public bool Visible
        { get; set; } = true;

        public DataParams Parameters
        { get; set; }

        public DataTrellises Trells
        { get; set; }

        public DataSphere  this[string key]
        {
            get
            {
                DataSphere result = null;
                if (SpheresIn != null)
                    if (SpheresIn.TryGetValue(key, out result))
                        return result;
                if (SphereIn != null)
                    if (SphereIn.SphereId == key)
                        result = SphereIn;
                return result;
            }
        }
        public DataTrellis this[string key, string trellname]
        {
            get
            {
                if (key != null)
                {
                    DataSphere result = null;
                    if (SpheresIn != null)
                    {
                        result = SpheresIn[key];
                        if (result != null)
                            if (result.Trells.Have(trellname))
                                return result.Trells[trellname];
                    }
                    if (SphereIn != null)
                    {
                        if (SphereIn.SphereId == key)
                        {
                            result = SphereIn;
                            if (result.Trells.Have(trellname))
                                return result.Trells[trellname];
                        }
                    }
                    if (Trells.Have(trellname))
                        return Trells[trellname];
                }
                else
                    if (Trells.Have(trellname))
                        return Trells[trellname];

                return null;
            }
            set
            {
                if (key != null)
                {
                    if (SpheresIn != null)
                        if (SpheresIn.ContainsKey(key))
                            if (SpheresIn[key].Trells.Have(trellname))
                            {
                                SpheresIn[key].Trells[trellname] = value;
                                return;
                            }
                    if (SphereIn != null)
                        if (SphereIn.SphereId == key)
                            if (SphereIn.Trells.Have(trellname))
                            {
                                SphereIn.Trells[trellname] = value;
                                return;
                            }
                    if (Trells.Have(trellname))
                    {
                        Trells[trellname] = value;
                        return;
                    }
                }
                if (Trells.Have(trellname))
                    Trells[trellname] = value;
            }
        }

        public DataPagination Pagination
        { get { return pagination; } set { pagination = value; } }

        public DataPages Pages
        { get { return pages; } set { pages = value; } }
        public DataPages NewPages()
        {
            Pagination.CreateNew();
            return Pages;
        }
        public DataPages RePages()
        {
            Pagination.UpdateAdd();
            return Pages;
        }

        public DataRelays Relays
        { get; set; }

        public Dictionary<string,
                DataTrellis> Trellises
        {
            get
            {
                return Trells.AsEnumerable().Select(t => new KeyValuePair<string, DataTrellis>(t.TrellName, t)).ToDictionary(k => k.Key, v => v.Value);
            }
            set
            {
                if(value != null && value.Count > 0)
                {
                    DataTrellises trells = new DataTrellises();
                    trells.AddRange(value.Values.ToList());
                    trells.SphereOn = this;
                    Trells = trells;
                }
            }
        }
        public DataTrellises RootTrells
        {
            get
            {
                DataTrellises trells = new DataTrellises(this);
                trells.AddRange(Trells.AsEnumerable().Where(v => v.ParentRelays.Count() == 0).ToList());
                return trells;
            }
        }
        public DataTrellises ExpandTrells
        {
            get
            {
                DataTrellises trells = new DataTrellises(this);
                trells.AddRange(Trells.AsEnumerable().Where(v => v.ParentRelays.Count() == 0 && v.ChildRelays.Count() > 0).ToList());
                return trells;
            }
        }

        public DataSpheres SpheresOn
        { get { return spheresOn; } set { spheresOn = value; } }
        public DataSpheres SpheresUp
        { get { return spheresUp; } set { spheresUp = value; } }
        public DataSpheres SpheresIn
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
                    value.SphereUp = this;
                    if (SpheresOn != null)
                    {
                        value.SpheresUp = SpheresOn;
                        SpheresOn.SpheresInAll.Add(value);
                    }
                    value.Config.SetMapConfig(this);
                    value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
                }
            }
        }

        public DataSphere SphereUp
        { get { return sphereUp; } set { sphereUp = value; } }
        public DataSphere SphereIn
        {
            get
            {
                return sphereIn;
            }
            set
            {
                sphereIn = value;
                value.SphereUp = this;
                value.SpheresUp = this.SpheresOn;
                value.Config.SetMapConfig(this);             
                value.Trells.Config.SetMapConfig(value);            
                value.Parameters.Registry.AddRange(new Dictionary<string, object>(Parameters.Registry));
            }
        }

        public DataSpheres NewSpheres(string iSpheresId = null)
        {
            string _SpheresId = (iSpheresId != null) ? iSpheresId : SphereId + "_" + 1.ToString();
            DataSpheres value = new DataSpheres(_SpheresId);
            SpheresIn = value;
            return value;
        }
        public DataSphere  NewSphere(string iSphereId = null)
        {
            string _SphereId = (iSphereId != null) ? SphereId + "_" + iSphereId : SphereId + "_" + 1.ToString();
            DataSphere value = new DataSphere(_SphereId);
            SphereIn = value;
            return value;
        }

        public int  IndexOf(string TrellName)
        {
            return Trells.IndexOf(TrellName);
        }
        public bool Have(string TrellName)
        {
            return Trells.Have(TrellName);
        }

        public object Collect(string trellName)
        {
            List<DataTrellis> gtab = Trells.AsEnumerable().Where(c => trellName == c.TrellName).ToList();
            if (gtab.Count > 0)
                return gtab.First();
            else
                return null;
        }
        public object[] Collect(string[] trellNames)
        {
            DataTrellis[] gtabs = Trells.AsEnumerable().Where(c => trellNames.Contains(c.TrellName)).ToArray();
            if (gtabs.Length > 0)
                return gtabs;
            else
                return null;
        }

        #region IDataGridBinder
        public object BindingSource
        {
            get
            {
                if (SpheresIn != null)
                    return SpheresIn;
                else
                    return Trells;
            }
        }

        [NonSerialized] private IDataGridStyle gridStyle;
        public IDataGridStyle GridStyle { get { return gridStyle; } set { gridStyle = value; } }
        public IDepotSync DepotSync
        { get { return BoundedGrid.DepotSync; } set { BoundedGrid.DepotSync = value; } }       
        public IDataGridBinder BoundedGrid
        { get { return SpheresOn.BoundedGrid; } set { SpheresOn.BoundedGrid = value; } }

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
            DataTiers[] result =  Trells.AsEnumerable().SelectMany(t => (DataTiers[])t.GetMessage()).ToArray();         
            return result;
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
            return (Config.Path != null) ? Config.Path : (SpheresOn != null) ? SpheresOn.Config.Path + "/" + SphereId : SphereId;
        }
        public string GetMapName()
        {
            return Config.Path + "/" + SphereId + ".sph";
        }
        public int    GetKeyShah()
        {
            return (Config.Path != null) ? Config.Path.Reverse<char>().ToString().GetHashCode() : 0;
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
        public int DeserialCount { get; set; }
        public int ProgressCount { get; set; }
        public int ItemsCount { get { return Trells.Count; } }

        #endregion

        #region IDataMorph
        public object Emulator(object source, string name = null)
        {
            return this.Emulate((DataSphere)source, name);
        }
        public object Imitator(object source, string name = null)
        {
            return this.Imitate((DataSphere)source, name);
        }
        public object Impactor(object source, string name = null)
        {
            DataSphere sph = this;
            if (source != null)
            {
                bool save = ((IDataConfig)source).State.Saved;
                sph = this.Impact((DataSphere)source, save, name);
                DataSpace.PrimeFinder(sph);
            }
            return sph;
        }
        public object Locator(string path = null)
        {
            return this.Locate(path);
        }
        #endregion

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
        public bool Edited
        { get { return State.Edited; } set { State.Edited = value; } }
        public bool Synced
        { get { return State.Synced; } set { State.Synced = value; } }
        public bool Saved
        { get { return State.Saved; } set { State.Saved = value; Trells.AsEnumerable().Select(t => t.Saved = value).ToArray(); } }
        public bool Quered
        { get { return State.Quered; } set { State.Quered = value; } }
        public bool Canceled
        { get { return State.Canceled; } set { State.Canceled = value; } }

        public bool IsDivision
        { get; set; }

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
            this.Impactor((DataSphere)Deserialize(ref read));
            State.Saved = false;
            dc.Dispose();
        }
        public void OpenDrive()
        {
            if(Drive == null)
                Drive = new DriveBank
                    (Config.File, Config.File, 
                     5 * 1024 * 1024, typeof(DataSphere));
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
            if (Drive != null)
                Drive.Close();
            Drive = null;
        }
        #endregion

        #region IDataTreeBinder

        public string TreeNodeName
        {
            get { return SphereId; }
        }
        public object TreeNodeTag
        {
            get { return this; }
        }
        public IDataTreeBinder[] TreeNodeChilds
        {
            get { return (SpheresIn != null) ? SpheresIn.Select(a => (IDataTreeBinder)a.Value).ToArray() : null; }
        }
        public IDataTreeBinder BoundedTree
        { get { return boundedTree; } set { boundedTree = value; } }

        #endregion
    }
}
