﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace System.Doors.Mathtab
{
    [Serializable]
    public class Less : BinaryOperator
    {
        public override double Apply(double a, double b)
        {
            return Convert.ToDouble(a < b);
        }
        public override void Compile(ILGenerator g)
        {
            g.Emit(OpCodes.Clt);
            g.Emit(OpCodes.Conv_R8);
        }
    }
}
