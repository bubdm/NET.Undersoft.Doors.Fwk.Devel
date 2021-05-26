using System;
using System.IO;
using System.Doors;

namespace System.Doors.Data.Depot
{
    public interface IDepotClient : IDisposable
    {
        IDepotContext Context { get; set; }

        IDorsEvent Connected { get; set; }
        IDorsEvent HeaderSent { get; set; }
        IDorsEvent MessageSent { get; set; }
        IDorsEvent HeaderReceived { get; set; }
        IDorsEvent MessageReceived { get; set; }
      
        void Connect();

        bool IsConnected();

        void Send(MessagePart messagePart, bool close);
      
        void Receive(MessagePart messagePart);
              
    }
}
