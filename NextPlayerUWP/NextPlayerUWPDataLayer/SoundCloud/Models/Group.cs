using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.SoundCloud.Models
{
    public class Group
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("permalink")]
        public string Permalink { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("short_description")]
        public string ShortDescription { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("artwork_url")]
        public string ArtworkUrl { get; set; }

        [JsonProperty("permalink_url")]
        public string PermalinkUrl { get; set; }

        [JsonProperty("creator")]
        public MiniUser Creator { get; set; }
    }
}
