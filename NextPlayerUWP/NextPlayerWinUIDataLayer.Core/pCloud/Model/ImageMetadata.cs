using Newtonsoft.Json;

namespace NextPlayerWinUIDataLayer.pCloud.Model
{
    public class ImageMetadata : BaseMetadata
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }
}
