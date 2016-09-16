using NextPlayerUWPDataLayer.pCloud.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud.Requests
{
    public class Folder
    {
        private Downloader downloader;
        private string authToken;
        private readonly string BaseUrl = "https://api.pcloud.com";

        public Folder(string authToken)
        {
            this.authToken = authToken;
            this.downloader = new Downloader();
        }

        public void SetDownloader(Downloader downloader)
        {
            this.downloader = downloader;
        }

        public async Task<BaseMetadata> ListFolder(int folderId, bool recursive = false, bool showdeleted = false, bool nofiles = false, bool noshares = false)
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
            var url = $"{BaseUrl}/listfolder?auth={authToken}&folderid={folderId}{optionalParams}";
            var response = await downloader.DownloadJsonAsync<BaseMetadata>(url);
            return response;
        }
    }
}
