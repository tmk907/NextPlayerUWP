using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("NowPlayingTable")]
    class NowPlayingTable
    {
        [PrimaryKey]
        public int Position { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Path { get; set; }
        public int SongId { get; set; }
    }
}
