using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("GenresTable")]
    public class GenresTable
    {
        [PrimaryKey, AutoIncrement]
        public int GenreId { get; set; }
        public string Genre { get; set; }
        public int SongsSumber { get; set; }
        public TimeSpan Duration { get; set; }

    }
}
