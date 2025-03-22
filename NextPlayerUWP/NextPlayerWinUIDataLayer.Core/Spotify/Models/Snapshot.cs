using Newtonsoft.Json;
using System;

namespace NextPlayerWinUIDataLayer.SpotifyAPI.Web.Models
{
    public class Snapshot : BasicModel
    {
        [JsonProperty("snapshot_id")]
        public String SnapshotId { get; set; }
    }
}