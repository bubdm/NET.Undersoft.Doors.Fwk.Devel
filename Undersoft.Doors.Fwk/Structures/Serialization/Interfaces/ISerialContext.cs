using System;
using System.IO;
using System.Collections;
using System.Doors;

namespace System.Doors
{
    public interface ISerialContext
    {
        DepotSite IdentitySite { get; }

        long BlockSize { get; set; }
        int  BlockOffset { get; set; }

        byte[] SerialBlock { get; set; }
        byte[] DeserialBlock { get; }

        int SerialBlockId { get; set; }
        int DeserialBlockId { get; set; }
    }
}



