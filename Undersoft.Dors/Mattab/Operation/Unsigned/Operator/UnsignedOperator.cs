using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;


namespace System.Dors.Mathtab
{
    [Serializable]
    public class UnsignedOperator : Formula
    {
        protected Formula e;


        public UnsignedOperator(Formula ee)
        {
            e = ee;
        }       

        public override void Compile(ILGenerator g, CompilerContext cc)
        {
            e.Compile(g, cc);
        }

        public override double Eval(int i, int j)
        {
            return e.Eval(j, i);
        }

        public override MattabSize Size
        {
            get { return e.Size; }
        }
    }
}
