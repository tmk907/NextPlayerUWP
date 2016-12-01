using PlaylistsNET.Model;
using System;

namespace NextPlayerUWPDataLayer.Playlists
{
    public class MaxPlaylistEntry
    {
        public string Path { get; set; }
        public string AlbumTitle { get; set; }
        public string AlbumArtist { get; set; }
        public string TrackTitle { get; set; }
        public string TrackArtist { get; set; }
        public TimeSpan Duration { get; set; }

        public MaxPlaylistEntry(M3uPlaylistEntry entry)
        {
            Path = entry.Path;
            TrackTitle = entry.Title;
            Duration = entry.Duration;
            AlbumTitle = entry.Album;
            AlbumArtist = entry.AlbumArtist;
        }

        public MaxPlaylistEntry(PlsPlaylistEntry entry)
        {
            Path = entry.Path;
            TrackTitle = entry.Title;
            Duration = entry.Length;
        }

        public MaxPlaylistEntry(WplPlaylistEntry entry)
        {
            Path = entry.Path;
            TrackTitle = entry.TrackTitle;
            TrackArtist = entry.TrackArtist;
            AlbumTitle = entry.AlbumTitle;
            AlbumArtist = entry.AlbumArtist;
            Duration = TimeSpan.Zero;
        }

        public MaxPlaylistEntry(ZplPlaylistEntry entry)
        {
            Path = entry.Path;
            TrackTitle = entry.TrackTitle;
            TrackArtist = entry.TrackArtist;
            AlbumTitle = entry.AlbumTitle;
            AlbumArtist = entry.AlbumArtist;
            Duration = TimeSpan.Zero;
        }
    }
}
