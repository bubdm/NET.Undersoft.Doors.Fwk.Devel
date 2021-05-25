using System.Collections.Generic;
using System.Linq;
using System.Dors;
using System.Dors.Mathtab;

namespace System.Dors.Data
{
    public static class DataRelayFigures
    {               
        public static DataTiers SubTotal(this DataTiers tiers, bool all = false)
        {
            if (tiers.Trell.Mode == DataModes.Tiers)
                return Result(tiers.Trell.TiersView, all);
            else
                return Result(tiers.Trell.SimsView, all);
        }
        public static DataTiers SubTotal(this DataTrellis trell, bool all = false)
        {
            if(trell.Mode == DataModes.Tiers)
                return Result(trell.TiersView, all);
            else
                return Result(trell.SimsView, all);
        }
        public static DataTiers TrimSubTiers(this DataTiers tiers, bool all = false)
        {
            DataRelay[] RelaysAllOrFiltered = RelaysOrdinal(tiers, all);

            if (RelaysAllOrFiltered.Length == 0 && !all)
                all = true;

            if (tiers.Relaying != null)
            {
               ICollection<DataRelay> childrelays = (!all) ? RelaysAllOrFiltered : tiers.Relaying.ChildRelays.ToArray();

                int i = 0;

                foreach (DataRelay relay in childrelays)
                {
                    DataTiers view = (tiers.Mode == DataModes.Sims ||
                                       tiers.Mode == DataModes.SimsView) ?
                                       relay.Child.Trell.SimsView :
                                       relay.Child.Trell.TiersView;
                    DataTier[] newview = tiers.TiersView.GetChildList()[i].List.SelectMany(t => t.Child).ToArray();                                       

                    view.AddViewRange(newview);
                    i++;


                }
                tiers.TiersView.ClearJoins();
            }
            return tiers.TiersView;
        }

        //private static DataTiers ResultByMap(DataTiers tiers, bool all = false)
        //{
        //    DataModes mode = (tiers.Mode == DataModes.Tiers) ? DataModes.TiersView : DataModes.SimsView;
                
        //    DataPylon[] joinPylons = tiers.Trell.JoinPylons.AsArray();
        //    if (joinPylons.Length > 0)
        //    {
        //        DataRelay[] RelaysAllOrFiltered = RelaysOrdinal(tiers, all);
        //        if (RelaysAllOrFiltered.Length == 0 && !all)
        //        {
        //            all = true;
        //            RelaysAllOrFiltered = null;
        //        }
        //        //tiers.ClearJoins();

        //        //JoinChildList[] subresult = (!all) ? tiers.GetChildList(RelaysAllOrFiltered).ToArray() : tiers.GetChildList();

        //        //if (subresult.Length > 0)
        //        //{

        //        tiers.State.withPropagate = false;
        //        int[] childinherits = tiers.Trell.ChildInherits;
        //        tiers.AsEnumerable().Select((o, z) => o.GetChildMaps(RelaysAllOrFiltered).Select((j, y) => j != null && j.Count > 0 ?
        //         o.Grating.PutRange(joinPylons.Where(s =>
        //                (s.JoinIndex != null) &&
        //                (s.JoinPattern != null &&
        //                s.JoinIndex[0] == y)).Select(s =>
        //                 new KeyValuePair<long, object>(s.Ordinal,
        //                 (s.JoinOperand == AggregateOperand.Default && o[s.Ordinal] == s.Default) ?
        //                  j.Select(f => f.Value[childinherits[y]][s.JoinOrdinal[0]]).First() :
        //                 (s.JoinOperand == AggregateOperand.Bind || s.JoinOperand == AggregateOperand.First) ?
        //                  j.Select(f => f.Value[childinherits[y]][s.JoinOrdinal[0]]).First() :
        //                 (s.JoinOperand == AggregateOperand.Last) ?
        //                  j.Select(f => f.Value[childinherits[y]][s.JoinOrdinal[0]]).Last() :
        //                 (s.JoinOperand == AggregateOperand.Sum) ?
        //                  j.Sum(f => Convert.ToDouble(f.Value[childinherits[y]][s.JoinOrdinal[0]])) :
        //                 (s.JoinOperand == AggregateOperand.Min) ?
        //                 j.Min(f => Convert.ToDouble(f.Value[childinherits[y]][s.JoinOrdinal[0]])) :
        //                   (s.JoinOperand == AggregateOperand.Max) ?
        //                 j.Max(f => Convert.ToDouble(f.Value[childinherits[y]][s.JoinOrdinal[0]])) :
        //                     (s.JoinOperand == AggregateOperand.Count) ?
        //                  j.Count() :
        //                   (s.JoinOperand == AggregateOperand.Avg) ?
        //                 Convert.ChangeType(j.Average(f => Convert.ToDouble(f.Value[childinherits[y]][s.JoinOrdinal[0]])), typeof(string)) :
        //                   (s.JoinOperand == AggregateOperand.Bis) ?
        //                  j.Select(f => (f.Value[childinherits[y]][s.JoinOrdinal[0]] != DBNull.Value) ?
        //                                                         f.Value[childinherits[y]][s.JoinOrdinal[0]].ToString() : "")
        //                                                             .Aggregate((x, u) => x + " " + u) : ""
        //                  )).ToDictionary(k => k.Key, v => v.Value)) : false).ToArray()).ToArray();

        //        tiers.State.withPropagate = true;

        //        //tiers.AddViewRange(tiers);
        //        return tiers;
        //        //}
        //        //else
        //        //{
        //        //    if (!all)
        //        //        tiers.Clear();
        //        //    return tiers;
        //        //}
        //    }
        //    else
        //        return tiers;            
        //}

        private static DataTiers Result(DataTiers tiers, bool all = false)
        {
            DataModes mode = (tiers.Mode == DataModes.Tiers) ? DataModes.TiersView : DataModes.SimsView;

            DataPylon[] joinPylons = tiers.Trell.JoinPylons.AsArray();
            if (joinPylons.Length > 0)
            {
                HashSet<int> joinChildRelayIds = new HashSet<int>();
                joinPylons.Where(j => j.JoinIndex != null).Select(j => joinChildRelayIds.Add(j.JoinIndex[0])).ToArray();
                DataRelay[] RelaysAllOrFiltered = RelaysOrdinal(tiers, all);
                if (RelaysAllOrFiltered.Length == 0 && !all)
                {
                    all = true;
                    RelaysAllOrFiltered = null;
                }

                int[] ids = null;

                JoinChildList[] subresult = null;
                if (!all)
                {
                    subresult = tiers.GetChildList(RelaysAllOrFiltered);
                    ids = RelaysAllOrFiltered.Select(r => tiers.Trell.ChildRelays.GetIdByName(r.RelayName)).Where(id => joinChildRelayIds.Contains(id)).ToArray();
                }
                else
                {
                    ids = joinChildRelayIds.ToArray();
                    subresult = tiers.GetChildList(ids);
                }

                if (subresult.Length > 0)
                {

                    tiers.State.withPropagate = false;
                    IList<JoinChild>[] listjch = subresult.Select((j, y) => j != null && ids.Contains(y) ? j.List : null).ToArray();
                    try
                    {
                        listjch.Select((l, y) => l.Select(o =>
                        joinPylons.Where(s =>
                                (s.JoinIndex != null) &&
                                (s.JoinPattern != null &&
                                s.JoinIndex[0] == y) &&
                                o.Child.Length > 0).Select(s =>
                                 o.Parent.Grating.Put(s.Ordinal,
                                 (s.JoinOperand == AggregateOperand.Default && o.Parent[s.Ordinal] == s.Default) ?
                                  o.Child.Select(f => f[s.JoinOrdinal[0]]).First() :
                                 (s.JoinOperand == AggregateOperand.Bind || s.JoinOperand == AggregateOperand.First) ?
                                  o.Child.Select(f => f[s.JoinOrdinal[0]]).First() :
                                 (s.JoinOperand == AggregateOperand.Last) ?
                                  o.Child.Select(f => f[s.JoinOrdinal[0]]).Last() :
                                 (s.JoinOperand == AggregateOperand.Sum) ?
                                  o.Child.Sum(f => f[s.JoinOrdinal[0]] is DateTime ?
                                    ((DateTime)f[s.JoinOrdinal[0]]).ToOADate() :
                                        Convert.ToDouble(f[s.JoinOrdinal[0]])) :
                                 (s.JoinOperand == AggregateOperand.Min) ?
                                 o.Child.Min(f => f[s.JoinOrdinal[0]] is DateTime ?
                                    ((DateTime)f[s.JoinOrdinal[0]]).ToOADate() :
                                        Convert.ToDouble(f[s.JoinOrdinal[0]])) :
                                   (s.JoinOperand == AggregateOperand.Max) ?
                                 o.Child.Max(f => f[s.JoinOrdinal[0]] is DateTime ?
                                    ((DateTime)f[s.JoinOrdinal[0]]).ToOADate() :
                                       Convert.ToDouble(f[s.JoinOrdinal[0]])) :
                                     (s.JoinOperand == AggregateOperand.Count) ?
                                  o.Child.Count() :
                                   (s.JoinOperand == AggregateOperand.Avg) ?
                                 Convert.ChangeType(o.Child.Average(f =>
                                    f[s.JoinOrdinal[0]] is DateTime ?
                                        ((DateTime)f[s.JoinOrdinal[0]]).ToOADate() :
                                            Convert.ToDouble(f[s.JoinOrdinal[0]])), typeof(string)) :
                                   (s.JoinOperand == AggregateOperand.Bis) ?
                                  o.Child.Select(f => (f[s.JoinOrdinal[0]] != DBNull.Value) ?
                                                                         f[s.JoinOrdinal[0]].ToString() : "")
                                                                             .Aggregate((x, u) => x + " " + u) : ""
                                  )).ToArray()).ToArray()).ToArray();
                    }
                    catch (Exception ex)
                    {
                    }
                    tiers.State.withPropagate = true;

                    //tiers.AddViewRange(tiers);
                    return tiers;
                }
                else
                {
                    if (!all)
                        tiers.Clear();
                    return tiers;
                }
            }
            else
                return tiers;
        }       

        //private static DataTiers ResultLoop(DataTiers tiers, bool all = false)
        //{
        //    DataModes mode = (tiers.Mode == DataModes.Tiers) ? DataModes.TiersView : DataModes.SimsView;

        //    DataPylon[] joinPylons = tiers.Trell.JoinPylons.AsArray();
        //    if (joinPylons.Length > 0)
        //    {
        //        DataRelay[] RelaysAllOrFiltered = RelaysOrdinal(tiers, all);
        //        if (RelaysAllOrFiltered.Length == 0 && !all)
        //        {
        //            all = true;
        //            RelaysAllOrFiltered = null;
        //        }

        //        int[] ids = null;

        //        JoinChildList[] subresult = null;
        //        if (!all)
        //        {
        //            subresult = tiers.GetChildList(RelaysAllOrFiltered);
        //            ids = RelaysAllOrFiltered.SelectMany(r => tiers.Trell.ChildRelays.Where(d => d.RelayName == r.RelayName).Select(i => tiers.Trell.ChildRelays.IndexOf(i))).ToArray();
        //        }
        //        else
        //        {
        //            subresult = tiers.GetChildList();
        //            ids = tiers.Trell.ChildRelays.Select((x, y) => y).ToArray();
        //        }

        //        if (subresult.Length > 0)
        //        {

        //            tiers.State.withPropagate = false;
        //            List<IList<JoinChild>> listjch = subresult.Select((j, y) => j != null && ids.Contains(y) ? j.List : null).ToList();
        //            try
        //            {
        //                int length = ids.Length;
        //                for (int i = 0; i < length; i++)
        //                {
        //                    int y = ids[i];
        //                    if (listjch[y] != null)
        //                    {
        //                        DataPylon[] _joinPylons = joinPylons.Where(s =>
        //                                (s.JoinIndex != null) &&
        //                                (s.JoinPattern != null &&
        //                                s.JoinIndex[0] == y)).ToArray();
        //                        if (_joinPylons.Length > 0)
        //                        {

        //                            listjch[y].Where(l => l.Child.Length > 0).Select(o => o.Parent.Grating.PutRange(_joinPylons.Select(s => 
        //                                    new Vessel<object>(s.Ordinal,
        //                                     (s.JoinOperand == AggregateOperand.Default && o.Parent[s.Ordinal] == s.Default) ?
        //                                      o.Child.Select(f => f[s.JoinOrdinal[0]]).First() :
        //                                     (s.JoinOperand == AggregateOperand.Bind || s.JoinOperand == AggregateOperand.First) ?
        //                                      o.Child.Select(f => f[s.JoinOrdinal[0]]).First() :
        //                                     (s.JoinOperand == AggregateOperand.Last) ?
        //                                      o.Child.Select(f => f[s.JoinOrdinal[0]]).Last() :
        //                                     (s.JoinOperand == AggregateOperand.Sum) ?
        //                                      o.Child.Sum(f => f[s.JoinOrdinal[0]] is DateTime ?
        //                                        ((DateTime)f[s.JoinOrdinal[0]]).ToOADate() :
        //                                            Convert.ToDouble(f[s.JoinOrdinal[0]])) :
        //                                     (s.JoinOperand == AggregateOperand.Min) ?
        //                                     o.Child.Min(f => f[s.JoinOrdinal[0]] is DateTime ?
        //                                        ((DateTime)f[s.JoinOrdinal[0]]).ToOADate() :
        //                                            Convert.ToDouble(f[s.JoinOrdinal[0]])) :
        //                                       (s.JoinOperand == AggregateOperand.Max) ?
        //                                     o.Child.Max(f => f[s.JoinOrdinal[0]] is DateTime ?
        //                                        ((DateTime)f[s.JoinOrdinal[0]]).ToOADate() :
        //                                           Convert.ToDouble(f[s.JoinOrdinal[0]])) :
        //                                         (s.JoinOperand == AggregateOperand.Count) ?
        //                                      o.Child.Count() :
        //                                       (s.JoinOperand == AggregateOperand.Avg) ?
        //                                     Convert.ChangeType(o.Child.Average(f =>
        //                                        f[s.JoinOrdinal[0]] is DateTime ?
        //                                            ((DateTime)f[s.JoinOrdinal[0]]).ToOADate() :
        //                                                Convert.ToDouble(f[s.JoinOrdinal[0]])), typeof(string)) :
        //                                       (s.JoinOperand == AggregateOperand.Bis) ?
        //                                      o.Child.Select(f => (f[s.JoinOrdinal[0]] != DBNull.Value) ?
        //                                                                             f[s.JoinOrdinal[0]].ToString() : "")
        //                                                                                 .Aggregate((x, u) => x + " " + u) : ""
        //                                      )).ToArray())).ToArray();
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //            }
        //            tiers.State.withPropagate = true;

        //            //tiers.AddViewRange(tiers);
        //            return tiers;
        //        }
        //        else
        //        {
        //            if (!all)
        //                tiers.Clear();
        //            return tiers;
        //        }
        //    }
        //    else
        //        return tiers;
        //}

        private static List<DataRelay> RelayedTrells(DataTiers tiers, bool all = false)
        {
            if (!all)
                return tiers.Trell.ChildRelays.Where(cr => cr.Child.Trell.Filter.Count > 0).ToList();
            else
                return tiers.Trell.ChildRelays.Select((x, y) => x).ToList();
        }

        private static DataRelay[] RelaysOrdinal(DataTiers tiers, bool all = false)
        {
            if (!all)
                return tiers.Trell.ChildRelays.Select((x,y) => (x.Child.Trell.Filter.Count > 0) ? x : null).Where(i => i != null).ToArray();
            else
                return tiers.Trell.ChildRelays.Select((x, y) => x).ToArray();
        }
    }

    public static class DataTotalFigures
    {
        public static DataTiers Total(this DataTiers tiers, bool all = false)
        {
            tiers.TiersTotal = Result(tiers, all);
            return tiers.TiersTotal;
        }
        public static DataTiers Total(this DataTrellis trell, bool all = false)
        {
            if (trell.Mode == DataModes.Tiers)
                return trell.Tiers.Total(all);
            else
                return trell.Sims.Total(all);
        }

        private static DataTiers Result(DataTiers tiers, bool all = true)
        {
            DataPylons totalPylons = tiers.Trell.TotalPylons;
            if (totalPylons.AsEnumerable().Any())
            {
                object[] result = totalPylons.AsEnumerable().AsParallel().SelectMany(s =>
                       new object[]
                       {
                           (!string.IsNullOrEmpty(s.PylonName)) ?
                            (s.TotalOperand == AggregateOperand.Sum) ?
                                Convert.ChangeType(tiers.TiersView.AsEnumerable().Sum(j => (j[s.TotalOrdinal] is DateTime) ?
                                            ((DateTime)j[s.TotalOrdinal]).ToOADate() : 
                                                Convert.ToDouble(j[s.Ordinal])), typeof(object)) :
                                 (s.TotalOperand == AggregateOperand.Min) ?
                                Convert.ChangeType(tiers.TiersView.AsEnumerable().Min(j => (j[s.TotalOrdinal] is DateTime) ?
                                            ((DateTime)j[s.TotalOrdinal]).ToOADate() :
                                                Convert.ToDouble(j[s.Ordinal])), typeof(object)) :
                                 (s.TotalOperand == AggregateOperand.Max) ?
                                Convert.ChangeType(tiers.TiersView.AsEnumerable().Max(j => (j[s.TotalOrdinal] is DateTime) ?
                                            ((DateTime)j[s.TotalOrdinal]).ToOADate() :
                                                Convert.ToDouble(j[s.Ordinal])), typeof(object)) :
                                 (s.TotalOperand == AggregateOperand.Avg) ?
                               Convert.ChangeType(tiers.TiersView.AsEnumerable().Average(j => (j[s.TotalOrdinal] is DateTime) ?
                                            ((DateTime)j[s.TotalOrdinal]).ToOADate() :
                                                Convert.ToDouble(j[s.Ordinal])), typeof(object)) :
                                 (s.TotalOperand == AggregateOperand.Bis) ?
                               Convert.ChangeType(tiers.TiersView.AsEnumerable().Select(j => (j[s.Ordinal] != DBNull.Value) ? j[s.Ordinal].ToString() : "").Aggregate((x, y) => x + " " + y), typeof(object)) : null : null
                            }
                 ).ToArray();
                DataTrellis trellTotal = new DataTrellis("Totals_" + tiers.Trell.TrellName);
                trellTotal.Tiers.Mode = tiers.Mode;
                trellTotal.Pylons.AddRange(totalPylons.AsEnumerable().ToList());
                DataTier nTier = new DataTier(trellTotal);
                nTier.PrimeArray = result;
                trellTotal.Tiers.Add(nTier);

                return trellTotal.Tiers;
            }
            else
                return null;
        }
    }

    public static class DataPylonFigures
    {
        public static DataTiers Compute(this DataTiers tiers, ComputeMode arithmeticMode, bool all = false)
        {
            return Result(tiers, arithmeticMode, all);
        }
        public static DataTiers Compute(this DataTrellis trell, ComputeMode arithmeticMode, bool all = false)
        {
            if (trell.Mode.IsTiersMode())
                return trell.TiersView.Compute(arithmeticMode, all);
            else
                return trell.SimsView.Compute(arithmeticMode, all);
        }

        private static DataTiers Result(DataTiers tiers, ComputeMode arithmeticMode, bool all = true)
        {
            try
            {
                if (arithmeticMode == ComputeMode.Lambda)
                {
                    tiers.TiersView.AsEnumerable().Select(s => tiers.Trell.LambdaHash.Select(n => tiers.Trell.Pylons[n]).OrderBy(o => o.ComputeOrdinal)
                                                  .Select(r => s[r.Ordinal] = r.LambdaMethod.Invoke(s)).ToArray()).ToArray();
                }              
                else if (arithmeticMode == ComputeMode.Mattab)
                {
                    tiers.Trell.Pylons.SetMattabData(tiers);                   
                    tiers.Trell.Pylons.LeftMattabPylons.Where(p => !p.PartialMattab).OrderBy(p => p.ComputeOrdinal).Select(p => p.EvaluateMattab(tiers.Mode)).ToArray();
                }
                return tiers;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
 
}
