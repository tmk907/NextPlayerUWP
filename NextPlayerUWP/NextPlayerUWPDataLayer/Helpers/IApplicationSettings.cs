using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Helpers
{
    public interface IApplicationSettings
    {
        void SaveSettingsValue(string key, object value);
        object ReadResetSettingsValue(string key);
        object ReadSettingsValue(string key);
        T ReadSettingsValue<T>(string key);

        void SaveSongIndex(int index);
        int ReadSongIndex();

        Task SaveToLocalFile<T>(string fileName, T data);
        Task<T> ReadLocalFile<T>(string fileName);

        void SaveData(string key, object data);
        T ReadData<T>(string key);
    }
}
