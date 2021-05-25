using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Dors.Drive;
using System.IO;
using System.Dors;

namespace System.Dors.Data
{
    public class TiersDescriptor : PropertyDescriptor
    {
        public TiersDescriptor(string name, string display) : base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name),
                                                                                           new DataIdxAttribute()})
        {
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return typeof(DataTrellis); } }
        public override bool IsReadOnly { get { return false; } }
        public override Type PropertyType { get { return typeof(DataTiers); } }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
            if (Name == "Data Tiers")
                ((DataTrellis)component).Tiers.TiersView = (DataTiers)value;
            else if (Name == "Data Simulate")
                ((DataTrellis)component).Sims.TiersView = (DataTiers)value;
            else if (Name == "Tiers Totals")
                ((DataTrellis)component).Tiers.TiersTotal = (DataTiers)value;
            else if (Name == "Simulate Totals")
                ((DataTrellis)component).Sims.TiersTotal = (DataTiers)value;
        }
        public override object GetValue(object component)
        {
            DataTiers child = null;
            if (Name == "Data Tiers")
            {
                ((DataTrellis)component).Mode = DataModes.Tiers;
                child = ((DataTrellis)component).TiersView;
                ((DataIdxAttribute)Attributes[typeof(DataIdxAttribute)])
                    .DataIdx = child.Config.DataIdx;
            }
            else if (Name == "Data Simulate")
            {
                ((DataTrellis)component).Mode = DataModes.Sims;
                child = ((DataTrellis)component).SimsView;
                ((DataIdxAttribute)Attributes[typeof(DataIdxAttribute)])
                   .DataIdx = child.Config.DataIdx;
            }
            else if (Name == "Tiers Totals")
            {
                ((DataTrellis)component).Mode = DataModes.Tiers;
                child = ((DataTrellis)component).TiersTotal;
                ((DataIdxAttribute)Attributes[typeof(DataIdxAttribute)])
                   .DataIdx = child.Config.DataIdx;
            }
            else if (Name == "Simulate Totals")
            {
                ((DataTrellis)component).Mode = DataModes.Sims;
                child = ((DataTrellis)component).SimsTotal;
                ((DataIdxAttribute)Attributes[typeof(DataIdxAttribute)])
                   .DataIdx = child.Config.DataIdx;
            }
            if (child != null)
            {
                DataTrellis trell = ((DataTrellis)component);
                DataSphere sphereOn = trell.Sphere;
                PropertyDescriptorCollection pdc = child.CreateDescriptors(Name, child.Config.DataIdx);

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
            }
                        
            return child;
        }
    }

    public class TierJoinDescriptor : PropertyDescriptor
    {
        private int index;
        private RelaySite relayMemberType;

        public TierJoinDescriptor(string name, int _index, string display = null, RelaySite _relayMemberType = RelaySite.None) : 
            base(name, new Attribute[] { new DisplayNameAttribute((display != null) ? display : name),
                                         new DataIdxAttribute()})
        {
            index = _index;
            relayMemberType = _relayMemberType;
        }

        public override Type ComponentType { get { return typeof(DataTier); } }
        public override Type PropertyType { get { return typeof(DataTiers); } }
        public override bool IsReadOnly { get { return false; } }
        public override bool CanResetValue(object component) { return true; }
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) { return false; }
        public override void SetValue(object component, object value)
        {
            DataTiers[] relayedTiersArray = null;
            if (relayMemberType == RelaySite.All)
                relayedTiersArray = new DataTiers[] { ((DataTier)component).Tiers };
            else if (relayMemberType == RelaySite.Parent)
                relayedTiersArray = ((DataTier)component).ParentTiers;
            else if (relayMemberType == RelaySite.Child)
                relayedTiersArray = ((DataTier)component).ChildTiers;

            relayedTiersArray[index] = (DataTiers)value;
        }
        public override object GetValue(object component)
        {
            DataTiers[] relayedTiersArray = null;
            DataTier parent = (DataTier)component;
            if (relayMemberType == RelaySite.All)
                if (parent.Tiers.Mode == DataModes.Sims || parent.Tiers.Mode == DataModes.SimsView)
                    relayedTiersArray = new DataTiers[] { parent.Trell.SimsView };
                else
                    relayedTiersArray = new DataTiers[] { parent.Trell.TiersView };
            else if (relayMemberType == RelaySite.Parent)
                relayedTiersArray = parent.ParentTiers;
            else if (relayMemberType == RelaySite.Child)
                relayedTiersArray = parent.ChildTiers;

            if (relayedTiersArray != null)
            {
                DataTiers child = null;
                if (relayedTiersArray.Length > index)
                {
                    child = relayedTiersArray[index];
                    if (child != null)
                    {
                        ((DataIdxAttribute)Attributes[typeof(DataIdxAttribute)]).DataIdx = child.Config.DataIdx;

                        DataSphere[] sphereArray = new DataSphere[] { parent.Trell.Sphere, child.Trell.Sphere };
                        foreach (DataSphere sphereOn in sphereArray)
                        {

                            PropertyDescriptorCollection pdc = child.CreateDescriptors(Name, child.Config.DataIdx);

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
                        }
                    }
                    else
                        child = new DataTiers();
                }
                return child;
            }
            return relayedTiersArray;
        }
    }
}