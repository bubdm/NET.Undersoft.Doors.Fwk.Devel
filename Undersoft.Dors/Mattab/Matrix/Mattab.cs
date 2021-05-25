using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Dors.Data;

namespace System.Dors.Mathtab
{         
    [Serializable]
    public class Mattab : LeftFormula, IMattab
    {
        [NonSerialized] private CompilerContext context;

        public IMattab      DataObject
        { get; set; }
        public Mattab       FormulaObject
        { get; set; }
        public SubMattab    SubFormulaObject
        { get; set; }
        public Formula      MattabFormula
        { get { return DataObject.MattabFormula; } set { DataObject.MattabFormula = value; } }
        public Formula      PartialFormula;
        public MattabTiers  TiersRef;
        public MattabData   Data
        {
            get { return DataObject.Data; }
            set { DataObject.Data = value; }
        }

        public IDataTiers   iTiers
        {
            get { return Data.iTiers; }
            set { Data.iTiers = value; }
        }

        public DataPylons MattabPylons
        { get { return DataObject.MattabPylons; } set { DataObject.MattabPylons = value; } }
        public DataPylon  MattabPylon
        { get; set; }

        public string   pylonName
        { get { return MattabPylon.PylonName; } }
        public Type     pylonType
        { get { return MattabPylon.DataType; } }
        public int      pylonId
        { get { return MattabPylon.Ordinal; } }

        public int      rowCount
        { get { return iTiers.Count; } }
        public int      colCount
        { get { return MattabPylons.Count; } }

        public int startId = 0;

        //public KeyValuePair<int, int> LeftFormulaPylon
        //{ get; set; }
        //public IList<SubMattab>       SubFormulaData64
        //{ get; set; } = new List<SubMattab>();
        //public IList<CompilerContext> SubContextData
        //{ get; set; } = new List<CompilerContext>();
        //public IDictionary<int, int>  MattabPylons
        //{ get { return DataObject.MattabPylons; } set { DataObject.MattabPylons = value; } }

        public void SyncMattabData()
        {
            //foreach (SubMattab sm in MattabPylons.Select(mp => mp.SubFormulaObject).ToArray())
            //    SetDimensions(sm, this);

            //if (context != null)
            //for (int i = 0; i < context.Count; i++)
            //    if (!ReferenceEquals(context.ParamTiers[i], iTiers))
            //        context.ParamTiers[i] = iTiers;
        }

        //public int   GetMattabPylonId(string name)
        //{
        //    return DataObject.GetMattabPylonId(name);
        //}
        //public int   GetMattabDataId(string name)
        //{
        //    return DataObject.GetMattabPylonId(name);
        //}
        //public Type  GetPylonType(string name)
        //{
        //    return DataObject.GetPylonType(name);
        //}
        public DataPylon AssignMattabPylon(string name)
        {
            return DataObject.AssignMattabPylon(name);
        }
        public DataPylon AssignMattabPylon(int ordinal)
        {
            return DataObject.AssignMattabPylon(ordinal);
        }
        public DataPylon RemoveMattabPylon(string name)
        {
            return DataObject.RemoveMattabPylon(name);
        }

        public void AssignCompilerContext(CompilerContext Context)
        {
            if (context == null || !ReferenceEquals(context, Context))
                context = Context;
        }

        public Mattab Clone()
        {
            Mattab mx = (Mattab)this.MemberwiseClone();
            return mx;
        }

        public Mattab(IMattab dataObject)
        {
            MattabPylon = dataObject.MattabPylon;
            DataObject = dataObject;
            dataObject.DataObject = this;
            FormulaObject = this;
        }

        public double       this[long index]
        {
            get { return Convert.ToDouble(iTiers[index / iTiers.Trell.Pylons.Count][(int)index % iTiers.Trell.Pylons.Count]); }
            set { iTiers[index / iTiers.Trell.Pylons.Count][(int)index % iTiers.Trell.Pylons.Count] = value; }
        }
        public double       this[long index, long field]
        {
            get { return Convert.ToDouble(iTiers[index][(int)field]); }
            set { iTiers[index][(int)field] = value; }
        }
        public SubMattab    this[string name]
        {
            get
            {
                if (TiersRef == null)
                    TiersRef = new MattabTiers(this);
                return TiersRef[name];
            }
        }
        public SubMattab    this[int r, string name]
        {
            get
            {
                if (TiersRef == null)
                    TiersRef = new MattabTiers(this, r, r);
                return TiersRef[name];
            }
        }
        public MattabTiers  this[int r]
        {
            get
            {
                return new MattabTiers(this, r, r);
            }
        }
        public MattabTiers  this[IndexRange q]
        {
            get { return new MattabTiers(this, q.first, q.last); }
        }      

        public static IndexRange Range(int i1, int i2)
        {
            return new IndexRange(i1, i2);
        }
       
        public override void Assign(int i, bool v)
        {
            this[Convert.ToInt64(i)] = Convert.ToDouble(v);
        }                                                  /// left expr '=" assignment
        public override void Assign(int i, double v)
        {
            this[Convert.ToInt64(i)] = v;
        }
        public override void Assign(int i, int j, bool v)
        {
            this[i, j] = Convert.ToDouble(v);
        }                                                  /// left expr '=" assignment
        public override void Assign(int i, int j, double v)
        {
            this[i, j] = v;
        }      

        public override void CompileAssign(ILGenerator g, CompilerContext cc, bool post, bool partial)
        {
            if (cc.IsFirstPass())
            {
                cc.Add(iTiers);
                PartialFormula = MattabFormula.RightFormula.Prepare(this[pylonName], false);
                PartialFormula.Compile(g, cc);
                MattabPylon.PartialMattab = true;
            }
            else
            {
                PartialFormula.Compile(g, cc);
            }

        }                    /// left expr '=" compilation 
        public override void Compile(ILGenerator g, CompilerContext cc)
        {
            if (cc.IsFirstPass())
            {
                cc.Add(iTiers);
                PartialFormula = MattabFormula.RightFormula.Prepare(this[pylonName], true);
                PartialFormula.Compile(g, cc);
                MattabPylon.PartialMattab = true;
            }
            else
            {
                PartialFormula.Compile(g, cc);
            }
        }               /// compilation first time, add a reference to the array for index access

        public override double Eval(int i, int j)
        {
            return this[i, j];
        }                                                            /// formula evaluation

        public override MattabSize Size
        {
            get { return new MattabSize(iTiers.Count, MattabPylons.Count); }
        }                                                           /// formula size determination			

        public void SetDimensions(SubMattab sm, Mattab mx = null, int offset = 0)
        {
            sm.startId = offset;
            sm.SetDimensions(Data, mx);
        }

        public SubMattab GetAll(DataPylon pylon)
        {
            SubMattab smx = new SubMattab(Data, pylon, this);
            return smx;
        }
        public SubMattab GetRange(int startRowId, int endRowId, DataPylon pylon)
        {
            SubMattab smx = new SubMattab(Data, pylon, this);
            return smx;
        }
        public SubMattab GetColumn(int j)
        {
            return GetRange(0, j, null);
        }
        public SubMattab GetColumnCount(int j1, int j2)
        {
            return GetRange(0, 1, null);
        }
        public SubMattab GetRow(int i)
        {
            return GetRange(i, 1, null);
        }
        public SubMattab GetRowCount(int i1, int i2)
        {
            return GetRange(i1, i2, null);
        }
        public SubMattab GetElements(int e1, int e2)
        {
            return new SubMattab(Data, null, FormulaObject);
        }                                                       /// get elements numbered considering the matrix as rows
      
        [Serializable]
        public class MattabTiers
        {
            internal MattabTiers(Mattab m)
            {                
                formulaObject = m;
                firstRow = 0;
                diffRow = (m.rowCount - firstRow) - 1;
            }
            internal MattabTiers(Mattab m, int startRowId, int endRowId)
            {
                firstRow = startRowId;
                diffRow = (endRowId - startRowId);
                formulaObject = m;
            }

            //public SubMattab this[AllPlaceHolder x]
            //{
            //    get { return mtx.GetRowCount(firstRow, lastRow); }
            //}                                                             /// gets all the rows in the firstRow-lastRow range

            public SubMattab this[int ordinal]
            {
                get
                {
                    DataPylon pyl = formulaObject.DataObject.AssignMattabPylon(ordinal);                                                         //IndexOfValue(name);
                    return formulaObject.GetRange(firstRow, lastRow, pyl);
                }
            }                                                                        /// gets a sub range
            public SubMattab this[string name]
            {
                get
                {
                    try
                    {
                        DataPylon pyl = formulaObject.DataObject.AssignMattabPylon(name);                                                         //IndexOfValue(name);
                        return formulaObject.GetAll(pyl);
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }                                                              

            public static explicit operator LeftFormula(MattabTiers r)
            {
                return r.formulaObject.GetElements(r.firstRow, r.lastRow);
            }                                       /// like in MATLAB (x:y) are just the elements in range

            private Mattab formulaObject;

            public int firstRow;
            public int diffRow = -1;
            public int lastRow
            {
                get { return (formulaObject.rowCount > (firstRow + diffRow + 1) && diffRow > -1) ? firstRow + diffRow : formulaObject.rowCount - 1; }
            }
        }      

        [Serializable]
        public struct IndexRange
        {
            internal IndexRange(int i1, int i2)
            {
                first = i1;
                last = i2;
            }
            internal int first, last;
        }                                                                                 /// m[Mattab.Range(5,2)]
    }


}
