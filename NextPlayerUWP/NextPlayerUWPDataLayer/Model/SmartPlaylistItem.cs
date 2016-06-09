namespace NextPlayerUWPDataLayer.Model
{
    public class SmartPlaylistItem : PlaylistItem
    {
        public string Sorting { get; set; }
        public int SongsNumber { get; set; }

        public SmartPlaylistItem(int id, bool issmart, string _name) : base(id, issmart, _name)
        {
            
        }

        public SmartPlaylistItem(int id, string _name, int songsNumber, string _sorting) : base(id, true, _name)
        {
            Sorting = _sorting;
            SongsNumber = songsNumber;
        }
    }
}
