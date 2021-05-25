using System.Dors;
using System.Collections.Generic;
using System.Linq;

namespace System.Dors.Data
{

    public static class DataRetyper
    {
        public static IEnumerable<DataTrellis> AsEnumerable(this DataTrellises collection)
        {
            return collection.Cast<DataTrellis>().Take(collection.Count);           
        }
        //public static IEnumerable<DataTier> AsEnumerable(this DataTiers collection)
        //{
        //    return collection.Cast<DataTier>();
        //}
        //public static IEnumerable<IDataCells> AsEnumerable(this DataCells collection)
        //{
        //    return collection.Cast<IDataCells>();
        //}
        public static IEnumerable<DataPylon> AsEnumerable(this DataPylons collection)
        {
            return collection.Cast<DataPylon>().Take(collection.Count);
        }       
        public static IEnumerable<FilterTerm> AsEnumerable(this FilterTerms collection)
        {
            return collection.Cast<FilterTerm>().Take(collection.Count);
        }
        public static IEnumerable<SortTerm> AsEnumerable(this SortTerms collection)
        {
            return collection.Cast<SortTerm>().Take(collection.Count);
        }
        public static IEnumerable<DataRelay> AsEnumerable(this DataRelays collection)
        {
            return collection.Cast<DataRelay>().Take(collection.Count);
        }       
    }
}