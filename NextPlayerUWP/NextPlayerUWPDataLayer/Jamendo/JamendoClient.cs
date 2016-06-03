using Newtonsoft.Json;
using NextPlayerUWPDataLayer.Jamendo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Jamendo
{
    public class JamendoClient
    {
        public JsonSerializerSettings JsonSettings { get; set; }

        private UriBuilder uriBuilder;

        public JamendoClient()
        {
            uriBuilder = new UriBuilder();
        }

        private async Task<string> DownloadAsync(string url)
        {
            String response;
            HttpClient httpClient = new HttpClient();
            //Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            try
            {
                Uri uri = new Uri(url);
                var httpResponse = await httpClient.GetAsync(uri);
                httpResponse.EnsureSuccessStatusCode();
                response = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                response = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            return response;
        }

        private async Task<Response<T>> DownloadJsonAsync<T>(string url)
        {
            String response = await DownloadAsync(url);
            if (response.StartsWith("Error"))
            {
                Response<T> res = new Response<T>();
                res.Headers = new Headers();
                res.Headers.Status = Enums.ResponseStatus.Failed;
                return res;
            }
            return JsonConvert.DeserializeObject<Response<T>>(response, JsonSettings);
        }

        public async Task<List<Radio>> GetRadios()
        {
            string uri = uriBuilder.GetRadios();
            var response = await DownloadJsonAsync<Radio>(uri);
            if (response.Headers.Status == Enums.ResponseStatus.Success)
            {
                List<Radio> radios = response.Results.ToList();
                return radios;
            }
            return new List<Radio>();
        }

        public async Task<Radio> GetRadio(int id)
        {
            string uri = uriBuilder.GetRadio(id);
            var response = await DownloadJsonAsync<Radio>(uri);
            if (response.Headers.Status == Enums.ResponseStatus.Success && response.Results.Count > 0)
            {
                Radio radio = response.Results.FirstOrDefault();
                return radio;
            }
            return null;
        }

        public async Task<RadioStream> GetStream(int id)
        {
            string uri = uriBuilder.GetStream(id);
            var response = await DownloadJsonAsync<RadioStream>(uri);
            if (response.Headers.Status == Enums.ResponseStatus.Success && response.Results.Count > 0)
            {
                RadioStream stream = response.Results.FirstOrDefault();
                return stream;
            }
            return null;
        }

        public async Task<RadioStream> GetStream(string name)
        {
            string uri = uriBuilder.GetStream(name);
            var response = await DownloadJsonAsync<RadioStream>(uri);
            if (response.Headers.Status == Enums.ResponseStatus.Success && response.Results.Count > 0)
            {
                RadioStream stream = response.Results.FirstOrDefault();
                return stream;
            }
            return null;
        }
    }
}
