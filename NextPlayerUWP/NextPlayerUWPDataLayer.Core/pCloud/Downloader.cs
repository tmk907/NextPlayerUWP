using Newtonsoft.Json;
using NextPlayerUWPDataLayer.pCloud.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
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
                using (var reader = new StreamReader((await httpResponse.Content.ReadAsStreamAsync()), Encoding.UTF8))
                {
                    response = reader.ReadToEnd();
                }

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
            var json = JsonConvert.DeserializeObject<T>(response);
            return json;
        }
    }
}
