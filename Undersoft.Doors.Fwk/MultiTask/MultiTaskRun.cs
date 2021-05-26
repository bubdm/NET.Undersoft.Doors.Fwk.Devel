using System.Data;
using System.Collections.Generic;
using System.Doors;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Net;
using System.IO;

namespace System.Doors.MultiTask
{
    public static class MultiTaskRun
    {

        public static void RunTest()
        {
            //MultiTaskGo antGo = new MultiTaskGo("System.Doors.MultiTask.FirstCurrency", "GetFirstCurrency", new Dictionary<string, object> { { "currname", "EUR" }, { "days",      14 } } );

            //Debug.WriteLine("Jade dalej");

            MultiTask farm = new MultiTask();

            Mission download = farm.Expanse(new List<MultiTaskMethod>()
            {
                new MultiTaskMethod("System.Doors.MultiTask.FirstCurrency", "GetFirstCurrency"),
                new MultiTaskMethod("System.Doors.MultiTask.SecondCurrency", "GetSecondCurrency"),
             
            },
                "GetCurrencies"
            );
            download.Visor.CreateMultiTask(2);
          
            Mission compute = farm.Expanse(new List<MultiTaskMethod>()
            {
                new MultiTaskMethod("Compute", "System.Doors.MultiTask.ComputeCurrency"),
                new MultiTaskMethod("Present", "System.Doors.MultiTask.PresentResult")
            },
                "ComputeCurrencies", 1 // Thread Count
            );

            List<Objective> ComputeRequirement = new List<Objective>()
            {
                download["GetFirstCurrency"],
                download["GetSecondCurrency"]
            };

            download["GetFirstCurrency"].Worker.AddEvoker(compute["System.Doors.MultiTask.ComputeCurrency.Compute"], ComputeRequirement);

            download["GetSecondCurrency"].Worker.AddEvoker(compute["System.Doors.MultiTask.ComputeCurrency.Compute"], ComputeRequirement);

            compute["Compute"].Worker.AddEvoker(compute["Present"]);

            download["GetFirstCurrency"].GoWork("EUR", 14);      
            download["GetSecondCurrency"].GoWork("USD", 7);


        }


    }

    public class FirstCurrency
    {
        public object GetFirstCurrency(string currency, int days)
        {
            Thread.Sleep(2000);

            KursNBP kurKraju = new KursNBP(days);

            try
            {
                decimal kurs = kurKraju.LoadRate(currency);
                Debug.WriteLine("Kurs 1 : " + currency + " z przed : " + days.ToString() + " wynosi : " + kurs.ToString("#.####"));

                return ParamsTool.New(currency, kurs);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return  null;
            }
        }
    }

    public class SecondCurrency
    {
        public object GetSecondCurrency(string currency, int days)
        {
            KursNBP kurKraju = new KursNBP(days);

            try
            {
                decimal kurs = kurKraju.LoadRate(currency);
                Debug.WriteLine("Kurs 2 : " + currency + " z przed : " + days.ToString() + " wynosi : " + kurs.ToString("#.####"));

                return ParamsTool.New(currency, kurs);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }
    }

    public class ComputeCurrency
    {
        public object Compute(string currency1, decimal kurs1, string currency2, decimal kurs2)
        {

            try
            {

                decimal _kurs1 = kurs1;
                decimal _kurs2 = kurs2;
                decimal wynik = _kurs1 * _kurs2;
                Debug.WriteLine("wynik: " + wynik.ToString());

                return ParamsTool.New(_kurs1, _kurs2, wynik);
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }
    }

    public class PresentResult
    {
        public object Present(decimal kurs1, decimal kurs2, decimal wynik)
        {

            try
            {

                string present = "Kurs PLN : " + kurs1.ToString() + " OBCA : " + kurs2.ToString() + " wynik : " + wynik.ToString();
                Debug.WriteLine(present);
                return present;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }
    }

    public class KursNBP
    {

        private const string file_dir = "http://www.nbp.pl/Kursy/xml/dir.txt";
        private const string xml_url = "http://www.nbp.pl/kursy/xml/";
        private int start_int = 1;
        public string file_name;
        public DateTime data_kursu;

        public KursNBP(int daysbefore)
        {
            GetFileName(daysbefore);
        }

        public Dictionary<string, decimal> GetCurrenciesRate(List<string> currency_names)
        {
            Dictionary<string, decimal> result = new Dictionary<string, decimal>();

            foreach (var item in currency_names)
            {
                result.Add(item, LoadRate(item));
            }
            return result;

        }

        public decimal LoadRate(string currency_name)
        {

            try
            {
                string file = xml_url + file_name + ".xml";
                DataSet ds = new DataSet(); ds.ReadXml(file);
                var tabledate = ds.Tables["tabela_kursow"].Rows.Cast<DataRow>().AsEnumerable();
                var przed_data_kursu = (from k in tabledate select new { Data = k["data_publikacji"].ToString() }).First();
                var tabela = ds.Tables["pozycja"].Rows.Cast<DataRow>().AsEnumerable();
                var kurs = (from k in tabela where k["kod_waluty"].ToString() == currency_name select new { Kurs = k["kurs_sredni"].ToString() }).First();
                data_kursu = Convert.ToDateTime(przed_data_kursu.Data);
                return Convert.ToDecimal(kurs.Kurs);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void GetFileName(int daysbefore)
        {

            try
            {
                int minusdays = daysbefore * -1;
                WebClient client = new WebClient();
                Stream strm = client.OpenRead(file_dir);
                StreamReader sr = new StreamReader(strm);
                string file_list = sr.ReadToEnd();
                string date_str = string.Empty;
                bool has_this_rate = false;
                DateTime date_of_rate = DateTime.Now.AddDays(minusdays);
                while (!has_this_rate)
                {
                    date_str = "a" + start_int.ToString().PadLeft(3, '0') + "z" + date_of_rate.ToString("yyMMdd");
                    if (file_list.Contains(date_str))
                    {
                        has_this_rate = true;
                    }

                    start_int++;

                    if (start_int > 365)
                    {
                        start_int = 1;
                        date_of_rate = date_of_rate.AddDays(-1);
                    }
                }
                file_name = date_str;
                data_kursu = date_of_rate;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
       
    }
}
