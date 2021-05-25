using System;
using System.Dors;
using System.ComponentModel;

namespace System.Dors.Data
{
    [Serializable]
    public class FilterDescriptor : PropertyDescriptor
    {
        public FilterDescriptor(string name, string display) : base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name),
                                                                                            new DataIdxAttribute()})
        {
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataTrellis); } }
        public override bool IsReadOnly { get { return true; } }
        public override Type PropertyType { get { return typeof(FilterTerms); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
            ((DataTrellis)component).Filter = (FilterTerms)value;
        }
        public override object GetValue(object component)
        {
            PropertyDescriptorCollection pdc = null;
            DataTrellis parent = ((DataTrellis)component);
            FilterTerms child = parent.Filter;
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
                    gridStyle.GetGridStyle("Filtering");
                    foreach (PropertyDescriptor pd in pdc)
                    {
                        if (pd.Name.Equals("PylonName"))
                            gridStyle.PylonComboField(parent.Pylons, "PylonName", "PylonName");
                        else if (pd.PropertyType == typeof(OperandType))
                            gridStyle.EnumComboField(typeof(OperandType), pd.Name, pd.DisplayName);
                        else if (pd.PropertyType == typeof(LogicType))
                            gridStyle.EnumComboField(typeof(LogicType), pd.Name, pd.DisplayName);
                        else if (pd.PropertyType == typeof(FilterStage))
                            gridStyle.EnumComboField(typeof(FilterStage), pd.Name, pd.DisplayName);
                        else if (pd.PropertyType == typeof(bool))
                            gridStyle.CustomBoolField(pd.Name, pd.Name);
                        else
                            gridStyle.CustomTextField(pd.Name, pd.Name);
                    }
                    gridStyle.SetGridStyle(((IDataGridBinder)child).BoundedGrid.BindingSource, "Filtering");
                }
            }
            return child;
        }
    }
}
