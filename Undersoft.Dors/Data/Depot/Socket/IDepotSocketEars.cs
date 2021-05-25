using System;
using System.Dors;

namespace System.Dors.Data.Depot
{
    public interface IDepotSocketEars : IDisposable
    {
        DepotIdentity Identity { get; set; }

        IDorsEvent HeaderReceived { get; set; }
        IDorsEvent MessageReceived { get; set; }
        IDorsEvent HeaderSent { get; set; }
        IDorsEvent MessageSent { get; set; }
        IDorsEvent SendEcho { get; set; }

        void OpenEars();

        bool IsConnected(int id);

        void OnConnectCallback(IAsyncResult result);

        void HeaderReceivedCallback(IAsyncResult result);

        void MessageReceivedCallback(IAsyncResult result);

        void Receive(MessagePart messagePart, int id);

        void Send(MessagePart messagePart, int id, bool _close);

        void HeaderSentCallback(IAsyncResult result);

        void MessageSentCallback(IAsyncResult result);

        void ClearResources();

        void ClearClients();

        void CloseClient(int id);

        void CloseEars();

        void Echo(string message);
    }
}
