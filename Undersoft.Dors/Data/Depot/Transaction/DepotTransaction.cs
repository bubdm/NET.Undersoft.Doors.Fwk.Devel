using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Dors;
using System.Dors.Data;

namespace System.Dors.Data.Depot
{
    public class DepotTransaction : IDisposable
    {
        private TransactionMessage mymessage;

        public DepotIdentity Identity;
        public IDepotContext Context;

        public DepotTransaction()
        {
            MyHeader = new TransactionHeader(this);
            Manager = new TransactionManager(this);
            MyMessage = new TransactionMessage(this, DirectionType.Send, null);
        }
        public DepotTransaction(DepotIdentity identity, object message = null, IDepotContext depotContex = null)
        {
            Context = depotContex;
            if (Context != null)
                MyHeader = new TransactionHeader(this, Context, identity);
            else
                MyHeader = new TransactionHeader(this, identity);

            Manager = new TransactionManager(this);
            Identity = identity;
            MyMessage = new TransactionMessage(this, DirectionType.Send, message);
        }
       
        public TransactionManager Manager;

        public TransactionHeader  MyHeader;
        public TransactionMessage MyMessage
        {
            get
            {
                return mymessage;
            }
            set
            {
                mymessage = value;                
            }
        }

        public TransactionHeader  HeaderReceived;
        public TransactionMessage MessageReceived;

        public void Dispose()
        {
            if (MyHeader != null)
                MyHeader.Dispose();
            if (mymessage != null)
                mymessage.Dispose();
            if (HeaderReceived != null)
                HeaderReceived.Dispose();
            if (MessageReceived != null)
                MessageReceived.Dispose();
            if(Context != null)
                Context.Dispose();
        }
    }

   
}
