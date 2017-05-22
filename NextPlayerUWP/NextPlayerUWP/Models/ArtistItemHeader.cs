using NextPlayerUWPDataLayer.Model;
using System;

namespace NextPlayerUWP.Models
{
    public class ArtistItemHeader
    {
        public int AlbumId { get; set; }
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public int Year { get; set; }
        public Uri ImageUri { get; set; }
    }

    public class ArtistGroupList : GroupList
    {
        new public ArtistItemHeader Header { get; set; }
    }
}
