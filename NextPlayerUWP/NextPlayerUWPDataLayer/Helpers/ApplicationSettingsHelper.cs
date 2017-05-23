using Microsoft.Toolkit.Uwp;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Helpers
{
    public static class ApplicationSettingsHelper
    {
        /// <summary>
        /// Function to read a setting value and clear it after reading it
        /// </summary>
        public static object ReadResetSettingsValue(string key)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                return null;
            }
            else
            {
                var value = ApplicationData.Current.LocalSettings.Values[key];
                ApplicationData.Current.LocalSettings.Values.Remove(key);
                return value;
            }
        }

        public static object ReadSettingsValue(string key)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                return null;
            }
            else
            {
                var value = ApplicationData.Current.LocalSettings.Values[key];
                return value;
            }
        }

        public static T ReadSettingsValue<T>(string key)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                try
                {
                    object value = ApplicationData.Current.LocalSettings.Values[key];
                    return (T)value;
                }
                catch (Exception ex)
                {

                }
            }
            return default(T);
        }

        /// <summary>
        /// Save a key value pair in settings. Create if it doesn't exist
        /// </summary>
        public static void SaveSettingsValue(string key, object value)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.LocalSettings.Values.Add(key, value);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[key] = value;
            }
        }

        public static void SaveSongIndex(int index)
        {
            int i = -1;
            object value = ReadSettingsValue(SettingsKeys.SongIndex);
            if (value != null) i = Int32.Parse(value.ToString());
            SaveSettingsValue(SettingsKeys.PrevSongIndex, i);
            SaveSettingsValue(SettingsKeys.SongIndex, index);
        }

        public static int ReadSongIndex()
        {
            object value = ReadSettingsValue(SettingsKeys.SongIndex);
            if (value != null)
            {
                return Int32.Parse(value.ToString());
            }
            else return 0;
        }

        public static Dictionary<int,string> PredefinedSmartPlaylistsId()
        {
            Dictionary<int, string> ids = new Dictionary<int, string>();
            ids.Add((int)ReadSettingsValue(AppConstants.OstatnioDodane),AppConstants.OstatnioDodane);
            ids.Add((int)ReadSettingsValue(AppConstants.OstatnioOdtwarzane),AppConstants.OstatnioOdtwarzane);
            ids.Add((int)ReadSettingsValue(AppConstants.NajczesciejOdtwarzane),AppConstants.NajczesciejOdtwarzane);
            ids.Add((int)ReadSettingsValue(AppConstants.NajgorzejOceniane),AppConstants.NajgorzejOceniane);
            ids.Add((int)ReadSettingsValue(AppConstants.NajlepiejOceniane),AppConstants.NajlepiejOceniane);
            ids.Add((int)ReadSettingsValue(AppConstants.NajrzadziejOdtwarzane), AppConstants.NajrzadziejOdtwarzane);
            return ids;
        }

        public static int ReadTileIdValue()
        {
            object value = ReadSettingsValue(SettingsKeys.TileIdValue);
            if (value != null)
            {
                return Int32.Parse(value.ToString());
            }
            else
            {
                SaveTileIdValue(1);
                return 1;
            }
        }

        public static void SaveTileIdValue(int id)
        {
            SaveSettingsValue(SettingsKeys.TileIdValue,id);
        }

        private const string separator = "|!@#$|";

        public static void SaveTileData(TileData tileData)
        {
            string val = (ReadSettingsValue(SettingsKeys.TileId) ?? "").ToString() + tileData.Id + separator;
            SaveSettingsValue(SettingsKeys.TileId, val);
            val = (ReadSettingsValue(SettingsKeys.TileName) ?? "").ToString() + tileData.Name + separator;
            SaveSettingsValue(SettingsKeys.TileName, val);
            val = (ReadSettingsValue(SettingsKeys.TileType) ?? "").ToString() + tileData.Type + separator;
            SaveSettingsValue(SettingsKeys.TileType, val);
            val = (ReadSettingsValue(SettingsKeys.TileImage) ?? "").ToString() + tileData.HasImage + separator;
            SaveSettingsValue(SettingsKeys.TileImage, val);
        }

        public static List<TileData> ReadTileData()
        {
            List<TileData> list = new List<TileData>();
            object id = ApplicationSettingsHelper.ReadResetSettingsValue(SettingsKeys.TileId);
            object name = ApplicationSettingsHelper.ReadResetSettingsValue(SettingsKeys.TileName);
            object type = ApplicationSettingsHelper.ReadResetSettingsValue(SettingsKeys.TileType);
            object image = ApplicationSettingsHelper.ReadResetSettingsValue(SettingsKeys.TileImage);

            if (id != null)
            {
                try
                {

                }
                catch(Exception ex)
                {
                    Diagnostics.Logger2.Current.WriteMessage("ReadTileData " + Environment.NewLine + ex.Message, NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);
                }
                string[] ids = id.ToString().Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                string[] names = name.ToString().Split(new string[] { separator }, StringSplitOptions.None);
                string[] types = type.ToString().Split(new string[] { separator }, StringSplitOptions.None);
                string[] images = image.ToString().Split(new string[] { separator }, StringSplitOptions.None);

                for(int i = 0; i < ids.Length; i++)
                {
                    list.Add(new TileData()
                    {
                        Id = ids[i],
                        Name = names[i],
                        Type = types[i],
                        HasImage = images[i]
                    });
                }
            }
            return list;
        }

        private const string sdCardFoldersFileName = "sdCard.txt";

        public static async Task SaveSdCardFoldersToScan(List<SdCardFolder> folders)
        {
            await SaveToLocalFile<List<SdCardFolder>>(sdCardFoldersFileName, folders);
        }

        public static async Task<List<SdCardFolder>> GetSdCardFoldersToScan()
        {
            return await ReadLocalFile<List<SdCardFolder>>(sdCardFoldersFileName);
        }

        public static async Task SaveToLocalFile<T>(string fileName, T data)
        {
            LocalObjectStorageHelper helper = new LocalObjectStorageHelper();
            try
            {
                await helper.SaveFileAsync<T>(fileName, data);
            }
            catch (UnauthorizedAccessException ex)
            {
            }
        }

        public static async Task<T> ReadLocalFile<T>(string fileName)
        {
            LocalObjectStorageHelper helper = new LocalObjectStorageHelper();
            bool exists = await helper.FileExistsAsync(fileName);
            T data = default(T);
            if (!exists)
            {
                try
                {
                    await helper.SaveFileAsync<T>(fileName, data);
                }
                catch (UnauthorizedAccessException ex)
                {
                }
            }
            else
            {
                try
                {
                    data = await helper.ReadFileAsync<T>(fileName);
                }
                catch (UnauthorizedAccessException)
                {

                }
            }
            return data;
        }

        public static void SaveData(string key, object data)
        {
            string serialized = JsonSerializationService.Instance.Serialize(data);
            SaveSettingsValue(key, serialized);
        }

        public static T ReadData<T>(string key)
        {
            string value = ReadSettingsValue(key) as string;
            if (value == null) return default(T);
            T data = JsonSerializationService.Instance.Deserialize<T>(value);
            return data;
        }
    }
}
