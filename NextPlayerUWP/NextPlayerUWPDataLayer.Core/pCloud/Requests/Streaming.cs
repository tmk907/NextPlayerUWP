using NextPlayerUWPDataLayer.pCloud.Model;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud.Requests
{
    public class Streaming : BaseRequest
    {
        public Streaming()
        {
            this.downloader = new Downloader();
        }

        public async Task<GetStreamingLinkResponse> GetFileLink(int fileId)
        {
            var url = $"{BaseUrl}/getfilelink?{authParam}={authToken}&fileid={fileId}";
            var response = await downloader.DownloadJsonAsync<GetStreamingLinkResponse>(url);
            return response;
        }

        public async Task<GetStreamingLinkResponse> GetAudioLink(int fileId)
        {
            var url = $"{BaseUrl}/getaudiolink?{authParam}={authToken}&fileid={fileId}";
            var response = await downloader.DownloadJsonAsync<GetStreamingLinkResponse>(url);
            return response;
        }

        public async Task<GetStreamingLinkResponse> GetAudioLink(int fileId, int abitrate)
        {
            var url = $"{BaseUrl}/getaudiolink?{authParam}={authToken}&fileid={fileId}&abitrate={abitrate}";
            var response = await downloader.DownloadJsonAsync<GetStreamingLinkResponse>(url);
            return response;
        }
    }
}
