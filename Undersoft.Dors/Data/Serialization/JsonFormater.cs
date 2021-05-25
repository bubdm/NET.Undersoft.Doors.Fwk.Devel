using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Dors;

namespace System.Dors.Data
{
    #region Headers - JsonFormatter   

    public static class JsonArea
    {
        public static int SetJson(this DataArea area, StringBuilder stringbuilder)
        {
            stringbuilder.AppendLine(area.SetJsonString());
            return stringbuilder.Length;
        }
        public static int SetJson(this DataArea area, Stream tostream)
        {
            BinaryWriter binwriter = new BinaryWriter(tostream);
            binwriter.Write(area.SetJsonString());
            return (int)tostream.Length;
        }
        public static int SetJson(this DataArea area, ISerialContext buffor, int offset = 0, DepotComplexity complexity = DepotComplexity.Standard)
        {
            if (offset > 0)
            {
                byte[] jsonBytes = Encoding.UTF8.GetBytes(area.SetJsonString(complexity));
                byte[] serialBytes = new byte[jsonBytes.Length + offset];
                jsonBytes.CopyTo(serialBytes, offset);
                buffor.SerialBlock = serialBytes;
                jsonBytes = null;
            }
            else
                buffor.SerialBlock = Encoding.UTF8.GetBytes(area.SetJsonString(complexity));

            return buffor.SerialBlock.Length;
        }

        public static string SetJsonString(this DataArea area, DepotComplexity complexity = DepotComplexity.Standard)
        {
            IDictionary<string, object> toJson = area.SetJsonBag();
            return DataJson.ToJson(toJson, complexity);
        }

        public static IDictionary<string, object> SetJsonBag(this DataArea area, DepotComplexity complexity = DepotComplexity.Standard)
        {
            return new Dictionary<string, object>() { { area.SpaceName, DataJsonProperties.GetJsonProperties(typeof(DataArea), complexity)
                                                                       .Select(k => new KeyValuePair<string, object>(k.Name, k.GetValue(area, null)))
                                                                       .ToDictionary(k => k.Key, v => v.Value) } };
        }
      
        public static Dictionary<string, DataSpheres> GetJsonObject(this DataArea area, IDictionary<string, object> _bag)
        {
            Dictionary<string, DataSpheres> result = new Dictionary<string, DataSpheres>();
            IDictionary<string, object> bags = _bag;

            foreach (KeyValuePair<string, object> bag in bags)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(DataSpheres));
                DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                DataSpheres trell = (DataSpheres)inst;
                result.Add(bag.Key, trell);
            }
            return result;
        }
        public static Dictionary<string, DataArea> GetJsonObject(this DataArea area, string JsonString)
        {
            Dictionary<string, DataArea> result = new Dictionary<string, DataArea>();
            Dictionary<string, object> bags = new Dictionary<string, object>();
            area.GetJsonBag(JsonString, bags);

            foreach (KeyValuePair<string, object> bag in bags)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(DataArea));
                DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                DataArea trell = (DataArea)inst;
                result.Add(bag.Key, trell);
            }
            return result;
        }

        public static void GetJsonBag(this DataArea area, string JsonString, IDictionary<string, object> _bag)
        {
            _bag.AddRange(DataJson.FromJson(JsonString));
        }

        public static DataArea GetJson(this DataArea area, string jsonstring)
        {
            try
            {
                DataArea trs = area.GetJsonObject(jsonstring)[area.SpaceName];
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataArea GetJson(this DataArea area, StringBuilder stringbuilder)
        {
            try
            {
                StringBuilder sb = stringbuilder;
                DataArea trs = area.GetJsonObject(sb.ToString())[area.SpaceName];
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataArea GetJson(this DataArea area, Stream fromstream)
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
                DataArea trs = area.GetJsonObject(sb.ToString())[area.SpaceName];
                sb = null;
                fromstream.Dispose();
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataArea GetJson(this DataArea area, ref object fromarray)
        {
            try
            {
                byte[] _fromarray = (byte[])fromarray;
                StringBuilder sb = new StringBuilder();

                sb.Append(_fromarray.Select(b => (char)b).ToArray());
                DataArea trs = area.GetJsonObject(sb.ToString())[area.SpaceName];

                fromarray = null;
                _fromarray = null;
                sb = null;
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    public static class JsonSpheres
    {
        public static int SetJson(this DataSpheres spheres, StringBuilder stringbuilder)
        {
            stringbuilder.AppendLine(spheres.SetJsonString());
            return stringbuilder.Length;
        }
        public static int SetJson(this DataSpheres spheres, Stream tostream)
        {
            BinaryWriter binwriter = new BinaryWriter(tostream);
            binwriter.Write(spheres.SetJsonString());
            return (int)tostream.Length;
        }
        public static int SetJson(this DataSpheres spheres, ISerialContext buffor)
        {
            buffor.SerialBlock = Encoding.UTF8.GetBytes(spheres.SetJsonString());
            return buffor.SerialBlock.Length;
        }

        public static string SetJsonString(this DataSpheres spheres)
        {
            IDictionary<string, object> toJson = spheres.SetJsonBag();
            return DataJson.ToJson(toJson);
        }

        public static IDictionary<string, object> SetJsonBag(this DataSpheres spheres)
        {
            return new Dictionary<string, object>()
            {
                {
                    spheres.SpheresId,
                    DataJsonProperties.GetJsonProperties(typeof(DataSpheres)).Select(k => new KeyValuePair<string, object>(k.Name, k.GetValue(spheres, null))).ToDictionary(k => k.Key, v => v.Value) } };
        }

        public static Dictionary<string, DataSphere> GetJsonObject(this DataSpheres spheres, IDictionary<string, object> _bag)
        {
            Dictionary<string, DataSphere> result = new Dictionary<string, DataSphere>();
            IDictionary<string, object> bags = _bag;
            foreach (KeyValuePair<string, object> bag in bags)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(DataSphere));
                DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                DataSphere trell = (DataSphere)inst;
                result.Add(bag.Key, trell);
            }
            return result;
        }
        public static Dictionary<string, DataSpheres> GetJsonObject(this DataSpheres spheres, string JsonString)
        {
            Dictionary<string, DataSpheres> result = new Dictionary<string, DataSpheres>();
            Dictionary<string, object> bags = new Dictionary<string, object>();
            spheres.GetJsonBag(JsonString, bags);

            foreach (KeyValuePair<string, object> bag in bags)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(DataSpheres));
                DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                DataSpheres trell = (DataSpheres)inst;
                result.Add(bag.Key, trell);
            }
            return result;
        }

        public static void GetJsonBag(this DataSpheres spheres, string JsonString, IDictionary<string, object> _bag)
        {
            _bag.AddRange(DataJson.FromJson(JsonString));
        }

        public static DataSpheres GetJson(this DataSpheres spheres, string jsonstring)
        {
            try
            {
                DataSpheres trs = spheres.GetJsonObject(jsonstring)[spheres.SpheresId];
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataSpheres GetJson(this DataSpheres spheres, StringBuilder stringbuilder)
        {
            try
            {
                StringBuilder sb = stringbuilder;
                DataSpheres trs = spheres.GetJsonObject(sb.ToString())[spheres.SpheresId];
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataSpheres GetJson(this DataSpheres spheres, Stream fromstream)
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
                DataSpheres trs = spheres.GetJsonObject(sb.ToString())[spheres.SpheresId];
                sb = null;
                fromstream.Dispose();
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataSpheres GetJson(this DataSpheres spheres, ref object fromarray)
        {
            try
            {
                byte[] _fromarray = (byte[])fromarray;
                StringBuilder sb = new StringBuilder();

                sb.Append(_fromarray.Select(b => (char)b).ToArray());
                DataSpheres trs = spheres.GetJsonObject(sb.ToString())[spheres.SpheresId];

                fromarray = null;
                _fromarray = null;
                sb = null;
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void GetJsonByInfo(ref object instance, ref object value, PropertyInfo info, bool isBag = true)
        {
            Dictionary<string, object> listobjects = ((Dictionary<string, object>)value);
            if (isBag)
            {                
                SortedDictionary<string, DataSpheres> tempdict = new SortedDictionary<string, DataSpheres>();
                foreach (KeyValuePair<string, object> obj in listobjects)
                {
                    object inst = new object();
                    Dictionary<string, object> subbag = (Dictionary<string, object>)obj.Value;
                    IEnumerable<PropertyInfo> submap = DataJson.PrepareInstance(out inst, typeof(DataSpheres));
                    DataJson.DeserializeType(submap, subbag, inst);
                    tempdict.TryAdd(obj.Key.ToString(), (DataSpheres)inst);
                }
                SortedDictionary<string, DataSpheres> sphrs = (SortedDictionary<string, DataSpheres>)info.GetValue(instance, null);
                sphrs.AddRange(tempdict);
            }
            else
            {
                object inst = new object();
                IEnumerable<PropertyInfo> submap = DataJson.PrepareInstance(out inst, typeof(DataSpheres));
                DataJson.DeserializeType(submap, listobjects, inst);
                info.SetValue(instance, inst, null);
            }
        }
    }

    public static class JsonSphere
    {
        public static int SetJson(this DataSphere sphere, StringBuilder stringbuilder)
        {
            stringbuilder.AppendLine(sphere.SetJsonString());
            return stringbuilder.Length;
        }
        public static int SetJson(this DataSphere sphere, Stream tostream)
        {
            BinaryWriter binwriter = new BinaryWriter(tostream);
            binwriter.Write(sphere.SetJsonString());
            return (int)tostream.Length;
        }
        public static int SetJson(this DataSphere sphere, ISerialContext buffor)
        {
            buffor.SerialBlock = Encoding.UTF8.GetBytes(sphere.SetJsonString());
            return buffor.SerialBlock.Length;
        }

        public static string SetJsonString(this DataSphere sphere)
        {
            IDictionary<string, object> toJson = sphere.SetJsonBag();
            return DataJson.ToJson(toJson);
        }

        public static IDictionary<string, object> SetJsonBag(this DataSphere sphere)
        {
            return new Dictionary<string, object>() { { sphere.SphereId, sphere } };
        }
        public static Dictionary<string, DataSphere> GetJsonObject(this DataSphere trells, IDictionary<string, object> _bag)
        {
            Dictionary<string, DataSphere> result = new Dictionary<string, DataSphere>();
            IDictionary<string, object> bags = _bag;
            if (bags.Count > 1)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(DataSphere));
                DataJson.DeserializeType(map, bags, inst);
                DataSphere trell = (DataSphere)inst;
                result.Add(trell.SphereId, trell);
            }
            else
            {
                foreach (KeyValuePair<string, object> bag in bags)
                {
                    object inst = new object();
                    IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(DataSphere));
                    DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                    DataSphere trell = (DataSphere)inst;
                    result.Add(bag.Key, trell);
                }
            }
            return result;
        }
        public static Dictionary<string, DataSphere> GetJsonObject(this DataSphere sphere, string JsonString)
        {
            Dictionary<string, DataSphere> result = new Dictionary<string, DataSphere>();
            Dictionary<string, object> bags = new Dictionary<string, object>();
            sphere.GetJsonBag(JsonString, bags);

            foreach (KeyValuePair<string, object> bag in bags)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(DataSphere));
                DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                DataSphere trell = (DataSphere)inst;
                result.Add(bag.Key, trell);
            }
            return result;
        }

        public static void GetJsonBag(this DataSphere sphere, string JsonString, IDictionary<string, object> _bag)
        {
            _bag.AddRange(DataJson.FromJson(JsonString));
        }

        public static DataSphere GetJson(this DataSphere sphere, string jsonstring)
        {
            try
            {
                DataSphere trs = sphere.GetJsonObject(jsonstring)[sphere.SphereId];
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataSphere GetJson(this DataSphere sphere, StringBuilder stringbuilder)
        {
            try
            {
                StringBuilder sb = stringbuilder;
                DataSphere trs = sphere.GetJsonObject(sb.ToString())[sphere.SphereId];
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataSphere GetJson(this DataSphere sphere, Stream fromstream)
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
                DataSphere trs = sphere.GetJsonObject(sb.ToString())[sphere.SphereId];
                sb = null;
                fromstream.Dispose();
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataSphere GetJson(this DataSphere sphere, ref object fromarray)
        {
            try
            {
                if (fromarray.GetType() == typeof(byte[]))
                {
                    byte[] _fromarray = (byte[])fromarray;
                    StringBuilder sb = new StringBuilder();

                    sb.Append(_fromarray.Select(b => (char)b).ToArray());
                    DataSphere sph = sphere.GetJsonObject(sb.ToString())[sphere.SphereId];

                    fromarray = null;
                    _fromarray = null;
                    sb = null;
                    return sph;
                }
                else if (fromarray is IDictionary)
                {
                    IDictionary<string, object> _fromarray = (IDictionary<string, object>)fromarray;
                    DataSphere sph = sphere.GetJsonObject(_fromarray).Values.First();
                    _fromarray = null;
                    return sph;
                }
                fromarray = null;
                return new DataSphere();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void GetJsonByInfo(ref object instance, ref object value, PropertyInfo info)
        {
            Dictionary<string, object> listobjects = ((Dictionary<string, object>)value);
            SortedDictionary<string, DataSphere> tempdict = new SortedDictionary<string, DataSphere>();
            foreach (KeyValuePair<string, object> obj in listobjects)
            {
                object inst = new object();
                Dictionary<string, object> subbag = (Dictionary<string, object>)obj.Value;
                IEnumerable<PropertyInfo> submap = DataJson.PrepareInstance(out inst, typeof(DataSphere));
                DataJson.DeserializeType(submap, subbag, inst);
                tempdict.TryAdd(obj.Key.ToString(), (DataSphere)inst);
            }
            SortedDictionary<string, DataSphere> sphr = (SortedDictionary<string, DataSphere>)info.GetValue(instance, null);
            sphr.AddRange(tempdict);
        }
    }

    public static class JsonTrellises
    {
        public static int SetJson(this DataTrellises trells, StringBuilder stringbuilder)
        {
            stringbuilder.AppendLine(trells.SetJsonString());
            return stringbuilder.Length;
        }
        public static int SetJson(this DataTrellises trells, Stream tostream)
        {
            BinaryWriter binwriter = new BinaryWriter(tostream);
            binwriter.Write(trells.SetJsonString());
            return (int)tostream.Length;
        }
        public static int SetJson(this DataTrellises trells, ISerialContext buffor)
        {
            buffor.SerialBlock = Encoding.UTF8.GetBytes(trells.SetJsonString());
            return buffor.SerialBlock.Length;
        }

        public static string SetJsonString(this DataTrellises trells)
        {
            IDictionary<string, object> toJson = trells.SetJsonBag();
            return DataJson.ToJson(toJson);
        }

        public static IDictionary<string, object> SetJsonBag(this DataTrellises trells)
        {
            return trells.AsEnumerable().SelectMany(t => t.SetJsonBag()).ToDictionary(k => k.Key, v => v.Value);
        }

        public static List<DataTrellis> GetJsonObject(this DataTrellises trells, IList<object> _bag)
        {
            List<DataTrellis> result = new List<DataTrellis>();
            foreach (object obj in _bag)
            {
                object inst = new object();
                Dictionary<string, object> subbag = (Dictionary<string, object>)obj;
                IEnumerable<PropertyInfo> submap = DataJson.PrepareInstance(out inst, typeof(DataTrellis));
                DataJson.DeserializeType(submap, subbag, inst);
                DataTrellis trell = (DataTrellis)inst;
                result.Add(trell);
            }        
            return result;
        }
        public static Dictionary<string, DataTrellis> GetJsonObject(this DataTrellises trells, IDictionary<string, object> _bag)
        {
            Dictionary<string, DataTrellis> result = new Dictionary<string, DataTrellis>();
            IDictionary<string, object> bags = _bag;
            foreach (KeyValuePair<string, object> bag in bags)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(DataTrellis));
                DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                DataTrellis trell = (DataTrellis)inst;
                result.Add(bag.Key, trell);
            }
            return result;
        }
        public static Dictionary<string, DataTrellis> GetJsonObject(this DataTrellises trells, string JsonString)
        {
            Dictionary<string, DataTrellis> result = new Dictionary<string, DataTrellis>();
            Dictionary<string, object> bags = new Dictionary<string, object>();
            trells.GetJsonBag(JsonString, bags);

            foreach (KeyValuePair<string, object> bag in bags)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(DataTrellis));
                DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                DataTrellis trell = (DataTrellis)inst;
                result.Add(bag.Key, trell);
            }
            return result;
        }

        public static void GetJsonBag(this DataTrellises trells, string JsonString, IDictionary<string, object> _bag)
        {
            _bag.AddRange(DataJson.FromJson(JsonString));
        }

        public static DataTrellises GetJson(this DataTrellises trells, string jsonstring)
        {
            try
            {
                DataTrellises trs = new DataTrellises();
                List<DataTrellis> tr = trells.GetJsonObject(jsonstring).Values.ToList();

                if (tr.Count > 0)
                {
                    trs.State.Impact(tr[0].State);
                    trs.Config.DepotId = tr[0].Config.DepotId;
                    trs.AddRange(tr);
                }
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataTrellises GetJson(this DataTrellises trells, StringBuilder stringbuilder)
        {
            try
            {
                StringBuilder sb = stringbuilder;
                DataTrellises trs = new DataTrellises();
                List<DataTrellis> tr = trells.GetJsonObject(sb.ToString()).Values.ToList();

                if (tr.Count > 0)
                {
                    trs.State.Impact(tr[0].State);
                    trs.Config.DepotId = tr[0].Config.DepotId;
                    trs.AddRange(tr);
                }
                sb = null;
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataTrellises GetJson(this DataTrellises trells, Stream fromstream)
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
                DataTrellises trs = new DataTrellises();
                List<DataTrellis> tr = trells.GetJsonObject(sb.ToString()).Values.ToList();

                if (tr.Count > 0)
                {
                    trs.State.Impact(tr[0].State);
                    trs.Config.DepotId = tr[0].Config.DepotId;
                    trs.AddRange(tr);
                }
                sb = null;
                fromstream.Dispose();
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataTrellises GetJson(this DataTrellises trells, ref object fromarray)
        {
            try
            {
                DataTrellises trs = new DataTrellises();
                List<DataTrellis> tr = new List<DataTrellis>();
                if (fromarray.GetType() == typeof(byte[]))
                {
                    byte[] _fromarray = (byte[])fromarray;
                    StringBuilder sb = new StringBuilder();

                    sb.Append(_fromarray.Select(b => (char)b).ToArray());
                    tr = trells.GetJsonObject(sb.ToString()).Values.ToList();
                    _fromarray = null;
                    sb = null;
                }
                else if(fromarray is IDictionary)
                {
                    IDictionary<string, object> _fromarray = (IDictionary<string, object>)fromarray;
                    tr = trells.GetJsonObject(_fromarray).Values.ToList();
                    _fromarray = null;                   
                }
                else if (fromarray is IList)
                {
                    IList<object> _fromarray = (IList<object>)fromarray;
                    tr = trells.GetJsonObject(_fromarray);
                    _fromarray = null;
                }               
                if (tr.Count > 0)
                {
                    trs.State.Impact(tr[0].State);
                    trs.Config.DepotId = tr[0].Config.DepotId;
                    trs.AddRange(tr);
                }
                fromarray = null;
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void GetJsonByInfo(ref object instance, ref object value, PropertyInfo info)
        {
            List<object> listobjects = (List<object>)value;
            List<DataTrellis> templist = new List<DataTrellis>();
            foreach (object obj in listobjects)
            {
                object inst = new object();
                Dictionary<string, object> subbag = (Dictionary<string, object>)obj;
                IEnumerable<PropertyInfo> submap = DataJson.PrepareInstance(out inst, typeof(DataTrellis));
                DataJson.DeserializeType(submap, subbag, inst);
                templist.Add((DataTrellis)inst);
            }
            DataTrellises trls = (DataTrellises)info.GetValue(instance, null);
            if (templist.Count > 0)
            {
                trls.State.Impact(templist[0].State);
                trls.Config.DepotId = templist[0].Config.DepotId;
                trls.AddRange(templist);
            }
        }
    }

    public static class JsonTrellis
    {
        public static int SetJson(this DataTrellis trell, StringBuilder stringbuilder)
        {
            stringbuilder.AppendLine(trell.SetJsonString());
            return stringbuilder.Length;
        }
        public static int SetJson(this DataTrellis trell, Stream tostream)
        {
            BinaryWriter binwriter = new BinaryWriter(tostream);
            binwriter.Write(trell.SetJsonString());
            return (int)tostream.Length;
        }
        public static int SetJson(this DataTrellis trell, ISerialContext buffor)
        {
            buffor.SerialBlock = Encoding.UTF8.GetBytes(trell.SetJsonString());
            return buffor.SerialBlock.Length;
        }

        public static string SetJsonString(this DataTrellis trellis)
        {
            IDictionary<string, object> toJson = trellis.SetJsonBag();
            return DataJson.ToJson(toJson);
        }

        public static IDictionary<string, object> SetJsonBag(this DataTrellis trellis)
        {
            return new Dictionary<string, object>() { { trellis.TrellName, trellis } };
        }

        public static Dictionary<string, DataTrellis> GetJsonObject(this DataTrellis trellis, IDictionary<string, object> _bag)
        {
            Dictionary<string, DataTrellis> result = new Dictionary<string, DataTrellis>();
            IDictionary<string, object> bags = _bag;
            if (bags.Count > 1)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(DataTrellis));
                DataJson.DeserializeType(map, bags, inst);
                DataTrellis trell = (DataTrellis)inst;
                result.Add(trell.TrellName, trell);
            }
            else
            {
                foreach (KeyValuePair<string, object> bag in bags)
                {
                    object inst = new object();
                    IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(DataTrellis));
                    DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                    DataTrellis trell = (DataTrellis)inst;
                    result.Add(bag.Key, trell);
                }
            }
            return result;
        }
        public static Dictionary<string, DataTrellis> GetJsonObject(this DataTrellis trellis, string JsonString)
        {
            Dictionary<string, DataTrellis> result = new Dictionary<string, DataTrellis>();
            Dictionary<string, object> bags = new Dictionary<string, object>();
            trellis.GetJsonBag(JsonString, bags);
            foreach(KeyValuePair<string, object> bag in bags)
            {
                object inst = new object();
                IEnumerable<PropertyInfo> map = DataJson.PrepareInstance(out inst, typeof(DataTrellis));
                DataJson.DeserializeType(map, (IDictionary<string, object>)bag.Value, inst);
                DataTrellis trell = (DataTrellis)inst;
                result.Add(bag.Key, trell);
            }
            return result;
        }

        public static void GetJsonBag(this DataTrellis trell, string JsonString, IDictionary<string, object> _bag)
        {
            _bag.AddRange(DataJson.FromJson(JsonString));
        }

        public static DataTrellis GetJson(this DataTrellis trell, string jsonstring)
        {
            try
            {
                DataTrellis trs = trell.GetJsonObject(jsonstring)[trell.TrellName];                
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataTrellis GetJson(this DataTrellis trell, StringBuilder stringbuilder)
        {
            try
            {
                StringBuilder sb = stringbuilder;
                DataTrellis trs = trell.GetJsonObject(sb.ToString())[trell.TrellName];
                sb = null;
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataTrellis GetJson(this DataTrellis trell, Stream fromstream)
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
                DataTrellis trs = trell.GetJsonObject(sb.ToString())[trell.TrellName];
                sb = null;
                fromstream.Dispose();
                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static DataTrellis GetJson(this DataTrellis trell, ref object fromarray)
        {
            try
            {
                DataTrellis trs = new DataTrellis();
                if (fromarray.GetType() == typeof(byte[]))
                {
                    byte[] _fromarray = (byte[])fromarray;
                    StringBuilder sb = new StringBuilder();

                    sb.Append(_fromarray.Select(b => (char)b).ToArray());
                    trs = trell.GetJsonObject(sb.ToString())[trell.TrellName];

                    fromarray = null;
                    _fromarray = null;
                    sb = null;
                }
                else if (fromarray is IDictionary)
                {
                    IDictionary<string, object> _fromarray = (IDictionary<string, object>)fromarray;
                    trs = trell.GetJsonObject(_fromarray).Values.First();
                    _fromarray = null;
                    return trs;
                }             

                return trs;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void GetJsonByInfo(ref object instance, ref object value, PropertyInfo info)
        {
            Dictionary<string, object> listobjects = ((Dictionary<string, object>)value);
            Dictionary<string, DataTrellis> tempdict = new Dictionary<string, DataTrellis>();
            foreach (KeyValuePair<string, object> obj in listobjects)
            {
                object inst = new object();
                Dictionary<string, object> subbag = (Dictionary<string, object>)obj.Value;
                IEnumerable<PropertyInfo> submap = DataJson.PrepareInstance(out inst, typeof(DataTrellis));
                DataJson.DeserializeType(submap, subbag, inst);
                tempdict.Put(obj.Key.ToString(), (DataTrellis)inst);
            }
            info.SetValue(instance, tempdict, null);
        }
    }

    #endregion

    #region Message - JsonStructures

    public static class JsonTiers
    {
        public static int SetJson(this DataTiers tiers, StringBuilder stringbuilder, int offset, int batchSize, DepotComplexity complexity = DepotComplexity.Standard)
        {
            DataTiers view = tiers.Trell.TiersView;
            int page = view.Trell.PagingDetails.Page;
            int pagesize = view.Trell.PagingDetails.PageSize;
            int cachepages = view.Trell.PagingDetails.CachedPages;
            batchSize = pagesize;
            int yLeft = 0;
            int length = tiers.Count;            
            int processed = 0;

            DataTiers cacheTiers = new DataTiers(tiers.Trell, DataMode.ViewMode(tiers.Trell.Mode));
            for (int i = page; i < page + cachepages; i++)
            {
                offset = (i - 1) * pagesize;
                yLeft = length - offset;
                if (yLeft >= 0)
                {
                    int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
                    for (int y = offset; y < offset + yLength; y++)
                    {
                        tiers[y].Page = i;
                        DataTier trs = new DataTier(tiers[y]);
                        trs.Tiers = cacheTiers;
                        trs.State.propagate = cacheTiers.State;
                        cacheTiers.AddRef(trs, true);
                        processed++;
                    }
                }
            }
            stringbuilder.AppendLine(cacheTiers.SetJsonString());
            int nextoffset = (processed < yLeft) ? processed + offset : -1;
            return nextoffset;
        }
        public static int SetJson(this DataTiers tiers, Stream tostream, int offset, int batchSize, DepotComplexity complexity = DepotComplexity.Standard)
        {
            BinaryWriter binwriter = new BinaryWriter(tostream);
            DataTiers view = tiers.Trell.TiersView;
            int page = view.Trell.PagingDetails.Page;
            int pagesize = view.Trell.PagingDetails.PageSize;
            int cachepages = view.Trell.PagingDetails.CachedPages;
            batchSize = pagesize;
            int yLeft = 0;
            int length = tiers.Count;
            int processed = 0;

            DataTiers cacheTiers = new DataTiers(tiers.Trell, DataMode.ViewMode(tiers.Trell.Mode));
            for (int i = page; i < page + cachepages; i++)
            {
                offset = (i - 1) * pagesize;
                yLeft = length - offset;
                if (yLeft >= 0)
                {
                    int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
                    for (int y = offset; y < offset + yLength; y++)
                    {
                        tiers[y].Page = i;
                        DataTier trs = new DataTier(tiers[y]);
                        trs.Tiers = cacheTiers;
                        trs.State.propagate = cacheTiers.State;
                        cacheTiers.AddRef(trs, true);
                        processed++;
                    }
                }
            }
            binwriter.Write(cacheTiers.SetJsonString());
            int nextoffset = (processed < yLeft) ? processed + offset : -1;
            return nextoffset;
        }
        public static int SetJson(this DataTiers tiers, ISerialContext buffor, int offset, int batchSize, DepotComplexity complexity = DepotComplexity.Standard)
        {
            DataTiers view = tiers.Trell.TiersView;          
            int page = view.Trell.PagingDetails.Page;
            int pagesize = view.Trell.PagingDetails.PageSize;
            int cachepages = view.Trell.PagingDetails.CachedPages;
            batchSize = pagesize;
            int yLeft = 0;
            int length = tiers.Count;
            int processed = 0;          

            DataTiers cacheTiers = new DataTiers(tiers.Trell, DataMode.ViewMode(tiers.Trell.Mode));
            cacheTiers.State.withPropagate = false;
            for (int i = page; i < page + cachepages; i++)
            {
                offset = (i - 1) * pagesize;
                yLeft = length - offset;
                if (yLeft >= 0)
                {
                    int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;

                    for (int y = offset; y < offset + yLength; y++)
                    {
                        tiers[y].Page = i;
                        cacheTiers.AddRef(tiers[y], true);
                        processed++;
                    }
                }
            }
            buffor.SerialBlock = cacheTiers.SetJsonString().ToBytes(CharCoding.UTF8);
            int nextoffset = (processed < yLeft) ? processed + offset : -1;
            return nextoffset;
        }

        public static int SetJson(this DataTiers[] tiersArray, StringBuilder stringbuilder, int offset, int batchSize, DepotComplexity complexity = DepotComplexity.Standard)
        {
            List<DataTiers> list = new List<DataTiers>();
            int processed = 0;

            foreach (DataTiers tiers in tiersArray.Where(t => !t.Trell.ParentRelays.Where(p => tiersArray.SelectMany(c => c.Trell.ChildRelays).Contains(p)).Any()))
            {
                DataTiers view = tiers.Trell.TiersView;                
                int page = view.Trell.PagingDetails.Page;
                int pagesize = view.Trell.PagingDetails.PageSize;
                int cachepages = view.Trell.PagingDetails.CachedPages;
                batchSize = pagesize;
                int yLeft = 0;
                int length = tiers.Count;

                Dictionary<int, DataRelay> childRelayId = tiers.Trell.ChildRelays.Select((c, x) => 
                                    new KeyValuePair<int, DataRelay>(x, c))
                                    .Where(k => tiersArray
                                    .Select(t => t.Trell).Contains(k.Value.Child.Trell))
                                    .ToDictionary(k => k.Key, v => v.Value);

                DataTiers cacheTiers = new DataTiers(tiers.Trell, DataMode.ViewMode(tiers.Trell.Mode));
                DataTiers[] childTiers = childRelayId.Select(c => new DataTiers(c.Value.Child.Trell, DataMode.ViewMode(c.Value.Child.Trell.Mode))).ToArray();

                for (int i = page; i < page + cachepages; i++)
                {
                    offset = (i - 1) * pagesize;
                    yLeft = length - offset;
                    if (yLeft >= 0)
                    {
                        int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
                        for (int y = offset; y < offset + yLength; y++)
                        {
                            tiers[y].Page = i;
                            cacheTiers.AddRef(tiers[y], true);
                            int ic = 0;
                            foreach (KeyValuePair<int, DataRelay> c in childRelayId)
                            {
                                DataTiers trs = tiers[y].ChildTiers[c.Key];
                                if (trs.Count == 0)
                                {
                                    //trs.Trell.TiersView.ClearJoins();
                                    childTiers[ic].AddRefRange(tiers[y].ChildTiers[c.Key].AsArray());
                                }
                                else
                                    childTiers[ic].AddRefRange(trs.AsArray());
                                ic++;
                            }
                            processed++;
                        }
                    }
                }
                list.Add(cacheTiers);
                list.AddRange(childTiers);
                foreach (DataTiers t in list)
                    t.Trell.State.ClearState();
            }
            stringbuilder.AppendLine(list.SetJsonString());
            return processed;
        }
        public static int SetJson(this DataTiers[] tiersArray, ISerialContext buffor, int offset, int batchSize, DepotComplexity complexity = DepotComplexity.Standard)
        {
            List<DataTiers> list = new List<DataTiers>();
            int processed = 0;

            foreach (DataTiers tiers in tiersArray.Where(t => !t.Trell.ParentRelays.Where(p => tiersArray.SelectMany(c => c.Trell.ChildRelays).Contains(p)).Any()))
            {
                DataTiers view = tiers.Trell.TiersView;
                int page = view.Trell.PagingDetails.Page;
                int pagesize = view.Trell.PagingDetails.PageSize;
                int cachepages = view.Trell.PagingDetails.CachedPages;
                batchSize = pagesize;
                int yLeft = 0;
                int length = tiers.Count;

                Dictionary<int, DataRelay> childRelayId = tiers.Trell.ChildRelays.Select((c, x) =>
                                    new KeyValuePair<int, DataRelay>(x, c))
                                    .Where(k => tiersArray
                                    .Select(t => t.Trell).Contains(k.Value.Child.Trell))
                                    .ToDictionary(k => k.Key, v => v.Value);

                DataTiers cacheTiers = new DataTiers(tiers.Trell, DataMode.ViewMode(tiers.Trell.Mode));
                DataTiers[] childTiers = childRelayId.Select(c => new DataTiers(c.Value.Child.Trell, DataMode.ViewMode(c.Value.Child.Trell.Mode))).ToArray();

                for (int i = page; i < page + cachepages; i++)
                {
                    offset = (i - 1) * pagesize;
                    yLeft = length - offset;
                    if (yLeft >= 0)
                    {
                        int yLength = (batchSize > 0) ? (yLeft >= batchSize) ? batchSize : yLeft : yLeft;
                        for (int y = offset; y < offset + yLength; y++)
                        {
                            tiers[y].Page = i;
                            cacheTiers.AddRef(tiers[y], true);
                            int ic = 0;
                            foreach (KeyValuePair<int, DataRelay> c in childRelayId)
                            {
                                childTiers[ic].AddRefRange(tiers[y].ChildTiers[c.Key].AsArray());
                                ic++;
                            }
                            processed++;
                        }
                    }
                }
                list.Add(cacheTiers);
                list.AddRange(childTiers);
                foreach (DataTiers t in list)
                    t.Trell.State.ClearState();
            }
            buffor.SerialBlock = list.SetJsonString().ToBytes(CharCoding.UTF8);
            return processed;
        }

        public static string SetJsonString(this DataTiers tiers, DepotComplexity complexity = DepotComplexity.Standard)
        {
            DataTiers[] tiersArray = new DataTiers[] { tiers };

            IDictionary<string, object> toJson = tiersArray.SetJsonBag();
            return DataJson.ToJson(toJson, complexity);
        }
        public static string SetJsonString(this ICollection<DataTiers> tiers, DepotComplexity complexity = DepotComplexity.Standard)
        {
            IDictionary<string, object> toJson = tiers.SetJsonBag();
            string ooo = DataJson.ToJson(toJson, complexity);
            return ooo;
        }
        public static string SetJsonString(this ICollection<DataTiers> tiers, IDictionary<string, object> jsonbag, DepotComplexity complexity = DepotComplexity.Standard)
        {
            return DataJson.ToJson(jsonbag, complexity);
        }

        public static IDictionary<string, object> SetJsonBag(this ICollection<DataTiers> tiers)
        {
            IDictionary<string, object> preJson =
                                new Dictionary<string, object>(){ { "DataBag",  tiers.Select(l => new KeyValuePair<string, object>(l.Trell.TrellName, new Dictionary<string, object>() { { "DataTiers",
                                            l } })).ToDictionary(k => k.Key, v => v.Value) } };

            return preJson;
        }

        public static Dictionary<string, DataTiers> GetJsonObject(this ICollection<DataTiers> tiers, string JsonString)
        {
            Dictionary<string, object> bag = new Dictionary<string, object>();
            Dictionary<string, DataTiers> result = new Dictionary<string, DataTiers>();
            tiers.GetJsonBag(JsonString, "DataBag", "]},", "},{", bag);

            PropertyInfo[] pi = typeof(DataTier).GetJsonProperties();

            foreach (DataTiers head in tiers)
            {
                if (bag.ContainsKey(head.Trell.TrellName))
                {
                    DataTrellis loc = head.Trell.Locate();
                    DataTiers datatiers = new DataTiers(loc, DataMode.ViewMode(loc.Mode));
                    DataTier tier = new DataTier(loc);
                    lock (loc.Tiers.access)
                        lock (loc.TiersView.access)
                        {
                            List<Dictionary<string, object>> subbag = (List<Dictionary<string, object>>)bag[head.Trell.TrellName];
                            foreach (Dictionary<string, object> dict in subbag)
                            {                                
                                DataJson.DeserializeType(pi, dict, tier);
                                datatiers.AddRef(loc.Tiers.Put(tier, false, true));
                            }
                            result.Add(head.Trell.TrellName, datatiers);
                        }
                }
            }

            return result;
        }
        public static DataTiers GetJsonObject(this DataTiers tiers, string JsonString)
        {
            Dictionary<string, object> bag = new Dictionary<string, object>();
            tiers.GetJsonBag(JsonString, "DataBag", "}]},", "},{", bag);

            PropertyInfo[] pi = typeof(DataTier).GetJsonProperties();
            DataTrellis loc = tiers.Trell.Locate();
            DataTier tier = new DataTier(loc);
            if (bag.ContainsKey(loc.TrellName))
            {
                DataTiers datatiers = new DataTiers(loc, DataMode.ViewMode(loc.Mode));
                lock (loc.Tiers.access)
                    lock (loc.TiersView.access)
                    {
                        List<Dictionary<string, object>> subbag = (List<Dictionary<string, object>>)bag[loc.TrellName];
                        foreach (Dictionary<string, object> dict in subbag)
                        {                           
                            DataJson.DeserializeType(pi, dict, tier);
                            datatiers.AddRef(loc.Tiers.Put(tier, false, true));
                        }                      
                    }
                return datatiers;
            }
            else
                return null;
        }

        public static void GetJsonBag(this ICollection<DataTiers> tiers, string jsonmsg, string expandlevel, string expandtrells, string expandtiers, IDictionary<string, object> _bag, int level = 0)
        {
            if (tiers.Count > 0)
                tiers.First().GetJsonBag(jsonmsg, expandlevel, expandtrells, expandtiers, _bag, level);
        }
        public static void GetJsonBag(this DataTiers tiers, string jsonmsg, string expandlevel, string expandtrells, string expandtiers, IDictionary<string, object> _bag, int level = 0)
        {
            int _level = level;
            IDictionary<string, object> bag = _bag;
            string[] trans = jsonmsg.Split(new string[] { "{\"" + expandlevel + "\":" }, StringSplitOptions.RemoveEmptyEntries);
            string msg = (trans.Length > 1) ? trans[1] : trans[0];
            string[] subroots = msg.Split(new string[] { expandtrells }, StringSplitOptions.None);
            string[] roots = subroots.SelectMany(s => s.Split(new string[] { "\":{\"" + "DataTiers" + "\":[" }, StringSplitOptions.None)).ToArray();
            int length = roots.Length;
            for (int i = 0; i < length; i += 2)
            {
                try
                {
                    string[] jstiers = roots[i + 1].TrimStart(new char[] { '{' }).Split(new string[] { expandtiers }, StringSplitOptions.RemoveEmptyEntries);
                    string key = roots[i].Trim(new char[] { '{', '[', '"', ',' });

                    List<Dictionary<string, object>> subbag = new List<Dictionary<string, object>>();
                    foreach (string jstier in jstiers)
                    {
                        try
                        {
                            Dictionary<string, object> tierbag = new Dictionary<string, object>();
                            string _jstier = jstier.TrimStart('{').TrimEnd('}');
                            string[] startsubtiers = _jstier.Substring(0, _jstier.IndexOf('{')).Split(new string[] { ",\"" }, StringSplitOptions.None);
                            for (int x = 0; x < startsubtiers.Length; x++)
                            {
                                string[] subtier = startsubtiers[x].Split(new string[] { "\":" }, StringSplitOptions.None);
                                if (x + 1 < startsubtiers.Length)
                                    tierbag.Add(subtier[0].Trim(new char[] { '[', '{', '"', '\r', '\n' }), subtier[1].Trim(new char[] { ']', '}', '"', '\r', '\n' }));
                                else
                                    tierbag.Add("Fields", subtier[1].Trim('"'));
                            }
                            Dictionary<string, object> itembag = new Dictionary<string, object>();

                            string midsubtier = jstier.Substring(jstier.IndexOf('{') + 1, jstier.IndexOf('}') - jstier.IndexOf('{'));
                            string[] midsubtiers = null;
                            if (!midsubtier.Contains('['))
                            {
                                midsubtiers = midsubtier.Split(new string[] { ",\"" }, StringSplitOptions.None);
                                for (int x = 0; x < midsubtiers.Length; x++)
                                {
                                    string[] subtier = midsubtiers[x].Split(new string[] { "\":" }, StringSplitOptions.None);
                                    itembag.Add(subtier[0].Trim(new char[] { '[', '{', '"', '\r', '\n' }), subtier[1].Trim(new char[] { ']', '}', '"', '\r', '\n' }));
                                }
                            }
                            else
                                itembag.AddRange(DataJson.FromJson("{" + midsubtier + "}"));

                            tierbag["Fields"] = itembag;

                            string endsubtier = jstier.Substring(jstier.IndexOf('}') + 1, jstier.Length - jstier.IndexOf('}') - 1).TrimStart(',');
                            if (endsubtier != null && endsubtier != string.Empty && endsubtier.Contains(":"))
                            {
                                int bracketid = endsubtier.IndexOf('{');
                                string[] endsubtiers = endsubtier.Substring(0, (bracketid > -1) ? bracketid : endsubtier.Length).Split(new string[] { "\"," }, StringSplitOptions.RemoveEmptyEntries);
                                for (int x = 0; x < endsubtiers.Length; x++)
                                {
                                    string[] subtier = endsubtiers[x].Split(new string[] { "\":" }, StringSplitOptions.None);
                                    tierbag.Add(subtier[0].TrimStart(new char[] { ',', '[', '{', '"', '\r', '\n' }), subtier[1].TrimEnd(new char[] { ',', ']', '}', '"', '\r', '\n' }));
                                }
                            }
                            subbag.Add(tierbag);
                        }
                        catch (Exception ex)
                        {
                        }

                    }
                    bag.Add(key, subbag);
                }
                catch (Exception ex)
                {
                }
            }

        }

        public static DataTiers GetJson(this DataTiers tiers, string jsonstring, out int rawcount)
        {
            int _rawcount = 0;
            DataTiers trs = tiers.GetJsonObject(jsonstring);
            rawcount = _rawcount;
            return trs;
        }
        public static DataTiers GetJson(this DataTiers tiers, StringBuilder stringbuilder, out int rawcount)
        {
            int _rawcount = 0;
            StringBuilder sb = stringbuilder;
            DataTiers trs = tiers.GetJsonObject(sb.ToString());
            rawcount = _rawcount;
            sb = null;
            return trs;
        }
        public static DataTiers GetJson(this DataTiers tiers, Stream fromstream, out int rawcount)
        {
            fromstream.Position = 0;
            byte[] array = new byte[4096];
            int _rawcount = 0;
            StringBuilder sb = new StringBuilder();        
            DataTiers trs = tiers.GetJsonObject(sb.ToString());           
            rawcount = _rawcount;
            sb = null;
            fromstream.Dispose();
            return trs;
        }
        public static DataTiers GetJson(this DataTiers tiers, ref object fromarray, out int rawcount)
        {
            string jsonstring = "";
            int _rawcount = 0;
            if (fromarray.GetType() == typeof(byte[]))
            {
                byte[] _fromarray = (byte[])fromarray;
                StringBuilder sb = new StringBuilder();
                jsonstring = sb.Append(_fromarray.ToChars(CharCoding.UTF8)).ToString();
            }
            else if (fromarray.GetType() == typeof(string))
                jsonstring = (string)fromarray;

            DataTiers trs = tiers.GetJsonObject(jsonstring);        

            fromarray = null;
            rawcount = _rawcount;
            return trs;
        }
        public static DataTiers[] GetJson(this DataTiers[] tiersArray, ref object fromarray, out int rawcount)
        {
            string jsonstring = "";
            int _rawcount = 0;
            if (fromarray.GetType() == typeof(byte[]))
            {
                byte[] _fromarray = (byte[])fromarray;
                StringBuilder sb = new StringBuilder();
                jsonstring = sb.Append(_fromarray.ToChars(CharCoding.UTF8)).ToString();
            }
            else if (fromarray.GetType() == typeof(string))
                jsonstring = (string)fromarray;

            Dictionary<string, DataTiers> trs = tiersArray.GetJsonObject(jsonstring);

            fromarray = null;
            rawcount = _rawcount;
            return trs.Values.ToArray();
        }

        public static int SerializeJsonTiers(this IDataSerial[] serials, ISerialContext buffor, int offset, int batchSize, DepotComplexity complexity = DepotComplexity.Standard)
        {
            DataTiers[] objarray = serials.SelectMany(s => s.GetMessage()).Cast<DataTiers>().ToArray();
            return objarray.SetJson(buffor, offset, batchSize, complexity);
        }
        public static int SerializeJsonTiers(this IDataSerial[] serials, StringBuilder buffor, int offset, int batchSize, DepotComplexity complexity = DepotComplexity.Standard)
        {
            DataTiers[] objarray = serials.SelectMany(s => s.GetMessage()).Cast<DataTiers>().ToArray();
            return objarray.SetJson(buffor, offset, batchSize, complexity);
        }

        public static DataTiers[] DeserializeJsonTiers(this IDataSerial[] serials, ref object fromarray, bool driveon = true)
        {
            DataTiers[] objarray = serials.SelectMany(s => s.GetMessage()).Cast<DataTiers>().ToArray();
            int rowcount = 0;
            DataTiers[] result = objarray.GetJson(ref fromarray, out rowcount);
            return result;
        }

        public static void GetJsonByInfo(ref object instance, ref object value, PropertyInfo info)
        {
            List<object> listobjects = (List<object>)value;
            DataTiers tiers = ((DataTiers)info.GetValue(instance, null)).Trell.Tiers;
            DataTrellis trell = tiers.Trell;
            foreach (object obj in listobjects)
            {
                object inst = new object();
                Dictionary<string, object> subbag = (Dictionary<string, object>)obj;
                IEnumerable<PropertyInfo> submap = DataJson.PrepareInstance(out inst, typeof(DataTier));
                DataTier tier = new DataTier(trell);
                foreach (PropertyInfo sub in submap)
                {
                    object val = subbag[sub.Name];
                    if (val.GetType() == typeof(List<object>))
                    {
                        object[] vals = ((List<object>)val).ToArray();
                        trell.Pylons.AsEnumerable()
                                .Where(p => p.Visible)
                                  .Select((x, y) =>
                                     (x.Ordinal <= vals.Length - 1) ?
                                       (x.DataType == typeof(DateTime)) ?
                                         (((List<object>)val)[y] = new DateTime(Convert.ToInt64((double)vals[y]))) :
                                           (x.DataType == typeof(bool)) ?
                                              (((List<object>)val)[y] = Convert.ToBoolean((double)vals[y])) : null : null).ToArray();
                        subbag[sub.Name] = ((List<object>)val).ToArray();
                    }
                    else if (val.GetType() == typeof(Dictionary<string, object>))
                    {
                        Dictionary<string, object> vals = (Dictionary<string, object>)val;
                        trell.Pylons.AsEnumerable()
                            .Where(p => p.Visible)
                               .Select((x, y) =>
                                   (x.Ordinal <= vals.Count - 1) ?
                                     (x.DataType == typeof(DateTime)) ?
                                        (((Dictionary<string, object>)val)[x.PylonName] =
                                            new DateTime(Convert.ToInt64((double)vals[x.PylonName]))) :
                                                (x.DataType == typeof(bool)) ?
                                                   (((Dictionary<string, object>)val)[x.PylonName] =
                                                        Convert.ToBoolean((double)vals[x.PylonName])) : null : null).ToArray();
                        subbag[sub.Name] = (Dictionary<string, object>)val;
                    }
                }
                DataJson.DeserializeType(submap, subbag, tier);
                tiers.Add(tier);
            }
        }      

    }
    #endregion
}
