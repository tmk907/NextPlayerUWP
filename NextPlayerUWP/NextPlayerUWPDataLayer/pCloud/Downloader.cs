using Newtonsoft.Json;
using NextPlayerUWPDataLayer.pCloud.Model;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud
{
    public class Downloader
    {
        private async Task<string> DownloadAsync(string url)
        {
            String response;
            HttpClient httpClient = new HttpClient();
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

        public async Task<T> DownloadJsonAsync<T>(string url)
        {
            String response = await DownloadAsync(url);
            if (response.StartsWith("Error"))
            {
                ErrorResponse errResponse = new ErrorResponse();
                errResponse.Error = response;
                errResponse.Result = "-1";
                //return errResponse;
            }
            return JsonConvert.DeserializeObject<T>(response);
        }
    }
}
