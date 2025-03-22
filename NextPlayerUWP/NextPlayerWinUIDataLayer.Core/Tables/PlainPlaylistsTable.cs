using SQLite;
using System;

namespace NextPlayerWinUIDataLayer.Tables
{
    [Table("PlainPlaylistsTable")]
    public class PlainPlaylistsTable
    {
        [PrimaryKey, AutoIncrement]
        public int PlainPlaylistId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsHidden { get; set; }
    }
}
