using System;
using System.Dors;

namespace System.Dors
{
    public interface IDataPylons
    {
        IDataPylon this[string PylonName] { get; }
        IDataPylon this[int id] { get; }
    }   

}