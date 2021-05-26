using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System.Doors;

namespace System.Doors.Data
{
    public class DataCellsArrayComparer : IEqualityComparer<IDataCells[]>
    {
        private int[] ids;
        public DataCellsArrayComparer(int[] _ids)
        {
            ids = _ids;
        }
        public bool Equals(IDataCells[] x, IDataCells[] y)
        {
            bool r = ids.Select((o, i) => x[i][o]).ToArray().GetShahCode64() == ids.Select((o, i) => y[i][o]).ToArray().GetShahCode64();
            return r;
        }

        public int GetHashCode(IDataCells[] obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                return ids.Select((o, i) => obj[i][o]).ToArray().GetShahCode32(); ;
            }
        }
    }

    public class DataCellsComparer : IEqualityComparer<IDataCells>
    {
        private int ids;
        public DataCellsComparer(int _ids)
        {
            ids = _ids;
        }
        public bool Equals(IDataCells x, IDataCells y)
        {
            return (x != null && y != null && x[ids].Equals(y[ids]));
        }

        public int GetHashCode(IDataCells obj)
        {
            if (obj == null)
                return 0;
            unchecked
            {
                return obj[ids].GetHashCode(); 
            }
        }
    }    
}
