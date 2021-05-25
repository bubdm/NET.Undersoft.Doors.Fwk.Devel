using System;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Dors;

namespace System.Dors.Data
{   
    public class DataCellsDescriptor : PropertyDescriptor
    {
        public DataCellsDescriptor(string name, DataPylon _pylon) : base(name, new Attribute[] { new DataIdxAttribute(_pylon.Trell.DataIdx + _pylon.PylonName) })
        {
            pylon = _pylon;
        }

        private DataPylon pylon;

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataTier); } }
        public override bool IsReadOnly { get { return false; } }
        public override Type PropertyType { get { return pylon.DataType; } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
            if(!pylon.isQuid)
                ((IDataCells)component)[pylon.Ordinal] = value;
            else
                ((IDataCells)component)[pylon.Ordinal] = new Quid(value.ToString());
        }
        public override object GetValue(object component)
        {
            if (!pylon.isQuid)
                return ((IDataCells)component)[pylon.Ordinal];
            else
            {
                object o = ((IDataCells)component)[pylon.Ordinal];
                if (o != null)
                    o.ToString();
                return o;
            }
        }
    }
}