using Newtonsoft.Json;
using NextPlayerUWPDataLayer.Jamendo.Enums;

namespace NextPlayerUWPDataLayer.Jamendo.Models
{
    public class Headers
    {
        [JsonProperty("status")]
        public ResponseStatus Status { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("error_message")]
        public string ErrorMessage { get; set; }

        [JsonProperty("warnings")]
        public string Warnings { get; set; }

        [JsonProperty("results_count")]
        public int ResultsCount { get; set; }
    }
}
