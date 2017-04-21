using Microsoft.Toolkit.Uwp;
using System;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace NextPlayerUWPDataLayer.Helpers
{
    public class ClientHttp
    {
        public static async Task<string> DownloadAsync(string uri)
        {
            return await DownloadAsync(new Uri(uri));
        }

        public static async Task<string> DownloadAsync(Uri uri)
        {
            using (var request = new HttpHelperRequest(uri, HttpMethod.Get))
            {
                using (var response = await HttpHelper.Instance.SendRequestAsync(request))
                {
                    return await response.GetTextResultAsync();
                }
            }
        }
    }
}
