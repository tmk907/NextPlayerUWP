using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.pCloud.Model
{
    public class BaseResponse
    {
        [JsonProperty("result")]
        public string Result { get; set; }
    }
}
