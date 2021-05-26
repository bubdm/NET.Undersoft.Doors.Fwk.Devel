using System;
using System.Collections.Concurrent;

namespace System.Doors
{
    public interface IDataBank
    {
        IDataLog DataLog { get; set; }
        IDataLog NetLog { get; set; }

        DepotIdentity ServiceIdentity
        { get; }
        DepotIdentity ServerIdentity
        { get; }

        string DrivePath { get; set; }
        ConcurrentDictionary<int, object> Registry { get; set; }
    }
}