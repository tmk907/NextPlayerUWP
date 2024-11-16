using Newtonsoft.Json;
using System.Collections.Generic;

namespace NextPlayerUWPDataLayer.pCloud.Model
{
    public class GetStreamingLinkResponse : BaseResponse
    {
        [JsonProperty("expires")]
        public string Expires { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("hosts")]
        public IList<string> Hosts { get; set; }
    }
}
