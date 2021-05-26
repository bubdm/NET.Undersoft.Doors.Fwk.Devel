using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace System.Doors.Intelops
{
    
    public static class CallEstimatorInstance
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

        //public static object LoadLibrary(string strFullyQualifiedName)
        //{
        //    //Assembly.Load("C://Windows/")

        //}
    }

}
