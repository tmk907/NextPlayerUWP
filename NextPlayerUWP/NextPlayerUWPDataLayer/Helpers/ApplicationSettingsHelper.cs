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
            object value = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.SongIndex);
            if (value != null) i = Int32.Parse(value.ToString());
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.PrevSongIndex, i);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.SongIndex, index);
        }

        public static int ReadSongIndex()
        {
            object value = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.SongIndex);
            if (value != null)
            {
                return Int32.Parse(value.ToString());
            }
            else return -1;
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
            object value = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TileIdValue);
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
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TileIdValue,id);
        }
    }
}
