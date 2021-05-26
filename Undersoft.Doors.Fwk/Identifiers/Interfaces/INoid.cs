using System;

namespace System.Doors
{
    public interface INoid
    {
        byte[] GetShah();

        ushort GetDriveId();
        ushort GetSectorId();
        ushort GetLineId();

        string GetMapPath();
        string GetMapName();

    }   
}