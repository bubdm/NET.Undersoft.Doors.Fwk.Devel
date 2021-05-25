using System;
using System.Collections.Generic;
using System.Linq;
using System.Dors;
using System.ComponentModel;

namespace System.Dors.Data
{
    
    public static class DataQuery
    {
        public static DataTiers Query(this DataTrellis nTrellis, out bool sorted, out bool filtered, int stage = 1)
        {
            DataFilter Filter = nTrellis.Filtering;
            DataSort Sort = nTrellis.Sorting;
            filtered = (Filter.Terms.Count > 0) ? true : false;
            sorted = (Sort.Terms.Count > 0) ? true : false;
            return ResolveQuery(nTrellis, Filter, Sort, stage);
        }
        public static DataTiers Query(this DataTrellis nTrellis, int stage = 1,  FilterTerms filter = null, SortTerms sort = null, bool saveonly = false, bool clearonend = false)
        {
            DataFilter Filter = nTrellis.Filtering;
            DataSort Sort = nTrellis.Sorting;
            DataModes mode = nTrellis.Mode;
            if (filter != null)
            {
                //Filter.Terms.Clear();
                Filter.Terms.AddNewRange(filter.AsEnumerable().ToArray());
            }
            if (sort != null)
            {
                //Sort.Terms.Clear();
                Sort.Terms.AddNewRange(sort.AsEnumerable().ToArray());
            }
            if (!saveonly)
            {
                DataTiers result = ResolveQuery(nTrellis, Filter, Sort, stage);
                if (clearonend)
                {
                    nTrellis.Filtering.Terms.Clear();
                    nTrellis.Filtering.Evaluator = null;
                    nTrellis.TiersView.FilterEvaluator = null;
                }
                return result;
            }
            return null;
        }
        public static DataTiers Query(this DataTrellis nTrellis, List <FilterTerm> filterList, List<SortTerm> sortList, bool saveonly = false, bool clearonend = false, int stage = 1)
        {
            DataFilter Filter = nTrellis.Filtering;
            DataSort Sort = nTrellis.Sorting;
            DataModes mode = nTrellis.Mode;
            if (filterList != null)
            {
                //Filter.Terms.Clear();
                Filter.Terms.AddNewRange(filterList);
            }
            if (sortList != null)
            {
                //Sort.Terms.Clear();
                Sort.Terms.AddNewRange(sortList);
            }
            if (!saveonly)
            {
                DataTiers result = ResolveQuery(nTrellis, Filter, Sort, stage);
                if (clearonend)
                    nTrellis.Filtering.Terms.Clear();
                return result;
            }
            return null;
        }

        public static DataTiers Query(this DataTiers tiers, out bool sorted, out bool filtered, int stage = 1)
        {
            DataFilter Filter = tiers.Trell.Filtering;
            DataSort Sort = tiers.Trell.Sorting;

            filtered = (Filter.Terms.Count > 0) ? true : false;
            sorted = (Sort.Terms.Count > 0) ? true : false;
            return ResolveQuery(tiers, Filter, Sort, stage);
        }
        public static DataTiers Query(this DataTiers tiers, int stage = 1, FilterTerms filter = null, SortTerms sort = null, bool saveonly = false, bool clearonend = false)
        {
            DataFilter Filter = tiers.Trell.Filtering;
            DataSort Sort = tiers.Trell.Sorting;

            if (filter != null)
            {
               // Filter.Terms.Clear();
                Filter.Terms.AddNewRange(filter.AsEnumerable().ToArray());
            }
            if (sort != null)
            {
               // Sort.Terms.Clear();
                Sort.Terms.AddNewRange(sort.AsEnumerable().ToArray());
            }
            if (!saveonly)
            {
                DataTiers result = ResolveQuery(tiers, Filter, Sort, stage);
                if (clearonend)
                {
                    tiers.Trell.Filtering.Terms.Clear();
                    tiers.Trell.Filtering.Evaluator = null;
                    tiers.TiersView.FilterEvaluator = null;
                }
                return result;
            }
            return null;
        }
        public static DataTiers Query(this DataTiers tiers, List<FilterTerm> filterList, List<SortTerm> sortList, bool saveonly = false, bool clearonend = false, int stage = 1)
        {
            DataFilter Filter = tiers.Trell.Filtering;
            DataSort Sort = tiers.Trell.Sorting;
            if (filterList != null)
            {
                //Filter.Terms.Clear();
                Filter.Terms.AddNewRange(filterList);
            }
            if (sortList != null)
            {
                //Sort.Terms.Clear();
                Sort.Terms.AddNewRange(sortList);
            }
            if (!saveonly)
            {
                DataTiers result = ResolveQuery(tiers, Filter, Sort, stage);
                if (clearonend)
                {
                    tiers.Trell.Filtering.Terms.Clear();
                    tiers.Trell.Filtering.Evaluator = null;
                    tiers.TiersView.FilterEvaluator = null;
                }
                return result;
            }
            return null;
        }
        public static DataTiers Query(this DataTiers tiers, DataTier[] appendtiers, int stage = 1)
        {
            DataFilter Filter = tiers.Trell.Filtering;
            DataSort Sort = tiers.Trell.Sorting;
            return ResolveQuery(tiers, Filter, Sort, stage, appendtiers);
        }

        public static DataTier[] Query(this DataTier[] tierarray, Func<DataTier, bool> evaluator)
        {
            return tierarray.Where(evaluator).ToArray();
        }
        public static DataTiers Query(this DataTiers tiers, Func<DataTier, bool> evaluator)
        {
            DataTiers view = tiers.TiersView = new DataTiers(tiers.Trell, new DataTier[0], tiers.Mode);
            view.AddViewRange(tiers.AsEnumerable().AsQueryable().Where(evaluator).ToArray());
            return view;
        }

        private static DataTiers ResolveQuery(DataTiers tiers, DataFilter Filter, DataSort Sort, int stage = 1, DataTier[] appendtiers = null)
        {
            FilterStage filterStage = (FilterStage)Enum.ToObject(typeof(FilterStage), stage);
            int filtercount = Filter.Terms.AsEnumerable().Where(f => f.Stage.Equals(filterStage)).ToArray().Length;
            int sortcount = Sort.Terms.Count;

            SyncDevisorAddedTiers(tiers);

            if (filtercount > 0)
                if (sortcount > 0)
                    return ExecuteQuery(tiers, Filter, Sort, stage, appendtiers);
                else
                    return ExecuteQuery(tiers, Filter, null, stage, appendtiers);
            else if (sortcount > 0)
                return ExecuteQuery(tiers, null, Sort, stage, appendtiers);
            else
                return ExecuteQuery(tiers, null, null, stage, appendtiers);

        }
        private static DataTiers ResolveQuery(DataTrellis nTrellis, DataFilter Filter, DataSort Sort, int stage = 1)
        {
            DataModes mode = nTrellis.Mode;
            FilterStage filterStage = (FilterStage)Enum.ToObject(typeof(FilterStage), stage);
            int filtercount = Filter.Terms.AsEnumerable().Where(f => f.Stage.Equals(filterStage)).ToArray().Length;
            int sortcount = Sort.Terms.Count;

           SyncDevisorAddedTiers(nTrellis.Tiers);

            if (filtercount > 0)
                if (sortcount > 0)
                    return ExecuteQuery(nTrellis.Tiers, Filter, Sort, stage);
                else
                    return ExecuteQuery(nTrellis.Tiers, Filter, null, stage);
            else if (sortcount > 0)
                return ExecuteQuery(nTrellis.Tiers, null, Sort, stage);
            else
                return ExecuteQuery(nTrellis.Tiers, null, null, stage);
        }

        private static DataTiers ExecuteQuery(DataTiers _tiers, DataFilter filter, DataSort sort, int stage = 1, DataTier[] appendtiers = null)
        {
            DataTrellis trell = _tiers.Trell;
            DataTiers tiers = null;
            DataTiers view = null;
            if (_tiers.Mode != DataModes.Sims && _tiers.Mode != DataModes.SimsView)
                view = trell.Tiers.TiersView;
            else
                view = trell.Sims.TiersView;

            if (appendtiers == null)
                if (stage > 1)
                    tiers = view;
                else if (stage < 0)
                {
                    tiers = _tiers;
                    view = _tiers.TiersView = new DataTiers(trell, new DataTier[0], _tiers.Mode);
                }
                else
                {
                    if (_tiers.Mode != DataModes.Sims && _tiers.Mode != DataModes.SimsView)
                        tiers = trell.Tiers;
                    else
                        tiers = trell.Sims;
                }

            if (filter != null && filter.Terms.Count > 0)
            {
                filter.Evaluator = filter.GetExpression(stage).Compile();
                view.FilterEvaluator = filter.Evaluator;

                if (sort != null && sort.Terms.Count > 0)
                {
                    bool isFirst = true;
                    IEnumerable<DataTier> tsrt = null;
                    IOrderedQueryable<DataTier> ttby = null;
                    if (appendtiers != null)
                        tsrt = appendtiers.AsEnumerable().Where(filter.Evaluator);
                    else
                        tsrt = tiers.AsEnumerable().Where(filter.Evaluator);

                    foreach (SortTerm fcs in sort.Terms)
                    {
                        if (isFirst)
                            ttby = tsrt.AsQueryable().OrderBy(o => o[fcs.PylonName], fcs.Direction, Comparer<object>.Default);
                        else
                            ttby = ttby.ThenBy(o => o[fcs.PylonName], fcs.Direction, Comparer<object>.Default);
                        isFirst = false;
                    }

                    if (appendtiers != null)
                        view.AppendView(ttby.ToArray());
                    else
                        view.AddViewRange(ttby.ToArray());
                }
                else
                {
                    if (appendtiers != null)
                        view.AppendView(appendtiers.AsQueryable().Where(filter.Evaluator).ToArray());
                    else
                        view.AddViewRange(tiers.AsEnumerable().AsQueryable().Where(filter.Evaluator).ToArray());
                }
            }
            else if (sort != null && sort.Terms.Count > 0)
            {
                view.FilterEvaluator = null;
                view.Trell.Filtering.Evaluator = null;

                bool isFirst = true;
                IOrderedQueryable<DataTier> ttby = null;
            
                foreach (SortTerm fcs in sort.Terms)
                {
                    if (isFirst)
                        if (appendtiers != null)
                            ttby = appendtiers.AsQueryable().OrderBy(o => o[fcs.PylonName], fcs.Direction, Comparer<object>.Default);
                        else
                            ttby = tiers.AsEnumerable().AsQueryable().OrderBy(o => o[fcs.PylonName], fcs.Direction, Comparer<object>.Default);
                    else
                        ttby = ttby.ThenBy(o => o[fcs.PylonName], fcs.Direction, Comparer<object>.Default);

                    isFirst = false;
                }

                if (appendtiers != null)
                    view.AppendView(ttby.ToArray());
                else
                    view.AddViewRange(ttby.ToArray());
            }
            else
            {
                if (stage < 2)
                {
                    view.FilterEvaluator = null;
                    view.Trell.Filtering.Evaluator = null;
                }

                if (appendtiers != null)
                    view.AppendView(appendtiers);
                else
                    view.AddViewRange(tiers.AsArray());
            }

            view.Trell.PagingDetails.ComputePageCount(view.Count);
            if (stage > 0)
            {
                if (_tiers.Mode != DataModes.Sims && _tiers.Mode != DataModes.SimsView)
                    trell.TiersView = view;
                else
                    trell.SimsView = view;
            }
            return view;
        }
       
        //private static DataTiers ExecuteQuery(DataTrellis _nTrellis, DataFilter filter, DataSort sort, int stage = 1)
        //{
        //    DataTiers tiers = null;
        //    DataTiers view = null;
        //    if (_nTrellis.Mode != DataModes.Sims && _nTrellis.Mode != DataModes.SimsView)
        //        view = _nTrellis.Tiers.TiersView;
        //    else
        //        view = _nTrellis.Sims.TiersView;

        //    if (stage > 1)
        //        tiers = view;
        //    else if (stage < 0)
        //    {
        //        tiers = _nTrellis.Tiers;
        //    }
        //    else
        //        tiers = _nTrellis.Tiers;          


        //    if (filter != null && filter.Terms.Count > 0)
        //        if (sort != null && sort.Terms.Count > 0)
        //        {
        //            bool isFirst = true;
        //            IOrderedQueryable<DataTier> ttby = null;

        //            IEnumerable<DataTier> tsrt = tiers.AsEnumerable().Where(filter.GetExpression(stage).Compile());

        //            foreach (SortTerm fcs in sort.Terms)
        //            {
        //                if (isFirst)
        //                    ttby = tsrt.AsQueryable().OrderBy(o => o[fcs.PylonName], fcs.Direction, Comparer<object>.Default);
        //                else
        //                    ttby = ttby.ThenBy(o => o[fcs.PylonName], fcs.Direction, Comparer<object>.Default);
        //                isFirst = false;
        //            }
        //            view.AddViewRange(ttby.ToArray());
        //        }
        //        else
        //            view.AddViewRange(tiers.AsEnumerable().AsQueryable().Where(filter.GetExpression(stage).Compile()).ToArray());
        //    else if (sort != null && sort.Terms.Count > 0)
        //    {
        //        bool isFirst = true;
        //        IOrderedQueryable<DataTier> ttby = null;

        //        foreach (SortTerm fcs in sort.Terms)
        //        {
        //            if (isFirst)
        //                ttby = tiers.AsEnumerable().AsQueryable().OrderBy(o => o[fcs.PylonName], fcs.Direction, Comparer<object>.Default);
        //            else
        //                ttby = ttby.ThenBy(o => o[fcs.PylonName], fcs.Direction, Comparer<object>.Default);

        //            isFirst = false;
        //        }
        //        view.AddViewRange(ttby.ToArray());
        //    }
        //    else
        //        view.AddViewRange(tiers.AsArray());
        //    view.Trell.PagingDetails.ComputePageCount(view.Count);
        //    return view;
        //}       

        public static bool SyncDevisorAddedTiers(DataTiers tiers)
        {
            if (!DataMode.IsViewMode(tiers.Mode) && !tiers.Trell.Devisor.IsPrime)
            {
                int devCount = tiers.Trell.Devisor.Count;
                int emuCount = tiers.Trell.Count;
                if (devCount != emuCount || (devCount > 0 && emuCount > 0) &&
                     (tiers.Trell.Devisor.Tiers[devCount - 1].PrimeIndex != tiers.Trell.Tiers[emuCount - 1].PrimeIndex))
                    {
                        int diffCount = devCount - emuCount;
                        for (int i = devCount - diffCount; i < diffCount; i++)
                        {
                            tiers.Trell.Tiers.Put(new DataTier(tiers.Trell, tiers.Trell.Devisor.Tiers[i]));
                        }
                        return true;
                    }
            }
            return false;
        }
    }

    public enum QueryMode
    {
        Standard,
        Simulate
    }
}
