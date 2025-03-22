using System;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace NextPlayerWinUIDataLayer.Helpers
{
    public class ClientHttp
    {
        public static async Task<string> DownloadAsync(string uri)
        {
            return await DownloadAsync(new Uri(uri));
        }

        public static async Task<string> DownloadAsync(Uri uri)
        {
            var httpClient = new HttpClient();

            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                using (var response = await httpClient.SendRequestAsync(request))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}
