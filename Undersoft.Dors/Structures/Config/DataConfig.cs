using System;
using System.IO;
using System.Linq;

namespace System.Dors
{
    [JsonObject]
    [Serializable]
    public class DataConfig
    {
        private object data;
        private string target;
        private string dataIdx;
        private int    dataid;
        public string  nameup;
         
         public DepotIdentity ServiceIdentity
         { get { return DataCore.Bank.ServiceIdentity; } }
         public DepotIdentity ServerIdentity
        { get { return DataCore.Bank.ServerIdentity; } }
         public SqlIdentity SourceIdentity
        { get; set; }

        public DataStore Store = DataStore.Space;

        public DataConfig()
        {
            Store = DataStore.Space;
        }
        public DataConfig(DataStore _store)
        {
            Store = _store;
        }
        public DataConfig(object _data, DataStore _store = DataStore.Space)
        {
            data = _data;
            Store = _store;
        }

        public string Disk
        {
            get
            {
                switch (Store)
                {
                    case DataStore.Bank:
                        return DataCore.Bank.DrivePath;
                    case DataStore.Space:
                        return DataCore.Space.DrivePath;
                    case DataStore.Config:
                        return "QDFS";
                    case DataStore.System:
                        return "QDFS";
                    default:
                        return DataCore.Space.DrivePath;
                }
            }
        }
        public string File
        { get { return (Disk != "") ? Disk + "/" + Place : Place; } }
        public string Path
        {
            get; set;
        }
        public string Place
        {
            get
            {
                if (target != null && target != string.Empty)
                    return target;
                else if (data != null)
                    return target = ((INoid)data).GetMapName();
                else
                    return null;
            }
            set
            {
                target = value;
            }
        }
        public string DataType
        { get { if (data != null)  return data.GetType().FullName; else return this.GetType().FullName; } set { } }

        public int    DataId
        {
            get
            {
                return GetDataId();
            }
        }
        public string DataIdx
        {
            get
            {
                return GetDataIdx();
            }
            set
            {
                dataIdx = value;
            }
        }

        public Quid DepotId
        {
            get;
            set;
        }
        public Quid AuthId
        { get; set; }

        public string DepotIdx
        {
            get { return DepotId.ToString();  }
            set { DepotId = value != null && value.Trim(' ') != "" ? new Quid(value) : Quid.Empty; }
        }       
        public string AuthIdx
        {
            get { return AuthId.ToString(); }
            set { AuthId = new Quid(value); }
        }

        public void SetMapConfig(object owner,
                                 string prefix = "",
                                 bool mkdir = true)
        {
            IDataConfig parent = (IDataConfig)owner;

            if (parent.Config.Store == DataStore.Config ||
                parent.Config.Store == DataStore.System)
                Store = parent.Config.Store;

            string target = "";
            string path = ((INoid)data).GetMapPath();

            if (path.Contains('/'))
                target = path.Split('/').Last();
            else
                target = path;

            string _target = "";
            string _path = parent.Config.Path;

            if (_path == null && parent.Config.Place != null)
            {
                string[] repaths = parent.Config.Place.Split(
                    new string[] { ("/" + parent.Config.Place.Split('/').Last()) },
                                        StringSplitOptions.RemoveEmptyEntries);
                _path = (repaths.Length > 0) ? repaths[0] : parent.Config.Place;
            }
            if (_path.Contains('/'))
                _target = parent.Config.Path.Split('/').Last();
            else
                _target = parent.Config.Path;

            if (prefix != "")
                prefix = "/" + prefix;

            if (!_target.Equals(target))
                Path = parent.Config.Path + prefix + "/" + target;
            else
                Path = parent.Config.Path + prefix;

            nameup = _target;

            DepotId = parent.Config.DepotId;

            SetMapFile(mkdir);

            if (Store == DataStore.Space)
            {
                DataCore.Space.Registry.TryAdd(GetDataId(), data);
                if (owner.GetType().Name == "DataDeposit")
                    DataCore.Bank.Registry.TryAdd(GetDataId(), data);
            }
            else
            {
                DataCore.Bank.Registry.TryAdd(GetDataId(), data);
            }
        }

        public void SetMapFile(bool mkdir = true)
        {
            Path = ((INoid)data).GetMapPath();
            Place = ((INoid)data).GetMapName();
            if (mkdir)
                MakeDirectory();
        }

        public void MakeDirectory()
        {
            Directory.CreateDirectory((Disk != "") ? Disk + "/" + Path : Path);
        }
        public void DeleteDirectory()
        {
            Directory.Delete((Disk != "") ? Disk + "/" + Path : Path, true);
        }

        public string GetDataIdx()
        {
            if (dataIdx == null)
            {
                GetDataId();
            }
            return dataIdx;
        }
        public int    GetDataId()
        {
            if (dataIdx == null)
            {
                byte[] shah = ((INoid)data).GetShah();
                dataIdx = shah.ToHex();
                if(Store == DataStore.Bank || (data.GetType().GetInterfaces().Contains(typeof(IDataTrellis)) && 
                    ((IDataTrellis)data).IsPrime))
                {
                    short sid = (short)new int[] { ((short)shah[3] << 8) | ((short)shah[2] << 0), ((short)shah[1] << 8) | shah[0] ,
                               ((short)shah[7] << 8) | ((short)shah[6] << 0) , ((short)shah[5] << 8) | shah[4] }
                             .Aggregate(17, (a, b) => (a + b) * 23);
                    dataid = sid;
                }
                else
                    dataid = new int[] { ((int)shah[3] << 24) | ((int)shah[2] << 16) | ((int)shah[1] << 8) | shah[0] ,
                               ((int)shah[7] << 24) | ((int)shah[6] << 16) | ((int)shah[5] << 8) | shah[4] }
                                .Aggregate(17, (a, b) => (a + b) * 23);
            }
            return dataid;
        }

        public byte[] GetDepotId()
        {
            return (ServerIdentity.Name + ServerIdentity.Ip + ":" + ServerIdentity.Port.ToString()).GetPCHexShah();
        }
        public Quid   GetDepotId(DepotIdentity identity)
        {
            return new Quid((identity.Name + identity.Ip + ":" + identity.Port.ToString()).GetPCHexShah());
        }
        public Quid   GetDepotId(string Name, string Host)
        {
            return new Quid((Name + Host).GetPCHexShah());
        }

        public byte[] GetServiceId()
        {
            return (ServiceIdentity.Name + ServiceIdentity.Host).GetPCHexShah();
        }
        public Quid   GetServiceId(DepotIdentity identity)
        {
            return new Quid((identity.Name + identity.Host).GetPCHexShah());
        }
        public Quid   GetServiceId(string Name, string Host)
        {
            return new Quid((Name + Host).GetPCHexShah());
        }

        public byte[] GetSourceId()
        {
            return (SourceIdentity.Database + SourceIdentity.Server).GetPCHexShah();
        }
        public Quid   GetSourceId(SqlIdentity identity)
        {
            return new Quid((identity.Database + identity.Server).GetPCHexShah());
        }
        public Quid   GetSourceId(string database, string server)
        {
            return new Quid((database + server).GetPCHexShah());
        }     
       
        public Guid   GetUserId(string Name, string Salt)
        {
            byte[] unid = new byte[16];
            if (Name != null)
            {
                byte[] key = Name.GetShah();
                key.CopyTo(unid, 0);
                byte[] host = (BitConverter.ToInt64(Salt.GetShah(), 0).ToString()).GetPCHexShah();
                host.CopyTo(unid, 8);
            }
            return new Guid(unid);
        }    

    }

    public static class DataMode
    {
        public static DataModes ViewMode(this DataModes mode)
        {
            return (mode == DataModes.Tiers || mode == DataModes.TiersView) ? DataModes.TiersView : DataModes.SimsView;
        }

        public static bool IsViewMode(this DataModes mode)
        {
            return (mode == DataModes.TiersView || mode == DataModes.SimsView);
        }

        public static bool IsTiersMode(this DataModes mode)
        {
            return (mode == DataModes.Tiers || mode == DataModes.TiersView);
        }
    }
  
    public enum DataStore
    {
        Bank,
        Space,
        Config,
        System
    }

    public enum DataModes
    {
        Tiers,
        Sims,
        TiersView,
        SimsView,        
        All
    }

    public enum DataEmulate
    {
        Sleeves,
        References,
        None
    }

    public enum RelaySite
    {
        All,
        Parent,
        Child,
        None
    }

    [Serializable]
    public enum DataGroup
    {
        Filter,
        Control,
        Bussines,
        Schema,
        Custom,
        None
    }

    [Serializable]
    public enum SourceProvider
    {
        MsSql,
        DataTable,
        XML,
        JSON,
        None
    }
}