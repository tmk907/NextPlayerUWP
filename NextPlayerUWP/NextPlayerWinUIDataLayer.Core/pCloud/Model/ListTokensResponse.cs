using Newtonsoft.Json;
using System.Collections.Generic;

namespace NextPlayerWinUIDataLayer.pCloud.Model
{
    public class ListTokensResponse : BaseResponse
    {
        [JsonProperty("tokens")]
        public IList<Token> Tokens { get; set; }
    }
}
