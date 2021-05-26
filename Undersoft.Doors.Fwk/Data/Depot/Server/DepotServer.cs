using System;
using System.Threading;
using System.Doors;

namespace System.Doors.Data.Depot
{
    public class DepotServer 
    {
        private IDepotSocketEars server;

        public void Start(DepotIdentity ServerIdentity, IDorsEvent OnEchoEvent = null)
        {            
            server = DepotSocketEars.Instance;
            server.Identity = ServerIdentity;

            new Thread(new ThreadStart(server.OpenEars)).Start();

            server.HeaderSent = new DepotEvent("HeaderSent", this);
            server.MessageSent = new DepotEvent("MessageSent", this);
            server.HeaderReceived = new DepotEvent("HeaderReceived", this);
            server.MessageReceived = new DepotEvent("MessageReceived", this);
            server.SendEcho = OnEchoEvent;        

            WriteEcho("Data Depot instance started");
        }

        public object HeaderReceived(object idepotcontext)
        {
            string clientEcho = ((IDepotContext)idepotcontext).Transaction.HeaderReceived.Context.Echo;
            WriteEcho(string.Format("Client header received"));
            if(clientEcho != null && clientEcho != "")
                WriteEcho(string.Format("Client echo: {0}", clientEcho));

            TransactionContext trctx = ((IDepotContext)idepotcontext).Transaction.MyHeader.Context;
            if (trctx.Echo == null || trctx.Echo == "")
                trctx.Echo = "Server say Hello";
            if(!((IDepotContext)idepotcontext).Synchronic)
                server.Send(MessagePart.Header, ((IDepotContext)idepotcontext).Id, false);
            else
                server.Receive(MessagePart.Message, ((IDepotContext)idepotcontext).Id);

            return ((IDepotContext)idepotcontext);
         }
        public object HeaderSent(object idepotcontext)
        {

            WriteEcho("Server header sent");

            IDepotContext context = (IDepotContext)idepotcontext;
            if (context.Close)
            {
                context.Transaction.Dispose();
                server.CloseClient(context.Id);
            }
            else
            {              
                if (!context.Synchronic)
                {
                    if (context.ReceiveMessage)
                        server.Receive(MessagePart.Message, context.Id);
                }
                if (context.SendMessage)
                    server.Send(MessagePart.Message, context.Id, false);
            }
            return context;
        }

        public object MessageReceived(object idepotcontext)
        {
            WriteEcho(string.Format("Client message received"));
            if (((IDepotContext)idepotcontext).Synchronic)
                server.Send(MessagePart.Header, ((IDepotContext)idepotcontext).Id, false);
           return (IDepotContext)idepotcontext;
        }
        public object MessageSent(object idepotcontext)
        {
            WriteEcho("Server message sent");
            IDepotContext result = (IDepotContext)idepotcontext;
            if (result.Close)
            {
                result.Transaction.Dispose();                
                server.CloseClient(result.Id);
            }
            return result;
        }

        public void ClearResources()
        {
            WriteEcho("Resource buffer cleaned");
            if(server != null)
             server.ClearResources();
        }
        public void ClearClients()
        {
            WriteEcho("Client registry cleaned");
            if (server != null)
                server.ClearClients();

        }

        public void Close()
        {
            if (server != null)
            {
                WriteEcho("Server instance shutdown ");
                server.CloseEars();
                server = null;
            }
            else
            {
                WriteEcho("Server instance doesn't exist ");
            }
        }

        public bool IsActive()
        {

            if (server != null)
            {
                WriteEcho("Server Instance Is Active");
                return true;
            }
            else
            {
                WriteEcho("Server Instance Doesn't Exist");
                return false;
            }
        }

        public void WriteEcho(string message)
        {
            if(server != null)
                server.Echo(message);
        }

    }
}
