using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Dors;

namespace System.Dors.Data.Depot
{
    public sealed class DepotClient : IDepotClient
    {
        private Socket socketEar;
        private ushort port;
        private IPAddress ip;
        private IPHostEntry host;
        private int timeout = 50;

        private readonly ManualResetEvent connectNotice = new ManualResetEvent(false);

        private IDepotContext context;
        public IDepotContext Context
        { get { return context; } set { context = value; } }       

        public IDorsEvent Connected { get; set; }
        public IDorsEvent HeaderSent { get; set; }
        public IDorsEvent MessageSent { get; set; }
        public IDorsEvent HeaderReceived { get; set; }
        public IDorsEvent MessageReceived { get; set; }

        private DepotIdentity identity;
        public  DepotIdentity Identity
        {
            get
            {
                return (identity != null) ?
                                 identity :
                                 identity = new DepotIdentity()
                                 {
                                     Id = 0,
                                     Ip = "127.0.0.1",
                                     Host = "localhost",
                                     Port = 44004,
                                     Limit = 0,
                                     Scale = 0,
                                     Site = DepotSite.Client
                                 };
            }
            set
            {
                if (value != null)
                {
                    value.Site = DepotSite.Client;
                    identity = value;
                }
            }
        }

        public  IPEndPoint EndPoint;

        public DepotClient(DepotIdentity ConnectionIdentity)
        {
            Identity = ConnectionIdentity;

            if(Identity.Ip == null || Identity.Ip == "")
                Identity.Ip = "127.0.0.1";
            ip =   IPAddress.Parse(Identity.Ip);
            port = Convert.ToUInt16(Identity.Port);
            host = Dns.GetHostEntry((Identity.Ip != null &&
                                     Identity.Ip != string.Empty) ?
                                     Identity.Ip :
                                     string.Empty);
            
            EndPoint = new IPEndPoint(ip, port);
        }

        public void Connect()
        {
                   ushort _port = port;
                string hostname = host.HostName;
                  IPAddress _ip = ip;
            IPEndPoint endpoint = new IPEndPoint(_ip, _port);            

            try
            {
                socketEar = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                context = new DepotContext(socketEar);
                socketEar.BeginConnect(endpoint, OnConnectCallback, context);
                connectNotice.WaitOne();

                Connected.Execute(this);
            }
            catch (SocketException ex)
            { }
        }

        public bool IsConnected()
        {
            if(socketEar != null && socketEar.Connected)
                return !(socketEar.Poll(timeout * 1000, SelectMode.SelectRead) && socketEar.Available == 0);
            return true;
        }

        private void OnConnectCallback(IAsyncResult result)
        {
            IDepotContext context = (IDepotContext)result.AsyncState;        

            try
            {
                context.SocketEar.EndConnect(result);
                connectNotice.Set();
            }
            catch (SocketException ex)
            {
            }
        }

        public void Receive(MessagePart messagePart)
        {
            AsyncCallback callback = HeaderReceivedCallBack;
            if (messagePart != MessagePart.Header && context.ReceiveMessage)
            {
                callback = MessageReceivedCallBack;
                context.ObjectsLeft = context.Transaction.HeaderReceived.Context.ObjectsCount;
                context.SocketEar.BeginReceive(context.MessageBuffer, 0, context.BufferSize, SocketFlags.None, callback, context);
            }
            else
                context.SocketEar.BeginReceive(context.HeaderBuffer, 0, context.BufferSize, SocketFlags.None, callback, context);
        }
        public void Send(MessagePart messagePart, bool _close)
        {
            if (!IsConnected())
                throw new Exception("Destination socket is not connected."); 
            AsyncCallback callback = HeaderSentCallback;
            if (messagePart == MessagePart.Header)
            {
                callback = HeaderSentCallback;
                TransactionMethod request = new TransactionMethod(Context.Transaction, MessagePart.Header, DirectionType.Send);
                request.Resolve();
            }
            else if (Context.SendMessage)
            {
                callback = MessageSentCallback;
                context.SerialBlockId = 0;
                TransactionMethod request = new TransactionMethod(context.Transaction, MessagePart.Message, DirectionType.Send);
                request.Resolve();
            }
            else
                return;
            context.SocketEar.BeginSend(context.SerialBlock, 0, context.SerialBlock.Length, SocketFlags.None, callback, context);
        }

        private void MessageReceivedCallBack(IAsyncResult result)
        {
            IDepotContext context = (IDepotContext)result.AsyncState;
            NoiseType noiseKind = NoiseType.None;

            int receive = context.SocketEar.EndReceive(result);

            if (receive > 0)
                noiseKind = context.IncomingMessage(receive);

            if (context.BlockSize > 0)
            {
                int buffersize = (context.BlockSize < context.BufferSize) ? (int)context.BlockSize : context.BufferSize;
                context.SocketEar.BeginReceive(context.MessageBuffer, 0, buffersize, SocketFlags.None, MessageReceivedCallBack, context);
            }
            else
            {
                object received = context.DeserialBlock;
                object readPosition = context.DeserialBlockId;

                if (noiseKind == NoiseType.Block || (noiseKind == NoiseType.End && (int)readPosition < (context.Transaction.HeaderReceived.Context.ObjectsCount - 1)))
                    context.SocketEar.BeginReceive(context.MessageBuffer, 0, context.BufferSize, SocketFlags.None, MessageReceivedCallBack, context);

                TransactionMethod request = new TransactionMethod(context.Transaction, MessagePart.Message, DirectionType.Receive);
                request.Resolve(received, readPosition);

                if (context.ObjectsLeft <= 0 && !context.BatchesReceivedNotice.SafeWaitHandle.IsClosed)
                    context.BatchesReceivedNotice.Set();

                if (noiseKind == NoiseType.End && (int)readPosition >= (context.Transaction.HeaderReceived.Context.ObjectsCount - 1))
                {
                    context.BatchesReceivedNotice.WaitOne();

                    if (context.SendMessage)          
                        context.MessageSentNotice.WaitOne();
                    
                    context.Close = true;

                    context.MessageReceivedNotice.Set();
                    MessageReceived.Execute(this);
                }
            }
        }
        private void MessageSentCallback(IAsyncResult result)
        {
            IDepotContext context = (IDepotContext)result.AsyncState;
            try
            {
                int sendcount = context.SocketEar.EndSend(result);               
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }

            if (context.SerialBlockId >= 0)
            {
                TransactionMethod request = new TransactionMethod(context.Transaction, MessagePart.Message, DirectionType.Send);
                request.Resolve();
                context.SocketEar.BeginSend(context.SerialBlock, 0, context.SerialBlock.Length, SocketFlags.None, MessageSentCallback, context);
            }
            else
            {
                if (!context.ReceiveMessage)
                    context.Close = true;

                context.MessageSentNotice.Set();
                MessageSent.Execute(this);
            }
        }

        private void HeaderReceivedCallBack(IAsyncResult result)
        {
            IDepotContext context = (IDepotContext)result.AsyncState;
            int receive = context.SocketEar.EndReceive(result);

            if (receive > 0)
                context.IncomingHeader(receive);

            if (context.BlockSize > 0)
            {
                int buffersize = (context.BlockSize < context.BufferSize) ? (int)context.BlockSize : context.BufferSize;
                context.SocketEar.BeginReceive(context.HeaderBuffer, 0, buffersize, SocketFlags.None, HeaderReceivedCallBack, context);
            }
            else
            {
                TransactionMethod request = new TransactionMethod(context.Transaction, MessagePart.Header, DirectionType.Receive);
                request.Resolve(context.DeserialBlock);

                if (!context.ReceiveMessage &&
                    !context.SendMessage)
                    context.Close = true;

                context.HeaderReceivedNotice.Set();
                HeaderReceived.Execute(this);
            }
        }
        private void HeaderSentCallback(IAsyncResult result)
        {
            IDepotContext context = (IDepotContext)result.AsyncState;
            try
            {
                int sendcount = context.SocketEar.EndSend(result);
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }

            context.HeaderSentNotice.Set();
            HeaderSent.Execute(this);
        }

        private void Close()
        {
            try
            {
                if (!IsConnected())
                {
                    context.Dispose();
                    return;
                }
                if (socketEar != null && socketEar.Connected)
                {
                    socketEar.Shutdown(SocketShutdown.Both);
                    socketEar.Close();
                }
                context.Dispose();
            }
            catch (SocketException)
            {
                // 4U2DO
            }
        }

        public void Dispose()
        {
            connectNotice.Dispose();
            Close();
        }      
    }
}
