using System;
using System.Linq;
using System.Collections.Generic;
using System.Doors;

namespace System.Doors.Data
{
    public static partial class ConfigBuilder
    {
        private static class ConfigDesign
        {
            public static void ServicesBank(object parameters, out object results)
            {
                object result = new object();

                DataDeposit serviceDeposit = DataBank.Vault[ConfigBuilder.ConfigVaultsName].NewDeposit("Services");
                serviceDeposit.Config.Store = DataStore.Config;

                DataTrellis serviceTrell = new DataTrellis("Services");
                serviceTrell.Config.Store = DataStore.Config;
                serviceTrell.Tiers.Config.Store = DataStore.Config;

                serviceTrell.Pylons.AddRange(new DataPylon[]
                    {
                    new DataPylon(typeof(int),   "Id")              { isAutoincrement = true, isIdentity = true },
                    new DataPylon(typeof(Quid),   "ServiceId")       { isQuid = true, isKey = true, PylonSize = 8 },
                    new DataPylon(typeof(string), "Service")         { PylonSize = 20 },
                    new DataPylon(typeof(string), "Name")            { PylonSize = 50 },             
                    new DataPylon(typeof(string), "Host")            { PylonSize = 16 },
                    new DataPylon(typeof(ushort), "Port")            { PylonSize = 2 },
                    new DataPylon(typeof(ushort), "Limit")           { PylonSize = 2 },
                    new DataPylon(typeof(bool),   "Active")          { PylonSize = 1 },
                    new DataPylon(typeof(string), "Status")          { PylonSize = 50 },
                    new DataPylon(typeof(string), "ImageKey")        { PylonSize = 50 },
                    new DataPylon(typeof(bool),   "IsDepot")         { PylonSize = 1 },
                    new DataPylon(typeof(Quid),   "DepotId")         { isQuid = true, PylonSize = 8 },
                    });
                serviceTrell.PrimeKey = serviceTrell.Pylons.GetPylons(new List<string>() { "ServiceId" });
                serviceTrell.Deposit = serviceDeposit;
                serviceDeposit.Box.Prime = serviceTrell;

                DataTier tier = new DataTier(serviceTrell);              
                tier.PrimeArray = new object[]
                    {
                  -1,
                    DataBank.Vault.Config.GetServiceId("Depot.S.0.A", "127.0.0.1:44004"),
                    "Unisolution Depot",
                    "Depot.S.0.A",                   
                    "127.0.0.1",
                    (ushort)44004,
                    (ushort)250,
                    true,
                    "Local",
                    "red.ico",
                    true,
                    Quid.Empty
                    };
                serviceTrell.Tiers.Add(tier);

                results = serviceTrell;
            }
            public static void ConsolesBank(object parameters, out object results)
            {
                DataTrellis serviceTrell = (DataTrellis)parameters;

                object result = new object();

                DataDeposit consoleDeposit = DataBank.Vault[ConfigBuilder.ConfigVaultsName].NewDeposit("Consoles");
                consoleDeposit.Config.Store = DataStore.Config;

                DataTrellis consoleTrell = new DataTrellis("Consoles");
                consoleTrell.Config.Store = DataStore.Config;
                consoleTrell.Tiers.Config.Store = DataStore.Config;

                consoleTrell.Pylons.AddRange(new DataPylon[]
                    {
                    new DataPylon(typeof(int),      "Id")            { isAutoincrement = true, isIdentity = true },
                    new DataPylon(typeof(Quid),     "ConsoleId")     { isQuid = true, isKey = true, PylonSize = 8 },
                    new DataPylon(typeof(Quid),     "ServiceId")     { isQuid = true, PylonSize = 8 },
                    new DataPylon(typeof(string),   "Service")       { PylonSize = 20 },
                    new DataPylon(typeof(string),   "Name")          { PylonSize = 50 },
                    new DataPylon(typeof(string),   "Login")         { PylonSize = 50 },
                    new DataPylon(typeof(string),   "Password")      { PylonSize = 128 },
                    new DataPylon(typeof(string),   "PasswordSalt")  { PylonSize = 128 },
                    new DataPylon(typeof(string),   "DataPlace")     { PylonSize = 255 },
                    new DataPylon(typeof(string),   "Host")          { PylonSize = 16 },
                    new DataPylon(typeof(ushort),   "Port")          { PylonSize = 2 },
                    new DataPylon(typeof(bool),     "Active")        { PylonSize = 1 },
                    new DataPylon(typeof(string),   "Token")         { PylonSize = 50 },
                    new DataPylon(typeof(DateTime), "RegisterTime")  { PylonSize = 8 },
                    new DataPylon(typeof(DateTime), "LastAction")    { PylonSize = 8 },
                    new DataPylon(typeof(DateTime), "LifeTime")      { PylonSize = 8 },
                    new DataPylon(typeof(string),   "Status")        { PylonSize = 50 },
                    new DataPylon(typeof(string),   "ImageKey")      { PylonSize = 50 },                  
                    });
                consoleTrell.PrimeKey = consoleTrell.Pylons.GetPylons(new List<string>() { "ConsoleId" });
                consoleTrell.Deposit = consoleDeposit;
                consoleDeposit.Box.Prime = consoleTrell;

                DataTier tier = new DataTier(consoleTrell);
                DateTime time = DateTime.Now;
                string salt = KeyHasher.Salt();
                tier.PrimeArray = new object[]
                    {
                    -1,
                    DataBank.Vault.Config.GetServiceId("Depot.C.0.A", "Depot"),
                    serviceTrell.Tiers[0]["ServiceId"],
                    "Unisolution Depot",
                    "Depot.C.0.A",
                    "prime",
                    KeyHasher.Encrypt("H0rny&W3t", 1, salt),
                    salt,
                    "Space/Space.nds",
                    "",
                    (ushort)0,
                    true,
                    "",
                    time,
                    time,
                    time,
                    "Local",
                    "red.ico"
                    };

                consoleTrell.Tiers.Add(tier);

                results = null;
            }
            public static void DepotsBank(object parameters, out object results)
            {
                DataTrellis serviceTrell = (DataTrellis)parameters;

                object result = new object();

                DataDeposit depotDeposit = DataBank.Vault[ConfigBuilder.ConfigVaultsName].NewDeposit("Depots");
                depotDeposit.Config.Store = DataStore.Config;

                DataTrellis depotTrell = new DataTrellis("Depots");
                depotTrell.Config.Store = DataStore.Config;
                depotTrell.Tiers.Config.Store = DataStore.Config;

                depotTrell.Pylons.AddRange(new DataPylon[]
                    {
                    new DataPylon(typeof(int),   "Id")              { isAutoincrement = true, isIdentity = true },
                    new DataPylon(typeof(Quid),   "DepotId")         { isQuid = true, isKey = true, PylonSize = 8 },
                    new DataPylon(typeof(string), "Name")            { PylonSize = 50 },
                    new DataPylon(typeof(string), "IP")              { PylonSize = 16 },
                    new DataPylon(typeof(ushort), "Port")            { PylonSize = 2 },
                    new DataPylon(typeof(ushort), "Limit")           { PylonSize = 2 },
                    new DataPylon(typeof(ushort), "Scale")           { PylonSize = 2 },
                    new DataPylon(typeof(string), "Type")            { PylonSize = 10 },
                    new DataPylon(typeof(bool),   "Active")          { PylonSize = 1 },
                    new DataPylon(typeof(string), "Status")          { PylonSize = 10 },
                    new DataPylon(typeof(Quid),   "ConsoleId")         { isQuid = true, PylonSize = 8 },
                    new DataPylon(typeof(string), "Login")           { PylonSize = 128 },
                    new DataPylon(typeof(string), "Password")        { PylonSize = 128 },
                    new DataPylon(typeof(string), "PasswordSalt")    { PylonSize = 128 },
                    new DataPylon(typeof(string), "Token")           { PylonSize = 128 },
                    new DataPylon(typeof(string), "DataPlace")       { PylonSize = 255 },
                    });

                depotTrell.PrimeKey = depotTrell.Pylons.GetPylons(new List<string>() { "DepotId" });
                depotTrell.Deposit = depotDeposit;
                depotDeposit.Box.Prime = depotTrell;

                DataTier tier = new DataTier(depotTrell);
                DateTime time = DateTime.Now;
                string salt = KeyHasher.Salt();
                tier.PrimeArray = new object[]
                    {
                      -1,
                    DataBank.Vault.Config.GetDepotId("Depot.D.0.A", "127.0.0.1:44004"),
                    "Depot.D.0.A",
                    "127.0.0.1",
                    (ushort)44004,
                    (ushort)250,
                    (ushort)1,
                    "Service",
                    true,
                    "Local",
                    Quid.Empty,
                    "prime",
                    "H0rny&W3t",
                    salt,
                    "",
                    ""
                    };

                depotTrell.Tiers.Add(tier);

                Quid depotId = (Quid)depotTrell.Tiers[0]["DepotId"];
                serviceTrell.Tiers[0]["DepotId"] = depotId;

                results = null;
            }
            public static void SourcesBank(object parameters, out object results)
            {
                DataTrellis serviceTrell = (DataTrellis)parameters;

                object result = new object();

                DataDeposit sourceDeposit = DataBank.Vault[ConfigBuilder.ConfigVaultsName].NewDeposit("Sources");
                sourceDeposit.Config.Store = DataStore.Config;

                DataTrellis sourceTrell = new DataTrellis("Sources");
                sourceTrell.Config.Store = DataStore.Config;
                sourceTrell.Tiers.Config.Store = DataStore.Config;

                sourceTrell.Pylons.AddRange(new DataPylon[]
                    {
                    new DataPylon(typeof(int),   "Id")              { isAutoincrement = true, isIdentity = true },
                    new DataPylon(typeof(Quid),    "SourceId")     { isQuid = true, isKey = true, PylonSize = 8 },
                    new DataPylon(typeof(Quid),    "ServiceId")    { isQuid = true, PylonSize = 8 },
                    new DataPylon(typeof(string),  "Name")         { PylonSize = 100 },
                    new DataPylon(typeof(string),  "Server")       { PylonSize = 50 },
                    new DataPylon(typeof(ushort),  "Port")         { PylonSize = 2 },
                    new DataPylon(typeof(bool),    "Security")     { PylonSize = 1 },
                    new DataPylon(typeof(string),  "UserId")       { PylonSize = 50 },
                    new DataPylon(typeof(string),  "Password")     { PylonSize = 50 },
                    new DataPylon(typeof(string),  "Database")     { PylonSize = 100 },
                    new DataPylon(typeof(string),  "Type")         { PylonSize = 10 },
                    new DataPylon(typeof(DateTime),"LastUpdate")   { PylonSize = 8 },
                    new DataPylon(typeof(bool),    "Active")       { PylonSize = 1 },
                    new DataPylon(typeof(string),  "Status")       { PylonSize = 50 }
                    });

                sourceTrell.PrimeKey = sourceTrell.Pylons.GetPylons(new List<string>() { "SourceId" });
                sourceTrell.Deposit = sourceDeposit;
                sourceDeposit.Box.Prime = sourceTrell;

                DataTier tier = new DataTier(sourceTrell);

                tier.PrimeArray = new object[]
                    {
                     -1,
                    DataBank.Vault.Config.GetSourceId("UnisolutionQT", "127.0.0.1"),
                    serviceTrell.Tiers[0]["ServiceId"],
                    "Depot.R.0.A",
                    "127.0.0.1",
                    (ushort)0,
                    true,
                    "sa",
                    "$t0kkk3",
                    "UnisolutionQT",
                    "MsSql",
                    DateTime.Now,
                    true,
                    "Local"
                    };

                sourceTrell.Tiers.Add(tier);

                results = null;
            }
            public static void DrivesBank(object parameters, out object results)
            {
                DataTrellis serviceTrell = (DataTrellis)parameters;

                object result = new object();

                DataDeposit driveDeposit = DataBank.Vault[ConfigBuilder.ConfigVaultsName].NewDeposit("Drives");
                driveDeposit.Config.Store = DataStore.Config;

                DataTrellis driveTrell = new DataTrellis("Drives");
                driveTrell.Config.Store = DataStore.Config;
                driveTrell.Tiers.Config.Store = DataStore.Config;

                driveTrell.Pylons.AddRange(new DataPylon[]
                    {
                    new DataPylon(typeof(int),   "Id")              { isAutoincrement = true, isIdentity = true },
                    new DataPylon(typeof(Quid),    "ServiceId")   { isQuid = true, isKey = true, PylonSize = 8 },
                    new DataPylon(typeof(ushort),  "ClusterId")   { PylonSize = 2 },
                    new DataPylon(typeof(string),  "BankName")    { PylonSize = 100 },
                    new DataPylon(typeof(string),  "BankPath")    { PylonSize = 200 },
                    new DataPylon(typeof(string),  "SpaceName")   { PylonSize = 100 },
                    new DataPylon(typeof(string),  "SpacePath")   { PylonSize = 200 },
                    new DataPylon(typeof(int),     "DepotCount")  { PylonSize = 4 },
                    new DataPylon(typeof(bool),    "Active")      { PylonSize = 1 },
                    });

                driveTrell.PrimeKey = driveTrell.Pylons.GetPylons(new List<string>() { "ServiceId" });
                driveTrell.Deposit = driveDeposit;
                driveDeposit.Box.Prime = driveTrell;

                DataTier tier = new DataTier(driveTrell);
                tier.PrimeArray = new object[] { -1, serviceTrell.Tiers[0]["ServiceId"], (ushort)0x00AA, "Bank", "QDFS", "Space", "QDFS", 1, true };

                driveTrell.Tiers.Add(tier);

                results = null;
            }
            public static void NetlogBank(object parameters, out object results)
            {
                DataTrellis serviceTrell = (DataTrellis)parameters;
                Quid serviceId = (Quid)serviceTrell.Tiers[0]["ServiceId"];
                object result = new object();

                DataDeposit netlogDeposit = DataBank.Vault[ConfigBuilder.ConfigVaultsName].NewDeposit("Netlog");
                netlogDeposit.Config.Store = DataStore.Config;

                DataTrellis netlogTrell = new DataTrellis("Netlog");
                netlogTrell.Config.Store = DataStore.Config;
                netlogTrell.Tiers.Config.Store = DataStore.Config;

                netlogTrell.Pylons.AddRange(new DataPylon[]
                    {
                    new DataPylon(typeof(int), netlogTrell,   "Id")             { isAutoincrement = true, isIdentity = true, isKey = true },
                    new DataPylon(typeof(Quid), netlogTrell,  "ServiceId")      { isQuid = true, isKey = true, PylonSize = 8 },
                    new DataPylon(typeof(string),  "Service")                   { PylonSize = 30 },
                    new DataPylon(typeof(DateTime),"EventTime")                 { PylonSize = 8 },
                    new DataPylon(typeof(string),  "Message")                   { PylonSize = 800 },
                    new DataPylon(typeof(string),  "Type")                      { PylonSize = 20 },
                    new DataPylon(typeof(int),     "Level")                     { PylonSize = 100 },
                    });

                netlogTrell.PrimeKey = netlogTrell.Pylons.GetPylons(new List<string>() { "Id", "ServiceId" });
                netlogTrell.Deposit = netlogDeposit;
                netlogDeposit.Box.Prime = netlogTrell;

                DataTier tier = new DataTier(netlogTrell);
                tier.PrimeArray = new object[]
                    {
                     -1,
                    serviceId,
                    "Unisolution Depot",
                    DateTime.Now,
                    "Netlog Succesfully Created",
                    "Information",
                    1
                    };

                netlogTrell.Tiers.Add(tier);

                results = null;
            }
            public static void EchologBank(object parameters, out object results)
            {
                DataTrellis serviceTrell = (DataTrellis)parameters;
                Quid serviceId = (Quid)serviceTrell.Tiers[0]["ServiceId"];
                object result = new object();

                DataDeposit echologDeposit = DataBank.Vault[ConfigBuilder.ConfigVaultsName].NewDeposit("Echolog");
                echologDeposit.Config.Store = DataStore.Config;

                DataTrellis echologTrell = new DataTrellis("Echolog");
                echologTrell.Config.Store = DataStore.Config;
                echologTrell.Tiers.Config.Store = DataStore.Config;

                echologTrell.Pylons.AddRange(new DataPylon[]
                    {
                    new DataPylon(typeof(int), echologTrell,  "Id")              { isAutoincrement = true, isIdentity = true, isKey = true },
                    new DataPylon(typeof(Quid), echologTrell,  "ServiceId")   { isQuid = true, isKey = true, PylonSize = 8 },
                    new DataPylon(typeof(string),  "Service")     { PylonSize = 30 },
                    new DataPylon(typeof(DateTime),"EventTime")   { PylonSize = 8 },
                    new DataPylon(typeof(string),  "Message")     { PylonSize = 800 },
                    new DataPylon(typeof(string),  "Type")        { PylonSize = 20 },
                    new DataPylon(typeof(int),     "Level")       { PylonSize = 100 },
                    });

                echologTrell.PrimeKey = echologTrell.Pylons.GetPylons(new List<string>() { "Id", "ServiceId" });
                echologTrell.Deposit = echologDeposit;
                echologDeposit.Box.Prime = echologTrell;

                DataTier tier = new DataTier(echologTrell);
                tier.PrimeArray = new object[]
                    {
                   -1,
                    serviceId,
                    "Unisolution Depot",
                    DateTime.Now,
                    "Echolog Succesfully Created",
                    "Information",
                    1
                    };

                echologTrell.Tiers.Add(tier);

                results = null;
            }

            public static void ServicesSpace(object parameters, out object results)
            {
                object result = new object();

                DataTrellis serviceTrell = DataBank.Vault["Config"]["Services"].Trell;
                DataSpace.Area[ConfigVaultsName][ConfigSphereName].Trells.Add(serviceTrell);

                results = serviceTrell;
            }
            public static void ConsolesSpace(object parameters, out object results)
            {
                DataTrellis serviceTrell = (DataTrellis)parameters;

                object result = new object();

                DataTrellis consoleTrell = DataBank.Vault["Config"]["Consoles"].Trell;
                DataSpace.Area[ConfigBuilder.ConfigVaultsName][ConfigBuilder.ConfigSphereName].Trells.Add(consoleTrell);

                serviceTrell.AddRelay(consoleTrell, new string[] { "ServiceId" });

                results = null;
            }
            public static void DepotsSpace(object parameters, out object results)
            {
                DataTrellis serviceTrell = (DataTrellis)parameters;

                object result = new object();

                DataTrellis depotTrell = DataBank.Vault["Config"]["Depots"].Trell;
                DataSpace.Area[ConfigBuilder.ConfigVaultsName][ConfigBuilder.ConfigSphereName].Trells.Add(depotTrell);

                serviceTrell.AddRelay(depotTrell, new string[] { "DepotId" });

                results = null;
            }
            public static void SourcesSpace(object parameters, out object results)
            {
                DataTrellis serviceTrell = (DataTrellis)parameters;

                object result = new object();

                DataTrellis sourceTrell = DataBank.Vault["Config"]["Sources"].Trell;
                DataSpace.Area[ConfigBuilder.ConfigVaultsName][ConfigBuilder.ConfigSphereName].Trells.Add(sourceTrell);

                serviceTrell.AddRelay(sourceTrell, new string[] { "ServiceId" });

                results = null;
            }
            public static void DrivesSpace(object parameters, out object results)
            {
                DataTrellis serviceTrell = (DataTrellis)parameters;

                object result = new object();

                DataTrellis driveTrell = DataBank.Vault["Config"]["Drives"].Trell;
                DataSpace.Area[ConfigBuilder.ConfigVaultsName][ConfigBuilder.ConfigSphereName].Trells.Add(driveTrell);

                serviceTrell.AddRelay(driveTrell, new string[] { "ServiceId" });

                results = null;
            }
            public static void NetlogSpace(object parameters, out object results)
            {
                DataTrellis serviceTrell = (DataTrellis)parameters;

                object result = new object();

                DataTrellis netlogTrell = DataBank.Vault["Config"]["Netlog"].Trell;
                DataSpace.Area[ConfigBuilder.ConfigVaultsName][ConfigBuilder.ConfigSphereName].Trells.Add(netlogTrell);

                serviceTrell.AddRelay(netlogTrell, new string[] { "ServiceId" });

                results = null;
            }
            public static void EchologSpace(object parameters, out object results)
            {
                DataTrellis serviceTrell = (DataTrellis)parameters;

                object result = new object();

                DataTrellis echologTrell = DataBank.Vault["Config"]["Echolog"].Trell;
                DataSpace.Area[ConfigBuilder.ConfigVaultsName][ConfigBuilder.ConfigSphereName].Trells.Add(echologTrell);

                serviceTrell.AddRelay(echologTrell, new string[] { "ServiceId" });

                results = null;
            }
        }
    }
}
