using Newtonsoft.Json;

namespace NextPlayerWinUIDataLayer.Radio.CuteRadio.Model
{
    public class Item
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
