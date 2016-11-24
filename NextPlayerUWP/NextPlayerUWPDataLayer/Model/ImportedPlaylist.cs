using System;
using System.Collections.Generic;

namespace NextPlayerUWPDataLayer.Model
{
    public class ImportedPlaylist
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime DateModified { get; set; }
        public List<Song> SongPaths { get; set; }
        public int PlainPlaylistId { get; set; }

        public ImportedPlaylist()
        {
            SongPaths = new List<Song>();
        }

        public class Song
        {
            public string Path { get; set; }
            public string DisplayName { get; set; }
        }
    }
}
