using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Doors;

namespace System.Doors.Data.Depot
{
    public sealed class DepotSocketEars : IDepotSocketEars
    {
        private static int HASHES_CAPACITY_STANDARD_VALUE = 251;                          // probably there is no need for more then 251 values in hashes, +1 cause 0based count
        private static int MAX_ACCESS_REQUEST_VALUE = Environment.ProcessorCount * 2;     // established max count for access requests to hashes
        private int timeout = 50;

        private bool shutdown = false;

        private readonly ManualResetEvent connectingNotice = new ManualResetEvent(false);

        private readonly ConcurrentDictionary<int, IDepotContext> clients =
                     new ConcurrentDictionary<int, IDepotContext>(
                         MAX_ACCESS_REQUEST_VALUE, HASHES_CAPACITY_STANDARD_VALUE);

        private readonly ConcurrentDictionary<string, byte[]> resources =
                    new ConcurrentDictionary<string, byte[]>(
                        MAX_ACCESS_REQUEST_VALUE, HASHES_CAPACITY_STANDARD_VALUE);

        public IDorsEvent HeaderSent { get; set; }
        public IDorsEvent HeaderReceived { get; set; }
        public IDorsEvent MessageSent { get; set; }
        public IDorsEvent MessageReceived { get; set; }
        public IDorsEvent SendEcho { get; set; }

        public static IDepotSocketEars Instance { get; } = new DepotSocketEars();

        private DepotIdentity identity;
        public DepotIdentity Identity
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
                                     Limit = 250,
                                     Scale = 1,
                                     Site = DepotSite.Server
                                 };
            }
            set
            {
                if (value != null)
                {
                    value.Site = DepotSite.Server;
                    identity = value;
                }
            }
        }

        private DepotSocketEars()
        {
        }

        public void OpenEars()
        {
            ushort port = Convert.ToUInt16(Identity.Port),
                  limit = Convert.ToUInt16(Identity.Limit);
            IPAddress address = IPAddress.Parse(Identity.Ip);
            IPEndPoint endpoint = new IPEndPoint(address, port);
            shutdown = false;
            try
            {
                using (Socket socketEar = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    socketEar.Bind(endpoint);
                    socketEar.Listen(limit);
                    while (!shutdown)
                    {

                        connectingNotice.Reset();
                        socketEar.BeginAccept(OnConnectCallback, socketEar);
                        connectingNotice.WaitOne();
                    }
                }
            }
            catch (SocketException sx)
            {
                Echo(sx.Message);
            }
        }

        private IDepotContext GetClient(int id)
        {
            IDepotContext context;
            return clients.TryGetValue(id, out context) ? context : null;
        }

        public bool IsConnected(int id)
        {
            IDepotContext context = GetClient(id);
            if (context != null && context.SocketEar != null && context.SocketEar.Connected)
                return !(context.SocketEar.Poll(timeout * 1000, SelectMode.SelectRead) && context.SocketEar.Available == 0);
            else
                return false;
        }

        public void OnConnectCallback(IAsyncResult result)
        {
            try
            {
                if (!shutdown)
                {
                    IDepotContext context;
                    int id = -1;
                    id = DateTime.Now.Ticks.ToString().GetHashCode();
                    context = new DepotContext(((Socket)result.AsyncState).EndAccept(result), id);
                    context.Transaction = new DepotTransaction(identity, null, context);
                    context.Resources = resources;
                    while (true)
                    {
                        if (!clients.TryAdd(id, context))
                        {
                            id = DateTime.Now.Ticks.ToString().GetHashCode();
                            context.Id = id;
                        }
                        else
                            break;
                    }
                    Echo("Client connected. Get Id " + id);
                    context.SocketEar.BeginReceive(context.HeaderBuffer, 0, context.BufferSize, SocketFlags.None, HeaderReceivedCallback, clients[id]);
                }
                connectingNotice.Set();
            }
            catch (SocketException sx)
            {
                Echo(sx.Message);
            }
        }

        public void Receive(MessagePart messagePart, int id)
        {
            IDepotContext context = GetClient(id);

            AsyncCallback callback = HeaderReceivedCallback;

            if (messagePart != MessagePart.Header && context.ReceiveMessage)
            {
                callback = MessageReceivedCallback;
                context.ObjectsLeft = context.Transaction.HeaderReceived.Context.ObjectsCount;
                context.SocketEar.BeginReceive(context.MessageBuffer, 0, context.BufferSize, SocketFlags.None, callback, context);
            }
            else
                context.SocketEar.BeginReceive(context.HeaderBuffer, 0, context.BufferSize, SocketFlags.None, callback, context);
        }
        public void Send(MessagePart messagePart, int id, bool _close)
        {
            IDepotContext context = GetClient(id);
            if (!IsConnected(context.Id))
                throw new Exception("Destination socket is not connected.");

            AsyncCallback callback = HeaderSentCallback;

            if (messagePart == MessagePart.Header)
            {
                callback = HeaderSentCallback;
                TransactionMethod request = new TransactionMethod(context.Transaction, MessagePart.Header, DirectionType.Send);
                request.Resolve();
            }
            else if (context.SendMessage)
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

        public void HeaderReceivedCallback(IAsyncResult result)
        {
            IDepotContext context = (IDepotContext)result.AsyncState;
            int receive = context.SocketEar.EndReceive(result);

            if (receive > 0)
                context.IncomingHeader(receive);

            if (context.Protocol == DepotProtocol.QDTP)
                DpotHeaderReceived(context);
            else if (context.Protocol == DepotProtocol.HTTP)
                HttpHeaderReceived(context);
        }
        public void DpotHeaderReceived(IDepotContext context)
        {
            if (context.BlockSize > 0)
            {
                int buffersize = (context.BlockSize < context.BufferSize) ? (int)context.BlockSize : context.BufferSize;
                context.SocketEar.BeginReceive(context.HeaderBuffer, 0, buffersize, SocketFlags.None, HeaderReceivedCallback, context);
            }
            else
            {
                TransactionMethod request = new TransactionMethod(context.Transaction, MessagePart.Header, DirectionType.Receive);
                request.Resolve(context.DeserialBlock);

                context.HeaderReceivedNotice.Set();

                try
                {
                    HeaderReceived.Execute(context);
                }
                catch (Exception ex)
                {
                    Echo(ex.Message);
                    CloseClient(context.Id);
                }
            }
        }
        public void HttpHeaderReceived(IDepotContext context)
        {
            if (context.BlockSize > 0)
            {
                context.SocketEar.BeginReceive(context.HeaderBuffer, 0, context.BufferSize, SocketFlags.None, HeaderReceivedCallback, context);
            }
            else
            {
                TransactionMethod request = new TransactionMethod(context.Transaction, MessagePart.Header, DirectionType.Receive);
                request.Resolve(context.DeserialBlock);

                context.HeaderReceivedNotice.Set();

                try
                {
                    HeaderReceived.Execute(context);
                }
                catch (Exception ex)
                {
                    Echo(ex.Message);
                    CloseClient(context.Id);
                }
            }
        }

        public void HeaderSentCallback(IAsyncResult result)
        {
            IDepotContext context = (IDepotContext)result.AsyncState;
            try
            {
                int sendcount = context.SocketEar.EndSend(result);
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }

            if (!context.ReceiveMessage && !context.SendMessage)
            {
                //int _timeout = 0;
                //while (IsConnected(context.Id) && timeout < 10) _timeout++;
                context.Close = true;
            }

            context.HeaderSentNotice.Set();

            try
            {
                HeaderSent.Execute(context);
            }
            catch (Exception ex)
            {
                Echo(ex.Message);
                CloseClient(context.Id);
            }
        }

        public void MessageReceivedCallback(IAsyncResult result)
        {
            IDepotContext context = (IDepotContext)result.AsyncState;
            NoiseType noiseKind = NoiseType.None;

            int receive = context.SocketEar.EndReceive(result);

            if (receive > 0)
                noiseKind = context.IncomingMessage(receive);

            if (context.BlockSize > 0)
            {
                int buffersize = (context.BlockSize < context.BufferSize) ? (int)context.BlockSize : context.BufferSize;
                context.SocketEar.BeginReceive(context.MessageBuffer, 0, buffersize, SocketFlags.None, MessageReceivedCallback, context);
            }
            else
            {
                object received = context.DeserialBlock;
                object readPosition = context.DeserialBlockId;

                if (noiseKind == NoiseType.Block || (noiseKind == NoiseType.End && (int)readPosition < (context.Transaction.HeaderReceived.Context.ObjectsCount - 1)))
                    context.SocketEar.BeginReceive(context.MessageBuffer, 0, context.BufferSize, SocketFlags.None, MessageReceivedCallback, context);

                TransactionMethod request = new TransactionMethod(context.Transaction, MessagePart.Message, DirectionType.Receive);
                request.Resolve(received, readPosition);

                if (context.ObjectsLeft <= 0 && !context.BatchesReceivedNotice.SafeWaitHandle.IsClosed)
                    context.BatchesReceivedNotice.Set();

                if (noiseKind == NoiseType.End && (int)readPosition >= (context.Transaction.HeaderReceived.Context.ObjectsCount - 1))
                {                 
                    context.BatchesReceivedNotice.WaitOne();
                    context.MessageReceivedNotice.Set();

                    try
                    {
                        MessageReceived.Execute(context);
                    }
                    catch (Exception ex)
                    {
                        Echo(ex.Message);
                        CloseClient(context.Id);
                    }
                }
            }
        }
        public void MessageSentCallback(IAsyncResult result)
        {
            IDepotContext context = (IDepotContext)result.AsyncState;
            try
            {
                int sendcount = context.SocketEar.EndSend(result);
            }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }

            if (context.SerialBlockId >= 0 || context.ObjectPosition < (context.Transaction.MyHeader.Context.ObjectsCount - 1))
            {
                TransactionMethod request = new TransactionMethod(context.Transaction, MessagePart.Message, DirectionType.Send);
                request.Resolve();
                context.SocketEar.BeginSend(context.SerialBlock, 0, context.SerialBlock.Length, SocketFlags.None, MessageSentCallback, context);
            }
            else
            {
                if (context.ReceiveMessage)
                    context.MessageReceivedNotice.WaitOne();

                //int _timeout = 0;
                // while (IsConnected(context.Id) && timeout < 10) _timeout++;
                context.Close = true;

                context.MessageSentNotice.Set();

                try
                {
                    MessageSent.Execute(context);
                }
                catch(Exception ex)
                {
                    Echo(ex.Message);
                    CloseClient(context.Id);
                }
            }
        }

        public void ClearResources()
        {
            resources.Clear();
        }
        public void ClearClients()
        {
            foreach (IDepotContext closeContext in clients.Values)
            {
                IDepotContext context = closeContext;

                if (context == null)
                {
                    throw new Exception("Client does not exist.");
                }

                try
                {
                    context.SocketEar.Shutdown(SocketShutdown.Both);
                    context.SocketEar.Close();
                }
                catch (SocketException sx)
                {
                    Echo(sx.Message);
                }
                finally
                {
                    context.Dispose();
                    Echo(string.Format("Client disconnected with Id {0}", context.Id));
                }
            }
            clients.Clear();
        }

        public void CloseClient(int id)
        {
            IDepotContext context = GetClient(id);

            if (context == null)
            {
                Echo(string.Format("Client {0} does not exist.", id));
            }
            else
            {
                try
                {
                    if (context.SocketEar != null && context.SocketEar.Connected)
                    {
                        context.SocketEar.Shutdown(SocketShutdown.Both);
                        context.SocketEar.Close();
                    }
                }
                catch (SocketException sx)
                {
                    Echo(sx.Message);
                }
                finally
                {
                    IDepotContext contextRemoved = null;
                    clients.TryRemove(context.Id, out contextRemoved);
                    contextRemoved.Dispose();
                    Echo(string.Format("Client disconnected with Id {0}", context.Id));
                }
            }
        }
        public void CloseEars()
        {
            foreach (IDepotContext closeContext in clients.Values)
            {
                IDepotContext context = closeContext;

                if (context == null)
                {
                    Echo(string.Format("Client  does not exist."));
                }
                else
                {
                    try
                    {
                        if (context.SocketEar != null && context.SocketEar.Connected)
                        {
                            context.SocketEar.Shutdown(SocketShutdown.Both);
                            context.SocketEar.Close();
                        }
                    }
                    catch (SocketException sx)
                    {
                        Echo(sx.Message);
                    }
                    finally
                    {
                        context.Dispose();
                        Echo(string.Format("Client disconnected with Id {0}", context.Id));
                    }
                }
            }
            clients.Clear();
            shutdown = true;
            connectingNotice.Set();
            GC.Collect();
        }     

        public void Dispose()
        {
            foreach (var id in clients.Keys)
            {
                CloseClient(id);
            }

            connectingNotice.Dispose();
        }

        public void Echo(string message)
        {
            if (SendEcho != null)
                SendEcho.Execute(message);            
        }

      

     
    }
}
