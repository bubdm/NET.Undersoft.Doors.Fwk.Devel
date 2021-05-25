using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Dors;
using System.Reflection;

namespace System.Dors.Data.Depot
{
    [Serializable]
    public class TransactionMessage : IDataSerial, IDisposable
    {
        [NonSerialized]
        private DepotTransaction transaction;

        private DirectionType direction;    

        public TransactionMessage()
        {
            content = new object();
            SerialCount = 0;
            DeserialCount = 0;
            direction = DirectionType.Receive;
        }
        public TransactionMessage(DepotTransaction _transaction, DirectionType _direction, object message = null)
        {
            transaction = _transaction;
            direction = _direction;

            if (message != null)
                Content = message;
            else
                content = new object();

            SerialCount = 0;
            DeserialCount = 0;
        }

        private object content;
        public object Content
        {
            get { return content; }
            set { transaction.Manager.MessageContent(ref content, value, direction); }
        }

        public string Notice
        { get; set; }

        public void Dispose()
        {
            content = null;
        }

        #region IDataSerial Serialization

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
            if(content != null)
                return (IDataSerial[])content;
            return null;
        }
        public object GetHeader()
        {
            if (direction == DirectionType.Send)
                return transaction.MyHeader.Content;
            else
                return transaction.HeaderReceived.Content;
        }

        public int SerialCount { get; set; }
        public int DeserialCount { get; set; }
        public int ProgressCount { get; set; }
        public int ItemsCount { get { return (content != null) ? ((IDataSerial[])content).Sum(t => t.ItemsCount): 0; } }
        public int ObjectsCount { get { return (content != null) ? ((IDataSerial[])content).Length : 0;  } }

        #endregion
    }

    public static class RawMessage
    {
        public static int SetRaw(this TransactionMessage bank, Stream tostream)
        {
            if (tostream == null) tostream = new MemoryStream();
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(tostream, bank);
            return (int)tostream.Length;
        }
        public static int SetRaw(this TransactionMessage bank, ISerialContext tostream)
        {
            int offset = tostream.BlockOffset;
            MemoryStream ms = new MemoryStream();
            ms.Write(new byte[offset], 0, offset);
            BinaryFormatter binform = new BinaryFormatter();
            binform.Serialize(ms, bank);
            tostream.SerialBlock = ms.ToArray();
            ms.Dispose();
            return tostream.SerialBlock.Length;
        }
        public static TransactionMessage GetRaw(this TransactionMessage bank, Stream fromstream)
        {
            try
            {
                BinaryFormatter binform = new BinaryFormatter();
                return (TransactionMessage)binform.Deserialize(fromstream);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static TransactionMessage GetRaw(this TransactionMessage bank, ref object fromarray)
        {
            try
            {
                MemoryStream ms = new MemoryStream((byte[])fromarray);
                BinaryFormatter binform = new BinaryFormatter();
                TransactionMessage _bank = (TransactionMessage)binform.Deserialize(ms);
                ms.Dispose();
                return _bank;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class JsonMessage
    {
        public static int SetJson(this TransactionMessage tmsg, StringBuilder stringbuilder)
        {
            stringbuilder.AppendLine(tmsg.SetJsonString());
            return stringbuilder.Length;
        }
        public static int SetJson(this TransactionMessage tmsg, Stream tostream)
        {
            BinaryWriter binwriter = new BinaryWriter(tostream);
            binwriter.Write(tmsg.SetJsonString());
            return (int)tostream.Length;
        }
        public static int SetJson(this TransactionMessage tmsg, ISerialContext buffor)
        {
            buffor.SerialBlock = Encoding.UTF8.GetBytes(tmsg.SetJsonString());
            return buffor.SerialBlock.Length;
        }

        public static string SetJsonString(this TransactionMessage tmsg)
        {
            IDictionary<string, object> toJson = tmsg.SetJsonBag();
            return DataJson.ToJson(toJson);
        }
        public static string SetJsonString(this TransactionMessage tmsg, IDictionary<string, object> jsonbag)
        {
            return DataJson.ToJson(jsonbag);
        }

        public static Dictionary<string, object> SetJsonBag(this TransactionMessage tmsg)
        {
            return new Dictionary<string, object>() { { "DepotMessage", DataJsonProperties.GetJsonProperties(typeof(TransactionMessage))
                                                                       .Select(k => new KeyValuePair<string, object>(k.Name, k.GetValue(tmsg, null)))
                                                                       .ToDictionary(k => k.Key, v => v.Value) } };
        }

        public static Dictionary<string, TransactionMessage> GetJsonObject(this TransactionMessage tmsg, IDictionary<string, object> _bag)
        {
            Dictionary<string, TransactionMessage> result = new Dictionary<string, TransactionMessage>();
            IDictionary<string, object> bags = _bag;
            foreach (KeyValuePair<string, object> bag in bags)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(TransactionMessage));
                DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                TransactionMessage trell = (TransactionMessage)inst;
                result.Add(bag.Key, trell);
            }
            return result;
        }
        public static Dictionary<string, TransactionMessage> GetJsonObject(this TransactionMessage tmsg, string JsonString)
        {
            Dictionary<string, TransactionMessage> result = new Dictionary<string, TransactionMessage>();
            Dictionary<string, object> bags = new Dictionary<string, object>();
            tmsg.GetJsonBag(JsonString, bags);

            foreach (KeyValuePair<string, object> bag in bags)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(TransactionMessage));
                DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                TransactionMessage trell = (TransactionMessage)inst;
                result.Add(bag.Key, trell);
            }
            return result;
        }

        public static void GetJsonBag(this TransactionMessage tmsg, string JsonString, IDictionary<string, object> _bag)
        {
            _bag.AddRange(DataJson.FromJson(JsonString));
        }

        public static TransactionMessage GetJson(this TransactionMessage tmsg, string jsonstring)
        {
            try
            {
                TransactionMessage trs = tmsg.GetJsonObject(jsonstring)["DepotMessage"];
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static TransactionMessage GetJson(this TransactionMessage tmsg, StringBuilder stringbuilder)
        {
            try
            {
                StringBuilder sb = stringbuilder;
                TransactionMessage trs = tmsg.GetJsonObject(sb.ToString())["DepotMessage"];
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static TransactionMessage GetJson(this TransactionMessage tmsg, Stream fromstream)
        {
            try
            {
                fromstream.Position = 0;
                byte[] array = new byte[4096];
                int read = 0;
                StringBuilder sb = new StringBuilder();
                while ((read = fromstream.Read(array, 0, array.Length)) > 0)
                {
                    sb.Append(array.Cast<char>());
                }
                TransactionMessage trs = tmsg.GetJsonObject(sb.ToString())["DepotMessage"];
                sb = null;
                fromstream.Dispose();
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static TransactionMessage GetJson(this TransactionMessage tmsg, ref object fromarray)
        {
            try
            {
                TransactionMessage trs = null;
                if (fromarray is String)
                {
                    trs = tmsg.GetJsonObject((String)fromarray)["DepotMessage"];
                }
                else
                {
                    byte[] _fromarray = (byte[])fromarray;
                    StringBuilder sb = new StringBuilder();

                    sb.Append(_fromarray.ToChars(CharCoding.UTF8));
                    trs = tmsg.GetJsonObject(sb.ToString())["DepotMessage"];

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
