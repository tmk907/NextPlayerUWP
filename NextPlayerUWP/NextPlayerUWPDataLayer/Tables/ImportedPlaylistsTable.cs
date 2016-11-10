using SQLite;
using System;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("ImportedPlaylistsTable")]
    public class ImportedPlaylistsTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime DateModified { get; set; }
        public int PlainPlaylistId { get; set; }
    }
}
