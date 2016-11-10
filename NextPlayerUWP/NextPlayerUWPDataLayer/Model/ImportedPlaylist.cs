using System;
using System.Collections.Generic;

namespace NextPlayerUWPDataLayer.Model
{
    public class ImportedPlaylist
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime DateModified { get; set; }
        public List<string> SongPaths { get; set; }
        public int PlainPlaylistId { get; set; }
        public ImportedPlaylist()
        {
            SongPaths = new List<string>();
        }
    }
}
