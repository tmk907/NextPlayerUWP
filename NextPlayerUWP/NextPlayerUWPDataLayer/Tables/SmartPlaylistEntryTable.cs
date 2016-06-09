using SQLite;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("SmartPlaylistEntryTable")]
    public class SmartPlaylistEntryTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int PlaylistId { get; set; }
        public string Comparison { get; set; }
        public string Item { get; set; }
        public string Value { get; set; }
        public string Operator { get; set; }
    }
}
