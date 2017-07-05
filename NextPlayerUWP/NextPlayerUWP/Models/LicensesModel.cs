using Newtonsoft.Json;
using System.Collections.Generic;

namespace NextPlayerUWP.Models
{
    public class LicensesModel
    {
        [JsonProperty("licenses")]
        public List<LicenseModel> Licenses { get; set; }
    }

    public class LicenseModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("license")]
        public LicenseTextModel License { get; set; } 
    }

    public class LicenseTextModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("copyright")]
        public string Copyright { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
