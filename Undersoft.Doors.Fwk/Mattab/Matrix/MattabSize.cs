﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Doors.Mathtab
{
    [Serializable]
    public class MattabSize
    {
        public MattabSize(int i, int j)
        {
            rows = i;
            cols = j;
        }

        public int rows;
        public int cols;

        public static MattabSize Scalar = new MattabSize(1, 1);

        public override bool Equals(object o)
        {
            if (o is MattabSize) return ((MattabSize)o) == this;
            return false;
        }

        public static bool operator !=(MattabSize o1, MattabSize o2)
        {
            return o1.rows != o2.rows || o1.cols != o2.cols;
        }

        public static bool operator ==(MattabSize o1, MattabSize o2)
        {
            return o1.rows == o2.rows && o1.cols == o2.cols;
        }

        public override int GetHashCode()
        {
            return rows * cols;
        }

        public override string ToString()
        {
            return "" + rows + " " + cols;
        }
    }
}
