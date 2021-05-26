using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Doors;

namespace System.Doors.Data
{
    public class DataCreator
    {
        public DataCreator()
        {
        }

        public DataTrellis CreateTrellis(string trellName, DataGroup Group = DataGroup.None)
        {
            DataTrellis ct = new DataTrellis(trellName);                 
            ct.TrellName = trellName;                    
            ct.Group = Group;
            return ct;
        }
        public DataTrellis CreateTrellis(string trellName, List<DataPylon> pylons, DataPylon[] PKeys, DataSphere nSet, DataGroup Group = DataGroup.None)
        {
            DataTrellis ct = new DataTrellis(trellName);
            ct.Sphere = nSet;
            ct.Pylons.AddRange(pylons);
            ct.TrellName = trellName;
            DataPylon[] pKeys = pylons.Where(c => PKeys.Where(p => p.PylonName == c.PylonName).Any()).ToArray();
            foreach (DataPylon p in pKeys) p.isKey = true;
            ct.PrimeKey = PKeys;
            ct.Relays = nSet.Relays;
            ct.Group = Group;

            return ct;
        }
        public DataTrellis CreateTrellis(DataEvent DataDelegateMethod)
        {
            if (DataDelegateMethod.InvokeInfo.Parameters != null)
            {
                DataTrellis ct = (DataTrellis)DataDelegateMethod.Execute();
                return ct;
            }
            else
                return null;
        }
        public DataTrellis CreateTrellis(DorsInvokeInfo DorsInvokeInfo)
        {
            if (DorsInvokeInfo.Parameters != null)
            {
                DataTrellis ct  = CreateTrellis(new DataEvent(DorsInvokeInfo));
                return ct;
            }
            else
                return null;
        }            

        public DataSphere EmulateDataSphere(DataSphere fromDataSphere, string DataSphereName = null)
        {
            DataSphere pSet = fromDataSphere;
            string DataSphereId = (DataSphereName != null) ? DataSphereName : pSet.SphereId;
            DataSphere nSet = new DataSphere(DataSphereId);
            nSet.Config = pSet.Config;
            nSet.Parameters.Registry.AddRange(new Dictionary<string, object>(pSet.Parameters.Registry));

            foreach (DataTrellis fromTrell in pSet.Trells)
            {
                DataTrellis emuTrell = fromTrell;
                DataTrellis _trell = new DataTrellis(emuTrell.TrellName);
                List<DataPylon> emuPylons = emuTrell.Pylons.AsEnumerable().ToList();
                List<DataPylon> columns = new List<DataPylon>(emuTrell.Pylons.AsEnumerable().ToList());
                DataPylon[] pKeys = columns.Where(c => emuTrell.PrimeKey.Where(p => p.PylonName == c.PylonName).Any()).ToArray();
                _trell.IsPrime = false;
                _trell.Prime = emuTrell;
                _trell.TrellName = (_trell.TrellName != null) ? _trell.TrellName : emuTrell.TrellName;
                _trell.PrimeKey = pKeys;
                _trell.Group = emuTrell.Group;
                _trell.Tiers = new DataTiers(_trell);
                _trell.Tiers.AddRange(emuTrell.Tiers.AsEnumerable().Select(ti => new DataTier(_trell, ti)).ToArray());
                nSet.Trells.Add(_trell);
                ImitateRelays(_trell, emuTrell);
            }
            return nSet;
        }

        public void BuildRelays(string relationName, DataTrellis parentTrell, DataTrellis childTrell, List<string> parentKeys, List<string> childKeys)
        {
            if (parentTrell.Relays != null || parentTrell.Sphere != null)
            {
                if (parentTrell.Relays == null)
                    parentTrell.Relays = parentTrell.Sphere.Relays;
                DataRelayMember parent = RelayMember(parentTrell, parentKeys);
                DataRelayMember child = RelayMember(childTrell, childKeys);
                DataSphere iSet = null;
                if (parentTrell.Sphere != null && childTrell.Sphere != null)
                    iSet = parentTrell.Sphere;
                DataRelay rd = CreateRelay(relationName, parent, child, iSet);
            }
        }
        public void BuildRelays(string relationName, DataTrellis parentTrell, DataTrellis childTrell, DataPylon[] parentKeys, DataPylon[] childKeys)
        {
            if (parentTrell.Relays != null || parentTrell.Sphere != null)
            {
                if (parentTrell.Relays == null)
                    parentTrell.Relays = parentTrell.Sphere.Relays;
                DataRelayMember parent = RelayMember(parentTrell, parentKeys);
                DataRelayMember child = RelayMember(childTrell, childKeys);
                DataSphere iSet = null;
                if (parentTrell.Sphere != null && childTrell.Sphere != null)
                    iSet = parentTrell.Sphere;
                DataRelay rd = CreateRelay(relationName, parent, child, iSet);                
            }
        }
        public void BuildRelays(string relationName, DataSphere iSet, DataTrellis parentTrell, DataTrellis childTrell, List<string> parentKeys, List<string> childKeys)
        {
            if (iSet.Trells[parentTrell.TrellName] == null)
            {
                iSet.Trells.Add(parentTrell);
                // parentTrell.Relays = iSet.Relays;
            }
            if (iSet.Trells[childTrell.TrellName] == null)
            {
                iSet.Trells.Add(childTrell);
                // childTrell.Relays = iSet.Relays;
            }

            DataRelayMember parent = RelayMember(parentTrell, parentKeys);
            DataRelayMember child = RelayMember(childTrell, childKeys);
            DataRelay rd = CreateRelay(relationName, parent, child, iSet);
            //iSet.Relays.Add(rd);
            //parentTrell.Relays.Add(rd);
            //childTrell.Relays.Add(rd);
        }
        public void BuildRelays(string relationName, DataSphere iSet, DataTrellis parentTrell, DataTrellis childTrell, DataPylon[] parentKeys, DataPylon[] childKeys)
        {
            if (iSet.Trells[parentTrell.TrellName] == null)
            {
                iSet.Trells.Add(parentTrell);
              //  parentTrell.Relays = iSet.Relays;
            }
            if (iSet.Trells[childTrell.TrellName] == null)
            {
                iSet.Trells.Add(childTrell);
              //  childTrell.Relays = iSet.Relays;
            }

            DataRelayMember parent = RelayMember(parentTrell, parentKeys);
            DataRelayMember child = RelayMember(childTrell, childKeys);
            DataRelay rd = CreateRelay(relationName, parent, child, iSet);
            //iSet.Relays.Add(rd);
            //parentTrell.Relays.Add(rd);
            //childTrell.Relays.Add(rd);
        }

        private DataRelay CreateRelay(string relayName, DataRelayMember parent, DataRelayMember child, DataSphere iSet)
        {
            DataRelay rd = new DataRelay() { Child = child, Parent = parent, RelayName = relayName };
            if (iSet != null)
                iSet.Relays.Add(rd);
            parent.Trell.Relays.Add(rd);
            if (parent.Trell.IsPrime || parent.Trell.IsDevisor)
                parent.Trell.Tiers.Registry.Keymap.RebuildMap(rd.RelayName);            
            parent.Trell.AssignJoinPylons();

            child.Trell.Relays.Add(rd);
            if (child.Trell.IsPrime || child.Trell.IsDevisor)
                child.Trell.Tiers.Registry.Keymap.RebuildMap(rd.RelayName);
            child.Trell.AssignJoinPylons();
            return rd;
        }

        private DataRelayMember RelayMember(DataTrellis trell, List<string> colNames)
        {
            DataRelayMember r = new DataRelayMember() { Trell = trell, TrellName = trell.TrellName, PylonKeys = trell.Pylons.AsEnumerable().Where(c => colNames.Contains(c.PylonName)).ToArray() };
            return r;
        }
        private DataRelayMember RelayMember(DataTrellis trell, DataPylon[] columns)
        {
            DataRelayMember r = new DataRelayMember() { Trell = trell, TrellName = trell.TrellName, PylonKeys = trell.Pylons.AsEnumerable().Where(c => columns.Where(cu => cu.PylonName == c.PylonName).Any()).ToArray() };
            return r;
        }

        public void ImitateRelays(DataTrellis toTrell, DataTrellis fromTrell)
        {
            if (toTrell.Sphere != null)
            {
                DataRelays iru = fromTrell.Relays;
                foreach (DataRelay irdu in iru)
                {
                    string iSetUpParentName = irdu.Parent.Trell.TrellName,
                           iSetUpChildName = irdu.Child.Trell.TrellName;
                    DataTrellis iSetParentTrell = toTrell.Sphere.Trells.Collect(iSetUpParentName);
                    DataTrellis iSetChildTrell = toTrell.Sphere.Trells.Collect(iSetUpChildName);
                    if (iSetParentTrell != null && iSetChildTrell != null)
                    {
                        DataPylon[] parentKeys = iSetParentTrell.Pylons.AsEnumerable().Where(c => irdu.Parent.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        DataPylon[] childKeys = iSetChildTrell.Pylons.AsEnumerable().Where(c => irdu.Child.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        if (parentKeys != null && childKeys != null)
                        {
                            BuildRelays(irdu.RelayName, iSetParentTrell, iSetChildTrell, parentKeys, childKeys);
                        }
                    }

                }
            }

        }
        public void ImitateRelays(DataSphere toDataSphere, DataSphere fromDataSphere)
        {
            if (fromDataSphere != null)
            {
                DataRelays iru = fromDataSphere.Relays;
                foreach (DataRelay irdu in iru)
                {
                    string iSetUpParentName = irdu.Parent.Trell.TrellName,
                           iSetUpChildName = irdu.Child.Trell.TrellName;
                    DataTrellis iSetParentTrell = toDataSphere.Trells.Collect(iSetUpParentName);
                    DataTrellis iSetChildTrell = toDataSphere.Trells.Collect(iSetUpChildName);
                    if (iSetParentTrell != null && iSetChildTrell != null)
                    {
                        DataPylon[] parentKeys = iSetParentTrell.Pylons.AsEnumerable().Where(c => irdu.Parent.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        DataPylon[] childKeys = iSetChildTrell.Pylons.AsEnumerable().Where(c => irdu.Child.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        if (parentKeys != null && childKeys != null)
                        {
                            BuildRelays(irdu.RelayName, iSetParentTrell, iSetChildTrell, parentKeys, childKeys);
                        }
                    }

                }
            }

        }
        public void ImitateRelays(DataSphere iSet)
        {
            if(iSet.SphereUp != null)
            {
                DataRelays iru = iSet.SphereUp.Relays;
                foreach (DataRelay irdu in iru)
                {
                    string iSetUpParentName = irdu.Parent.Trell.TrellName,
                           iSetUpChildName = irdu.Child.Trell.TrellName;
                    DataTrellis iSetParentTrell = iSet.Trells.Collect(iSetUpParentName);
                    DataTrellis iSetChildTrell = iSet.Trells.Collect(iSetUpChildName);
                    if (iSetParentTrell != null && iSetChildTrell != null)
                    {
                        DataPylon[] parentKeys = iSetParentTrell.Pylons.AsEnumerable().Where(c => irdu.Parent.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        DataPylon[] childKeys = iSetChildTrell.Pylons.AsEnumerable().Where(c => irdu.Child.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        if (parentKeys != null && childKeys != null)
                        {
                            BuildRelays(irdu.RelayName, iSetParentTrell, iSetChildTrell, parentKeys, childKeys);
                        }
                    }

                }
            }

        }
    }
}
