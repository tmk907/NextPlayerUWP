using NextPlayerUWPDataLayer.pCloud.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.pCloud
{
    public class PCloudClient
    {
        private string refreshToken;
        private string authToken;
        public PCloudClient(string refreshToken)
        {
            this.refreshToken = refreshToken;
        }

        private General general;
        public General General
        {
            get
            {
                if (general == null)
                {
                    general = new General(authToken);
                }
                return general;
            }
        }

        private Auth auth;
        public Auth Auth
        {
            get
            {
                if (auth == null)
                {
                    auth = new Auth(authToken);
                }
                return auth;
            }
        }
    }
}
