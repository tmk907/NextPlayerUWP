namespace NextPlayerUWPDataLayer.Playlists
{
    public class PlaylistParserFactory
    {
        public IPlaylistParser GetParser(string type)
        {
            IPlaylistParser parser;
            if (type == ".m3u" || type == ".m3u8")
            {
                parser = new M3uPlaylistParser();
            }
            else if (type == ".wpl")
            {
                parser = new WplPlaylistParser();
            }
            else if (type == ".pls")
            {
                parser = new PlsPlaylistParser();
            }
            else if (type == ".zpl")
            {
                parser = new ZplPlaylistParser();
            }
            else
            {
                parser = new UnsupportedPlaylistParser();
            }
            return parser;
        }
    }
}
