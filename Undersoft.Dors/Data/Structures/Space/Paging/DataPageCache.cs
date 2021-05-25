using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Dors.Data
{
    [Serializable]
    public class DataPageCache
    {
        public int Counter { get; set; }
        public int Page { get; set; }
        public int SkipRows { get; set; }
        public int TakeRows { get; set; }
    }
}

 


