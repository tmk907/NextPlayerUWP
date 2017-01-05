using NextPlayerUWPDataLayer.pCloud.Model;
using System;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud.Requests
{
    public class Folder : BaseRequest
    {
        public Folder()
        {
            this.downloader = new Downloader();
        }

        public async Task<MetadataResponse> ListFolder(int folderId, bool recursive = false, bool showdeleted = false, bool nofiles = false, bool noshares = false)
        {
            StringBuilder sb = new StringBuilder();
            if (recursive)
            {
                sb.Append("&recursive=1");
            }
            if (showdeleted)
            {
                sb.Append("&showdeleted=1");
            }
            if (nofiles)
            {
                sb.Append("&nofiles=1");
            }
            if (noshares)
            {
                sb.Append("&noshares=1");
            }
            var optionalParams = sb.ToString();
            var url = $"{BaseUrl}/listfolder?{authParam}={authToken}&folderid={folderId}{optionalParams}";
            var response = await downloader.DownloadJsonAsync<MetadataResponse>(url);
            return response;
        }
    }
}
