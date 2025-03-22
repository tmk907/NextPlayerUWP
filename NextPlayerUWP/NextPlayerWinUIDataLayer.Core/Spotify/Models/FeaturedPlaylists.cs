using Newtonsoft.Json;
using System;

namespace NextPlayerWinUIDataLayer.SpotifyAPI.Web.Models
{
    public class FeaturedPlaylists : BasicModel
    {
        [JsonProperty("message")]
        public String Message { get; set; }

        [JsonProperty("playlists")]
        public Paging<SimplePlaylist> Playlists { get; set; }
    }
}