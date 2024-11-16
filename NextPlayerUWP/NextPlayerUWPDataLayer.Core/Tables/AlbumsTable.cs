using SQLite;
using System;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("AlbumsTable")]
    public class AlbumsTable
    {
        [PrimaryKey,AutoIncrement]
        public int AlbumId { get; set; }
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public TimeSpan Duration { get; set; }
        public string ImagePath { get; set; }
        public int SongsNumber { get; set; }
        public int Year { get; set; }
        public DateTime LastAdded { get; set; }
    }
}
