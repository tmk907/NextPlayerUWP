using SQLite;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("CachedScrobble")]
    public class CachedScrobble
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string Artist { get; set; }
        public string Track { get; set; }
        public string Timestamp { get; set; }
        public string Function { get; set; }
    }
}
