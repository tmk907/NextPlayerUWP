using Newtonsoft.Json;
using System;

namespace NextPlayerUWPDataLayer.pCloud.Model
{
    public class UserInfoResponse : BaseResponse
    {
        [JsonProperty("userid")]
        public int UserId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("emailverified")]
        public bool Emailverified { get; set; }

        [JsonProperty("registered")]
        public DateTime Registered { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("premium")]
        public bool Premium { get; set; }

        [JsonProperty("usedquota")]
        public int Usedquota { get; set; }

        [JsonProperty("quota")]
        public int Quota { get; set; }
    }

}
