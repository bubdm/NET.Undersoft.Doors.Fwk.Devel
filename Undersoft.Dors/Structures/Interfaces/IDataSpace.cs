using System;
using System.Collections.Concurrent;

namespace System.Dors
{
    public interface IDataSpace
    {
        DepotIdentity ServiceIdentity
        { get; set; }
        DepotIdentity ServerIdentity
        { get; set; }

        string DrivePath { get; set; }
        ConcurrentDictionary<int, object> Registry { get; set; }
    }
}