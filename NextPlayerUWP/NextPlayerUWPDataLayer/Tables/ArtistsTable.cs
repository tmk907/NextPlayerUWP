using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("ArtistsTable")]
    public class ArtistsTable
    {
        [PrimaryKey,AutoIncrement]
        public int ArtistId { get; set; }
        public string Artist { get; set; }
        public TimeSpan Duration { get; set; }
        public int SongsNumber { get; set; }
        public int AlbumsNumber { get; set; }

    }
}
