using System;
using System.Doors;

namespace System.Doors
{
    public interface IDataPylon
    {
        int PylonId { get; set; }
        Type DataType { get; set; }
        int Ordinal { get; set; }
        string PylonName { get; set; }
    }
}