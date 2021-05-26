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
using System.Diagnostics;

namespace System.Doors.Data.Tests
{
    public static class DorsDataTests
    {
        private static string rootDir = "QDFS_Tests";
        private static IDorsEvent writeEcho;
        private static bool firstRun = true;
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
                    }
                    else
                    {
                        // Tworzenie obszaru DataBank - główny, wspólny kontener danych
                        DataVault systemVlt = DataBank.Vault.Create("MySystem");
                        writeEcho.Execute("Utworzenie Vault " + systemVlt.DisplayName);
                        DataDeposit userDep = systemVlt.Create("User");
                        writeEcho.Execute("Utworzenie Vault " + systemVlt.DisplayName + "/" + userDep.DisplayName);

                        //
                        // definicja tabeli (Trellis) zawierającej dane użytkowników systemu
                        // stosowanie unikalnych (w skali bazy danych) nazw pól pozwala na skorzystanie z możliwości automatyzacji relacji pól
                        //

                        DataTrellis userTrellPrime = new DataTrellis("User");
                        userTrellPrime.Pylons.AddRange(new DataPylon[]
                            {
                                new DataPylon(typeof(int), userTrellPrime,   "UserId")         { isAutoincrement = true, isKey = true, isIdentity = true },
                                new DataPylon(typeof(string), "UserGroupId")    { PylonSize = 20, JoinOperand = AggregateOperand.Bind },
                                new DataPylon(typeof(string), "UserName")       { PylonSize = 50 },
                                new DataPylon(typeof(string), "UserPass")       { PylonSize = 30 },
                                new DataPylon(typeof(int),    "UserActive")     {  }
                            });
                        // klucze główne zdefiniowanej tabeli
                        userTrellPrime.PrimeKey = userTrellPrime.Pylons.GetPylons(new string[] { "UserId", "UserGroupId" });

                        DataBank.Vault["MySystem"]["User"].Trell = userTrellPrime;

                        DataDeposit userCfg = systemVlt.Create("UserConfig");
                        writeEcho.Execute("Utworzenie Vault " + systemVlt.DisplayName + "/" + userCfg.DisplayName);

                        DataTrellis usrConfigTrellPrime = new DataTrellis("UserConfig");
                        usrConfigTrellPrime.Pylons.AddRange(new DataPylon[]
                            {
                                new DataPylon(typeof(int),    "UserId")             {  },
                                new DataPylon(typeof(Guid),   "ConfigItemId")       {  },
                                new DataPylon(typeof(string), "ConfigItemName")     { PylonSize = 50 },
                                new DataPylon(typeof(string), "ConfigItemValStr")   { PylonSize = 50 },
                                new DataPylon(typeof(float),  "ConfigItemValNum")   {  }
                            });

                        usrConfigTrellPrime.PrimeKey = usrConfigTrellPrime.Pylons.GetPylons(new string[] { "UserId", "ConfigItemId" });

                        // umocowanie zdefiniowanej tabeli w strukturze bazy
                        userCfg.Trell = usrConfigTrellPrime;

                        writeEcho.Execute("Utworzenie tabeli " + userTrellPrime.GetMapPath() + "/" + userTrellPrime.DisplayName);

                        DataVault platformsVlt = DataBank.Vault.Create("Platform");
                        writeEcho.Execute("Utworzenie Vault " + platformsVlt.GetMapPath());

                        // metoda tworzy Vault wraz z Vaults-em o domyślnej nazwie
                        DataVault amzVault = platformsVlt.CreateWithVlts("Amazon");
                        writeEcho.Execute("Utworzenie Vault " + platformsVlt.GetMapPath() + "/" + amzVault.DisplayName);

                        DataVault ebuyVault = platformsVlt.CreateWithVlts("eBuy");
                        writeEcho.Execute("Utworzenie Vault " + platformsVlt.GetMapPath() + "/" + ebuyVault.DisplayName);

                        // tworzenie depozytów w Vault-ach Amazona
                        DataDeposit amzProdDep = amzVault.Create("Product");
                        writeEcho.Execute("Utworzenie depozytu " + amzVault.GetMapPath() + "/" + amzProdDep.DisplayName);
                        DataDeposit amzPartnersDep = amzVault.Create("Partner");
                        writeEcho.Execute("Utworzenie depozytu " + amzVault.GetMapPath() + "/" + amzPartnersDep.DisplayName);
                        DataDeposit amzOrderDep = amzVault.Create("Order");
                        writeEcho.Execute("Utworzenie depozytu " + amzVault.GetMapPath() + "/" + amzOrderDep.DisplayName);

                        // tworzenie depozytów w Vault-ach eBuy-a
                        DataDeposit ebuyProdDep = ebuyVault.Create("Product");
                        writeEcho.Execute("Utworzenie depozytu " + ebuyVault.GetMapPath() + "/" + ebuyProdDep.DisplayName);
                        DataDeposit eBuyPartnersDep = ebuyVault.Create("Partner");
                        writeEcho.Execute("Utworzenie depozytu " + ebuyVault.GetMapPath() + "/" + eBuyPartnersDep.DisplayName);
                        DataDeposit eBuyOrderDep = ebuyVault.Create("Order");
                        writeEcho.Execute("Utworzenie depozytu " + ebuyVault.GetMapPath() + "/" + eBuyOrderDep.DisplayName);

                        // utworzenie depozytu wraz z zawierającym go Vault-em - inny sposób tworzenia struktury
                        DataDeposit depAllegro = DataBank.Vault.Create("Platform").NewDeposit("AllegroDep");

                        DataBank.Vault.WriteDrive();
                        DataBank.Vault.Drive.Exists = true;
                    }
                }
                /////////////////////////////////////////////////////////// SPACE CREATE /////////////////////////////////////////////////////////////

                DataTrellis userTrell = null;
                DataTrellis userConfigTrell = null;
                if (firstRun)
                {
                    DataSpace.Area.OpenDrive();
                    if (DataSpace.Area.Drive.Exists)
                    {
                        DataSpace.Area.ReadDrive();
                        userTrell = DataSpace.Area["SysSpace"]["SystemSph", "User"];
                        userConfigTrell = DataSpace.Area["SysSpace"]["SystemSph", "UserConfig"];
                    }
                    else
                    {
                        // Tworzenie obszaru DataSpace - dane i widoki użytkowników
                        DataSpheres sysSpace = DataSpace.Area.Create("SysSpace");
                        writeEcho.Execute("Utworzenie obszaru sfer " + sysSpace.GetMapPath() + "/" + sysSpace.DisplayName);
                        DataSphere sphSys = sysSpace.NewSphere("SystemSph");
                        writeEcho.Execute("Utworzenie sfery " + sphSys.GetMapPath() + "/" + sphSys.DisplayName);

                        sphSys.Trells.Add(DataBank.Vault["MySystem"]["User"].Trell);

                        DataSphere sphDarek = sysSpace.NewSphere("DarekSph");
                        writeEcho.Execute("Utworzenie sfery " + sphDarek.GetMapPath() + "/" + sphDarek.DisplayName);
                        DataSphere sphMietek = sysSpace.NewSphere("MietekSph");
                        writeEcho.Execute("Utworzenie sfery " + sphMietek.GetMapPath() + "/" + sphMietek.DisplayName);

                        sphSys.Trells.Add(DataBank.Vault["MySystem"]["UserConfig"].Trell);

                        userTrell = DataSpace.Area["SysSpace"]["SystemSph", "User"];
                        userConfigTrell = DataSpace.Area["SysSpace"]["SystemSph", "UserConfig"];

                        DataSpace.Area.WriteDrive();
                        DataSpace.Area.Drive.Exists = true;
                    }
                }
                else
                {
                    userTrell = DataSpace.Area["SysSpace"]["SystemSph", "User"];
                    userConfigTrell = DataSpace.Area["SysSpace"]["SystemSph", "UserConfig"];
                }

                firstRun = false;


                //
                // uzupełnienie tabeli User o rekordy z danymi użytkownikami - sposób 1
                //               

                // struktury dla nowych rekordów
                DataTier tiUser1 = new DataTier(userTrell);
                DataTier tiUser2 = new DataTier(userTrell);
                DataTier tiUser3 = new DataTier(userTrell);

                // przypisanie wartości do poszczególncyh pól. Pola autonumerowane muszą posiadać wartośc (<> null)
                // tiUser1.PrimeArray = new object[] { 0, "AMZ", "Zdzichu", "pass", 1 };

                tiUser1.DataArray = new object[] { 0, "AMZ", "Zdzichu", "pass", 1 };
                userTrell.Tiers.Put(tiUser1);

                tiUser2.DataArray = new object[] { 0, "ALL", "Rudy Mundek", "pass2", 0 };
                userTrell.Tiers.Put(tiUser2);

                tiUser3.PrimeArray = new object[] { 0, "AMZ", "Smutny Alojz", "pass3", 1 };
                userTrell.Tiers.Put(tiUser3);

                // sprawdzenie stanu licznika id w PrimeArray
                int iNextIdx = userTrell.Pylons["UserId"].AutoIndex;

                // zapis wprowadzonych danych (rekordów) do tabeli (na dysk)
                //userTrell.Tiers.SaveChanges();
                //writeEcho.Execute("Zapis danych do tabeli User - sposób 1");
                MultiTaskGo SaveAsync = new MultiTaskGo(userTrell.Tiers, "SaveChanges", true, false);

                //
                // uzupełnienie tabeli User o rekordy z danymi użytkownikami - sposób 2
                //

                // struktura dla nowego rekordu
                DataTier tiNewUser = userTrell.Tiers.AddNew();

                // przypisanie wartości do pól wykorzystując ich nazwy
                //tiNewUser["UserId"] = 0;    // pole autoinkrementowane, ale musi być jakaś wartość
                tiNewUser["UserGroupId"] = "AMZ";
                tiNewUser["UserName"] = "Gruby Rycho";
                tiNewUser["UserPass"] = "RychuPass";
                tiNewUser["UserActive"] = 1;
                tiNewUser.SaveChanges();
                // writeEcho.Execute("Zapis danych do tabeli User - sposób 2");

                //
                // uzupełnienie tabeli User o rekordy z danymi użytkownikami - sposób 3
                //

                // przypisanie wartości do pól poprzez ich numer w rekordzie
                // wykorzystujemy strukturę z poprzedniego sposobu
                // tiNewUser[1] = 0;    // pole autoinkrementowane, pomijamy

                //List<DataTier> listUsers = new List<DataTier>();
                //for (int i = 0; i < 1; i++)
                //{
                //    DataTier tiNewUser2 = userTrell.Tiers.AddNew();
                //    //tiNewUser2[0] = 0;
                //    tiNewUser2[1] = "ALL";
                //    tiNewUser2[2] = "Zdzicho";
                //    tiNewUser2[3] = "haslo2 Zdzicha" + stopwatch.Elapsed.TotalSeconds.ToString();
                //    tiNewUser2[4] = 0;
                //    listUsers.Add(tiNewUser2);
                //}
                //for (int i = 0; i < listUsers.Count; i++)
                //{
                //    listUsers[i].SaveChanges();
                //}
                    DataTier tiNewUser2 = userTrell.Tiers.AddNew();
                    //tiNewUser2[0] = 0;
                    tiNewUser2[1] = "ALL";
                    tiNewUser2[2] = "Zdzicho";
                    tiNewUser2[3] = "haslo2 Zdzicha";
                    tiNewUser2[4] = 0;
                    tiNewUser2.SaveChanges();

                // writeEcho.Execute("Zapis danych do tabeli User - sposób 3");

                // demonstracja sprawdzenia ilości wierszy w tabeli oraz widoku
                int iUserTiersCount = userTrell.Tiers.Count;                // ilość wierszy w tabeli - sposób 1 
                int iUserCount = userTrell.Count;                           // ilość wierszy w tabeli - sposób 2
                int iUserViewCount = userTrell.CountView;                   // licznik pofiltrowanych elementów - preferowany sposób
                int iUserTiersViewCount = userTrell.TiersView.Count;        // licznik pofiltrowanych elementów - sposób 2
                userTrell.Query();                                          // odświeża widok (ponownie wykonuje zapytanie filtrujące)

                // Prezentacja danych z tabeli w formie odczytu macierzowego
                writeEcho.Execute("Odczyt danych o użytkownikach z macierzy");
                writeEcho.Execute("Id 1: " + userTrell.Tiers[0][0] + "  " + userTrell.Tiers[0][1] + "  " + userTrell.Tiers[0][2] + "  " + userTrell.Tiers[0][3] + "  " + userTrell.Tiers[0][4]);
                writeEcho.Execute("Id 2: " + userTrell.Tiers[1][0] + "  " + userTrell.Tiers[1][1] + "  " + userTrell.Tiers[1][2] + "  " + userTrell.Tiers[1][3] + "  " + userTrell.Tiers[1][4]);
                writeEcho.Execute("Id 3: " + userTrell.Tiers[2][0] + "  " + userTrell.Tiers[2][1] + "  " + userTrell.Tiers[2][2] + "  " + userTrell.Tiers[2][3] + "  " + userTrell.Tiers[2][4]);
                writeEcho.Execute("Id 4: " + userTrell.Tiers[3][0] + "  " + userTrell.Tiers[3][1] + "  " + userTrell.Tiers[3][2] + "  " + userTrell.Tiers[3][3] + "  " + userTrell.Tiers[3][4]);

                // Odczyt i wyświetlenie na konsoli BizDepot-a wszystkich wprowadzonych użytkowników - sposób 1
                /*List<DataTier> usr = userTrell.Tiers.Collect(4, 0) ;
                foreach (DataTier dt in usr)
                {
                    writeEcho.Execute("Id / Grupa / Nazwa / Hasło / Aktywny:  " + usr[""] + " " + usr[1]);
                }*/

                // tutaj dodajemy dane jak w linii 129/145 (i powyżej) i dalej

                // Odczyt i wyświetlenie na konsoli BizDepot-a wszystkich wprowadzonych użytkowników - sposób 2
                writeEcho.Execute("Odczyt danych o użytkownikach w pętli");
                for (int i = 0; i < iUserCount; i++)
                {
                    writeEcho.Execute("Id / Grupa / Nazwa / Hasło / Aktywny:  " + userTrell.Tiers[i][0] + " " + userTrell.Tiers[i][1] + " " + userTrell.Tiers[i][2] + " " + userTrell.Tiers[i][3] + " " + userTrell.Tiers[i][4]);
                }

                //userTrell.Tiers.Put(tier);
                //userTrell.Tiers.Add(tier);

                //userTrell.Mode = DataModes.Sims;
                //userTrell.Mode = System.Doors.DataModes.Tiers;

                //DataTiers tsview = userTrell.TiersView;
                //------------------------- ******** ------------------


                // wyszukuje i oddaje wiersze zawierające wartości w podanych kolumnach  
                DataTiers foundedTiers0 = userTrell.Tiers.Collect(new string[] { "UserGroupId", "UserName" },
                                                                new List<object[]>()
                                                                {
                                                                    new object[] { "AMZ", "Zdzichu"  },
                                                                    new object[] { "AMZ", "Gruby Rycho" },
                                                                    new object[] { "ALL", "Czesio" }
                                                                });
                DataTier[] foundedTiers1 = userTrell.Tiers.Collect(new string[] { "UserGroupId", "UserName" }, new object[] { "AMZ", "Zdzichu" });

                DataTier[] foundedTiers2 = userTrell.Tiers.Collect("UserName", "Zdzichu");

                DataTier foundTierByKeys = userTrell.Tiers.Collect(ParamsTool.New(1, "AMZ"));

                
                
                

                // dodanie tabeli z konfiguracją użytkownika do DataSpace

                writeEcho.Execute("Utworzenie tabeli " + userConfigTrell.GetMapPath() + "/" + userConfigTrell.DisplayName);

                // dodanie tabeli do DataSpace 
                // sphMain.Trells.Add();

                // dodawanie danych w DS add, put (zerknąć na listingi)
                // usuwanie danych w DS remove
                // aktualizacja danych w DS tiers[nr_wiersza][idx_kolumny lub nazwa kolumny] = (column_type)value, tiers[nr_wiersza].DataArray = object[] (zamiana całej kolekcji)
                //                          tiers[nr_wiersza].PrimeArray = object[]

                // relacje pomiędzy DataTrellis
                //userTrell.AddRelay

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
