using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.pCloud.Model
{
    public class LogoutResponse : BaseResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
