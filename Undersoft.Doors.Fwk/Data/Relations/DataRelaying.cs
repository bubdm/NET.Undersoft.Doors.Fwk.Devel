using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Doors;

namespace System.Doors.Data
{
    public interface IDataTreatment : IDataConfig
    {    
        JoinChildList[] GetChildList(IList<DataRelay> relays = null);
        JoinParentList[] GetParentList(IList<DataRelay> relays = null);

        void ClearJoins();

        DataTier[] AsArray();

        DataTrellis Trell { get; set; }
        DataTiers Self { get; }
        DataTiers Tiers { get; set; }
        DataTiers TiersView { get; }
        DataTiers TiersTotal { get; }
        DataModes Mode { get; set; }
    }

    [JsonObject]
    [Serializable]
    public class DataRelaying
    {
        #region Private NonSerialized
        [NonSerialized] private DataRelays childRelays;
        [NonSerialized] private DataRelays parentRelays;
        [NonSerialized] public JoinParentList[] joinParentList;
        [NonSerialized] public JoinChildList[] joinChildList;
        [NonSerialized] public JoinFullList[] joinFullList;
        //[NonSerialized] private JoinCubeList joinCubeList;
        [NonSerialized] private DataTiers Tiers;
        //[NonSerialized] private JoinCubeRelay[] joinCubeRelays;
        #endregion

        private DataTrellis Trell;
        private DataModes Mode;

        public DataRelaying()
        {
        }
        public DataRelaying(DataTrellis trell)
        {
            Trell = trell;
            childRelays = new DataRelays();
            parentRelays = new DataRelays();
        }
        public DataRelaying(DataTiers tiers, DataModes mode)
        {
            Tiers = tiers;
            Trell = tiers.Trell;
            Mode = mode;
            childRelays = new DataRelays();
            parentRelays = new DataRelays();
        }

        public DataRelays ChildRelays
        {
            get
            {
                if (Trell.Relays != null && Trell.Relays.Count != childRelays.Count + parentRelays.Count)
                {
                    childRelays.Clear();
                    childRelays.AddRange(Trell.Relays.AsEnumerable().Where(v => ReferenceEquals(v.Parent.Trell, Trell)).OrderBy(o => o.RelayName).ToList());
                    parentRelays.Clear();
                    parentRelays.AddRange(Trell.Relays.AsEnumerable().Where(v => ReferenceEquals(v.Child.Trell, Trell)).OrderBy(o => o.RelayName).ToList());
                    if (joinChildList == null || childRelays.Count < joinChildList.Length)
                    {
                        joinChildList = new JoinChildList[childRelays.Count];
                    }
                    else if(childRelays.Count > joinChildList.Length)
                    {
                        JoinChildList[] tempjoinChildList = new JoinChildList[childRelays.Count];
                        joinChildList.CopyTo(tempjoinChildList, 0);
                        joinChildList = tempjoinChildList;
                    }
                }
                return childRelays;
            }
            set
            {
                childRelays = value;
            }
        }
        public DataRelays ParentRelays
        {
            get
            {
                if (Trell.Relays != null && Trell.Relays.Count != childRelays.Count + parentRelays.Count)
                {
                    childRelays.Clear();
                    childRelays.AddRange(Trell.Relays.AsEnumerable().Where(v => ReferenceEquals(v.Parent.Trell, Trell)).OrderBy(o => o.RelayName).ToList());                  
                    parentRelays.Clear();
                    parentRelays.AddRange(Trell.Relays.AsEnumerable().Where(v => ReferenceEquals(v.Child.Trell, Trell)).OrderBy(o => o.RelayName).ToList());
                    if (joinParentList == null || parentRelays.Count < joinParentList.Length)
                    {
                        joinParentList = new JoinParentList[parentRelays.Count];
                    }
                    else if (parentRelays.Count > joinParentList.Length)
                    {
                        JoinParentList[] tempjoinParentList = new JoinParentList[parentRelays.Count];
                        joinParentList.CopyTo(tempjoinParentList, 0);
                        joinParentList = tempjoinParentList;
                    }
                }
                return parentRelays;
            }
            set
            {
                parentRelays = value;
            }
        }
        public DataRelays CubeRelays
        {
            get
            {               
                return Trell.Cube.CubeRelays;
            }
        }

        public JoinFullList[] GetJoinFulls(IList<DataRelay> relays)
        {
            int[] ids = null;
            if (relays != null)
                ids = relays.Select(r => ChildRelays.GetIdByName(r.RelayName)).ToArray();
            else
                ids = ChildRelays.Select((x, y) => y).ToArray();

            if (joinChildList == null || ChildRelays.Count != joinChildList.Length)
                joinFullList = new JoinFullList[ChildRelays.Count];

            ids.AsParallel().Select(x => joinFullList[x] = GetJoinFull(Tiers, ChildRelays[x], Mode)).ToArray();
            return joinFullList;
        }
        public JoinFullList[] GetJoinFulls()
        {
            return joinFullList;
        }

        public void SetChildJointParams()
        {
            if (joinChildList == null)
                joinChildList = new JoinChildList[ChildRelays.Count];
            else if (joinChildList.Length < ChildRelays.Count)
                Array.Resize(ref joinChildList, ChildRelays.Count);
        }

        public void SetParentJointParams()
        {
            if (joinChildList == null)
                joinChildList = new JoinChildList[ChildRelays.Count];
            else if (joinChildList.Length < ChildRelays.Count)
                Array.Resize(ref joinChildList, ChildRelays.Count);
        }

        public JoinChildList[] GetJoinChilds(IList<DataRelay> relays, int[] idsarray = null, bool setparams = true)
        {
            int[] ids = idsarray;
            if (ids == null)
                if (relays != null)
                    ids = relays.Select(r => ChildRelays.GetIdByName(r.RelayName)).ToArray();
                else
                    ids = ChildRelays.Select((x, y) => y).ToArray();

            if (setparams)
                SetChildJointParams();

            ids.AsParallel().Select(x => joinChildList[x] = GetJoinChild(Tiers, ChildRelays[x], Mode)).ToArray();
            return joinChildList;
        }
        public JoinChildList[] GetJoinChilds()
        {
            return joinChildList;
        }

        public JoinParentList[] GetJoinParents(IList<DataRelay> relays, int[] idsarray = null, bool setparams = true)
        {
            int[] ids = idsarray;
            if (ids == null)
                if (relays != null)
                    ids = relays.Select(r => ParentRelays.GetIdByName(r.RelayName)).ToArray();
                else
                    ids = ParentRelays.Select((x, y) => y).ToArray();

            if (setparams)
                SetParentJointParams();

            ids.AsParallel().Select(x => joinParentList[x] = GetJoinParent(Tiers, ParentRelays[x], Mode)).ToArray();
            return joinParentList;
        }
        public JoinParentList[] GetJoinParents()
        {
            return joinParentList;
        }

        //public JoinCubeList GetJoinCubes(DataTiers cubetiers, DataModes mode)
        //{
        //    DataCube qbe = cubetiers.Trell.Cube;
        //    DataTiers subtiers = new DataTiers(cubetiers.Trell, cubetiers.AsEnumerable().Select(s => s.SubTier).ToArray());

        //    if (joinCubeList == null || joinCubeList.List.Count != subtiers.Count)
        //    {
        //        joinCubeList = GetJoinCube(qbe.SubCube, qbe.SubTiers, mode);
        //    }
        //    return joinCubeList;
        //}
        //public JoinCubeList GetJoinCubes()
        //{
        //    DataCube qbe = Trell.Cube;

        //    if (joinCubeList == null || joinCubeList.List.Count != qbe.SubTiers.Count)
        //    {
        //        joinCubeList = GetJoinCube(qbe.SubCube, qbe.SubTiers, Mode);
        //    }
        //    return joinCubeList;
        //}

        //public JoinParentList GetJoinParent(DataTier[] tiers, DataRelay relay, DataModes mode)
        //{
        //    DataTiers ptiers = null;
            
        //    if (mode == DataModes.SimsView || mode == DataModes.Sims)
        //    {
        //        relay.Parent.Trell.Mode = DataModes.Sims;
        //        if (relay.Parent.Trell.SimsView.Count == 0)
        //            relay.Parent.Trell.Sims.Query();
        //        ptiers = relay.Parent.Trell.SimsView;
        //    }
        //    else
        //    {
        //        relay.Parent.Trell.Mode = DataModes.Tiers;
        //        if (relay.Parent.Trell.TiersView.Count == 0)
        //            relay.Parent.Trell.Tiers.Query();
        //        ptiers = relay.Parent.Trell.TiersView;
        //    }

        //    int[] parentKeyOrdinal = relay.Parent.KeysOrdinal;
        //    int[] childKeyOrdinal = relay.Child.KeysOrdinal;
        //    if (tiers.Length > 0)
        //    {
        //        //if (!tiers[0].IsCube)
        //        //    return new JoinParentList(tiers.GroupJoin(ptiers.AsEnumerable(),
        //        //                        ch => GetShahKey32(ch, ref childKeyOrdinal),
        //        //                        pr => GetShahKey32(pr, ref parentKeyOrdinal),
        //        //                        (ch, pr) => new JoinParent { Parent = pr.ToList(), Child = ch }, new HashComparer()).ToList());
        //        //else
        //        //    return new JoinParentList(tiers.GroupJoin(ptiers.AsEnumerable(),
        //        //                      ch => GetCubeShahKey32(ch, ref childKeyOrdinal),
        //        //                      pr => GetCubeShahKey32(pr, ref parentKeyOrdinal),
        //        //                      (ch, pr) => new JoinParent { Parent = pr.ToList(), Child = ch }, new HashComparer()).ToList());
        //        DataNoid registry = tiers[0].Tiers.Registry;
        //        if (!tiers[0].IsCube)
        //            return new JoinParentList(ptiers.AsEnumerable().Select(pr =>
        //                                        new JoinParent
        //                                        {
        //                                            Child = pr,
        //                                            Parent = (DataTier[])registry.Keymap[GetShahKey64(pr, childKeyOrdinal)]
        //                                        }).ToList());
        //        else
        //            return new JoinParentList(ptiers.AsEnumerable().Select(pr =>
        //                                       new JoinParent
        //                                       {
        //                                           Child = pr,
        //                                           Parent = (DataTier[])registry.Keymap[GetCubeShahKey64(pr, childKeyOrdinal)]
        //                                       }).ToList());
        //    }
        //    else
        //        return null;
        //}
        //public JoinChildList GetJoinChild(DataTier[] tiers, DataRelay relay, DataModes mode)
        //{
        //    DataTiers ptiers = null;
        //    if (mode == DataModes.SimsView || mode == DataModes.Sims)
        //    {
        //        relay.Child.Trell.Mode = DataModes.Sims;
        //        if (relay.Child.Trell.SimsView.Count == 0)
        //            relay.Child.Trell.Sims.Query();
        //        ptiers = relay.Child.Trell.SimsView;
        //    }
        //    else
        //    {
        //        relay.Child.Trell.Mode = DataModes.Tiers;
        //        if (relay.Child.Trell.TiersView.Count == 0)
        //            relay.Child.Trell.Tiers.Query();
        //        ptiers = relay.Child.Trell.TiersView;
        //    }

        //    int[] parentKeyOrdinal = relay.Parent.KeysOrdinal;
        //    //int[] childKeyOrdinal = relay.Child.KeysOrdinal;
        //    if (tiers.Length > 0)
        //        //if (!tiers[0].IsCube)
        //        //    return new JoinChildList(tiers.GroupJoin(ptiers.AsEnumerable(),
        //        //                            pr => GetShahKey32(pr, ref parentKeyOrdinal),
        //        //                            ch => GetShahKey32(ch, ref childKeyOrdinal),
        //        //                            (pr, ch) => new JoinChild { Parent = pr, Child = ch.ToList() }, new HashComparer()).ToList());
        //        //else
        //        //    return new JoinChildList(tiers.GroupJoin(ptiers.AsEnumerable(),
        //        //                        pr => GetCubeShahKey32(pr, ref parentKeyOrdinal),
        //        //                        ch => GetCubeShahKey32(ch, ref childKeyOrdinal),
        //        //                        (pr, ch) => new JoinChild { Parent = pr, Child = ch.ToList() }, new HashComparer()).ToList());
        //    if (!tiers[0].IsCube)
        //        return new JoinChildList(tiers.AsEnumerable().Select(pr =>
        //                                    new JoinChild
        //                                    {
        //                                        Parent = pr,
        //                                        Child = (DataTier[])ptiers.Registry.Keymap[GetShahKey64(pr, parentKeyOrdinal)]
        //                                    }).ToList());
        //    else
        //        return new JoinChildList(tiers.AsEnumerable().Select(pr =>
        //                                   new JoinChild
        //                                   {
        //                                       Parent = pr,
        //                                       Child = (DataTier[])ptiers.Registry.Keymap[GetCubeShahKey64(pr, parentKeyOrdinal)]
        //                                   }).ToList());
        //    else
        //        return null;
        //}
        //public JoinFullList GetJoinFull(DataTier[] tiers, DataRelay relay, DataModes mode)
        //{
        //    DataTiers ptiers = null;
        //    if (mode == DataModes.SimsView || mode == DataModes.Sims)
        //    {
        //        relay.Child.Trell.Mode = DataModes.Sims;
        //        if (relay.Child.Trell.SimsView.Count == 0)
        //            relay.Child.Trell.Sims.Query();
        //        ptiers = relay.Child.Trell.SimsView;
        //    }
        //    else
        //    {
        //        relay.Child.Trell.Mode = DataModes.Tiers;
        //        if (relay.Child.Trell.TiersView.Count == 0)
        //            relay.Child.Trell.Tiers.Query();
        //        ptiers = relay.Child.Trell.TiersView;
        //    }

        //    int[] parentKeyOrdinal = relay.Parent.KeysOrdinal;
        //    int[] childKeyOrdinal = relay.Child.KeysOrdinal;
        //    if (tiers.Length > 0)
        //        if (!tiers[0].IsCube)
        //            return new JoinFullList(tiers.Join(ptiers.AsEnumerable(),
        //                                pr => GetShahKey32(pr, parentKeyOrdinal),
        //                                ch => GetShahKey32(ch, childKeyOrdinal),
        //                                (pr, ch) => new JoinFull { Parent = pr, Child = ch }, new HashComparer()).ToList());
        //        else
        //            return new JoinFullList(tiers.Join(ptiers.AsEnumerable(),
        //                              pr => GetCubeShahKey32(pr, parentKeyOrdinal),
        //                              ch => GetCubeShahKey32(ch, childKeyOrdinal),
        //                              (pr, ch) => new JoinFull { Parent = pr, Child = ch }, new HashComparer()).ToList());
        //    else
        //        return null;
        //}

        public JoinParentList GetJoinParent(DataTiers tiers, DataRelay relay, DataModes mode)
        {
            DataTiers ptiers = null;
            if (mode == DataModes.SimsView || mode == DataModes.Sims)
            {
                relay.Parent.Trell.Mode = DataModes.Sims;
                if (relay.Parent.Trell.SimsView.Count == 0)
                    relay.Parent.Trell.Sims.Query();
                ptiers = relay.Parent.Trell.SimsView;
            }
            else
            {
                relay.Parent.Trell.Mode = DataModes.Tiers;
                if (relay.Parent.Trell.TiersView.Count == 0)
                    relay.Parent.Trell.Tiers.Query();
                ptiers = relay.Parent.Trell.TiersView;
            }

            int[] childKeyOrdinal = relay.Child.KeysOrdinal;
            if (ptiers.FilterEvaluator != null)
            {

                if (!tiers.IsCube)
                    return new JoinParentList(ptiers.AsEnumerable().Select(pr =>
                                                new JoinParent
                                                {
                                                    Child = pr,
                                                    Parent = ((DataTier[])tiers.Registry.Keymap[pr.GetShahKey64(childKeyOrdinal)]).Where(ptiers.FilterEvaluator).ToArray() 
                                                }).ToList());
                else
                    return new JoinParentList(ptiers.AsEnumerable().Select(pr =>
                                               new JoinParent
                                               {
                                                   Child = pr,
                                                   Parent = ((DataTier[])tiers.Registry.Keymap[pr.GetCubeShahKey64(childKeyOrdinal)]).Where(ptiers.FilterEvaluator).ToArray()
                                               }).ToList());
            }
            else
            {
                if (!tiers.IsCube)
                    return new JoinParentList(ptiers.AsEnumerable().Select(pr =>
                                                new JoinParent
                                                {
                                                    Child = pr,
                                                    Parent = ((DataTier[])tiers.Registry.Keymap[pr.GetShahKey64(childKeyOrdinal)])
                                                }).ToList());
                else
                    return new JoinParentList(ptiers.AsEnumerable().Select(pr =>
                                               new JoinParent
                                               {
                                                   Child = pr,
                                                   Parent = ((DataTier[])tiers.Registry.Keymap[pr.GetCubeShahKey64(childKeyOrdinal)])
                                               }).ToList());
            }

        }
        public JoinParentList GetJoinParent(DataTiers tiers, int parentrelayid, DataModes mode)
        {
            return new JoinParentList(tiers.AsEnumerable().Select(pr =>
                                        new JoinParent
                                        {
                                            Child = pr,
                                            Parent = pr.GetParentList(parentrelayid)
                                        }).ToList());

        }
        public JoinChildList GetJoinChild(DataTiers tiers, DataRelay relay, DataModes mode)
        {
            DataTiers ptiers = null;
            if (mode == DataModes.SimsView || mode == DataModes.Sims)
            {
                relay.Child.Trell.Mode = DataModes.Sims;
                if (relay.Child.Trell.SimsView.Count == 0)
                    relay.Child.Trell.Sims.Query();
                ptiers = relay.Child.Trell.SimsView;
            }
            else
            {
                relay.Child.Trell.Mode = DataModes.Tiers;
                if (relay.Child.Trell.TiersView.Count == 0)
                    relay.Child.Trell.Tiers.Query();
                ptiers = relay.Child.Trell.TiersView;
            }

            int[] parentKeyOrdinal = relay.Parent.KeysOrdinal;
            if (ptiers.FilterEvaluator != null)
            {
                if (!tiers.IsCube)
                    return new JoinChildList(tiers.AsEnumerable().Select(pr =>
                                                new JoinChild
                                                {
                                                    Parent = pr,
                                                    Child = ((DataTier[])ptiers.Registry.Keymap[pr.GetShahKey64(parentKeyOrdinal)])
                                                                    .Where(ptiers.FilterEvaluator).ToArray()
                                                }).ToList());
                else
                    return new JoinChildList(tiers.AsEnumerable().Select(pr =>
                                               new JoinChild
                                               {
                                                   Parent = pr,
                                                   Child = ((DataTier[])ptiers.Registry.Keymap[pr.GetCubeShahKey64(parentKeyOrdinal)])
                                                            .Where(ptiers.FilterEvaluator).ToArray()
                                               }).ToList());
            }
            else
            {
                if (!tiers.IsCube)
                    return new JoinChildList(tiers.AsEnumerable().Select(pr =>
                                                new JoinChild
                                                {
                                                    Parent = pr,
                                                    Child = ((DataTier[])ptiers.Registry.Keymap[pr.GetShahKey64(parentKeyOrdinal)]).ToArray()
                                                }).ToList());
                else
                    return new JoinChildList(tiers.AsEnumerable().Select(pr =>
                                               new JoinChild
                                               {
                                                   Parent = pr,
                                                   Child = ((DataTier[])ptiers.Registry.Keymap[pr.GetCubeShahKey64(parentKeyOrdinal)]).ToArray()
                                               }).ToList());
            }

        }
        public JoinChildList GetJoinChild(DataTiers tiers, int childrelayid, DataModes mode)
        {
            return new JoinChildList(tiers.AsEnumerable().Select(pr =>
                                        new JoinChild
                                        {
                                            Parent = pr,
                                            Child = pr.GetChildList(childrelayid)
                                        }).ToList());
        }
        public JoinFullList GetJoinFull(DataTiers tiers, DataRelay relay, DataModes mode)
        {
            DataTiers ptiers = null;
            if (mode == DataModes.SimsView || mode == DataModes.Sims)
            {
                relay.Child.Trell.Mode = DataModes.Sims;
                if (relay.Child.Trell.SimsView.Count == 0)
                    relay.Child.Trell.Sims.Query();
                ptiers = relay.Child.Trell.SimsView;
            }
            else
            {
                relay.Child.Trell.Mode = DataModes.Tiers;
                if (relay.Child.Trell.TiersView.Count == 0)
                    relay.Child.Trell.Tiers.Query();
                ptiers = relay.Child.Trell.TiersView;
            }

            int[] parentKeyOrdinal = relay.Parent.KeysOrdinal;
            int[] childKeyOrdinal = relay.Child.KeysOrdinal;

            if (!tiers[0].IsCube)
                return new JoinFullList(tiers.AsEnumerable().SelectMany(pr =>
                                        ((DataTier[])ptiers.Registry.Keymap[pr.GetShahKey64(parentKeyOrdinal)]).Select(ch =>
                                            new JoinFull
                                            {
                                                Parent = pr,
                                                Child = ch
                                            }).ToList()).ToList());
            else
                return new JoinFullList(tiers.AsEnumerable().SelectMany(pr =>
                                         ((DataTier[])ptiers.Registry.Keymap[pr.GetCubeShahKey64(parentKeyOrdinal)]).Select(ch =>
                                             new JoinFull
                                             {
                                                 Parent = pr,
                                                 Child = ch
                                             }).ToList()).ToList());
        }

        //public JoinCubeList GetJoinCube(DataSubCube subcube, DataTiers subtiers, DataModes mode)
        //{
        //    int length = 0;
        //    if (joinCubeRelays == null)
        //    {
        //        joinCubeRelays = new JoinCubeRelay[subcube.SubCubeRelays.Count];
        //        length = joinCubeRelays.Length;
        //        for (int i = 0; i < length; i++)
        //        {
        //            joinCubeRelays[i] = GetJoinCubeRelay(subcube, subtiers, subcube.SubCubeRelays[i], mode);
        //        }
        //    }
        //    else
        //        length = joinCubeRelays.Length;

        //    return new JoinCubeList(subtiers.AsEnumerable().Select(t => GetJoinSubCube(joinCubeRelays, t, new JoinCube(t, length))).ToArray());
        //}

        //public JoinCube GetJoinSubCube(JoinCubeRelay[] joinCubeRelays, DataTier subtier, JoinCube jc)
        //{
        //    int childindex = 0;
        //    foreach (JoinCubeRelay joinCubeRelay in joinCubeRelays)
        //    {
        //        DataTier childTier = joinCubeRelay.childTiers.Registry.Keymap.GetLast(GetCubeShahKey64(subtier, joinCubeRelay.parentKeyOrdinal));
        //        if (childTier != null)
        //        {
        //            JoinCube _jc;
        //            if (joinCubeRelay.subCubeRelays != null)
        //            {
        //                _jc = new JoinCube(childTier, joinCubeRelay.subCubeRelays.Length);
        //                GetJoinSubCube(joinCubeRelay.subCubeRelays, childTier, _jc);
        //            }
        //            else
        //                _jc = new JoinCube(childTier);
        //            jc.Child[childindex++] = _jc;
        //        }
        //    }
        //    return jc;
        //}

        //public JoinCubeRelay GetJoinCubeRelay(DataSubCube cube, DataTiers subtiers, DataSubCubeRelay cuberelay, DataModes mode)
        //{
        //    DataTiers childtiers = null;
        //    if (mode == DataModes.SimsView || mode == DataModes.Sims)
        //    {
        //        cuberelay.CubeRelay.Child.Trell.Mode = DataModes.Sims;
        //        if (cuberelay.CubeRelay.Child.Trell.SimsView.Count == 0)
        //            cuberelay.CubeRelay.Child.Trell.Sims.Query();
        //        childtiers = cuberelay.CubeRelay.Child.Trell.SimsView;
        //    }
        //    else
        //    {
        //        cuberelay.CubeRelay.Child.Trell.Mode = DataModes.Tiers;
        //        if (cuberelay.CubeRelay.Child.Trell.TiersView.Count == 0)
        //            cuberelay.CubeRelay.Child.Trell.Tiers.Query();
        //        childtiers = cuberelay.CubeRelay.Child.Trell.TiersView;
        //    }

        //    JoinCubeRelay jcr = new JoinCubeRelay();
        //    jcr.subTiers = subtiers;
        //    jcr.childTiers = childtiers;
        //    jcr.parentKeyOrdinal = cuberelay.CubeRelay.Parent.KeysOrdinal;

        //    if (cuberelay.SubCube != null)
        //    {
        //        jcr.subCubeRelays = cuberelay.SubCube.SubCubeRelays.Select(r =>
        //                            GetJoinCubeRelay(cuberelay.SubCube,
        //                                        childtiers,
        //                                        r,
        //                                        mode)).ToArray();
        //    }

        //    return jcr;
        //}

        //public static Noid GetRelayNoid(this DataTier tier, int[] keyOrdinal)
        //{
        //    return new Noid(keyOrdinal.Select(x => tier.iN[x]).ToArray().GetShah());
        //}
        //public static Noid GetCubeRelayNoid(this DataTier tier, int[] keyOrdinal)
        //{
        //    return new Noid(keyOrdinal.Select(x => tier[x]).ToArray().GetShah());
        //}

        //public static Int64 GetShahKey64(this DataTier tier, int[] keyOrdinal)
        //{
        //    if (tier.iN != null)
        //        return keyOrdinal.Select(x => tier.iN[x]).ToArray().GetShahCode64();
        //    else
        //        return 0;
        //}
        //public static Int64 GetCubeShahKey64(this DataTier tier, int[] keyOrdinal)
        //{
        //    return keyOrdinal.Select(x => tier[x]).ToArray().GetShahCode64();
        //}

        //public static Int32 GetShahKey32(this DataTier tier, int[] keyOrdinal)
        //{
        //    if (tier.iN != null)
        //        return keyOrdinal.Select(x => tier.iN[x]).ToArray().GetShahCode32();
        //    else
        //        return 0;
        //}
        //public static Int32 GetCubeShahKey32(this DataTier tier, int[] keyOrdinal)
        //{
        //    return keyOrdinal.Select(x => tier[x]).ToArray().GetShahCode32();
        //}

        public void ClearJoins(IList<DataRelay> relays)
        {
            int[] ids = null; ;

            if (relays != null)
                ids = ParentRelays.Where((r, y) => relays
                                    .Where(t => t.RelayName == r.RelayName)
                                        .Any()).Select(u => ParentRelays.IndexOf(u)).ToArray();
            else
                ids = ParentRelays.Select((x, y) => y).ToArray();

            for (int i = 0; i<ids.Length; i++)               
                joinParentList[ids[i]].List.Clear();

            if (relays != null)
                ids = ChildRelays.Where((r, y) => relays.Where(t => t.RelayName == r.RelayName).Any()).Select(u => ChildRelays.IndexOf(u)).ToArray();
            else
                ids = ChildRelays.Select((x, y) => y).ToArray();

            for (int i = 0; i < ids.Length; i++)
                joinChildList[ids[i]].List.Clear();
        }

        public void ClearJoins()
        {
            joinFullList = null;
            joinChildList = null;
            joinParentList = null;
            //joinFulls = null;
            //joinChilds = null;
            //joinParents = null;
            //joinFullList = null;
            //joinChildList = null;
            //joinParentList = null;
        }
    }  
}
