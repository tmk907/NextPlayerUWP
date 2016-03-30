using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NextPlayerUWPDataLayer.Services
{
    public class SpotifyService
    {
        private const string ClientID = "70936dc2ba764975b864113e7bc62ee0";
        private const string ClientSecret = "9947554c70d5499a9ed30e99b67418f0";

        private const string baseURL = "https://api.spotify.com";

        

        public SpotifyService()
        {
            

        }

        public async Task SearchAlbum(string album)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(baseURL);
            builder.Append("v1/search");
            builder.Append("?q=album:\"");
            builder.Append(album.Replace(' ', '+'));
            builder.Append("\"");
            builder.Append("&type = album");
        }


    }
}
