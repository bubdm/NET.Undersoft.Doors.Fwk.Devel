using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;


namespace System.Quantic.Mathtab
{
    class MattabMultiplyOperation64 : BinaryFormula
    {
        int myIndex;

        public MattabMultiplyOperation64(Formula e1, Formula e2) : base(e1, e2)
        {

        }

        // Compilation First Pass: check sizes //
        // Code Generation the operator code //
        public override void Compile(ILGenerator g, CompilerContext cc)
        {
            if (cc.IsFirstPass())
            {
                expr1.Compile(g, cc);
                expr2.Compile(g, cc);
                if (expr1.Size.cols != expr2.Size.rows)
                    throw new SizeMismatchException("Mattab Multiplication Error");
                // allocate a new temporaneous variable //
                myIndex = cc.AllocIndexVariable();
                return;
            }

            Label topLabel = g.DefineLabel();
            int i1 = cc.GetIndexVariable(0);
            int i2 = cc.GetIndexVariable(1);
            int me = cc.GetIndexVariable(myIndex);

            // loop with the variable k directly on stack //
            //       
			//	double r = 0;	// the value is on the stack!
			//	k = 0;
			//	do { r += e1[i,k]*e2[k,j]; k++; } while (k < e1.cols)			

            g.Emit(OpCodes.Ldc_I4_0);   // k = 0
            g.Emit(OpCodes.Stloc, me);

            g.Emit(OpCodes.Ldc_R8, 0);
            g.MarkLabel(topLabel);

            cc.SetIndexVariable(1, me);
            expr1.Compile(g, cc);
            cc.SetIndexVariable(0, me);
            cc.SetIndexVariable(1, i2);
            expr2.Compile(g, cc);
            cc.SetIndexVariable(0, i1);
            cc.SetIndexVariable(1, i2);
            g.Emit(OpCodes.Mul);
            g.Emit(OpCodes.Add);

            // increment k
            g.Emit(OpCodes.Ldloc, me);          // k =>
            g.Emit(OpCodes.Ldc_I4_1);           // 1
            g.Emit(OpCodes.Add);                // +
            g.Emit(OpCodes.Dup);
            g.Emit(OpCodes.Stloc, me);          // k <=

            // here from first jump
            g.Emit(OpCodes.Ldc_I4, expr1.Size.cols);    // k < cols
            g.Emit(OpCodes.Blt, topLabel);

            // on the stack we have a value ...
        }

        public override double Eval(int i, int j)
        {
            int q = expr1.Size.cols;
            double r = 0;
            for (int k = 0; k < q; q++)
                r += expr1.Eval(i, k) * expr2.Eval(k, j);
            return r;
        }

        public override MattabSize Size
        {
            get { return new MattabSize(expr1.Size.rows, expr2.Size.cols); }
        }
    }
}
