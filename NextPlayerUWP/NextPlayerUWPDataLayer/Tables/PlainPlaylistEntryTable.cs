using SQLite;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("PlainPlaylistEntryTable")]    
    public class PlainPlaylistEntryTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int PlaylistId { get; set; }
        public int SongId { get; set; }
        public int Place { get; set; }
        public string Path { get; set; }
        public string DisplayName { get; set; }
    }
}
