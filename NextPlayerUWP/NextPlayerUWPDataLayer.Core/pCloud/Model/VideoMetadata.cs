using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.pCloud.Model
{
    public class VideoMetadata : BaseMetadata
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("fps")]
        public double FPS { get; set; }

        [JsonProperty("videocodec")]
        public string VideoCodec { get; set; }

        [JsonProperty("audiocodec")]
        public string AudioCodec { get; set; }

        [JsonProperty("videobitrate")]
        public int VideoBitrate { get; set; }

        [JsonProperty("audiobitrate")]
        public int AudioBitrate { get; set; }

        [JsonProperty("audiosample")]
        public int AudioSample { get; set; }

        [JsonProperty("rotate")]
        public int Rotate { get; set; }

    }
}
