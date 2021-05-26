using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Doors.Mathtab;
using System.Doors;

namespace System.Doors.Data
{  
    [JsonObject]
    [Serializable]
    public class DataPylon : IDataPylon, IMattab, INoid
    {
        #region Private NonSerialized

        [NonSerialized] private Func<DataTier, object> lambdaMethod;
        [NonSerialized] private Expression<Func<DataTier, object>> lambdaFormula;        
        [NonSerialized] private PreparedEvaluator mattabEvaluator;       
        [NonSerialized] private PreparedFormula mattabFormula;
        [NonSerialized] private IMattab dataObject;
        [NonSerialized] private Mattab formulaObject;
        [NonSerialized] public  FieldInfo PylonField;

        private AggregateOperand totalOperand;
        private AggregateOperand joinOperand;

        private DataTiers Tiers
        {
            get
            {
                switch (ComputeSource)
                {
                    case DataModes.Tiers:
                        return Trell.TiersView;
                    case DataModes.Sims:
                        return Trell.SimsView;
                    default:
                        return Trell.Tiers;
                }
            }
        }

        [NonSerialized] private DataPylons pylons;
        [NonSerialized] private DataPylons keys;
        [NonSerialized] private DataRelay[] relays;
        [NonSerialized] private DataTrellis trell;
        [NonSerialized] private DataPylons mattabPylons;
        [NonSerialized] private SubMattab subFormulaObject;

        private DataPylon join;
        private DataPylon total;
        private DataConfig config;

        private int lineOffset = -1;
        private object objDefault;
        [NonSerialized] private Type dataType;
        private string dataTypeName;
        private string pylonName;
        private bool visible = true;
        private bool editable = true;
        private bool isdbnull = false;
        private bool isnoid = false;
        private bool isquid = false;
        private bool isindex = false;
        private bool isidentity = false;
        private bool iskey = false;
        private bool iscube = false;
        private bool isauto = false;
        #endregion

        public DataTrellis Trell
        { get { return trell; } set { trell = value; } }
        public DataPylons Keys
        { get { return keys; } set { keys = value; } }
        public DataPylons Pylons
        { get { return pylons; } set { pylons = value; } }
        public DataConfig Config
        { get { return config; } set { config = value; } }        

        public int PylonId
        { get { return (Trell != null) ? Trell.Pylons.IndexOf(this) : -1; } set { } }

        public DataPylon()
        {
            PylonName = "";
            DisplayName = "";
            Visible = true;
            Ordinal = -1;
            MaxLength = -1;
            JoinOrdinal = null;
            AutoIndex = 0;
            AutoStep = 1;
            isDBNull = false;
            isIdentity = false;
            isKey = false;
            isIndex = false;
            isAutoincrement = false;
            JoinOperand = AggregateOperand.None;
            TotalOperand = AggregateOperand.None;
            ArithmeticMode = ArithmeticModes.None;
            Config = new DataConfig(this, DataStore.Space);

        }
        public DataPylon(string name, string display = null)
        {
            PylonName = name;
            DisplayName = (display != null) ? display : name;
            Visible = true;
            Ordinal = -1;
            MaxLength = -1;
            JoinOrdinal = null;
            AutoIndex = 0;
            AutoStep = 1;
            isDBNull = false;
            isIdentity = false;
            isKey = false;
            isIndex = false;
            isAutoincrement = false;
            JoinOperand = AggregateOperand.None;
            TotalOperand = AggregateOperand.None;
            ArithmeticMode = ArithmeticModes.None;
            Config = new DataConfig(this, DataStore.Space);

        }
        public DataPylon(Type type, string name = "", string display = null)
        {
            DataType = type;
            PylonName = name;
            DisplayName = (display != null) ? display : name;
            Visible = true;
            Ordinal = -1;
            AutoIndex = 0;
            AutoStep = 1;
            JoinOrdinal = null;
            isDBNull = false;
            isIdentity = false;
            isKey = false;
            isIndex = false;
            isAutoincrement = false;
            MaxLength = -1;
            JoinOperand = AggregateOperand.None;
            TotalOperand = AggregateOperand.None;
            ArithmeticMode = ArithmeticModes.None;
            Config = new DataConfig(this, DataStore.Space);
        }
        public DataPylon(Type type, DataTrellis trell, string name = "", string display = null)
        {
            DataType = type;
            Trell = trell;
            Pylons = trell.Pylons;
            PylonName = name;
            DisplayName = (display != null) ? display : name;
            Visible = true;
            Ordinal = -1;
            AutoIndex = 0;
            AutoStep = 1;
            JoinOrdinal = null;
            isDBNull = false;
            isIdentity = false;
            isKey = false;
            isIndex = false;
            isAutoincrement = false;
            MaxLength = -1;
            JoinOperand = AggregateOperand.None;
            TotalOperand = AggregateOperand.None;
            ArithmeticMode = ArithmeticModes.None;
            Config = new DataConfig(this, DataStore.Space);
        }
        public DataPylon(DataTrellis trell, string name = "", string display = null)
        {

            Trell = trell;
            if (trell != null)
                Pylons = trell.Pylons;
            else
                Pylons = new DataPylons();
            PylonName = name;
            DisplayName = (display != null) ? display : name;
            Visible = true;
            Ordinal = -1;
            AutoIndex = 0;
            AutoStep = 1;
            JoinOrdinal = null;
            isDBNull = false;
            isIdentity = false;
            isKey = false;
            isIndex = false;
            isAutoincrement = false;
            MaxLength = -1;
            JoinOperand = AggregateOperand.None;
            TotalOperand = AggregateOperand.None;
            ArithmeticMode = ArithmeticModes.None;
            Config = new DataConfig(this);

        }

        public Type DataType
        {
            get
            {
                if (dataType == null)
                    DataType = TypeAssemblies.GetType(dataTypeName);
                return dataType;
            }
            set
            {
                dataTypeName = value.FullName;
                dataType = value;
                Type type = value;
                if (PylonSize < 0)
                {
                    if (type.IsPrimitive)
                        PylonSize = Marshal.SizeOf(type);
                    else if (type.Equals(typeof(String)))
                        PylonSize = 50;
                    else if (type.GetInterfaces().Contains(typeof(ICollection)))
                        PylonSize = 4;
                    else if (type.Equals(typeof(DateTime)))
                        PylonSize = 8;
                }
            }
        }

        public int LineOffset
        {
            get
            {
                return lineOffset;
            }
            set { lineOffset = value; }
        }

        public object Default
        {
            get
            {
                if (isCube && CubeLevel < 0)
                {
                    if (objDefault == null)
                        if (DataType == typeof(string))
                            objDefault = string.Empty;
                        else
                            objDefault = DataType.GetDefaultValue();
                }

                return objDefault;
            }
            set
            {
                objDefault = value;
                if ((value != null) && Trell != null)
                    Trell.Pylons.DefultPylons = Trell.Pylons.AsEnumerable().Where(p => p.Default != null).ToList();
            }
        }
        [NonSerialized]
        public double Revalue = 0;       

        public string PylonName
        {
            get { return pylonName; }
            set
            {
                if (Trell != null && Trell.Pylons.Have(value))
                {
                    DataPylon pylon = Trell.Pylons.GetPylon(value);

                    if (Pylons.isKeys && Keys != null && Keys.Contains(this))
                        Keys[Keys.IndexOf(this)] = pylon;
                    else if (Pylons.Contains(this))
                        Pylons[Pylons.IndexOf(this)] = pylon;
                }
                pylonName = value;
            }
        }
        public string DisplayName
        { get; set; }
        public string MappingName
        { get; set; }

        public int Ordinal
        { get; set; }
        public int PylonSize
        { get; set; } = -1;
        public int MaxLength
        { get; set; }
        public int AutoIndex
        { get; set; } = 0;
        public int AutoStep
        { get; set; } = 1;
        public int[] JoinIndex
        { get; set; }
        public int[] JoinOrdinal
        { get; set; }
        public int[] CubeIndex
        { get; set; }
        public int CubeOrdinal
        { get; set; }
        public int InheritorId
        { get; set; }
        public int CubeLevel
        { get; set; } = -1;
        public int TotalOrdinal
        { get; set; }
        public int ComputeOrdinal
        { get; set; }

        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
                if (Pylons != null && Pylons.VisiblePylons.Count > 0)
                    if (!Pylons.VisiblePylons.Contains(this))
                    {
                        if (value)
                        {
                            Pylons.VisiblePylons.Add(this);
                            Pylons.VisiblePylons = Pylons.VisiblePylons.OrderBy(p => p.Ordinal).ToList();
                        }
                    }
                    else
                    {
                        if (!value)
                        {
                            Pylons.VisiblePylons.Remove(this);
                            Pylons.VisiblePylons = Pylons.VisiblePylons.OrderBy(p => p.Ordinal).ToList();
                        }
                    }
            }
        }

        public bool Editable
        { get { return editable; } set { editable = value; } }

        public bool isDBNull
        { get { return isdbnull; } set { isdbnull = value; } }
        public bool isIdentity
        { get { return isidentity; } set { isidentity = value; if (value) editable = false; } }
        public bool isKey
        { get { return iskey; } set { iskey = value; if (value) editable = false; } }
        public bool isIndex
        { get { return isindex; } set { isindex = value; if (value) editable = false; } }
        public bool isNoid
        { get { return isnoid; } set { isnoid = value; if (value) editable = false; } }
        public bool isQuid
        { get { return isquid; } set { isquid = value; DataType = typeof(Quid); PylonSize = 8; Default = Quid.Empty; if (Pylons != null && Pylons.TypeArray.Length > 0) Pylons.TypeArray[Ordinal] = typeof(Quid); if (value) editable = false; } }
        public bool isCube
        { get { return iscube; } set { iscube = value; } }

        public bool isAutoincrement
        {
            get
            {
                return isauto;
            }
            set
            {
                if (isauto != value)
                    SetAutoIdHandler(value);
                if (value)
                    editable = false;
                else
                    editable = true;
            }
        }

        public DataRelay[] JoinRelays
        { get { return relays; } set { relays = value; } }
        public DataPylon JoinPattern
        { get { return join; } set { join = value; } }
        public DataPylon TotalPattern
        { get { return total; } set { total = value; } }

        public int NativeId;

        public DataModes ComputeSource
        { get; set; } = DataModes.Tiers;

        public ArithmeticModes ArithmeticMode
        {
            get;
            set;
        }
        public AggregateOperand JoinOperand
        {
            get
            {
                return joinOperand;
            }
            set
            {
                joinOperand = value;
                //editable = ((value == AggregateOperand.None ||
                //             value == AggregateOperand.Bind) &&
                //             totalOperand == AggregateOperand.None) ?
                //             true : false;
            }
        }
        public AggregateOperand TotalOperand
        {
            get
            {
                return totalOperand;
            }
            set
            {
                totalOperand = value;
                //editable = (value == AggregateOperand.None &&
                //           (joinOperand == AggregateOperand.None ||
                //            joinOperand == AggregateOperand.Bind)) ?
                //            true : false;
            }
        }

        public RevaluateOperand RevalOperand
        { get; set; }
        public RevaluateType RevalType
        { get; set; }

        public IDorsEvent OnSetEvent;
        public IDorsEvent OnDeleteEvent;
        public IDorsEvent OnInsertEvent;
        public IDorsEvent OnClearEvent;

        public DataPylon[] AsArray()
        {
            return new DataPylon[] { this };
        }

        public object GetDefault()
        {
            if (Default != null)
            {
                return Default;
            }
            else
                return null;
        }
        public object GetAutoIds()
        {
            if (isAutoincrement)
            {
                object result = AutoIndex;
                AutoIndex += AutoStep;
                return result;
            }
            else
                return null;
        }

        public void SetAutoIdHandler(bool value)
        {
            isauto = (DataType != typeof(int) && DataType != typeof(uint)) ? false : value;

            if (isauto)
            {
                if (Trell != null && Trell.IsPrime)
                {
                    if (Trell.Count > 0)
                        AutoIndex = (Trell.Prime.Tiers.AsEnumerable().Max(x => (int)x[Ordinal])) + AutoStep;
                    else
                        AutoIndex = 0;

                    Trell.Pylons.AutoIdPylons = Trell.Pylons.AsEnumerable().Where(p => p.isAutoincrement).ToList();
                    Trell.Pylons.hasAutoId = true;
                }
            }
            else
                Trell.Pylons.hasAutoId = Trell.Pylons.AutoIdPylons.Count == 0 ? false : true;
        }

        public DataPylon Copy()
        {
            DataPylon nval = (DataPylon)this.MemberwiseClone();

            //string[] enable = new string[] {    "PylonName", "DisplayName", "MappingName", "Ordinal", "Visible", "Editable", "Default", "CubeIndex",
            //                                    "CubeOrdinal", "CubeLevel",  "JoinIndex", "JoinOperand", "TotalOperand"  };

            //PropertyInfo[] piarray = typeof(DataPylon).GetProperties().Where(p => enable.Contains(p.Name)).ToArray();
            //foreach (PropertyInfo pi in piarray)
            //{
            //    object pval = pi.GetValue(this);
            //    if (pval != null)
            //    {
            //        if (pval is Enum)
            //        {
            //            if (pval.ToString() != "None")
            //                pi.SetValue(nval, pval);
            //        }
            //        else
            //        {
            //            pi.SetValue(nval, pval);
            //        }
            //    }
            //}
            return nval;
        }

        public DataPylon Clone()
        {
            DataPylon nval = (DataPylon)this.MemberwiseClone();
            return nval;
        }

        #region Serialization
        public byte[] GetShah()
        {
            return (Config.Place != null) ? Config.Place.GetShah() : null;
        }

        public ushort GetDriveId()
        {
            return 0;
        }
        public ushort GetSectorId()
        {
            return 0;
        }
        public ushort GetLineId()
        {
            return 0;
        }

        public string GetMapPath()
        {
            return Config.Path ?? null;
        }
        public string GetMapName()
        {
            return Config.Path ?? null;
        }
        #endregion
      
        #region Mattab Formula

        public Mattab CloneMattab()
        {
            return formulaObject.Clone();
        }
        public Mattab NewMattab(DataModes mode = DataModes.Tiers)
        {
            ComputeSource = mode;
            Pylons.ComputeSource = mode;
            return FormulaObject = new Mattab(this);
        }
        public Mattab GetMattab(DataModes mode = DataModes.Tiers)
        {
            ComputeSource = mode;
            Pylons.ComputeSource = mode;
            if (!ReferenceEquals(FormulaObject, null))
                return FormulaObject;
            else
            {
                MattabPylons = new DataPylons();
                return FormulaObject = new Mattab(this);
            }
        }
       
        public Mattab FormulaObject
        { get { return formulaObject; } set { formulaObject = value; } }

        public double this[long index, long field]
        {
            get
            {
                return Convert.ToDouble(Tiers[(int)index][(int)field]);
            }
            set
            {
                Tiers[(int)index][MattabPylons[(int)field].Ordinal] = Convert.ChangeType(value, Trell.Pylons[MattabPylons[(int)field].Ordinal].DataType);
            }
        }
       
        public IMattab  DataObject
        { get { return dataObject; } set { dataObject = value; } }

        public MattabData Data
        {
            get
            {
                return Pylons.Data;
            }
            set
            {
                Pylons.Data = value;
            }
        }

        public DataPylons MattabPylons
        { get { return mattabPylons; } set { mattabPylons = value; } }
        public DataPylon  MattabPylon
        { get { return this; } }
        public SubMattab  SubFormulaObject
        { get; set; }
        public Formula RightFormula
        { get; set; }

        public bool PartialMattab = false;

        public PreparedEvaluator MattabEvaluator
        { get { return mattabEvaluator; } set { mattabEvaluator = value; } }

        public LeftFormula EvaluateMattab(DataModes mode = DataModes.Tiers)
        {
            ComputeSource = mode;
            if (mattabEvaluator != null)
            {
                Evaluator evaluate = new Evaluator(mattabEvaluator.Eval);
                evaluate();
            }
            return mattabFormula.lexpr;
        }
        
        public Formula MattabFormula
        {
            get
            {
                return (!ReferenceEquals(mattabFormula, null)) ? mattabFormula : null;
            }
            set
            {
                if (!ReferenceEquals(value, null))
                {                    
                    mattabFormula = value.Prepare(FormulaObject[this.PylonName]);

                    ArithmeticMode = ArithmeticModes.Mattab;
                    Trell.MattabHash.Add(Ordinal);
                }
                else if (Trell.MattabHash.Contains(Ordinal))
                {
                    Trell.MattabHash.Remove(Ordinal);
                    ArithmeticMode = ArithmeticModes.None;
                }
            }
        }      

        public PreparedEvaluator CompileMattab()
        {
            if (mattabEvaluator == null)
                mattabEvaluator = mattabFormula.GetEvaluator(mattabFormula);

            return mattabEvaluator;
        }

        public DataPylon AssignMattabPylon(int ordinal)
        {
            DataPylon pyl = Trell.Pylons[ordinal];
            string name = pyl.PylonName;
            if (pyl != null)
                if (!MattabPylons.Have(name))
                {
                    if (!Pylons.MattabPylons.Have(name))
                    {
                        pyl.ArithmeticMode = ArithmeticModes.Mattab;
                        Pylons.MattabPylons.Add(pyl);
                    }

                    if (pyl.Ordinal == Ordinal && !Pylons.LeftMattabPylons.Have(name))
                        Pylons.LeftMattabPylons.Add(pyl);

                    MattabPylons.Add(pyl);
                }
            return pyl;
        }

        public DataPylon AssignMattabPylon(string name)
        {
            if (MattabPylons == null)
                MattabPylons = new DataPylons();

            DataPylon pyl = Trell.Pylons[name];
            if (pyl != null)
                if (!MattabPylons.Have(name))
                {
                    if (!Pylons.MattabPylons.Have(name))
                    {
                        pyl.ArithmeticMode = ArithmeticModes.Mattab;
                        Pylons.MattabPylons.Add(pyl);
                    }

                    if (pyl.Ordinal == Ordinal)
                    {
                        if (Pylons.LeftMattabPylons == null)
                        {
                            Pylons.LeftMattabPylons = new DataPylons();
                            Pylons.LeftMattabPylons.Add(pyl);
                        }
                        else if (!Pylons.LeftMattabPylons.Have(name))
                            Pylons.LeftMattabPylons.Add(pyl);
                    }
                    MattabPylons.Add(pyl);
                }
            return pyl;
        }
        public DataPylon RemoveMattabPylon(string name)
        {
            DataPylon pyl = Trell.Pylons[name];
            if (pyl != null)
            {
                return RemoveMattabPylon(pyl.Ordinal);                               
            }
            return pyl;
        }
        public DataPylon RemoveMattabPylon(int ordinal)
        {
            DataPylon pyl = Trell.Pylons[ordinal];
            if (pyl != null)
            {
                if (MattabPylons.Have(pyl.PylonName))
                {
                    MattabPylons.Remove(MattabPylons[pyl.PylonName]);

                    if (Pylons.MattabPylons.Have(pyl.PylonName))
                    {
                        Pylons.MattabPylons.Remove(Pylons.MattabPylons[pyl.PylonName]);
                    }

                    if (!ReferenceEquals(FormulaObject, null) && Pylons.LeftMattabPylons.Have(pyl.PylonName))
                    {
                        Pylons.LeftMattabPylons.Remove(Pylons.MattabPylons[pyl.PylonName]);
                        FormulaObject = null;
                        mattabFormula = null;
                    }
                    pyl.ArithmeticMode = ArithmeticModes.None;
                }
            }
            return pyl;
        }
      
        #endregion

        #region Lambda Formula
        public Func<DataTier, object> LambdaMethod
        {
            get
            {
                return lambdaMethod;
            }
            set
            {
                lambdaMethod = value;
            }
        }
        public Expression<Func<DataTier, object>> LambdaFormula
        {
            get
            {
                return lambdaFormula;
            }
            set
            {
                if (value != null)
                {
                    lambdaFormula = value;
                    lambdaMethod = value.Compile();
                    if (ArithmeticMode == ArithmeticModes.None)
                        ArithmeticMode = ArithmeticModes.Lambda;
                    Trell.LambdaHash.Add(Ordinal);
                }
            }
        }
        #endregion

    }

    public static class DataPylonClonable
    {
        public static string[] ClonableNames = new string[] {    "DisplayName", "MappingName", "Ordinal", "Visible", "Editable", "Default", "CubeIndex",
                                                                 "CubeOrdinal", "CubeLevel",  "JoinIndex", "ArithmeticMode", "JoinOperand", "TotalOperand" };

        public static PropertyInfo[] ClonableInfo = typeof(DataPylon).GetProperties().Where(p => ClonableNames.Contains(p.Name)).ToArray();
    }

    [Serializable]
    public enum AggregateOperand
    {
        None, 
        Sum,
        Avg,
        Min,
        Max,
        Bis,
        First,
        Last,
        Bind,      
        Count,
        Default
    }
    [Serializable]
    public enum ArithmeticModes
    {
        None,
        Mattab,
        Lambda,
        Both       
    }
    [Serializable]
    public enum ComputeMode
    {
        None,
        Mattab,
        Lambda
    }
    [Serializable]
    public enum RevaluateOperand
    {
        None,
        Add,
        Subtract,
        Multiply,
        Divide
    }
    [Serializable]
    public enum RevaluateType
    {
        None,
        Value,
        Percent,
        Margin
    }

}
