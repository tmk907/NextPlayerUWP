using Newtonsoft.Json;
using System;

namespace NextPlayerUWPDataLayer.pCloud.Model
{
    public class DigestResponse : BaseResponse
    {
        [JsonProperty("digest")]
        public string Digest { get; set; }

        [JsonProperty("expires")]
        public DateTime Expires { get; set; }
    }
}
