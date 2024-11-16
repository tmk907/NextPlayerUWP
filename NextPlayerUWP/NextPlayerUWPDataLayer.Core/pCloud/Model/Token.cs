using Newtonsoft.Json;
using System;

namespace NextPlayerUWPDataLayer.pCloud.Model
{
    public class Token
    {
        [JsonProperty("tokenid")]
        public long Tokenid { get; set; }

        [JsonProperty("device")]
        public string Device { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("expires_inactive")]
        public DateTime ExpiresInactive { get; set; }

        [JsonProperty("expires")]
        public DateTime Expires { get; set; }
    }
}
