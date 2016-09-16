using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud.Requests
{
    public class Collection
    {
        private Downloader downloader;
        private string authToken;
        private readonly string BaseUrl = "https://api.pcloud.com";

        public Collection(string authToken)
        {
            this.authToken = authToken;
            this.downloader = new Downloader();
        }

        public void SetDownloader(Downloader downloader)
        {
            this.downloader = downloader;
        }
    }
}
