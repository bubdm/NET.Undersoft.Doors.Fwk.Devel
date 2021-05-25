using System.Collections.Generic;
using System.Dors;

namespace System.Dors.Data
{
    public static class DriveConfig
    {
        public static Quid GetServiceId(string serviceName)
        {
            DataTier[] services = DataBank.Vault["Config"]["Services"].Collect("Name", serviceName);
            if (services.Length > 0)
            {
                DataTier sc = services[0];
                return (Quid)sc["ServiceId"];
            }

            return Quid.Empty;
        }

        public static string GetBankDrivePath()
        {
            return GetBankDrivePath(-1);
        }
        public static string GetSpaceDrivePath()
        {
            return GetSpaceDrivePath(-1);
        }

        public static string SetBankDrivePath(string path = "")
        {
            if(path != string.Empty)
                DataBank.Vault["Config"]["Drives"].Trell.Tiers[0]["BankPath"] = path;
            return DataBank.DrivePath = GetBankDrivePath(-1);
        }
        public static string SetSpaceDrivePath(string path = "")
        {
            if (path != string.Empty)
                DataBank.Vault["Config"]["Drives"].Trell.Tiers[0]["SpacePath"] = path;
            return DataSpace.DrivePath = GetSpaceDrivePath(-1);
        }

        public static string GetBankDrivePath(int Id)
        {           
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Drives"].Collect("Id", Id);
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                return sc["BankPath"].ToString();

            }
            return string.Empty;
        }
        public static string GetSpaceDrivePath(int Id)
        {
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Drives"].Collect("Id", Id);
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                return sc["SpacePath"].ToString();

            }
            return string.Empty;
        }      

        public static string GetBankDrivePath(string serviceName)
        {
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Drives"].Collect("ServiceId", GetServiceId(serviceName));
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                return sc["BankPath"].ToString();

            }
            return string.Empty;
        }
        public static string GetSpaceDrivePath(string serviceName)
        {
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Drives"].Collect("ServiceId", GetServiceId(serviceName));
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                return sc["SpacePath"].ToString();

            }
            return string.Empty;
        }

        public static ushort GetClusterId()
        {
            return GetClusterId(-1);
        }
        public static ushort GetClusterId(int Id)
        {
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Drives"].Collect("Id", Id);
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                return (ushort)sc["ClusterId"];

            }
            return 0;
        }
        public static ushort GetClusterId(string serviceName)
        {
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Drives"].Collect("ServiceId", GetServiceId(serviceName));
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                return (ushort)sc["ClusterId"];

            }
            return 0;
        }

        public static ushort SetClusterId(ushort clusterId = 0)
        {
            if (clusterId > 0 && clusterId < 1024)
                DataBank.Vault["Config"]["Drives"].Trell.Tiers[0]["ClusterId"] = clusterId;

            return DataBank.ClusterId = GetClusterId(-1);
        }

        public static string GetBankName()
        {
            return GetBankName(-1);
        }
        public static string GetSpaceName()
        {
            return GetSpaceName(-1);
        }

        public static string SetBankName(string name = "")
        {
            if (name != string.Empty)
                DataBank.Vault["Config"]["Drives"].Trell.Tiers[0]["BankName"] = name;
            return DataBank.BankName = GetBankName(-1);
        }
        public static string SetSpaceName(string name = "")
        {
            if (name != string.Empty)
                DataBank.Vault["Config"]["Drives"].Trell.Tiers[0]["SpaceName"] = name;
            return DataSpace.SpaceName = GetSpaceName(-1);
        }

        public static string GetBankName(int Id)
        {
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Drives"].Collect("Id", Id);
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                return sc["BankName"].ToString();

            }
            return string.Empty;
        }
        public static string GetSpaceName(int Id)
        {
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Drives"].Collect("Id", Id);
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                return sc["SpaceName"].ToString();

            }
            return string.Empty;
        }

        public static string GetBankName(string serviceName)
        {
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Drives"].Collect("ServiceId", GetServiceId(serviceName));
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                return sc["BankName"].ToString();

            }
            return string.Empty;
        }
        public static string GetSpaceName(string serviceName)
        {
            if (!DataBank.Vault.Have("Config")) ConfigBuilder.Build();
            DataTier[] services = DataBank.Vault["Config"]["Drives"].Collect("ServiceId", GetServiceId(serviceName));
            if (services.Length > 0)
            {
                DataTier sc = services[0];

                return sc["SpaceName"].ToString();

            }
            return string.Empty;
        }
    }
}
