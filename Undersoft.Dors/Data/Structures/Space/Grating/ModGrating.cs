using System;
using System.Linq;
using System.Collections.Generic;
using System.Quantic.Core;

namespace System.Quantic.Data
{
    public class ModGrating : IDataGrating
    {
        public DataTiers  Tiers
        { get; set; }    
        public DataState State
        { get { return Tiers.Trell.State; } }
       
        public short Level
        { get { return Tiers.Trell.EditLevel; } }
        public short EditLength
        { get { return Tiers.Trell.EditLength; } }

        public int PylonsCount
        {
            get { return Tiers.Trell.Pylons.Count; }
        }

        private bool single = true;

        public bool saved
        {
            get { return State.Saved; }
            set { State.Saved = value; }
        }
        public bool edited
        { 
            get { return State.Edited; }
            set { State.Edited = value; }
        }
        public bool synced
        {
            get { return State.Synced; }
            set { State.Synced = value; }
        }
        public bool cancel
        { get { return State.Canceled; } }

        public HashList<object> changes { get; set; }
       
        public ModGrating(DataTiers tiers)
        {
            Tiers = tiers;
            changes = new HashList<object>();
        }
        public void AssignGrating(DataTier tier, HashList<object> input)
        {
            PutRange(tier.Index, input);
        }
        public void AssignGrating(DataTier tier, Dictionary<long, object> input)
        {
            PutRange(tier.Index, input);
        }
        public void AssignGrating(DataTier tier, object[][] input)
        {
            PutRange(tier.Index, input);
        }

        public bool Add(int tierindex, int index, object value)
        {
            int position = index + (tierindex * PylonsCount);
            bool waschange = false;
            object _obj;
            int[] noidord = new int[] { Tiers.Trell.NoidOrdinal, Tiers.Trell.QuidOrdinal };
            if (!noidord.Contains(index))
                if (cancel || !changes.TryGet(position, out _obj))
                {
                    if (!Tiers[tierindex].iN[index].IsSameOrNull(value))
                    {
                        if (EditLength > 1)
                        {
                            single = false;
                            object[] obj = new object[EditLength + 1];
                            obj[0] = value;
                            changes.Add(position, obj);
                        }
                        else
                        {
                            single = true;
                            changes.Add(position, value);
                        }
                        waschange = true;
                    }
                }
                else
                {
                    if (!single)
                    {
                        object[] obj = (object[])_obj;
                        int changeid = Array.IndexOf(obj, null);
                        if (obj[changeid - 1].IsSameOrNull(value))
                        {
                            if (changeid >= EditLength)
                            {
                                obj[EditLength] = value; Array.Reverse(obj); Array.Resize(ref obj, EditLength);
                                Array.Reverse(obj); Array.Resize(ref obj, EditLength + 1);
                            };
                            obj[changeid - 1] = value;
                            changes[position] = obj;
                            waschange = true;
                        }
                    }
                    else if (!_obj.IsSameOrNull(value))
                    {
                        changes[position] = value;
                        waschange = true;
                    }

                }
            if (waschange)
            {
                if (Tiers.Trell.IsEditable(index))
                    edited = true;
                return true;
            }
            else
                return false;
        }
        public bool Put(int tierindex, int index, object value)
        {
            int position = index + (tierindex * PylonsCount);
            bool waschange = false;
            object _obj;
            int[] noidord = new int[] { Tiers.Trell.NoidOrdinal, Tiers.Trell.QuidOrdinal };         
            if (!noidord.Contains(index))
                if (!changes.TryGet(position, out _obj) || cancel)
                {
                    if (!Tiers[tierindex].iN[index].IsSameOrNull(value))
                    {
                        if (EditLength > 1)
                        {
                            single = false;
                            object[] obj = new object[EditLength + 1];
                            obj[0] = value;
                            changes.Add(position, obj);
                        }
                        else
                        {
                            single = true;
                            changes.Add(position, value);
                        }
                        waschange = true;
                    }
                }
                else
                {
                    if (!single)
                    {
                        object[] obj = (object[])_obj;
                        int changeid = Array.IndexOf(obj, null);
                        if (changeid >= EditLength)
                        {
                            obj[EditLength] = value; Array.Reverse(obj); Array.Resize(ref obj, EditLength);
                            Array.Reverse(obj); Array.Resize(ref obj, EditLength + 1);
                        };
                        obj[changeid - 1] = value;
                        changes[position] = obj;
                        waschange = true;
                    }
                    else if (!_obj.IsSameOrNull(value))
                    {
                        changes[position] = value;
                        waschange = true;
                    }
                }
            if (waschange)
            {
                if (Tiers.Trell.IsEditable(index))
                    edited = true;
                return true;
            }
            return false;
        }

        public bool Add(int tierindex, IDataCells cubetier, int cubeordinal, int index, object value)
        {
            int position = index + (tierindex * PylonsCount);
            bool waschange = false;
            object _obj;
            int[] noidord = new int[] { cubetier.NoidOrdinal, cubetier.QuidOrdinal };
            if (!noidord.Contains(index))
                if (!changes.TryGet(position, out _obj) || cancel)
                {
                    if (!cubetier[cubeordinal].IsSameOrNull(value))
                    {
                        if (EditLength > 1)
                        {
                            single = false;
                            object[] obj = new object[EditLength + 1];
                            obj[0] = value;
                            changes.Add(position, obj);
                        }
                        else
                        {
                            single = true;
                            changes.Add(position, value);
                        }
                        waschange = true;
                    }
                }
                else
                {
                    if (!single)
                    {
                        object[] obj = (object[])_obj;
                        int changeid = Array.IndexOf(obj, null);
                        if (obj[changeid - 1].Equals(value))
                        {
                            if (changeid >= EditLength)
                            {
                                obj[EditLength] = value; Array.Reverse(obj); Array.Resize(ref obj, EditLength);
                                Array.Reverse(obj); Array.Resize(ref obj, EditLength + 1);
                            };
                            obj[changeid - 1] = value;
                            changes[position] = obj;
                            waschange = true;
                        }
                    }
                    else if (!_obj.IsSameOrNull(value))
                    {
                        changes[position] = value;
                        waschange = true;
                    }

                }
            if (waschange)
            {
                if (cubetier.IsEditable(index))
                    edited = true;
                return true;
            }
            else
                return false;
        }
        public bool Put(int tierindex, IDataCells cubetier, int cubeordinal, int index, object value)
        {
            int position = index + (tierindex * PylonsCount);
            bool waschange = false;
            object _obj;
            int[] noidord = new int[] { cubetier.NoidOrdinal, cubetier.QuidOrdinal };
            if (!noidord.Contains(index))
                if (!changes.TryGet(position, out _obj) || cancel)
                {
                    if (!cubetier[cubeordinal].IsSameOrNull(value))
                    {
                        if (EditLength > 1)
                        {
                            single = false;
                            object[] obj = new object[EditLength + 1];
                            obj[0] = value;
                            changes.Add(position, obj);
                        }
                        else
                        {
                            single = true;
                            changes.Add(position, value);
                        }
                        waschange = true;
                    }
                }
                else
                {
                    if (!single)
                    {
                        object[] obj = (object[])_obj;
                        int changeid = Array.IndexOf(obj, null);
                        if (changeid >= EditLength)
                        {
                            obj[EditLength] = value; Array.Reverse(obj); Array.Resize(ref obj, EditLength);
                            Array.Reverse(obj); Array.Resize(ref obj, EditLength + 1);
                        };
                        obj[changeid - 1] = value;
                        changes[position] = obj;
                        waschange = true;
                    }
                    else if (!_obj.IsSameOrNull(value))
                    {
                        changes[position] = value;
                        waschange = true;
                    }
                }
            if (waschange)
            {
                if (cubetier.IsEditable(index))
                    edited = true;
                return true;
            }
            return false;
        }

        public IDataGrating AddRange(long tierindex, Dictionary<long, object> input)
        {
            int pylcount = PylonsCount;
            foreach (KeyValuePair<long, object> vessel in input)
            {
                changes.Add(vessel.Key + (tierindex * pylcount), vessel.Value);
            }
            return this;
        }
        public IDataGrating AddRange(long tierindex, object[][] input)
        {
            int pylcount = PylonsCount;
            foreach (object[] item in input)
                changes.Add((int)item[0] + (tierindex * pylcount), item[1]);
            return this;
        }

        public IDataGrating PutRange(long tierindex, Dictionary<long, object> input)
        {
            int pylcount = PylonsCount;
            foreach (KeyValuePair<long, object> vessel in input)
            {
                changes.Put(vessel.Key + (tierindex * pylcount), vessel.Value);
            }
            // }
            //  else
            //      changes = new SortedList<int, object>(input);
            return this;
        }
        public IDataGrating PutRange(long tierindex, HashList<object> input)
        {
            int pylcount = PylonsCount;
            foreach (Vessel<object> vessel in input)
            {
                vessel.Key += (tierindex * pylcount);
                changes.Put(vessel);
            }
            // }
            //  else
            //      changes = new SortedList<int, object>(input);
            return this;
        }
        public IDataGrating PutRange(long tierindex, object[][] input)
        {
            int pylcount = PylonsCount;
            foreach (object[] item in input)
            {
                changes.Put((tierindex * pylcount) + (int)item[0], item[1]);
            }
            return this;
        }

        public object ChangeOn(int tierindex, object result, int ordinal)
        {
            int x = (tierindex * PylonsCount) + ordinal;
            object _obj;
            if (changes.TryGet(x, out _obj))
            {
                Type t = Tiers.Trell.Pylons[ordinal].DataType;
                if (!single)
                {
                    object[] obj = (object[])_obj;
                    int changeid = Array.IndexOf(obj, null) - Level;
                    object o = obj[(changeid < 0) ? 0 : changeid];
                    if (o.GetType() != t)
                        return Convert.ChangeType(o, t);
                    else
                        return o;
                }
                if (_obj == null && t == typeof(string))
                    return "";
                else if (_obj.GetType() == t)
                    return _obj;
                else
                    return Convert.ChangeType(_obj, t);
            }
            return result;
        }
        public object ChangeOn(int tierindex, int ordinal)
        {
            int x = (tierindex * PylonsCount) + ordinal;
            object _obj;
            if (changes.TryGet(x, out _obj))
            {
                Type t = Tiers.Trell.Pylons.TypeArray[ordinal];
                if (!single)
                {
                    object[] obj = (object[])_obj;
                    int changeid = Array.IndexOf(obj, null) - Level;
                    object o = obj[(changeid < 0) ? 0 : changeid];
                    if (o.GetType() != t)
                        return Convert.ChangeType(o, t);
                    else
                        return o;
                }
                if (_obj == null && t == typeof(string))
                    return "";
                else if (_obj.GetType() == t)
                    return _obj;
                else
                    return Convert.ChangeType(_obj, t);
            }
            return null;
        }

        public int[] EditableList(int tierindex)
        {
            if (changes.Count > 0)
            {
                int pylcount = PylonsCount;
                return Tiers.Trell.Pylons
                    .Where(o => o.Editable && changes.ContainsKey((tierindex * pylcount) + o.Ordinal))
                        .Select(o => o.Ordinal).ToArray();
            }
            return null;
        }

        public void Cancel()
        {
            if (cancel)
                Clear();
        }
        public void Cancel(int tierindex)
        {
            if (cancel)
            {
                int pylcount = PylonsCount;
                Tiers.Trell.Pylons
                    .Select(o => o.Editable ? changes.Remove((tierindex * pylcount) + o.Ordinal) : null).ToArray();
            }
        }

        public void Clear()
        {  
            changes.Clear();         
        }
        public void Clear(int tierindex)
        {
            if (cancel)
            {
                int pylcount = PylonsCount;
                Tiers.Trell.Pylons.Select(o => o.Editable ? changes.Remove((tierindex * pylcount) + o.Ordinal) : null).ToArray();
            }
        }
    }
}