using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Data;
using System.Dors;

namespace System.Dors.Data
{
    [JsonIgnore]
    [Serializable]
    public class DataSort
    {
        public DataTrellis Trell;

        public SortTerms Terms;

        public DataSort(DataTrellis nTable)
        {
            Trell = nTable;
            Terms = new SortTerms(nTable);
        }

    }
}
