using SQLite;
using System;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("PlainPlaylistsTable")]
    public class PlainPlaylistsTable
    {
        [PrimaryKey, AutoIncrement]
        public int PlainPlaylistId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime DateModified { get; set; }
    }
}
