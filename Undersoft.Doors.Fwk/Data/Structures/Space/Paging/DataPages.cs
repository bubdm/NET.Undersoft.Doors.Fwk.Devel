using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Doors;

namespace System.Doors.Data
{
    public class DataPages
    {
        public DataPage FirstPage { get; set; }
        public DataPage CurrentPage { get; set; }        

        private object access = new object();

        private SortedList<int, DataPage> paging = new SortedList<int, DataPage>();
        public DataPage Get(int key)
        {
            DataPage result = null;

            if (paging.ContainsKey(key))
            {
                result = paging[key];
                CurrentPage = result;
            }
            return result;
        }
        public void Set(int key, DataPage value)
        {
            object local = access;
            lock (local)
            {
                if (paging.ContainsKey(key))
                {
                    paging[key] = value;
                }
                else
                {
                    paging.Add(key, value);
                }
            }
        }
        public KeyValuePair<int, DataPage> SetPair
        {
            set
            {
                Set(value.Key, value.Value);
            }
        }
        public void NewRange(SortedList<int, DataSphere> pages)
        {
            paging.Clear();
            SortedList<int, DataPage> npages = new SortedList<int, DataPage>();
            foreach (KeyValuePair<int, DataSphere> ds in pages)
            {
                npages.Add(ds.Key, new DataPage(ds.Value.SphereId, ds.Key, ds.Value));
                if (ds.Key == 1)
                    FirstPage = npages[1];
            }
            paging.AddRange(npages);
        }
        public void PutRange(SortedList<int, DataSphere> pages)
        {
            SortedList<int, DataPage> npages = new SortedList<int, DataPage>();
            foreach (KeyValuePair<int, DataSphere> ds in pages)
            {
                npages.Add(ds.Key, new DataPage(ds.Value.SphereId, ds.Key, ds.Value));
                if (ds.Key == 1)
                    FirstPage = npages[1];
            }
            paging.PutRange(npages);
        }
        public SortedList<int, DataPage> Pages
        {
            get
            {
                return paging;
            }
            set
            {
                object local = access;
                lock (local)
                {
                    paging = value;
                }
            }
        }
        public void Clear()
        {
            paging.Clear();
        }
        public DataPage this[int page]
        {
            get
            {
                if (paging.ContainsKey(page))
                {
                    CurrentPage = paging[page];
                    return CurrentPage;
                }
                else
                    return null;
            }

        }
    }
}

 


