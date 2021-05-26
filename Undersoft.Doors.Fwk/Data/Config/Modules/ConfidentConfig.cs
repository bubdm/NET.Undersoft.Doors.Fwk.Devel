using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Doors.Data;
using System.Doors;

namespace System.Doors.Data
{
    public static class ConfidentConfig
    {
        public static DepotIdentity GetLocalService()
        {
            return DataBank.ServiceIdentity = GetServiceIdentity(-1);
        }

        public static DepotIdentity GetLocalConsole()
        {
            return DataBank.ServiceIdentity = GetConsoleIdentity(-1);
        }

        public static DepotIdentity GetServiceIdentity(int Id)
        {
            DepotIdentity ci = new DepotIdentity();
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Services"].Trell.Tiers.Collect("Id", Id);
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                ci.Id = (int)sc["Id"];
                ci.Port = (ushort)sc["Port"];               
                ci.Host = sc["Host"].ToString().Split(':')[0] + ((ci.Port > 0) ? ":" + ci.Port.ToString() : "");
                ci.Name = sc["Name"].ToString();      
                ci.Limit = (ushort)sc["Limit"];
                ci.Mode = ServiceMode.Service;
                Enum.TryParse<ServiceMode>(sc["Type"].ToString(), out ci.Mode);
                ci.AuthId = DataBank.Vault.Config.GetServiceId(ci).ToString();
                sc["ServiceId"] = DataBank.Vault.Config.GetServiceId(ci);
                return ci;
            }
            return null;
        }

        public static DepotIdentity GetConsoleIdentity(int Id)
        {
            DepotIdentity ci = new DepotIdentity();
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Consoles"].Trell.Tiers.Collect("Id", Id);
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                ci.Id = (int)sc["Id"];
                ci.Port = (ushort)sc["Port"];              
                ci.Host = sc["Host"].ToString().Split(':')[0] + ((ci.Port > 0) ? ":" + ci.Port.ToString() : "");
                ci.Name = sc["Name"].ToString();   
                ci.Key =  sc["Key"].ToString();
                ci.AuthId = new Quid((sc["Name"].ToString() + sc["Key"].ToString()).GetPCHexShah()).ToString();
                ci.Token = sc["Token"].ToString();
                ci.RegisterTime = (DateTime)sc["RegisterTime"];
                ci.LastAction = (DateTime)sc["LastAction"];
                ci.LifeTime = (DateTime)sc["LifeTime"];
                ci.Mode = ServiceMode.Console;
                Enum.TryParse<ServiceMode>(sc["Type"].ToString(), out ci.Mode);
                sc["ConsoleId"] = new Quid((sc["Name"].ToString() + sc["Key"].ToString()).GetPCHexShah());
                ci.Active = (bool)sc["Active"];
                return ci;
            }
            return null;
        }

        public static DepotIdentity GetConsoleIdentity(string consoleName)
        {
            DepotIdentity ci = null;
            DataTier[] services = DataBank.Vault["Config"]["Consoles"].Trell.Tiers.Collect("Name", consoleName);
            if (services.Length > 0)
                ci = GetConsoleIdentity((int)services[0]["Id"]);
            return ci;
        }

        public static DepotIdentity GetServiceIdentity(string serviceName)
        {
            DepotIdentity ci = null;
            DataTier[] services = DataBank.Vault["Config"]["Services"].Trell.Tiers.Collect("Name", serviceName);
            if (services.Length > 0)
                ci = GetServiceIdentity((int)services[0]["Id"]);

            return ci;
        }

        public static DepotIdentity SetConsoleIdentity(DepotIdentity confidentIdentity)
        {
            DepotIdentity ci = confidentIdentity;
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Consoles"].Trell.Tiers.Collect("Id", ci.Id);
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                sc["Token"] = ci.Token;
                sc["RegisterTime"] = ci.RegisterTime;
                sc["LastAction"] = ci.LastAction;
                sc["LifeTime"] = ci.LifeTime;

                sc.WriteDrive();

                sc["Active"] = ci.Active;

                return ci;
            }
            return null;
        }
      
        public static string RegisterConsole(string name, string authId)
        {
            string token = null;
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DepotIdentity si = GetConsoleIdentity(name);
            if (si != null)
                if (si.AuthId.Equals(authId))
                {
                    DateTime time = DateTime.Now;
                    si.Token = new Guid(si.Key.GetShah().Concat(BitConverter.GetBytes(time.Ticks)).ToArray()).ToString();
                    si.RegisterTime = time;
                    si.LastAction = time;
                    si.LifeTime = time.AddHours(0.5);
                    si.Active = true;
                    SetConsoleIdentity(si);
                    token = si.Token;
                }
            return token;
        }

        public static DepotIdentity DeactivteConsole(DepotIdentity confidentIdentity)
        {
            DepotIdentity ci = confidentIdentity;
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Consoles"].Trell.Tiers.Collect("Id", ci.Id);
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                sc["Token"] = ci.Token;
                sc["RegisterTime"] = ci.RegisterTime;
                sc["LastAction"] = ci.LastAction;
                sc["LifeTime"] = ci.LifeTime;
                sc["Active"] = false;
                ci.Active = false;
                sc.WriteDrive();

                return ci;
            }
            return null;
        }

        public static bool VerifyConsoleIdentity(DepotIdentity confidentIdentity)
        {
            bool verify = false;
            DepotIdentity ci = confidentIdentity;
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DepotIdentity si = GetConsoleIdentity(ci.Name);
            if (si != null)            
                verify = (si.Token.Equals(ci.Token));                     
            return verify;
        }

        public static bool VerifyConsoleIdentity(string name, string token)
        {
            bool verify = false;
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DepotIdentity si = GetConsoleIdentity(name);
            if (si != null)
                verify = (si.Token.Equals(token));
            return verify;
        }
    }

}
