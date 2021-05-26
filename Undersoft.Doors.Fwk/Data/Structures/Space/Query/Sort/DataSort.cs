using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Data;
using System.Doors;

namespace System.Doors.Data
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
