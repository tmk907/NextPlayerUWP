using System;

namespace NextPlayerUWPDataLayer.Model
{
    public class Tags
    {
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public string Artists { get; set; } //Performers in taglib
        public string Comment { get; set; }
        public string Composers { get; set; }
        public string Conductor { get; set; }
        public int Disc { get; set; }
        public int DiscCount { get; set; }
        public string FirstArtist { get; set; } //FirstPerformer
        public string FirstComposer { get; set; }
        public string Genres { get; set; }
        public string Lyrics { get; set; }
        public uint Rating { get; set; }
        public string Title { get; set; }
        public int Track { get; set; }
        public int TrackCount { get; set; }
        public int Year { get; set; }
    }
}
