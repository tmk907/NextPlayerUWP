﻿using NextPlayerUWPDataLayer.Enums;

namespace NextPlayerUWPDataLayer.Model
{
    public class NowPlayingSong
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Path { get; set; }
        public string ImagePath { get; set; }
        public int SongId { get; set; }
        public int Position { get; set; }
        public MusicSource SourceType { get; set; }
    }
}
