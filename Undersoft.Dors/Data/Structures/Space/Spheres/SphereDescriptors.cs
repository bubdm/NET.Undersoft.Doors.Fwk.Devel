using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Collections.Concurrent;
using System.Collections;
using System.ComponentModel;
using System.Dors;
using System.Dors.Drive;

namespace System.Dors.Data
{
    public class SpheresDescriptor : PropertyDescriptor
    {
        public SpheresDescriptor(string name, string display) : base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name),
                                                                                            new DataIdxAttribute()})
        {
        }
        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataSphere); } }
        public override bool IsReadOnly { get { return true; } }
        public override Type PropertyType { get { return typeof(DataSpheres); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
        }
        public override object GetValue(object component)
        {
            DataSpheres child = ((DataSphere)component).SpheresIn;
            if (child == null)
                child = ((DataSphere)component).SpheresOn;
            if (child != null)
            {
                DataSphere sphereOn = child.SphereUp;
                ((DataIdxAttribute)Attributes[typeof(DataIdxAttribute)]).DataIdx = child.Config.DataIdx;
                PropertyDescriptorCollection pdc = child.CreateDescriptors(Name, child.Config.DataIdx);
                DataSpheres spheres = child;

                if (sphereOn != null)
                    pdc = sphereOn.Trells.CreateDescriptors(Name, child.Config.DataIdx, pdc);

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

                if (((IDataGridBinder)component).BoundedGrid != null)
                    ((IDataGridBinder)child).BoundedGrid = ((IDataGridBinder)component).BoundedGrid;
            }
            return child;
        }
    }

    public class SphereRelaysDescriptor : PropertyDescriptor
    {
        public SphereRelaysDescriptor(string name, string display) : base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name),
                                                                                                  new DataIdxAttribute()})
        {
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataSphere); } }
        public override bool IsReadOnly { get { return false; } }
        public override Type PropertyType { get { return typeof(DataRelays); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
            ((DataSphere)component).Relays = (DataRelays)value;
        }
        public override object GetValue(object component)
        {
            PropertyDescriptorCollection pdc = null;
            DataTrellis trells = new DataTrellis("ParentNames");
            trells.Pylons.AddRange(new DataPylon[] { new DataPylon(typeof(int),    "Id",          "Id")          { Ordinal = 0 },
                                                     new DataPylon(typeof(string), "TrellName",   "TrellName")   { PylonSize = 50, Ordinal = 1 },
                                                     new DataPylon(typeof(string), "DisplayName", "DisplayName") { PylonSize = 50, Ordinal = 2 } });
            trells.PrimeKey = trells.Pylons[0].AsArray();
            int i = 0;
            foreach (DataTrellis trell in ((DataSphere)component).Trells)
            {
                DataTier tier = new DataTier(trells);
                tier.PrimeArray = new object[] { i, trell.TrellName, trell.DisplayName };
                trells.Tiers.Add(tier);
                i++;
            }
            DataSphere parent = ((DataSphere)component);
            if (parent != null)
            {
                DataRelays child = parent.Relays;
                if (child != null)
                {
                    DataSphere sphereOn = child.SphereOn;
                    pdc = child.CreateDescriptors(Name, child.Config.DataIdx);
                    DataSpheres spheres = sphereOn.SpheresOn;
                    if (sphereOn != null)
                    {
                        pdc = sphereOn.Trells.CreateDescriptors(Name, child.Config.DataIdx, pdc);
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
                    {
                        ((IDataGridBinder)child).BoundedGrid = ((IDataGridBinder)component).BoundedGrid;
                        IDataGridStyle gridStyle = (((IDataGridBinder)child).BoundedGrid).GridStyle;
                        gridStyle.GetGridStyle("Relations");
                        foreach (PropertyDescriptor pd in pdc)
                        {
                            if (pd.Name.Equals("ParentName"))
                                gridStyle.CustomComboField(trells.Tiers, "ParentName", "ParentName", "TrellName", "DisplayName");
                            else if (pd.Name.Equals("ChildName"))
                                gridStyle.CustomComboField(trells.Tiers, "ChildName", "ChildName", "TrellName", "DisplayName");
                        }
                        gridStyle.SetGridStyle(((IDataGridBinder)child).BoundedGrid.BindingSource, "Relations");
                    }
                }
                return child;
            }
            return null;
        }
    }
}
