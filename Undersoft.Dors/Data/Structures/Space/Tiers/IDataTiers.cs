using System.Collections.Generic;
using System.Dors;

namespace System.Dors.Data
{
    public interface IDataTiers
    {
        DataTrellis Trell
        { get; set; }

        int Count { get; }

        DataTier this[int index]
        { get; set; }
        DataTier this[long index]
        { get; set; }
        object this[int index, int field]
        { get; set; }

        DataTier[] AsArray();

        int IndexOf(DataTier value);
        
        bool Contains(DataTier data);
        bool Contains(ref object data);

        DataTier AddNew();
        DataTier Find(DataTier data);

        int   Add(DataTier value, bool save = false);
        void  AddRange(IList<DataTier> tiers);

        DataTier Put(DataTier value, bool save = false, bool sync = false, bool propagate = true);
        DataTier Put(ref object n, bool save = false, bool sync = false, bool propagate = true);
        DataTier Put(ref Noid keys, bool save = false);

        int  AddView(DataTier data);
        void AddViewRange(DataTier[] data);
        DataTier[] AppendView(DataTier[] data);

        void Delete(DataTier value);
        void DeleteRange(IList<DataTier> data);
    }
}