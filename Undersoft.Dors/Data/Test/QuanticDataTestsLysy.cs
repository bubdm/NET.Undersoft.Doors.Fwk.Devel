using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Dors;
using System.Text;
using System.Linq;
using System.Dors.Mathtab;

namespace System.Dors.Data.Tests
{
    public static class DorsDataTestsLysy
    {
        public static string rootDirectory = "QDFS_Lysy";
        public static string orgBankPath = DriveConfig.GetBankDrivePath();
        public static string orgSpacePath = DriveConfig.GetSpaceDrivePath();
        public static string orgBankName = DriveConfig.GetBankName();
        public static string orgSpaceName = DriveConfig.GetSpaceName();
        public static bool firstStart = true;

        public static void RunTests(IDorsEvent writeEcho, string rootDir = null)
        {
            DriveConfig.SetClusterId();
            DriveConfig.SetBankName("BankLysego");
            DriveConfig.SetBankDrivePath(rootDirectory); //ustawia własną ścieżkę/nazwę
            DriveConfig.SetSpaceName("SpaceLysego");
            DriveConfig.SetSpaceDrivePath(rootDirectory); //ustawia własną ścieżkę/nazwę
           //Nie ustawia własnej nazwy Banku
             //Nie ustawia własnej nazwy Space
          

            try
            {
                if (firstStart)
                {
                   
                    if (DataBank.Vault.TryReadDrive()) //zmienić warunek jak ruszy ReadDrive()
                                                       //   * DH *   //
                                                        // Może posiadać różne iterpretacje 
                                                      //# OpenDrive jest częścią technologii MemoryMappedFile
                                                      //która to posiada 2 typy userów creator i accessor
                                                      //(ten kto pierwszy otworzył plik staje się Ownerem, każdy kolejny accesorem)
                                                      //Logika brzmiała tak otworz i odpowiedz w Exists czy istnieje
                                                      // Dodałem metody bool TryOpenDrive() i TryReadDrive() - 
                                                      // TRUE czyli dysk istnieje 
                        //Wysypuje się na czytaniu dysku !!!!!!!!!!!!!
                        //Powodem takiego zachwowania jest odwrotna interpretacja 
                        //Jeżeli OpenDrive() i Exists true wtedy ReadDrive jeżeli false wtedy Methody z innych miejsc
                        //DataBank.Vault.ReadDrive();

                    {
                        /****************************************************************CREATE DATABANK***********************************************/

                        //tworzenie vaulta
                        DataVault sysVault = DataBank.Vault.Create("SysLysy");
                        writeEcho.Execute(new object[] { "Utworzenie vaulta: " + sysVault.DisplayName });

                        //tworzenie depozytów
                        DataDeposit users = sysVault.Create("Users");
                        DataDeposit products = sysVault.Create("Products");
                        DataDeposit markets = sysVault.Create("Markets");
                        DataDeposit orders = sysVault.Create("Orders");
                        writeEcho.Execute(new string[] { "Utworzenie depozytów: " + users.DisplayName
                                                                          + products.DisplayName
                                                                          + markets.DisplayName
                                                                          + orders.DisplayName }); //dodanie loga z etykietami

                        /****************************************************************CREATE TRELLIS***********************************************/

                        DataTrellis userTrellis = new DataTrellis("User"); //inicjalizacja trellki - tabelki
                        userTrellis.Pylons.AddRange(new DataPylon[] //dodanie pylonów - kolumn
                            {
                                new DataPylon(typeof(int),      "UserId")   { isAutoincrement = true, isKey = true, isIdentity = true },
                                new DataPylon(typeof(string),   "UserName") { PylonSize = 100 },
                                new DataPylon(typeof(string),   "Pass")     { PylonSize = 20 },
                                new DataPylon(typeof(int),      "IsActive")
                            });

                        userTrellis.PrimeKey = userTrellis.Pylons.GetPylons(new string[] { "UserId" }); //ustawienie klucza głównego
                        sysVault["Users"].Trell = userTrellis; //przypisanie trelki do struktury

                        DataTrellis productTrellis = new DataTrellis("Products");
                        productTrellis.Pylons.AddRange(new DataPylon[]
                            {
                        new DataPylon(typeof(int), userTrellis,   "ProductId")         { isAutoincrement = true, isKey = true, isIdentity = true },
                        new DataPylon(typeof(string), "Code")       { PylonSize = 10 },
                        new DataPylon(typeof(int),    "IsActive")
                            });

                        productTrellis.PrimeKey = productTrellis.Pylons.GetPylons(new string[] { "ProductId" });
                        sysVault["Products"].Trell = productTrellis;

                        DataTrellis marketTrellis = new DataTrellis("Markets");
                        marketTrellis.Pylons.AddRange(new DataPylon[]
                            {
                        new DataPylon(typeof(int), userTrellis,   "MarketId")         { isAutoincrement = true, isKey = true, isIdentity = true },
                        new DataPylon(typeof(string), "MarketName")       { PylonSize = 50 },
                        new DataPylon(typeof(int),    "IsActive")     {  }
                            });

                        marketTrellis.PrimeKey = marketTrellis.Pylons.GetPylons(new string[] { "MarketId" });
                        sysVault["Markets"].Trell = marketTrellis;

                        DataTrellis ordersTrellis = new DataTrellis("Orders");
                        ordersTrellis.Pylons.AddRange(new DataPylon[]
                            {
                        new DataPylon(typeof(int), userTrellis,"OrderId")         { isAutoincrement = true, isKey = true, isIdentity = true },
                        new DataPylon(typeof(decimal), "Price"),
                        new DataPylon(typeof(int),    "IsActive")
                            });

                        ordersTrellis.PrimeKey = ordersTrellis.Pylons.GetPylons(new string[] { "OrderId" });
                        sysVault["Orders"].Trell = ordersTrellis;
                    }
                }
                /****************************************************************CREATE SPACE***********************************************/
                //trelki z banku
                DataTrellis userTrell = DataBank.Vault["SysLysy"]["Users"].Trell;
                DataTrellis productTrell = DataBank.Vault["SysLysy"]["Products"].Trell;
                DataTrellis marketTrell = DataBank.Vault["SysLysy"]["Markets"].Trell;
                DataTrellis orderTrell = DataBank.Vault["SysLysy"]["Orders"].Trell;

                if (firstStart)
                {
                    if (!DataSpace.Area.TryReadDrive())
                    {
                        //Tworzenie obszaru roboczego - sphery użytkowników
                        DataSpheres systemSpace = DataSpace.Area.Create("SystemSpace");
                        DataSphere systemSphere = systemSpace.NewSphere("Business");
                        writeEcho.Execute("Utworzenie space i sphere " + systemSpace.GetMapPath() + "/" + systemSphere.DisplayName);

                        //dodanie trellek z banku
                        systemSphere.Trells.Add(userTrell);
                        systemSphere.Trells.Add(productTrell);
                        systemSphere.Trells.Add(marketTrell);

                        //dodanie wybranych trelek za pomocą AddRange()

                        DataSphere allUsersSphere = systemSpace.NewSphere("AllUsersSphere");
                        allUsersSphere.Trells.AddRange(new List<DataTrellis>() { userTrell, orderTrell });

                        DataSpace.Area.WriteDrive();
                        DataSpace.Area.Drive.Exists = true;
                    }
                }

                firstStart = false;

                /****************************************************************ADD DATA***********************************************/
                DataTier userTier1 = new DataTier(userTrell);
                DataTier userTier2 = new DataTier(userTrell);
                DataTier userTier3 = new DataTier(userTrell);
                DataTier userTier4 = new DataTier(userTrell);
                DataTier productTier1 = new DataTier(productTrell);
                DataTier productTier2 = new DataTier(productTrell);
                DataTier marketTier1 = new DataTier(marketTrell);
                DataTier marketTier2 = new DataTier(marketTrell);
                DataTier orderTier1 = new DataTier(orderTrell);
                DataTier orderTier2 = new DataTier(orderTrell);

                //Autoincrement musi być wartością <> null. W przypadku wpisania "na siłę" inkrementacji nie zapisze danych.
                userTier1.DataArray = new object[] { 0, "Mateusz", "haszPass", 1 };
                userTrell.Tiers.Put(userTier1);
                userTier2.DataArray = new object[] { 0, "All Bundy", "&###^%^^&2", 1 };
                userTrell.Tiers.Put(userTier2);
                userTier3.DataArray = new object[] { 0, "Mateusz2", "haszPass", 1 };
                userTrell.Tiers.Put(userTier3);
                userTier4.DataArray = new object[] { 0, "All Bundy2", "&###^%^^&2", 1 };
                userTrell.Tiers.Put(userTier4);
                productTier1.DataArray = new object[] { 0, "0090909", 1 };
                productTrell.Tiers.Put(productTier1);
                productTier2.DataArray = new object[] { 0, "5649644", 1 };
                productTrell.Tiers.Put(productTier2);

                List<DataTier> listProductsTier = new List<DataTier>();
                listProductsTier.Add(productTier1);
                listProductsTier.Add(productTier2);



                ///****************************************************************ADD 49998 tiers***********************************************/
                ///Próba dodania > 49999 Tiersów kończy się błędem
                ///Ciekawa próba. Nie wpadłbym na to że ktoś może zapisywac co jeden wiersz. 
                // W Pliku Word nie zapisujem arkusza po kazdym wpisanym znaku.
                // Entity nasza konkurencja pozwala na zapisanie wywoływane z poziomu kolekcji czyli listy nie daje dla krotki 
                // W tym przypadku jest podobnie. wywłoanie po zakończeniu dodawania
                // dla pojedynczych nie jest sprawdzane i NIGDY nie bedzie ilosc pozostałego miejsca w sectorze indeksowym
                // w celu rozszerzenia pliku indeksow o kolejne naprzyklad teraz mamy tworzenie co 100k lub 50k wierszy  i tylko tam jest obługa :)  
                //  userTrell.Tiers.SaveChanges(true, false);
                DataTrellis userTrellTemp = new DataTrellis("Temp", userTrell);
                userTrellTemp.Imitate(userTrell);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                for (int i = 0; i < 100002; i++)
                {
                    DataTier userTier5 = new DataTier(userTrell);
                    userTier5[1] = "Makaron";
                    userTier5[2] = "Lubella z solą" + i;
                    userTier5[3] = 0;
                    DataTier tr = userTrell.Tiers.Put(userTier5);
                    if((i % 2) > 0)
                        userTrellTemp.Tiers.AddRef(tr);
                }
                userTrellTemp.Tiers.AddRefRange(userTrell.Tiers.AsEnumerable().Where(t => t["UserName"] == "Makaron").ToArray());
              
                //# DH #// - prawidłowo - do zastanowienia czy mozna zmienic na prywatna dla wiersza (DataTier) lub inaczej zabezpieczyc
                userTrell.Tiers.SaveChanges();
                //stopwatch.Stop();


               

                ///****************************************************************REMOVE! TRELL***********************************************/
                var userTierDelete = userTrell.Tiers.Query(new List<FilterTerm>() { new FilterTerm("UserId", OperandType.EqualOrLess, 5000) }, null);
                if (userTierDelete != null)
                    userTrell.Tiers.DeleteRange(userTierDelete.AsEnumerable().ToList());

                //userTrell.Tiers.Query();

                //...Delete(userTierDelete) //nie działa - Delete działa na zasadzie oflagowania wiersza binanrnie na 16 bajcie z 24 indeksu Noid
                //...Delete usuwa z kolekcji po pewnym czasie poniewaz usuwa jak nie bedzie tabela uzwyana ponadto delete propaguje na kazdą emyulację.
                // Tabela Prime - inaczej Root - Elder itp. nigdy nie usuwaja fizycznie danych a oglagowane usuwaja się z kolekcji przy nastepnej akcji                              


                ///****************************************************************COPY DATA***********************************************////
                //wrzucanie kodów z pliku wygenerowanego na podstawie tabelki UnisolutionQT. Plik wyeksportowany zew. apką - problem z dodaniem System.Data.SqlClient.dll do projekktu UnisolutionQT
                //using (StreamReader stream = new StreamReader("D://baza.txt"))
                //{

                //    string[] tableCodes = stream.ReadToEnd().Split(new char[] { ';' });

                //    List<DataTier> listProductsCodes = new List<DataTier>();
                //    for (int i = 0; i < tableCodes.Length; i++)
                //    {
                //        DataTier newProductData = new DataTier(productTrell);
                //        newProductData.DataArray = new object[] { 0, tableCodes[i], 1 };
                //        listProductsCodes.Add(newProductData);
                //    }
                //    productTrell.Tiers.PutRange(listProductsCodes);
                //}

                ///****************************************************************REMOVE ALL TIERS***********************************************/
                ///Usuwa co drugi połowe tiersów - do sprawdzenia               
                // * DH * Probem zwiazany z zakładanie nowego pliku z indeksami
                // * DH ** Ciągłe sprawdzanie przy kazdym wierszu tez mija sie z celem,
                var tiersCount = productTrell.Tiers.Count;
                for (int i = 0; i < 40000; i++)
                {
                    if (productTrell.Tiers[i] != null)
                    {
                        productTrell.Tiers.Remove(productTrell.Tiers[i]);
                    }

                }
            
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
