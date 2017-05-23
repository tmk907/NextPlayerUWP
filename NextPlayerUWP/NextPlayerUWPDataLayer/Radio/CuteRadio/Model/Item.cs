using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.Radio.CuteRadio.Model
{
    public class Item
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
