using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Doors;

namespace System.Doors.MultiTask
{
    [Serializable]
    public class MultiTaskMethod : IDorsEvent
    {
        [NonSerialized] private DorsInvoke DorsInvoke;

        public string MultiTaskName
        { get; set; }
        public DorsInvokeInfo InvokeInfo
        { get; set; }

        public MultiTaskMethod(string MethodName, string TargetClassName, params object[] parameters)
        {
            InvokeInfo = new DorsInvokeInfo(MethodName, TargetClassName, parameters);
            MultiTaskName = InvokeInfo.TargetName + "." + InvokeInfo.MethodName;
        }
        public MultiTaskMethod(string MethodName, object TargetClassObject, params object[] parameters)
        {
            InvokeInfo = new DorsInvokeInfo(MethodName, TargetClassObject, parameters);
            MultiTaskName = InvokeInfo.TargetName + "." + InvokeInfo.MethodName;
        }
        public MultiTaskMethod(DorsInvokeInfo MethodInvokeInfo)
        {
            InvokeInfo = MethodInvokeInfo;
            MultiTaskName = InvokeInfo.TargetName + "." + InvokeInfo.MethodName;
        }

        public object Execute(params object[] FunctionParameters)
        {
            if (DorsInvoke == null)
                DorsInvoke = new DorsInvoke(InvokeInfo);

            if (FunctionParameters.Length > 0)
                return DorsInvoke.Execute(FunctionParameters);
            else if (InvokeInfo.Parameters != null && InvokeInfo.Parameters.Length > 0)
                return DorsInvoke.Execute(InvokeInfo.Parameters);
            else
                return DorsInvoke.Execute();
        }
    }

    public class MultiTaskRegistry
    {
        public MultiTaskMethod Get(string key)
        {
            MultiTaskMethod method = null;

            Methods.TryGetValue(key, out method);

            return method;
        }
        public void Set(string key, MultiTaskMethod value)
        {
            Methods.AddOrUpdate(key, value, (k, v) => v = value);
        }
        public KeyValuePair<string, MultiTaskMethod> SetPair
        {

            set
            {
                Set(value.Key, value.Value);
            }
        }
        public KeyValuePair<string, MultiTaskMethod> GetPair(string key)
        {
                 MultiTaskMethod ant = Get(key);
            if (ant != null)
                return new KeyValuePair<string, MultiTaskMethod>(key, ant);
            else
                return new KeyValuePair<string, MultiTaskMethod>();
        }
        public ConcurrentDictionary<string, MultiTaskMethod> Methods
        { get; } = new ConcurrentDictionary<string, MultiTaskMethod>();
        public MultiTaskMethod this[string key]
        {
            get
            {
                return Get(key);
            }
            set
            {
                Set(key, value);
            }
        }
    }
}
