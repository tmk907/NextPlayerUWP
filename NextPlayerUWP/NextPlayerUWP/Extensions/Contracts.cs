using Newtonsoft.Json;

namespace NextPlayerUWP.Extensions
{
    public class Commands
    {
        public const string GetLyrics = "GetLyrics";
        public const string GetAlbumInfo = "GetAlbumInfo";
        public const string GetArtistInfo = "GetArtistInfo";
    }

    public class Responses
    {
        public const string Lyrics = "Lyrics";
        public const string AlbumInfo = "AlbumInfo";
        public const string ArtistInfo = "ArtistInfo";
        public const string Status = "Status";
    }

    public class LyricsParameters
    {
        [JsonProperty("album")]
        public string Album { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class ArtistInfoParameters
    {
        [JsonProperty("artist")]
        public string Artist { get; set; }
    }

    public class AlbumInfoParameters
    {
        [JsonProperty("album")]
        public string Album { get; set; }
    }
}
