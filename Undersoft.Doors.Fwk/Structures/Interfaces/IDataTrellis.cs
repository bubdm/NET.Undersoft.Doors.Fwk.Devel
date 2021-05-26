using System;
using System.Collections.Generic;

namespace System.Doors
{
    public interface IDataTrellises
    {
        string SphereId { get; set; }
        DataConfig Config { get; set; }
        DataState State { get; set; }
}

    public interface IDataTrellis
    {
        string TrellName
        { get; set; }
        string DisplayName
        { get; set; }
        string MappingName
        { get; set; }
       
        bool Visible
        { get; set; }
        bool IsPrime
        { get; set; }

        DataModes Mode
        { get; set; }

        IDataPylons iPylons { get; }

        int  NoidOrdinal
        { get; }

        short EditLevel
        { get; set; }
        short EditLength
        { get; set; }
        short SimLevel
        { get; set; }
        short SimLength
        { get; set; }

        int  IndexOf(object tier);
        bool HasPylon(string PylonName);
    }
}