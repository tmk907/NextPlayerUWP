using Newtonsoft.Json;

namespace NextPlayerWinUIDataLayer.pCloud.Model
{
    public class MetadataResponse : BaseResponse
    {
        [JsonProperty("metadata")]
        public BaseMetadata Metadata { get; set; }
    }
}
