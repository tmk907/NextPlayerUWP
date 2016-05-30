using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.Jamendo.Models
{
    public class Playingnow
    {
        [JsonProperty("track_id")]
        public string TrackId { get; set; }

        [JsonProperty("artist_id")]
        public string ArtistId { get; set; }

        [JsonProperty("album_id")]
        public string AlbumId { get; set; }

        [JsonProperty("album_name")]
        public string AlbumName { get; set; }

        [JsonProperty("track_name")]
        public string TrackName { get; set; }

        [JsonProperty("track_image")]
        public string TrackImage { get; set; }

        [JsonProperty("artist_name")]
        public string ArtistName { get; set; }
    }
}
