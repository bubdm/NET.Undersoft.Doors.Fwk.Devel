using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Doors
{
    public static class CoreInfo
    {
        public static string AssemblyGuid
        {
            get
            {
                object[] attributes;
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly is null)
                {
                    attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(GuidAttribute), false);
                }
                else
                {
                    attributes = entryAssembly.GetCustomAttributes(typeof(GuidAttribute), false);
                }
                if (attributes.Length == 0) { return String.Empty; }
                return ((GuidAttribute)attributes[0]).Value.ToUpper();
            }
        }
    }
}
