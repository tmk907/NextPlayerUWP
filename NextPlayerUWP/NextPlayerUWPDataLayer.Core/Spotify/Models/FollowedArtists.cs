using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.SpotifyAPI.Web.Models
{
    public class FollowedArtists : BasicModel
    {
        [JsonProperty("artists")]
        public CursorPaging<FullArtist> Artists { get; set; }
    }
}