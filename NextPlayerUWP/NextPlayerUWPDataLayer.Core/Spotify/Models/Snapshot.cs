using Newtonsoft.Json;
using System;

namespace NextPlayerUWPDataLayer.SpotifyAPI.Web.Models
{
    public class Snapshot : BasicModel
    {
        [JsonProperty("snapshot_id")]
        public String SnapshotId { get; set; }
    }
}