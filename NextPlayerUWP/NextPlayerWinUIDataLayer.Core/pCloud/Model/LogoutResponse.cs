using Newtonsoft.Json;

namespace NextPlayerWinUIDataLayer.pCloud.Model
{
    public class LogoutResponse : BaseResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
