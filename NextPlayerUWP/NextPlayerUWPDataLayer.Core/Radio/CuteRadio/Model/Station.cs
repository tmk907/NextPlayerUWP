using Newtonsoft.Json;
using System;

namespace NextPlayerUWPDataLayer.Radio.CuteRadio.Model
{
    public class Station
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("genre")]
        public string Genre { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("playCount")]
        public int PlayCount { get; set; }

        [JsonProperty("lastPlayed")]
        public DateTime? LastPlayed { get; set; }

        [JsonProperty("favourite")]
        public bool Favourite { get; set; }

        [JsonProperty("creatorId")]
        public int CreatorId { get; set; }

        [JsonProperty("approved")]
        public bool Approved { get; set; }
    }
}
