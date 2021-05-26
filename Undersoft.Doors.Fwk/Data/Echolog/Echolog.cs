using System;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Doors;

namespace System.Doors.Data.Echolog
{
    public static class Echolog
    {
        /// <summary>
        /// poziom logowania 1- błędy, 2,3,4 - oziomy różnych komunikatów
        /// </summary>
        private static int _logLevel = 0;
        /// <summary>
        /// kolejka komunikatów
        /// </summary>
        private static ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
        /// <summary>
        /// wątek zrzucający komunikaty do pliku
        /// </summary>
        private static Thread oThread = new Thread(new ThreadStart(HandleProc));
        /// <summary>
        /// czy wątek pracuje
        /// </summary>
        public static bool threadLive = true;
        /// <summary>
        /// czas ostatniego kasowania starych plików
        /// </summary>
        private static DateTime ClearLogTime = DateTime.Now.AddDays(-1).AddHours(-1).AddMinutes(-1);
        /// <summary>
        /// Interfejs zapisu logow jako wiersze w przestrzeni N^Data (zapisywancyh na dysk przez N^Drive) 
        /// dostęp do tabeli Prime = DataBank.Vault["Config"]["Echolog"].Trell
        /// lub do przestrzenie roboczej tabeli DataSpace.Area["Config"]["Service", "Echolog"] 
        /// lub  DataSpace.Area["Config"]["Service"].Trells["Echolog"]
        /// </summary>
        public static IDataLog DataLog { get { return DataCore.Bank.DataLog; } }

        /// <summary>
        /// logowanie komunikatów
        /// </summary>
        /// <param name="requiredLogLevel">poziom logowania</param>
        /// <param name="message">komunikat</param>
        public static void Log(int requiredLogLevel, String message, bool format = false)
        {
            try
            {
                if (_logLevel >= requiredLogLevel)
                {
                    string _message = requiredLogLevel.ToString() + "#Information#" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "#" + DateTime.Now.Millisecond.ToString()
                                                                        + "#" + message;
                    logQueue.Enqueue(_message);
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requiredLogLevel">poziom logowania</param>
        /// <param name="exp">wyjątek</param>
        public static void Log(int requiredLogLevel, Exception exp)
        {
            try
            {
                if (_logLevel >= requiredLogLevel)
                {
                        string message = requiredLogLevel.ToString()+"#Exception#"+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "#" + DateTime.Now.Millisecond.ToString() 
                                                                     + "#" + exp.Message
                                                                     + "\r\n" + exp.Source
                                                                     + "\r\n" + exp.StackTrace;
                        logQueue.Enqueue(message);
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// procedura wątku
        /// </summary>
        private static void HandleProc()
        {
            //LogClear();
            while (threadLive)
            {
                try
                {
                    Thread.Sleep(1000);
                    if (logQueue.Count > 0)
                    {
                        while (logQueue.Count > 0)
                        {
                            string message;
                            if (logQueue.TryDequeue(out message))
                            {
                                if (DataLog != null)
                                    DataLog.WriteLog(message);
                            }
                        }
                    }
                    //if (DateTime.Now.Day != ClearLogTime.Day)
                    //{
                    //    if (DateTime.Now.Hour != ClearLogTime.Hour)
                    //    {
                    //        if (DateTime.Now.Minute != ClearLogTime.Minute)
                    //        {
                    //            Echolog.LogClear();
                    //            ClearLogTime = DateTime.Now;
                    //        }
                    //    }
                    //}
                }
                catch
                {

                }
            }
        }
        /// <summary>
        /// start wątku z definiowanym poziomem logowania
        /// </summary>
        /// <param name="logLevel">poziom logowania</param>
        public static void Start(int logLevel)
        {
            _logLevel = logLevel;
            oThread.IsBackground = true;
            oThread.Start();
        }
        /// <summary>
        /// start wątku z parametrami domyślnymi
        /// </summary>
        public static void Start()
        {
            _logLevel = 2;
            oThread.IsBackground = true;
            oThread.Start();
        }
        /// <summary>
        /// stop wątku
        /// </summary>
        public static void Stop()
        {
            threadLive = false;
            oThread.Join();
        }
        /// <summary>
        /// usuwanie starszych plików niż 7 dni
        /// </summary>
        public static void LogClear()
        {
            try
            {
                DateTime time = DateTime.Now.AddDays(-7);
            }
            catch (Exception ex)
            {
                Log(1, ex);
            }

        }      
    }
}
