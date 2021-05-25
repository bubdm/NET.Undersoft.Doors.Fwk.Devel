using System.Quantic.Core;
using System.Collections.Generic;

namespace System.Quantic.Data
{
    public interface IDataChanges
    {
        DataTiers Tiers { get; set; }
        DataState State { get; }

        int PylonCount { get; }
        int NoidOrdinal { get; }
        int QuidOrdinal { get; }

        HashList<object> changes { get; set;}

        bool Add(IDataCells tier, int index, object value);
        bool Add(int tierid, int index, object value);

        bool Put(IDataCells tier, int index, object value);
        bool Put(int tierid, int index, object value);

        bool Add(IDataCells tier, IDataCells cubetier, int cubeordinal, int index, object value);
        bool Add(int tierid, IDataCells cubetier, int cubeordinal, int index, object value);

        bool Put(IDataCells tier, IDataCells cubetier, int cubeordinal, int index, object value);
        bool Put(int tierid, IDataCells cubetier, int cubeordinal, int index, object value);

        IDataChanges AddRange(Dictionary<long, object> input);
        IDataChanges AddRange(object[][] input);

        IDataChanges PutRange(Dictionary<long, object> input);
        IDataChanges PutRange(object[][] input);

        object ChangeOn(object result, int tierid, int x);
        object ChangeOn(object result, IDataCells tier, int x);
        object ChangeOn(int tierid, int x);
        object ChangeOn(IDataCells tier, int x);

        IList<Vessel<object>>  EditedCells(IDataCells tier);

        void SaveChanges();

        void Clear();
    }
}