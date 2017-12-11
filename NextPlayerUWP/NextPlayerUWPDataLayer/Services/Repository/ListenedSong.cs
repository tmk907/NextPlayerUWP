using System;

namespace NextPlayerUWPDataLayer.Services.Repository
{
    public class ListenedSong
    {
        public int EventId { get; set; }
        public int SongId { get; set; }
        public DateTime DatePlayed { get; set; }
        public TimeSpan PlaybackDuration { get; set; }
    }
}
