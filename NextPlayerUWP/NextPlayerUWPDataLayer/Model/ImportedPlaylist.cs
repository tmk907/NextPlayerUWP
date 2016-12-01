using NextPlayerUWPDataLayer.Playlists;
using System;
using System.Collections.Generic;

namespace NextPlayerUWPDataLayer.Model
{
    public class ImportedPlaylist
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime DateModified { get; set; }
        public List<int> SongIds { get; set; }
        public List<MaxPlaylistEntry> max { get; set; }
        public int PlainPlaylistId { get; set; }

        public ImportedPlaylist()
        {
            SongIds = new List<int>();
            max = new List<MaxPlaylistEntry>();
        }
    }
}
