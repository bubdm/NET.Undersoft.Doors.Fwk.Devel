using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Doors;

namespace System.Doors.Data
{
    [JsonObject]
    [Serializable]
    public class DataPageDetails
    {
        public DataPageDetails()
        {
            CachePageList = new List<DataPageCache>();

            Page = 1;
            PageCount = 1;
            RowsCount = 0;
            CachedPages = 1;
            PageSize = 50;
            First = 1;
            Last = 1;
            Glide = 2;
            PageActive = false;
            SkipRows = 0;
            TakeRows = 0;
        }
        public DataPageDetails(bool defaults = false)
        {
            CachePageList = new List<DataPageCache>();
            if (!defaults)
            {
                Page = 1;
                PageCount = 1;
                RowsCount = 0;
                CachedPages = 1;
                PageSize = 50;
                First = 1;
                Last = 1;
                Glide = 2;
                PageActive = false;
                SkipRows = 0;
                TakeRows = 0;
            }
            else
            {
                DataPageDetails pd = DataPageDefaults.Defaults;
                Page = pd.Page;
                PageCount = pd.PageCount;
                RowsCount = pd.RowsCount;
                CachedPages = pd.CachedPages;
                PageSize = pd.PageSize;
                First = pd.First;
                Last = pd.Last;
                Glide = pd.Glide;
                PageActive = pd.PageActive;
                SkipRows = pd.SkipRows;
                TakeRows = pd.TakeRows;
            }
        }

        [JsonIgnore]
        public List<DataPageCache> CachePageList
        {
            get;
            set;
        }

        public bool PageActive { get; set; }
        public int Page { get; set; }
        public int PageCount { get; set; }
        public int RowsCount { get; set; }
        public int CachedPages { get; set; }
        public int PageSize { get; set; }
        public int First { get; set; }
        public int Last { get; set; }
        public int Glide { get; set; }      
        public int SkipRows { get; set; }
        public int TakeRows { get; set; }

        public int ComputePageCount(int tiersCount)
        {
            PageCount = (tiersCount / PageSize) + (((tiersCount % PageSize) > 0) ? 1 : 0);
            return PageCount;
        }
    }
}

 


