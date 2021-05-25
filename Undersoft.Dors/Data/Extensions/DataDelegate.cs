using System.Dors;

namespace System.Dors.Data
{
    [Serializable]
    public class DataEvent : IDorsEvent
    {
        [NonSerialized] private DorsInvoke DorsInvoke;

        public DorsInvokeInfo InvokeInfo
        { get; set; }

        public DataEvent(string MethodName, string TargetClassName, params object[] FunctionParameters)
        {
            InvokeInfo = new DorsInvokeInfo(MethodName, TargetClassName, FunctionParameters);
        }
        public DataEvent(string MethodName, object TargetClassObject, params object[] FunctionParameters)
        {
            InvokeInfo = new DorsInvokeInfo(MethodName, TargetClassObject, FunctionParameters);
        }
        public DataEvent(DorsInvokeInfo MethodInvokeInfo)
        {
            InvokeInfo = MethodInvokeInfo;
        }

        public object Execute(params object[] FunctionParameters)
        {
            if (DorsInvoke == null)
                DorsInvoke = new DorsInvoke(InvokeInfo.TargetObject, InvokeInfo.MethodName);

            if (FunctionParameters.Length > 0)
                return DorsInvoke.Execute(FunctionParameters);
            else if (InvokeInfo.Parameters != null && InvokeInfo.Parameters.Length > 0)
                return DorsInvoke.Execute(InvokeInfo.Parameters);
            else
                return DorsInvoke.Execute();
        }
    }   
}


