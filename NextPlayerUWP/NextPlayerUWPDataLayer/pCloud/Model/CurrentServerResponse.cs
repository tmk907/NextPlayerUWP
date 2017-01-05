using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.pCloud.Model
{
    public class CurrentServerResponse : BaseResponse
    {
        [JsonProperty("ipv6")]
        public string Ipv6 { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("ipbin")]
        public string Ipbin { get; set; }
    }
}
