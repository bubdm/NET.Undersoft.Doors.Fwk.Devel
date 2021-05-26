using System;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace System.Doors.Data.Depot.Netlog
{
    public static class NetlogRunner
    {
        public static void Start()
        {
            Netlog.Start(2);
        }
        public static void Stop()
        {
            Netlog.Stop();
        }
    }
}
