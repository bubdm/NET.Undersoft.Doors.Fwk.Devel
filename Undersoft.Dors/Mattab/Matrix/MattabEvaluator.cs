using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dors.Data;

namespace System.Dors.Mathtab
{
	/// Class for the Generated Code
	public abstract class PreparedEvaluator
    {
        public int ParametersCount = 0;

        public IDataTiers[] DataParameters = new IDataTiers[1];

        public abstract void Eval();

        public void SetParams(IDataTiers[] p, int paramCount)
        {
            DataParameters = p;
            ParametersCount = paramCount;
        }
        public bool SetParams(IDataTiers p, int index)
        {
            if (index < ParametersCount)
            {
                if (ReferenceEquals(DataParameters[index], p))
                    return false;
                else
                    DataParameters[index] = p;
            }
            return false;
        }
        public void SetParams(IDataTiers p)
        {
            Put(p);
        }

        public int Put(IDataTiers v)
        {
            int index = GetIndexOf(v);
            if (index < 0)
            {
                DataParameters[ParametersCount] = v;
                return 1 + ParametersCount++;
            }
            else
            {
                DataParameters[index] = v;
            }
            return index;
        }

        public int GetIndexOf(IDataTiers v)
        {
            for (int i = 0; i < ParametersCount; i++)
                if (DataParameters[i] == v) return 1 + i;
            return -1;
        }
    
        public int GetRowCount(int paramid)
        {
            return DataParameters[paramid].Count;
        }

        public int GetColumnCount(int paramid)
        {
            return DataParameters[paramid].Trell.Pylons.Count;
        }

    }
}
