using SQLite;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("NowPlayingTable")]
    class NowPlayingTable
    {
        [PrimaryKey]
        public int Position { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Path { get; set; }
        public string ImagePath { get; set; }
        public int SongId { get; set; }
    }
}
