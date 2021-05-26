using System.Net.Sockets;
using System.Doors;
using System.Doors.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace System.Doors.Data.Depot
{
    public interface IDepotContext : ISerialContext
    {
        DepotProtocol Protocol { get; set; }
        ProtocolMethod Method { get; set; }
        IDictionary<string, byte[]> Resources { get; set; }

        ManualResetEvent HeaderSentNotice { get; set; }
        ManualResetEvent MessageSentNotice { get; set; }
        ManualResetEvent HeaderReceivedNotice { get; set; }
        ManualResetEvent MessageReceivedNotice { get; set; }
        ManualResetEvent BatchesReceivedNotice { get; set; }

        Socket SocketEar { get; set; }
        DepotTransaction Transaction { get; set; }
   
        int Id { get; set; }

        bool Synchronic { get; set; }
        bool SendMessage { get; set; }
        bool ReceiveMessage { get; set; }

        int BufferSize { get; }      
        byte[] HeaderBuffer { get; }
        byte[] MessageBuffer { get; }

        int ObjectPosition { get; set; }
        int ObjectsLeft { get; set; }      

        NoiseType IncomingHeader(int received);
        NoiseType IncomingMessage(int received);

        bool Close { get; set; }
        bool Denied { get; set; }

        string Echo { get; }

        void Append(string text);

        void Reset();

        void Dispose();

        DepotProtocol IdentifyProtocol();

        StringBuilder RequestBuilder { get; set; }
        StringBuilder ResponseBuilder { get; set; }

        Hashtable HttpHeaders { get; set; }
        Hashtable HttpOptions { get; set; }

        void HandleGetRequest(string content_type = "text/html");
        void HandlePostRequest(string content_type = "text/html");
        void HandleOptionsRequest(string content_type = "text/html");
        void HandleDeniedRequest();
    }
}
