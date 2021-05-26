using System;
using System.Doors;

namespace System.Doors.Data
{
    public class EchologConfig : IDataLog
    {
        private Quid serviceId;
        private string service;

        public EchologConfig(Quid ServiceId, string Service)
        {
            serviceId = ServiceId;
            service = Service;
        }

        public void WriteLog(string message)
        {
            string[] details = message.Split('#');
            string level = details[0];
            string type = details[1];
            string date = details[2];
            string msg = details[4];

            DataTrellis trellis = DataBank.Vault["Config"]["Echolog"].Trell;
            DataTier tier = new DataTier(trellis);
            tier.PrimeArray = new object[] { 0, serviceId, service, DateTime.Parse(date), msg, type, int.Parse(level) };
            trellis.Tiers.Put(tier, true);
        }
    }
}
