using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;


namespace System.Dors.Mathtab
{
    // Binary Member By Member Oprator Formula
    [Serializable]
    public class CompareMBMOperation : BinaryFormula
    {
        BinaryOperator oper;
        //MattabSize size;

        public CompareMBMOperation(Formula e1, Formula e2, BinaryOperator op) : base(e1, e2)
        {
            oper = op;
            //MattabSize o1 = expr1.Size;
            //MattabSize o2 = expr2.Size;

            //if (o1 != o2 && (o1 != MattabSize.Scalar && o2 != MattabSize.Scalar))
            //    throw new SizeMismatchException("Binary Memberwise Mismatch");

            //size = o1 == MattabSize.Scalar ? o2 : o1;
        }

        // Compilation First Pass: check sizes
        // Code Generation: the operator code
        public override void Compile(ILGenerator g, CompilerContext cc)
        {
            expr1.Compile(g, cc);
            expr2.Compile(g, cc);
            if (cc.IsFirstPass())
                return;
            oper.Compile(g);
        }

        public override double Eval(int i, int j)
        {
            return oper.Apply(expr1.Eval(i, j), expr2.Eval(i, j));
        }

        public override MattabSize Size
        {
            get { return expr1.Size == MattabSize.Scalar ? expr2.Size : expr1.Size; }
        }
    }
}
