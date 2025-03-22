using Newtonsoft.Json;
using System;

namespace NextPlayerWinUIDataLayer.pCloud.Model
{
    public class DigestResponse : BaseResponse
    {
        [JsonProperty("digest")]
        public string Digest { get; set; }

        [JsonProperty("expires")]
        public DateTime Expires { get; set; }
    }
}
