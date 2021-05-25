using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace System.Dors.MultiTask
{
    [Serializable]
    public delegate object MultiTaskDelegate(object parameters);
    public class MultiTaskAction
    {
        public string MethodName { get; set; }
        public object ClassObject { get; set; }

        public MultiTaskDelegate Action
        {
            get
            {
                try
                {
                    MultiTaskDelegate ActionDelegate = (MultiTaskDelegate)Delegate.CreateDelegate(typeof(MultiTaskDelegate), ClassObject, MethodName, false);
                    MultiTaskDelegate ad = new MultiTaskDelegate(ActionDelegate);
                    return ad;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }
    }

    public class MultiTaskDelegateParameters
    {
        public MultiTaskDelegateParameters(string methodName, string className_methodBelongs, object parameters = null)
        {
            MethodName = methodName;
            ClassObjectString = className_methodBelongs;
            Parameters = parameters;
        }
        public MultiTaskDelegateParameters(string methodName, object classObject, object parameters = null)
        {
            MethodName = methodName;
            ClassObject = classObject;
            classObjectString = ClassObject.GetType().ToString();
            Parameters = parameters;
        }

        public string MethodName
        { get; set; }
        public object ClassObject
        { get; set; }

        private string classObjectString;
        public string ClassObjectString
        {
            get
            {
                return classObjectString;
            }
            set
            {
                classObjectString = value;
                ClassObject = ClassInstance.GetInstanceMethod(classObjectString);
            }
        }

        public object Parameters
        { get; set; }
    }

    public static class ClassInstance
    {
        public static object GetInstanceMethod(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return Activator.CreateInstance(type);
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
