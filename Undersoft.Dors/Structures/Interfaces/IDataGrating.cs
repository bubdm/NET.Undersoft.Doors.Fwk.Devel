using System.Dors;
using System.Collections.Generic;
using System.Collections;

namespace System.Dors
{
    public interface IDataGrating: IEnumerable
    {
        IDataTier iTier
        { get; set; }
        DataState State
        { get; }

        // SortedList<int, object> changes { get; set; }
        HashList<object> changes { get; set;}

        bool Add(int id, object value);
        bool Put(int id, object value);
        bool Add(IDataCells cubetier, int cubeid, int id, object value);
        bool Put(IDataCells cubetier, int cubeid, int id, object value);

        void AddRange(Vessel<object>[] input);
        void AddRange(Dictionary<long, object> input);
        void AddRange(object[][] input);

        void PutRange(Vessel<object>[] input);
        void PutRange(Dictionary<long, object> input);
        void PutRange(object[][] input);

        object ChangeOn(int x);       

        void Clear();
    }
}