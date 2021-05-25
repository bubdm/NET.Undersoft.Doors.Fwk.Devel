using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace System.Dors.Data
{
    public static class TestDataCellsTreatment
    {
        public static void TestTreatmentBuild()
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            DataSphere iSet1 = DataSpace.Area["Data_1"]["Data_1_1"].SpheresIn["Data_1_1_1_1"];
            DataTrellis[] itabs0 = iSet1.Trells.Collect(iSet1.Trells.AsEnumerable().Select(t => t.TrellName).ToArray());
            itabs0[1].Filter.AddRange(new List<FilterTerm>()
            {
               new FilterTerm("updated", "EqualOrMore", DateTime.Now.AddDays(-150), "And", 1),
                new FilterTerm("bestcount",  "EqulOrLess", DateTime.Now, "And", 1),
                new FilterTerm("bestcount", "EqualOrMore", 1)
                //new FilterTerm(CriteriaType.multi, itab0.NColumns["search_status"], OperandType.None, new List<object>() { "ok","wrong" }, OperandType.None ) 
            });
            itabs0[1].Sort.Add(new SortTerm("NazwaAmz", "ASC", 0));
            DataTiers box0 = itabs0[1].Query();
            DataTiers cTotalsA = itabs0[1].Total();
            DataTiers cRowsA = itabs0[0].SubTotal();
            DataPagination dpa = new DataPagination(itabs0[0].Sphere);
            SortedList<int, DataSphere> mdpa = dpa.CombineTrellsForPaging();
            itabs0[0].PagingDetails.CachePageList.Clear();
            itabs0[0].PagingDetails.Page = 14;
            DataPagination dpab = new DataPagination(itabs0[0].Sphere);
            SortedList<int, DataSphere> mdpab = dpab.CombineTrellsForPaging();
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            DataSphere iSet2 = DataSpace.Area["Data_1"]["Data_1_1"].SpheresIn["Data_1_1_1_2"];
            DataTrellis[] itabs1 = iSet2.Trells.Collect(iSet2.Trells.AsEnumerable().Select(t => t.TrellName).ToArray());
            itabs1[1].Filter.AddRange(new List<FilterTerm>()
            {
                new FilterTerm("updated", "EqualOrMore", DateTime.Now.AddDays(-150), "And", 1),
                new FilterTerm("bestcount",  "EqulOrLess", DateTime.Now, "And", 1),
                new FilterTerm("bestcount", "EqualOrMore", 1)
                //new FilterTerm(CriteriaType.multi, itab1.NColumns["search_status"], OperandType.None, new List<object>() { "ok","wrong" }, OperandType.None )
            });
            itabs1[1].Sort.Add(new SortTerm("NazwaAmz", "ASC", 0));
            DataTiers box1 = itabs1[1].Query();
            DataTiers cTotalsB = itabs1[1].Total();
            DataTiers cRowsB = itabs1[0].SubTotal();
            DataPagination dpb = new DataPagination(itabs1[0].Sphere);
            SortedList<int, DataSphere> mdpb = dpb.CombineTrellsForPaging();
            itabs1[0].PagingDetails.CachePageList.Clear();
            itabs1[0].PagingDetails.Page = 10;
            DataPagination dpba = new DataPagination(itabs1[0].Sphere);
            SortedList<int, DataSphere> mdpba = dpba.CombineTrellsForPaging();
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        }
    }
}
