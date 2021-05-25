using System;
using System.Collections.Generic;
using System.Dors;

namespace System.Dors
{
    public interface IDataCells
    {
        IDataTier iTier { get; }
        IDataNative iN { get; }

        bool IsPrime { get; }
        bool IsSingleton { get; }
        bool IsCube { get; }
        bool Edited { get; set; }

        int PrimeIndex { get; set; }
        int Index { get; set; }

        IDataCells[] AsArray();

        IEnumerable<IDataCells> AsEnumerable();

        object N { get; set; }

        object Write { get; set; }

        object this[int id] { get; set; }
        object this[string cell] { get; set; }

        int IndexOf(object value);

        bool Contains(object value);
        bool HasCell(string CellName);
        bool HasPylon(string PylonName);

        void Replicate(int id);

        object Find(object value);

        int NoidOrdinal { get; }
        int QuidOrdinal { get; }

        bool IsEditable(int index);

    }


    public static class ShahCells
    {

        public static Noid GetNoid(this IDataCells obj)
        {
            return ((Noid)obj[obj.iTier.NoidOrdinal]);
        }
        public static Byte[] GetShah(this IDataCells obj, Int32 cell)
        {
            if (cell >= 0)
                return obj[cell].GetShah();
            else
                return ((Noid)obj[obj.iTier.NoidOrdinal]).GetBytes();
        }
    }

}