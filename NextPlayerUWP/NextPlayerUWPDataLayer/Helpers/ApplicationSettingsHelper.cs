using NextPlayerUWPDataLayer.Constants;
using System;
using System.Collections.Generic;
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

        //public static T ReadSettingsValue<T>(string key)
        //{
        //    if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
        //    {
        //        try
        //        {
        //            T value = (T)ApplicationData.Current.LocalSettings.Values[key];
        //            return value;
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //    return null;
        //}

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
            object value = ReadSettingsValue(AppConstants.SongIndex);
            if (value != null) i = Int32.Parse(value.ToString());
            SaveSettingsValue(AppConstants.PrevSongIndex, i);
            SaveSettingsValue(AppConstants.SongIndex, index);
        }

        public static int ReadSongIndex()
        {
            object value = ReadSettingsValue(AppConstants.SongIndex);
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
            object value = ReadSettingsValue(AppConstants.TileIdValue);
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
            SaveSettingsValue(AppConstants.TileIdValue,id);
        }

        private const string separator = "|!@#$|";

        public static void SaveTileData(TileData tileData)
        {
            string val = (ReadSettingsValue(AppConstants.TileId) ?? "").ToString() + tileData.Id + separator;
            SaveSettingsValue(AppConstants.TileId, val);
            val = (ReadSettingsValue(AppConstants.TileName) ?? "").ToString() + tileData.Name + separator;
            SaveSettingsValue(AppConstants.TileName, val);
            val = (ReadSettingsValue(AppConstants.TileType) ?? "").ToString() + tileData.Type + separator;
            SaveSettingsValue(AppConstants.TileType, val);
            val = (ReadSettingsValue(AppConstants.TileImage) ?? "").ToString() + tileData.HasImage + separator;
            SaveSettingsValue(AppConstants.TileImage, val);
        }

        public static List<TileData> ReadTileData()
        {
            List<TileData> list = new List<TileData>();
            object id = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.TileId);
            object name = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.TileName);
            object type = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.TileType);
            object image = ApplicationSettingsHelper.ReadResetSettingsValue(AppConstants.TileImage);

            if (id != null)
            {
                try
                {

                }
                catch(Exception ex)
                {
                    Diagnostics.Logger.Save("ReadTileData " + Environment.NewLine + ex.Message);
                    Diagnostics.Logger.SaveToFile();
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
    }
}
