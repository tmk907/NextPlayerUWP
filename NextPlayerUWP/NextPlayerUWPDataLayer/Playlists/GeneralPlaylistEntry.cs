using PlaylistsNET.Model;
using System;

namespace NextPlayerUWPDataLayer.Playlists
{
    public class GeneralPlaylistEntry
    {
        public string Path { get; set; }
        public string AlbumTitle { get; set; }
        public string AlbumArtist { get; set; }
        public string TrackTitle { get; set; }
        public string TrackArtist { get; set; }
        public TimeSpan Duration { get; set; }

        public GeneralPlaylistEntry(M3uPlaylistEntry entry)
        {
            Path = entry.Path ?? "";
            TrackTitle = entry.Title;
            Duration = entry.Duration;
            AlbumTitle = entry.Album;
            AlbumArtist = entry.AlbumArtist;
        }

        public GeneralPlaylistEntry(PlsPlaylistEntry entry)
        {
            Path = entry.Path ?? "";
            TrackTitle = entry.Title;
            Duration = entry.Length;
        }

        public GeneralPlaylistEntry(WplPlaylistEntry entry)
        {
            Path = entry.Path ?? "";
            TrackTitle = entry.TrackTitle;
            TrackArtist = entry.TrackArtist;
            AlbumTitle = entry.AlbumTitle;
            AlbumArtist = entry.AlbumArtist;
            Duration = TimeSpan.Zero;
        }

        public GeneralPlaylistEntry(ZplPlaylistEntry entry)
        {
            Path = entry.Path ?? "";
            TrackTitle = entry.TrackTitle;
            TrackArtist = entry.TrackArtist;
            AlbumTitle = entry.AlbumTitle;
            AlbumArtist = entry.AlbumArtist;
            Duration = TimeSpan.Zero;
        }
    }
}
