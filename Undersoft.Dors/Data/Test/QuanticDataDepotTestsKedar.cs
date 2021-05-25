/*
    Dors Data Test Examples
    
    Demonstracyjne testy podstawowych funkcji biblioteki Dors
    Sopot 2018-06-19

    wersja 1.07

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dors.Data;
using System.Dors;
using System.Dors.Drive;
using System.Dors.MultiTask;
using System.Dors.Mathtab;
using System.Dors.Data.Depot;
using System.Dors;
using System.Dors.Intelops;
using System.Diagnostics;

namespace System.Dors.Data.Tests
{
    public static class DorsDataDepotTestsKedar
    {

        public static void VanEmdeBoasTreeTest()
        {          
        }

      
        // metoda inicjująca test
        public static void RunTests(IDorsEvent WriteEcho, string RootDir = null)
        {

            VanEmdeBoasTreeTest();

           // DataBank.Vault.OpenDrive();

           // if(DataBank.Vault.Drive.Exists)
           // {
           //     DataBank.Vault.ReadDrive();
           // }

           // DataSpace.Area.OpenDrive();

           // if (DataSpace.Area.Drive.Exists)
           // {
           //     DataSpace.Area.ReadDrive();
           // }
           

           // DataTrellis trl = DataSpace.Area["Data"]["Universum"]["Globals"]["Items"].Trells["GlobalItems"];


           // DataTrellis cubeItem = DataSpace.Area["Data"]["Universum"]["Cubes"]["Items"].Trells["Items"];

           // DataTrellis cubeItemMarkets = DataSpace.Area["Data"]["Universum"]["Cubes"]["Items"].Trells["ItemMarkets"];


           // DataTrellis trl2 = DataSpace.Area["Data"]["Universum"]["Globals"]["Items"].Trells["GlobalItemCodes"];
           // DataRelay rel = trl.AddRelay(trl2, new string[] { "ItemCode", "ItemCodeType" });



           // DataTrellis cube = new DataTrellis("Cube", false, true);
           // cube.Pylons.AddRange(trl.Pylons);
           // cube.Pylons.AddRange(trl2.Pylons);
           // cube.Pylons.Add(new DataPylon(typeof(double), "Compute"));
           // cube.Cube.CubeRelays.Add(rel);

           // cube.Cube.CreateCube();
           // cube.Cube.AssignSubTiers();

           // //a jak zapytac Cube'a?

           // //wtedy nowy Tier z Cube'a to polaczenie innych tiers (np. dla konkretnego id prdouktu i rank?, lub kolekcja/zbiór tierów, gdy dla danego id na rynkach ceny? - to już jest na rys. w Excelu

            
           // //przykład mathrix metody jakiś, żebym mógł sobie analizy różne robić ciekawe

           // foreach (DataTier t in trl.Tiers[0].GetChild("GlobalItemCodes"))
           //     t.GetParent("GlobalItems");
           //// trl.Tiers.Query( new List<FilterTerm>() { new FilterTerm("ItemQuid", "EqualOrLess", 08503409503) }, null);
           // trl.Tiers.SubTotal();

           // trl.WriteDrive();


        }

    }
}
