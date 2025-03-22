using System.Collections.Generic;

namespace NextPlayerWinUIDataLayer.SpotifyAPI.Web.Models
{
    public class ListResponse<T> : BasicModel
    {
        public List<T> List { get; set; }
    }
}