using System;

namespace System.Dors
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