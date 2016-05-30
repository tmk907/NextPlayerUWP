using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.Jamendo.Models
{
    public class RadioStream
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("dispname")]
        public string Dispname { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("stream")]
        public string StreamUrl { get; set; }

        [JsonProperty("playingnow")]
        public Playingnow PlayingNow { get; set; }

        [JsonProperty("callmeback")]
        public int CallMeBack { get; set; }
    }
}
