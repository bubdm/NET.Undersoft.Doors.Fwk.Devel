using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Doors.Data
{
    public class DataPage
    {
        public DataPage()
        { }
        public DataPage(string setid, int page, DataSphere pageset)
        {
            SetId = setid;
            Page = page;
            PageSet = pageset;
        }

        public string SetId { get; set; }
        public int Page { get; set; }
        public int RowsCount { get; set; }
        public DataSphere PageSet { get; set; }   
    }
}

 


