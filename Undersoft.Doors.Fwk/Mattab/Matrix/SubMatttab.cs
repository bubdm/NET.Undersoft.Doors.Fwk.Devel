using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Doors.Data;
using System.Doors;

namespace System.Doors.Mathtab
{
    [Serializable]
    public class SubMattab : LeftFormula
    {
        public IDataTiers iTiers { get { return FormulaObject.Data.iTiers; } }

        public DataPylon MattabPylon;
        public Mattab    FormulaObject;
        
        public string pylonName { get { return MattabPylon.PylonName; } }
        public Type pylonType { get { return MattabPylon.DataType; } }
        public int pylonId { get { return MattabPylon.Ordinal; } }

        public int rowCount { get { return iTiers.Count; } }
        public int colCount { get { return FormulaObject.MattabPylons.Count; } }

        public int startId = 0;     
              
        public SubMattab(MattabData matrixData, DataPylon mattabPylon, Mattab mx)
        {
            if (mattabPylon != null) MattabPylon = mattabPylon;
          
            SetDimensions(matrixData, mx);
        }      

        public void SetDimensions(MattabData matrixData, Mattab formulaObject = null)
        {
            if (!ReferenceEquals(formulaObject, null)) FormulaObject = formulaObject;
            MattabPylon.SubFormulaObject = this;            
        }

        public double this[long index]
        {
            get { return Convert.ToDouble(iTiers[index][pylonId]); }
            set { iTiers[index][pylonId] = value; }
        }
        public double this[long index, int PylonId]
        {
            get { return Convert.ToDouble(iTiers[index][PylonId]); }
            set { iTiers[index][PylonId] = value; }
        }

        public override void Assign(int i, double v)
        {
            this[i] = v;
        }
        public override void Assign(int i, bool v)
        {
            this[i] = Convert.ToDouble(v);
        }
        public override void Assign(int i, int j, double v)
        {
            this[i, j] = v;
        }
        public override void Assign(int i, int j, bool v)
        {
            this[i, j] = Convert.ToDouble(v);
        }

        public override void CompileAssign(ILGenerator g, CompilerContext cc, bool post, bool partial)
        {
            if (cc.IsFirstPass())
            {
                cc.Add(iTiers);
                //FormulaObject.AssignCompilerContext(cc);
                return;
            }

            int i1 = cc.GetIndexVariable(0);

            if (!post)
            {
                if (!partial)
                {
                    CompilerContext.GenLocalLoad(g, cc.GetIndexOf(iTiers));  

                    if (startId != 0)
                        g.Emit(OpCodes.Ldc_I4, startId);

                    g.Emit(OpCodes.Ldloc, i1);

                    if (startId != 0)
                        g.Emit(OpCodes.Add);

                    g.EmitCall(OpCodes.Callvirt, typeof(IDataTiers).GetMethod("get_Item", new Type[] { typeof(int) }), null);
                    CompilerContext.GenLocalStore(g, cc.GetSubIndexOf(iTiers));
                    CompilerContext.GenLocalLoad(g, cc.GetSubIndexOf(iTiers));
                }
                else
                {
                    CompilerContext.GenLocalLoad(g, cc.GetSubIndexOf(iTiers));     
                }

                if (!iTiers.Trell.IsPrime)
                {
                    g.Emit(OpCodes.Ldc_I4, pylonId);
                }
                else
                {
                    g.EmitCall(OpCodes.Callvirt, typeof(IDataCells).GetMethod("get_iN", new Type[] { }), null);
                    g.Emit(OpCodes.Ldc_I4, pylonId);
                }
            }
            else
            {
                if (partial)
                {
                    g.Emit(OpCodes.Dup);
                    CompilerContext.GenLocalStore(g, cc.GetBufforIndexOf(iTiers));           // index
                }

                if (!iTiers.Trell.IsPrime)
                {
                    g.Emit(OpCodes.Box, typeof(double));
                    g.EmitCall(OpCodes.Callvirt, typeof(IDataCells).GetMethod("set_Item", new Type[] { typeof(int), typeof(object) }), null);
                }
                else
                {
                    g.Emit(OpCodes.Box, typeof(double));
                    g.EmitCall(OpCodes.Callvirt, typeof(IDataNative).GetMethod("set_Item", new Type[] { typeof(int), typeof(object) }), null);
                }

                if (partial)
                    CompilerContext.GenLocalLoad(g, cc.GetBufforIndexOf(iTiers));           // index
            }            
        }

        // Compilation First Pass: add a reference to the array variable
        // Code Generation: access the element through the i index
        public override void Compile(ILGenerator g, CompilerContext cc)
        {
            if (cc.IsFirstPass())
            {
                cc.Add(iTiers);
            }
            else
            {
                CompilerContext.GenLocalLoad(g, cc.GetSubIndexOf(iTiers));           // index

                if (!iTiers.Trell.IsPrime)
                {
                    g.Emit(OpCodes.Ldc_I4, pylonId);
                    g.EmitCall(OpCodes.Callvirt, typeof(IDataCells).GetMethod("get_Item", new Type[] { typeof(int) }), null);
                    g.Emit(OpCodes.Unbox_Any, pylonType);
                }
                else
                {
                    g.EmitCall(OpCodes.Callvirt, typeof(IDataCells).GetMethod("get_iN", new Type[] { }), null);                   
                    g.Emit(OpCodes.Ldc_I4, pylonId);
                    g.EmitCall(OpCodes.Callvirt, typeof(IDataNative).GetMethod("get_Item", new Type[] { typeof(int) }), null);
                    g.Emit(OpCodes.Unbox_Any, pylonType);
                }
                g.Emit(OpCodes.Conv_R8);
            }
        }

        public override MattabSize Size
        {
            get { return new MattabSize(rowCount, colCount); }
        }

        public override double Eval(int i, int j)
        {
            return this[i, j];
        }
    }
}
