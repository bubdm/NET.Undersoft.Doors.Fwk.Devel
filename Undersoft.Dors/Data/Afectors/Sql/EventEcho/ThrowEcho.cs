using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Dors.Data.Afectors.Sql
{
    public class ExceptionEcho : Exception
    {
        public ExceptionEcho(string message)
            : base(message)
        {
        }
    }
}
