using Newtonsoft.Json;

namespace NextPlayerWinUIDataLayer.pCloud.Model
{
    public class BaseResponse
    {
        [JsonProperty("result")]
        public string Result { get; set; }
    }
}
