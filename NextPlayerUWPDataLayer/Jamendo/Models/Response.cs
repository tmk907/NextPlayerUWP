using Newtonsoft.Json;
using System.Collections.Generic;

namespace NextPlayerUWPDataLayer.Jamendo.Models
{
    public class Response<TResult>
    {
        [JsonProperty("headers")]
        public Headers Headers { get; set; }

        [JsonProperty("results")]
        public IList<TResult> Results { get; set; }
    }
}
