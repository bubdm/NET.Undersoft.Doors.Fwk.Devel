using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Doors;

namespace System.Doors.Data
{   
    [Serializable]
    public class DataCube
    {
        #region Private NonSerialized
        [NonSerialized] private DataTrellis cubeTrell;
        [NonSerialized] private DataTrellis subTrell;
        [NonSerialized] private DataSubCube subCube;
        [NonSerialized] public JoinCubeRelay[] JoinCubeRelays;
        #endregion

        public DataRelays   CubeRelays
        {
            get;
            set;
        }

        public DataTrellis  CubeTrell
        { get { return cubeTrell; } set { cubeTrell = value; } }
        public DataTiers    CubeTiers
        { get { return CubeTrell.Tiers; } }
        public DataPylons   CubePylons
        {
            get
            {               
                return CubeTrell.Pylons;
            }
        }

        public DataTrellis   SubTrell
        {
            get
            {
                if (subTrell == null)                
                    if (CubeRelays.Count > 0)
                    {
                        DataTrellis[] rootTrells = CubeRelays.AsEnumerable()
                             .GroupBy(pt => pt.Parent.Trell)
                                 .Select(t => t.Key)
                                     .Where(r => !CubeRelays.AsEnumerable()
                                         .Where(cr => ReferenceEquals(r, cr.Child.Trell)).Any()).ToArray();

                        if (rootTrells.Length > 0)
                            subTrell = !rootTrells[0].IsCube ? rootTrells[0] : rootTrells[0].Cube.SubTrell;
                    }                
                return subTrell;
            }
            set
            {
                subTrell = value;
            }
        }
        public DataTiers     SubTiers
        {
            get
            {
                return SubTrell.Tiers;
            }
        }

        public int[] CubeSinglePylonIds;
        public int[] CubeRootPylonIds;
        public DataPylon[] CubeJoinPylonIds;


        public int Level = 0;      
        public bool FindRelays = false;

        public DataCube()   
        {
            CubeRelays = new DataRelays();
        }
        public DataCube(DataTrellis cubetrell)
        {
            CubeTrell = cubetrell;
            CubeRelays = new DataRelays();
        }
        public DataCube(DataTrellis cubetrell, DataRelays cuberelays)
        {
            CubeTrell = cubetrell;
            CubeRelays = cuberelays;
        }                         

        public DataSubCube SubCube
        { get { if (subCube == null) CreateCube(); return subCube; }  set { subCube = value; } }

        public void CreateCube(string[] pylonNames = null)
        {
            CreateCube(CubeTrell, CubeRelays, pylonNames);


        }
        public void CreateCube(DataRelays cuberelays, string[] pylonNames = null)
        {
            CreateCube(CubeTrell, cuberelays, pylonNames);
        }
        public void CreateCube(DataTrellis cubetrell, DataRelays cuberelays, string[] pylonNames = null)
        {
            if (!ReferenceEquals(cubetrell, CubeTrell))
                CubeTrell = cubetrell;
            if (!ReferenceEquals(cuberelays, CubeRelays))
                CubeRelays = cuberelays;
            if (SubTrell != null && CubeRelays != null)
            {
                if (pylonNames != null)
                    CreateCubePylons(pylonNames);

                CubePylons.Select(cp => cp.CubeLevel = -1).ToArray();
                AssignSubPylons(SubTrell, new[] { -1 }, Level, -1);
                subCube = new DataSubCube(SubTrell, this, new int[] { -1 }, Level + 1);
            }
            AssignSinglePylons();
            CubeSinglePylonIds = CubePylons.Where(cp => cp.CubeLevel < 0).Select(p => p.Ordinal).ToArray();
            CubeRootPylonIds = CubePylons.Where(cp => cp.CubeLevel == 0).Select(p => p.Ordinal).ToArray();
            CubeJoinPylonIds = CubePylons.Where(cp => cp.CubeLevel > 0).ToArray();
            if(subCube != null)
                JoinCubeRelays = GetCubeRelayIds(subCube);
        }

        public JoinCubeRelay[] GetCubeRelayIds(DataSubCube subcube)
        {
            JoinCubeRelay[] joinCubeRelays = new JoinCubeRelay[subcube.SubCubeRelays.Count];
            int length = joinCubeRelays.Length;
            for (int i = 0; i < length; i++)
            {
                joinCubeRelays[i] = GetJoinCubeRelay(subcube, SubTiers, subcube.SubCubeRelays[i], CubeTiers.Mode);
            }
            return joinCubeRelays;
        }

        public JoinCubeRelay GetJoinCubeRelay(DataSubCube cube, DataTiers subtiers, DataSubCubeRelay cuberelay, DataModes mode)
        {
            DataTiers childtiers = null;
            if (mode == DataModes.SimsView || mode == DataModes.Sims)
            {
                cuberelay.CubeRelay.Child.Trell.Mode = DataModes.Sims;
                if (cuberelay.CubeRelay.Child.Trell.SimsView.Count == 0)
                    cuberelay.CubeRelay.Child.Trell.Sims.Query();
                childtiers = cuberelay.CubeRelay.Child.Trell.SimsView;
            }
            else
            {
                cuberelay.CubeRelay.Child.Trell.Mode = DataModes.Tiers;
                if (cuberelay.CubeRelay.Child.Trell.TiersView.Count == 0)
                    cuberelay.CubeRelay.Child.Trell.Tiers.Query();
                childtiers = cuberelay.CubeRelay.Child.Trell.TiersView;
            }

            JoinCubeRelay jcr = new JoinCubeRelay();
            jcr.CubeRelayIndex = cuberelay.CubeIndex;
            jcr.subTiers = subtiers;
            jcr.childTiers = childtiers;
            jcr.parentKeyOrdinal = cuberelay.CubeRelay.Parent.KeysOrdinal;

            if (cuberelay.SubCube != null)
            {
                jcr.subCubeRelays = cuberelay.SubCube.SubCubeRelays.Select(r =>
                                    GetJoinCubeRelay(cuberelay.SubCube,
                                                childtiers,
                                                r,
                                                mode)).ToArray();
            }

            return jcr;
        }

        public void CreateCubePylons(string[] pylonNames)
        {
            foreach (string pylonName in pylonNames.Where(p => !CubePylons.Have(p)))
            {
                DataPylon pyl = SubTrell.Pylons.GetPylon(pylonName);
                if (pyl == null)
                {
                    DataPylon[] pyls = CubeRelays.Select(c => c.Child.Trell.Pylons.GetPylon(pylonName)).Where(p => p != null).ToArray();
                    if (pyls.Length > 0)
                        pyl = pyls[0];
                }
                
                if(pyl != null)
                {
                    DataPylon _pyl = pyl.Clone();
                    _pyl.Ordinal = -1;
                    _pyl.isCube = true;
                    CubePylons.Add(_pyl);
                }
            }
        }

        public bool AssignSubPylons(DataTrellis trell, int[] cubeindex, int cubelevel, int inheritid)
        {
            DataPylon[] cubepylons = CubePylons.AsEnumerable()
                                            .Where(cp => (trell.Pylons.Have(cp.PylonName) ||
                                            ((cp.JoinPattern != null) ? 
                                            trell.Pylons.Have(cp.JoinPattern.PylonName) : 
                                            false)) && cp.CubeLevel < 0).ToArray();
            if (cubepylons.Length > 0)
            {
                foreach (DataPylon cubepylon in cubepylons)
                {
                    DataPylon subpylon = trell.Pylons[cubepylon.PylonName];
                    if (subpylon == null)
                        subpylon = trell.Pylons[cubepylon.JoinPattern.PylonName];
                    cubepylon.CubeOrdinal = subpylon.Ordinal;
                    cubepylon.CubeIndex = cubeindex;
                    cubepylon.CubeLevel = cubelevel;
                    cubepylon.InheritorId = inheritid;
                    cubepylon.DataType = subpylon.DataType;                    
                }
                return true;
            }        
            return false;
        }

        public bool AssignSinglePylons()
        {
            DataPylon[] cubepylons = CubePylons.AsEnumerable()
                                            .Where(cp => cp.CubeLevel < 0).ToArray();
            if (cubepylons.Length > 0)
            {
                foreach (DataPylon cubepylon in cubepylons)
                    cubepylon.CubeOrdinal = cubepylon.Ordinal;
                return true;
            }
            return false;
        }

        public void AssignSubTiers()
        {
            if (SubTrell != null)
            {
                try
                {
                    //PrepareJoinTiers(SubCube);
                    //CubeTiers.Relaying.GetJoinCubes();

                    foreach (DataTier subtier in SubTiers)
                    {
                        DataTier cubetier = new DataTier(CubeTrell, SubTrell, subtier);
                        CubeTiers.TryAddInner(cubetier);
                    }
                    CubeTiers.Registry.WriteDrive();
                }
                catch (Exception ex)
                {

                }
            }
        }

        public JoinChildList[] PrepareJoinTiers(DataSubCube subCube)
        {
            subCube.SubCubeRelays.Select(r => r.SubCube != null ? PrepareJoinTiers(r.SubCube) : null).ToList();
            return subCube.SubTrell.Tiers.GetChildList(subCube.SubCubeRelays.Select(r => r.CubeRelay).ToList());
        }

        public DataTier AssignSubTier(DataTier SubTier, bool save = false)
        {
            DataTier cubetier = new DataTier(CubeTrell, SubTrell, SubTier);
            if (CubeTiers.TryAddInner(cubetier, save))
                return cubetier;
            return null;
        }

        public DataTier AddCubeTier(bool save = false)
        {
            DataTier subtier = SubTiers.AddNew();
            DataTier cubetier = new DataTier(CubeTrell, SubTrell, subtier);
            CubeTiers.TryAddInner(cubetier, save);
            return cubetier;
        }

        public void SyncCubeTiers()
        {
            int cubecount = CubeTiers.Count;
            int subcount = SubTiers.Count;
            if (cubecount != subcount)
            {
                CubeTiers.DeleteRange(CubeTiers.AsEnumerable().Where(s => s.SubTier.Deleted).ToList());
                cubecount = CubeTiers.Count;
                if (subcount > cubecount)
                {
                    for (int i = cubecount; i < subcount; i++)
                    {
                        DataTier cubetier = new DataTier(CubeTrell, SubTrell, SubTiers[i]);
                        CubeTiers.TryAddInner(cubetier);
                    }
                    CubeTiers.Registry.WriteDrive();
                }
            }
        }

        public void RebuildCubeTiers()
        {
            Clear();
            AssignSubTiers();
        }

        public void Clear()
        {
            CubeTiers.TiersView.ClearJoins();
            CubeTiers.TiersView.Clear();
            CubeTiers.ClearJoins();
            CubeTiers.Clear();
        }       
    }
    [Serializable]
    public class DataSubCube
    {
        public int Level = 0;
        public int[] CubeIndex;

        public DataSubCube()
        {
            SubCubeRelays = new List<DataSubCubeRelay>();
        }
        public DataSubCube(DataTrellis trell, DataCube cube, int[] cubeindex, int level)
        {
            Level = level;
            CubeIndex = cubeindex;
            SubCubeRelays = new List<DataSubCubeRelay>();
            BuildSubCubeRelays(trell, cube);
        }

        public DataTrellis  SubTrell
        { get; set; }
        public DataCube Cube
        { get; set; }

        public IList<DataSubCubeRelay> SubCubeRelays
        { get; set; }

        public void BuildSubCubeRelays(DataTrellis trell, DataCube cube)
        {
            SubTrell = trell;
            Cube = cube;
            SubCubeRelays.Clear();
            DataRelay[] relays = Cube.CubeRelays.AsEnumerable().Where(cr => ReferenceEquals(trell, cr.Parent.Trell)).ToArray();
            foreach (DataRelay relay in relays)
                AddSubCubeRelay(relay);
        }
        public void AddSubCubeRelay(DataRelay relay)
        {
            DataSubCubeRelay dcr = new DataSubCubeRelay(relay, this);
            SubCubeRelays.Add(dcr);
            int[] cubeIndex = CubeIndex.Concat(new int[] { dcr.CubeIndex }).ToArray();
            Cube.AssignSubPylons(relay.Child.Trell, cubeIndex, Level, relay.Child.Trell.Tiers.Registry.InheritId);
            DataRelay[] subrelays = Cube.CubeRelays.AsEnumerable().Where(cr => ReferenceEquals(cr.Parent.Trell, relay.Child.Trell)).ToArray();
            if (subrelays.Length > 0)
            {
                DataTrellis childtrell = subrelays[0].Parent.Trell;
                DataRelays childrelays = new DataRelays(subrelays);
                dcr.SubCube = new DataSubCube(childtrell, Cube, cubeIndex, Level + 1);
            }
            
        }
        public void RebuildSubCubeRelays()
        {
            if (SubTrell != null && Cube.CubeRelays != null)
            {
                SubCubeRelays.Clear();
                DataRelay[] relays = Cube.CubeRelays.AsEnumerable().Where(cr => ReferenceEquals(SubTrell, cr.Parent.Trell)).ToArray();
                foreach (DataRelay relay in relays)
                    AddSubCubeRelay(relay);
            }
        }
    }
    [Serializable]
    public class DataSubCubeRelay
    {
        public DataSubCube ParentSub;

        public DataSubCubeRelay()
        { }
        public DataSubCubeRelay(DataRelay cubeRelay, DataSubCube subparent)
        {
            CubeRelay = cubeRelay;
            ParentSub = subparent;
            //cubeindex = ParentSub.SubTrell.ChildRelays.IndexOf(CubeRelay);
        }

        public int CubeIndex
        {
            get
            {
                //if (cubeindex < 0 && CubeRelay != null)
                return ParentSub.SubTrell.ChildRelays.IndexOf(CubeRelay);
                //return cubeindex; 

                //return ParentSub.SubCubeRelays.IndexOf(this);
            }
        }

        public DataRelay   CubeRelay { get; set; }
        public DataSubCube SubCube { get; set; }
    }
    [Serializable]
    public class JoinCubeRelay
    {
        public int[] parentKeyOrdinal;
        public int CubeRelayIndex;
        public DataTiers subTiers;
        public DataTiers childTiers;
        public JoinCubeRelay[] subCubeRelays;
    }
}
