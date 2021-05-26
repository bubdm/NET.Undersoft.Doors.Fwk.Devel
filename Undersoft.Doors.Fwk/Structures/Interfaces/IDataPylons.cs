using System;
using System.Doors;

namespace System.Doors
{
    public interface IDataPylons
    {
        IDataPylon this[string PylonName] { get; }
        IDataPylon this[int id] { get; }
    }   

}