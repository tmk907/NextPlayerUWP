using Newtonsoft.Json;

namespace NextPlayerWinUIDataLayer.pCloud.Model
{
    public class ErrorResponse : BaseResponse
    {
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
