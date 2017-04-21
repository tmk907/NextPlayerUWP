using Microsoft.Toolkit.Uwp;
using Newtonsoft.Json;
using NextPlayerUWPDataLayer.Radio.CuteRadio.Model;
using System;
using System.Collections.Generic;
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

        private async Task<List<Station>> GetStationsList(string uri)
        {
            List<Station> stations = new List<Station>();

            try
            {
                string response = await DownloadAsync(uri);
                Stations items = JsonConvert.DeserializeObject<Stations>(response);
                foreach (var item in items.Items)
                {
                    stations.Add(item);
                }
            }
            catch (Exception ex)
            {

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

                }
                offset += 20;
                uri = uriBuilder.GetLanguages(offset);
            }
            await cache.CacheLanguages(languages);
            return languages;
        }

        public async Task<List<Station>> GetStations(int page = 0)
        {
            List<Station> stations = new List<Station>();

            string uri = uriBuilder.GetStations(page * limit);
            try
            {
                string response = await DownloadAsync(uri);
                Stations items = JsonConvert.DeserializeObject<Stations>(response);
                foreach (var item in items.Items)
                {
                    stations.Add(item);
                }
            }
            catch (Exception ex)
            {

            }

            return stations;
        }

        public async Task<List<Station>> SearchStations(string name, int page)
        {
            List<Station> stations = new List<Station>();

            string uri = uriBuilder.SearchStations(name, page * limit);
            try
            {
                string response = await DownloadAsync(uri);
                Stations items = JsonConvert.DeserializeObject<Stations>(response);
                foreach (var item in items.Items)
                {
                    stations.Add(item);
                }
            }
            catch (Exception ex)
            {

            }

            return stations;
        }

        public async Task<Station> GetStation(int id)
        {
            Station station;
            string uri = uriBuilder.GetStation(id);
            try
            {
                string response = await DownloadAsync(uri);
                station = JsonConvert.DeserializeObject<Station>(response);
                   
            }
            catch (Exception ex)
            {
                station = new Station()
                {
                    Id = -1,
                    Source = "",
                };
            }
            return station;
        }

        public async Task<List<Station>> GetStationsByCountry(string country, int page)
        {
            string uri = uriBuilder.GetStationsByCountry(country, page * limit);
            return await GetStationsList(uri);
        }

        public async Task<List<Station>> GetStationsByGenre(string genre, int page)
        {
            string uri = uriBuilder.GetStationsByGenre(genre, page * limit);
            return await GetStationsList(uri);
        }

        public async Task<List<Station>> GetStationsByLanguage(string language, int page)
        {
            string uri = uriBuilder.GetStationsByLanguage(language, page * limit);
            return await GetStationsList(uri);
        }
    }
}
