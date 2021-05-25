using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Dors.Data;

namespace System.Dors.Mathtab
{      
    public interface IMattab
    {
        IMattab     DataObject { get; set; }
        Mattab      FormulaObject { get; set; }
        SubMattab   SubFormulaObject
        { get; set; }
        Formula     MattabFormula { get; set; }

        DataPylons MattabPylons { get; set; }
        DataPylon  MattabPylon { get; }

        MattabData Data { get; set; }

        double this[long index, long field] { get; set; }

        DataPylon AssignMattabPylon(int ordinal);
        DataPylon AssignMattabPylon(string name);

        DataPylon RemoveMattabPylon(string name);

        //void SyncMattabData();

        //void RemovePylonMode(int index);

        //int SetPylonMode(string name);

        //int GetMattabPylonId(string name);

        //int GetMattabDataId(string name);

        //Type DataType { get; }

        //string PylonName { get; }

        //KeyValuePair<int, int>  LeftFormulaPylon { get; set; }
        //IDictionary<int, int>   MattabPylons { get; set; }
        //IList<SubMattab>      SubFormulaData64 { get; set; }
        //IList<CompilerContext>  SubContextData { get; set; }
    }
}
