using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Dors
{
    public static class DataCore
    {
        public static IDataBank Bank
        {
            get;
            set;
        }
        public static IDataSpace Space
        {
            get;
            set;
        }

        public static object GetDefaultValue(this Type type)
        {
            if (type == null || !type.IsValueType || type == typeof(void))
                return null;

            if (type.IsPrimitive || !type.IsNotPublic)
            {
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        "{" + MethodInfo.GetCurrentMethod() + "} Error:\n\nThe Activator.CreateInstance method could not " +
                        "create a default instance of the supplied value type <" + type +
                        "> (Inner Exception message: \"" + e.Message + "\")", e);
                }
            }
            throw new ArgumentException("{" + MethodInfo.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type +
                    "> is not a publicly-visible type, so the default value cannot be retrieved");
        }
    }
}
