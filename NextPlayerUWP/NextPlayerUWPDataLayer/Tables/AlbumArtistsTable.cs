using SQLite;
using System;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("AlbumArtistsTable")]
    public class AlbumArtistsTable
    {
        [PrimaryKey, AutoIncrement]
        public int AlbumArtistId { get; set; }
        public string AlbumArtist { get; set; }
        public TimeSpan Duration { get; set; }
        public int SongsNumber { get; set; }
        public int AlbumsNumber { get; set; }
        public DateTime LastAdded { get; set; }
    }
}
