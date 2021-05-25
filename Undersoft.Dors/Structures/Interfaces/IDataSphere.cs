using System;
using System.Collections.Generic;

namespace System.Dors
{
    public interface IDataSphere
    {       
        int  IndexOf(string TrellName);
        bool Have(string TrellName);

        object Collect(string trellName);
        object[] Collect(string[] trellNames);
    }
}