using NextPlayerUWPDataLayer.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Diagnostics
{
    public class Logger
    {
        private const string filename = "log2.txt";
        private const string filenameBG = "logBG2.txt";
        private const string lastfmlog = "lastfmlog.txt";

        private static string temp = "";
        private static string tempBG = "";

        private static bool BGLogON = true;

        public async static void SaveToFile()
        {
            string content = temp;
            temp = "";
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
                await FileIO.AppendTextAsync(file, content);
            }
            catch (Exception e)
            {
                Save("error log\n"+content+"\n");
            }
        }

        public static void Save(string data)
        {
            temp += DateTime.Now.ToString() + Environment.NewLine + data + Environment.NewLine;
        }

        
        public async static Task<string> Read()
        {
            string text;
            
            StorageFolder local = ApplicationData.Current.LocalFolder;
            try
            {
                Stream stream = await local.OpenStreamForReadAsync(filename);

                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
            }
            catch(FileNotFoundException e) 
            {
                text = e.Message;
            }
            
            return text;
        }

        public async static Task<string> ReadBG()
        {
            string text;

            StorageFolder local = ApplicationData.Current.LocalFolder;
            try
            {
                Stream stream = await local.OpenStreamForReadAsync(filenameBG);

                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
            }
            catch (FileNotFoundException e)
            {
                text = e.Message;
            }

            return text;
        }

        public async static Task ClearAll()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await ApplicationData.Current.LocalFolder.CreateFileAsync(filenameBG, CreationCollisionOption.ReplaceExisting);
        }

        public async static void SaveToFileBG()
        {
            if (BGLogON)
            {
                string content = tempBG;
                tempBG = "";
                System.Text.Encoding.UTF8.GetBytes(content.ToCharArray());
                try
                {
                    StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filenameBG, CreationCollisionOption.OpenIfExists);

                    await FileIO.AppendTextAsync(file, content);
                }
                catch (Exception e)
                {
                    SaveBG("error log\n" + content + "\n" + e.Message);
                }
            }
        }
        public static void SaveBG(string data)
        {
            if (BGLogON)
            {
                tempBG += DateTime.Now.ToString() + " " + data + "\n" + Environment.NewLine;
            }
        }


        public async static void SaveLastFm(string data)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(lastfmlog, CreationCollisionOption.OpenIfExists);
                await FileIO.AppendTextAsync(file, DateTime.Now.ToString() + Environment.NewLine + data + Environment.NewLine);
            }
            catch (Exception e)
            {
                
            }
        }

        public async static Task<string> ReadLastFm()
        {
            string text = "";
            StorageFolder local = ApplicationData.Current.LocalFolder;
            try
            {
                Stream stream = await local.OpenStreamForReadAsync(lastfmlog);
                using (StreamReader reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
            }
            catch (FileNotFoundException e)
            {
                
            }

            return text;
        }

        public async static void ClearLastFm()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync(lastfmlog, CreationCollisionOption.ReplaceExisting);
        }

        public static void SaveInSettings(string data)
        {
            ApplicationSettingsHelper.SaveSettingsValue("temperror", data);
        }
        public static void SaveFromSettingsToFile()
        {
            var a = ApplicationSettingsHelper.ReadResetSettingsValue("temperror");
            if (a != null)
            {
                try
                {
                    Save(a as string);
                    SaveToFile();
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
