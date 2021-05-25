using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Data;

namespace System.Dors.Data
{
    
    public class DataPagination
    {
        DataTrellis Trell;
        private DataSphere Set;

        public DataPagination(DataSphere _Set)
        {
            Set = _Set;
        }
        public DataPagination(DataTrellis trell)
        {
            Trell = trell;
            Set = Trell.Sphere;
        }

        public void CreateNew()
        {
            SortedList<int, DataSphere> noexdds = CombineTrellsForPaging();
            Set.Pages.NewRange(noexdds);
        }
        public void UpdateAdd()
        {
            SortedList<int, DataSphere> noexdds = CombineTrellsForPaging();
            Set.Pages.PutRange(noexdds);
        }

        public List<DataTrellis> GetNoExpandForPaging
        {
            get
            {
                return Set.RootTrells.AsEnumerable().Where(rt => !Set.ExpandTrells.AsEnumerable().Where(et => rt.TrellName == et.TrellName).Any()).ToList();
            }
        }
        public List<DataTrellis> GetExpandForPaging
        {
            get
            {
                return Set.ExpandTrells.AsEnumerable().ToList();
            }
        }

        public SortedList<int, DataSphere> PrepareRelatedTrells()
        {
            SortedList<int, DataSphere> nns = new SortedList<int, DataSphere>();
            DataTrellis[] RowsToSkip = GetExpandForPaging.Select(et => PagingMathHandler(et)).ToArray();
            int maxPageCount = 0;

            foreach (DataTrellis trell in RowsToSkip)
            {
                int rowsCount = trell.PagingDetails.TakeRows;
                int dsCount = rowsCount / trell.PagingDetails.PageSize + (rowsCount % trell.PagingDetails.PageSize > 0 ? 1 : 0);
                maxPageCount = Math.Max(dsCount, maxPageCount);
            }

            for (int i = 0; i < maxPageCount; i++)
                nns.Add(i + 1, (DataSphere)new DataSphere("temp").Imitate(Set));

            foreach (DataTrellis trell in RowsToSkip)
            {
                DataTiers mms = new DataTiers(trell, trell.Tiers.AsEnumerable().Skip(trell.PagingDetails.SkipRows).Take(trell.PagingDetails.TakeRows).ToArray());
                List<List<DataTier[]>> cmjs = mms.GetChildList().Select(j => j.List.Select(c => c.Child).ToList()).ToList();
                foreach (DataPageCache cu in trell.PagingDetails.CachePageList)
                {
                    DataTier[] mjs = mms.AsEnumerable().Skip(cu.SkipRows).Take(cu.TakeRows).ToArray();
                    List<List<DataTier[]>> tcmjs = cmjs.Skip(cu.SkipRows).Take(cu.TakeRows).ToList();
                    nns[cu.Counter].Trells[trell.TrellName].Tiers = new DataTiers(trell, mjs);
                    foreach (List<DataTier[]> tmjs in tcmjs)
                    {
                        if (tmjs.Count > 0)
                        {
                            foreach (DataTier[] amjs in tmjs)
                            {
                                if (amjs.Length > 0)
                                {
                                    string tname = amjs.First().Trell.TrellName;
                                    nns[cu.Counter].Trells[tname].Tiers.AddRange(amjs);
                                }
                            }
                        }
                    }
                }
            }
            return nns;
        }
        public SortedList<int, DataSphere> CombineTrellsForPaging()
        {
            SortedList<int, DataSphere> noexdds = PrepareRelatedTrells();
            foreach (DataSphere ds in noexdds.Values)
            {
                foreach (DataTrellis trell in GetNoExpandForPaging)
                {
                    string pname = trell.TrellName;
                    ds.Trells[pname].Tiers = trell.Tiers;
                }               
            }       
            return noexdds;
        }

        public DataTrellis PagingMathHandler(DataTrellis trell)
        {
            int pageSize = trell.PagingDetails.PageSize;
            if (pageSize > 0)
            {
                int rowCount = trell.Tiers.Count;
                int pageCount = rowCount / pageSize + (rowCount % pageSize > 0 ? 1 : 0);
                trell.PagingDetails.PageCount = pageCount;
                trell.PagingDetails.RowsCount = rowCount;
                int Page = trell.PagingDetails.Page;
                int PageId = Page - 1;
                int lastPageSize = pageSize;
                int vectorPage = Convert.ToInt32((trell.PagingDetails.CachedPages - (trell.PagingDetails.CachedPages % 2)) / 2);
                int vectorUp = vectorPage;
                int vectorDown = vectorPage;

                int startPageId = (PageId - vectorDown);
                if (startPageId < 0)
                {
                    vectorUp = vectorPage + Math.Abs(0 - vectorPage);
                    vectorDown = vectorDown - Math.Abs(0 - vectorPage);
                    startPageId = 0;
                }

                int endPageId = (PageId + vectorUp);
                if (endPageId > pageCount - 1)
                {
                    vectorDown = vectorPage + Math.Abs((pageCount - 1) - endPageId);
                    vectorUp = vectorUp - Math.Abs((pageCount - 1) - endPageId);
                    endPageId = pageCount - 1;
                }

                startPageId = (PageId - vectorDown);
                if (startPageId < 0)
                {
                    vectorDown = vectorDown - Math.Abs(0 - vectorPage);
                    startPageId = 0;
                }

                int midPageId = PageId + vectorUp - vectorDown;
                if (((endPageId * pageSize) + lastPageSize) > rowCount)
                {
                    lastPageSize = rowCount - (endPageId * pageSize);
                }

                int realCacheCount = vectorUp + vectorDown;
                if(realCacheCount > pageCount)
                {
                    realCacheCount = (pageCount - 1 < 0) ? 0 : pageCount-1;
                }

                int takeRows = (realCacheCount * pageSize) + lastPageSize;
                for (int i = 0; i < realCacheCount+1; i++)
                {
                    trell.PagingDetails.CachePageList.Add(new DataPageCache()
                    {
                        Counter = i + 1,
                        Page = (startPageId + 1) + i,
                        SkipRows = 0 + (i * pageSize),
                        TakeRows = (i + 1 < realCacheCount + 1) ? pageSize : lastPageSize
                    });
                }

                trell.PagingDetails.SkipRows = startPageId * pageSize;
                trell.PagingDetails.TakeRows = takeRows;
                trell.PagingDetails.First = startPageId + 1;
                trell.PagingDetails.Last = endPageId + 1;
                trell.PagingDetails.CachedPages = realCacheCount + 1;

                return trell;
            }
            else
                return trell;

        }
    }
}

