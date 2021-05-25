using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Dors
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
