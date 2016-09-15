using NextPlayerUWPDataLayer.pCloud.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud.Requests
{
    public class Streaming
    {
        private Downloader downloader;
        private string authToken;
        private readonly string BaseUrl = "https://api.pcloud.com";

        public Streaming(string authToken)
        {
            this.authToken = authToken;
            this.downloader = new Downloader();
        }

        public void SetDownloader(Downloader downloader)
        {
            this.downloader = downloader;
        }

        public async Task<GetStreamingLinkResponse> GetFileLink(int fileId)
        {
            var url = $"{BaseUrl}/getfilelink?auth={authToken}&fileid={fileId}";
            var response = await downloader.DownloadJsonAsync<GetStreamingLinkResponse>(url);
            return response;
        }

        public async Task<GetStreamingLinkResponse> GetAudioLink(int fileId)
        {
            var url = $"{BaseUrl}/getaudiolink?auth={authToken}&fileid={fileId}";
            var response = await downloader.DownloadJsonAsync<GetStreamingLinkResponse>(url);
            return response;
        }

        public async Task<GetStreamingLinkResponse> GetAudioLink(int fileId, int abitrate)
        {
            var url = $"{BaseUrl}/getaudiolink?auth={authToken}&fileid={fileId}&abitrate={abitrate}";
            var response = await downloader.DownloadJsonAsync<GetStreamingLinkResponse>(url);
            return response;
        }
    }
}
