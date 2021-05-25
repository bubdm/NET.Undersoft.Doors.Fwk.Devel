using System;
using System.Linq;
using System.Collections.Generic;
using System.Quantic.Core;

namespace System.Quantic.Data
{
    //public class TiersChanges : IDataChanges
    //{
    //    public DataPylons Pylons { get; set; }
    //    public DataTiers  Tiers
    //    { get; set; }    
    //    public DataState State
    //    { get { return Tiers.State; } }

    //    public int  PylonCount { get { return Pylons.Count; } }
    //    public int  NoidOrdinal { get { return Pylons.NoidOrdinal; } }
    //    public int  QuidOrdinal { get { return Pylons.QuidOrdinal; } }

    //    public short Level
    //    { get { return Tiers.Trell.EditLevel; } }
    //    public short EditLength
    //    { get { return Tiers.Trell.EditLength; } }

    //    private bool single = true;

    //    public bool saved
    //    {
    //        get { return State.Saved; }
    //        set { State.Saved = value; }
    //    }
    //    public bool edited
    //    { 
    //        get { return State.Edited; }
    //        set { State.Edited = value; }
    //    }
    //    public bool synced
    //    {
    //        get { return State.Synced; }
    //        set { State.Synced = value; }
    //    }
    //    public bool cancel
    //    { get { return State.Canceled; } }

    //    public HashList<object> changes { get; set; }

    //    public TiersChanges(DataTiers tiers)
    //    {
    //        Tiers = tiers;
    //        single = Tiers.Trell.EditLength > 1 ? false : true;
    //        changes = new HashList<object>(1024);
    //        Pylons = tiers.Trell.Pylons;
    //    }
    //    public TiersChanges(DataTiers tiers, HashList<object> input)
    //    {
    //        Tiers = tiers;
    //        single = Tiers.Trell.EditLength > 1 ? false : true;
    //        Pylons = tiers.Trell.Pylons;
    //        bool added = false;

    //        if (tiers.Changes != null)
    //            if (tiers.Changes.changes.Count > 0)
    //            {
    //                changes = tiers.Changes.changes;
    //                added = true;
    //            }

    //        if (!added)
    //            changes = new HashList<object>(1024);

    //        foreach (Vessel<object> obj in input)
    //            changes.Add(obj.Key, obj.Value);

    //    }
    //    public TiersChanges(DataTiers tiers, Dictionary<long, object> input)
    //    {
    //        Tiers = tiers;
    //        single = Tiers.Trell.EditLength > 1 ? false : true;

    //        Pylons = tiers.Trell.Pylons;

    //        bool added = false;
    //        if (tiers.Changes != null)
    //            if (tiers.Changes.changes.Count > 0)
    //            {
    //                changes = tiers.Changes.changes;
    //                added = true;
    //            }

    //        if (!added)
    //            changes = new HashList<object>(1024);

    //        changes.Add(input);

    //    }
    //    public TiersChanges(DataTiers tiers, object[][] input, bool inputCopy = false)
    //    {
    //        bool added = false;
    //        single = Tiers.Trell.EditLength > 1 ? false : true;

    //        Pylons = tiers.Trell.Pylons;

    //        if (tiers.Changes != null)
    //            if (tiers.Changes.changes.Count > 0)
    //            {
    //                changes = tiers.Changes.changes;
    //                added = true;
    //            }

    //        if (!added)
    //            changes = new HashList<object>(1024);

    //        foreach (object[] item in input)
    //            changes.Add((long)item[0], item[1]);

    //    }

    //    private bool add(IDataCells tier, int index, object value)
    //    {
    //        if (tier.IsEditable(index))
    //        {
    //            int position = tier.Index * PylonCount + index;
    //            Vessel<object> obj = changes.GetVessel(position);
    //            if (obj == null || cancel)
    //            {
    //                if (!tier.iN[index].IsSameOrNull(value))
    //                {
    //                    if (!single)
    //                    {
    //                        object[] _obj = new object[EditLength + 1];
    //                        _obj[0] = value;
    //                        changes.Add(new Vessel<object>(position, _obj));
    //                    }
    //                    else
    //                    {
    //                        changes.Add(new Vessel<object>(position, value));
    //                    }
    //                    return true;
    //                }
    //            }
    //            else
    //            {
    //                if (!single)
    //                {
    //                    object[] _obj = (object[])obj.Value;
    //                    int changeid = Array.IndexOf(_obj, null);
    //                    if (_obj[changeid - 1].Equals(value))
    //                    {
    //                        if (changeid >= EditLength)
    //                        {
    //                            _obj[EditLength] = value; Array.Reverse(_obj); Array.Resize(ref _obj, EditLength);
    //                            Array.Reverse(_obj); Array.Resize(ref _obj, EditLength + 1);
    //                        };
    //                        _obj[changeid - 1] = value;
    //                        obj.Value = _obj;
    //                        return true;
    //                    }
    //                }
    //                else if (!obj.Value.IsSameOrNull(value))
    //                {
    //                    obj.Value = value;
    //                    return true;
    //                }
    //            }
    //        }

    //        return false;
    //    }
    //    private bool add(IDataCells tier, IDataCells cubetier, int cubeordinal, int index, object value)
    //    {
    //        if (tier.IsEditable(index))
    //        {
    //            int position = tier.Index * PylonCount + index;
    //            Vessel<object> obj = changes.GetVessel(position);
    //            if (obj == null || cancel)
    //            {
    //                if (!cubetier[cubeordinal].IsSameOrNull(value))
    //                {
    //                    if (!single)
    //                    {
    //                        object[] _obj = new object[EditLength + 1];
    //                        _obj[0] = value;
    //                        changes.Add(new Vessel<object>(position, _obj));
    //                    }
    //                    else
    //                    {
    //                        changes.Add(new Vessel<object>(position, value));
    //                    }
    //                    return true;
    //                }
    //            }
    //            else
    //            {
    //                if (!single)
    //                {
    //                    object[] _obj = (object[])obj.Value;
    //                    int changeid = Array.IndexOf(_obj, null);
    //                    if (_obj[changeid - 1].Equals(value))
    //                    {
    //                        if (changeid >= EditLength)
    //                        {
    //                            _obj[EditLength] = value; Array.Reverse(_obj); Array.Resize(ref _obj, EditLength);
    //                            Array.Reverse(_obj); Array.Resize(ref _obj, EditLength + 1);
    //                        };
    //                        _obj[changeid - 1] = value;
    //                        obj.Value = _obj;
    //                        return true;
    //                    }
    //                }
    //                else if (!obj.Value.IsSameOrNull(value))
    //                {
    //                    obj.Value = value;
    //                    return true;
    //                }
    //            }
    //        }
    //        return false;
    //    }

    //    public bool Add(int tierid, int index, object value)
    //    {            
    //        DataTier tier = Tiers[tierid];
    //        if (add(tier, index, value))
    //        {
    //            if (!tier.Edited)
    //                tier.Edited = true;
    //            return true;
    //        }
    //        return false;            
    //    }
    //    public bool Add(IDataCells tier, int index, object value)
    //    {
    //        if (add(tier, index, value))
    //        {
    //            if (!tier.Edited)
    //                tier.Edited = true;
    //            return true;
    //        }
    //        return false;
    //    }
    //    public bool Add(int tierid, IDataCells cubetier, int cubeordinal, int index, object value)
    //    {
    //        DataTier tier = Tiers[tierid];
    //        if (add(tier, cubetier, cubeordinal, index, value))
    //        {
    //            if (!tier.Edited)
    //                if (tier.IsEditable(index))
    //                    tier.Edited = true;
    //            return true;
    //        }
    //        return false;
    //    }
    //    public bool Add(IDataCells tier, IDataCells cubetier, int cubeordinal, int index, object value)
    //    {
    //        if (add(tier, cubetier, cubeordinal, index, value))
    //        {
    //            if (!tier.Edited)
    //                tier.Edited = true;
    //            return true;
    //        }
    //        return false;
    //    }

    //    private bool put(IDataCells tier, int index, object value)
    //    {
    //        if (tier.IsEditable(index))
    //        {
    //            int position = tier.Index * PylonCount + index;
    //            Vessel<object> obj = changes.GetVessel(position);
    //            if (obj == null || cancel)
    //            {
    //                if (!single)
    //                {
    //                    object[] _obj = new object[EditLength + 1];
    //                    _obj[0] = value;
    //                    changes.Put(position, _obj);
    //                }
    //                else
    //                {
    //                    changes.Put(position, value);
    //                }
    //                return true;
    //            }
    //            else
    //            {
    //                if (!single)
    //                {
    //                    object[] _obj = (object[])obj.Value;
    //                    int changeid = Array.IndexOf(_obj, null);
    //                    if (changeid >= EditLength)
    //                    {
    //                        _obj[EditLength] = value;
    //                        Array.Reverse(_obj);
    //                        Array.Resize(ref _obj, EditLength);
    //                        Array.Reverse(_obj);
    //                        Array.Resize(ref _obj, EditLength + 1);
    //                    };
    //                    _obj[changeid - 1] = value;
    //                    obj.Value = _obj;
    //                    return true;
    //                }
    //                else
    //                {
    //                    obj.Value = value;
    //                    return true;
    //                }
    //            }
    //        }
    //        return false;
    //    }

    //    public bool Put(int tierid, int index, object value)
    //    {
    //        DataTier tier = Tiers[tierid];
    //        if (put(tier, index, value))
    //        {
    //            if (!tier.Edited)
    //                tier.Edited = true;
    //            return true;
    //        }
    //        return false;
    //    }
    //    public bool Put(IDataCells tier, int index, object value)
    //    {
    //        if (put(tier, index, value))
    //        {
    //            if (!tier.Edited)
    //                tier.Edited = true;
    //            return true;
    //        }
    //        return false;
    //    }

    //    public bool Put(int tierid, IDataCells cubetier, int cubeordinal, int index, object value)
    //    {
    //        DataTier tier = Tiers[tierid];
    //        if (put(tier, index, value))
    //        {
    //            if (!tier.Edited)
    //                tier.Edited = true;
    //            return true;
    //        }
    //        return false;
    //    }
    //    public bool Put(IDataCells tier, IDataCells cubetier, int cubeordinal, int index, object value)
    //    {
    //        if (put(tier, index, value))
    //        {
    //            if (!tier.Edited)
    //                tier.Edited = true;
    //            return true;
    //        }
    //        return false;
    //    }

    //    public IDataChanges AddRange(Dictionary<long, object> input)
    //    {
    //        changes.Add(input);
    //        return this;
    //    }
    //    public IDataChanges AddRange(object[][] input)
    //    {
    //        foreach (object[] item in input)
    //            changes.Add((int)item[0], item[1]);
    //        return this;
    //    }

    //    public IDataChanges PutRange(Dictionary<long, object> input)
    //    {
    //        try
    //        {
    //            changes.Put(input);
    //        }
    //        catch (Exception e)
    //        {

    //        }
    //        return this;
    //    }
    //    public IDataChanges PutRange(object[][] input)
    //    {
    //        foreach (object[] item in input)
    //            changes.Put((int)item[0], item[1]);
    //        return this;
    //    }

    //    private object changeOn(IDataCells tier, int x)
    //    {
    //        int position = tier.Index * PylonCount + x;
    //        Vessel<object> _obj = changes.GetVessel(position);
    //        if (_obj != null)
    //        {
    //            Type t = Tiers.Trell.Pylons[x].DataType;
    //            if (!single)
    //            {
    //                object[] obj = (object[])_obj.Value;
    //                int changeid = Array.IndexOf(obj, null) - Level;
    //                object o = obj[(changeid < 0) ? 0 : changeid];
    //                if (o.GetType() != t)
    //                    return Convert.ChangeType(o, t);
    //                else
    //                    return o;
    //            }
    //            if (_obj.Value != null)
    //            {
    //                if (_obj.Value.GetType() == t)
    //                    return _obj.Value;
    //                return _obj.Value = Convert.ChangeType(_obj.Value, t);
    //            }
    //            else if (t == typeof(string))
    //                return _obj.Value = "";
    //        }
    //        return null;
    //    }

    //    public object ChangeOn(int tierid, int x)
    //    {
    //        DataTier tier = Tiers[tierid];
    //        return changeOn(tier, x);
    //    }
    //    public object ChangeOn(IDataCells tier, int x)
    //    {
    //        return changeOn(tier, x);
    //    }
    //    public object ChangeOn(object result, int tierid, int x)
    //    {
    //        DataTier tier = Tiers[tierid];
    //        object _result = changeOn(tier, x);
    //        if (_result != null)
    //            return _result;
    //        return result;          
    //    }
    //    public object ChangeOn(object result, IDataCells tier, int x)
    //    {
    //        object _result = changeOn(tier, x);
    //        if (_result != null)
    //            return _result;
    //        return result;
    //    }

    //    public IList<Vessel<object>> EditedCells(IDataCells tier)
    //    {
    //        int offset = tier.Index * Tiers.Trell.Pylons.Count;
    //        List<Vessel<object>> ol = new List<Vessel<object>>();
    //        foreach(int i in Tiers.Trell.Pylons.EditablePylons)
    //        {
    //            Vessel<object> o = changes.GetVessel(offset + i);
    //            if(o != null)
    //                ol.Add(o);
    //        }
    //        return ol;
    //    }

    //    public void SaveChanges()
    //    {

    //        if(changes.Count > 0)
    //        {
    //            int pylCount = Tiers.Trell.Pylons.Count;
    //            if (!Tiers.Trell.IsCube)
    //            {
    //                    foreach (Vessel<object> change in changes)
    //                    {
    //                        DataTier tr = Tiers[change.Key / pylCount];
    //                        tr.iN[(int)(change.Key % pylCount)] = change.Value;
    //                        tr.Cells.ClearBlames();
    //                    }                    
    //            }
    //            else
    //            {
    //                foreach (Vessel<object> change in changes)
    //                {
    //                    DataTier tr = Tiers[change.Key / pylCount];

    //                    DataCells datacells = null;
    //                    if (Tiers.Mode != DataModes.Sims && Tiers.Mode != DataModes.SimsView)
    //                        datacells = tr.Cells;
    //                    else
    //                        datacells = tr.Devisor.Cells;
    //                    DataTier ctier = datacells.Tier.SubTier;
    //                    DataTiers[] _temp = ctier.GetCubeChildMap(Tiers.Trell.Cube.CubeRelays);
    //                    int index = (int)(change.Key % pylCount);
    //                    DataPylon cpyl = Tiers.Trell.Pylons[index];
    //                    IDataCells datacell = datacells[index];
    //                    if (!ReferenceEquals((DataTier)datacell, datacells.Tier))
    //                    {
    //                        datacell.iN[cpyl.CubeOrdinal[0]] = change.Value; //Changes.ChangeOn(index);
    //                    }
    //                    else
    //                    {
    //                        int cl = cpyl.CubeLevel;
    //                        if (cl > 0)
    //                        {
    //                            int c = 0;
    //                            DataTiers temp = _temp[cpyl.CubeIndex[1]];
    //                            for (c = 0; c < cl; c++)
    //                            {
    //                                //DataTier ttemp = ctier;
    //                                if (c > 0)
    //                                    temp = ctier.GetCubeChildMap(Tiers.Trell.Cube.CubeRelays)[cpyl.CubeIndex[c + 1]];
    //                                if (temp != null && temp.Count > 0)
    //                                {
    //                                    ctier = temp[0];
    //                                }
    //                                else
    //                                {
    //                                    ctier = temp.AddNew();
    //                                    temp.AutoKeys.Select(k => ctier.iN[k.Key] = k.Value).ToArray();
    //                                }
    //                            }
    //                            int ci = cpyl.Pylons[index].CubeIndex[c];
    //                            ctier.iN[cpyl.CubeOrdinal[0]] = change.Value; //Changes.ChangeOn(Index, index);
    //                            Tiers.Trell.Pylons.Where(p => p.CubeIndex != null &&
    //                                                p.CubeIndex.Length == (c + 1) &&
    //                                                    p.CubeIndex[c] == ci)
    //                                                        .Select(o => datacells.Replace(o.Ordinal, ctier)).ToArray();
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    public void Cancel()
    //    {
    //        if (cancel)
    //            Clear();
    //    }

    //    public void Clear()
    //    {  
    //        if (changes != null)
    //            changes.Clear();         
    //        changes = null;
    //    }
    //}
}