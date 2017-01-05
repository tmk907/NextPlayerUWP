using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;

namespace NextPlayerUWPDataLayer.Diagnostics
{
    public sealed class Logger2
    {
        private const string addressBeta = "http://ttt907.nazwa.pl/nextplayerbetalogs/logs.php";
        private const string addressNormal = "http://ttt907.nazwa.pl/next-player-logs/logs.php";
        private string serverAddress;

        private static readonly Logger2 current = new Logger2();

        static Logger2() { }

        public static Logger2 Current
        {
            get
            {
                return current;
            }
        }

        private Logger2()
        {
            minLevel = Level.Warning;
            cache = new StringBuilder();
#if DEBUG
            serverAddress = addressBeta;
#else
            serverAddress = addressNormal;
#endif
        }

        private StringBuilder cache;

        public enum Level
        {
            Debug = 0,
            Information = 1,
            Warning = 2,
            Error = 3,
            WarningError = 6,
            DontLog = 10,
        }

        private Level minLevel;

        public void SetLevel(Level level)
        {
            minLevel = level;
        }

        public void LogAppUnhadledException(UnhandledExceptionEventArgs e)
        {
            WriteMessage(string.Format("UnhandledException - Exit - {0}", e.Exception.ToString()));
            Task t = WriteToFile();
            t.Wait(3000);
            // Give the application 3 seconds to write to the log file. Should be enough time. 
            SaveAppdata();
            e.Handled = false;
            //}
        }

        private void SaveAppdata()
        {
            //StorageFolder folder = ApplicationData.Current.LocalFolder;
            //Task<StorageFile> tFile = folder.CreateFileAsync("AppData.txt").AsTask<StorageFile>();
            //tFile.Wait();
            //StorageFile file = tFile.Result;
            //Task t = FileIO.WriteTextAsync(file, "This Is Application data").AsTask();
            //t.Wait();
        }

        private StorageFile _errorFile;
        public StorageFile ErrorFile
        {
            get { return _errorFile; }
            set { _errorFile = value; }
        }

        public async void CreateErrorFile()
        {
            try
            {
                // Open Error File  
                StorageFolder local = ApplicationData.Current.LocalFolder;
                ErrorFile = await local.CreateFileAsync("ErrorFile.txt", CreationCollisionOption.OpenIfExists);
            }
            catch (Exception)
            {
                // If cannot open our error file, then that is a shame. This should always succeed 
                // you could try and log to an internet serivce(i.e. Azure Mobile Service) here so you have a record of this failure.
                //TelemetryAdapter.TrackEvent("CreateLogFailure");
            }
        }

        public void WriteMessage(string strMessage, Level level = Level.Error)
        {
            if ((int)level >= (int)minLevel)
            {
                try
                {
                    cache.Append(string.Format("{0} - {1} - {2}\r\n", DateTime.Now.ToLocalTime().ToString(), level.ToString(), strMessage));
                }
                catch (Exception)
                {
                }
            }
        }

        public async Task WriteToFile()
        {
            if (ErrorFile != null && cache.Length > 0)
            {
                try
                {
                    await FileIO.AppendTextAsync(ErrorFile, cache.ToString());
                }
                catch (Exception)
                {
                    // If another option is available to the app to log error(i.e.Azure Mobile Service, etc...) then try that here
                    //TelemetryAdapter.TrackEvent("SaveToLogFailure");
                }
            }
        }

        public async Task SendLogs()
        {
            if (ErrorFile != null)
            {
                string text = "";
                try
                {
                    text = await FileIO.ReadTextAsync(ErrorFile);
                }
                catch (Exception) { }
                if (String.IsNullOrEmpty(text)) return;
                try
                {
                    Uri uri = new Uri(serverAddress);
                    var data = new List<KeyValuePair<string, string>>();
                    data.Add(new KeyValuePair<string, string>("log", text));

                    Package package = Package.Current;
                    PackageVersion version = package.Id.Version;
                    string ver = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
                    string arch = package.Id.Architecture.ToString();

                    data.Add(new KeyValuePair<string, string>("architecture", arch));
                    data.Add(new KeyValuePair<string, string>("version", ver));

                    using (var httpclient = new HttpClient())
                    {
                        using (var content = new FormUrlEncodedContent(data))
                        {
                            using (var responseMessages = await httpclient.PostAsync(uri, content))
                            {
                                string x = await responseMessages.Content.ReadAsStringAsync();
                                if (x == "OK")
                                {
                                    await FileIO.WriteTextAsync(ErrorFile, "");
                                }
                            }
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        public static void DebugWrite(string caller, string data)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("{0} {1} {2}", DateTime.Now.TimeOfDay, caller, data);
#endif
        }
    }
}
