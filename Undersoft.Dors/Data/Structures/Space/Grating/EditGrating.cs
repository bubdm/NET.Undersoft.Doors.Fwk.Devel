using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Dors;

namespace System.Dors.Data
{
    public class EditGrating : IDataGrating, IEnumerable
    {
        public IDataTier  iTier
        { get; set; }    
        public DataState State
        { get { return iTier.State; } }
       
        public short Level
        { get { return iTier.iTrell.EditLevel; } }
        public short EditLength
        { get { return iTier.iTrell.EditLength; } }

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

        public EditGrating(DataTier tier)
        {
            iTier = tier;
            single = iTier.iTrell.EditLength > 1 ? false : true;
            changes = new HashList<object>(8);
        }
        public EditGrating(DataTier tier, HashList<object> input)
        {
            iTier = tier;
            single = iTier.iTrell.EditLength > 1 ? false : true;
            changes = input;
        }
        public EditGrating(DataTier tier, Dictionary<long, object> input)
        {
            iTier = tier;
            single = iTier.iTrell.EditLength > 1 ? false : true;
            changes = tier.Grating.changes;
            changes.Add(input);

        }
        public EditGrating(DataTier tier, object[][] input)
        {
            single = iTier.iTrell.EditLength > 1 ? false : true;
            changes = tier.Grating.changes;
            foreach (object[] item in input)
                changes.Add((int)item[0], item[1]);
        }

        private bool add(int index, object value)
        {
            if (iTier.IsEditable(index))
            {
                Vessel<object> obj = changes.GetVessel(index);
                if (obj == null || cancel)
                {
                    if (!iTier.iN[index].IsSameOrNull(value))
                    {
                        if (!single)
                        {
                            object[] _obj = new object[EditLength + 1];
                            _obj[0] = value;
                            changes.AddNew(index, _obj);
                        }
                        else
                        {
                            changes.AddNew(index, value);
                        }
                        return true;
                    }
                }
                else
                {
                    if (!single)
                    {
                        object[] _obj = (object[])obj.Value;
                        int changeid = Array.IndexOf(_obj, null);
                        if (_obj[changeid - 1].Equals(value))
                        {
                            if (changeid >= EditLength)
                            {
                                _obj[EditLength] = value;
                                Array.Reverse(_obj);
                                Array.Resize(ref _obj, EditLength);
                                Array.Reverse(_obj);
                                Array.Resize(ref _obj, EditLength + 1);
                            };
                            _obj[changeid - 1] = value;
                            obj.Value = _obj;
                            return true;
                        }
                    }
                    else if (!obj.Value.IsSameOrNull(value))
                    {
                        obj.Value = value;
                        return true;
                    }
                }
            }

            return false;
        }
        private bool add(IDataCells cubetier, int cubeordinal, int index, object value)
        {
            if (iTier.IsEditable(index))
            {               
                Vessel<object> obj = changes.GetVessel(index);
                if (obj == null || cancel)
                {
                    if (!cubetier[cubeordinal].IsSameOrNull(value))
                    {
                        if (!single)
                        {
                            object[] _obj = new object[EditLength + 1];
                            _obj[0] = value;
                            changes.AddNew(index, _obj);
                        }
                        else
                        {
                            changes.AddNew(index, value);
                        }
                        return true;
                    }
                }
                else
                {
                    if (!single)
                    {
                        object[] _obj = (object[])obj.Value;
                        int changeid = Array.IndexOf(_obj, null);
                        if (_obj[changeid - 1].Equals(value))
                        {
                            if (changeid >= EditLength)
                            {
                                _obj[EditLength] = value; Array.Reverse(_obj); Array.Resize(ref _obj, EditLength);
                                Array.Reverse(_obj); Array.Resize(ref _obj, EditLength + 1);
                            };
                            _obj[changeid - 1] = value;
                            obj.Value = _obj;
                            return true;
                        }
                    }
                    else if (!obj.Value.IsSameOrNull(value))
                    {
                        obj.Value = value;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool Add(int index, object value)
        {            
            if (add(index, value))
            {
                if (!edited)
                    edited = true;
                return true;
            }
            return false;           
        }
        public bool Add(IDataCells cubetier, int cubeordinal, int index, object value)
        {
            if (add(cubetier, cubeordinal, index, value))
            {
                if (!edited)
                    edited = true;
                return true;
            }
            return false;
        }

        private bool put(int index, object value)
        {
            if (iTier.IsEditable(index))
            {               
                Vessel<object> obj = changes.GetVessel(index);
                if (obj == null || cancel)
                {
                    if (!single)
                    {
                        object[] _obj = new object[EditLength + 1];
                        _obj[0] = value;
                        changes.Put(index, _obj);
                    }
                    else
                    {
                        changes.Put(index, value);
                    }
                    return true;
                }
                else
                {
                    if (!single)
                    {
                        object[] _obj = (object[])obj.Value;
                        int changeid = Array.IndexOf(_obj, null);
                        if (changeid >= EditLength)
                        {
                            _obj[EditLength] = value;
                            Array.Reverse(_obj);
                            Array.Resize(ref _obj, EditLength);
                            Array.Reverse(_obj);
                            Array.Resize(ref _obj, EditLength + 1);
                        };
                        _obj[changeid - 1] = value;
                        obj.Value = _obj;
                        return true;
                    }
                    else
                    {
                        obj.Value = value;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool Put(int index, object value)
        {          
            if (put(index, value))
            {
                if (!edited)
                    edited = true;
                return true;
            }
            return false;
        }
        public bool Put(IDataCells cubetier, int cubeordinal, int index, object value)
        {
            if (put(index, value))
            {
                if (!edited)
                    edited = true;
                return true;
            }
            return false;
        }

        public void AddRange(Vessel<object>[] input)
        {
            changes.Add(input);
            //return true;
        }
        public void AddRange(Dictionary<long, object> input)
        {
            changes.Add(input);
            //return true;
        }
        public void AddRange(object[][] input)
        {
            foreach (object[] item in input)
                changes.Add((int)item[0], item[1]);
            //return true;
        }

        public void PutRange(Vessel<object>[] input)
        {
            changes.Put(input);
            //return true;
        }
        public void PutRange(Dictionary<long, object> input)
        {
            changes.Put(input);
            //return true;
        }
        public void PutRange(object[][] input)
        {
            foreach (object[] item in input)
                changes.Put((int)item[0], item[1]);
            //return true;
        }

        public object ChangeOn(int x)
        {
            if (changes != null && changes.Count > 0)
            {
                object _obj = changes.Get(x);
                if (_obj != null)
                {
                    Type t =iTier.iTrell.iPylons[x].DataType;
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

                    if (_obj.GetType() == t)
                        return _obj;
                    return Convert.ChangeType(_obj, t);
                }
            }
            return null;
        }           

        public IEnumerator GetEnumerator()
        {
           return changes.GetEnumerator();
        }

        public void Cancel()
        {
            if (cancel)
                Clear();
        }

        public void Clear()
        {  
            if (changes != null)
                changes.Clear();         
            changes = null;
        }
    }
}