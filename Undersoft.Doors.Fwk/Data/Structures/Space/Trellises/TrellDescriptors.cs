using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Doors;
using System.Doors.Drive;

namespace System.Doors.Data
{
    public class TrellsDescriptor : PropertyDescriptor
    {
        public TrellsDescriptor(string name, string display) : base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name),
                                                                                            new DataIdxAttribute()})
        {
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataSphere); } }
        public override Type PropertyType { get { return typeof(DataTrellises); } }
        public override bool IsReadOnly { get { return false; } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
            ((DataSphere)component).Trells = (DataTrellises)value;
        }
        public override object GetValue(object component)
        {
            if (component != null)
            {
                DataTrellises child = null;
                DataSphere parent = ((DataSphere)component);
                if (Name == "Data Trellises")
                    child = ((DataSphere)component).Trells;
                else if (Name == "Root Trellises")
                    child = ((DataSphere)component).RootTrells;
                else if (Name == "Expand Trellises")
                    child = ((DataSphere)component).ExpandTrells;

                if (child != null)
                {
                    DataSphere sphereOn = child.SphereOn;
                    ((DataIdxAttribute)Attributes[typeof(DataIdxAttribute)]).DataIdx = child.SphereOn.Config.DataIdx;
                    PropertyDescriptorCollection pdc = child.CreateDescriptors(Name, child.SphereOn.Config.DataIdx);                                  
                    if (sphereOn != null)
                    {
                        DataSpheres spheres = sphereOn.SpheresOn;
                        if (spheres != null)
                        {
                            DataSpheres tempspheres = spheres;
                            while (spheres != null)
                            {
                                pdc = spheres.CreateDescriptors(Name, child.SphereOn.Config.DataIdx, pdc);
                                tempspheres = spheres;
                                spheres = spheres.SpheresUp;
                            }
                            spheres = tempspheres;
                            if (spheres.AreaOn != null)
                                pdc = spheres.AreaOn.CreateDescriptors(Name, child.SphereOn.Config.DataIdx, pdc);
                        }
                    }

                    if (((IDataGridBinder)component).BoundedGrid != null)
                        ((IDataGridBinder)child).BoundedGrid = ((IDataGridBinder)component).BoundedGrid;
                }
                return child;
            }
            return null;
        }
    }
}