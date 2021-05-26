using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Doors;
using System.Doors.Data;

namespace System.Doors.Data.Depot
{
    public class DepotTreatment : IDisposable
    {
        public  IDepotContext Context;
        private DepotTransaction transaction;
        private TransactionContext context;
        private DepotSite site;

        public DepotTreatment(DepotTransaction _transaction)
        {
            transaction = _transaction;
            Context = transaction.Context;
            context = transaction.MyHeader.Context;
            site = context.IdentitySite;
        }
       
        public bool Assign(object _content, DirectionType _direction, out object[] messages)
        {
            messages = null;
            if (((IDataConfig)_content).State != null)
            {
                TreatmentMethod treatmentMethod = new TreatmentMethod(_content, _direction, transaction);
                treatmentMethod.Resolve(out messages);

                if (messages != null)
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public void Dispose()
        {
            if (Context != null)
                Context.Dispose();
        }
    }

    public class TreatmentMethod : IDisposable
    {
        [NonSerialized]
        public IDepotContext context;
        private TransactionContext transContext;
        private DepotSite site;
        private DirectionType direction;
        private IDataSerial content;
        private DataState state;        

        public TreatmentMethod(object _content)
        {                     
            site = DepotSite.Server;
            direction =  DirectionType.None;
            state = ((IDataConfig)_content).State;
            content = (IDataSerial)_content;
        }
        public TreatmentMethod(object _content, DirectionType _direction, DepotTransaction transaction)
        {
            context = transaction.Context;
            transContext = transaction.MyHeader.Context;
            site = transContext.IdentitySite;
            direction = _direction;
            state = ((IDataConfig)_content).State;
            content = (IDataSerial)_content;
        }

        public void Resolve(out object[] messages)
        {
            messages = null;
            switch (site)
            {
                case DepotSite.Server:
                    switch (direction)
                    {
                        case DirectionType.Receive:
                            messages = content.GetMessage();
                            break;
                        case DirectionType.Send:
                            switch (state.Synced)
                            {
                                case false:
                                    SrvSendSync(out messages);
                                    break;
                            }
                            break;
                        case DirectionType.None:
                            switch (state.Synced)
                            {
                                case false:
                                    SrvSendSync(out messages);
                                    break;
                            }
                            break;
                    }
                    break;
                case DepotSite.Client:
                    switch (direction)
                    {
                        case DirectionType.Receive:
                            messages = content.GetMessage();
                            break;
                        case DirectionType.Send:
                            switch (state.Synced)
                            {
                                case false:
                                    CltSendSync(out messages);
                                    break;
                            }
                            break;
                    }
                    break;
            }
        }

        private void CltSendSync(out object[] messages)
        {
            if(direction != DirectionType.None)
            if (state.Edited || state.Saved ||
                state.Quered || state.Canceled)
            {
                context.Synchronic = true;
                transContext.Synchronic = true;
            }

            object[] messages_ = ((IDataTreatment[])content.GetMessage());
            messages = null;
            if (messages_ != null)
            {
                messages = ((IDataTiers[])messages_)
                                .Select(t => 
                                    new DataTiers(t.Trell, 
                                                  t.AsArray().Where(p => !p.Synced).ToArray(), 
                                                  DataModes.TiersView)).ToArray();
            }
        }

        private void SrvSendSync(out object[] messages)
        {
            if (direction != DirectionType.None)
                if (state.Edited || state.Saved ||
                state.Quered || state.Canceled)
                {
                    context.Synchronic = true;
                    transContext.Synchronic = true;
                }

            messages = null;          
            switch (state.Edited)
            {
                case true:
                    SrvSendEdit(out messages);
                    break;
            }
            switch (state.Canceled)
            {
                case true:
                    SrvSendCancel(out messages);
                    break;
            }
            switch (state.Saved)
            {
                case true:
                    SrvSendSave(out messages);
                    break;
            }
            switch (state.Quered)
            {
                case true:
                    SrvSendQuery(out messages);
                    break;
            }
            if (messages == null)
            {
                IDataTreatment[] preMessages = ((IDataTreatment[])content.GetMessage());
                if (preMessages != null)
                {
                    preMessages = preMessages.Select(t => (!t.Trell.Synced) ? (t.Trell.IsPrime) ?
                                                            t.Tiers :
                                                            t.TiersView :
                                                            new DataTiers(t.Trell,
                                                            (t.Mode.IsTiersMode()) ?
                                                            DataModes.TiersView : DataModes.SimsView)).ToArray();

                    preMessages.Select(t => t.Self.Compute(ComputeMode.Mattab)).ToArray();

                    messages = preMessages.Select(t => t.Tiers.SubTotal()).ToArray();
                }
            }
        }

        private void SrvSendEdit(out object[] messages)
        {
            IDataTreatment[] preMessages = ((IDataTreatment[])content.GetMessage())
                                            .Select(t => new DataTiers(t.Trell, t.Tiers.AsEnumerable().Where(r => 
                                                                       r.State.Edited &&
                                                                       !r.State.Synced).ToArray(),
                                                                       (t.Mode == DataModes.Tiers || t.Mode == DataModes.TiersView) ?
                                                                    DataModes.TiersView : DataModes.SimsView)).ToArray();
                                messages = preMessages.Select(t => t.Self.Compute(ComputeMode.Mattab)).ToArray();
        }
        private void SrvSendSave(out object[] messages)
        {          
            IDataTreatment[] treatmentarray = (IDataTreatment[])content.GetMessage();
            messages = null;
            List<DataTiers> tierslist = new List<DataTiers>();
            if (treatmentarray != null)
                foreach (IDataTreatment treatment in treatmentarray)
                {
                    DataTier[] tarray = treatment.Tiers.SaveChanges();
                    DataTiers trs = new DataTiers(treatment.Trell, tarray, (treatment.Mode == DataModes.Tiers || treatment.Mode == DataModes.TiersView) ?
                                                  DataModes.TiersView : DataModes.SimsView);
                    tierslist.Add(trs);
                }                             
                messages = tierslist.ToArray();            
        }
        private void SrvSendCancel(out object[] messages)
        {
            IDataTreatment[] treatmentarray = (IDataTreatment[])content.GetMessage();
            messages = null;
            List<DataTiers> tierslist = new List<DataTiers>();
            if (treatmentarray != null)
                foreach (IDataTreatment treatment in treatmentarray)
                {
                    DataTier[] tarray = treatment.Tiers.ClearChanges();
                    DataTiers trs = new DataTiers(treatment.Trell, tarray, (treatment.Mode == DataModes.Tiers || treatment.Mode == DataModes.TiersView) ?
                                                  DataModes.TiersView : DataModes.SimsView);
                    tierslist.Add(trs);
                }
            messages = tierslist.ToArray();
        }
        private void SrvSendQuery(out object[] messages)
        {
            IDataTreatment[] preMessages = ((IDataTreatment[])content.GetMessage());
            //int length = preMessages.Length;
            //bool[] sorted = new bool[length * 2], filtered = new bool[length * 2];

            //messages = preMessages.Select((t, y) => t.Tiers.Query(out sorted[y], out filtered[y], 1)).ToArray();

            //if (filtered.Where(f => f).Any())
            //{
            //    preMessages.Select(t => t.Tiers.SubTotal()).ToArray();
            //}

            //messages = preMessages.Select((t, y) => t.Tiers.Query(out sorted[y + length], out filtered[y + length], 2)).ToArray();

            preMessages.Select((t, y) => t.Tiers.Query(1)).ToArray();

            preMessages.Select(t => t.Tiers.SubTotal()).ToArray();

            messages = preMessages.Select((t, y) => t.Tiers.Query(2)).ToArray();

            preMessages.Select(t => t.State.Quered = false).ToArray();
        }

        public void Dispose()
        {
        }
    }
}
