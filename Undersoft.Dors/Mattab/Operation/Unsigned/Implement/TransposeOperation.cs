using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;


namespace System.Dors.Mathtab
{
    [Serializable]
    public class TransposeOperation : UnsignedOperator
    {
        public TransposeOperation(Formula e) : base(e)
        {

        }
        
        public override void Compile(ILGenerator g, CompilerContext cc)
        {
            if (cc.IsFirstPass())
            {
                e.Compile(g, cc);
                return;
            }

            // swap the indexes at the compiler level
            int i1 = cc.GetIndexVariable(0);
            int i2 = cc.GetIndexVariable(1);
            cc.SetIndexVariable(1, i1);
            cc.SetIndexVariable(0, i2);
            e.Compile(g, cc);
            cc.SetIndexVariable(0, i1);
            cc.SetIndexVariable(1, i2);
        }

        public override double Eval(int i, int j)
        {
            return e.Eval(j, i);
        }

        public override MattabSize Size
        {
            get
            {
                MattabSize o = e.Size;
                return new MattabSize(o.cols, o.rows);
            }
        }

    }
}
