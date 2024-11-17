using Newtonsoft.Json;
using System.Collections.Generic;

namespace NextPlayerUWPDataLayer.Radio.CuteRadio.Model
{
    public class Collection
    {
        [JsonProperty("items")]
        public IList<Item> Items { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        [JsonProperty("previous")]
        public string Previous { get; set; }
    }
}
