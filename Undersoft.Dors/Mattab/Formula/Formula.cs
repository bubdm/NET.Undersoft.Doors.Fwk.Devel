using System.Reflection.Emit;

namespace System.Dors.Mathtab
{
    [Serializable]
    public abstract class Formula
    {
        public abstract double Eval(int i, int j);     

        public abstract void Compile(ILGenerator g, CompilerContext cc);

        public virtual MattabSize Size
        {
            get
            {
                return new MattabSize(0, 0);
            }
        }

        // addition
        public static Formula operator +(Formula e1, Formula e2)
        {
            return new BinaryMBMOperation(e1, e2, new Plus());
        }

        // subtraction
        public static Formula operator -(Formula e1, Formula e2)
        {
            return new BinaryMBMOperation(e1, e2, new Minus());
        }

        // multiplication
        public static Formula operator *(Formula e1, Formula e2)
        {
            return new BinaryMBMOperation(e1, e2, new Multiply());
        }

        // division
        public static Formula operator /(Formula e1, Formula e2)
        {
            return new BinaryMBMOperation(e1, e2, new Divide());            
        }

        // equal
        public static Formula operator ==(Formula e1, Formula e2)
        {
            return new CompareMBMOperation(e1, e2, new Equal());
        }

        // not equal
        public static Formula operator !=(Formula e1, Formula e2)
        {
            return new CompareMBMOperation(e1, e2, new NotEqual());
        }     

        // lesser
        public static Formula operator <(Formula e1, Formula e2)
        {
            return new CompareMBMOperation(e1, e2, new Less());
        }

        // or
        public static Formula operator |(Formula e1, Formula e2)
        {
            return new CompareMBMOperation(e1, e2, new OrOperand());
        }

        // greater
        public static Formula operator >(Formula e1, Formula e2)
        {
            return new CompareMBMOperation(e1, e2, new Greater());
        }

        // and
        public static Formula operator &(Formula e1, Formula e2)
        {
            return new CompareMBMOperation(e1, e2, new AndOperand());
        }

        // not equal literal
        public static bool operator !=(Formula e1, object o)
        {
            if (o == null)
                return NullCheck.IsNotNull(e1);
            else
                return !e1.Equals(o);
        }

        // equal literal
        public static bool operator ==(Formula e1, object o)
        {
            if (o == null)
                return NullCheck.IsNull(e1);
            else
                return e1.Equals(o);
        }

        // power e2 is always a literal
        public Formula Pow(Formula e2)
        {
            return new PowerMBMOperation(this, e2);
        }

        public static Formula MemPow(Formula e1, Formula e2)
        {
            return new PowerMBMOperation(e1, e2);
        }

        public PreparedFormula Prepare(Formula f, LeftFormula m, bool partial = false)
        {
            PreparedFormula = new PreparedFormula(m, f, partial);
            PreparedFormula.LeftFormula = m;
            PreparedFormula.RightFormula = f;
            return PreparedFormula;
        }
        public PreparedFormula Prepare(LeftFormula m, bool partial = false)
        {
            PreparedFormula = new PreparedFormula(m, this, partial);
            PreparedFormula.LeftFormula = m;
            PreparedFormula.RightFormula = this;
            return PreparedFormula;
        }

        [NonSerialized] public Formula LeftFormula;
        [NonSerialized] public Formula RightFormula;
        [NonSerialized] public PreparedFormula PreparedFormula;

        public void Execute(PreparedEvaluator ev)
        {            
            Evaluator e = new Evaluator(ev.Eval);
            e();
        }

        public Evaluator Evaluate(PreparedFormula m)
        {
            PreparedEvaluator evaluator = GetEvaluator(m);
            Evaluator ev = new Evaluator(evaluator.Eval);
            return ev;
        }
        public Evaluator Evaluate(PreparedEvaluator e)
        {
            Evaluator ev = new Evaluator(e.Eval);
            return ev;
        }
        public Evaluator Evaluate(Formula f, LeftFormula m)
        {
            PreparedEvaluator evaluator = GetEvaluator(f, m);
            Evaluator ev = new Evaluator(evaluator.Eval);
            return ev;
        }
        //public Evaluator Evaluate(Formula f, out Mattab mtx)
        //{
        //    MattabSize s = Size;
        //    mtx = new Mattab(s.rows, s.cols);
        //    PreparedEvaluator evaluator = Compiler.Compile(new PreparedFormula(mtx, f));
        //    Evaluator ev = new Evaluator(evaluator.Eval);
        //    return ev;
        //}

        public PreparedEvaluator GetEvaluator(PreparedFormula m)
        {
            return Compiler.Compile(m);
        }
        public PreparedEvaluator GetEvaluator(Formula f, LeftFormula m)
        {
            PreparedEvaluator evaluator = Compiler.Compile(new PreparedFormula(m, f));
            return evaluator;
        }     

        public Formula Transpose()
        {
            return new TransposeOperation(this);
        }

        // basic mathrix multiplication
        //public Formula Multiplication(Formula e2)
        //{
        //    return new MattabMultiplyOperation(this, e2);
        //}

        // from double
        public static implicit operator Formula(double f)
        {
            return new UnsignedFormula(f);
        }

        public static Formula Cos(Formula e)
        {
            return new FunctionOperation(e, FunctionOperation.FunctionType.Cos);
        }
        public static Formula Sin(Formula e)
        {
            return new FunctionOperation(e, FunctionOperation.FunctionType.Sin);
        }
        public static Formula Log(Formula e)
        {
            return new FunctionOperation(e, FunctionOperation.FunctionType.Log);
        }

        public override int GetHashCode()
        {
            return this.GetHashCode();
        }

        public override bool Equals(object o)
        {
            if (o == null)
                return false;
            return this.Equals(o);
        }
    }

    public static class NullCheck
    {
        public static bool IsNotNull(object o)
        {
            if (o is ValueType)
                return false;
            else
                return !ReferenceEquals(o, null);
        }
        public static bool IsNull(object o)
        {
            if (o is ValueType)
                return false;
            else
                return ReferenceEquals(o, null);
        }
    }

    public class SizeMismatchException : Exception
    {
        public SizeMismatchException(string s) : base(s)
        {

        }
    }
}
