using SQLite;
using System;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("ListeningHistoryTable")]
    public class ListeningHistoryTable
    {
        [PrimaryKey, AutoIncrement]
        public int EventId { get; set; }
        public int SongId { get; set; }
        public DateTime DatePlayed { get; set; }
        public TimeSpan PlaybackDuration { get; set; }
    }
}
