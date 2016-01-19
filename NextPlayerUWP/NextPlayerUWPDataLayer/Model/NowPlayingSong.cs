namespace NextPlayerUWPDataLayer.Model
{
    public class NowPlayingSong
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Path { get; set; }
        public int SongId { get; set; }
        public int Position { get; set; }
    }
}
