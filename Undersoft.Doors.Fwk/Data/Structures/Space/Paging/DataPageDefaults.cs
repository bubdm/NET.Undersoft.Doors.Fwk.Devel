using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Doors.Data
{
    public static class DataPageDefaults
    {
        private static DataPageDetails defaults = new DataPageDetails();
        public static DataPageDetails Defaults
        {
            get
            {
                defaults.Page = 1;
                defaults.PageCount = 1;
                defaults.RowsCount = 0;
                defaults.CachedPages = 1;
                defaults.PageSize = 50;
                defaults.First = 1;
                defaults.Last = 1;
                defaults.Glide = 3;
                defaults.PageActive = true;
                defaults.SkipRows = 0;
                defaults.TakeRows = 0;
                return defaults;
            }
        }
    }
}

 


