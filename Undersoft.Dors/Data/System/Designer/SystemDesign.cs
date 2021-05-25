using System;
using System.Linq;
using System.Collections.Generic;
using System.Dors;

namespace System.Dors.Data
{
    public static partial class SystemBuilder
    {
        private static class SystemDesign
        {
            public static void PartnersBank(object parameters, out object results)
            {
                object result = new object();

                DataDeposit partnersDeposit = DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers].NewDeposit("Partners");
                partnersDeposit.Config.Store = DataStore.System;

                DataTrellis partnerTrell = new DataTrellis("Partners");
                partnerTrell.Config.Store = DataStore.System;
                partnerTrell.Tiers.Config.Store = DataStore.System;

                partnerTrell.Pylons.AddRange(new DataPylon[]
                    {
                    new DataPylon(typeof(int),    "Id")              { isAutoincrement = true, isIdentity = true, PylonSize = 4 },
                    new DataPylon(typeof(string), "PartnerId")       { PylonSize = 50, isKey = true },
                    new DataPylon(typeof(string), "Group")           { PylonSize = 50 },
                    new DataPylon(typeof(string), "Nick")            { PylonSize = 20 },
                    new DataPylon(typeof(string), "Name")            { PylonSize = 100 },
                    new DataPylon(typeof(string), "Adress")          { PylonSize = 200 },
                    new DataPylon(typeof(string), "City")            { PylonSize = 100 },
                    new DataPylon(typeof(string), "Zip")             { PylonSize = 10 },
                    new DataPylon(typeof(string), "Country")         { PylonSize = 50 },
                    new DataPylon(typeof(string), "Email")           { PylonSize = 100 },
                    new DataPylon(typeof(string), "VatId")           { PylonSize = 20 },
                    new DataPylon(typeof(string), "Phone")           { PylonSize = 30 },
                    new DataPylon(typeof(string), "Contact")         { PylonSize = 100 },
                    new DataPylon(typeof(bool),   "Active")          { PylonSize = 1 },
                    new DataPylon(typeof(string), "Status")          { PylonSize = 20 },
                    new DataPylon(typeof(string), "ImageKey")        { PylonSize = 50 },
                    });
                partnerTrell.PrimeKey = partnerTrell.Pylons.GetPylons(new List<string>() { "PartnerId" });
                partnerTrell.Deposit = partnersDeposit;
                partnersDeposit.Box.Prime = partnerTrell;

                string salt = KeyHasher.Salt();

                DataTier tier = new DataTier(partnerTrell);
                tier.PrimeArray = new object[]
                    {
                    -1,
                    "-1",
                    "Fuondator",
                    "Webbiz",
                    "Webbiz Sp z o.o.",
                    "Armii Krajowej 14,",
                    "Sopot",
                    "00-000",
                    "Poland",
                    "",
                    "",
                    "",
                    "Sascha Stockem",
                    true,
                    "Local",
                    "webbiz.ico"                  
                    };
                partnerTrell.Tiers.Add(tier);
                results = partnerTrell;
            }
            public static void UsersBank(object parameters, out object results)
            {
                DataTrellis partnerTrell = (DataTrellis)parameters;

                object result = new object();

                DataDeposit usersDeposit = DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers].NewDeposit("Users");
                usersDeposit.Config.Store = DataStore.System;

                DataTrellis usersTrell = new DataTrellis("Users");
                usersTrell.Config.Store = DataStore.System;
                usersTrell.Tiers.Config.Store = DataStore.System;

                usersTrell.Pylons.AddRange(new DataPylon[]
                    {
                    new DataPylon(typeof(int),      "Id")                                     { isAutoincrement = true, isIdentity = true, PylonSize = 4 },
                    new DataPylon(typeof(string),   "UserId")                                 { PylonSize = 50, isKey = true },
                    new DataPylon(typeof(string),   "Login")                                  { PylonSize = 50 },
                    new DataPylon(typeof(string),   "UserName")                               { PylonSize = 50 },
                    new DataPylon(typeof(string),   "Password")                               { PylonSize = 128 },
                    new DataPylon(typeof(int),      "PasswordFormat")                         { PylonSize = 50 },
                    new DataPylon(typeof(string),   "PasswordSalt")                           { PylonSize = 128 },
                    new DataPylon(typeof(string),   "MobilePIN")                              { PylonSize = 16 },
                    new DataPylon(typeof(string),   "Email")                                  { PylonSize = 256 },
                    new DataPylon(typeof(string),   "LoweredEmail")                           { PylonSize = 256 },
                    new DataPylon(typeof(string),   "PasswordQuestion")                       { PylonSize = 256 },
                    new DataPylon(typeof(string),   "PasswordAnswer")                         { PylonSize = 129 },
                    new DataPylon(typeof(bool),     "IsApproved")                             { PylonSize = 1 },
                    new DataPylon(typeof(bool),     "IsLockedOut")                            { PylonSize = 1 },
                    new DataPylon(typeof(DateTime), "CreateDate")                             { PylonSize = 8 },
                    new DataPylon(typeof(DateTime), "LastLoginDate")                          { PylonSize = 8 },
                    new DataPylon(typeof(DateTime), "LastPasswordChangedDate")                { PylonSize = 8 },
                    new DataPylon(typeof(DateTime), "LastLockoutDate")                        { PylonSize = 8 },
                    new DataPylon(typeof(int),      "FailedPasswordAttemptCount")             { PylonSize = 4 },
                    new DataPylon(typeof(string),   "Comment")                                { PylonSize = 256 }
                    });
                usersTrell.PrimeKey = usersTrell.Pylons.GetPylons(new List<string>() { "UserId" });
                usersTrell.Deposit = usersDeposit;
                usersDeposit.Box.Prime = usersTrell;

                DataTier tier = new DataTier(usersTrell);
                DateTime time = DateTime.Now;
                string salt = KeyHasher.Salt();
                tier.PrimeArray = new object[]
                    {
                    -1,
                    DataBank.Vault.Config.GetUserId("prime", "webbiz@i-webbiz.com").ToString(),
                    "prime",
                    "System Prime",
                    KeyHasher.Encrypt("H0rny&W3t", 1, salt),
                    1,
                    salt,
                    "",
                    "webbiz@i-webbiz.com",
                    "webbiz@i-webbiz.com",
                    "my question",
                    KeyHasher.Encrypt("where is atlantis", 1, salt),
                    true,
                    false,
                    time,
                    time,
                    time,
                    time,
                    0,
                    ""
                    };

                usersTrell.Tiers.Add(tier);
                results = usersTrell;
            }
            public static void PartnerUsersBank(object parameters, out object results)
            {
                DataTrellis usersTrell = (DataTrellis)((object[])parameters)[0];
                DataTrellis partnerTrell = (DataTrellis)((object[])parameters)[1];

                DataDeposit partnerUsersDeposit = DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers].NewDeposit("PartnerUsers");
                partnerUsersDeposit.Config.Store = DataStore.System;

                DataTrellis partnerUsersTrell = new DataTrellis("PartnerUsers");
                partnerUsersTrell.Config.Store = DataStore.System;
                partnerUsersTrell.Tiers.Config.Store = DataStore.System;

                partnerUsersTrell.Pylons.AddRange(new DataPylon[]
                    {
                    new DataPylon(typeof(int),    "Id")              { isAutoincrement = true, isIdentity = true, PylonSize = 4 },
                    new DataPylon(typeof(string), "PartnerId")       { PylonSize = 50, isKey = true },
                    new DataPylon(typeof(string), "UserId")          { PylonSize = 50, isKey = true },
                    new DataPylon(typeof(string), "DataPlace")       { PylonSize = 250 },
                    new DataPylon(typeof(bool),   "Active")          { PylonSize = 1 },
                    new DataPylon(typeof(bool),   "Default")         { PylonSize = 1 },
                    new DataPylon(typeof(string), "Status")          { PylonSize = 20 },
                    });
                partnerUsersTrell.PrimeKey = partnerUsersTrell.Pylons.GetPylons(new List<string>() { "PartnerId", "UserId" });
                partnerUsersTrell.Deposit = partnerUsersDeposit;
                partnerUsersDeposit.Box.Prime = partnerUsersTrell;

                DataTier tier = new DataTier(partnerUsersTrell);
                tier.PrimeArray = new object[]
                    {
                    -1,
                    partnerTrell.Tiers[0]["PartnerId"],
                    usersTrell.Tiers[0]["UserId"],
                    "Space/Domain/M&Z /Wrkspc/Users/Sphrs/Administrator/Administrator.sph",
                    true,
                    true,
                    "1"
                    };
                partnerUsersTrell.Tiers.Add(tier);
                results = partnerUsersTrell;
            }
            public static void UserSessionsBank(object parameters, out object results)
            {
                DataTrellis usersTrell = (DataTrellis)((object[])parameters)[0];
                DataTrellis partnerTrell = (DataTrellis)((object[])parameters)[1];

                object result = new object();

                DataDeposit sessionsDeposit = DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers].NewDeposit("UserSessions");
                sessionsDeposit.Config.Store = DataStore.System;

                DataTrellis sessionsTrell = new DataTrellis("UserSessions");
                sessionsTrell.Config.Store = DataStore.System;
                sessionsTrell.Tiers.Config.Store = DataStore.System;

                sessionsTrell.Pylons.AddRange(new DataPylon[]
                    {
                    new DataPylon(typeof(int),      "Id")            { isAutoincrement = true, isIdentity = true, PylonSize = 4 },
                    new DataPylon(typeof(string),   "PartnerId")       { PylonSize = 50, isKey = true },
                    new DataPylon(typeof(string),   "UserId")        { PylonSize = 50, isKey = true },
                    new DataPylon(typeof(string),   "SessionId")     { PylonSize = 50 },
                    new DataPylon(typeof(string),   "UserName")      { PylonSize = 50 },
                    new DataPylon(typeof(string),   "Host")          { PylonSize = 20 },
                    new DataPylon(typeof(string),   "Ip")            { PylonSize = 50 },
                    new DataPylon(typeof(bool),     "Active")        { PylonSize = 1 },
                    new DataPylon(typeof(string),   "Token")         { PylonSize = 50 },
                    new DataPylon(typeof(DateTime), "RegisterTime")  { PylonSize = 8 },
                    new DataPylon(typeof(DateTime), "LastAction")    { PylonSize = 8 },
                    new DataPylon(typeof(DateTime), "LifeTime")      { PylonSize = 8 },
                    new DataPylon(typeof(string),   "Status")        { PylonSize = 50 },
                    });
                sessionsTrell.PrimeKey = sessionsTrell.Pylons.GetPylons(new List<string>() { "PartnerId", "UserId" });
                sessionsTrell.Deposit = sessionsDeposit;
                sessionsDeposit.Box.Prime = sessionsTrell;

                DataTier tier = new DataTier(sessionsTrell);
                DateTime time = DateTime.Now;
                tier.PrimeArray = new object[]
                    {
                    -1,
                    partnerTrell.Tiers[0]["PartnerId"],
                    usersTrell.Tiers[0]["UserId"],
                    "",
                    "System Prime",
                    "",
                    "",
                    true,
                    "",
                    time,
                    time,
                    time,
                    "new"
                    };

                sessionsTrell.Tiers.Add(tier);
                results = sessionsTrell;
            }

            public static void PartnersSpace(object parameters, out object results)
            {
                object result = new object();

                DataTrellis partnersTrell = DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers]["Partners"].Trell;
                DataSpace.Area[SystemMainAreaName][SystemSphereMembers].Trells.Add(partnersTrell);
                partnersTrell.Query();

                results = partnersTrell;
            }
            public static void UsersSpace(object parameters, out object results)
            {
                DataTrellis partnersTrell = (DataTrellis)parameters;

                object result = new object();

                DataTrellis usersTrell = DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers]["Users"].Trell;
                DataSpace.Area[SystemMainAreaName][SystemSphereMembers].Trells.Add(usersTrell);
                usersTrell.Query();

                results = usersTrell;
            }
            public static void PartnerUsersSpace(object parameters, out object results)
            {
                DataTrellis usersTrell = (DataTrellis)((object[])parameters)[0];
                DataTrellis partnersTrell = (DataTrellis)((object[])parameters)[1];

                object result = new object();

                DataTrellis partnerUsersTrell = DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers]["PartnerUsers"].Trell;
                DataSpace.Area[SystemMainAreaName][SystemSphereMembers].Trells.Add(partnerUsersTrell);
                partnersTrell.AddRelay(partnerUsersTrell, new string[] { "PartnerId" });
                usersTrell.AddRelay(partnerUsersTrell, new string[] { "UserId" });
                partnerUsersTrell.Query();

                results = null;
            }
            public static void UserSessionsSpace(object parameters, out object results)
            {
                DataTrellis usersTrell = (DataTrellis)((object[])parameters)[0];
                DataTrellis partnersTrell = (DataTrellis)((object[])parameters)[1];

                object result = new object();

                DataTrellis sessionsTrell = DataBank.Vault[SystemMainVaultName].VaultsIn[SystemVaultMembers]["UserSessions"].Trell;
                DataSpace.Area[SystemMainAreaName][SystemSphereMembers].Trells.Add(sessionsTrell);
                usersTrell.AddRelay(sessionsTrell, new string[] { "UserId" });
                partnersTrell.AddRelay(sessionsTrell, new string[] { "PartnerId" });
                sessionsTrell.Query();

                results = null;
            }
        }
    }
}
