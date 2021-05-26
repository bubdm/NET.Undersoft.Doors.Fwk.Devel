using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
//#if NET40
//using System.Dynamic;
//#endif
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Doors;
using System.Dynamic;

namespace System.Doors.Data
{    
    public enum JsonToken
    {
        Unknown,
        LeftBrace,
        RightBrace,
        Colon,
        Comma,
        LeftBracket,
        RightBracket,
        String,
        Number,
        True,
        False,
        Null
    } /// Possible JSON tokens in parsed input.

    public class InvalidJsonException : Exception
    {
        public InvalidJsonException(string message)
            : base(message)
        {

        }
    }

    public interface IJson { }

    public class DataJson
    {      
        private static readonly 
            IDictionary<string, 
                IDictionary<Type, 
                    PropertyInfo[]>> _cache;

        private static readonly char[] _base16 = new[]
                             {
                                 '0', '1', '2', '3',
                                 '4', '5', '6', '7',
                                 '8', '9', 'A', 'B',
                                 'C', 'D', 'E', 'F'
                             };
        private static readonly string[][] _unichar = new string[][]
                             {
                               new string[] { new string('"', 1),  @"\""", },
                               new string[] { new string('/', 1),  @"\/", },
                               new string[] { new string('\\', 1), @"\\", },
                               new string[] { new string('\b', 1), @"\b", },
                               new string[] { new string('\f', 1), @"\f", },
                               new string[] { new string('\n', 1), @"\n", },
                               new string[] { new string('\r', 1), @"\r", },
                               new string[] { new string('\t', 1), @"\t", }
                             };
        private static NumberStyles    _numbers = NumberStyles.Float;

        static DataJson()
        {
            _cache = new Dictionary<string, IDictionary<Type, PropertyInfo[]>>();
            foreach (string cmplx in Enum.GetNames(typeof(DepotComplexity)))
                _cache.Add(cmplx, new Dictionary<Type, PropertyInfo[]>());            
        }

        public static object Serialize(object instance)
        {
            Type type = instance.GetType();
            IDictionary <string, object> bag = GetBagForObject(type, instance);

            return ToJson(bag);
        }
        public static string Serialize<T>(T instance)
        {
            IDictionary<string, object> bag = GetBagForObject(instance);

            return ToJson(bag);
        }

        public static object Deserialize(string json, Type type)
        {
            object instance;
            var map = PrepareInstance(out instance, type);
            var bag = FromJson(json);

            DeserializeImpl(map, bag, instance);
            return instance;
        }
        public static T      Deserialize<T>(string json)
        {
            T instance;
            var map = PrepareInstance(out instance);
            var bag = FromJson(json);

            DeserializeImpl(map, bag, instance);
            return instance;
        }     

        private static void DeserializeImpl(IEnumerable<PropertyInfo> map, IDictionary<string, object> bag, object instance)
        {
            DeserializeType(map, bag, instance);
        }
        private static void DeserializeImpl<T>(IEnumerable<PropertyInfo> map, IDictionary<string, object> bag, T instance)
        {
            DeserializeType(map, bag, instance);
        }

        public static void  DeserializeType(IEnumerable<PropertyInfo> map, IDictionary<string, object> bag, object instance)
        {
            System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";

            foreach (PropertyInfo info in map)
            {
                bool mutated = false;
                string key = info.Name;
                if (!bag.ContainsKey(key))
                {
                    key = info.Name.Replace("_", "");
                    if (!bag.ContainsKey(key))
                    {
                        key = info.Name.Replace("-", "");
                        if (!bag.ContainsKey(key))
                            continue;
                    }
                }

                object value = bag[key];

                if (value != null && value.GetType() == typeof(String))
                    if (value.Equals("null"))
                        value = null;                

                if (value != null)
                {                                                                 
                    if (info.PropertyType == typeof(byte[]))                    
                       if (value.GetType() == typeof(List<object>)) value = ((List<object>)value).Select(symbol => Convert.ToByte(symbol)).ToArray();
                                                               else value = ((object[])value).Select(symbol => Convert.ToByte(symbol)).ToArray();
                    else if (info.PropertyType == typeof(Quid))     value = new Quid(value.ToString());
                    else if (info.PropertyType == typeof(Single))   value = Convert.ToSingle(value, nfi);
                    else if (info.PropertyType == typeof(DateTime)) value = Convert.ToDateTime(value);
                    else if (info.PropertyType == typeof(double))   value = Convert.ToDouble(value, nfi);                    
                    else if (info.PropertyType == typeof(decimal))  value = Convert.ToDecimal(value, nfi);                    
                    else if (info.PropertyType == typeof(int))      value = Convert.ToInt32(value);
                    else if (info.PropertyType == typeof(long))     value = Convert.ToInt64(value);
                    else if (info.PropertyType == typeof(short))    value = Convert.ToInt16(value);                    
                    else if (info.PropertyType == typeof(bool))     value = Convert.ToBoolean(value);

                    else if (info.PropertyType == typeof(IDataNative))
                    {
                        object n = info.GetValue(instance, null);
                        DeserializeType(n.GetType().GetProperties(), 
                                        (Dictionary<string, object>)value, n);   
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(SortedDictionary<string, DataSpheres>))
                    {
                        JsonSpheres.GetJsonByInfo(ref instance, ref value, info);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(SortedDictionary<string, DataSphere>))
                    {
                        JsonSphere.GetJsonByInfo(ref instance, ref value, info);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(Dictionary<string, DataTrellis>))
                    {
                        JsonTrellis.GetJsonByInfo(ref instance, ref value, info);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DataSpheres))
                    {
                        JsonSpheres.GetJsonByInfo(ref instance, ref value, info, false);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DataTrellises))
                    {
                        JsonTrellises.GetJsonByInfo(ref instance, ref value, info);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DataTiers))
                    {
                        JsonTiers.GetJsonByInfo(ref instance, ref value, info);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DataPageDetails))
                    {
                        object inst = new object();
                        Dictionary<string, object> subbag = (Dictionary<string, object>)value;
                        IEnumerable<PropertyInfo> submap = PrepareInstance(out inst, typeof(DataPageDetails));
                        DeserializeType(submap, subbag, inst);
                        DataPageDetails pyls = (DataPageDetails)info.GetValue(instance, null);
                        pyls.Impact((DataPageDetails)inst);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DataRelaying))
                    {
                        value = (DataRelaying)info.GetValue(instance, null);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DataRelays))
                    {
                        if (((ICollection)value).Count > 0)
                        {
                            List<object> listobjects = (List<object>)value;
                            List<DataRelay> list = new List<DataRelay>();
                            foreach (object obj in listobjects)
                            {
                                object inst = new object();
                                Dictionary<string, object> subbag = (Dictionary<string, object>)obj;
                                IEnumerable<PropertyInfo> submap = PrepareInstance(out inst, typeof(DataRelay));
                                DeserializeType(submap, subbag, inst);
                                list.Add((DataRelay)inst);
                            }
                            DataRelays pyls = (DataRelays)info.GetValue(instance, null);
                            pyls.AddRange(list);
                        }
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DataRelayMember))
                    {
                        object inst = new object();
                        Dictionary<string, object> subbag = (Dictionary<string, object>)value;
                        IEnumerable<PropertyInfo> submap = PrepareInstance(out inst, typeof(DataRelayMember));
                        DeserializeType(submap, subbag, inst);
                        value = inst;
                    }
                    else if (info.PropertyType == typeof(DataRelay))
                    {
                        value = (DataRelay)info.GetValue(instance, null);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DataPylons))
                    {
                        if (((ICollection)value).Count > 0)
                        {
                            List<object> listobjects = (List<object>)value;
                            List<DataPylon> list = new List<DataPylon>();
                            foreach (object obj in listobjects)
                            {
                                object inst = new object();
                                Dictionary<string, object> subbag = (Dictionary<string, object>)obj;
                                IEnumerable<PropertyInfo> submap = PrepareInstance(out inst, typeof(DataPylon));
                                DeserializeType(submap, subbag, inst);
                                list.Add((DataPylon)inst);
                            }
                            DataPylons pyls = (DataPylons)info.GetValue(instance, null);
                            pyls.AddRange(list);
                        }
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DataFavorites))
                    {
                        if (((ICollection)value).Count > 0)
                        {
                            List<object> listobjects = (List<object>)value;
                            List<DataFavorite> list = new List<DataFavorite>();
                            foreach (object obj in listobjects)
                            {
                                object inst = new object();
                                Dictionary<string, object> subbag = (Dictionary<string, object>)obj;
                                IEnumerable<PropertyInfo> submap = PrepareInstance(out inst, typeof(DataFavorite));
                                DeserializeType(submap, subbag, inst);
                                list.Add((DataFavorite)inst);
                            }
                            DataFavorites terms = (DataFavorites)info.GetValue(instance, null);
                            terms.AddRange(list);
                        }
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(FilterTerms))
                    {
                        if (((ICollection)value).Count > 0)
                        {
                            List<object> listobjects = (List<object>)value;
                            List<FilterTerm> list = new List<FilterTerm>();
                            foreach (object obj in listobjects)
                            {
                                object inst = new object();
                                Dictionary<string, object> subbag = (Dictionary<string, object>)obj;
                                IEnumerable<PropertyInfo> submap = PrepareInstance(out inst, typeof(FilterTerm));
                                DeserializeType(submap, subbag, inst);
                                list.Add((FilterTerm)inst);
                            }
                            FilterTerms terms = (FilterTerms)info.GetValue(instance, null);
                            terms.AddRange(list);
                        }
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(SortTerms))
                    {
                        if (((ICollection)value).Count > 0)
                        {
                            List<object> listobjects = (List<object>)value;
                            List<SortTerm> list = new List<SortTerm>();
                            foreach (object obj in listobjects)
                            {
                                object inst = new object();
                                Dictionary<string, object> subbag = (Dictionary<string, object>)obj;
                                IEnumerable<PropertyInfo> submap = PrepareInstance(out inst, typeof(SortTerm));
                                DeserializeType(submap, subbag, inst);
                                list.Add((SortTerm)inst);
                            }
                            SortTerms terms = (SortTerms)info.GetValue(instance, null);
                            terms.AddRange(list);
                        }
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DataPylon[]))
                    {
                        if (((ICollection)value).Count > 0)
                        {
                            List<object> listobjects = (List<object>)value;
                            List<DataPylon> list = new List<DataPylon>();
                            foreach (object obj in listobjects)
                            {
                                object inst = new object();
                                Dictionary<string, object> subbag = (Dictionary<string, object>)obj;
                                IEnumerable<PropertyInfo> submap = PrepareInstance(out inst, typeof(DataPylon));
                                DeserializeType(submap, subbag, inst);
                                list.Add((DataPylon)inst);
                            }
                            DataPylon[] pyls = (DataPylon[])info.GetValue(instance, null);
                            pyls = list.ToArray();
                        }
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DataState))
                    {
                        object inst = new object();
                        Dictionary<string, object> subbag = (Dictionary<string, object>)value;
                        IEnumerable<PropertyInfo> submap = PrepareInstance(out inst, typeof(DataState));
                        DeserializeType(submap, subbag, inst);
                        DataState state = (DataState)info.GetValue(instance, null);
                        state.Impact((DataState)inst);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(TransactionContext))
                    {
                        object inst = new object();
                        Dictionary<string, object> subbag = (Dictionary<string, object>)value;
                        IEnumerable<PropertyInfo> submap = PrepareInstance(out inst, typeof(TransactionContext));
                        DeserializeType(submap, subbag, inst);
                        info.SetValue(instance, inst, null);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DepotIdentity))
                    {
                        object inst = new object();
                        Dictionary<string, object> subbag = (Dictionary<string, object>)value;
                        IEnumerable<PropertyInfo> submap = PrepareInstance(out inst, typeof(DepotIdentity));
                        DeserializeType(submap, subbag, inst);
                        info.SetValue(instance, inst, null);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DataConfig))
                    {
                        object inst = new object();
                        Dictionary<string, object> subbag = (Dictionary<string, object>)value;
                        IEnumerable<PropertyInfo> submap = PrepareInstance(out inst, typeof(DataConfig));
                        DeserializeType(submap, subbag, inst);
                        DataConfig config = (DataConfig)info.GetValue(instance, null);
                        config.Impact((DataConfig)inst);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(DataParams))
                    {
                        object inst = new object();
                        Dictionary<string, object> subbags = (Dictionary<string, object>)value;
                        DataParams parameters = (DataParams)info.GetValue(instance, null);
                        foreach (KeyValuePair<string, object> subbag in subbags)
                            parameters.Add(subbag);
                        mutated = true;
                    }
                    else if (info.PropertyType == typeof(Type))
                    {
                        object typevalue = info.GetValue(instance, null);
                        if (value != null)
                            typevalue = Type.GetType(value.ToString());
                        value = typevalue;
                    }
                    else if (info.PropertyType.IsEnum)
                    {
                        object enumvalue = info.GetValue(instance, null);
                        enumvalue = Enum.Parse(info.PropertyType, value.ToString());
                        value = enumvalue;
                    }
                }

                if (!mutated)
                    info.SetValue(instance, value, null);
            }
        }

        public static IDictionary<string, object> FromJson(string json)
        {
            
            JsonToken type;

            var result = FromJson(json, out type);

            switch (type)
            {
                case JsonToken.LeftBrace:
                    var @object = (IDictionary<string, object>)result.Single().Value;
                    return @object;
            }

            return result;
        }
        public static IDictionary<string, object> FromJson(string json, out JsonToken type)
        {
            
            var data = json.ToCharArray();
            var index = 0;

            // Rewind index for first token
            var token = NextToken(data, ref index);
            switch (token)
            {
                case JsonToken.LeftBrace:   // Start Object
                case JsonToken.LeftBracket: // Start Array
                    index--;
                    type = token;
                    break;
                default:
                    throw new InvalidJsonException("JSON must begin with an object or array");
            }

            return ParseObject(data, ref index);
        }

        public static string ToJson(IDictionary<string, object> bag, DepotComplexity complexity = DepotComplexity.Standard)
        {            
            var sb = new StringBuilder(4096);

            SerializeItem(sb, bag, null, complexity);

            return sb.ToString();
        }
        public static string ToJson(IDictionary<int, object> bag, DepotComplexity complexity = DepotComplexity.Standard)
        {
            
            var sb = new StringBuilder(0);
            
            SerializeItem(sb, bag, null, complexity);

            return sb.ToString();
        }

        internal static IDictionary<string, object> GetBagForObject(Type type, object instance, DepotComplexity complexity = DepotComplexity.Standard)
        {
            CacheReflection(type, complexity);

            if (type.FullName == null)
            {
                return null;
            }

            bool anonymous = type.FullName.Contains("__AnonymousType");
            PropertyInfo[] map = _cache[complexity.ToString()][type];

            IDictionary<string, object> bag = InitializeBag();
            foreach (PropertyInfo info in map)
            {
                if (info != null)
                {
                    var readWrite = (info.CanWrite && info.CanRead);
                    if (!readWrite && !anonymous)
                    {
                        continue;
                    }
                    object value = null;
                    try
                    {
                        value = info.GetValue(instance, null);
                    }
                    catch (Exception ex)
                    {
                        ex.ToDataLog();
                    }
                    bag.Add(info.Name, value);
                }
            }

            return bag;
        }
        internal static IDictionary<string, object> GetBagForObject<T>(T instance, DepotComplexity complexity = DepotComplexity.Standard)
        {
            return GetBagForObject(typeof(T), instance, complexity);
        }

        internal static Dictionary<string, object> InitializeBag()
        {
            return new Dictionary<string, object>(
                0, StringComparer.OrdinalIgnoreCase
                );
        }

        public static IEnumerable<PropertyInfo> PrepareInstance(out object instance, Type type)
        {
            instance = Activator.CreateInstance(type);

            CacheReflection(type);

            return _cache["Standard"][type];
        }
        public static IEnumerable<PropertyInfo> PrepareInstance<T>(out T instance)
        {
            instance = Activator.CreateInstance<T>();
            Type item = typeof(T);

            CacheReflection(item);

            return _cache["Standard"][item];
        }

        internal static void CacheReflection(Type item, DepotComplexity complexity = DepotComplexity.Standard)
        {
            if (_cache[complexity.ToString()].ContainsKey(item))
                return;

            PropertyInfo[] verified = new PropertyInfo[0];
            PropertyInfo[] prepare = item.GetJsonProperties(complexity);
            if (prepare != null)
                verified = prepare;

            _cache[complexity.ToString()].Add(item, verified);
        }

        internal static void SerializeItem(StringBuilder sb, object item, string key = null, DepotComplexity complexity = DepotComplexity.Standard)
        {          
            if (item == null)
            {
                sb.Append("null");
                return;
            }

            if (item is IDictionary)
            {
                SerializeObject(item, sb, false, complexity);
                return;
            }           

            if (item is ICollection && !(item is string))
            {
                SerializeArray(item, sb, complexity);
                return;
            }

            if (item is Quid)
            {
                sb.Append("\"" + ((Quid)item).ToString() + "\"");
                return;
            }

            if (item is Noid)
            {
                sb.Append("\"" + ((Noid)item).ToString() + "\"");
                return;
            }

            if (item is DateTime)
            {
                sb.Append("\""+((DateTime)item).ToString("yyyy-MM-dd HH:mm:dd")+"\"");
                return;
            }

            if (item is Enum)
            {
                sb.Append("\"" + item.ToString() + "\"");
                return;
            }

            if (item is Type)
            {
                sb.Append("\"" + ((Type)item).FullName + "\"");
                return;
            }

            if (item is bool)
            {
                sb.Append(((bool)item).ToString().ToLower());
                return;
            }

            if(item is ValueType)
            {
                sb.Append(item.ToString().Replace(',','.'));
                return;
            }

            IDictionary<string, object> 
                bag = GetBagForObject(item.GetType(), item, complexity);

            SerializeItem(sb, bag, key, complexity);
        }
        internal static void SerializeDateTime(StringBuilder sb, object item = null)
        {
            sb.Append("\"" + ((DateTime)item).ToString("yyyy-MM-dd HH:mm:dd") + "\"");
            
            //var elapsed = DateTime.UtcNow - new DateTime(1970, 1, 1).ToUniversalTime();
            //var epoch = (long)elapsed.TotalSeconds;
            //SerializeString(sb, epoch);
        }
        internal static void SerializeArray(object item, StringBuilder sb, DepotComplexity complexity = DepotComplexity.Standard)
        {
            Type type = item.GetType();
            if (type.IsDefined(typeof(JsonObjectAttribute), false))
            {
                var bag = GetBagForObject(item.GetType(), item, complexity);
                SerializeItem(sb, bag, null, complexity);
            }
            else
            {
                ICollection array = (ICollection)item;

                sb.Append("[");
                var count = 0;

                var total = array.Cast<object>().Count();                
                foreach (object element in array)
                {
                    SerializeItem(sb, element, null, complexity);
                    count++;
                    if (count < total)
                        sb.Append(",");
                }
                sb.Append("]");
            }
        }
        internal static void SerializeObject(object item, StringBuilder sb, bool intAsKey = false, DepotComplexity complexity = DepotComplexity.Standard)
        {
            sb.Append("{");

            IDictionary nested = (IDictionary)item;
            int i = 0;
            int count = nested.Count;
            foreach (DictionaryEntry entry in nested)
            {
                sb.Append("\"" + entry.Key + "\"");
                sb.Append(":");

                object value = entry.Value;
                if (value is string)
                {
                    SerializeString(sb, value);
                }
                else
                {
                    SerializeItem(sb, entry.Value, entry.Key.ToString(), complexity);
                }
                if (i < count - 1)
                {
                    sb.Append(",");
                }
                i++;
            }

            sb.Append("}");
        }
        internal static void SerializeString(StringBuilder sb, object item)
        {
            char[] symbols = ((string)item).ToCharArray();
            SerializeUnicode(sb, symbols);
        }
        internal static void SerializeUnicode(StringBuilder sb, char[] symbols)
        {
            sb.Append("\"");

            string[] unicodes = symbols.Select(symbol => GetUnicode(symbol)).ToArray();

            foreach (var unicode in unicodes)
                sb.Append(unicode);          

            sb.Append("\"");
        }

        internal static string GetUnicode(char character)
        {
            switch (character)
            {
                case '"': return @"\""";
                case '/': return @"\/";
                case '\\': return @"\\";
                case '\b': return @"\b";
                case '\f': return @"\f";
                case '\n': return @"\n";
                case '\r': return @"\r";
                case '\t': return @"\t";
            }
            return new string(character, 1);

            //var code = (int)character;
            //var basicLatin = code >= 32 && code <= 243;
            //if (basicLatin)
            //{
            //    return new string(character, 1);
            //}

            //var unicode = BaseConvert(code, _base16, 4);
            //return string.Concat("\\u", unicode);
        }
        internal static string BaseConvert(int input, char[] charSet, int minLength)
        {
            var sb = new StringBuilder();
            var @base = charSet.Length;

            while (input > 0)
            {
                var index = input % @base;
                sb.Insert(0, new[] { charSet[index] });
                input = input / @base;
            }

            while (sb.Length < minLength)
            {
                sb.Insert(0, "0");
            }

            return sb.ToString();
        }

        internal static IDictionary<string, object> 
                               ParseObject(IList<char> data, ref int index)
        {
            var result = InitializeBag();

            index++; // Skip first token

            while (index < data.Count - 1)
            {
                var token = NextToken(data, ref index);
                switch (token)
                {
                    // End Tokens
                    case JsonToken.Unknown:             // Bad Data
                    case JsonToken.True:
                    case JsonToken.False:
                    case JsonToken.Null:
                    case JsonToken.Colon:
                    case JsonToken.RightBracket:
                    case JsonToken.Number:
                        throw new InvalidJsonException(string.Format(
                            "Invalid JSON found while parsing an object at index {0}.", index
                            ));
                    case JsonToken.RightBrace:          // End Object
                        index++;
                        return result;
                    // Skip Tokens
                    case JsonToken.Comma:
                        index++;
                        break;
                    // Start Tokens
                    case JsonToken.LeftBrace:           // Start Object
                        var @object = ParseObject(data, ref index);
                        if (@object != null)
                        {
                            result.Add(string.Concat("object", result.Count), @object);
                        }
                        index++;
                        break;
                    case JsonToken.LeftBracket:         // Start Array
                        var @array = ParseArray(data, ref index);
                        if (@array != null)
                        {
                            result.Add(string.Concat("array", result.Count), @array);
                        }
                        index++;
                        break;
                    case JsonToken.String:
                        var pair = ParsePair(data, ref index);
                        result.Add(pair.Key, pair.Value);
                        break;
                    default:
                        throw new NotSupportedException("Invalid token expected.");
                }
            }

            return result;
        }
        internal static IEnumerable<object> 
                               ParseArray(IList<char> data, ref int index)
        {
            var result = new List<object>();

            index++; // Skip first bracket
            while (index < data.Count - 1)
            {
                var token = NextToken(data, ref index);
                switch (token)
                {
                    // End Tokens
                    case JsonToken.Unknown:             // Bad Data
                        throw new InvalidJsonException(string.Format(
                            "Invalid JSON found while parsing an array at index {0}.", index
                            ));
                    case JsonToken.RightBracket:        // End Array
                        index++;
                        return result;
                    // Skip Tokens
                    case JsonToken.Comma:               // Separator
                    case JsonToken.RightBrace:          // End Object
                    case JsonToken.Colon:               // Separator
                        index++;
                        break;
                    // Value Tokens
                    case JsonToken.LeftBrace:           // Start Object
                        var nested = ParseObject(data, ref index);
                        result.Add(nested);
                        break;
                    case JsonToken.LeftBracket:         // Start Array
                    case JsonToken.String:
                    case JsonToken.Number:
                    case JsonToken.True:
                    case JsonToken.False:
                    case JsonToken.Null:
                        var value = ParseValue(data, ref index);
                        result.Add(value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return result;
        }
        internal static KeyValuePair<string, object> 
                               ParsePair(IList<char> data, ref int index)
        {
            var valid = true;

            var name = ParseString(data, ref index);
            if (name == null)
            {
                valid = false;
            }

            if (!ParseToken(JsonToken.Colon, data, ref index))
            {
                valid = false;
            }

            if (!valid)
            {
                throw new InvalidJsonException(string.Format(
                            "Invalid JSON found while parsing a value pair at index {0}.", index
                            ));
            }

            index++;
            var value = ParseValue(data, ref index);
            return new KeyValuePair<string, object>(name, value);
        }
        internal static bool   ParseToken(JsonToken token, IList<char> data, ref int index)
        {
            var nextToken = NextToken(data, ref index);
            return token == nextToken;
        }
        internal static string ParseString(IList<char> data, ref int index)
        {
            var symbol = data[index];
            IgnoreWhitespace(data, ref index, symbol);
            symbol = data[++index]; // Skip first quotation

            var sb = new StringBuilder();
            while (true)
            {
                if (index >= data.Count - 1)
                {
                    return null;
                }
                switch (symbol)
                {
                    case '"':  // End String
                        index++;
                        return sb.ToString();
                    case '\\': // Control Character
                        symbol = data[++index];
                        switch (symbol)
                        {
                            case '/':
                            case '\\':
                            case '"':
                                sb.Append(symbol);
                                break;
                            case 'b':
                                sb.Append('\b');
                                break;
                            case 'f':
                                sb.Append('\f');
                                break;
                            case 'n':
                                sb.Append('\n');
                                break;
                            case 'r':
                                sb.Append('\r');
                                break;
                            case 't':
                                sb.Append('\t');
                                break;
                            case 'u': // Unicode literals
                                if (index < data.Count - 5)
                                {
                                    var array = data.ToArray();
                                    var buffer = new char[4];
                                    Array.Copy(array, index + 1, buffer, 0, 4);

                                    // http://msdn.microsoft.com/en-us/library/aa664669%28VS.71%29.aspx
                                    // http://www.yoda.arachsys.com/csharp/unicode.html
                                    // http://en.wikipedia.org/wiki/UTF-32/UCS-4
                                    var hex = new string(buffer);
                                    var unicode = (char)Convert.ToInt32(hex, 16);
                                    sb.Append(unicode);
                                    index += 4;
                                }
                                else
                                {
                                    break;
                                }
                                break;
                        }
                        break;
                    default:
                        sb.Append(symbol);
                        break;
                }
                symbol = data[++index];
            }
        }
        internal static object ParseValue(IList<char> data, ref int index)
        {

            var token = NextToken(data, ref index);
            switch (token)
            {
                // End Tokens
                case JsonToken.RightBracket:    // Bad Data
                case JsonToken.RightBrace:
                case JsonToken.Unknown:
                case JsonToken.Colon:
                case JsonToken.Comma:
                    throw new InvalidJsonException(string.Format(
                            "Invalid JSON found while parsing a value at index {0}.", index
                            ));
                // Value Tokens
                case JsonToken.LeftBrace:
                    return ParseObject(data, ref index);
                case JsonToken.LeftBracket:
                    return ParseArray(data, ref index);
                case JsonToken.String:
                    return ParseString(data, ref index);
                case JsonToken.Number:
                    return ParseNumber(data, ref index);
                case JsonToken.True:
                    return true;
                case JsonToken.False:
                    return false;
                case JsonToken.Null:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        internal static object ParseNumber(IList<char> data, ref int index)
        {
            var symbol = data[index];
            IgnoreWhitespace(data, ref index, symbol);

            var start = index;
            var length = 0;
            while (ParseToken(JsonToken.Number, data, ref index))
            {
                length++;
                index++;
            }
     
            double result;
            var buffer = new string(data.Skip(start).Take(length).ToArray());
            if (!double.TryParse(buffer, _numbers, CultureInfo.InvariantCulture, out result))
            {
                throw new InvalidJsonException(
                    string.Format("Value '{0}' was not a valid JSON number", buffer)
                    );
            }
            return result;
        }

        internal static JsonToken NextToken(IList<char> data, ref int index)
        {
    
            var symbol = data[index];
            var token = GetTokenFromSymbol(symbol);
            token = IgnoreWhitespace(data, ref index, ref token, symbol);

            GetKeyword("true", JsonToken.True, data, ref index, ref token);
            GetKeyword("false", JsonToken.False, data, ref index, ref token);
            GetKeyword("null", JsonToken.Null, data, ref index, ref token);

            return token;
        }

        internal static JsonToken GetTokenFromSymbol(char symbol)
        {
            return GetTokenFromSymbol(symbol, JsonToken.Unknown);
        }
        internal static JsonToken GetTokenFromSymbol(char symbol, JsonToken token)
        {
            switch (symbol)
            {
                case '{':
                    token = JsonToken.LeftBrace;
                    break;
                case '}':
                    token = JsonToken.RightBrace;
                    break;
                case ':':
                    token = JsonToken.Colon;
                    break;
                case ',':
                    token = JsonToken.Comma;
                    break;
                case '[':
                    token = JsonToken.LeftBracket;
                    break;
                case ']':
                    token = JsonToken.RightBracket;
                    break;
                case '"':
                    token = JsonToken.String;
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '.':
                case 'e':
                case 'E':
                case '+':
                case '-':
                    token = JsonToken.Number;
                    break;
            }
            return token;
        }

        internal static void      IgnoreWhitespace(IList<char> data, ref int index, char symbol)
        {
            var token = JsonToken.Unknown;
            IgnoreWhitespace(data, ref index, ref token, symbol);
            return;
        }
        internal static JsonToken IgnoreWhitespace(IList<char> data, ref int index, ref JsonToken token, char symbol)
        {
            switch (symbol)
            {
                case ' ':
                case '\\':
                case '/':
                case '\b':
                case '\f':
                case '\n':
                case '\r':
                case '\t':
                    index++;
                    token = NextToken(data, ref index);
                    break;
            }
            return token;
        }

        internal static void   GetKeyword(string word, JsonToken target, IList<char> data,ref int index, ref JsonToken result)
        {
            int buffer = data.Count - index;
            if (buffer < word.Length)
            {
                return;
            }

            for (var i = 0; i < word.Length; i++)
            {
                if (data[index + i] != word[i])
                {
                    return;
                }
            }

            result = target;
            index += word.Length;
        }     
    }

    public static class DataJsonProperties
    {
      
        public static Dictionary<string, string[]> Registry = new Dictionary<string, string[]>()
        {
               {
                "System.Doors.Data.Depot.TransactionHeader",
                new string[]
                {
                    "Context",   "Content"
                }
             },
            {
                "System.Doors.Data.Depot.TransactionMessage",
                new string[]
                {
                    "Notice",   "Content"
                }
             },
            {
                "System.Doors.TransactionContext",
                new string[]
                {
                    "Identity",   "ContentType",  "Complexity",
                    "Echo"
                }
             },
             {
                "System.Doors.DepotIdentity_Advanced",
                new string[]
                {
                    "Id",     "Name",   "Key",  "Token", "UserId",
                    "DeptId", "Ip", "DataPlace"
                }
             },
              {
                "System.Doors.DepotIdentity_Basic",
                new string[]
                {
                    "UserId", "DeptId", "Token", "DataPlace",
                }
             },
               {
                "System.Doors.DepotIdentity",
                new string[]
                {
                    "Name",   "Key",  "Token", "UserId",
                    "DeptId", "DataPlace",
                }
             },
                {
                "System.Doors.DepotIdentity_Guide",
                new string[]
                {
                    "Token", "UserId", "DeptId", "DataPlace"
                }
             },
            {
                "System.Doors.Data.DataArea",
                new string[]
                {
                    "SpaceName",   "DisplayName",  "Config",  "State",
                    "Areas"
                }
             },
              {
                "System.Doors.Data.DataArea_Guide",
                new string[]
                {
                    "SpaceName", "Areas", "Config"
                }
             },
             {
                "System.Doors.Data.DataSpheres",
                new string[]
                {
                    "SpheresId",   "DisplayName",  "Config",  "State",
                    "Spheres",     "SpheresIn"
                }
             },
              {
                "System.Doors.Data.DataSpheres_Guide",
                new string[]
                {
                    "SpheresId", "Spheres", "SpheresIn", "Config"
                }
             },
             {
                "System.Doors.Data.DataSphere",
                new string[]
                {
                    "SphereId",   "DisplayName",  "Config",     "State",
                    "Relays",     "Trellises",   "SpheresIn"
                }
            },
                {
                "System.Doors.Data.DataSphere_Guide",
                new string[]
                {
                    "SphereId", "Trellises", "SpheresIn", "Config"
                }
            },
               {
                "System.Doors.Data.DataTrellis",
                new string[]
                {
                    "TrellName",  "DisplayName", "Mode",       "Config",  "State",
                    "Checked",    "Edited",      "Synced",     "Saved",
                    "Quered",     "CountView",   "Count",        "PagingDetails",
                    "Pylons",     "PrimeKey",    "EditLevel",    "SimLevel",
                    "Filter",     "Sort",        "Favorites",    "Relaying",
                    "TiersTotal", "SimsTotal"
                }
            },
            {
                "System.Doors.Data.DataTrellis_Advanced",
                new string[]
                {
                    "TrellName",     "DisplayName",  "Visible",       "Mode",
                    "Checked",       "Edited",       "Synced",        "Saved",
                    "Quered",        "CountView",    "Count",        "Config",
                    "IsPrime",       "State",        "Pylons",       "PrimeKey",
                    "EditLevel",     "SimLevel",     "Filter",       "Sort",
                    "PagingDetails", "Favorites",    "Relaying",     "TiersTotal",
                    "SimsTotal",     "EditLength",   "SimLength",    "MappingName",
                    "Relays"
                }
            },
             {
                "System.Doors.Data.DataTrellis_Basic",
                new string[]
                {
                    "TrellName",    "DisplayName",   "CountView",   "Config",
                    "State",        "Checked",      "Edited",        "Synced",
                     "Saved",       "Quered",       "EditLevel",     "SimLevel",
                     "Mode",        "Filter",      "Sort",          "PagingDetails",
                    "Favorites",    "TiersTotal", "SimsTotal"
                }
            },
               {
                "System.Doors.Data.DataTrellis_Guide",
                new string[]
                {
                    "TrellName", "Config"
                }
            },
            {
                "System.Doors.Data.DataTier",
                new string[]
                {
                    "Checked",   "Index",      "Page",     "PageIndex",
                    "ViewIndex", "NoId",       "Edited",   "Deleted",
                    "Added",     "Synced",     "Saved", 
                    "Fields",    "ChildJoins", "ParentJoins"
                }
            },           
             {
                "System.Doors.Data.DataField",
                new string[]
                {
                    "Value"
                }
            },
            {
                "System.Doors.Data.DataPylon",
                new string[]
                {
                    "PylonId",     "Ordinal",           "Visible",
                    "PylonName",   "DisplayName",       "DataType",
                    "Default",     "isKey",             "Editable", 
                    "Revalue",     "RevalOperand",      "RevalType",
                    "TotalOperand"
                }
            },
       
             {
                "System.Doors.Data.FilterTerm",
                new string[]
                {
                    "Index",     "PylonName",    "Operand",
                    "Value",     "Logic",        "Stage",
                }
            },
               {
                "System.Doors.Data.SortTerm",
                new string[]
                {
                    "Index",     "PylonName",    "Direction"
                }
            },
                 {
                "System.Doors.DataConfig",
                new string[]
                {
                    "Place",  "DataIdx",  "Path", "DepotIdx", "DataType"
                }
            },
                     {
                "System.Doors.DataConfig_Guide",
                new string[]
                {
                    "Place",  "DataIdx", "DataType"
                }
            },
                    {
                "System.Doors.DataConfig_Basic",
                new string[]
                {
                    "Place", "DataIdx", "DepotIdx", "DataType"
                }
            },
              {
                "System.Doors.DataConfig_Advanced",
                new string[]
                {
                    "Place", "DataIdx", "Path", "Disk", "File", "DepotIdx", "DataType"
                }
            },
                   {
                "System.Doors.DataState",
                new string[]
                {
                    "Edited",     "Deleted",
                    "Added",      "Synced",     "Canceled",
                    "Saved",      "Quered",
                }
            },
            {
                "System.Doors.Data.DataPageDetails",
                new string[]
                {
                    "Page",        "PageActive",  "CachedPages",
                    "PageCount",   "PageSize",
                }
            },
             {
                "System.Doors.Data.DataRelaying",
                new string[]
                {
                    "ChildRelays",  "ParentRelays"
                }
            },
              {
                "System.Doors.Data.DataRelay",
                new string[]
                {
                    "RelayName",  "Parent",  "Child",
                }
            },
            { 
                "System.Doors.Data.DataRelayMember",
                new string[]
                {
                    "TrellName",  "RelayKeys"
                }
            }
        };

        public static NumberFormatInfo JsonNumberInfo()
        {
            System.Globalization.NumberFormatInfo nfi = new System.Globalization.NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";            
            return nfi;
        }
      
        public static PropertyInfo[] GetJsonProperties(this Type type, DepotComplexity complexity = DepotComplexity.Standard)
        {
            string name = type.FullName;
            string cname = "";
            if (complexity != DepotComplexity.Standard)
                cname = name + "_" + complexity.ToString();
            else
                cname = name;

            if (Registry.ContainsKey(cname))
                return Registry[cname].Select(t => type.GetProperty(t)).ToArray();
            else if (Registry.ContainsKey(name))
                return Registry[name].Select(t => type.GetProperty(t)).ToArray();
            else
            return null;
        }
        public static PropertyInfo[] GetJsonProperties<T>(DepotComplexity complexity = DepotComplexity.Standard)
        {
            Type type = typeof(T);
            string name = type.FullName;
            string cname = "";
            if (complexity != DepotComplexity.Standard)
                cname = name + "_" + complexity.ToString();
            else
                cname = name;

            if (Registry.ContainsKey(cname))
                return Registry[cname].Select(t => type.GetProperty(t)).ToArray();
            else if (Registry.ContainsKey(name))
                return Registry[name].Select(t => type.GetProperty(t)).ToArray();
            else
                return null;
        }
    }    
}
