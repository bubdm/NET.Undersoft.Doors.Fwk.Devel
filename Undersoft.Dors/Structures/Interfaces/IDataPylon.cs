using System;
using System.Dors;

namespace System.Dors
{
    public interface IDataPylon
    {
        int PylonId { get; set; }
        Type DataType { get; set; }
        int Ordinal { get; set; }
        string PylonName { get; set; }
    }
}