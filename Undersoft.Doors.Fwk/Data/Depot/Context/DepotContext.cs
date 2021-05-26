using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Doors;
using System.Threading;

namespace System.Doors.Data.Depot
{
    public unsafe sealed class DepotContext : IDepotContext, IDisposable
    {
        #region Private / NonSerialized
        private const int Buffer_Size = 4096;
        private int Block_Offset = 16;

        [NonSerialized] private bool disposed = false;

        [NonSerialized] private byte[] headerbuffer = new byte[Buffer_Size];
        [NonSerialized] private byte[] messagebuffer = new byte[Buffer_Size];

        [NonSerialized] private Socket ear;
        [NonSerialized] private readonly DepotStream instream;
        [NonSerialized] private readonly DepotStream outstream;
        private int id;
        [NonSerialized] private StringBuilder sb = new StringBuilder();
        [NonSerialized] public MemoryStream msReceive;
        [NonSerialized] public MemoryStream msSend;

        [NonSerialized] public object accessReceive = new object();
        [NonSerialized] public object accessSend = new object();

        [NonSerialized] public byte[] binReceive = new byte[0];
        [NonSerialized] public byte[] binSend = new byte[0];

        [NonSerialized] public IntPtr binReceiveHandler;
        [NonSerialized] public IntPtr binSendHandler;

        [NonSerialized] public StringBuilder requestBuilder = new StringBuilder();
        [NonSerialized] public StringBuilder responseBuilder = new StringBuilder();

        [NonSerialized] public Hashtable httpHeaders = new Hashtable();
        [NonSerialized] public Hashtable httpOptions = new Hashtable();

        [NonSerialized] private DepotTransaction tr;

        public ManualResetEvent HeaderSentNotice { get; set; } = new ManualResetEvent(false);
        public ManualResetEvent MessageSentNotice { get; set; } = new ManualResetEvent(false);
        public ManualResetEvent HeaderReceivedNotice { get; set; } = new ManualResetEvent(false);
        public ManualResetEvent MessageReceivedNotice { get; set; } = new ManualResetEvent(false);
        public ManualResetEvent BatchesReceivedNotice { get; set; } = new ManualResetEvent(false);
        #endregion

        public DepotProtocol Protocol { get; set; } = DepotProtocol.NONE;
        public ProtocolMethod Method { get; set; } = ProtocolMethod.NONE;
        [NonSerialized] private IDictionary<string, byte[]> resources;
        public IDictionary<string, byte[]> Resources { get { return resources; } set { resources = value; } }

        public DepotContext(Socket _ear, int _id = -1, bool withStream = false)
        {
            this.ear = _ear;
            this.msReceive = new MemoryStream();
            this.msSend = new MemoryStream();
            if (withStream)
            {
                this.instream = new DepotStream(this.ear);
                this.outstream = new DepotStream(this.ear);
            }
            this.id = _id;
            this.Close = false;
            this.Denied = false;
            this.ObjectPosition = 0;
            this.ObjectsLeft = 0;
            this.DeserialBlockId = 0;
            this.BlockSize = 0;
            this.SendMessage = true;
            this.ReceiveMessage = true;
            this.disposed = true;

            HeaderSentNotice.Reset();
            HeaderReceivedNotice.Reset();
            MessageSentNotice.Reset();
            MessageReceivedNotice.Reset();
            BatchesReceivedNotice.Reset();
        }

        public int Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public bool Close
        { get; set; }
        public bool Denied
        { get; set; }
       
        public bool Synchronic
        { get; set; }
        public bool SendMessage
        { get; set; }
        public bool ReceiveMessage
        { get; set; }

        public int BufferSize
        {
            get
            {
                return Buffer_Size;
            }
        }

        public byte[] HeaderBuffer
        {
            get
            {
                return this.headerbuffer;
            }
        }
        public byte[] MessageBuffer
        {
            get
            {
                return this.messagebuffer;
            }
        }

        public long BlockSize
        { get; set; }
        public int BlockOffset
        {
            get
            {
                return Block_Offset;
            }
            set
            {
                Block_Offset = value;
            }
        }

        public byte[] SerialBlock
        {
            get
            {
                return binSend;
            }
            set
            {
                if (value != null)
                {
                    lock (accessSend)
                    {
                        disposed = false;
                        binSend = value;
                        if (Protocol != DepotProtocol.HTTP)
                        {
                            long size = binSend.Length - BlockOffset;
                            new byte[] { (byte) 'D',
                                     (byte) 'P',
                                     (byte) 'O',
                                     (byte) 'T' }.CopyTo(binSend, 0);
                            BitConverter.GetBytes(size).CopyTo(binSend, 4);
                            BitConverter.GetBytes(ObjectPosition).CopyTo(binSend, 12);
                        }
                        value = null;
                    }
                }
            }
        }
        public int SerialBlockId
        {
            get; set;
        }

        public IntPtr SerialHandler
        {
            get
            {
                return binSendHandler;
            }
        }
        public IntPtr DeserialHandler
        {
            get
            {
                return binReceiveHandler;
            }
        }

        public byte[] DeserialBlock
        {
            get
            {
                byte[] result = null;
                lock (accessReceive)
                {
                    disposed = false;
                    BlockSize = 0;
                    result = binReceive;
                    binReceive = new byte[0];
                }
                return result;
            }
        }
        public int DeserialBlockId
        { get; set; }

        public NoiseType IncomingHeader(int received)
        {
            disposed = false;
            NoiseType noiseKind = NoiseType.None;
            if (Protocol == DepotProtocol.NONE)
                IdentifyProtocol();
            if (Protocol == DepotProtocol.QDTP)
                return QdtpHeader(received);
            else if (Protocol == DepotProtocol.HTTP)
                return HttpHeader(received);
            return noiseKind;
        }
        public NoiseType QdtpHeader(int received)
        {
            NoiseType noiseKind = NoiseType.None;

            lock (accessReceive)
            {
                int offset = 0, length = received;
                bool inprogress = false;
                if (BlockSize == 0)
                {
                    BlockSize = BitConverter.ToInt64(HeaderBuffer, 4);
                    DeserialBlockId = BitConverter.ToInt32(HeaderBuffer, 12);

                    binReceive = new byte[BlockSize];
                    GCHandle gc = GCHandle.Alloc(binReceive, GCHandleType.Pinned);                    
                    binReceiveHandler = GCHandle.ToIntPtr(gc);

                    offset = BlockOffset;
                    length -= BlockOffset;
                }

                if (BlockSize > 0)
                    inprogress = true;

                BlockSize -= length;

                if (BlockSize < 1)
                {
                    long endPosition = length;
                    noiseKind = HeaderBuffer.SeekNoise(out endPosition, SeekDirection.Backward);
                }

                int destid = (binReceive.Length - ((int)BlockSize + length));

                if (inprogress)
                {
                    fixed (void* headbuff = HeaderBuffer)
                    {
                        MemoryNative.Copy(GCHandle.FromIntPtr(binReceiveHandler).AddrOfPinnedObject() + destid, new IntPtr(headbuff) + offset, (ulong)length);
                        //Marshal.Copy(HeaderBuffer, offset, GCHandle.FromIntPtr(binReceiveHandler).AddrOfPinnedObject() + destid, length);
                    }
                }
            }
            return noiseKind;
        }
        public NoiseType HttpHeader(int received)
        {
            NoiseType noiseKind = NoiseType.None;

            lock (accessReceive)
            {
                if (BlockSize == 0)
                {
                    BlockSize = 1;
                    msReceive = new MemoryStream();
                }

                msReceive.Write(HeaderBuffer, 0, received);

                if (received < BufferSize)
                {
                    BlockSize = 0;
                    msReceive.Position = 0;
                    ParseRequest(msReceive);
                    ReadHeaders(msReceive);
                    if (VerifyRequest())
                        binReceive = msReceive.ToArray().Skip(Convert.ToInt32(msReceive.Position)).ToArray();
                    else
                        Denied = true;                    
                }
            }
            return noiseKind;
        }

        public NoiseType IncomingMessage(int received)
        {
            disposed = false;
            NoiseType noiseKind = NoiseType.None;
            if (Protocol == DepotProtocol.QDTP)
                return QdtpMessage(received);
            return noiseKind;
        }
        public NoiseType QdtpMessage(int received)
        {
            NoiseType noiseKind = NoiseType.None;

            lock (accessReceive)
            {
                int offset = 0, length = received;
                bool inprogress = false;

                if (BlockSize == 0)
                {
                    BlockSize = BitConverter.ToInt64(MessageBuffer, 4);
                    DeserialBlockId = BitConverter.ToInt32(MessageBuffer, 12);

                    binReceive = new byte[BlockSize];
                    GCHandle gc = GCHandle.Alloc(binReceive, GCHandleType.Pinned);
                    binReceiveHandler = GCHandle.ToIntPtr(gc);

                    offset = BlockOffset;
                    length -= BlockOffset;
                }

                if (BlockSize > 0)
                    inprogress = true;

                BlockSize -= length;

                if (BlockSize < 1)
                {
                    long endPosition = length;
                    noiseKind = MessageBuffer.SeekNoise(out endPosition, SeekDirection.Backward);
                }

                int destid = (binReceive.Length - ((int)BlockSize + length));
                if (inprogress)
                {
                    fixed (void* msgbuff = MessageBuffer)
                    {
                        MemoryNative.Copy(GCHandle.FromIntPtr(binReceiveHandler).AddrOfPinnedObject() + destid, new IntPtr(msgbuff) + offset, (ulong)length);
                        //Marshal.Copy(HeaderBuffer, offset, GCHandle.FromIntPtr(binReceiveHandler).AddrOfPinnedObject() + destid, length);
                    }
                    //Marshal.Copy(MessageBuffer, offset, GCHandle.FromIntPtr(binReceiveHandler).AddrOfPinnedObject() + destid, length);
                }
            }
            return noiseKind;
        }

        public int ObjectPosition
        { get; set; }
        public int ObjectsLeft
        { get; set; }

        public Socket SocketEar
        {
            get
            {
                return this.ear;
            }
            set
            {
                this.ear = value;
            }
        }

        public DepotTransaction Transaction
        {
            get
            {
                return this.tr;
            }
            set
            {
                if (value.Context == null)
                {
                    value.Context = this;
                    if (value.Identity != null)
                        value.MyHeader.BindContext(value.Context);
                }
                if (value.MyMessage.Content != null)
                {
                    if (value.MyMessage.Content.GetType() == typeof(object[][]))
                        value.MyHeader.Context.ObjectsCount = ((object[][])value.MyMessage.Content).Length;
                }
                this.tr = value;
            }
        }

        public DepotSite IdentitySite
        {
            get
            {
                return Transaction.MyHeader.Context.IdentitySite;
            }
        }

        public DepotProtocol IdentifyProtocol()
        {
            StringBuilder sb = new StringBuilder();
            Protocol = DepotProtocol.NONE;
            ProtocolMethod method = ProtocolMethod.NONE;
            for (int i = 0; i < HeaderBuffer.Length; i++)
            {
                NoiseType splitter = NoiseType.None;
                HeaderBuffer[i].IsSpliter(out splitter);
                if ((splitter != NoiseType.Empty) &&
                    (splitter != NoiseType.Space) &&
                    (splitter != NoiseType.Line))
                    sb.Append(HeaderBuffer[i].ToChars(CharCoding.UTF8));
                if (sb.Length > 3)
                {
                    method = ProtocolMethod.NONE;
                    Enum.TryParse(sb.ToString().ToUpper(), out method);
                    if (method != ProtocolMethod.NONE)
                    {
                        switch (method)
                        {
                            case ProtocolMethod.DPOT:
                                Protocol = DepotProtocol.QDTP;
                                break;
                            case ProtocolMethod.SYNC:
                                Protocol = DepotProtocol.HTTP;
                                break;
                            case ProtocolMethod.GET:
                                Protocol = DepotProtocol.HTTP;
                                break;
                            case ProtocolMethod.POST:
                                Protocol = DepotProtocol.HTTP;
                                break;
                            case ProtocolMethod.OPTIONS:
                                Protocol = DepotProtocol.HTTP;
                                break;
                            default:
                                Protocol = DepotProtocol.NONE;
                                break;
                        }
                    }
                    Method = method;
                    if (Protocol != DepotProtocol.NONE)
                    {
                        sb = null;
                        return Protocol;
                    }
                }
            }
            sb = null;
            return Protocol;
        }

        DepotEvent SendEcho
        { get; set; }
        public string Echo
        {
            get
            {
                return this.sb.ToString();
            }
        }

        public void Append(string text)
        {
            this.sb.Append(text);
        }

        public void Reset()
        {
            if (!disposed)
            {
                sb.Clear();
                sb = new StringBuilder();
                msSend.Dispose();
                msSend = new MemoryStream();
                msReceive.Dispose();
                msReceive = new MemoryStream();

                lock (accessReceive)
                {
                    if (!binReceiveHandler.Equals(IntPtr.Zero))
                    {
                        GCHandle gc = GCHandle.FromIntPtr(binReceiveHandler);
                        gc.Free();
                        binReceive = new byte[0];
                    }
                }
                lock (accessSend)
                    binSend = new byte[0];
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                sb.Clear();
                msSend.Dispose();
                msReceive.Dispose();

                lock (accessReceive)
                {
                    if (!binReceiveHandler.Equals(IntPtr.Zero))
                    {
                        GCHandle gc = GCHandle.FromIntPtr(binReceiveHandler);
                        gc.Free();
                        binReceive = null;
                    }
                }
                lock (accessSend)
                    binSend = null;

                HeaderSentNotice.Dispose();
                HeaderReceivedNotice.Dispose();
                MessageSentNotice.Dispose();
                MessageReceivedNotice.Dispose();
                BatchesReceivedNotice.Dispose();

                disposed = true;
            }
        }

        #region Http Raw Stream Processing - TODO - After testing create HttpContext and HttpProcessor class, then move below there  

        public StringBuilder RequestBuilder { get { return requestBuilder; } set { requestBuilder = value; } }
        public StringBuilder ResponseBuilder { get { return responseBuilder; } set { responseBuilder = value; } }

        [NonSerialized] private String http_method;
        [NonSerialized] private String http_url;
        [NonSerialized] private String http_protocol_version;

        public Hashtable HttpHeaders { get { return httpHeaders; } set { httpHeaders = value; } }
        public Hashtable HttpOptions { get { return httpOptions; } set { httpOptions = value; } }

        public void ParseRequest(Stream ms)
        {
            String request = streamReadLine(ms);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            http_method = tokens[0].ToUpper();
            http_url = tokens[1];
            http_protocol_version = tokens[2];

        }

        public void ReadHeaders(Stream ms)
        {
            String line;
            while ((line = streamReadLine(ms)) != null)
            {
                if (line.Equals(""))
                    return;

                int separator = line.IndexOf(':');
                if (separator == -1)
                    throw new Exception("invalid http header line: " + line);

                String name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                    pos++; // strip any spaces                

                string value = line.Substring(pos, line.Length - pos);
                HttpHeaders[name] = value;
            }
        }

        public bool VerifyRequest()
        {
            bool verified = false;
            string ip = tr.MyHeader.Context.RemoteEndPoint.Address.ToString();

            if (HttpHeaders.ContainsKey("DepotToken"))
                if (HttpHeaders["DepotToken"].ToString() != "")
                {
                    string token = HttpHeaders["DepotToken"].ToString();
                    DepotIdentity di = null;
                    if (MembersConfig.RegisterMember(token, out di, ip))
                    {
                        verified = true;
                        HttpOptions["DepotToken"] = di.Token;
                        HttpOptions["DepotUserId"] = di.UserId;
                        HttpOptions["DepotDeptId"] = di.DeptId;
                    }
                }

            if (!verified)
                if (HttpHeaders.ContainsKey("Authorization"))
                    if (HttpHeaders["Authorization"].ToString() != "")
                    {
                        string[] codes = HttpHeaders["Authorization"].ToString().Split(' ');
                        string decode64 = "";
                        string name = "";
                        string key = "";
                        if (codes.Length > 1)
                        {
                            decode64 = Encoding.UTF8.GetString(Convert.FromBase64String(codes[1]));
                            string[] namekey = decode64.Split(':');
                            name = namekey[0];
                            key = namekey[1];

                            DepotIdentity di = null;
                            if (MembersConfig.RegisterMember(name, key, out di, ip))
                            {
                                verified = true;
                                HttpOptions["DepotToken"] = di.Token;
                                HttpOptions["DepotUserId"] = di.UserId;
                                HttpOptions["DepotDeptId"] = di.DeptId;
                            }
                        }
                    }

            return verified;
        }

        public void HandleGetRequest(string content_type = "text/html")
        {
            if (http_url.Equals("/") ||
                http_url.Equals(""))
                http_url = "/Index.html";

            if (!Resources.TryGetValue(http_url, out binSend))
            {
                MemoryStream ms = new MemoryStream();
                if (GetJavaScriptProject(ms))
                {
                    content_type = GetHttpExtensionType();
                    writeSuccess(content_type);
                }
                else
                {
                    if (File.Exists("../../Web" + http_url))
                    {
                        content_type = GetHttpExtensionType();
                        Stream fs = File.Open("../../Web" + http_url, FileMode.Open);
                        fs.CopyTo(ms);
                        fs.Close();
                        writeSuccess(content_type);
                    }
                    else
                        writeFailure();
                }

                if (HttpHeaders.ContainsKey("Connection"))
                    writeClose(HttpHeaders["Connection"].ToString());
                else
                    writeClose();

                if (content_type.Contains("text") ||
                    content_type.Contains("json"))
                {
                    ResponseBuilder.Append((ms.ToArray().ToChars(CharCoding.UTF8)));
                    binSend = ResponseBuilder.ToString().ToBytes(CharCoding.UTF8);
                    Resources.Add(http_url, binSend);
                }
                else
                {
                    binSend = ResponseBuilder.ToString().ToBytes(CharCoding.UTF8).Concat(ms.ToArray()).ToArray();
                    Resources.Add(http_url, binSend);
                }
                ms.Dispose();
            }
        }
        public void HandlePostRequest(string content_type = "text/html")
        {
            writeSuccess(content_type);
            writeOptions();
            writeClose();
            string requestBuilder = RequestBuilder.ToString();//.Trim(new char[] { '\n', '\r' });
            ResponseBuilder.AppendLine(requestBuilder);
            string responseString = ResponseBuilder.ToString();//.TrimEnd(new char[] { '\n', '\r' });           
            binSend = responseString.ToBytes(CharCoding.UTF8);
        }
        public void HandleOptionsRequest(string content_type = "text/html")
        {
            writeSuccess(content_type);
            writeOptions();
            writeClose();
            binSend = ResponseBuilder.ToString().ToBytes(CharCoding.UTF8);
        }
        public void HandleDeniedRequest()
        {
            writeDenied();
            writeClose();
            binSend = ResponseBuilder.ToString().ToBytes(CharCoding.UTF8);
        }

        public string GetHttpExtensionType()
        {
            string extension = http_url.Substring(http_url.LastIndexOf('.'));
            string result = null;
            httpExtensions.TryGetValue(extension, out result);
            return result = (result == null) ? "text/html" : result;
        }

        public bool   GetJavaScriptProject(MemoryStream ms)
        {
            string extension = http_url.Substring(http_url.LastIndexOf('.'));
            if (extension.Equals(".qjs") || extension.Equals(".bjs"))
            {
                string jspdir = "../../Web" + http_url.Substring(0, http_url.LastIndexOf('.'));
                List<string> jspfiles = new List<string>();
                DirSearch(jspdir, jspfiles);
                foreach (string jspfile in jspfiles)
                {
                    Stream fs = File.Open(jspfile, FileMode.Open);
                    fs.CopyTo(ms);
                    //ms.Write(("\n").ToBytes(CharCoding.UTF8), 0, 1);
                    fs.Close();
                }
                return true;
            }
            return false;
        }

        public void DirSearch(string dir, List<string> jspfiles)
        {
            try
            {
                foreach (string f in Directory.GetFiles(dir))
                    jspfiles.Add(f);
                foreach (string d in Directory.GetDirectories(dir))
                    DirSearch(d, jspfiles);
            }
            catch (Exception ex)
            {
            }
        }

        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            StringBuilder data = new StringBuilder();
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data.Append(((byte)next_char).ToChars(CharCoding.UTF8));
            }
            return data.ToString();
        }

        private void writeOptions()
        {
            if(HttpOptions.Count > 0)
                foreach(DictionaryEntry option in HttpOptions)
                    ResponseBuilder.AppendLine(string.Format("{0}: {1}",option.Key, option.Value));

            ResponseBuilder.AppendLine("Accept: application/json");
            ResponseBuilder.AppendLine("Access-Control-Allow-Headers: content-type");
            ResponseBuilder.AppendLine("Access-Control-Allow-Origin: " + HttpHeaders["Origin"].ToString());
        }

        private void writeSuccess(string content_type = "text/html")
        {
            ResponseBuilder.AppendLine("HTTP/1.1 200 OK");
            ResponseBuilder.AppendLine("Content-Type: " + content_type);
        }

        private void writeFailure()
        {
            ResponseBuilder.AppendLine("HTTP/1.1 404 File not found");
        }

        private void writeDenied()
        {
            ResponseBuilder.AppendLine("HTTP/1.1 401.7 Access denied by authorization strategy on QDTP server");
        }

        private void writeClose(string state = "close")
        {
            ResponseBuilder.AppendLine("Connection: " + state);
            ResponseBuilder.AppendLine("");
        }

        [NonSerialized]
        private Dictionary<string, string> httpExtensions = new Dictionary<string, string>()
        {
            { ".html", "text/html" },
            { ".css",  "text/css"  },
            { ".less", "text/css"  },
            { ".png",  "image/png" },
            { ".ico",  "image/ico" },
            { ".jpg",  "image/jpg" },
            { ".bmp",  "image/bmp" },
            { ".gif",  "image/gif" },
            { ".js",   "text/javascript" },
            { ".qjs",  "text/javascript" },
            { ".bjs",  "text/babel" },
            { ".json", "application/json" },
            { ".woff", "font/woff" },
            { ".woff2","font/woff2" },
            { ".ttf",  "font/ttf" },
            { ".svg",  "image/svg" }
        };

        #endregion

    }
}
