using SQLite;
using System;

namespace NextPlayerWinUIDataLayer.Tables
{
    [Table("FoldersTable")]
    public class FoldersTable
    {
        [PrimaryKey, AutoIncrement]
        public int FolderId { get; set; }
        public string Folder { get; set; }
        public string Directory { get; set; }
        public int SongsNumber { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime LastAdded { get; set; }
    }
}
