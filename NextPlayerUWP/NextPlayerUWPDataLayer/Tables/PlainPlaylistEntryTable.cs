using System;
using SQLite;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("PlainPlaylistEntryTable")]    
    class PlainPlaylistEntryTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int PlaylistId { get; set; }
        public int SongId { get; set; }
        public int Place { get; set; }
    }
}
