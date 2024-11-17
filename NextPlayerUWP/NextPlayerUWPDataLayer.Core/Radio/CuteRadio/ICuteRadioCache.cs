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
    
    public interface ICuteRadioCache
    {
        Task CacheCountries(List<string> countries);
        Task CacheGenres(List<string> genres);
        Task CacheLanguages(List<string> languages);
        Task<List<string>> GetCountries();
        Task<List<string>> GetGenres();
        Task<List<string>> GetLanguages();
    }
}
