using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.pCloud.Model
{
    public class GetIPResponse : BaseResponse
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }
}
