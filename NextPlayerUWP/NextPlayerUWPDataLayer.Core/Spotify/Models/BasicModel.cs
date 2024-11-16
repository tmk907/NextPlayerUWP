using Newtonsoft.Json;
using System;

namespace NextPlayerUWPDataLayer.SpotifyAPI.Web.Models
{
    public abstract class BasicModel
    {
        [JsonProperty("error")]
        public Error Error { get; set; }

        public Boolean HasError()
        {
            return Error != null;
        }
    }
}