using SQLite;
using System;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("GenresTable")]
    public class GenresTable
    {
        [PrimaryKey, AutoIncrement]
        public int GenreId { get; set; }
        public string Genre { get; set; }
        public int SongsNumber { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime LastAdded { get; set; }
    }
}
