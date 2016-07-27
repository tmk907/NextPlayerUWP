using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.SoundCloud.Models
{
    public class MiniUser
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("permalink")]
        public string Permalink { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("permalink_url")]
        public string PermalinkUrl { get; set; }

        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }
    }
}
