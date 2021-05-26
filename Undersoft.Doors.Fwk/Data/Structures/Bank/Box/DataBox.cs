using System.Data;
using System;
using System.Collections.Generic;
using System.Doors.Drive;
using System.IO;
using System.Doors;

namespace System.Doors.Data
{
    [Serializable]
    public class DataBox : INoid, IDataConfig, IDataSerial, IDriveRecorder
    {
        #region Private / NonSerialized
        private IDictionary<string, IDorsEvent> accessorDelegates;
        private IDictionary<string, IDorsEvent> mutatorDelegates;
        [NonSerialized] private IDrive drive;
        [NonSerialized] public DataDeposit DepositOn;
        private DataTrellis prime;
        private string boxType;
        #endregion

        public DataConfig Config
        { get; set; }
        public DataParams Parameters
        { get; set; }
        public DataState State
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

        public string BoxId
        { get; set; }
        public string DisplayName
        { get; set; }

        public DataBox(DataDeposit depositOn)
        {
            DepositOn = depositOn;
            BoxId = depositOn.DepositId;
            DisplayName = BoxId;
            Config = new DataConfig(this, DataStore.Bank);
            State = new DataState();
            Parameters = new DataParams();
            accessorDelegates = new Dictionary<string, IDorsEvent>();
            mutatorDelegates = new Dictionary<string, IDorsEvent>();
        }

        public DataTrellis Prime
        {
            get
            {
                return prime;
            }
            set
            {
                value.Config.SetMapConfig(this.DepositOn);
                value.Tiers.Config.SetMapConfig(this.DepositOn);
                value.Tiers.Registry.Config.SetMapConfig(this.DepositOn);
                value.Tiers.Registry.OpenDrive();
                prime = value;
                if(prime.Deposit == null)
                    prime.Deposit = DepositOn;
            }
        }

        public Type BoxType
        {
            get
            {
                if (boxType != null)
                    return Type.GetType(boxType);
                return null;
            }
            set
            {
                boxType = value.FullName;
            }
        }

        public IDorsEvent GetAccessorDelegate(string name)
        {
            IDorsEvent result = null;
            accessorDelegates.TryGetValue(name, out result);
            return result;
        }      
        public IDorsEvent SetAccessorDelegate(string name, IDorsEvent setEvent)
        {
            IDorsEvent result = null;
            if (accessorDelegates.ContainsKey(name))
                accessorDelegates[name] = setEvent;
            else
                accessorDelegates.Add(name, setEvent);
            return result;
        }

        public IDorsEvent GetMutatorDelegate(string name)
        {
            IDorsEvent result = null;
            mutatorDelegates.TryGetValue(name, out result);
            return result;
        }
        public IDorsEvent SetMutatorDelegate(string name, IDorsEvent setEvent)
        {
            IDorsEvent result = null;
            if (mutatorDelegates.ContainsKey(name))
                mutatorDelegates[name] = setEvent;
            else
                mutatorDelegates.Add(name, setEvent);
            return result;
        }

        public object GetPrime(string name, params object[] Parameters)
        {
            if (accessorDelegates.Count > 0)
            {
                if (BoxType == null)
                    BoxType = typeof(DataTrellis);

                if (Prime != null)
                    if (Parameters.Length > 0)
                        GetAccessorDelegate(name).Execute(Parameters);
                    else
                        GetAccessorDelegate(name).Execute();
                else
                    Prime = (DataTrellis)(GetAccessorDelegate(name).Execute());
                return Prime;
            }
            return null;
        }
        public object GetPrime(string name, DorsInvokeInfo delegateParameters)
        {
            if (BoxType == null)
                BoxType = typeof(DataTrellis);
            SetAccessorDelegate(name, new DataEvent(delegateParameters));
            Prime = (DataTrellis)(GetAccessorDelegate(name).Execute());
            return Prime;
        }

        public object SetPrime(string name, params object[] Parameters)
        {
            if (mutatorDelegates.Count > 0)
            {
                if (Parameters.Length > 0)
                    GetMutatorDelegate(name).Execute(Parameters);
                else
                    GetMutatorDelegate(name).Execute();
                return Prime;
            }
            return null;
        }
        public object SetPrime(string name, DorsInvokeInfo delegateParameters)
        {
            if (BoxType == null)
                BoxType = typeof(DataTrellis);
            SetMutatorDelegate(name, new DataEvent(delegateParameters));
            GetMutatorDelegate(name).Execute();
            return Prime;
        }

        #region Serialization
        public int Serialize(Stream tostream, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        {
            return prime.SetRaw(tostream);
        }
        public int Serialize(ISerialContext buffor, int offset, int batchSize, SerialFormat serialFormat = SerialFormat.Raw)
        {
            return prime.SetRaw(buffor);
        }

        public object Deserialize(Stream fromstream, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        {
            return prime.GetRaw(fromstream);
        }
        public object Deserialize(ref object fromarray, bool driveon = true, SerialFormat serialFormat = SerialFormat.Raw)
        {
            return prime.GetRaw(ref fromarray);
        }

        public object[] GetMessage()
        {
            return Prime != null ?
                   new DataTiers[] { Prime.Tiers } : null;
        }
        public object GetHeader()
        {
            return prime;
        }

        public int ItemsCount
        { get { return prime.Count; } }
        public int SerialCount
        { get; set; }
        public int DeserialCount
        { get; set; }
        public int ProgressCount
        { get; set; }

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
            return Config.Path ?? ((DepositOn != null) ? DepositOn.Config.Path : BoxId );
        }
        public string GetMapName()
        {
            return Config.Path + "/" + BoxId + ".box";
        }
        public ushort GetDriveId()
        {
            return 0;
        }
        #endregion

        #region IDriveRecorder       
        public IDrive Drive
        { get { return drive; } set { drive = value; } }
        public void WriteDrive()
        {
            if (Prime != null)
            {
                Prime.State.Saved = false;
                if (Drive == null) OpenDrive();
                DriveContext dc = new DriveContext();
                Prime.Serialize(dc, 0, 0);
                dc.WriteDrive(Drive);
                dc.Dispose();
            }
        }
        public void ReadDrive()
        {
            if (Prime != null)
            {
                if (Drive == null) OpenDrive();
                DriveContext dc = new DriveContext();
                object read = dc.ReadDrive(Drive);
                Prime.Impact((DataTrellis)Deserialize(ref read));
                State.Saved = false;
                dc.Dispose();
            }
        }
        public void OpenDrive()
        {
            if (Drive == null && Prime != null)
                Drive = new DriveBank(Config.File, Config.File, 10 * 1024 * 1024, typeof(DataTrellis));
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


}
