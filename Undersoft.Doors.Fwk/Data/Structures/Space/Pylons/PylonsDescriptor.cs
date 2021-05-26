using System.ComponentModel;
using System.Doors;

namespace System.Doors.Data
{
    public class DataPylonsDescriptor : PropertyDescriptor
    {
        public DataPylonsDescriptor(string name, string display) : base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name),
                                                                                                new DataIdxAttribute()})
        {
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataTrellis); } }
        public override bool IsReadOnly { get { return true; } }
        public override Type PropertyType { get { return typeof(DataPylons); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
            ((DataTrellis)component).Pylons = (DataPylons)value;
        }
        public override object GetValue(object component)
        {
            PropertyDescriptorCollection pdc = null;
            DataTrellis parent = ((DataTrellis)component);
            DataPylons child = parent.Pylons;
            if (child != null)
            {
                DataSphere sphereOn = parent.Sphere;
                ((DataIdxAttribute)Attributes[typeof(DataIdxAttribute)]).DataIdx = child.Config.DataIdx;
                pdc = child.CreateDescriptors(Name, child.Config.DataIdx);

                if (sphereOn != null)
                {
                    pdc = sphereOn.Trells.CreateDescriptors(Name, child.Config.DataIdx, pdc);

                    DataSpheres spheres = sphereOn.SpheresOn;
                    if (spheres != null)
                    {
                        DataSpheres tempspheres = spheres;
                        while (spheres != null)
                        {
                            pdc = spheres.CreateDescriptors(Name, child.Config.DataIdx, pdc);
                            tempspheres = spheres;
                            spheres = spheres.SpheresUp;
                        }
                        spheres = tempspheres;
                        if (spheres.AreaOn != null)
                            pdc = spheres.AreaOn.CreateDescriptors(Name, child.Config.DataIdx, pdc);
                    }
                }

                if (((IDataGridBinder)component).BoundedGrid != null)
                    ((IDataGridBinder)child).BoundedGrid = ((IDataGridBinder)component).BoundedGrid;

                if (((IDataGridBinder)child).BoundedGrid != null)
                {
                    IDataGridStyle gridStyle = (((IDataGridBinder)child).BoundedGrid).GridStyle;
                    gridStyle.GetGridStyle("Field Pylons");
                    foreach (PropertyDescriptor pd in pdc)
                    {
                        if (pd != null)
                        {
                            if (pd.Name.Equals("DataType"))
                                gridStyle.DataTypeComboField("DataType", "DataType");
                            else if (pd.PropertyType == typeof(ArithmeticModes))
                                gridStyle.EnumComboField(typeof(ArithmeticModes), pd.Name, pd.DisplayName);
                            else if (pd.PropertyType == typeof(AggregateOperand))
                                gridStyle.EnumComboField(typeof(AggregateOperand), pd.Name, pd.DisplayName);
                            else if (pd.PropertyType == typeof(bool))
                                gridStyle.CustomBoolField(pd.Name, pd.Name);
                            else
                                gridStyle.CustomTextField(pd.Name, pd.Name);
                        }
                    }
                    gridStyle.SetGridStyle(((IDataGridBinder)child).BoundedGrid.BindingSource, "Field Pylons");
                }
            }
            return child;
        }
    }
}
