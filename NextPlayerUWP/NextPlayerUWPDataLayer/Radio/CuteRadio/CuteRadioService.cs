using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Radio.CuteRadio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace NextPlayerUWPDataLayer.Radio.CuteRadio
{
    public class CuteRadioService
    {
        private UriBuilder uriBuilder;
        private const int limit = 20;
        private CuteRadioCache cache;

        public CuteRadioService()
        {
            uriBuilder = new UriBuilder();
            cache = new CuteRadioCache();
        }

        private async Task<string> DownloadAsync(string uri)
        {
            using (var request = new HttpHelperRequest(new Uri(uri), HttpMethod.Get))
            {
                using (var response = await HttpHelper.Instance.SendRequestAsync(request))
                {
                    return await response.GetTextResultAsync();
                }
            }
        }

        private async Task<List<RadioItem>> GetStationsList(string uri)
        {
            List<RadioItem> stations = new List<RadioItem>();

            try
            {
                string response = await DownloadAsync(uri);
                Stations items = JsonConvert.DeserializeObject<Stations>(response);
                foreach (var item in items.Items)
                {
                    stations.Add(new RadioItem(item.Id, Enums.RadioType.CuteRadio)
                    {
                        Name = item.Title,
                        StreamUrl = item.Source,
                    });
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return stations;
        }

        public async Task<List<string>> GetAllCountries()
        {
            List<string> countries = await cache.GetCountries();

            if (countries.Count != 0) return countries;

            int offset = 0;
            string uri = uriBuilder.GetCountries(offset);
            while (offset < 280)
            {
                try
                {
                    string response = await DownloadAsync(uri);
                    Collection items = JsonConvert.DeserializeObject<Collection>(response);
                    foreach (var item in items.Items)
                    {
                        countries.Add(item.Name);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                offset += 20;
                uri = uriBuilder.GetCountries(offset);
            }
            await cache.CacheCountries(countries);
            return countries;
        }

        public async Task<List<string>> GetAllGenres()
        {
            List<string> genres = await cache.GetGenres();

            if (genres.Count != 0) return genres;

            int offset = 0;
            string uri = uriBuilder.GetGenres(offset);
            while (offset < 280)
            {
                try
                {
                    string response = await DownloadAsync(uri);
                    Collection items = JsonConvert.DeserializeObject<Collection>(response);
                    foreach (var item in items.Items)
                    {
                        genres.Add(item.Name);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                offset += 20;
                uri = uriBuilder.GetGenres(offset);
            }
            await cache.CacheGenres(genres);
            return genres;
        }

        public async Task<List<string>> GetAllLanguages()
        {
            List<string> languages = await cache.GetLanguages();
            if (languages.Count != 0) return languages;

            int offset = 0;
            string uri = uriBuilder.GetLanguages(offset);
            while (offset < 280)
            {
                try
                {
                    string response = await DownloadAsync(uri);
                    Collection items = JsonConvert.DeserializeObject<Collection>(response);
                    foreach (var item in items.Items)
                    {
                        languages.Add(item.Name);
                    }
                }
                catch (Exception ex)
                {

                    throw;
                }
                offset += 20;
                uri = uriBuilder.GetLanguages(offset);
            }
            await cache.CacheLanguages(languages);
            return languages;
        }

        public async Task<List<RadioItem>> GetStations(int page = 0)
        {
            List<RadioItem> stations = new List<RadioItem>();

            string uri = uriBuilder.GetStations(page * limit);
            try
            {
                string response = await DownloadAsync(uri);
                Stations items = JsonConvert.DeserializeObject<Stations>(response);
                foreach (var item in items.Items)
                {
                    stations.Add(new RadioItem(item.Id,Enums.RadioType.CuteRadio)
                    {
                        Name = item.Title,
                        StreamUrl = item.Source,
                    });
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return stations;
        }

        public async Task<List<RadioItem>> SearchStations(string name, int page)
        {
            List<RadioItem> stations = new List<RadioItem>();

            string uri = uriBuilder.SearchStations(name, page * limit);
            try
            {
                string response = await DownloadAsync(uri);
                Stations items = JsonConvert.DeserializeObject<Stations>(response);
                foreach (var item in items.Items)
                {
                    stations.Add(new RadioItem(item.Id, Enums.RadioType.CuteRadio)
                    {
                        Name = item.Title,
                        StreamUrl = item.Source,
                    });
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return stations;
        }

        public async Task<RadioItem> GetStation(int id)
        {
            RadioItem radio;
            string uri = uriBuilder.GetStation(id);
            try
            {
                string response = await DownloadAsync(uri);
                Station item = JsonConvert.DeserializeObject<Station>(response);
                radio = new RadioItem(id, Enums.RadioType.CuteRadio)
                {
                    Name = item.Title,
                    StreamUrl = item.Source,
                };    
            }
            catch (Exception ex)
            {
                radio = new RadioItem();
                throw;
            }
            return radio;
        }

        public async Task<List<RadioItem>> GetStationsByCountry(string country, int page)
        {
            string uri = uriBuilder.GetStationsByCountry(country, page * limit);
            return await GetStationsList(uri);
        }

        public async Task<List<RadioItem>> GetStationsByGenre(string genre, int page)
        {
            string uri = uriBuilder.GetStationsByGenre(genre, page * limit);
            return await GetStationsList(uri);
        }

        public async Task<List<RadioItem>> GetStationsByLanguage(string language, int page)
        {
            string uri = uriBuilder.GetStationsByLanguage(language, page * limit);
            return await GetStationsList(uri);
        }
    }
}
