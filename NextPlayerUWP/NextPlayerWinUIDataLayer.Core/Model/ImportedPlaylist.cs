using NextPlayerWinUIDataLayer.Playlists;
using System;
using System.Collections.Generic;

namespace NextPlayerWinUIDataLayer.Model
{
    public class ImportedPlaylist
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime DateModified { get; set; }
        public List<int> SongIds { get; set; }
        public List<GeneralPlaylistEntry> Entries { get; set; }
        public int PlainPlaylistId { get; set; }

        public ImportedPlaylist()
        {
            SongIds = new List<int>();
            Entries = new List<GeneralPlaylistEntry>();
        }
    }
}
