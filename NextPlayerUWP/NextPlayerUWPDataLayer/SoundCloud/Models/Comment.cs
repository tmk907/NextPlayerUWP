using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.SoundCloud.Models
{
    public class Comment
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("track_id")]
        public int TrackId { get; set; }

        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("user")]
        public MiniUser User { get; set; }
    }
}
