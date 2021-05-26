using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Doors;

namespace System.Doors.Data
{    
    public class DataTrellRelaysDescriptor : PropertyDescriptor
    {
        public DataTrellRelaysDescriptor(string name, string display) : base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name),
                                                                                            new DataIdxAttribute()})
        {
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataTrellis); } }
        public override bool IsReadOnly { get { return false; } }
        public override Type PropertyType { get { return typeof(DataRelays); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
            ((DataTrellis)component).Relays = (DataRelays)value;
        }
        public override object GetValue(object component)
        {
            PropertyDescriptorCollection pdc = null;
            DataTrellis trells = new DataTrellis("ParentNames");
            trells.Pylons.AddRange(new DataPylon[] { new DataPylon(typeof(int), "Id", "Id") { Ordinal = 0 },
                                                  new DataPylon(typeof(string), "TrellName", "TrellName") { PylonSize = 50, Ordinal = 1 },
                                                  new DataPylon(typeof(string), "DisplayName", "DisplayName") { PylonSize = 50, Ordinal = 2 } });
            trells.PrimeKey = trells.Pylons[0].AsArray();
            int i = 0;
            foreach (DataRelay trell in ((DataTrellis)component).Relays)
            {
                DataTier tier = new DataTier(trells);
                tier.PrimeArray = new object[] { i, trell.Parent.Trell.TrellName, trell.Parent.Trell.DisplayName };
                trells.Tiers.Add(tier);
                DataTier tier2 = new DataTier(trells);
                tier2.PrimeArray = new object[] { i, trell.Child.Trell.TrellName, trell.Child.Trell.DisplayName };
                trells.Tiers.Add(tier2);
                i++;
            }
            DataTrellis parent = ((DataTrellis)component);
            DataRelays child = ((DataTrellis)component).Relays;

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

                if (((IDataGridBinder)child).BoundedGrid == null && ((IDataGridBinder)component).BoundedGrid != null)
                    ((IDataGridBinder)child).BoundedGrid = ((IDataGridBinder)component).BoundedGrid;
            }
            if (((IDataGridBinder)child).BoundedGrid != null)
            {
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

            return child;
        }
    }

    public class DataRelayPylonsDescriptor : PropertyDescriptor
    {
        public DataRelayPylonsDescriptor(string name, string display, DataRelaySite _site) : base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name),
                                                                                            new DataIdxAttribute()})
        {
            site = _site;
        }
        public DataRelaySite site;
        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataRelay); } }
        public override bool IsReadOnly { get { return true; } }
        public override Type PropertyType { get { return typeof(DataPylons); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
            if (site == DataRelaySite.Parent)
                ((DataRelay)component).ParentPylons = (DataPylons)value;
            else
                ((DataRelay)component).ChildPylons = (DataPylons)value;
        }
        public override object GetValue(object component)
        {
            PropertyDescriptorCollection pdc = null;
            DataPylons child = null;
            DataTrellis trellis = null;
            if (site == DataRelaySite.Parent)
            {
                child = ((DataRelay)component).Parent.RelayKeys;
                trellis = ((DataRelay)component).Parent.Trell;
            }
            else
            {
                child = ((DataRelay)component).Child.RelayKeys;
                trellis = ((DataRelay)component).Child.Trell;
            }

            if (child != null)
            {
                DataSphere sphereOn = trellis.Sphere;
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
                if (((IDataGridBinder)trellis).BoundedGrid != null)
                    ((IDataGridBinder)child).BoundedGrid = ((IDataGridBinder)trellis).BoundedGrid;

                if (((IDataGridBinder)child).BoundedGrid != null)
                {
                    IDataGridStyle gridStyle = (((IDataGridBinder)child).BoundedGrid).GridStyle;

                    if (site == DataRelaySite.Parent)
                        gridStyle.GetGridStyle("ParentKeys");
                    else
                        gridStyle.GetGridStyle("ChildKeys");

                    foreach (PropertyDescriptor pd in pdc)
                    {
                        if (pd.Name.Equals("PylonName"))
                            gridStyle.PylonComboField(trellis.Pylons, "PylonName", "PylonName");
                        else if (pd.Name.Equals("DataType"))
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
                    if (site == DataRelaySite.Parent)
                        gridStyle.SetGridStyle(((IDataGridBinder)child).BoundedGrid.BindingSource, "ParentKeys");
                    else
                        gridStyle.SetGridStyle(((IDataGridBinder)child).BoundedGrid.BindingSource, "ChildKeys");
                }
            }
            return child;
        }
    }
}
