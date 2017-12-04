using Microsoft.Toolkit.Uwp;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Helpers
{
    public class ApplicationSettingsService : IApplicationSettings
    {
        public T ReadData<T>(string key)
        {
            string value = ReadSettingsValue(key) as string;
            if (value == null) return default(T);
            T data = JsonSerializationService.Instance.Deserialize<T>(value);
            return data;
        }

        public void SaveData(string key, object data)
        {
            string serialized = JsonSerializationService.Instance.Serialize(data);
            SaveSettingsValue(key, serialized);
        }

        public object ReadSettingsValue(string key)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                var value = ApplicationData.Current.LocalSettings.Values[key];
                return value;
            }
            else
            {
                return null;
            }
        }

        public object ReadResetSettingsValue(string key)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                var value = ApplicationData.Current.LocalSettings.Values[key];
                ApplicationData.Current.LocalSettings.Values.Remove(key);
                return value;
            }
            else
            {
                return null;
            }
        }

        public T ReadSettingsValue<T>(string key)
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

        public int ReadSongIndex()
        {
            object value = ReadSettingsValue(SettingsKeys.SongIndex);
            if (value != null)
            {
                return Int32.Parse(value.ToString());
            }
            else return 0;
        }

        public void SaveSettingsValue(string key, object value)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.LocalSettings.Values[key] = value;
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values.Add(key, value);
            }
        }

        public void SaveSongIndex(int index)
        {
            SaveSettingsValue(SettingsKeys.SongIndex, index);
        }

        public async Task<T> ReadLocalFile<T>(string fileName)
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

        public async Task SaveToLocalFile<T>(string fileName, T data)
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
    }
}
