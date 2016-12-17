using Newtonsoft.Json;

namespace NextPlayerUWPDataLayer.pCloud.Model
{
    public class MetadataResponse : BaseResponse
    {
        [JsonProperty("metadata")]
        public BaseMetadata Metadata { get; set; }
    }
}
