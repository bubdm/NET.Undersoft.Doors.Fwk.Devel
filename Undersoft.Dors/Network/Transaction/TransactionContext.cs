using System;
using System.Net;
using System.Dors;

namespace System.Dors
{
    [JsonObject]
    [Serializable]
    public class TransactionContext
    {
        [NonSerialized] private Type contentType;

        public DepotIdentity Identity { get; set; }
        public DepotSite IdentitySite
        { get; set; } = DepotSite.Client;
        public DepotComplexity Complexity
        { get; set; } = DepotComplexity.Standard;
        [NonSerialized] public IPEndPoint LocalEndPoint;
        [NonSerialized] public IPEndPoint RemoteEndPoint;
        public string EventMethod;
        public string EventClass;
        public bool Synchronic
        { get; set; } = false;
        public bool SendMessage
        { get; set; } = true;
        public bool ReceiveMessage
        { get; set; } = true;
        public Type ContentType
        {
            get
            {
                if (contentType == null && ContentTypeName != null)
                    ContentType = TypeAssemblies.GetType(ContentTypeName);
                return contentType;
            }
            set
            {
                if (value != null)
                {
                    ContentTypeName = value.FullName;
                    contentType = value;
                }
            }
        }   
        public string ContentTypeName { get; set; }
        public int ObjectsCount
        { get; set; } = 0;
        public string Echo { get; set; }
        public int Errors { get; set; }
    }
}
