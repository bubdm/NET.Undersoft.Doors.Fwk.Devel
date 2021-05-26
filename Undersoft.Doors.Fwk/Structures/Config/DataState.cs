using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace System.Doors
{
    [JsonObject]
    [Serializable]
    public class DataState
    {
        #region Private Get / Set implementation
        private bool check = false;
        private bool quered = false;
        private bool saved = false;
        private bool canceled = false;
        private bool synced = false;
        private bool expeled = false;
        private bool edited = false;
        private bool deleted = false;
        private bool added = false;
        private long clock = 0;
        public  bool withPropagate = true;
        #endregion

        public DataState propagate;

        public DataState()
        { }
        public DataState(IDataConfig _propagate)
        {
            propagate = _propagate.State;
        }

        public long Clock
        {
            get
            {
                return clock;
            }
            set
            {
                if (clock != value)
                {
                    clock = value;
                    if (value > clock)                        
                        synced = false;
                    if (propagate != null && 
                        withPropagate)
                        propagate.Clock = value;
                }
            }
        }

        public bool Checked
        {
            get
            {
                return check;
            }
            set
            {
                if (!value)
                    check = value;
                else
                {
                    check = value;
                    synced = false;
                }
                if (propagate != null &&
                    withPropagate)
                    propagate.Checked = value;
            }
        }
        public bool Quered
        {
            get
            {
                return quered;
            }
            set
            {
                if (!value)
                    quered = value;
                else
                {
                    quered = value;
                    synced = false;
                }
                if (propagate != null &&
                     withPropagate)
                    propagate.Quered = value;
            }
        }
        public bool Saved
        {
            get
            {
                return saved;
            }
            set
            {
                if (!value)
                {
                    //if ((!edited || deleted) && !synced)
                        saved = value;
                }
                else
                {
                    saved = value;
                    synced = false;
                }
                if (propagate != null &&
                    withPropagate)
                    propagate.Saved = value;
            }
        }
        public bool Canceled
        {
            get
            {
                return canceled;
            }
            set
            {

                if (!value)
                {
                    if ((saved || deleted || edited) && !synced)
                        canceled = value;

                }
                else
                {
                        canceled = value;
                        synced = false;
                }
                if (propagate != null &&
                    withPropagate)
                    propagate.Canceled = value;
            }
        }
        public bool Synced
        {
            get
            {
                return synced; }
            set
            {
                synced = value;
                if (propagate != null &&
                    withPropagate)
                    propagate.Synced = value;
            }
        }
        public bool Expeled
        {
            get
            {
                return expeled;
            }
            set
            {
                expeled = value;
                if (propagate != null &&
                    withPropagate)
                    propagate.Expeled = value;
            }
        }
        public bool Edited
        {
            get
            {
                return edited;
            }
            set
            {
                if (!value)
                {
                    if ((saved || deleted || canceled || expeled) && !synced)
                        edited = value;
                }
                else
                {
                        edited = value;
                        synced = false;
                        SetClock();
                }
                if (propagate != null &&
                    withPropagate)
                    propagate.Edited = value;
            }
        }
        public bool Deleted
        {
            get
            {
                return deleted;
            }
            set
            {
                if (!value)
                {
                    if ((!saved && !edited && !canceled && !added) && !synced)
                        deleted = value;
                }
                else
                {
                        deleted = value;
                        synced = false;
                        SetClock();
                }
                if (propagate != null &&
                    withPropagate)
                    propagate.Deleted = value;
            }
        }
        public bool Added
        {
            get
            {
                return added;
            }
            set
            {
                if (!value)
                {
                    if ((saved || deleted || canceled) && !synced)
                        added = value;

                }
                else
                {
                        added = value;
                        synced = false;
                        SetClock();
                }
                if (propagate != null &&
                    withPropagate)
                    propagate.Added = value;
            }
        }

        public void SetClock()
        {
            Clock = DateTime.Now.Ticks;
        }

        public void ClearState()
        {
            expeled = false;
            check = false;
            canceled = false;          
            added = false;
            edited = false;
            quered = false;
            saved = false;        
            synced = false;
            deleted = false;
            if (propagate != null &&
                withPropagate)
                propagate.ClearState();
        }

        public bool AnyState()
        {
            if (check)    return true;
            if (quered)   return true;
            if (saved)    return true;
            if (canceled) return true;         
            if (edited)   return true;
            return false;
        }

        public void Impact(IDataConfig _state, bool propagate = true, bool syncing = false)
        {
            withPropagate = propagate;
            Expeled = _state.State.Expeled;
            Checked = _state.State.Checked;
            Canceled = _state.State.Canceled;         
            Added = _state.State.Added;
            if (!syncing)
                Edited = _state.State.Edited;
            Quered = _state.State.Quered;
            Saved = _state.State.Saved;                                       
            Synced = _state.State.Synced;
            Deleted = _state.State.Deleted;
            withPropagate = true;
        }    
    }

    public static class DataStateTools
    {
        public static void   Impact(this DataState s, DataState _state, bool propagate = true, bool syncing = false)
        {
            s.withPropagate = propagate;
            s.Expeled = _state.Expeled;
            s.Checked = _state.Checked;
            s.Canceled = _state.Canceled;         
            s.Added = _state.Added;
            if(!syncing)
                s.Edited = _state.Edited;
            s.Quered = _state.Quered;
            s.Saved = _state.Saved;          
            s.Synced = _state.Synced;
            s.Deleted = _state.Deleted;
            s.withPropagate = true;
        }       
        public static ushort ToUInt16(this DataState s)
        {
            BitArray bits = s.ToBits();
            byte[] byt = new byte[2];
            bits.CopyTo(byt, 0);
            return BitConverter.ToUInt16(byt, 0);
        }
        public static void   FromBits(this DataState s, Noid noid)
        {
            BitArray bits = noid.GetStateBits();
            s.Expeled = bits[0];
            s.Checked = bits[1];
            s.Canceled = bits[2];
            s.Added = bits[3];
            s.Edited = bits[4];
            s.Quered = bits[5];
            s.Saved = bits[6];
            s.Synced = bits[7];
            s.Deleted = bits[8];
        }
        public static BitArray ToBits(this DataState s, Noid noid)
        {
            BitArray bits = s.ToBits();
            byte[] b = new byte[2];
            bits.CopyTo(b, 0);
            noid[14, 2] = b;
            return bits;
        }
        public static BitArray ToBits(this DataState s)
        {
            BitArray bits = new BitArray(new bool[] { s.Expeled, s.Checked, s.Canceled, s.Added,
                                                      s.Edited, s.Quered, s.Saved, s.Synced, s.Deleted
                                                        });
            return bits;
        }

        public static long NewBitClock(this DataState s, Noid noid)
        {
            s.Clock = DateTime.Now.ToBinary();
            return noid.ClockLongValue = s.Clock;
        }
        public static long ToBitClock(this DataState s, DateTime clock)
        {
            s.Clock = clock.ToBinary();
            return s.Clock;
        }
        public static long ToBitClock(this DataState s, Noid noid)
        {
            return noid.ClockLongValue = s.Clock;
        }
        public static Int64  FromBitClock(this DataState s, Noid noid)
        {
            s.Clock = noid.ClockLongValue;
            return s.Clock;
        }     
    }
}