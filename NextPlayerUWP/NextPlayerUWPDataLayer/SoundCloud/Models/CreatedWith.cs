using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.SoundCloud.Models
{
    public class CreatedWith
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("permalink_url")]
        public string PermalinkUrl { get; set; }

        [JsonProperty("external_url")]
        public string ExternalUrl { get; set; }
    }
}
