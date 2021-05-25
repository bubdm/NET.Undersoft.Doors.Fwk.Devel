using System;
using System.Threading;
using System.Dors;
using System.Dors.Data;

namespace System.Dors.Data.Depot
{
    public class DepotConnection : IDepotSync
    {
        private readonly ManualResetEvent completeNotice = new ManualResetEvent(false);
        private bool isAsync = true;

        private DepotClient Client
        { get; set; }

        private IDorsEvent connected;
        private IDorsEvent headerSent;
        private IDorsEvent messageSent;
        private IDorsEvent headerReceived;
        private IDorsEvent messageReceived;

        public  IDorsEvent CompleteEvent;
        public  IDorsEvent EchoEvent;

        public  IDepotContext Context
        { get; set; }

        public  DepotTransaction Transaction
        { get; set; }

        public  object Content
        { get { return Transaction.MyHeader.Content; } set { Transaction.MyHeader.Content = value; } }

        public DepotConnection(DepotIdentity ClientIdentity, IDorsEvent OnEchoEvent = null, IDorsEvent OnCompleteEvent = null)
        {         
            DepotIdentity ci = ClientIdentity;
            ci.Site = DepotSite.Client;
            DepotClient client = new DepotClient(ci);
            Transaction = new DepotTransaction(ci);

            connected = new DepotEvent("Connected", this);
            headerSent = new DepotEvent("HeaderSent", this);
            messageSent = new DepotEvent("MessageSent", this);
            headerReceived = new DepotEvent("HeaderReceived", this);
            messageReceived = new DepotEvent("MessageReceived", this);

            client.Connected = connected;
            client.HeaderSent = headerSent;
            client.MessageSent = messageSent;
            client.HeaderReceived = headerReceived;
            client.MessageReceived = messageReceived;

            CompleteEvent = OnCompleteEvent;
            EchoEvent = OnEchoEvent;

            Client = client;

            WriteEcho("Client Connection Created");
        }

        public object Initiate(bool IsAsync = true)
        {
            isAsync = IsAsync;
            Client.Connect();
            if (!isAsync)
            {
                completeNotice.WaitOne();
                return Context;
            }
            return null;
        }

        public object Connected(object idepotclient)
        {
            WriteEcho("Client Connection Established");
            Transaction.MyHeader.Context.Echo = "Client say Hello. ";
            Context = Client.Context;
            Client.Context.Transaction = Transaction;

            IDepotClient idc = (IDepotClient)idepotclient;

            idc.Send(MessagePart.Header, false);

           return idc.Context;
        }

        public object HeaderSent(object idepotclient)
        {
            WriteEcho("Client header sent");
            IDepotClient idc = (IDepotClient)idepotclient;
            if (!idc.Context.Synchronic)
                idc.Receive(MessagePart.Header);
            else
                idc.Send(MessagePart.Message, false);

           return idc.Context;
        }
        public object HeaderReceived(object idepotclient)
        {
            string serverEcho = Transaction.HeaderReceived.Context.Echo;
            WriteEcho(string.Format("Server header received"));
            if (serverEcho != null && serverEcho != "")
                WriteEcho(string.Format("Server echo: {0}", serverEcho));

            IDepotClient idc = (IDepotClient)idepotclient;

            if (idc.Context.Close)
                idc.Dispose();
            else
            {
                if (!idc.Context.Synchronic)
                {
                    if (idc.Context.SendMessage)
                        idc.Send(MessagePart.Message, false);
                }
                if (idc.Context.ReceiveMessage)
                    idc.Receive(MessagePart.Message);
            }

            if (!idc.Context.ReceiveMessage &&
                !idc.Context.SendMessage)
            {
                if(CompleteEvent != null)
                    CompleteEvent.Execute(idc.Context);
                if (!isAsync)
                    completeNotice.Set();
            }
            return idc.Context;            
        }

        public object MessageSent(object idepotclient)
        {
            WriteEcho("Client message sent");

            IDepotClient idc = (IDepotClient)idepotclient;
            if (idc.Context.Synchronic)
                idc.Receive(MessagePart.Header);
       
            if (!idc.Context.ReceiveMessage)
            {
                if (CompleteEvent != null)
                    CompleteEvent.Execute(idc.Context);
                if (!isAsync)
                    completeNotice.Set();
            }
            return idc.Context;
        }
        public object MessageReceived(object idepotclient)
        {
            WriteEcho(string.Format("Server message received"));

            IDepotContext context = ((IDepotClient)idepotclient).Context;
            if (context.Close)
                ((IDepotClient)idepotclient).Dispose();

            if (CompleteEvent != null)
                CompleteEvent.Execute(context);
            if(!isAsync)
                completeNotice.Set();
            return context;
        }

        private void WriteEcho(string message)
        {
            if (EchoEvent != null)
                EchoEvent.Execute(message);
        }

        public void SetCallback(string methodName, object classObject)
        {
            CompleteEvent = new DepotEvent(methodName, classObject);
        }
        public void SetCallback(IDorsEvent OnCompleteEvent)
        {
            CompleteEvent = OnCompleteEvent;
        }

        public void Reconnect()
        {
            DepotIdentity ci = new DepotIdentity() { AuthId = Client.Identity.AuthId,
                                                       Site = DepotSite.Client,
                                                       Mode = ServiceMode.Console,
                                                       Name = Client.Identity.Name,
                                                       Token = Client.Identity.Token,
                                                       UserId = Client.Identity.UserId,
                                                       DeptId = Client.Identity.DeptId,
                                                       DataPlace = Client.Identity.DataPlace,
                                                         Id = Client.Identity.Id,
                                                         Ip = Client.EndPoint.Address.ToString(),
                                                       Port = Client.EndPoint.Port,
                                                        Key = Client.Identity.Key };
            Transaction.Dispose();            
            DepotClient client = new DepotClient(ci);
            Transaction = new DepotTransaction(ci);
            client.Connected = connected;
            client.HeaderSent = headerSent;
            client.MessageSent = messageSent;
            client.HeaderReceived = headerReceived;
            client.MessageReceived = messageReceived;
            Client = client;
        }

        public void Close()
        {
            Client.Dispose();
        }
    }
}
