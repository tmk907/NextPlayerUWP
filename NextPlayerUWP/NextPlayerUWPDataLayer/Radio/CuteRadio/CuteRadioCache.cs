using NextPlayerUWPDataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Radio.CuteRadio
{
    public class CachedRadioData
    {
        public DateTime UpdatedAt { get; set; }
        public List<string> Data { get; set; }
    }

    public class CuteRadioCache
    {
        private readonly TimeSpan maxCacheDuration;

        public CuteRadioCache()
        {
            maxCacheDuration = TimeSpan.FromDays(14);
        }

        public async Task<List<string>> GetCountries()
        {
            return await GetData(LocalFileNames.RadioCountriesList);
        }

        public async Task<List<string>> GetGenres()
        {
            return await GetData(LocalFileNames.RadioGenresList);
        }

        public async Task<List<string>> GetLanguages()
        {
            return await GetData(LocalFileNames.RadioLanguagesList);
        }

        public async Task CacheCountries(List<string> countries)
        {
            await SaveData(LocalFileNames.RadioCountriesList, countries);
        }

        public async Task CacheGenres(List<string> genres)
        {
            await SaveData(LocalFileNames.RadioGenresList, genres);
        }

        public async Task CacheLanguages(List<string> languages)
        {
            await SaveData(LocalFileNames.RadioLanguagesList, languages);
        }

        private async Task<List<string>> GetData(string fileName)
        {
            List<string> list = new List<string>();
            var data = await ApplicationSettingsHelper.ReadLocalFile<CachedRadioData>(fileName);
            if (data == null || data.Data == null || data.UpdatedAt == null)
            {
                await ClearCache(fileName);
            }
            else
            {
                if (data.UpdatedAt + maxCacheDuration > DateTime.Now)
                {
                    list = data.Data;
                }
                else
                {
                    await ClearCache(fileName);
                }
            }
            return list;
        }

        private async Task SaveData(string fileName, List<string> data, bool replace = true)
        {
            if (!replace)
            {
                var cached = await ApplicationSettingsHelper.ReadLocalFile<CachedRadioData>(fileName);
                if (cached.UpdatedAt + maxCacheDuration > DateTime.Now) return;
            }
            await ApplicationSettingsHelper.SaveToLocalFile<CachedRadioData>(fileName, new CachedRadioData()
            {
                Data = data,
                UpdatedAt = DateTime.Now,
            });
        }

        private async Task ClearCache(string fileName)
        {
            await ApplicationSettingsHelper.SaveToLocalFile<CachedRadioData>(fileName, new CachedRadioData()
            {
                Data = new List<string>(),
                UpdatedAt = DateTime.MinValue
            });
        }
    }
}
