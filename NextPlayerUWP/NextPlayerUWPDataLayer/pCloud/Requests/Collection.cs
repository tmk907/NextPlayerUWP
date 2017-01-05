using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud.Requests
{
    public class Collection : BaseRequest
    {
        public Collection()
        {
            this.downloader = new Downloader();
        }
    }
}
