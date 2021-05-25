using System.IO;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Dors.Drive;
using System.Dors;

namespace System.Dors.Data
{
    [Serializable]
    public class DataDeposit : IDataMorph, IDataSerial, INoid, IDataConfig, IDataGridBinder, IListSource, IDriveRecorder
    {
        #region Private / NonSerialized
        private SqlIdentity sqlSource;
        [NonSerialized] private IDrive drive;
        [NonSerialized] private DataVault vault;
        #endregion

        public DataConfig Config
        { get; set; }
        public DataParams Parameters
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

        public IDrive Drive
        { get { return drive; } set { drive = value; } }

        public string DepositId
        { get; set; }
        public string DisplayName
        { get; set; }
        public string DataIdx
        { get { return Config.DataIdx; } }

        public DataDeposit(string _DataPlace, DataVault vaultOn, bool useSqlLocal = true)
        {
            vault = vaultOn;
            DepositId = _DataPlace;
            DisplayName = _DataPlace;
            Config = new DataConfig(this, DataStore.Bank);         
            State = new DataState();
            Parameters = new DataParams();
            Box = new DataBox(this);
        }

        public DataTrellis Trell
        {
            get
            {
                return Box.Prime; 
            }
            set
            {
                Box.Prime = value;
            }
        }

        public DataBox Box
        { get; set; }

        public int Count
        { get { return Box.Prime != null ? 
                      (Box.Prime).Tiers != null ? 
                      (Box.Prime).Tiers.Count : 0 : 0; } }
       
        public DataDepositType DepositType
        { get; set; }

        public DataVault Vault
        { get { return vault; } set { vault = value; } }   

        #region IDataGridBinder
        public object BindingSource { get { return this.Box.Prime; } }
        public IDepotSync DepotSync { get { return BoundedGrid.DepotSync; } set { BoundedGrid.DepotSync = value; } }
        public IDataGridStyle GridStyle { get; set; }
        public IDataGridBinder BoundedGrid { get { return Vault.BoundedGrid; } set { Vault.BoundedGrid = value; } }

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
            return Box.Prime != null ?                                     
                   new DataTiers[] { Box.Prime.Tiers } : null; 
        }
        public object   GetHeader()
        {
            return this;
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
        public string GetMapPath()
        {
            return Config.Path ?? ((Vault != null) ? Vault.Config.Path + "/" + DepositId : DepositId);
        }
        public string GetMapName()
        {
            return Vault.Config.Path + "/" + DepositId + ".dpst";
        }      
        public ushort GetDriveId()
        {
            return 0;
        }
        public ushort GetStateBits()
        {
            return State.ToUInt16();
        }

        public int ItemsCount
        { get { return Count; } }
        public int SerialCount
        { get; set; }
        public int DeserialCount
        { get; set; }
        public int ProgressCount
        { get; set; }
        #endregion

        #region  IDataMorph
        public object Emulator(object source, string name = null)
        {
            throw new NotImplementedException();
        }
        public object Imitator(object source, string name = null)
        {
            throw new NotImplementedException();
        }
        public object Impactor(object source, string name = null)
        {
            if (this.Box.Prime == null)
                this.Box.Prime = new DataTrellis();

           return this.Impact((DataDeposit)source, true, DataGroup.Bussines);
        }
        public object Locator(string path = null)
        {
            return this.Locate(path);
        }
        #endregion

        #region  SqlAfector

        public string SqlQuery
        { get; set; }

        public SqlIdentity SqlSource
        {
            get
            {
                return (Config.Store != DataStore.Config && sqlSource == null) ?
                        sqlSource = SqlConfig.GetSqlIdentity() :
                        sqlSource;
            }
            set { sqlSource = value; }
        }

        #region  SqlAccessor

        public DataTrellis SqlTrellis(string TableName = null, 
                                      string sqlQuery = null, 
                                      IList<string> KeyNames = null, 
                                      IList<string> IndexNames = null, 
                                      bool isCube = false)
        {
            try
            {
                Box.SetAccessorDelegate(SqlSource.AuthId, new DataEvent(new DorsInvokeInfo("GetSqlTrellis", "System.Dors.Data.Afectors.Sql.SqlAccessor",
                                                    SqlSource.ConnectionString,
                                                    (sqlQuery != null) ? sqlQuery : this.SqlQuery,
                                                    TableName,
                                                    null,
                                                    KeyNames,
                                                    IndexNames,
                                                    false,
                                                    isCube)));

                Box.GetPrime(SqlSource.AuthId, SqlSource.ConnectionString,
                            (sqlQuery != null) ? sqlQuery : this.SqlQuery,
                            TableName,
                            this,
                            KeyNames,
                            IndexNames,
                            false,
                            isCube);
                Box.Prime.Deposit = Box.DepositOn;
                Box.Prime.AfectMap = new List<AfectMapping>() { new AfectMapping(TableName) };
                return Box.Prime;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public DataTrellis SqlTrellis(DorsInvokeInfo dataDelegateParameters, 
                                      string TableName = null, 
                                      string sqlQuery = null, 
                                      IList<string> KeyNames = null, 
                                      IList<string> IndexNames = null,
                                      bool isCube = false)
        {
            dataDelegateParameters.Parameters = ParamsTool.New(SqlSource.ConnectionString,
                                                (sqlQuery != null) ? sqlQuery : this.SqlQuery,
                                                TableName,
                                                null,
                                                KeyNames,
                                                IndexNames,
                                                false,
                                                isCube);
            try
            {
                Box.SetAccessorDelegate(SqlSource.AuthId, new DataEvent(dataDelegateParameters));
                Box.GetPrime(SqlSource.AuthId, SqlSource.ConnectionString,
                            (sqlQuery != null) ? sqlQuery : this.SqlQuery,
                            TableName,
                            this,
                            KeyNames,
                            IndexNames,
                            false,
                            isCube);
                Box.Prime.Deposit = Box.DepositOn;
                return Box.Prime;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTrellis SqlTrellis()
        {
            if (Box.GetAccessorDelegate(SqlSource.Name) != null)
            {
                Box.GetPrime(SqlSource.AuthId, SqlSource.ConnectionString,
                             this.SqlQuery,
                             Box.Prime.TrellName,
                             this,
                             null,
                             null,
                             true,
                             Box.Prime.IsCube);
                Box.Prime.Deposit = this;
            }
            else
            {
                Box.SetAccessorDelegate(SqlSource.AuthId, new DataEvent(new DorsInvokeInfo("GetSqlTrellis", "System.Dors.Data.Afectors.Sql.SqlAccessor",
                                                    SqlSource.ConnectionString,
                                                    this.SqlQuery,
                                                    Box.Prime.TrellName,
                                                    null,
                                                    null,
                                                    null,
                                                    true,
                                                    Box.Prime.IsCube)));
                Box.GetPrime(SqlSource.AuthId, SqlSource.ConnectionString,
                             this.SqlQuery,
                             Box.Prime.TrellName,
                             this,
                             null,
                             null,
                             true,
                             Box.Prime.IsCube);
                Box.Prime.Deposit = this;
            }
            return Box.Prime;
        }

        #endregion

        #region SqlMutator

        public DataTier[] TiersToSql(DataTier[] tiers = null, bool renew = false, bool isCube = false)
        {
            try
            {
                if (Box.GetMutatorDelegate(SqlSource.AuthId) == null)
                    Box.SetMutatorDelegate(SqlSource.AuthId, new DataEvent(new DorsInvokeInfo("SetSqlTiers", "System.Dors.Data.Afectors.Sql.SqlMutator")));

                Box.SetPrime(SqlSource.AuthId, SqlSource.ConnectionString, tiers == null ? this.Trell.Tiers.AsArray() : tiers, renew, isCube);

                return tiers;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #endregion

        #region IDriveRecorder
        public void WriteDrive()
        {
            State.Saved = false;
            DriveContext dc = new DriveContext();
            Serialize(dc, 0, 0);
            dc.WriteDrive(Drive);
            dc.Dispose();
        }
        public void ReadDrive()
        {
            DriveContext dc = new DriveContext();
            object read = dc.ReadDrive(Drive);
            DataDeposit dpst = (DataDeposit)Deserialize(ref read);
            this.Impact(dpst);
            State.Saved = false;
            dc.Dispose();
        }
        public void OpenDrive()
        {
            if (Drive == null)
                Drive = new DriveBank(Config.File, Config.File, 10 * 1024 * 1024, typeof(DataDeposit));
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
    }   

    [Serializable]
    public enum DataDepositType
    {
        Bussines,
        Schema,
        Statistics,
        Settings,
        Notifacations,
        Events,
    }
}
