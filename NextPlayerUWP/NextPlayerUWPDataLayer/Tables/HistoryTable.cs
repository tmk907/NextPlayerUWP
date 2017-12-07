using SQLite;
using System;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("HistoryTable")]
    public class HistoryTable
    {
        [PrimaryKey, AutoIncrement]
        public int HistId { get; set; }
        public int SongId { get; set; }
        public DateTime DatePlayed { get; set; }
        public TimeSpan PlaybackDuration { get; set; }
    }
}
