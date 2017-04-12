using Newtonsoft.Json;

namespace NextPlayerUWP.Extensions
{
    public class ArtistInfoRequest
    {
        [JsonProperty("artist")]
        public string Artist { get; set; }
    }

    public class ArtistInfoResponse
    {

    }

    public class Commands
    {
        public const string GetLyrics = "GetLyrics";
        public const string GetAlbumInfo = "GetAlbumInfo";
        public const string GetArtistInfo = "GetArtistInfo";
    }

    public class LyricsRequest
    {
        [JsonProperty("album")]
        public string Album { get; set; }

        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public class LyricsResponse
    {
        [JsonProperty("lyrics")]
        public string Lyrics { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class Responses
    {
        public const string Status = "Status";
        public const string Result = "Result";
    }

    public class AlbumInfoRequest
    {
        [JsonProperty("album")]
        public string Album { get; set; }
    }

    public class AlbumInfoResponse
    {

    }
}
