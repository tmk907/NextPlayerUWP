﻿using SQLite;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("PlainPlaylistsTable")]
    public class PlainPlaylistsTable
    {
        [PrimaryKey, AutoIncrement]
        public int PlainPlaylistId { get; set; }
        public string Name { get; set; }
    }
}
