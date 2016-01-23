using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int SongsNumber { get; set; }
    }
}
