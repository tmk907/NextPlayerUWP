using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.SpotifyAPI.Web.Models
{
    public class CategoryPlaylist : BasicModel
    {
        [JsonProperty("playlists")]
        public Paging<SimplePlaylist> Playlists { get; set; }
    }
}