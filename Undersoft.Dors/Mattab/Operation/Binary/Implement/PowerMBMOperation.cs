using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace System.Dors.Mathtab
{
    [Serializable]
    public class PowerMBMOperation : BinaryFormula
    {
        public PowerMBMOperation(Formula e1, Formula e2) : base(e1, e2)
        {

        }

        public override void Compile(ILGenerator g, CompilerContext cc)
        {
            if (cc.IsFirstPass())
            {
                expr1.Compile(g, cc);
                expr2.Compile(g, cc);
                if (!(expr2.Size == MattabSize.Scalar))
                    throw new SizeMismatchException("Pow Operator requires a scalar second operand");
                return;
            }
            expr1.Compile(g, cc);
            expr2.Compile(g, cc);
            g.EmitCall(OpCodes.Call, typeof(Math).GetMethod("Pow"), null);
        }

        public override double Eval(int i, int j)
        {
            return Math.Pow(expr1.Eval(i, j), expr2.Eval(i, j));
        }

        public override MattabSize Size
        {
            get { return expr1.Size; }
        }
    }
}
