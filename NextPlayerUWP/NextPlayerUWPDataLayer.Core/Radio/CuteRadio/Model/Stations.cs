using Newtonsoft.Json;
using System.Collections.Generic;

namespace NextPlayerUWPDataLayer.Radio.CuteRadio.Model
{
    public class Stations
    {
        [JsonProperty("items")]
        public IList<Station> Items { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        [JsonProperty("previous")]
        public string Previous { get; set; }
    }
}
