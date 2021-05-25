using System.ComponentModel;
using System.Dors;

namespace System.Dors.Data
{
    public class SortDescriptor : PropertyDescriptor
    {
        public SortDescriptor(string name, string display) : base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name),
                                                                                          new DataIdxAttribute()})
        {
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataTrellis); } }
        public override bool IsReadOnly { get { return true; } }
        public override Type PropertyType { get { return typeof(SortTerms); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
            ((DataTrellis)component).Sort = (SortTerms)value;
        }
        public override object GetValue(object component)
        {
            PropertyDescriptorCollection pdc = null;
            DataTrellis parent = ((DataTrellis)component);
            SortTerms child = parent.Sort;
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
                    gridStyle.GetGridStyle("Sorting");
                    foreach (PropertyDescriptor pd in pdc)
                    {
                        if (pd.Name.Equals("PylonName"))
                            gridStyle.PylonComboField(parent.Pylons, "PylonName", "PylonName");
                        else if (pd.PropertyType == typeof(SortDirection))
                            gridStyle.EnumComboField(typeof(SortDirection), pd.Name, pd.DisplayName);
                    }
                    gridStyle.SetGridStyle(((IDataGridBinder)child).BoundedGrid.BindingSource, "Sorting");
                }
            }
            return child;
        }
    }
}

