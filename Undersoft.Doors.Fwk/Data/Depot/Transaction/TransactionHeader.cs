using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Doors;

namespace System.Doors.Data.Depot
{

    [Serializable]
    public class TransactionHeader : IDataSerial, IDisposable
    {
        [NonSerialized]
        private DepotTransaction transaction;

        public TransactionHeader()
        {
            Context = new TransactionContext();
            SerialCount = 0; DeserialCount = 0;
        }
        public TransactionHeader(DepotTransaction _transaction)
        {
            Context = new TransactionContext();          
            transaction = _transaction;
            SerialCount = 0; DeserialCount = 0;
        }
        public TransactionHeader(DepotTransaction _transaction, DepotIdentity identity)
        {
            Context = new TransactionContext();
            Context.Identity = identity;
            Context.IdentitySite = identity.Site;
            transaction = _transaction;
            SerialCount = 0; DeserialCount = 0;
        }
        public TransactionHeader(DepotTransaction _transaction, IDepotContext context)
        {
            Context = new TransactionContext();
            Context.LocalEndPoint = (IPEndPoint)context.SocketEar.LocalEndPoint;
            Context.RemoteEndPoint = (IPEndPoint)context.SocketEar.RemoteEndPoint;
            transaction = _transaction;
            SerialCount = 0; DeserialCount = 0;
        }
        public TransactionHeader(DepotTransaction _transaction, IDepotContext context, DepotIdentity identity)
        {
            Context = new TransactionContext();
            Context.LocalEndPoint = (IPEndPoint)context.SocketEar.LocalEndPoint;
            Context.RemoteEndPoint = (IPEndPoint)context.SocketEar.RemoteEndPoint;
            Context.Identity = identity;          
            Context.IdentitySite = identity.Site;
            transaction = _transaction;
            SerialCount = 0; DeserialCount = 0;
        }

        public void BindContext(IDepotContext context)
        {
            Context.LocalEndPoint = (IPEndPoint)context.SocketEar.LocalEndPoint;
            Context.RemoteEndPoint = (IPEndPoint)context.SocketEar.RemoteEndPoint;                     
        }       
      
        public TransactionContext Context
        { get; set; }

        public object Content
        {
            get;
            set;
        }

        public void Dispose()
        {
            Content = null;
        }

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
            return null;
        }
        public object GetHeader()
        {
            return this;
        }

        public int SerialCount { get; set; }
        public int DeserialCount { get; set; }
        public int ProgressCount { get; set; }
        public int ItemsCount { get { return Context.ObjectsCount; } }

        #endregion
    }

    public static class RawHeader
    {
        public static int SetRaw(this TransactionHeader bank, Stream tostream)
        {
            if (tostream == null) tostream = new MemoryStream();
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(tostream, bank);
            return (int)tostream.Length;
        }
        public static int SetRaw(this TransactionHeader bank, ISerialContext tostream)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(new byte[tostream.BlockOffset], 0, tostream.BlockOffset);
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(ms, bank);
            tostream.SerialBlock = ms.ToArray();
            ms.Dispose();
            return tostream.SerialBlock.Length;
        }
        public static TransactionHeader GetRaw(this TransactionHeader bank, Stream fromstream)
        {
            try
            {
                BinaryFormatter binform = new BinaryFormatter();
                return (TransactionHeader)binform.Deserialize(fromstream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static TransactionHeader GetRaw(this TransactionHeader bank, ref object fromarray)
        {
            try
            {
                MemoryStream ms = new MemoryStream((byte[])fromarray);
                BinaryFormatter binform = new BinaryFormatter();
                TransactionHeader _bank = (TransactionHeader)binform.Deserialize(ms);
                ms.Dispose();
                return _bank;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class JsonHeader
    {
        public static int SetJson(this TransactionHeader thdr, StringBuilder stringbuilder)
        {
            stringbuilder.AppendLine(thdr.SetJsonString());
            return stringbuilder.Length;
        }
        public static int SetJson(this TransactionHeader thdr, Stream tostream)
        {
            BinaryWriter binwriter = new BinaryWriter(tostream);
            binwriter.Write(thdr.SetJsonString());
            return (int)tostream.Length;
        }
        public static int SetJson(this TransactionHeader thdr, ISerialContext buffor, int offset = 0)
        {
            if (offset > 0)
            {
                byte[] jsonBytes = Encoding.UTF8.GetBytes(thdr.SetJsonString());
                byte[] serialBytes = new byte[jsonBytes.Length + offset];
                jsonBytes.CopyTo(serialBytes, offset);
                buffor.SerialBlock = serialBytes;
                jsonBytes = null;
            }
            else
                buffor.SerialBlock = Encoding.UTF8.GetBytes(thdr.SetJsonString());

            return buffor.SerialBlock.Length;
        }

        public static string SetJsonString(this TransactionHeader thdr)
        {
            IDictionary<string, object> toJson = thdr.SetJsonBag();
            return DataJson.ToJson(toJson, thdr.Context.Complexity);
        }
        public static string SetJsonString(this TransactionHeader thdr, IDictionary<string, object> jsonbag)
        {
            return DataJson.ToJson(jsonbag, thdr.Context.Complexity);
        }

        public static IDictionary<string, object> SetJsonBag(this TransactionHeader thdr)
        {
            return new Dictionary<string, object>() { { "DepotHeader", DataJsonProperties.GetJsonProperties(typeof(TransactionHeader), thdr.Context.Complexity)
                                                                       .Select(k => new KeyValuePair<string, object>(k.Name, k.GetValue(thdr, null)))
                                                                       .ToDictionary(k => k.Key, v => v.Value) } };
        }

        public static Dictionary<string, TransactionHeader> GetJsonObject(this TransactionHeader thdr, IDictionary<string, object> _bag)
        {
            Dictionary<string, TransactionHeader> result = new Dictionary<string, TransactionHeader>();
            IDictionary<string, object> bags = _bag;
            foreach (KeyValuePair<string, object> bag in bags)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(TransactionHeader));
                DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                TransactionHeader trell = (TransactionHeader)inst;
                result.Add(bag.Key, trell);
            }
            return result;
        }
        public static Dictionary<string, TransactionHeader> GetJsonObject(this TransactionHeader thdr, string JsonString)
        {
            Dictionary<string, TransactionHeader> result = new Dictionary<string, TransactionHeader>();
            Dictionary<string, object> bags = new Dictionary<string, object>();
            thdr.GetJsonBag(JsonString, bags);

            foreach (KeyValuePair<string, object> bag in bags)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(TransactionHeader));
                DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                TransactionHeader trell = (TransactionHeader)inst;
                result.Add(bag.Key, trell);
            }
            return result;
        }

        public static void GetJsonBag(this TransactionHeader thdr, string JsonString, IDictionary<string, object> _bag)
        {
            _bag.AddRange(DataJson.FromJson(JsonString));
        }

        public static TransactionHeader GetJson(this TransactionHeader thdr, string jsonstring)
        {
            try
            {
                TransactionHeader trs = thdr.GetJsonObject(jsonstring)["DepotHeader"];
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static TransactionHeader GetJson(this TransactionHeader thdr, StringBuilder stringbuilder)
        {
            try
            {
                StringBuilder sb = stringbuilder;
                TransactionHeader trs = thdr.GetJsonObject(sb.ToString())["DepotHeader"];
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static TransactionHeader GetJson(this TransactionHeader thdr, Stream fromstream)
        {
            try
            {
                fromstream.Position = 0;
                byte[] array = new byte[4096];
                int read = 0;
                StringBuilder sb = new StringBuilder();
                while ((read = fromstream.Read(array, 0, array.Length)) > 0)
                {
                    sb.Append(array.Select(b => (char)b).ToArray());
                }
                TransactionHeader trs = thdr.GetJsonObject(sb.ToString())["DepotHeader"];
                sb = null;
                fromstream.Dispose();
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static TransactionHeader GetJson(this TransactionHeader thdr, ref object fromarray)
        {
            try
            {
                TransactionHeader trs = null;
                if (fromarray is String)
                {
                    trs = thdr.GetJsonObject((String)fromarray)["DepotHeader"];
                }
                else
                {
                    byte[] _fromarray = (byte[])fromarray;
                    StringBuilder sb = new StringBuilder();

                    sb.Append(_fromarray.ToChars(CharCoding.UTF8));
                    trs = thdr.GetJsonObject(sb.ToString())["DepotHeader"];

                    fromarray = null;
                    _fromarray = null;
                    sb = null;
                }
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

}
