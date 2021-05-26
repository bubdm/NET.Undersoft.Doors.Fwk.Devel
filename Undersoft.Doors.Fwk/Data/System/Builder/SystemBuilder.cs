using System;
using System.Linq;
using System.Collections.Generic;
using System.Doors;

namespace System.Doors.Data
{
    public static partial class SystemBuilder
    {
        public readonly static string SystemMainVaultName = "System";
        public readonly static string SystemMainAreaName =  "System";
        public readonly static string SystemVaultsName =    "Areas";
        public readonly static string SystemVaultMembers =  "Members";
        public readonly static string SystemSphereMembers = "Members";
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
            DataBank.Vault.Create(SystemMainVaultName);
            DataBank.Vault[SystemMainVaultName].Config.Store = DataStore.System;
            DataBank.Vault[SystemMainVaultName].NewVaults(SystemVaultsName);
            DataBank.Vault[SystemMainVaultName].VaultsIn.Create(SystemVaultMembers);
            DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers].Config.Store = DataStore.System;
            DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers].OpenDrive();

            DataSpace.Area.Create(SystemMainAreaName);
            DataSpace.Area[SystemMainAreaName].Config.Store = DataStore.System;
            DataSpace.Area[SystemMainAreaName].OpenDrive();

            if (DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers].Drive.Exists)
            {
                DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers].ReadDrive();
                foreach (DataDeposit deposit in DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers].Values)
                    deposit.Trell.Tiers.ReadDrive();
            }
            else
            {
                object partnersTrell = null;
                SystemDesign.PartnersBank(null, out partnersTrell);

                object usersTrell = null;
                SystemDesign.UsersBank(partnersTrell, out usersTrell);

                object sessionsTrell = null;
                SystemDesign.UserSessionsBank(new object[] { usersTrell, partnersTrell }, out sessionsTrell);

                object result = null;
                SystemDesign.PartnerUsersBank(new object[] { usersTrell, partnersTrell }, out result);

                DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers].WriteDrive();

                foreach (DataDeposit deposit in DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers].Values)
                    deposit.Trell.Tiers.WriteDrive();
            }

            if (DataCore.Bank.DataLog == null)
            {
                EchologConfig ce = new EchologConfig(new Quid(ConfidentConfig.GetLocalService().AuthId), "BizDepot");
                DataCore.Bank.DataLog = ce;
            }

            results = null;
        }

        private static void SpaceBuilder(object parameters, out object results)
        {
            if (site == DepotSite.Server)
                DepotConfig.SetSpaceDepotId();

            DataSpace.Area[SystemMainAreaName].NewSphere(SystemSphereMembers);
            DataSpace.Area[SystemMainAreaName][SystemSphereMembers].Config.Store = DataStore.System;
            DataSpace.Area[SystemMainAreaName][SystemSphereMembers].OpenDrive();

            if (DataSpace.Area[SystemMainAreaName][SystemSphereMembers].Drive.Exists)
            {
                DataSpace.Area[SystemMainAreaName][SystemSphereMembers].ReadDrive();
            }
            else
            {
                object partnersTrell = null;
                SystemDesign.PartnersSpace(null, out partnersTrell);

                object usersTrell = null;
                SystemDesign.UsersSpace(partnersTrell, out usersTrell);

                object sessionsTrell = null;
                SystemDesign.UserSessionsSpace(new object[] { usersTrell, partnersTrell }, out sessionsTrell);

                object result = null;
                SystemDesign.PartnerUsersSpace(new object[] { usersTrell, partnersTrell }, out result);

                DataSpace.Area[SystemMainAreaName][SystemSphereMembers].WriteDrive();
            }

            results = null;
        }     
    }
}
