using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Dors;

namespace System.Dors.Data
{
   
    public static class DataMorph
    {       
        public static DataTrellises Impact(this DataTrellises nTrells, DataTrellises pTrells, bool toDrive = false, string SphereName = null)
        {
            if (pTrells.SphereOn != null)
            {
                DataSphere nSphere = pTrells.SphereOn.Locate();
                if (nSphere != null)
                    nSphere.Impact(pTrells.SphereOn);
            }
            else
            {
                if (nTrells.Config.DepotId.IsEmpty)
                {
                    string SphereId = (SphereName != null) ? SphereName : pTrells.SphereId;
                    nTrells.SphereId = SphereId;
                    nTrells.Config = pTrells.Config;
                }

                nTrells.State.Impact(pTrells.State, false);

                foreach (DataTrellis emuTrell in pTrells)
                {
                    DataTrellis nTrell = null;

                    if (!nTrells.Have(emuTrell.TrellName))
                        nTrell = emuTrell.Locate();
                    else
                        nTrell = nTrells[emuTrell.TrellName];

                    if (nTrell == null)
                    {
                        if (!emuTrell.IsPrime)
                            nTrell = new DataTrellis(emuTrell.TrellName, false);
                        else
                        {
                            if (emuTrell.Deposit != null)
                            {
                                DataDeposit nDeposit = ((DataDeposit)emuTrell.Deposit.Locator());
                                if (nDeposit != null)
                                    if (nDeposit.Trell != null)
                                        nTrell = nDeposit.Trell;
                            }
                        }
                        if (nTrell != null)
                            nTrells.Add(nTrell);
                    }
                    if (nTrell != null)
                    {
                        nTrell.Impactor(emuTrell);
                        nTrell.ImitateRelays(emuTrell);
                    }
                }
            }

            pTrells = null;
            return nTrells;
        }
        public static DataTrellises Locate(this DataTrellises fromTrells, string _path = null)
        {
            DataTrellises pTrells = fromTrells;
            DataTrellises nTrells = null;
            bool isFirst = true;
            if (pTrells.SphereOn != null)
            {
                DataSphere nSphere = pTrells.SphereOn.Locate();
                if (nSphere != null)
                    nTrells = nSphere.Trells;
            }
            if (nTrells == null)
            {
                nTrells = new DataTrellises();

                foreach (DataTrellis emuTrell in pTrells)
                {
                    DataTrellis nTrell = null;
                    nTrell = emuTrell.Locate();
                    if (nTrell == null)
                        if (!emuTrell.IsPrime)
                            nTrell = new DataTrellis(emuTrell.TrellName, false);
                        else
                        {
                            DataDeposit nDeposit = ((DataDeposit)emuTrell.Deposit.Locator());
                            if (nDeposit != null)
                                if (nDeposit.Trell != null)
                                    nTrell = nDeposit.Trell;
                        }
                    if (nTrell != null)
                    {
                        nTrells.Add(nTrell);
                        if (isFirst)
                        {
                            if (fromTrells.Config.DepotId.IsEmpty)
                            {
                                nTrells.Config.DepotId = nTrell.Config.DepotId;
                                if (emuTrell.Config.DepotId.IsNotEmpty)
                                    fromTrells.Config.DepotId = nTrell.Config.DepotId;
                            }
                            else
                                nTrells.Config.DepotId = fromTrells.Config.DepotId;

                            isFirst = false;
                        }
                    }
                }
            }
            return nTrells;
        }

        public static DataTrellis Rebuild(this DataTrellis trell, DataPylon[] pylons = null, DataPylon[] PKeys = null, DataSphere nSphere = null, DataGroup Group = DataGroup.None)
        {
            if (nSphere != null)
            {
                trell.Sphere = nSphere;
                trell.Relays = nSphere.Relays;
            }
            if (trell.Prime != null)
                trell.IsPrime = false;
            if (pylons != null)
                trell.Pylons.AddRange(pylons);
            else if (trell.Prime != null)
                trell.Pylons.AddRange( new List<DataPylon>(trell.Prime.Pylons.AsEnumerable().ToList()));
            if (PKeys != null)
                trell.PrimeKey = trell.Pylons.AsEnumerable().Where(c => PKeys.Where(p => p.PylonName == c.PylonName).Any()).ToArray();
            else if (trell.Prime != null)
                trell.PrimeKey = trell.Pylons.AsEnumerable().Where(c => trell.Prime.PrimeKey.Where(p => p.PylonName == c.PylonName).Any()).ToArray();
            foreach (DataPylon p in trell.PrimeKey) p.isKey = true;
            trell.Group = Group;
            DataTiers tiers = new DataTiers(trell);
            if (trell.Prime != null)
                tiers.AddRange(trell.Prime.Tiers.AsEnumerable().Select(ti => new DataTier(trell, ti)).ToArray());
            else if (trell.Tiers != null && trell.Tiers.Count > 0)
                tiers.AddRange(trell.Tiers.AsEnumerable().Select(ti => new DataTier(trell, ti)).ToArray());
            trell.Tiers = tiers;
            return trell;
        }
        public static DataTrellis Rebuild(this DataTrellis trell, DataEvent DataDelegateMethod)
        {
            if (DataDelegateMethod.InvokeInfo.Parameters != null)
            {
                trell = (DataTrellis)(DataDelegateMethod.Execute());
                return trell;
            }
            else
                return null;
        }
        public static DataTrellis Rebuild(this DataTrellis trell, DorsInvokeInfo DataDelegateMethodParameters)
        {
            if (DataDelegateMethodParameters.Parameters != null)
            {
                trell.Rebuild(new DataEvent(DataDelegateMethodParameters));
                return trell;
            }
            else
                return null;
        }

        public static DataTrellis Emulate(this DataTrellis trell, DataTrellis emuTrell, DataEmulate mode = DataEmulate.None, DataSphere set = null, DataGroup Group = DataGroup.None)
        {
            if (emuTrell.Tiers.Count > 0)
            {
                trell.Pylons.Emulate(emuTrell.Pylons);
                if (trell.Pylons.newFormula)
                    trell.Pylons.Where(p => !ReferenceEquals(p.RightFormula, null))
                                    .Select(f => f.MattabFormula = f.RightFormula).ToArray();
                trell.IsCube = emuTrell.IsCube;
                if (emuTrell.IsCube && trell.Cube == null)
                {
                    trell.Cube = new DataCube(trell, emuTrell.Cube.CubeRelays);
                    trell.Cube.CreateCube();
                    trell.Cube.FindRelays = true;
                }

                trell.IsPrime = false;

                if (trell.Prime == null || ReferenceEquals(trell.Prime, trell))
                    trell.Prime = (emuTrell.IsPrime) ? emuTrell : emuTrell.Prime;

                if (trell.Devisor == null || ReferenceEquals(trell.Devisor, trell))
                {
                    trell.Devisor = emuTrell;
                    emuTrell.IsDevisor = true;
                }
               
                trell.PrimeKey = trell.Pylons.AsEnumerable().Where(c => emuTrell.PrimeKey.Where(p => p.PylonName.Equals(c.PylonName)).Any()).ToArray();
                trell.TrellName = (trell.TrellName != null && trell.TrellName != "Table") ? trell.TrellName : emuTrell.TrellName;
                trell.DisplayName = (trell.DisplayName != null && trell.DisplayName != "Table") ? trell.DisplayName : emuTrell.DisplayName;
                trell.Visible = emuTrell.Visible;
                trell.Group = emuTrell.Group;

                if (mode != DataEmulate.None)
                    trell.EmulateMode = mode;
                else
                    trell.EmulateMode = emuTrell.EmulateMode;

                trell.AfectMap = new List<AfectMapping>(emuTrell.AfectMap);

                int length = emuTrell.Tiers.Count;
                if (trell.EmulateMode == DataEmulate.Sleeves)
                    for (int i = 0; i < length; i++)
                        trell.Tiers.AddInner(new DataTier(trell, emuTrell.Tiers[i]));
                else
                    for (int i = 0; i < length; i++)
                        trell.Tiers.AddInner(emuTrell.Tiers[i]);

               // trell.MattabHash = new HashSet<int>(emuTrell.MattabHash);
               // trell.MattabHash.Select(o => trell.Pylons[o].MattabFormula = emuTrell.Pylons[o].MattabFormula).ToArray();
               // trell.LambdaHash = new HashSet<int>(emuTrell.LambdaHash);
               // trell.LambdaHash.Select(o => trell.Pylons[o].LambdaFormula = emuTrell.Pylons[o].LambdaFormula).ToArray();

                trell.Filter.AddNewRange(emuTrell.Filter.AsEnumerable().Where(f => f.Value != null && f.PylonName != null).ToArray());
                trell.Sort.AddNewRange(emuTrell.Sort.AsEnumerable().Where(f => f.PylonName != null).ToArray());
                trell.Favorites.FavoriteRange(emuTrell.Favorites.Clone());
                trell.Tiers.Query();
                if (set != null) trell.Sphere = set;

                return trell;
            }
            else
                return trell.Imitate(emuTrell, mode, set, Group);
        }      
        public static DataTrellis Imitate(this DataTrellis trell, DataTrellis emuTrell, DataEmulate mode = DataEmulate.None, DataSphere set = null, DataGroup Group = DataGroup.None)
        {
            emuTrell.CreateModel(false);

            trell.Pylons.Emulate(emuTrell.Pylons);
            if (trell.Pylons.newFormula)
                trell.Pylons.Where(p => !ReferenceEquals(p.RightFormula, null))
                                .Select(f => f.MattabFormula = f.RightFormula).ToArray();
            trell.IsCube = emuTrell.IsCube;
            if (emuTrell.IsCube && trell.Cube == null)
            {
                trell.Cube = new DataCube(trell, emuTrell.Cube.CubeRelays);
                trell.Cube.CreateCube();
                trell.Cube.FindRelays = true;
            }

            trell.IsPrime = false;
            if (trell.Prime == null || ReferenceEquals(trell.Prime, trell))
                trell.Prime = (emuTrell.IsPrime) ? emuTrell : emuTrell.Prime;

            if (trell.Devisor == null || ReferenceEquals(trell.Devisor, trell))
            {
                trell.Devisor = emuTrell;
                emuTrell.IsDevisor = true;
            }

            trell.PrimeKey = trell.Pylons.AsEnumerable().Where(c => emuTrell.PrimeKey.Where(p => p.PylonName.Equals(c.PylonName)).Any()).ToArray();
            trell.TrellName = (trell.TrellName != null && trell.TrellName != "Table") ? trell.TrellName : emuTrell.TrellName;
            trell.DisplayName = (trell.DisplayName != null && trell.DisplayName != "Table") ? trell.DisplayName : emuTrell.DisplayName;
            trell.Visible = emuTrell.Visible;
            trell.Group = emuTrell.Group;
            if (mode != DataEmulate.None)
                trell.EmulateMode = mode;
            else
                trell.EmulateMode = emuTrell.EmulateMode;
            trell.AfectMap = new List<AfectMapping>(emuTrell.AfectMap);
            trell.Tiers = new DataTiers(trell);
            trell.Filter.AddNewRange(emuTrell.Filter.AsEnumerable().Where(f => f.Value != null && f.PylonName != null).ToArray());
            trell.Sort.AddNewRange(emuTrell.Sort.AsEnumerable().Where(f => f.PylonName != null).ToArray());
            trell.Favorites.FavoriteRange(emuTrell.Favorites.Clone());
            
            //trell.MattabHash = new HashSet<int>(emuTrell.MattabHash);
            //trell.MattabHash.Select(o => trell.Pylons[o].Mathrix = emuTrell.Pylons[o].Mathrix.Clone()).ToArray();
            //trell.LambdaHash = new HashSet<int>(emuTrell.LambdaHash);
            //trell.LambdaHash.Select(o => trell.Pylons[o].LambdaFormula = emuTrell.Pylons[o].LambdaFormula).ToArray();
            trell.Sphere = set;

            return trell;
        }
        public static DataTrellis Impact(this DataTrellis trell, DataTrellis emuTrell, bool toDrive = false, bool propagateState = true, DataSphere set = null, DataGroup Group = DataGroup.None)
        {
            if(!trell.IsCube)
                trell.IsCube = emuTrell.IsCube;

            if (emuTrell.IsCube &&
                (trell.Cube == null ||
                trell.Cube.FindRelays))
            {
                trell.Cube = new DataCube(trell, emuTrell.Cube.CubeRelays);
                trell.Cube.FindRelays = true;
            }

            if (!trell.IsPrime)
                trell.IsPrime = emuTrell.IsPrime;

            if (!trell.IsDevisor)
                trell.IsDevisor = emuTrell.IsDevisor;

            if (emuTrell.IsPrime)
                if (trell.Counter < emuTrell.Counter)
                    trell.Counter = emuTrell.Counter;

            if (emuTrell.Prime != null && trell.Prime == null)
                trell.Prime = emuTrell.Prime.Locate();

            if (emuTrell.Devisor != null && trell.Devisor == null)
                trell.Devisor = emuTrell.Devisor.Locate();

            if (emuTrell.Pylons != null)
            {
                trell.Pylons.Impact(emuTrell.Pylons);
                trell.Pylons.hasAutoId = emuTrell.Pylons.hasAutoId;
                if (trell.Pylons.newFormula)
                    trell.Pylons.Where(p => !ReferenceEquals(p.RightFormula, null))
                                    .Select(f => f.MattabFormula = f.RightFormula).ToArray();
            }
            //if (emuTrell.MattabHash.Count > trell.MattabHash.Count || emuTrell.MattabHash.Any(m => !trell.MattabHash.Contains(m)))
            //{
            //   // trell.MattabHash = new HashSet<int>(emuTrell.MattabHash);
            //   // trell.MattabHash.Select(o => trell.Pylons[o].Mathrix = emuTrell.Pylons[o].Mathrix.Clone()).ToArray();
            //}
            //if (emuTrell.LambdaHash.Count > trell.LambdaHash.Count || emuTrell.LambdaHash.Any(m => !trell.LambdaHash.Contains(m)))
            //{
            //   // trell.LambdaHash = new HashSet<int>(emuTrell.LambdaHash);
            //   // trell.LambdaHash.Select(o => trell.Pylons[o].LambdaFormula = emuTrell.Pylons[o].LambdaFormula).ToArray();
            //}

            if (emuTrell.PrimeKey != null)
                if (trell.PrimeKey == null || emuTrell.PrimeKey.Length != trell.PrimeKey.Length)
                    trell.PrimeKey = trell.Pylons.AsEnumerable().Where(c => emuTrell.PrimeKey.Where(p => p.PylonName == c.PylonName).Any()).ToArray();

            if (emuTrell.TrellName != null)
                trell.TrellName = (trell.TrellName != null && trell.TrellName != "Table") ? trell.TrellName : emuTrell.TrellName;

            if (emuTrell.DisplayName != null)
                trell.DisplayName = (trell.DisplayName != null && trell.DisplayName != "Table") ? trell.DisplayName : emuTrell.DisplayName;

            trell.Visible = emuTrell.Visible;

            if (trell.IsPrime)
                if (emuTrell.Deposit != null)
                    trell.Deposit = emuTrell.Deposit.Locate();

            if (emuTrell.Filter != null)
                trell.Filter.AddNewRange(emuTrell.Filter.AsEnumerable().Where(f => f.Value != null && f.PylonName != null).ToArray());
            if (emuTrell.Sort != null)
                trell.Sort.AddNewRange(emuTrell.Sort.AsEnumerable().Where(f => f.PylonName != null).ToArray());
            if (emuTrell.Favorites != null)
                trell.Favorites.FavoriteRange(emuTrell.Favorites);

            if (emuTrell.Group != DataGroup.None)
                trell.Group = emuTrell.Group;
            if (emuTrell.EmulateMode != DataEmulate.References)
                trell.EmulateMode = emuTrell.EmulateMode;

            if (emuTrell.State != null)
                trell.State.Impact(emuTrell.State, propagateState);

            trell.DeserialCount = emuTrell.SerialCount;
            trell.Tiers.DeserialCount = emuTrell.SerialCount;
            trell.Tiers.TiersView.DeserialCount = emuTrell.SerialCount;
            trell.Sims.DeserialCount = emuTrell.SerialCount;
            trell.Sims.TiersView.DeserialCount = emuTrell.SerialCount;

            if (emuTrell.PagingDetails != null)
                trell.PagingDetails.Impact(emuTrell.PagingDetails);

            if (trell.Config.DepotId.IsEmpty)
            {
                trell.Config = emuTrell.Config;
                if (trell.Parameters != null)
                    trell.Parameters.Registry.AddRange(new Dictionary<string, object>(emuTrell.Parameters.Registry));
            }
            DataSpace.Registry.TryAdd(trell.Config.GetDataId(), trell);

            if (toDrive)
                trell.WriteDrive();

            emuTrell = null;
            return trell;
        }
        public static DataTrellis Locate(this DataTrellis trell, string path = null)
        {
            object result = null;           
            DataTrellis _trell = null;
            if (trell != null)
            {
                DataSpace.Registry.TryGetValue(trell.Config.DataId, out result);
                if (result != null)
                    _trell = (DataTrellis)result;
                else
                {
                    string _path = (path == null) ? trell.Config.Place : path;
                    string[] items = _path.Split('/');
                    int length = items.Length;
                    int last = length - 1;
                    bool isTrellis = (items[last].Split('.').Length > 1) ? true : false;
                    DataSpheres sets = DataSpace.Area[items[1]];
                    DataSphere set = null;
                    if (length > 2 && sets != null)
                    {
                        if (isTrellis)
                        {
                            if ((last) == 3)
                                _trell = sets[items[2], items[3].Split('.')[0]];
                            else
                            {
                                set = sets[items[2]];
                                if (set != null && length > 4)
                                {
                                    for (int i = 3; i < length; i++)
                                    {
                                        DataSphere tempset = null;
                                        if ((last - 1) == i)
                                            _trell = set[items[i], items[last].Split('.')[0]];
                                        else
                                        {
                                            tempset = set[items[i]];
                                            if (tempset != null)
                                                set = tempset;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return _trell;
        }

        public static DataRelay AddRelay(this DataTrellis parentTrell, DataTrellis childTrell, string[] parentKeys, string[] childKeys)
        {
            DataRelayMember parent = parentTrell.RelayMember(parentKeys);
            DataRelayMember child = childTrell.RelayMember(childKeys);
            return parent.CreateRelay(parentTrell.TrellName + "_" + childTrell.TrellName, child);
        }
        public static DataRelay AddRelay(this DataTrellis parentTrell, DataTrellis childTrell, string relayname, string[] parentKeys, string[] childKeys)
        {
            DataRelayMember parent = parentTrell.RelayMember(parentKeys);
            DataRelayMember child = childTrell.RelayMember(childKeys);
            string _relayname = (relayname != null) ? relayname : parentTrell.TrellName + "_" + childTrell.TrellName;
            return parent.CreateRelay(_relayname, child);            
        }
        public static DataRelay AddRelay(this DataTrellis parentTrell, DataTrellis childTrell, DataPylon[] parentKeys, DataPylon[] childKeys)
        {
            DataRelayMember parent = parentTrell.RelayMember(parentKeys);
            DataRelayMember child = childTrell.RelayMember(childKeys);
            return parent.CreateRelay(parentTrell.TrellName + "_" + childTrell.TrellName, child);     
        }
        public static DataRelay AddRelay(this DataTrellis parentTrell, DataTrellis childTrell, string relayname, DataPylon[] parentKeys, DataPylon[] childKeys)
        {
            DataRelayMember parent = parentTrell.RelayMember(parentKeys);
            DataRelayMember child = childTrell.RelayMember(childKeys);
            return parent.CreateRelay(relayname, child);
        }

        public static DataRelay AddRelay(this DataTrellis parentTrell, DataTrellis childTrell, string[] keys)
        {
            DataRelayMember parent = parentTrell.RelayMember(keys);
            DataRelayMember child = childTrell.RelayMember(keys);
            return parent.CreateRelay(parentTrell.TrellName + "_" + childTrell.TrellName, child);
        }
        public static DataRelay AddRelay(this DataTrellis parentTrell, DataTrellis childTrell, string relayname, string[] keys)
        {
            DataRelayMember parent = parentTrell.RelayMember(keys);
            DataRelayMember child = childTrell.RelayMember(keys);
            string _relayname = (relayname != null) ? relayname : parentTrell.TrellName + "_" + childTrell.TrellName;
            return parent.CreateRelay(_relayname, child);
        }
        public static DataRelay AddRelay(this DataTrellis parentTrell, DataTrellis childTrell, DataPylon[] keys)
        {
            DataRelayMember parent = parentTrell.RelayMember(keys);
            DataRelayMember child = childTrell.RelayMember(keys);
            return parent.CreateRelay(parentTrell.TrellName + "_" + childTrell.TrellName, child);
        }

        private static DataRelay CreateRelay(this DataRelayMember parent, string relayName, DataRelayMember child)
        {
            DataRelay rd = new DataRelay() { Child = child, Parent = parent, RelayName = relayName };
            DataSphere cs = child.Trell.Sphere;
            DataSphere ps = parent.Trell.Sphere;
            rd.PropagateRelay();

            return rd;
        }
        public static void PropagateRelay(this DataRelay relay)
        {
            DataTrellis ct = relay.Child.Trell;
            DataTrellis pt = relay.Parent.Trell;
            if (ct.Relays == null)
                ct.Relays = new DataRelays();
            ct.Relays.Add(relay);
            if (ct.IsPrime || ct.IsDevisor)
                ct.Tiers.Registry.Keymap.RebuildMap(relay.RelayName);
            ct.AssignJoinPylons();
           
            if (pt.Relays == null)
                pt.Relays = new DataRelays();
            pt.Relays.Add(relay);
            if (pt.IsPrime || pt.IsDevisor)
                pt.Tiers.Registry.Keymap.RebuildMap(relay.RelayName);
            pt.AssignJoinPylons();

            DataSphere cs = ct.Sphere;
            DataSphere ps = pt.Sphere;
            if (cs != null && ps != null)
            {
                if (ps != null)
                {
                    if (ps.Relays == null)
                        ps.Relays = new DataRelays();
                    //if (!ps.Relays.Have(relay.RelayName))
                    //{
                        ps.Relays.Add(relay);
                 //   }
                }
                if (cs != null)
                    if (ps == null || ((ps != null) && !ReferenceEquals(cs, ps)))
                    {
                        if (cs.Relays == null)
                            cs.Relays = new DataRelays();
                        //if (!cs.Relays.Have(relay.RelayName))
                        //{
                            cs.Relays.Add(relay);
                        //}
                    }
            }
        }

        private static DataRelayMember RelayMember(this DataTrellis trell, string[] colNames)
        {
            DataRelayMember r = new DataRelayMember(trell);
            r.TrellName = trell.TrellName;
            r.PylonKeys = trell.Pylons.AsEnumerable().Where(c => colNames.Contains(c.PylonName)).ToArray();
            return r;
        }
        private static DataRelayMember RelayMember(this DataTrellis trell, DataPylon[] columns)
        {
            DataRelayMember r = new DataRelayMember(trell);
            r.TrellName = trell.TrellName;
            r.PylonKeys = trell.Pylons.AsEnumerable().Where(c => columns.Where(cu => cu.PylonName == c.PylonName).Any()).ToArray();
            return r;
        }

        public static DataSpheres EmulateRelays(this DataSpheres nSpheres, DataSpheres fromDataSpheres, object topSphere, string SpheresName = null)
        {
            DataSpheres pSpheres = fromDataSpheres;
            foreach (KeyValuePair<string, DataSphere> item in pSpheres.Spheres)
            {
                DataSphere nSphere = null;
                if (nSpheres.Spheres.ContainsKey(item.Value.SphereId))
                {
                    nSphere = nSpheres.Spheres[item.Value.SphereId];
                    nSphere.EmulateRelays(item.Value, topSphere);
                }
            }
            if (pSpheres.SpheresIn != null)
            {
                if (nSpheres.SpheresIn == null)
                    nSpheres.SpheresIn = new DataSpheres(pSpheres.SpheresIn.SpheresId);
                nSpheres.SpheresIn.EmulateRelays(pSpheres.SpheresIn, topSphere);
            }
            return nSpheres;
        }
        public static DataTrellis EmulateRelays(this DataTrellis toTrell, DataTrellis fromTrell, object lookupSphere)
        {            
            DataRelays emuRelays = fromTrell.Relays;
            DataCreator ndc = new DataCreator();
            foreach (DataRelay emuRelay in emuRelays)
            {
                if (!toTrell.Relays.Have(emuRelay.RelayName))
                {
                    DataTrellis parentTrell = null;
                    DataTrellis childTrell = null;

                    string parentName = emuRelay.Parent.Trell.TrellName,
                           childName = emuRelay.Child.Trell.TrellName;
                    ArrayList lookparent = new ArrayList();
                    ArrayList lookchild = new ArrayList();
                    if (lookupSphere is DataSpheres)
                    {
                        lookparent = ((DataSpheres)lookupSphere).Lookup(parentName);
                        lookchild = ((DataSpheres)lookupSphere).Lookup(childName);
                    }
                    else if (lookupSphere is DataSphere)
                    {
                        lookparent = ((DataSphere)lookupSphere).Lookup(parentName);
                        lookchild = ((DataSphere)lookupSphere).Lookup(childName);
                    }

                    DataTrellis[] parentTrells = lookparent.ToArray().Where(pt => pt is DataTrellis).Cast<DataTrellis>().ToArray();
                    DataTrellis[] childTrells = lookchild.ToArray().Where(pt => pt is DataTrellis).Cast<DataTrellis>().ToArray();
                    if (parentTrells.Length > 0 && childTrells.Length > 0)
                    {
                        parentTrell = parentTrells[0];
                        childTrell = childTrells[0];

                        DataPylon[] parentKeys = parentTrell.Pylons.AsEnumerable().Where(c => emuRelay.Parent.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        DataPylon[] childKeys = childTrell.Pylons.AsEnumerable().Where(c => emuRelay.Child.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        if (parentKeys != null && childKeys != null)
                        {
                            ndc.BuildRelays(emuRelay.RelayName, parentTrell, childTrell, parentKeys, childKeys);
                        }
                    }
                }

            }
            return toTrell;
        }
        public static DataTrellis ImitateRelays(this DataTrellis toTrell, DataTrellis fromTrell)
        {
            if (toTrell != null)
            {
                DataRelays iru = fromTrell.Relays;
                DataCreator ndc = new DataCreator();
                foreach (DataRelay irdu in iru)
                {
                    string iSetUpParentName = irdu.Parent.Trell.TrellName,
                           iSetUpChildName = irdu.Child.Trell.TrellName;
                    DataTrellis iSetParentTrell = toTrell.Sphere.Trells[iSetUpParentName];
                    DataTrellis iSetChildTrell = toTrell.Sphere.Trells[iSetUpChildName];
                    if (iSetParentTrell != null && iSetChildTrell != null)
                    {
                        DataPylon[] parentKeys = iSetParentTrell.Pylons.AsEnumerable().Where(c => irdu.Parent.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        DataPylon[] childKeys = iSetChildTrell.Pylons.AsEnumerable().Where(c => irdu.Child.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        if (parentKeys != null && childKeys != null)
                        {
                            ndc.BuildRelays(irdu.RelayName, iSetParentTrell, iSetChildTrell, parentKeys, childKeys);
                        }
                    }

                }
            }
            return toTrell;
        }
        public static DataSphere  EmulateRelays(this DataSphere toDataSphere, DataSphere fromDataSphere, object lookupSphere)
        {
            DataRelays emuRelays = fromDataSphere.Relays;
            DataCreator ndc = new DataCreator();
            foreach (DataRelay emuRelay in emuRelays)
            {
                if (!toDataSphere.Relays.Have(emuRelay.RelayName))
                {
                    DataTrellis parentTrell = null;
                    DataTrellis childTrell = null;

                    string parentName = emuRelay.Parent.Trell.TrellName,
                       childName = emuRelay.Child.Trell.TrellName;
                    ArrayList lookparent = new ArrayList();
                    ArrayList lookchild = new ArrayList();
                    if (lookupSphere is DataSpheres)
                    {
                        lookparent = ((DataSpheres)lookupSphere).Lookup(parentName);
                        lookchild = ((DataSpheres)lookupSphere).Lookup(childName);
                    }
                    else if (lookupSphere is DataSphere)
                    {
                        lookparent = ((DataSphere)lookupSphere).Lookup(parentName);
                        lookchild = ((DataSphere)lookupSphere).Lookup(childName);
                    }

                    DataTrellis[] parentTrells = lookparent.ToArray().Where(pt => pt is DataTrellis).Cast<DataTrellis>().ToArray();
                    DataTrellis[] childTrells = lookchild.ToArray().Where(pt => pt is DataTrellis).Cast<DataTrellis>().ToArray();
                    if (parentTrells.Length > 0 && childTrells.Length > 0)
                    {
                        parentTrell = parentTrells[0];
                        childTrell = childTrells[0];

                        DataPylon[] parentKeys = parentTrell.Pylons.AsEnumerable().Where(c => emuRelay.Parent.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        DataPylon[] childKeys = childTrell.Pylons.AsEnumerable().Where(c => emuRelay.Child.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        if (parentKeys != null && childKeys != null)
                        {
                            ndc.BuildRelays(emuRelay.RelayName, parentTrell, childTrell, parentKeys, childKeys);
                        }
                    }
                }
            }

            if (fromDataSphere.SpheresIn != null)
                toDataSphere.SpheresIn.EmulateRelays(fromDataSphere.SpheresIn, lookupSphere);

            if (fromDataSphere.SphereIn != null)
                toDataSphere.SphereIn.EmulateRelays(fromDataSphere.SphereIn, lookupSphere);

            return toDataSphere;
        }
        public static DataSphere  ImitateRelays(this DataSphere toDataSphere, DataSphere fromDataSphere)
        {
            if (fromDataSphere != null)
            {
                DataRelays iru = fromDataSphere.Relays;
                DataCreator ndc = new DataCreator();
                foreach (DataRelay irdu in iru)
                {
                    string iSetUpParentName = irdu.Parent.Trell.TrellName,
                           iSetUpChildName = irdu.Child.Trell.TrellName;
                    DataTrellis iSetParentTrell = toDataSphere.Trells[iSetUpParentName];
                    DataTrellis iSetChildTrell = toDataSphere.Trells[iSetUpChildName];
                    if (iSetParentTrell != null && iSetChildTrell != null)
                    {
                        DataPylon[] parentKeys = iSetParentTrell.Pylons.AsEnumerable().Where(c => irdu.Parent.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        DataPylon[] childKeys = iSetChildTrell.Pylons.AsEnumerable().Where(c => irdu.Child.RelayKeys.AsEnumerable().Select(pk => pk.PylonName).Contains(c.PylonName)).ToArray();
                        if (parentKeys != null && childKeys != null)
                        {
                            ndc.BuildRelays(irdu.RelayName, iSetParentTrell, iSetChildTrell, parentKeys, childKeys);
                        }
                    }

                }
            }
            return toDataSphere;
        }

        public static DataSpheres EmulateCubes(this DataSpheres nSpheres, object topSphere, string SpheresName = null)
        {
            foreach (KeyValuePair<string, DataSphere> item in nSpheres.Spheres)
            {
                DataSphere nSphere = null;
                if (nSpheres.Spheres.ContainsKey(item.Value.SphereId))
                    nSphere = nSpheres.Spheres[item.Value.SphereId];
                nSphere.EmulateCubes(topSphere);
            }
            if (nSpheres.SpheresIn != null)
            {
                if (nSpheres.SpheresIn == null)
                    nSpheres.SpheresIn = new DataSpheres(nSpheres.SpheresIn.SpheresId);
                nSpheres.SpheresIn.EmulateCubes(topSphere);
            }
            return nSpheres;
        }
        public static DataTrellis EmulateCubes(this DataTrellis toTrell, object lookupSphere, bool save = false)
        {
            toTrell.Sphere.EmulateCubes(toTrell.Sphere, save);            
            return toTrell;
        }
        public static DataSphere  EmulateCubes(this DataSphere toDataSphere, object lookupSphere, bool save = false)
        {
            foreach (DataTrellis trs in toDataSphere.Trells)
            {
                if (trs.IsCube && trs.Cube.FindRelays)
                {
                    if (trs.Cube.CubeRelays != null &&
                        trs.Cube.CubeRelays.Count > 0)
                    {
                        DataRelays emuRelays = trs.Cube.CubeRelays;
                        DataRelays tempRelays = new DataRelays();
                        string tempname = null;
                        ArrayList lookparent = new ArrayList();
                        foreach (DataRelay emuRelay in emuRelays)
                        {
                            DataTrellis parentTrell = null;
                            string parentName = emuRelay.Parent.Trell.TrellName;

                            if (tempname == null || tempname.Equals(parentName))
                            {
                                if (lookupSphere is DataSpheres)
                                    lookparent = ((DataSpheres)lookupSphere).Lookup(parentName);
                                else if (lookupSphere is DataSphere)
                                    lookparent = ((DataSphere)lookupSphere).Lookup(parentName);
                            }

                            DataTrellis[] parentTrells = lookparent.ToArray()
                                                            .Where(pt => pt is DataTrellis)
                                                                .Cast<DataTrellis>().ToArray();
                            if (parentTrells.Length > 0)
                            {
                                parentTrell = parentTrells[0];
                                tempRelays.Add(parentTrell.Relays[emuRelay.RelayName]);
                            }
                        }
                        trs.Cube.CubeRelays = tempRelays;
                    }
                    trs.Cube.FindRelays = false;
                    trs.Cube.CreateCube();
                }
            }

            if (toDataSphere.SpheresIn != null)
                toDataSphere.SpheresIn.EmulateCubes(lookupSphere);            

            if (toDataSphere.SphereIn != null)           
                toDataSphere.SphereIn.EmulateCubes(lookupSphere);            

            return toDataSphere;
        }

        public static  DataSphere  Emulate(this DataSphere nSphere, DataSphere fromDataSphere, string SphereName = null, DataSphere cubeRelaySphere = null)
        {
            DataSphere pSphere = fromDataSphere;
            string DataSphereId = (SphereName != null) ? SphereName : pSphere.SphereId;
            if (nSphere.Config.DepotId.IsEmpty || nSphere.Config.Place == string.Empty)
            {
                string SphereId = (SphereName != null) ? SphereName : pSphere.SphereId;
                nSphere.SphereId = SphereId;
                nSphere.Parameters.Registry.AddRange(new Dictionary<string, object>(pSphere.Parameters.Registry));
            }

            foreach (DataTrellis fromTrell in pSphere.Trells)
            {
                DataTrellis emuTrell = fromTrell;
                DataTrellis _trell = (!nSphere.Trells.Have(emuTrell.TrellName)) ?
                    new DataTrellis(emuTrell.TrellName, fromTrell) : 
                    nSphere.Trells[emuTrell.TrellName];
                _trell.Emulate(emuTrell);
                nSphere.Trells.Add(_trell);
            }

            if (pSphere.SpheresIn != null)
            {
                if(nSphere.SpheresIn == null)
                    nSphere.SpheresIn = new DataSpheres(pSphere.SpheresIn.SpheresId);
                nSphere.SpheresIn.emulate(pSphere.SpheresIn);
            }
            if (pSphere.SphereIn != null)
            {
                if (nSphere.SphereIn == null)
                    nSphere.SphereIn = new DataSphere(pSphere.SphereIn.SphereId);
                nSphere.SphereIn.emulate(pSphere.SphereIn);
            }

            nSphere.EmulateRelays(fromDataSphere, nSphere);
            if (cubeRelaySphere != null)
            {
                nSphere.Devisor = cubeRelaySphere;
                nSphere.EmulateCubes(cubeRelaySphere);
            }
            else
                nSphere.EmulateCubes(nSphere);

            pSphere.IsDivision = true;
            nSphere.IsDivision = true;

            return nSphere;
        }
        private static DataSphere  emulate(this DataSphere nSphere, DataSphere fromDataSphere, string SphereName = null)
        {
            DataSphere pSphere = fromDataSphere;
            string DataSphereId = (SphereName != null) ? SphereName : pSphere.SphereId;
            if (nSphere.Config.DepotId.IsEmpty || nSphere.Config.Place == string.Empty)
            {
                string SphereId = (SphereName != null) ? SphereName : pSphere.SphereId;
                nSphere.SphereId = SphereId;
                nSphere.Parameters.Registry.AddRange(new Dictionary<string, object>(pSphere.Parameters.Registry));
            }

            foreach (DataTrellis fromTrell in pSphere.Trells)
            {
                DataTrellis emuTrell = fromTrell;
                DataTrellis _trell = (!nSphere.Trells.Have(emuTrell.TrellName)) ? 
                    new DataTrellis(emuTrell.TrellName, fromTrell) : 
                    nSphere.Trells[emuTrell.TrellName];
                _trell.Emulate(emuTrell);
                nSphere.Trells.Add(_trell);
            }

            nSphere.ImitateRelays(pSphere);

            if (pSphere.SpheresIn != null)
            {
                if (nSphere.SpheresIn == null)
                    nSphere.SpheresIn = new DataSpheres(pSphere.SpheresIn.SpheresId);
                nSphere.SpheresIn.emulate(pSphere.SpheresIn);
            }
            if (pSphere.SphereIn != null)
            {
                if (nSphere.SphereIn == null)
                    nSphere.SphereIn = new DataSphere(pSphere.SphereIn.SphereId);
                nSphere.SphereIn.emulate(pSphere.SphereIn);
            }

            return nSphere;
        }

        public static  DataSphere Imitate(this DataSphere nSphere, DataSphere fromDataSphere, string SphereName = null, DataSphere cubeRelaySphere = null)
        {
            DataSphere pSphere = fromDataSphere;
            string DataSphereId = (SphereName != null) ? SphereName : pSphere.SphereId;
            if (nSphere.Config.DepotId.IsEmpty || nSphere.Config.Place == string.Empty)
            {
                string SphereId = (SphereName != null) ? SphereName : pSphere.SphereId;
                nSphere.SphereId = SphereId;
                nSphere.Parameters.Registry.AddRange(new Dictionary<string, object>(pSphere.Parameters.Registry));
            }

            foreach (DataTrellis fromTrell in pSphere.Trells)
            {
                DataTrellis emuTrell = fromTrell;
                DataTrellis _trell = (!nSphere.Trells.Have(emuTrell.TrellName)) ?
                    new DataTrellis(emuTrell.TrellName, fromTrell) :
                    nSphere.Trells[emuTrell.TrellName];
                _trell.Imitate(emuTrell);
                nSphere.Trells.Add(_trell);
            }

            if (pSphere.SpheresIn != null)
            {
                if (nSphere.SpheresIn == null)
                    nSphere.SpheresIn = new DataSpheres(pSphere.SpheresIn.SpheresId);
                nSphere.SpheresIn.imitate(pSphere.SpheresIn);
            }
            if (pSphere.SphereIn != null)
            {
                if (nSphere.SphereIn == null)
                    nSphere.SphereIn = new DataSphere(pSphere.SphereIn.SphereId);
                nSphere.SphereIn.imitate(pSphere.SphereIn);
            }

            nSphere.EmulateRelays(fromDataSphere, nSphere);
            if (cubeRelaySphere != null)
            {
                nSphere.Devisor = cubeRelaySphere;
                nSphere.EmulateCubes(cubeRelaySphere);
            }
            else
                nSphere.EmulateCubes(nSphere);

            pSphere.IsDivision = true;
            nSphere.IsDivision = true;

            return nSphere;
        }
        private static DataSphere imitate(this DataSphere nSphere, DataSphere fromDataSphere, string SphereName = null)
        {
            DataSphere pSphere = fromDataSphere;
            string DataSphereId = (SphereName != null) ? SphereName : pSphere.SphereId;
            if (nSphere.Config.DepotId.IsEmpty || nSphere.Config.Place == string.Empty)
            {
                string SphereId = (SphereName != null) ? SphereName : pSphere.SphereId;
                nSphere.SphereId = SphereId;
                nSphere.Parameters.Registry.AddRange(new Dictionary<string, object>(pSphere.Parameters.Registry));
            }

            foreach (DataTrellis fromTrell in pSphere.Trells)
            {
                DataTrellis emuTrell = fromTrell;
                DataTrellis _trell = (!nSphere.Trells.Have(emuTrell.TrellName)) ?
                    new DataTrellis(emuTrell.TrellName, fromTrell) :
                    nSphere.Trells[emuTrell.TrellName];
                _trell.Imitate(emuTrell);
                nSphere.Trells.Add(_trell);
            }

            nSphere.ImitateRelays(pSphere);

            if (pSphere.SpheresIn != null)
            {
                if (nSphere.SpheresIn == null)
                    nSphere.SpheresIn = new DataSpheres(pSphere.SpheresIn.SpheresId);
                nSphere.SpheresIn.imitate(pSphere.SpheresIn);
            }
            if (pSphere.SphereIn != null)
            {
                if (nSphere.SphereIn == null)
                    nSphere.SphereIn = new DataSphere(pSphere.SphereIn.SphereId);
                nSphere.SphereIn.imitate(pSphere.SphereIn);
            }

            return nSphere;
        }

        public static DataSphere   Impact(this DataSphere nSphere, DataSphere pSphere, bool toDrive = false, string SphereName = null)
        {
            if (nSphere.Config.DepotId.IsEmpty)
            {
                string SphereId = (SphereName != null) ? SphereName : pSphere.SphereId;
                nSphere.SphereId = SphereId;
                nSphere.Config = pSphere.Config;
                nSphere.Parameters.Registry.AddRange(new Dictionary<string, object>(pSphere.Parameters.Registry));
            }
            if (pSphere.IsDivision)
                nSphere.IsDivision = pSphere.IsDivision;

            nSphere.State.Impact(pSphere.State, false);

            foreach (DataTrellis emuTrell in pSphere.Trells)
            {
                DataTrellis _trell = null;

                if (!nSphere.Trells.Have(emuTrell.TrellName))
                {
                    if (!emuTrell.IsPrime)
                        _trell = new DataTrellis(emuTrell.TrellName, emuTrell, true);
                    else if (emuTrell.Deposit != null)
                        _trell = ((DataDeposit)emuTrell.Deposit.Locator()).Box.Prime;
                    else if (emuTrell.Prime != null)
                        _trell = ((DataTrellis)emuTrell.Prime.Locator());

                    nSphere.Trells.Add(_trell);
                    nSphere.State.Added = true;
                    _trell.State.Added = true;
                }
                else
                    _trell = nSphere.Trells[emuTrell.TrellName];

                if (nSphere.State.Saved)
                    _trell.State.Saved = true;

                _trell.Impact(emuTrell, toDrive, false);
            }           

            if (pSphere.SpheresIn != null)
            {
                if (nSphere.SpheresIn == null)
                    nSphere.SpheresIn = new DataSpheres(pSphere.SpheresIn.SpheresId);

                if (pSphere.SpheresIn.IsDivision)
                    nSphere.SpheresIn.Impact(pSphere.SpheresIn);
                else
                    nSphere.SpheresIn.impact(pSphere.SpheresIn);
            }

            if (pSphere.SphereIn != null)
            {
                if (nSphere.SphereIn == null)
                    nSphere.SphereIn = new DataSphere(pSphere.SphereIn.SphereId);

                if (pSphere.SphereIn.IsDivision)
                    nSphere.SphereIn.Impact(pSphere.SphereIn);
                else
                    nSphere.SphereIn.impact(pSphere.SphereIn);
            }

            if (pSphere.Devisor != null)
            {
                nSphere.Devisor = pSphere.Devisor.Locate();
                nSphere.EmulateRelays(pSphere, nSphere);
                nSphere.EmulateCubes(nSphere.Devisor);
            }
            else
            {
                nSphere.EmulateRelays(pSphere, nSphere);
                nSphere.EmulateCubes(nSphere);
            }      

            if (toDrive)
                nSphere.WriteDrive();
          
            pSphere = null;
            return nSphere;
        }
        public static DataSphere   impact(this DataSphere nSphere, DataSphere pSphere, bool toDrive = false, string SphereName = null)
        {
            if (nSphere.Config.DepotId.IsEmpty)
            {
                string SphereId = (SphereName != null) ? SphereName : pSphere.SphereId;
                nSphere.SphereId = SphereId;
                nSphere.Config = pSphere.Config;
                nSphere.IsDivision = pSphere.IsDivision;
                nSphere.Parameters.Registry.AddRange(new Dictionary<string, object>(pSphere.Parameters.Registry));
            }

            nSphere.State.Impact(pSphere.State, false);

            foreach (DataTrellis emuTrell in pSphere.Trells)
            {
                DataTrellis _trell = null;

                if (!nSphere.Trells.Have(emuTrell.TrellName))
                {
                    if (!emuTrell.IsPrime)
                        _trell = new DataTrellis(emuTrell.TrellName, emuTrell, true);
                    else if (emuTrell.Deposit != null)
                        _trell = ((DataDeposit)emuTrell.Deposit.Locator()).Box.Prime;
                    else if (emuTrell.Prime != null)
                        _trell = ((DataTrellis)emuTrell.Prime.Locator());

                    nSphere.Trells.Add(_trell);
                    nSphere.State.Added = true;
                    _trell.State.Added = true;
                }
                else
                    _trell = nSphere.Trells[emuTrell.TrellName];

                if (nSphere.State.Saved)
                    _trell.State.Saved = true;

                _trell.Impact(emuTrell, toDrive);
            }            

            if (pSphere.SpheresIn != null)
            {
                if (nSphere.SpheresIn == null)
                    nSphere.SpheresIn = new DataSpheres(pSphere.SpheresIn.SpheresId);

                if (pSphere.SpheresIn.IsDivision)
                    nSphere.SpheresIn.Impact(pSphere.SpheresIn);
                else
                    nSphere.SpheresIn.impact(pSphere.SpheresIn);
            }

            if (pSphere.SphereIn != null)
            {
                if (nSphere.SphereIn == null)
                    nSphere.SphereIn = new DataSphere(pSphere.SphereIn.SphereId);

                if (pSphere.SphereIn.IsDivision)
                    nSphere.SphereIn.Impact(pSphere.SphereIn);
                else
                    nSphere.SphereIn.impact(pSphere.SphereIn);
            }

            if (toDrive)
                nSphere.WriteDrive();

            pSphere = null;
            return nSphere;
        }

        public static DataSphere   Locate(this DataSphere nSphere, string _path = null)
        {
            string path = (_path == null) ? nSphere.Config.Place : _path;
            string[] items = path.Split('/');
            int length = items.Length;
            DataSpheres sets = DataSpace.Area[items[1]];
            DataSphere set = null;
            if (length > 2 && sets != null)
            {
                set = sets[items[2]];
                if (set != null && length > 3)
                {
                    for (int i = 3; i < length; i++)
                    {
                        DataSphere tempset = set[items[i]];
                        if(tempset != null)
                            set = tempset;
                    }
                }
            }
            return set;             
        }
        public static ArrayList    Lookup(this DataSphere nSphere, string objectname)
        {
            ArrayList result = new ArrayList();

            if (nSphere.Trells.Have(objectname))
                result.Add(nSphere.Trells[objectname]);

            if (nSphere.SpheresIn != null)
            {
                if (nSphere.SpheresIn.Have(objectname))
                    result.Add(nSphere.SpheresIn[objectname]);

                foreach(DataSphere sph in nSphere.SpheresIn.Spheres.Values)
                {
                    ArrayList subresult = sph.Lookup(objectname);
                    if (subresult.Count > 0)
                        result.AddRange(subresult);
                }
            }
            
            return result;
        }

        public static  DataSpheres Emulate(this DataSpheres nSpheres, DataSpheres fromDataSpheres, string SpheresName = null, DataSpheres cubeRelaySpheres = null)
        {
            DataSpheres pSpheres = fromDataSpheres;
            if (nSpheres.Config.DepotId.IsEmpty)
            {
                string SpheresId = (SpheresName != null) ? SpheresName : pSpheres.SpheresId;
                nSpheres.SpheresId = SpheresId;
                nSpheres.Parameters.Registry.AddRange(new Dictionary<string, object>(pSpheres.Parameters.Registry));
            }
            foreach (KeyValuePair<string, DataSphere> item in pSpheres.Spheres)
            {
                DataSphere nSphere = null;
                if (!nSpheres.Spheres.ContainsKey(item.Value.SphereId))
                {
                    nSphere = new DataSphere(item.Value.SphereId);
                    nSpheres.Add(item.Key, nSphere);
                }
                else
                    nSphere = nSpheres.Spheres[item.Value.SphereId];

                nSphere.emulate(item.Value);

            }
            if (pSpheres.SpheresIn != null)
            {
                if (nSpheres.SpheresIn == null)
                    nSpheres.SpheresIn = new DataSpheres(pSpheres.SpheresIn.SpheresId);
                nSpheres.SpheresIn.emulate(pSpheres.SpheresIn);
            }

            nSpheres.EmulateRelays(fromDataSpheres, nSpheres);
            if (cubeRelaySpheres != null)
            {
                nSpheres.Devisor = cubeRelaySpheres;
                nSpheres.EmulateCubes(cubeRelaySpheres);
            }
            else
                nSpheres.EmulateCubes(nSpheres);

            pSpheres.IsDivision = true;
            nSpheres.IsDivision = true;

            return nSpheres;
        }
        private static DataSpheres emulate(this DataSpheres nSpheres, DataSpheres fromDataSpheres, string SpheresName = null)
        {
            DataSpheres pSpheres = fromDataSpheres;
            if (nSpheres.Config.DepotId.IsEmpty)
            {
                string SpheresId = (SpheresName != null) ? SpheresName : pSpheres.SpheresId;
                nSpheres.SpheresId = SpheresId;
                nSpheres.Parameters.Registry.AddRange(new Dictionary<string, object>(pSpheres.Parameters.Registry));
            }
            foreach (KeyValuePair<string, DataSphere> item in pSpheres.Spheres)
            {
                DataSphere nSphere = null;
                if (!nSpheres.Spheres.ContainsKey(item.Value.SphereId))
                {
                    nSphere = new DataSphere(item.Value.SphereId);
                    nSpheres.Add(item.Key, nSphere);
                    nSpheres.State.Added = true;
                }
                else
                    nSphere = nSpheres.Spheres[item.Value.SphereId];

                nSphere.emulate(item.Value);               
                
            }
            if (pSpheres.SpheresIn != null)
            {
                if (nSpheres.SpheresIn == null)
                    nSpheres.SpheresIn = new DataSpheres(pSpheres.SpheresIn.SpheresId);
                nSpheres.SpheresIn.emulate(pSpheres.SpheresIn);
            }
            return nSpheres;
        }

        public static  DataSpheres Imitate(this DataSpheres nSpheres, DataSpheres fromDataSpheres, string SpheresName = null, DataSpheres cubeRelaySpheres = null)
        {
            DataSpheres pSpheres = fromDataSpheres;
            if (nSpheres.Config.DepotId.IsEmpty)
            {
                string SpheresId = (SpheresName != null) ? SpheresName : pSpheres.SpheresId;
                nSpheres.SpheresId = SpheresId;
                nSpheres.Parameters.Registry.AddRange(new Dictionary<string, object>(pSpheres.Parameters.Registry));
            }
            foreach (KeyValuePair<string, DataSphere> item in pSpheres.Spheres)
            {
                DataSphere nSphere = null;
                if (!nSpheres.Spheres.ContainsKey(item.Value.SphereId))
                {
                    nSphere = new DataSphere(item.Value.SphereId);
                    nSpheres.Add(item.Key, nSphere);
                }
                else
                    nSphere = nSpheres.Spheres[item.Value.SphereId];

                nSphere.imitate(item.Value);

            }
            if (pSpheres.SpheresIn != null)
            {
                if (nSpheres.SpheresIn == null)
                    nSpheres.SpheresIn = new DataSpheres(pSpheres.SpheresIn.SpheresId);
                nSpheres.SpheresIn.imitate(pSpheres.SpheresIn);
            }

            nSpheres.EmulateRelays(fromDataSpheres, nSpheres);
            if (cubeRelaySpheres != null)
            {
                nSpheres.Devisor = cubeRelaySpheres;
                nSpheres.EmulateCubes(cubeRelaySpheres);
            }
            else
                nSpheres.EmulateCubes(nSpheres);

            pSpheres.IsDivision = true;
            nSpheres.IsDivision = true;

            return nSpheres;
        }
        private static DataSpheres imitate(this DataSpheres nSpheres, DataSpheres fromDataSpheres, string SpheresName = null)
        {
            DataSpheres pSpheres = fromDataSpheres;
            if (nSpheres.Config.DepotId.IsEmpty)
            {
                string SpheresId = (SpheresName != null) ? SpheresName : pSpheres.SpheresId;
                nSpheres.SpheresId = SpheresId;
                nSpheres.Parameters.Registry.AddRange(new Dictionary<string, object>(pSpheres.Parameters.Registry));
            }
            foreach (KeyValuePair<string, DataSphere> item in pSpheres.Spheres)
            {
                DataSphere nSphere = null;
                if (!nSpheres.Spheres.ContainsKey(item.Value.SphereId))
                {
                    nSphere = new DataSphere(item.Value.SphereId);
                    nSpheres.Add(item.Key, nSphere);
                    nSpheres.State.Added = true;
                }
                else
                    nSphere = nSpheres.Spheres[item.Value.SphereId];

                nSphere.imitate(item.Value);

            }
            if (pSpheres.SpheresIn != null)
            {
                if (nSpheres.SpheresIn == null)
                    nSpheres.SpheresIn = new DataSpheres(pSpheres.SpheresIn.SpheresId);
                nSpheres.SpheresIn.imitate(pSpheres.SpheresIn);
            }
            return nSpheres;
        }

        public static DataSpheres Impact(this DataSpheres nSpheres, DataSpheres pSpheres, bool toDrive = false, string SpheresName = null)
        {
            if (nSpheres.Config.DepotId.IsEmpty)
            {
                string SpheresId = (SpheresName != null) ? SpheresName : pSpheres.SpheresId;
                nSpheres.SpheresId = SpheresId;
                nSpheres.Config = pSpheres.Config;
                nSpheres.Parameters.Registry.AddRange(new Dictionary<string, object>(pSpheres.Parameters.Registry));
            }

            if (pSpheres.IsDivision)
                nSpheres.IsDivision = pSpheres.IsDivision;

            foreach (KeyValuePair<string, DataSphere> item in pSpheres.Spheres)
            {
                DataSphere nSphere = null;
                if (!nSpheres.Spheres.ContainsKey(item.Value.SphereId))
                {
                    nSphere = new DataSphere(item.Value.SphereId);
                    nSpheres.Add(item.Key, nSphere);
                }
                else
                    nSphere = nSpheres.Spheres[item.Value.SphereId];

                if (item.Value.IsDivision)
                    nSphere.Impact(item.Value);
                else
                    nSphere.impact(item.Value);

            }
            if (pSpheres.SpheresIn != null)
            {
                if (nSpheres.SpheresIn == null)
                    nSpheres.SpheresIn = new DataSpheres(pSpheres.SpheresIn.SpheresId);

                if (pSpheres.SpheresIn.IsDivision)
                    nSpheres.SpheresIn.Impact(pSpheres.SpheresIn);
                else
                    nSpheres.SpheresIn.impact(pSpheres.SpheresIn);
            }

            if (pSpheres.Devisor != null)
            {
                nSpheres.Devisor = pSpheres.Devisor.Locate();
                nSpheres.EmulateRelays(pSpheres, nSpheres);
                nSpheres.EmulateCubes(nSpheres.Devisor);
            }
            else
            {
                nSpheres.EmulateRelays(pSpheres, nSpheres);
                nSpheres.EmulateCubes(nSpheres);          
            }

            if (toDrive)
                nSpheres.WriteDrive();

            pSpheres = null;
            return nSpheres;
        }
        private static DataSpheres impact(this DataSpheres nSpheres, DataSpheres pSpheres, bool toDrive = false, string SpheresName = null)
        {
            if (nSpheres.Config.DepotId.IsEmpty)
            {
                string SpheresId = (SpheresName != null) ? SpheresName : pSpheres.SpheresId;
                nSpheres.SpheresId = SpheresId;
                nSpheres.Config = pSpheres.Config;
                nSpheres.IsDivision = pSpheres.IsDivision;
                nSpheres.Parameters.Registry.AddRange(new Dictionary<string, object>(pSpheres.Parameters.Registry));
            }
            foreach (KeyValuePair<string, DataSphere> item in pSpheres.Spheres)
            {
                DataSphere nSphere = null;
                if (!nSpheres.Spheres.ContainsKey(item.Value.SphereId))
                {
                    nSphere = new DataSphere(item.Value.SphereId);
                    nSpheres.Add(item.Key, nSphere);
                }
                else
                    nSphere = nSpheres.Spheres[item.Value.SphereId];

                if (item.Value.IsDivision)
                    nSphere.Impact(item.Value);
                else
                    nSphere.impact(item.Value);

            }
            if (pSpheres.SpheresIn != null)
            {
                if (nSpheres.SpheresIn == null)
                    nSpheres.SpheresIn = new DataSpheres(pSpheres.SpheresIn.SpheresId);

                if (pSpheres.SpheresIn.IsDivision)
                    nSpheres.SpheresIn.Impact(pSpheres.SpheresIn);
                else
                    nSpheres.SpheresIn.impact(pSpheres.SpheresIn);
            }

            if (toDrive)
                nSpheres.WriteDrive();

            pSpheres = null;
            return nSpheres;
        }

        public static DataSpheres  Locate(this DataSpheres nSpheres, string path = null)
        {
            string _path = (path == null) ? nSpheres.Config.Place : path;
            string[] items = _path.Split('/');
            int length = items.Length;
            return (length > 1)? DataSpace.Area[items[1]] : null;
        }
        public static ArrayList    Lookup(this DataSpheres nSpheres, string objectname)
        {
            ArrayList result = new ArrayList();

            foreach (DataSphere sph in nSpheres.Spheres.Values)
            {
                ArrayList subresult = sph.Lookup(objectname);
                if (subresult.Count > 0)
                    result.AddRange(subresult);
            }

            return result;
        }

        public static DataArea  Emulate(this DataArea nArea, DataArea fromDataArea, string _SpaceName = null)
        {
            DataArea pArea = fromDataArea;
            string SpaceName = (_SpaceName != null) ? _SpaceName : pArea.SpaceName;
            nArea.SpaceName = SpaceName;
            nArea.Config = pArea.Config;
            nArea.Parameters.Registry.AddRange(new Dictionary<string, object>(pArea.Parameters.Registry));
            foreach (KeyValuePair<string, DataSpheres> item in pArea.Areas)
            {
                DataSpheres nSpheres = new DataSpheres(item.Value.SpheresId).Emulate(item.Value);             
                nArea.Add(item.Key, nSpheres);
            }      
            return nArea;
        }
        public static DataArea  Imitate(this DataArea nArea, DataArea fromDataArea, string _SpaceName = null)
        {
            DataArea pArea = fromDataArea;
            string SpaceName = (_SpaceName != null) ? _SpaceName : pArea.SpaceName;
            nArea.SpaceName = SpaceName;
            nArea.Config = pArea.Config;
            nArea.Parameters.Registry.AddRange(new Dictionary<string, object>(pArea.Parameters.Registry));
            foreach (KeyValuePair<string, DataSpheres> item in pArea.Areas)
            {
                DataSpheres nSpheres = new DataSpheres(item.Value.SpheresId).Imitate(item.Value);
                nArea.Add(item.Key, nSpheres);
            }
            return nArea;
        }
        public static DataArea  Impact(this DataArea nArea, DataArea pArea, bool toDrive = false, string _SpaceName = null)
        {
            if (nArea.Config.DepotId.IsEmpty)
            {
                string SpaceName = (_SpaceName != null && _SpaceName != string.Empty) ? _SpaceName :
                                   (nArea.SpaceName != null && nArea.SpaceName != string.Empty) ? nArea.SpaceName :
                                    pArea.SpaceName;
                nArea.SpaceName = SpaceName;
                DataSpace.SpaceName = nArea.SpaceName;
                nArea.Config.DepotId = pArea.Config.DepotId;
                nArea.Parameters.Registry.AddRange(new Dictionary<string, object>(pArea.Parameters.Registry));
            }

            foreach (KeyValuePair<string, DataSpheres> item in pArea.Areas)
            {
                DataSpheres nSpheres = (DataSpace.Area.Have(item.Key)) ? DataSpace.Area[item.Key] : new DataSpheres(item.Value.SpheresId);
                nArea.Add(item.Key, nSpheres);

                if (item.Value.IsDivision)
                {
                    nSpheres.IsDivision = true;
                    nSpheres.Impact(item.Value);
                }
                else
                    nSpheres.impact(item.Value);
            }

            if (toDrive)
                nArea.WriteDrive();

            pArea = null;
            return nArea;
        }
        public static DataArea  Locate(this DataArea nArea, string _path = null)
        {
            string path = (_path == null) ? nArea.Config.Place : _path;
            string[] items = path.Split('/');
            if (DataSpace.Area.SpaceName == items[0])
                return DataSpace.Area;
            else
                return null;
        }
        public static ArrayList Lookup(this DataArea nArea, string objectname)
        {
            ArrayList result = new ArrayList();

            foreach (DataSpheres sphs in nArea.Areas.Values)
            {
                ArrayList subresult = sphs.Lookup(objectname);
                if (subresult.Count > 0)
                    result.AddRange(subresult);
            }

            return result;
        }

        public static DataVaults Impact(this DataVaults nVaults, DataVaults pVaults, bool toDrive = false, string _BankName = null)
        {
            if (nVaults.Config.DepotId.IsEmpty)
            {
                string BankName = (_BankName != null && _BankName != string.Empty) ? _BankName :
                                  (nVaults.BankName != null && nVaults.BankName != string.Empty) ? nVaults.BankName :
                                   pVaults.BankName;
                nVaults.BankName = BankName;
                nVaults.Config = pVaults.Config;
                nVaults.Config.DepotId = pVaults.Config.DepotId;
                nVaults.Parameters.Registry.AddRange(new Dictionary<string, object>(pVaults.Parameters.Registry));             
            }

            foreach (DataVault fromVault in pVaults.Vaults.Values)
            {
                DataVault emuVault = fromVault;
                if (!nVaults.Have(emuVault.VaultId))
                    nVaults.Create(emuVault.VaultId);
                DataVault vault = nVaults[emuVault.VaultId];
                vault.Impact(emuVault);

                if (emuVault.VaultsIn != null)
                {
                    if (vault.VaultsIn != null)
                        vault.VaultsIn.Impact(emuVault.VaultsIn);
                    else
                        vault.VaultsIn = new DataVaults(emuVault.VaultsIn.BankName, false).Impact(emuVault.VaultsIn);
                    vault.VaultsIn.VaultsUp = nVaults;
                }

                nVaults.Add(emuVault.VaultId, vault);              
            }

            if (toDrive)
                nVaults.WriteDrive();

            pVaults = null;
            return nVaults;
        }
        public static DataVaults Locate(this DataVaults nVaults, string _path = null)
        {
            string path = (_path == null) ? nVaults.Config.Place : _path;
            string[] items = path.Split('/');
            if (DataBank.Vault.BankName == items[0])
                return DataBank.Vault;
            else
                return null;
        }

        public static DataVault Impact(this DataVault nVault, DataVault pVault, string vaultId = null)
        {
            if (nVault.Config.DepotId.IsEmpty)
            {
                // Exception in this place - delete QDFS directory
                string VaultId = (vaultId != null) ? vaultId : pVault.VaultId;
                nVault.VaultId = VaultId;
                nVault.Config = pVault.Config;
                nVault.Parameters.Registry.AddRange(new Dictionary<string, object>(pVault.Parameters.Registry));
            }

            foreach (DataDeposit fromDeposit in pVault.Deposit.Values)
            {
                DataDeposit emuDeposit = fromDeposit;
                if (!nVault.Have(emuDeposit.DepositId))
                    nVault.NewDeposit(emuDeposit.DepositId);
                DataDeposit deposit = nVault[emuDeposit.DepositId];
                deposit.Impact(emuDeposit);
            }

            pVault = null;
            return nVault;
        }
        public static DataVault Locate(this DataVault nVault, string _path = null)
        {
            string path = (_path == null) ? nVault.Config.Place : _path;
            string[] items = path.Split('/');
            int length = items.Length;
            DataVault vlt = DataBank.Vault[items[1]];
            DataVault vlts = vlt;
            if (length > 2 && vlt != null)
            {
                for (int i = 3; i < length; i += 2)
                {
                    DataVault tempvlts = null;
                    if (vlts.VaultsIn != null)
                    {
                        if (vlts.VaultsIn.BankName.Equals(items[i - 1]))
                            vlts.VaultsIn.TryGetValue(items[i], out tempvlts);
                    }
                    if (tempvlts != null)
                        vlts = tempvlts;
                }
            }
            return vlts;
        }

        public static DataDeposit Impact(this DataDeposit _deposit, DataDeposit _emuDeposit, bool toDrive = false, DataGroup Group = DataGroup.None)
        {
            if (_deposit.Config.DepotId.IsEmpty)
            {
                _deposit.Config = _emuDeposit.Config;
                if(_emuDeposit.SqlQuery != null)
                    if (_emuDeposit.SqlQuery != string.Empty)
                        _deposit.SqlQuery = _emuDeposit.SqlQuery;
                _deposit.Parameters.Registry.AddRange(new Dictionary<string, object>(_emuDeposit.Parameters.Registry));
            }         

            if (_emuDeposit.Box.Prime != null)
            {
                DataTrellis emuTrell = _emuDeposit.Box.Prime;
                DataTrellis trell = null;

                if (_deposit.Box.Prime == null)
                    trell = new DataTrellis(_emuDeposit.Box.Prime.TrellName, true);
                else
                    trell = _deposit.Box.Prime;

                trell.Impact(emuTrell);

                _deposit.Box.Prime = trell;
            }

            if (toDrive)
                _deposit.WriteDrive();

            _emuDeposit = null;
            return _deposit;
        }
        public static DataDeposit Locate(this DataDeposit _deposit, string _path = null)
        {
            object result = null;
            DataBank.Registry.TryGetValue(_deposit.Config.DataId, out result);
            DataDeposit dpst = null;
            if (result != null)
                dpst = (DataDeposit)result;
            else
            {
                string path = (_path == null) ? _deposit.Config.Place : _path;
                string[] items = path.Split('/');
                int length = items.Length;
                string dpstName = items[length - 1].Split('.')[0];
                DataVault vlt = null;
               
                if (_deposit.Vault != null)
                {
                    vlt = (DataVault)_deposit.Vault.Locator();
                }
                else
                {
                    vlt = new DataVault("TEMP");
                    string vltpath = items.Take(length - 1).Aggregate((a, b) => a + "/" + b);
                    vlt = (DataVault)vlt.Locator(vltpath);
                }
                vlt.TryGetValue(dpstName, out dpst);
            }
            return dpst;
        }

        public static DataPylons Emulate(this DataPylons nPyls, DataPylons pPyls)
        {           
            foreach (DataPylon pPyl in pPyls)
            {
                if (!nPyls.Have(pPyl.PylonName))
                {
                    DataPylon nPyl = pPyl.Clone();
                    nPyl.Pylons = nPyls;
                    if ((!ReferenceEquals(nPyl.MattabFormula, null) &&
                        (!ReferenceEquals(pPyl.RightFormula, null))))
                    {
                        nPyl.NewMattab();
                        nPyl.RightFormula = pPyl.RightFormula;
                        nPyls.newFormula = true;
                    }
                    nPyls.Add(pPyl);
                }
            }
            return nPyls;
        }
        public static DataPylons Impact(this DataPylons nPyls, DataPylons pPyls)
        {
           
            foreach (DataPylon pPyl in pPyls)
            {
                if (!nPyls.Have(pPyl.PylonName))
                {
                    DataPylon nPyl = pPyl.Clone();
                    nPyl.Pylons = nPyls;
                    if ((!ReferenceEquals(nPyl.MattabFormula, null) && 
                        (!ReferenceEquals(pPyl.RightFormula, null))))
                    {
                        nPyl.NewMattab();
                        nPyl.RightFormula = pPyl.RightFormula;
                        nPyls.newFormula = true;
                    }
                    nPyls.Add(pPyl);
                }
                else
                {
                    if (!pPyl.isNoid)
                    {
                        DataPylon nPyl = nPyls.GetPylon(pPyl.PylonName);
                        foreach (PropertyInfo pi in DataPylonClonable.ClonableInfo)
                        {
                            object pval = pi.GetValue(pPyl);
                            object nval = pi.GetValue(nPyl);
                            if (pval != null)
                            {
                                if (!pval.Equals(nval))
                                {
                                    if (pval is Enum)
                                    {
                                        if (pval.ToString() != "None")
                                            pi.SetValue(nPyl, pval);
                                    }
                                    else
                                    {
                                        pi.SetValue(nPyl, pval);
                                    }
                                }
                            }
                        }
                    }
                }
            }
                      
            return nPyls;
        }

        public static void Impact(this DataPageDetails c, DataPageDetails pc)
        {
            c.Page = pc.Page;
            c.PageActive = pc.PageActive;
            c.PageSize = pc.PageSize;
            c.CachedPages = (pc.CachedPages > 0) ? pc.CachedPages : c.CachedPages;
        }
        public static void Impact(this DataConfig c, DataConfig pc, bool propagate = true)
        {
            c.DepotIdx = pc.DepotIdx;
            //c.AuthIdx = pc.AuthIdx;
            c.Path = pc.Path;
            c.Place = pc.Place;
    }

        public static void SaveTrells(this DataSpheres dss)
        {
            foreach (DataSphere ds in dss.Values)
            {
                foreach (DataTrellis trl in ds.Trells)
                    if (trl.Tiers.Count > 0)
                        if (trl.IsPrime)
                            trl.Tiers.WriteDrive();
                        else
                            trl.Tiers.Registry.WriteDrive();

                if (ds.SpheresIn != null)
                    SaveTrells(ds.SpheresIn);
            }
        }
    }

    [Serializable]
    public class AfectMapping
    {
        public AfectMapping(string dbTableName)
        {
            KeyOrdinal = new HashSet<int>();
            ColumnOrdinal = new HashSet<int>();
            DbTableName = dbTableName;
        }
        public AfectMapping(string dbTableName, HashSet<int> keyOrdinal, HashSet<int> columnOrdinal)
        {
            KeyOrdinal = keyOrdinal;
            ColumnOrdinal = columnOrdinal;
            DbTableName = dbTableName;
        }

        public string DbTableName { get; set; }

        public HashSet<int> KeyOrdinal { get; set; }
        public HashSet<int> ColumnOrdinal { get; set; }
    }


}