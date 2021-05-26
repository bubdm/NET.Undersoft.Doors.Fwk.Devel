using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Doors.Data;

namespace System.Doors.Mathtab
{      
    public class MattabData
    {
        public int RowCount;
        public int RowOffset;

        public MattabData()
        {            

        }
        public MattabData(IDataTiers itiers)
        {
            iTiers = itiers;
        }
        public MattabData(IDataTiers itiers, int rowOffset, int rowCount)
        {
            RowCount = rowCount;
            RowOffset = rowOffset;
            iTiers = itiers;
        }       

        public double this[int rowid, int cellid]
        {
            get
            {
                return Convert.ToDouble(iTiers[rowid][cellid]);
            }
            set
            {
                iTiers[rowid][cellid] = value;
            }
        }

        public IDataTiers iTiers;

        public bool Enabled = false;
        public int Count
        { get { return iTiers.Count; } }
    }
}
