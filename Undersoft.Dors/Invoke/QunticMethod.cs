using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Dors
{
    public delegate object DorsDelegate(object target, object[] parameters);

    public class DorsInvoke
    {
        public Object TargetObject;
        public Delegate Method;

        public MethodInfo Info;
        public ParameterInfo[] Parameters;

        public int NumberOfArguments;

        public DorsInvoke(Object TargetClassObject, String MethodName)
        {
            TargetObject = TargetClassObject;
            Type t2 = TargetClassObject.GetType();
            MethodInfo m2 = t2.GetMethod(MethodName);
            Method = GetMethodInvoker(m2);
            NumberOfArguments = m2.GetParameters().Length;
            Info = m2;
            Parameters = m2.GetParameters();
        }
        public DorsInvoke(String TargetClassName, String MethodName)
        {
            TargetObject = CallInstance.ActivateMethod(TargetClassName);
            Type t2 = TargetObject.GetType();
            MethodInfo m2 = t2.GetMethod(MethodName);
            Method = GetMethodInvoker(m2);
            NumberOfArguments = m2.GetParameters().Length;
            Info = m2;
            Parameters = m2.GetParameters();
        }
        public DorsInvoke(DorsInvokeInfo MethodInvokeInfo)
        {
            TargetObject = MethodInvokeInfo.TargetObject;
            Type t2 = TargetObject.GetType();
            MethodInfo m2 = t2.GetMethod(MethodInvokeInfo.MethodName);
            Method = GetMethodInvoker(m2);
            NumberOfArguments = m2.GetParameters().Length;
            Info = m2;
            Parameters = m2.GetParameters();
        }

        public object Execute(params object[] FunctionParameters)
        {
            try
            {
                return Method.DynamicInvoke(TargetObject, FunctionParameters);
            }
            catch (Exception e)
            {
                Object o = new Object();
                o = e.Message;
                return (o);

            }
        }

        public object TypeConvert(object source, Type DestType)
        {

            object NewObject = System.Convert.ChangeType(source, DestType);

            return (NewObject);
        }

        private Delegate GetMethodInvoker(MethodInfo methodInfo)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty,
                          typeof(object), new Type[] { typeof(object),
                          typeof(object[]) },
                          methodInfo.DeclaringType.Module);
            ILGenerator il = dynamicMethod.GetILGenerator();
            ParameterInfo[] ps = methodInfo.GetParameters();
            Type[] paramTypes = new Type[ps.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    paramTypes[i] = ps[i].ParameterType.GetElementType();
                else
                    paramTypes[i] = ps[i].ParameterType;
            }
            LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                locals[i] = il.DeclareLocal(paramTypes[i], true);
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                EmitDirectInt(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastToReference(il, paramTypes[i]);
                il.Emit(OpCodes.Stloc, locals[i]);
            }
            if (!methodInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldloca_S, locals[i]);
                else
                    il.Emit(OpCodes.Ldloc, locals[i]);
            }
            if (methodInfo.IsStatic)
                il.EmitCall(OpCodes.Call, methodInfo, null);
            else
                il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            if (methodInfo.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else
                EmitBoxPrimitives(il, methodInfo.ReturnType);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    EmitDirectInt(il, i);
                    il.Emit(OpCodes.Ldloc, locals[i]);
                    if (locals[i].LocalType.IsValueType)
                        il.Emit(OpCodes.Box, locals[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }

            il.Emit(OpCodes.Ret);
            Delegate invoder = (DorsDelegate)
               dynamicMethod.CreateDelegate(typeof(DorsDelegate));
            return invoder;
        }

        private static void EmitCastToReference(ILGenerator il,
                                                System.Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }

        private static void EmitBoxPrimitives(ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
        }

        private static void EmitDirectInt(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            if (value > -129 && value < 128)
            {
                il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
        }
    }

    [Serializable]
    public class DorsInvokeInfo
    {
        [NonSerialized] private object targetObject;

        public string MethodName
        { get; set; }
        public string TargetName
        {
            get;
            set;
        }
        public object TargetObject
        {
            get
            {
                if (targetObject == null)
                    if (TargetName != null)
                    targetObject = CallInstance.ActivateMethod(TargetName);
                return targetObject;
            }
            set
            {
                targetObject = value;
            }
        }
        public object[] Parameters
        { get; set; }

        public DorsInvokeInfo(string ClassMethodName, string TargetClassName, params object[] parameters)
        {
            MethodName = ClassMethodName;
            TargetName = TargetClassName;
            TargetObject = CallInstance.ActivateMethod(TargetClassName);
            Parameters = parameters;
        }
        public DorsInvokeInfo(string ClassMethodName, object TargetClassObject, params object[] parameters)
        {
            MethodName = ClassMethodName;
            TargetObject = TargetClassObject;
            TargetName = TargetClassObject.GetType().FullName;
            Parameters = parameters;
        }

        public void NewParams(params object[] parameters)
        {
            ClearParams();
            AppendParams(parameters);
        }
        public void AppendParams(params object[] parameters)
        {
            if (Parameters != null && Parameters.Length > 0)
                Parameters.Concat(parameters);
            else
                Parameters = parameters;
        }
        public void ClearParams()
        {
            Parameters = null;
        }
    }

    public static class CallInstance
    {
        public static object ActivateMethod(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return Activator.CreateInstance(type);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return Activator.CreateInstance(type);
            }
            return null;
        }
        public static object ActivateMethod(string strFullyQualifiedName, params object[] constructorParams)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return Activator.CreateInstance(type, constructorParams);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return Activator.CreateInstance(type, constructorParams);
            }
            return null;
        }
    }

    public class DorsModifyEventArgs<T> : EventArgs
    {
        public readonly T ModifiedItem;
        public readonly ModifyType ModifyType;
        public readonly T ReplacedWith;

        public DorsModifyEventArgs(ModifyType modify, T item,
            T replacement)
        {
            ModifyType = modify;
            ModifiedItem = item;
            ReplacedWith = replacement;
        }
    }

    public enum ModifyType
    {
        Added,
        Removed,
        Replaced,
        Cleared
    };
}
