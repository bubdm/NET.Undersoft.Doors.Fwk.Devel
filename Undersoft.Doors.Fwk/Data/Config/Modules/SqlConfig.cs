using System;
using System.Collections.Generic;
using System.Linq;
using System.Doors;

namespace System.Doors.Data
{
    public static class SqlConfig
    {
        public static string SqlConnectString()
        {
            SqlIdentity si = GetSqlIdentity();
            return si.ConnectionString;
        }

        public static string SqlConnectString(int Id)
        {
            SqlIdentity si = GetSqlIdentity(Id);
            return si.ConnectionString;
        }

        public static string SqlConnectString(string serviceName)
        {
            SqlIdentity si = GetSqlIdentity(serviceName);
            return si.ConnectionString;
        }

        public static SqlIdentity GetSqlIdentity()
        {
            return GetSqlIdentity(-1);
        }

        public static SqlIdentity GetSqlIdentity(int Id)
        {
            SqlIdentity ci = new SqlIdentity();
            if (!DataBank.Vault.Have("Config"))
            {
                ConfigBuilder.Build();
            } 

            DataTier[] services = DataBank.Vault["Config"]["Sources"].Collect("Id", Id);
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                ci.Id = (int)sc["Id"];
                ci.Port = (ushort)sc["Port"];
                ci.Server = sc["Server"].ToString() + ((ci.Port > 0) ? ":" + ci.Port.ToString() : "");
                ci.UserId = sc["UserId"].ToString();
                ci.Password = Convert.ToString(sc["Password"]);
                ci.Database = Convert.ToString(sc["Database"]);                
                bool.TryParse(sc["Security"].ToString(), out ci.Security);
                Enum.TryParse<SqlProvider>(sc["Type"].ToString(), out ci.Provider);
                ci.AuthId = DataBank.Vault.Config.GetSourceId(ci).ToString();               
                sc["SourceId"] = DataBank.Vault.Config.GetSourceId(ci);
                int l = ci.Password.Length;
            }
            return ci;
        }

        public static SqlIdentity GetSqlIdentity(string serviceName)
        {
            SqlIdentity ci = null;
            DataTier[] services = DataBank.Vault["Config"]["Sources"].Collect("Name", serviceName);
            if (services.Length > 0)
                ci = GetSqlIdentity((int)services[0]["Id"]);

            return ci;
        }

        public static SqlIdentity SetSqlIdentity(SqlIdentity si)
        {            
            if (!DataBank.Vault.Have("Config"))
            {
                ConfigBuilder.Build();
            }

            DataTier[] services = DataBank.Vault["Config"]["Sources"].Collect("Id", si.Id);
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                sc["Id"] = si.Id;
                sc["Port"] = (ushort)si.Port;
                sc["Server"] = si.Server;
                sc["UserId"] = si.UserId;
                sc["Password"] = si.Password;
                sc["Database"] = si.Database;
                si.AuthId = DataBank.Vault.Config.GetSourceId(si).ToString();
                sc["SourceId"] = DataBank.Vault.Config.GetSourceId(si);
                int l = si.Password.Length;
                sc.SaveChanges();
            }
            return si;
        }
    }
}
