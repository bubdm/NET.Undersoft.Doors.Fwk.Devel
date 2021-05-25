using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Dors.Data.Afectors.Sql
{
    public class NetEcho : Exception
    {
        public NetEcho(string message)
            : base(message)
        {
        }
    }
}
