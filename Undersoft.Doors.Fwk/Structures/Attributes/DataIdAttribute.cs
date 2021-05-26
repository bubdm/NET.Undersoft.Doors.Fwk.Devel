using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Doors
{
    [AttributeUsage(AttributeTargets.All)]
    public sealed class DataIdxAttribute : Attribute
    {
        public string DataIdx;

        public DataIdxAttribute(string dataidx = null)
        {
            DataIdx = dataidx;
        }
    }
}
