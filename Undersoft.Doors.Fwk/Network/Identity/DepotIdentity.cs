using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Doors
{
    [Serializable]
    public class DepotIdentity
    {        
        public int Id { get; set; }
        public int Limit { get; set; }
        public int Scale { get; set; }
        public int Port { get; set; }        
        public string Ip { get; set; }
        public string Host { get; set; }
        public string DeptId { get; set; }
        public string UserId { get; set; }
        public string AuthId { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Token { get; set; }
        public string DataPlace { get; set; }
        public DateTime RegisterTime { get; set; }
        public DateTime LastAction { get; set; }
        public DateTime LifeTime { get; set; }
        public bool Active { get; set; }
        public DepotSite Site;
        public ServiceMode Mode;
    }

    [Serializable]
    public enum DepotSite
    {
        Client,
        Server       
    }

    [Serializable]
    public enum DirectionType
    {
        Send,
        Receive,
        None
    }

    [Serializable]
    public enum DepotProtocol
    {
        NONE,
        QDTP,
        HTTP      
    }

    [Serializable]
    public enum ProtocolMethod
    {
        NONE,
        DPOT,
        SYNC,
        GET,
        POST,
        OPTIONS,
        PUT,
        DELETE,
        PATCH
    }

    [Serializable]
    public enum DepotComplexity
    {
        Guide,
        Basic,
        Standard,
        Advanced
    }

    [Serializable]
    public enum MessagePart
    {
        Header,
        Message,
    }

    public enum ServiceMode
    {
        None,
        Console,
        Service
    }

}
