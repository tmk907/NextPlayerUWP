using Newtonsoft.Json;
using NextPlayerUWPDataLayer.SoundCloud.Enums;
using System.Collections.Generic;

namespace NextPlayerUWPDataLayer.SoundCloud.Models
{
    public class Playlist
    {

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("user")]
        public MiniUser User { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("permalink")]
        public string Permalink { get; set; }

        [JsonProperty("permalink_url")]
        public string PermalinkUrl { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("sharing")]
        public string Sharing { get; set; }

        [JsonProperty("embeddable_by")]
        public string EmbeddableBy { get; set; }

        [JsonProperty("purchase_url")]
        public string PurchaseUrl { get; set; }

        [JsonProperty("artwork_url")]
        public string ArtworkUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("genre")]
        public string Genre { get; set; }

        [JsonProperty("tag_list")]
        public string TagList { get; set; }

        [JsonProperty("label_id")]
        public int LabelId { get; set; }

        [JsonProperty("label_name")]
        public string LabelName { get; set; }

        [JsonProperty("release")]
        public int Release { get; set; }

        [JsonProperty("release_year")]
        public int ReleaseYear { get; set; }

        [JsonProperty("release_month")]
        public int ReleaseMonth { get; set; }

        [JsonProperty("release_day")]
        public int ReleaseDay { get; set; }

        [JsonProperty("streamable")]
        public bool Streamable { get; set; }

        [JsonProperty("downloadable")]
        public bool Downloadable { get; set; }

        [JsonProperty("ean")]
        public string Ean { get; set; }

        [JsonProperty("playlist_type")]
        public SCPlaylistType PlaylistType { get; set; }

        [JsonProperty("tracks")]
        public IList<Track> Tracks { get; set; }
    }

}
