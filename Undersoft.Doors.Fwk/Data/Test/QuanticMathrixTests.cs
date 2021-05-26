/*
    Dors Data Test Examples
    
    Demonstracyjne testy podstawowych funkcji biblioteki Dors
    Sopot 2018-06-19

    wersja 1.07

*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Doors.Data;
using System.Doors;
using System.Doors.Drive;
using System.Doors.MultiTask;
using System.Doors.Mathtab;
using System.Diagnostics;

namespace System.Doors.Data.Tests
{
    public static class DorsMathrixTests
    {
        private static string rootDir = "QDFS_Tests";
        private static IDorsEvent writeEcho;
        private static bool firstRun = true;
        public static DataTrellis sales = null;
        public static DataTrellis stocks = null;
        public static DataTrellis stockTraffic = null;
        public static DataTrellis salesPrime = null;
        public static DataTrellis stocksPrime = null;
        public static DataTrellis stockTrafficPrime = null;

        // metoda inicjująca test
        public static void RunTests(IDorsEvent WriteEcho, string RootDir = null)
        {
            if(RootDir != null)
                rootDir = RootDir;

            writeEcho = WriteEcho;

            try
            {
                // odczyt z konfiguracji nazw dla DataBank, DataSpace, ID klastra oraz 
                // ustalenie ścieżki do katalogów zawierających pliki danych
                DriveConfig.SetBankName();
                
                // pobranie oryginalnej ścieżki banku w celu odtworzenia 
                string originBankPath = DriveConfig.GetBankDrivePath();

                DriveConfig.SetBankDrivePath(rootDir);
                DriveConfig.SetClusterId();

                DriveConfig.SetSpaceName();

                // pobranie oryginalnej ścieżki przestrzeni w celu odtworzenia 
                string originSpacePath = DriveConfig.GetSpaceDrivePath();

                DriveConfig.SetSpaceDrivePath(rootDir);

                //
                // utworzenie struktury
                //              

                /////////////////////////////////////////////////////////// BANK CREATE /////////////////////////////////////////////////////////////

                DataBank.Vault.OpenDrive();
                if (firstRun)
                {
                    if (DataBank.Vault.Drive.Exists)
                    {
                        DataBank.Vault.ReadDrive();

                        salesPrime = DataBank.Vault["Data"]["Sales"].Trell;
                        stocksPrime = DataBank.Vault["Data"]["Stocks"].Trell;
                        stockTrafficPrime = DataBank.Vault["Data"]["StockTraffic"].Trell;
                    }
                    else
                    {
                        // Tworzenie obszaru DataBank - główny, wspólny kontener danych
                        DataVault systemVlt = DataBank.Vault.Create("Data");
                        writeEcho.Execute("Utworzenie Vault " + systemVlt.DisplayName);
                        systemVlt.Create("Sales");
                        writeEcho.Execute("Utworzenie Vault " + systemVlt.DisplayName + "/" + DataBank.Vault["Data"]["Sales"].DisplayName);

                        //
                        // definicja tabeli (Trellis) zawierającej dane użytkowników systemu
                        // stosowanie unikalnych (w skali bazy danych) nazw pól pozwala na skorzystanie z możliwości automatyzacji relacji pól
                        //

                        DataTrellis table1 = new DataTrellis("Sales");
                        table1.Pylons.AddRange(new DataPylon[]
                            {
                                new DataPylon(typeof(Quid), table1,   "ID")         { isQuid = true, isKey = true, isIdentity = true },
                                new DataPylon(typeof(string), "ProductID")         { PylonSize = 24 },
                                new DataPylon(typeof(double), "PriceNet")    {  },
                                new DataPylon(typeof(double), "Quantity")       { },
                                new DataPylon(typeof(double), "Tax")       { },
                                new DataPylon(typeof(double), "PriceGross")       { },
                                new DataPylon(typeof(double),    "AmountNet")     {  },
                                new DataPylon(typeof(double),    "AmountGross")     {  },
                                new DataPylon(typeof(string), "Name")         { PylonSize = 16 },
                                new DataPylon(typeof(int), "Active0")         {  },
                                new DataPylon(typeof(bool), "Active1")         {  },
                                new DataPylon(typeof(bool), "Active2")         {  },
                                new DataPylon(typeof(bool), "Active3")         {  },
                                 new DataPylon(typeof(bool), "Active4")         {  }
                            });
                        // klucze główne zdefiniowanej tabeli
                        table1.PrimeKey = table1.Pylons.GetPylons(new string[] { "ID" });

                        DataBank.Vault["Data"]["Sales"].Trell = table1;

                        systemVlt.Create("Stocks");
                        writeEcho.Execute("Utworzenie Vault " + systemVlt.DisplayName + "/" + DataBank.Vault["Data"]["Stocks"].DisplayName);

                        DataTrellis table2 = new DataTrellis("Stocks");
                        table2.Pylons.AddRange(new DataPylon[]
                            {
                                new DataPylon(typeof(Quid), table2,   "ID")         { isQuid = true, isKey = true, isIdentity = true },
                                new DataPylon(typeof(string), "ProductID")         { PylonSize = 30 },
                                new DataPylon(typeof(double), "CostNet")    {  },
                                new DataPylon(typeof(double), "Quantity")       { },
                                new DataPylon(typeof(double), "Tax")       { },
                                new DataPylon(typeof(double), "CostGross")       { },
                                new DataPylon(typeof(double),    "AmountNet")     {  },
                                new DataPylon(typeof(double),    "AmountGross")     {  },
                            });
                        // klucze główne zdefiniowanej tabeli
                        table2.PrimeKey = table2.Pylons.GetPylons(new string[] { "ID" });

                        DataBank.Vault["Data"]["Stocks"].Trell = table2;

                        systemVlt.Create("StockTraffic");
                        writeEcho.Execute("Utworzenie Vault " + systemVlt.DisplayName + "/" + DataBank.Vault["Data"]["StockTraffic"].DisplayName);

                        DataTrellis table3 = new DataTrellis("StockTraffic");
                        table3.Pylons.AddRange(new DataPylon[]
                            {
                                new DataPylon(typeof(Quid), table3,   "ID")         { isQuid = true, isKey = true, isIdentity = true },
                                new DataPylon(typeof(string), "ProductID")         { PylonSize = 30 },
                                new DataPylon(typeof(double), "CostNet")    {  },
                                new DataPylon(typeof(double), "Quantity")       { },
                                 new DataPylon(typeof(double), "QuantityIn")       { },
                                 new DataPylon(typeof(double), "QuantityOut")       { },
                                new DataPylon(typeof(double), "Tax")       { },
                                  new DataPylon(typeof(DateTime), "Update")       { },
                                new DataPylon(typeof(DateTime), "Create")       { },
                                new DataPylon(typeof(double),    "AmountNet")     {  },
                                new DataPylon(typeof(double),    "AmountGross")     {  },
                            });
                        // klucze główne zdefiniowanej tabeli
                        table3.PrimeKey = table3.Pylons.GetPylons(new string[] { "ID" });

                        DataBank.Vault["Data"]["StockTraffic"].Trell = table3;
                       
                        salesPrime = table1;
                        stocksPrime = table2;
                        stockTrafficPrime = table3;                       

                        DataBank.Vault.WriteDrive();
                        DataBank.Vault.Drive.Exists = true;

                    }

                    Mattab mx2 = salesPrime.Pylons["AmountNet"].GetMattab(DataModes.Tiers);
                    mx2.MattabFormula = (mx2["PriceNet"] * mx2["Quantity"]);
                    salesPrime.Pylons["AmountNet"].ComputeOrdinal = 1;

                    Mattab mx3 = salesPrime.Pylons["PriceGross"].GetMattab(DataModes.Tiers);
                    mx3.MattabFormula = (mx3["PriceNet"] * mx3["Tax"]);
                    salesPrime.Pylons["PriceGross"].ComputeOrdinal = 0;

                    Mattab mx4 = salesPrime.Pylons["AmountGross"].GetMattab(DataModes.Tiers);
                    mx4.MattabFormula = mx3 * /* mx4["PriceGross"] */ mx4["Quantity"];
                    salesPrime.Pylons["AmountGross"].ComputeOrdinal = 2;

                    Mattab mx6 = stocksPrime.Pylons["AmountNet"].GetMattab(DataModes.Tiers);
                    mx6.MattabFormula = mx6["CostNet"] * mx6["Quantity"];
                    stocksPrime.Pylons["AmountNet"].ComputeOrdinal = 0;

                    Mattab mx5 = stocksPrime.Pylons["AmountGross"].GetMattab(DataModes.Tiers);
                    mx5.MattabFormula = mx6 * mx5["Tax"];
                    stocksPrime.Pylons["AmountGross"].ComputeOrdinal = 1;
                }
                /////////////////////////////////////////////////////////// SPACE CREATE /////////////////////////////////////////////////////////////


                if (firstRun)
                {
                    DataSpace.Area.OpenDrive();
                    if (DataSpace.Area.Drive.Exists)
                    {
                        DataSpace.Area.ReadDrive();
                        sales = DataSpace.Area["Domain"]["Inventory", "Sales"];
                        stocks = DataSpace.Area["Domain"]["Inventory", "Stocks"];
                        stockTraffic = DataSpace.Area["Domain"]["Inventory", "StockTraffic"];

                        sales.GetMessage();
                        stocks.GetMessage();
                        stockTraffic.GetMessage();
                    }
                    else
                    {
                        // Tworzenie obszaru DataSpace - dane i widoki użytkowników
                        DataSpheres sysSpace = DataSpace.Area.Create("Data");
                        writeEcho.Execute("Utworzenie obszaru sfer " + sysSpace.GetMapPath() + "/" + sysSpace.DisplayName);
                        DataSphere inventory = sysSpace.NewSphere("Inventory");
                        writeEcho.Execute("Utworzenie sfery " + inventory.GetMapPath() + "/" + inventory.DisplayName);

                        inventory.Trells.Add(DataBank.Vault["Data"]["Sales"].Trell);
                        inventory.Trells.Add(DataBank.Vault["Data"]["Stocks"].Trell);
                        inventory.Trells.Add(DataBank.Vault["Data"]["StockTraffic"].Trell);

                        sales = DataSpace.Area["Data"]["Inventory", "Sales"];
                        stocks = DataSpace.Area["Data"]["Inventory", "Stocks"];
                        stockTraffic = DataSpace.Area["Data"]["Inventory", "StockTraffic"];

                        //
                        // uzupełnienie tabeli User o rekordy z danymi użytkownikami - sposób 1
                        //               

                        for (double i = 0; i < 50000; i++)
                        {
                            DataTier tier = new DataTier(sales);
                            tier.DataArray = new object[] { Quid.Empty, "PRD_" + i, 0.01d * i, 0.1d + i, 1.23 };//(1d * 10) * (i+1), 20d * (i+1), 1.23d };
                            sales.Tiers.Put(tier);
                        }

                        sales.Tiers.SaveChanges();

                        for (double i = 0; i < 50000; i++)
                        {
                            DataTier tier = new DataTier(stocks);
                            tier.DataArray = new object[] { Quid.Empty, "PRD_" + i, 0.01d * i, 0.1d + i, 1.23d };
                            stocks.Tiers.Put(tier);
                        }

                        stocks.Tiers.SaveChanges();


                        for (double i = 0; i < 50000; i++)
                        {
                            for (double j = 0; j < 2; j++)
                            {
                                DataTier tier = new DataTier(stockTraffic);
                                tier.DataArray = new object[] { Quid.Empty, "PRD_" + i, 0.01d * i + j, 0.1d + i + j, 0.1d + i, 0.1d + i, 1.23d, DateTime.Now, DateTime.Now };
                                stockTraffic.Tiers.Put(tier);
                            }
                        }

                        stockTraffic.Tiers.SaveChanges();

                        sales.AddRelay(stocks, new string[] { "ProductID" });

                        stocks.AddRelay(stockTraffic, new string[] { "ProductID" });                      

                        DataSpheres domSpace = DataSpace.Area.Create("Domain");

                        domSpace.Emulate(sysSpace);

                        domSpace.SaveTrells();

                        DataSpace.Area.WriteDrive();
                        DataSpace.Area.Drive.Exists = true;

                        sales = DataSpace.Area["Domain"]["Inventory", "Sales"];
                        stocks = DataSpace.Area["Domain"]["Inventory", "Stocks"];
                        stockTraffic = DataSpace.Area["Domain"]["Inventory", "StockTraffic"];
                    }                   

                    Mattab mx2 = sales.Pylons["AmountNet"].GetMattab(DataModes.Tiers);
                    mx2.MattabFormula = (mx2["PriceNet"] * mx2["Quantity"]);
                    sales.Pylons["AmountNet"].ComputeOrdinal = 1;

                    Mattab mx3 = sales.Pylons["PriceGross"].GetMattab(DataModes.Tiers);
                    mx3.MattabFormula = (mx3["PriceNet"] * mx3["Tax"]);
                    sales.Pylons["PriceGross"].ComputeOrdinal = 0;

                    Mattab mx4 = sales.Pylons["AmountGross"].GetMattab(DataModes.Tiers);
                    mx4.MattabFormula = mx3 * /* mx4["PriceGross"] */ mx4["Quantity"];
                    sales.Pylons["AmountGross"].ComputeOrdinal = 2;

                    Mattab mx6 = stocks.Pylons["AmountNet"].GetMattab(DataModes.Tiers);
                    mx6.MattabFormula = mx6["CostNet"] * mx6["Quantity"];
                    stocks.Pylons["AmountNet"].ComputeOrdinal = 0;

                    Mattab mx5 = stocks.Pylons["AmountGross"].GetMattab(DataModes.Tiers);
                    mx5.MattabFormula = mx6 * /* mx5["AmountNet"] */  mx5["Tax"];
                    stocks.Pylons["AmountGross"].ComputeOrdinal = 1;
                }
                else
                {
                    sales = DataSpace.Area["Domain"]["Inventory", "Sales"];
                    stocks = DataSpace.Area["Domain"]["Inventory", "Stocks"];
                    stockTraffic = DataSpace.Area["Domain"]["Inventory", "StockTraffic"];
                }

                firstRun = false;
             

                // Odtworzenie oryginalnych ścieżek zapisu 
                DriveConfig.SetBankDrivePath(originBankPath);
                DriveConfig.SetSpaceDrivePath(originSpacePath);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
