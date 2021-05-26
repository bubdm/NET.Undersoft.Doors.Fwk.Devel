using System;
using System.Linq;
using System.Collections.Generic;
using System.Doors;

namespace System.Doors.Data
{
    public static partial class ConfigBuilder
    {
        public readonly static string ConfigVaultsName = "Config";
        public readonly static string ConfigAreaName   = "Config";
        public readonly static string ConfigSphereName = "Services";
        private static DepotSite site;
        
        public static void Build(DepotSite Site = DepotSite.Client)
        {
            site = Site;
            object result = null;
            BankBuilder(null, out result);
            SpaceBuilder(null, out result);
        }

        private static void BankBuilder(object parameters, out object results)
        {
            DataBank.Vault.Create(ConfigVaultsName);
            DataBank.Vault[ConfigVaultsName].Config.Store = DataStore.Config;
            DataBank.Vault[ConfigVaultsName].OpenDrive();

            DataSpace.Area.Create(ConfigAreaName);
            DataSpace.Area[ConfigAreaName].Config.Store = DataStore.Config;
            DataSpace.Area[ConfigAreaName].OpenDrive();

            if (DataBank.Vault[ConfigVaultsName].Drive.Exists)
            {
                DataBank.Vault[ConfigVaultsName].ReadDrive();
                foreach (DataDeposit deposit in DataBank.Vault[ConfigVaultsName].Values)
                    deposit.Trell.Tiers.ReadDrive();
            }
            else
            {
                object serviceTrell = null;
                ConfigDesign.ServicesBank(null, out serviceTrell);

                object result = null;
                ConfigDesign.ConsolesBank(serviceTrell, out result);
                ConfigDesign.DepotsBank(serviceTrell, out result);
                ConfigDesign.SourcesBank(serviceTrell, out result);
                ConfigDesign.DrivesBank(serviceTrell, out result);
                ConfigDesign.NetlogBank(serviceTrell, out result);
                ConfigDesign.EchologBank(serviceTrell, out result);

                DataBank.Vault[ConfigVaultsName].WriteDrive();

                foreach (DataDeposit deposit in DataBank.Vault[ConfigVaultsName].Values)
                    deposit.Trell.Tiers.WriteDrive();
            }
            if (DataCore.Bank.DataLog == null)
            {
                EchologConfig ce = new EchologConfig(new Quid(DepotConfig.LocalService.AuthId), "BizDepot");
                DataCore.Bank.DataLog = ce;
            }
            results = null;
        }

        private static void SpaceBuilder(object parameters, out object results)
        {
            if (site == DepotSite.Server)
                DepotConfig.SetSpaceDepotId();

            DataSpace.Area[ConfigAreaName].NewSphere(ConfigSphereName);
            DataSpace.Area[ConfigAreaName][ConfigSphereName].Config.Store = DataStore.Config;
            DataSpace.Area[ConfigAreaName][ConfigSphereName].OpenDrive();

            if (DataSpace.Area[ConfigAreaName][ConfigSphereName].Drive.Exists)
            {
                DataSpace.Area[ConfigAreaName][ConfigSphereName].ReadDrive(); 
            }
            else
            {              

                object serviceTrell = null;
                ConfigDesign.ServicesSpace(null, out serviceTrell);

                object result = null;
                ConfigDesign.ConsolesSpace(serviceTrell, out result);
                ConfigDesign.DepotsSpace(serviceTrell, out result);
                ConfigDesign.SourcesSpace(serviceTrell, out result);
                ConfigDesign.DrivesSpace(serviceTrell, out result);
                ConfigDesign.NetlogSpace(serviceTrell, out result);
                ConfigDesign.EchologSpace(serviceTrell, out result);

                DataSpace.Area[ConfigAreaName][ConfigSphereName].WriteDrive();
            }

            results = null;
        }     
    }
}
