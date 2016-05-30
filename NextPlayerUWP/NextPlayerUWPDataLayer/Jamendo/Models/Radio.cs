using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.Jamendo.Models
{
    public class Radio
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("dispname")]
        public string DisplayName { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }
    }
}
