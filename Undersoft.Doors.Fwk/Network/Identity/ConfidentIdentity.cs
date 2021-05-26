using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Doors
{ 
    [Serializable]
    public class ConfidentIdentity
    {
        public int Id;
        public int Port;
        public int Limit;
        public string Host;
        public string Name;
        public string Key;
        public string AuthId;
        public string Token;        
        public long RegisterTime;
        public long LastAction;
        public long LifeTime;
        public bool Active;
        public ServiceMode Mode;
    }

}
