using System.Doors;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Doors.Data.Depot
{
    [Serializable]
    public class DepotEvent : IDorsEvent
    {
        [NonSerialized] private DorsInvoke DorsInvoke;

        public DorsInvokeInfo InvokeInfo
        { get; set; }

        public DepotEvent(string MethodName, string TargetClassName, params object[] parameters)
        {
            InvokeInfo = new DorsInvokeInfo(MethodName, TargetClassName, parameters);
        }
        public DepotEvent(string MethodName, object TargetClassObject, params object[] parameters)
        {
            InvokeInfo = new DorsInvokeInfo(MethodName, TargetClassObject, parameters);
        }
        public DepotEvent(DorsInvokeInfo MethodInvokeInfo)
        {
            InvokeInfo = MethodInvokeInfo;
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
}


