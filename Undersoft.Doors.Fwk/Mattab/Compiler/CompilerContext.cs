﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.ComponentModel;
using System.Text;
using System.Doors;
using System.Doors.Data;

namespace System.Doors.Mathtab
{
    public delegate void Evaluator();

    [Serializable]
    public class CompilerContext
    {
        [NonSerialized] int pass = 0;
        [NonSerialized] int paramCount;
        [NonSerialized] int indexVariableCount;      
        [NonSerialized] int[] indexVariables;
        [NonSerialized] IDataTiers[] paramTiers = new IDataTiers[10];

        public CompilerContext()
        {
            indexVariableCount = 0;         
        }

        public int Add(IDataTiers v)
        {
            int index = GetIndexOf(v);
            if (index < 0)
            {
                paramTiers[paramCount] = v;
                return indexVariableCount + paramCount++;
            }
            return index;
        }

        public int GetIndexOf(IDataTiers v)
        {
            for (int i = 0; i < paramCount; i++)
                if (paramTiers[i] == v) return indexVariableCount + i;
            return -1;
        }
        public int GetSubIndexOf(IDataTiers v)
        {
            for (int i = 0; i < paramCount; i++)
                if (paramTiers[i] == v) return indexVariableCount + i + paramCount;
            return -1;
        }

        public int GetBufforIndexOf(IDataTiers v)
        {
            for (int i = 0; i < paramCount; i++)
                if (paramTiers[i] == v) return indexVariableCount + i + paramCount + 1;
            return -1;
        }

        public int Count
        {
            get { return paramCount; }
        }

        public IDataTiers[] ParamTiers
        {
            get { return paramTiers; }
        }

        public static void GenLocalLoad(ILGenerator g, int a)
        {
            switch (a)
            {
                case 0: g.Emit(OpCodes.Ldloc_0); break;
                case 1: g.Emit(OpCodes.Ldloc_1); break;
                case 2: g.Emit(OpCodes.Ldloc_2); break;
                case 3: g.Emit(OpCodes.Ldloc_3); break;
                default:
                    g.Emit(OpCodes.Ldloc, a);
                    break;
            }
        }
        public static void GenLocalStore(ILGenerator g, int a)
        {
            switch (a)
            {
                case 0: g.Emit(OpCodes.Stloc_0); break;
                case 1: g.Emit(OpCodes.Stloc_1); break;
                case 2: g.Emit(OpCodes.Stloc_2); break;
                case 3: g.Emit(OpCodes.Stloc_3); break;
                default:
                    g.Emit(OpCodes.Stloc, a);
                    break;
            }
        }

        public bool IsFirstPass()
        {
            return pass == 0;
        }

        public void NextPass()
        {
            pass++;
            // local variables array
            indexVariables = new int[indexVariableCount];
            for (int i = 0; i < indexVariableCount; i++)
                indexVariables[i] = i;
        }

        // index access by variable number		
        public int GetIndexVariable(int number)
        {
            return indexVariables[number];
        }

        public void SetIndexVariable(int number, int value)
        {
            indexVariables[number] = value;
        }

        public int AllocIndexVariable()
        {
            return indexVariableCount++;
        }

        public void GenerateLocalInit(ILGenerator g)
        {
            // declare indexes
            for (int i = 0; i < indexVariableCount; i++)
                g.DeclareLocal(typeof(int));

            // declare parameters
            string paramFieldName = "DataParameters";

            for (int i = 0; i < paramCount; i++)
                g.DeclareLocal(typeof(IDataTiers));

            for (int i = 0; i < paramCount; i++)
                g.DeclareLocal(typeof(IDataCells));

            g.DeclareLocal(typeof(double));

            // load the parameters from parameters array
            for (int i = 0; i < paramCount; i++)
            {
                // simple this.parameterTiers[i]
                g.Emit(OpCodes.Ldarg_0); //this
                g.Emit(OpCodes.Ldfld, typeof(PreparedEvaluator).GetField(paramFieldName));
                g.Emit(OpCodes.Ldc_I4, i);
                g.Emit(OpCodes.Ldelem_Ref);
                g.Emit(OpCodes.Stloc, indexVariableCount + i);
            }
        }
    }
}
