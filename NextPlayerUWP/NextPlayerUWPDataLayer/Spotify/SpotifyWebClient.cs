using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;

namespace NextPlayerUWPDataLayer.SpotifyAPI.Web
{
    internal class SpotifyWebClient : IClient
    {
        public JsonSerializerSettings JsonSettings { get; set; }

        public SpotifyWebClient()
        {
        }

        public async Task<string> DownloadAsync(string url)
        {
            String response;
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            try
            {
                Uri uri = new Uri(url);
                httpResponse = await httpClient.GetAsync(uri);
                httpResponse.EnsureSuccessStatusCode();
                response = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                response = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            return response;
        }
        
        public async Task<T> DownloadJsonAsync<T>(string url)
        {
            String response = await DownloadAsync(url);
            return JsonConvert.DeserializeObject<T>(response, JsonSettings);
        }

        public string Download(string url)
        {
            throw new NotImplementedException();
        }

        public byte[] DownloadRaw(string url)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> DownloadRawAsync(string url)
        {
            throw new NotImplementedException();
        }

        public T DownloadJson<T>(string url)
        {
            throw new NotImplementedException();
        }

        public string Upload(string url, string body, string method)
        {
            throw new NotImplementedException();
        }

        public Task<string> UploadAsync(string url, string body, string method)
        {
            throw new NotImplementedException();
        }

        public byte[] UploadRaw(string url, string body, string method)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> UploadRawAsync(string url, string body, string method)
        {
            throw new NotImplementedException();
        }

        public T UploadJson<T>(string url, string body, string method)
        {
            throw new NotImplementedException();
        }

        public Task<T> UploadJsonAsync<T>(string url, string body, string method)
        {
            throw new NotImplementedException();
        }

        public void SetHeader(string header, string value)
        {
            throw new NotImplementedException();
        }

        public void RemoveHeader(string header)
        {
            throw new NotImplementedException();
        }

        public List<KeyValuePair<string, string>> GetHeaders()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}