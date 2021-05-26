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
using System.Doors.Data;
using System.Doors;
using System.Doors.Drive;
using System.Doors.MultiTask;
using System.Doors;
using System.Doors.Intelops;
using System.Diagnostics;
using System.Doors.Mathtab;
using System.Windows;

namespace System.Doors.Data.Tests
{
    public struct myObject
    {
        double x1;
        double x2;
        public myObject(double _x1, double _x2)
        {
            x1 = _x1;
            x2 = _x2;
        }
    }

    public static class DorsDataTestsKedar
    {
        private static string rootDir = "QDFS_Tests";
        private static IDorsEvent writeEcho;
        private static TestRR trend = new TestRR();


        // metoda inicjująca test
        public static void RunTests(IDorsEvent WriteEcho, string RootDir = null)
        {

            //Lagrange lg = new Lagrange();

            //TestRR testRR = new TestRR();

            HyperMapTests hyperMapTests = new HyperMapTests();

      
            hyperMapTests.ScopeTreeRHGlobalTest(WriteEcho);



            //trendEstimator.Set(x1.ToList(), y.ToList());
            //trendEstimator.Set(x, x2.ToList());
            //yValue1 = trendEstimator.LagrangeInterpolation(xValue);
            //yValue2 = trendEstimator.LinearRegressionEstimation(xValue);
            //yValue3 = trendEstimator.KalmanEstimator(xValue);



            //-----------------------------------//

            // odczyt z konfiguracji nazw dla DataBank, DataSpace, ID klastra oraz 
            // ustalenie ścieżki do katalogów zawierających pliki danych
            //DriveConfig.SetBankName();

            //// pobranie oryginalnej ścieżki banku w celu odtworzenia 
            //string originBankPath = DriveConfig.GetBankDrivePath();

            //DriveConfig.SetBankDrivePath("QDFS");
            //DriveConfig.SetClusterId();
            //DriveConfig.SetSpaceName();

            //// pobranie oryginalnej ścieżki przestrzeni w celu odtworzenia 
            //string originSpacePath = DriveConfig.GetSpaceDrivePath();

            //DriveConfig.SetSpaceDrivePath("QDFS");

            //DataTrellis userTrell = null;
            ////if (DataSpace.Area.Drive.Exists)
            //DataSpace.Area.ReadDrive();


            //    //userTrell = DataSpace.Area["Data"].Spheres["Universum"].SpheresIn.Spheres["Globals"].SpheresIn.Spheres["Items"].Trellises["GlobalItems"];
            //userTrell = DataSpace.Area["Data"]["Universum"]["Globals"]["Items", "GlobalItems"];



            //Mattab mx = pyls["MarketItemLowestOfferPrice^"].GetMattab(DataModes.Tiers);
            //mx.MattabFormula = mx["MarketItemLowestOfferPrice^$"] * mx["MarketItemCurrencyRate"];

            //-----------------------------------//











            //// odczyt z konfiguracji nazw dla DataBank, DataSpace, ID klastra oraz 
            //// ustalenie ścieżki do katalogów zawierających pliki danych
            //DriveConfig.SetBankName();

            //// pobranie oryginalnej ścieżki banku w celu odtworzenia 
            //string originBankPath = DriveConfig.GetBankDrivePath();

            //DriveConfig.SetBankDrivePath("QDFS");
            //DriveConfig.SetClusterId();

            //DriveConfig.SetSpaceName();

            //// pobranie oryginalnej ścieżki przestrzeni w celu odtworzenia 
            //string originSpacePath = DriveConfig.GetSpaceDrivePath();

            //DriveConfig.SetSpaceDrivePath("QDFS");

            //DataTrellis userTrell = null;
            //if (DataSpace.Area.Drive.Exists)
            //{
            //    DataSpace.Area.ReadDrive();
            //    //userTrell = DataSpace.Area["Data"].Spheres["Universum"].SpheresIn.Spheres["Globals"].SpheresIn.Spheres["Items"].Trellises["GlobalItems"];
            //    userTrell = DataSpace.Area["Data"]["Universum"]["Globals"]["Items", "GlobalItems"];
            //}

            //int originSize = userTrell.Count;
            //for (int i = 0; i < originSize; i++)
            //{
            //    DataTier tiNewUser = new DataTier(userTrell);

            //    tiNewUser[1] = "cokolwiek"+i.ToString();
            //    tiNewUser[2] = "Gruby Rycho";
            //    tiNewUser[3] = "RychuPass";

            //    userTrell.Tiers.Put(tiNewUser);
            //}
            //userTrell.Query(); //filtrowanie / przygotowanie widoku

            //userTrell.Tiers.SaveChanges(true, true);

            //userTrell.Query(new List<FilterTerm>() { new FilterTerm(userTrell.Pylons[1], OperandType.Like, "cokolwiek") }, null);
            //originSize = userTrell.TiersView.Count;
            //userTrell.Tiers.DeleteRange(userTrell.TiersView.AsEnumerable().ToList());  // wywala blad

            //originSize = userTrell.Count;
            //MultiTaskGo SaveAsync = new MultiTaskGo(userTrell.Tiers, "SaveChanges", true, false);

            //for (int i = 0; i < userTrell.Count; i++)
            //{
            //    Debug.WriteLine("Item:  " + userTrell.Tiers[i][0] + " " + userTrell.Tiers[i][1] + " " + userTrell.Tiers[i][2] + " " + userTrell.Tiers[i][3] + " " + userTrell.Tiers[i][4]);
            //}
            //Quid quid;           
            //quid = Quid.Empty;
            //string s = "TUTyU";

            //Quid quid1 = new Quid(s);

            //Noid noid;
            //noid = new Noid(0x1122334455667788L, 0x1234, 0x1234, 0x1234, 0x0034, 0x0233445566778899L);

            //char[] testchar = noid.GetPCHexChar();

            //noid = new Noid(0, 0, 0, 0, 0, 0);
            //noid.SetPCHexByChar(testchar);


            //Noid noidfoo = new Noid(0x1122334455667788L, 0x1234, 0x1234, 0x1234, 0x1234, 0x0233445566778899L);
            //Noid noidbar = new Noid(noidfoo.ToString());
            //bool noidtest = noidfoo.EqualsContent(noidbar);

            //byte[] b = new byte[8];
            //noid = new Noid(b, 0x1234, 0x1234, 0x1234, 0x1234, 0x0034567821436579L);


            //Similarness similarness = new Similarness();
            //similarness.SimilarTo(similarness.FeatureItemList[1]);
            //similarness.SimilarInGroupsTo(similarness.FeatureItemList[1]);
            //similarness.SimilarInOtherGroupsTo(similarness.FeatureItemList[1]);



            //Quid foo = new Quid(170, 13456, 2222223);
            //Quid bar = new Quid(foo.ToString());
            //bool test = foo.LongValue == bar.LongValue;

            //quid.LongValue = (0x30197e23L <<32) | (0xbc75L) << 16 | (0x039dL);

            //quid.LongValue = (0x00007L << 32) | (0xbc75L) << 16 | (0x039dL);
            //quid.GetPHexByte();

            //byte[] pchbyte = quid.GetPCHexByte();
            //char[] pchchar = quid.GetPCHexChar();

            //quid.LongValue = 0;
            //quid.SetPCHexByChar(pchchar);

            //quid.LongValue = 0;
            //quid.SetPCHexByByte(pchbyte);

            //byte[] phbytetest = quid.GetPHexByte();
            //byte[] phbyteprime = quid.GetPHexBytePrimeId();
            //byte[] phbytedata = quid.GetPHexByteDataId();
            //byte[] phbytecluster = quid.GetPHexByteClusterId();

            //char[] phchartetest = quid.GetPHexChar();
            //char[] phchharprime = quid.GetPHexCharPrimeId();
            //char[] phchardata = quid.GetPHexCharDataId();
            //char[] phcharcluster = quid.GetPHexCharClusterId();

            //int lengthc = phbytetest.Length;

            //quid.LongValue = 0;
            ////quid.SetPHexByByte(phbytetest);
            //quid.SetPHexByChar(phchartetest);


            //writeEcho = WriteEcho;
            //writeEcho.Execute("Utworzenie Vault RRRR ");


            //  DorsMap<object> qm = new DorsMap<object>(-1 * 256 * 256 * 256, 256 * 256 * 256);





        }

        public static void RunStatisticsTests(IDorsEvent WriteEcho, EstimatorMethod method)
        {
            trend.RunTest(WriteEcho, method);
        }

    }
}
