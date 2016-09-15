using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.pCloud.Model
{
    public class ErrorResponse : BaseResponse
    {
        [JsonProperty("error")]
        public string Error { get; set; }
    }
}
