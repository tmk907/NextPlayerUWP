using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("SongsTable")]
    class SongsTable
    {
        [PrimaryKey, AutoIncrement]
        public int SongId { get; set; }
        public string Filename { get; set; }
        public long FileSize { get; set; }
        [Indexed]
        public string FolderName { get; set; }
        public string Path { get; set; }
        public string DirectoryName { get; set; }

        public uint Bitrate { get; set; }
        public TimeSpan Duration { get; set; }

        public DateTime LastPlayed { get; set; }
        public DateTime DateAdded { get; set; }
        public uint PlayCount { get; set; }
        [Indexed]
        public int IsAvailable { get; set; }

        //Tags
        [Indexed]
        public string Album { get; set; }
        [Indexed]
        public string AlbumArtist { get; set; }
        [Indexed]
        public string Artists { get; set; } //Performers in taglib
        public string Comment { get; set; }
        public string Composers { get; set; }
        public string Conductor { get; set; }
        public int Disc { get; set; }
        public int DiscCount { get; set; }
        public string FirstArtist { get; set; } //FirstPerformer
        public string FirstComposer { get; set; }
        public string Genre { get; set; }
        public string Lyrics { get; set; }
        public uint Rating { get; set; }
        [Indexed]
        public string Title { get; set; }
        public int Track { get; set; }
        public int TrackCount { get; set; }
        public int Year { get; set; }
    }
}
