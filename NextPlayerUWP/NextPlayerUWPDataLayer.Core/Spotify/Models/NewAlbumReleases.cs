using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.SpotifyAPI.Web.Models
{
    public class NewAlbumReleases : BasicModel
    {
        [JsonProperty("albums")]
        public Paging<SimpleAlbum> Albums { get; set; }
    }
}