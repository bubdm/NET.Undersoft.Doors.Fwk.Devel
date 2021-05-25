using System;

namespace System.Dors
{
    public interface IDepotSync
    {
        object Content { get; set; }

        void SetCallback(string methodName, object classObject);
        void SetCallback(IDorsEvent OnCompleteEvent);

        void   Reconnect();
        object Initiate(bool isAsync = true);
        void   Close();
    }
}