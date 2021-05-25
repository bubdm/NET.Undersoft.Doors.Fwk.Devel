using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Data;
using System.Globalization;
using System.Dors.Data;
using System.Dors;

namespace System.Dors.Data
{
    public static class DepotConfig
    {    
        public static DepotIdentity GetDepotIdentity()
        {
            return GetDepotIdentity(-1);
        }

        public static Quid GetSpaceDepotId()
        {
           DataBank.ServerIdentity = GetDepotIdentity(-1);
           return new Quid(DataSpace.Area.Config.GetDepotId());
        }

        public static Quid SetSpaceDepotId()
        {
            return DataSpace.Area.Config.DepotId =  GetSpaceDepotId();
        }

        public static DepotIdentity GetClientIdentity(int Id)
        {
            DepotIdentity ci = new DepotIdentity();
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Depots"].Trell.Tiers.Collect("Id", Id);

            if (services.Length > 0)
            {
                DataTier sc = services[0];

                ci.Id = (int)sc["Id"];
                ci.Name = Convert.ToBase64String(Encoding.ASCII.GetBytes(sc["Login"].ToString().ToArray()));
                ci.Key = Convert.ToBase64String(Encoding.ASCII.GetBytes(sc["Password"].ToString().ToArray()));
                ci.Port = (ushort)sc["Port"];
                ci.Ip = sc["IP"].ToString();
                ci.Host = sc["Name"].ToString() + ((ci.Port > 0) ? ":" + ci.Port.ToString() : "");
                ci.Site = DepotSite.Client;
                ci.Mode = ServiceMode.Console;
                if (sc.HasPylon("Type"))
                    Enum.TryParse(sc["Type"].ToString(), out ci.Site);                
            }
            return ci;
        }
        public static DepotIdentity GetClientIdentity()
        {
            return DataBank.ServerIdentity = GetClientIdentity(-1);
        }
        public static DepotIdentity GetClientIdentity(string serviceName)
        {
            DataTier[] tiers = DataSpace.Area["Config"]["Services", "Services"].Tiers.Collect("Name", serviceName);
            if (tiers.Length > 0)
            {
                DataTiers dpts = tiers[0].GetChild("Depots");
                if (dpts.Count > 0)
                {
                   return DepotConfig.GetClientIdentity((int)dpts[0]["Id"]);
                }
            }
            return null;
        }

        public static DepotIdentity SetClientIdentity(DepotIdentity ci)
        {           
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();

            DataTrellis trl = DataBank.Vault["Config"]["Depots"].Trell;
            DataTier[] services = trl.Collect("Id", ci.Id);
            DataTier sc = null;

            if (services.Length < 1) sc =  trl.Tiers.Put(new DataTier(trl));
            else sc = services[0];                          

            sc["Id"] = ci.Id;
            sc["Login"] = ci.Name;
            sc["Password"] = ci.Key;
            sc["Port"] = ci.Port;
            sc["IP"] = ci.Ip;
            sc["Name"] = ci.Host;
            ci.Site = DepotSite.Client;
            ci.Mode = ServiceMode.Console;
            if (sc.HasPylon("Type"))
                sc["Type"] = ci.Site;

            return ci;
        }

        public static DepotIdentity GetDepotIdentity(int Id)
        {
            DepotIdentity ci = new DepotIdentity();
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[]  services = DataBank.Vault["Config"]["Depots"].Trell.Tiers.Collect("Id", Id);
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                ci.Id = (int)sc["Id"];
                ci.Port = (ushort)sc["Port"];
                ci.Scale = (ushort)sc["Scale"];
                ci.Limit = (ushort)sc["Limit"];
                ci.Ip = sc["IP"].ToString();
                ci.Host = sc["IP"].ToString();
                ci.Name = sc["Name"].ToString();
                ci.Site = DepotSite.Server;
                ci.Mode = ServiceMode.Service;
                if (sc.HasPylon("Type"))
                    Enum.TryParse(sc["Type"].ToString(), out ci.Site);
            }
            return ci;
        }
        public static DepotIdentity GetDepotIdentity(string consoleName)
        {
            DepotIdentity ci = null;
            DataTier[]  services = DataBank.Vault["Config"]["Depots"].Trell.Tiers.Collect("Name", consoleName);
            if (services.Length > 0)
                ci = GetDepotIdentity((int)services[0]["Id"]);

            return ci;
        }

        public static DepotIdentity SetDepotIdentity(DepotIdentity ci)
        {
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();

            DataTrellis trl = DataBank.Vault["Config"]["Depots"].Trell;
            DataTier[] services = trl.Collect("Id", ci.Id);
            DataTier sc = null;

            if (services.Length < 1) sc = trl.Tiers.Put(new DataTier(trl));
            else sc = services[0];

            sc["Id"] = ci.Id;
            sc["Login"] = ci.Name;
            sc["Password"] = ci.Key;
            sc["Port"] = (ushort)ci.Port;
            ci.Site = DepotSite.Server;
            ci.Mode = ServiceMode.Service;
            if (sc.HasPylon("Type"))
                sc["Type"] = ci.Site.ToString();

            return ci;
        }

        public static DepotIdentity GetLocalConsole()
        {
            return DataBank.ServiceIdentity = GetConsoleIdentity(-1);
        }
        public static DepotIdentity GetConsoleIdentity(int Id)
        {
            DepotIdentity ci = new DepotIdentity();
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[]  services = DataBank.Vault["Config"]["Consoles"].Trell.Tiers.Collect("Id", Id);
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                ci.Id = (int)sc["Id"];
                ci.Port = (ushort)sc["Port"];
                ci.Host = sc["Host"].ToString().Split(':')[0] + ((ci.Port > 0) ? ":" + ci.Port.ToString() : "");
                ci.Name = sc["Name"].ToString();
                ci.Key = sc["Password"].ToString();
                ci.AuthId = new Quid((sc["Name"].ToString() + sc["Password"].ToString()).GetPCHexShah()).ToString();
                ci.Token = sc["Token"].ToString();
                ci.RegisterTime = (DateTime)sc["RegisterTime"];
                ci.LastAction = (DateTime)sc["LastAction"];
                ci.LifeTime = (DateTime)sc["LifeTime"];
                ci.Mode = ServiceMode.Console;
                if (sc.HasPylon("Type"))
                    Enum.TryParse<ServiceMode>(sc["Type"].ToString(), out ci.Mode);
                sc["ConsoleId"] = new Quid((sc["Name"].ToString() + sc["Password"].ToString()).GetPCHexShah());
                ci.Active = (bool)sc["Active"];
                return ci;
            }
            return null;
        }
        public static DepotIdentity GetConsoleIdentity(string FieldName, object Value)
        {
            DataTier[] members = DataBank.Vault["Config"]["Consoles"].Trell.Tiers.Collect(FieldName, Value);
            if (members != null && members.Length > 0)
            {
                DataTier member = members[0];
                DepotIdentity di = SetConsoleIdentity(member);
                return di;
            }
            return null;
        }
        public static DepotIdentity GetConsoleIdentity(string consoleName)
        {
            DepotIdentity ci = null;
            DataTier[]  services = DataBank.Vault["Config"]["Consoles"].Trell.Tiers.Collect("Name", consoleName);
            if (services.Length > 0)
                ci = GetConsoleIdentity((int)services[0]["Id"]);
            return ci;
        }

        public static DepotIdentity LocalService => DataBank.ServiceIdentity = GetServiceIdentity(-1);
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
                if(sc.HasPylon("Type"))
                    Enum.TryParse<ServiceMode>(sc["Type"].ToString(), out ci.Mode);
                ci.AuthId = DataBank.Vault.Config.GetServiceId(ci).ToString();
                sc["ServiceId"] = DataBank.Vault.Config.GetServiceId(ci);
                return ci;
            }
            return null;
        }
        public static DepotIdentity GetServiceIdentity(string serviceName)
        {
            DepotIdentity ci = null;
            DataTier[]  services = DataBank.Vault["Config"]["Services"].Trell.Tiers.Collect("Service", serviceName);
            if(services == null)
                services = DataBank.Vault["Config"]["Services"].Trell.Tiers.Collect("Name", serviceName);
            if (services != null && services.Length > 0)
                ci = GetServiceIdentity((int)services[0]["Id"]);

            return ci;
        }

        public static DataTier GetConsoleData(string FieldName, object Value)
        {
            DataTier[]  members = DataBank.Vault["Config"]["Consoles"].Trell.Tiers.Collect(FieldName, Value);
            if (members != null && members.Length > 0)
            {
                DataTier member = members[0];
                return member;
            }
            return null;
        }
        public static DataTier GetSessionData(string token)
        {
            DataTier[]  sessions = DataBank.Vault["Config"]["Consoles"].Trell.Tiers.Collect("Token", token);
            if (sessions != null && sessions.Length > 0)
            {
                DataTier member = sessions[0];
                return member;
            }
            return null;
        }

        private static DepotIdentity SetConsoleIdentity(DataTier member)
        {
            DepotIdentity di = new DepotIdentity();
            di.Id = (int)member["Id"];
            di.Name = member["Login"].ToString();
            di.UserId = member["ConsoleId"].ToString();

            di.DeptId = member["ServiceId"].ToString();
            di.DataPlace = member["DataPlace"].ToString();

            di.RegisterTime = (DateTime)member["RegisterTime"];
            di.LastAction = (DateTime)member["LastAction"];
            di.LifeTime = (DateTime)member["LifeTime"];

            return di;
        }
        private static DepotIdentity SetConsoleIdentity(DepotIdentity confidentIdentity)
        {
            DepotIdentity ci = confidentIdentity;
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[]  services = DataBank.Vault["Config"]["Consoles"].Trell.Tiers.Collect("Id", ci.Id);
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

        public static bool RegisterConsole(DepotIdentity identity, bool encoded = false)
        {
            DepotIdentity di = identity;
            di.Active = false;
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();

            DataTier member = null;
            bool verify = false;
            if (di.UserId != null && di.UserId != "")
                member = GetConsoleData("ConsoleId", new Quid(di.UserId));
            else if (di.Name != null && di.Name != "")
            {
                string name = (encoded) ? Encoding.ASCII.GetString(Convert.FromBase64String(di.Name)) : di.Name;
                member = GetConsoleData("Login", name);
            }

            if (member != null)
            {
                DataTier session = member;
                session["Host"] = di.Ip;
                if (di.Key != null && di.Key != "")
                {
                    string key = (encoded) ? Encoding.ASCII.GetString(Convert.FromBase64String(di.Key)) : di.Key;
                    verify = VerifyMemberIdentity(member, key);
                    if (verify)
                        di.Token = CreateMemberToken(member);
                }
                else if (di.Token != null && di.Token != "")
                {
                    verify = VerifyMemberToken(member, di.Token);
                }

                if (verify)
                {
                    di.Key = null;
                    di.Name = null;
                    di.UserId = member["ConsoleId"].ToString();
                    di.DeptId = member["ServiceId"].ToString();
                    di.DataPlace = member["DataPlace"].ToString();
                    di.RegisterTime = (DateTime)session["RegisterTime"];
                    di.LastAction = (DateTime)session["LastAction"];
                    di.LifeTime = (DateTime)session["LifeTime"];
                    di.Active = true;
                }
            }

            return verify;
        }
        public static bool RegisterConsole(string name, string key, out DepotIdentity di, string ip = "")
        {
            di = GetConsoleIdentity("Login", name);
            if (di != null)
            {
                if (ip != "") di.Ip = ip;
                di.Key = key;
                return RegisterConsole(di);
            }
            return false;
        }
        public static bool RegisterConsole(string token, out DepotIdentity di, string ip = "")
        {
            DataTier session = GetSessionData(token);
            if (session != null)
            {
                di = SetConsoleIdentity(session);
                if (di != null)
                {
                    if (ip != "") di.Ip = ip;
                    di.Token = token;
                    return RegisterConsole(di);
                }
                return false;
            }
            di = null;
            return false;
        }

        public static bool VerifyMemberIdentity(DataTier member, string passwd)
        {
            bool verify = false;

            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();

            string hashpasswd = member["Password"].ToString();
            string saltpasswd = member["PasswordSalt"].ToString();
            verify = KeyHasher.Verify(hashpasswd, saltpasswd, passwd);

            return verify;
        }

        public static bool VerifyMemberToken(DataTier member, string token)
        {
            bool verify = false;

            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();

            DataTier session = member;

            string _token = session["Token"].ToString();

            if (_token.Equals(token))
            {
                DateTime time = DateTime.Now;
                DateTime registerTime = (DateTime)session["RegisterTime"];
                DateTime lastAction = (DateTime)session["LastAction"];
                DateTime lifeTime = (DateTime)session["LifeTime"];
                if (lifeTime > time)
                    verify = true;
                else if (lastAction > time.AddMinutes(-30))
                {
                    session["LifeTime"] = time.AddMinutes(30);
                    session["LastAction"] = time;
                    verify = true;
                }
            }
            return verify;
        }

        public static string CreateMemberToken(DataTier member)
        {
            string token = null;

            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();

            DataTier session = member;

            string _token = member["Password"].ToString();
            string timesalt = Convert.ToBase64String(DateTime.Now.Ticks.ToString().ToBytes(CharCoding.ASCII));
            token = KeyHasher.Encrypt(_token, 1, timesalt);
            session["Token"] = token;
            DateTime time = DateTime.Now;
            session["RegisterTime"] = time;
            session["LifeTime"] = time.AddMinutes(30);
            session["LastAction"] = time;
            return token;
        }
    }
}
