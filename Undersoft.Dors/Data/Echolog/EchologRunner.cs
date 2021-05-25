using System;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace System.Dors.Data.Echolog
{
    public static class EchologRunner
    {
        public static void Start()
        {
            Echolog.Start(2);
        }
        public static void Stop()
        {
            Echolog.Stop();
        }
    }
}
