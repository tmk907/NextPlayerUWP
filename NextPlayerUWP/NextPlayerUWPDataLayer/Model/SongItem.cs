﻿using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Tables;
using System;
using System.ComponentModel;

namespace NextPlayerUWPDataLayer.Model
{
    public class SongItem : MusicItem, INotifyPropertyChanged
    {
        private string album;
        public string Album
        {
            get
            {
                return album;
            }
            set
            {
                if (value != album)
                {
                    album = value;
                    onPropertyChanged(this, "Album");
                }
            }
        }
        private string artist;
        public string Artist {
            get
            {
                return artist;
            }
            set
            {
                if (value != artist)
                {
                    artist = value;
                    onPropertyChanged(this, "Artist");
                }
            }
        }
        private TimeSpan duration;
        public TimeSpan Duration { get { return duration; } set { duration = value; } }
        private string path;
        public string Path { get { return path; } set { path = value; } }

        //private string contentPath;
        //public string ContentPath
        //{
        //    get
        //    {
        //        if (contentPath == null)
        //        {

        //            switch (sourceType)
        //            {
        //                case MusicSource.LocalFile:
        //                    contentPath = Path;
        //                    break;
        //                case MusicSource.LocalNotMusicLibrary:
        //                    contentPath = Path;//!!
        //                    break;
        //                case MusicSource.RadioJamendo:
        //                    contentPath = "";
        //                    break;
        //                case MusicSource.Dropbox:
        //                    CloudStorage.CloudStorageServiceFactory cssf = new CloudStorage.CloudStorageServiceFactory();
        //                    cssf.
        //            }
        //        }
        //    }

        private int rating;
        public int Rating
        {
            get
            {
                return rating;
            }
            set
            {
                if (value != rating)
                {
                    rating = value;
                    onPropertyChanged(this, "Rating");
                }
            }
        }
        private int songId;
        public int SongId { get { return songId; } set { songId = value; } }
        private string title;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                if (value != title)
                {
                    title = value;
                    onPropertyChanged(this, "Title");
                }
            }
        }
        private int disc;
        public int Disc
        {
            get
            {
                return disc;
            }
            set
            {
                if (value != disc)
                {
                    disc = value;
                    onPropertyChanged(this, "Disc");
                }
            }
        }
        private int trackNumber;
        public int TrackNumber
        {
            get
            {
                return trackNumber;
            }
            set
            {
                if (value != trackNumber)
                {
                    trackNumber = value;
                    onPropertyChanged(this, "TrackNumber");
                }
            }
        }
        private string composer;
        public string Composer
        {
            get
            {
                return composer;
            }
            set
            {
                if (value != composer)
                {
                    composer = value;
                    onPropertyChanged(this, "Composer");
                }
            }
        }
        private int year;
        public int Year
        {
            get
            {
                return year;
            }
            set
            {
                if (value != year)
                {
                    year = value;
                    onPropertyChanged(this, "Year");
                }
            }
        }
        private DateTime dateAdded;
        public DateTime DateAdded { get { return dateAdded; } }
        private DateTime lastPlayed;
        public DateTime LastPlayed
        {
            get { return lastPlayed; }
            set
            {
                if (value != lastPlayed)
                {
                    lastPlayed = value;
                    onPropertyChanged(this, "LastPlayed");
                }
            }
        }
        private int playCount;
        public int PlayCount
        {
            get { return playCount; }
            set
            {
                if(value != playCount)
                {
                    playCount = value;
                    onPropertyChanged(this, "PlayCount");
                }
            }
        }
        private string genres;
        public string Genres
        {
            get { return genres; }
            set
            {
                if (value != genres)
                {
                    genres = value;
                    onPropertyChanged(this, "Genres");
                }
            }
        }
        private string albumArtist;
        public string AlbumArtist
        {
            get { return albumArtist; }
            set
            {
                if (value != albumArtist)
                {
                    albumArtist = value;
                    onPropertyChanged(this, "AlbumArtist");
                }
            }
        }
        private bool isPlaying;
        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                if (value != isPlaying)
                {
                    isPlaying = value;
                    onPropertyChanged(this, "IsPlaying");
                }
            }
        }
        public string FirstArtist
        {
            get
            {
                return artist.Split(';')[0];
            }
        }

        private bool isAlbumArtSet;
        public bool IsAlbumArtSet
        {
            get { return isAlbumArtSet; }
            set
            {
                if (value != isAlbumArtSet)
                {
                    isAlbumArtSet = value;
                    onPropertyChanged(this, "IsAlbumArtSet");
                }
            }
        }

        private MusicSource sourceType;
        public MusicSource SourceType
        {
            get { return sourceType; }
            set { sourceType = value; }
        }

        private string coverPath;
        public string CoverPath
        {
            get { return coverPath; }
            set
            {
                coverPath = value;
                albumArtUri = null;
                onPropertyChanged(this, "AlbumArtUri");
            }
        }

        private Uri albumArtUri;
        public Uri AlbumArtUri
        {
            get
            {
                if (albumArtUri == null)
                {
                    if (isAlbumArtSet)
                    {
                        albumArtUri = new Uri((coverPath == AppConstants.AlbumCover) ? AppConstants.SongCoverBig : coverPath);
                    }
                    else
                    {
                        albumArtUri = new Uri(AppConstants.SongCoverBig);
                    }
                }
                return albumArtUri;
            }
        }

        public SongItem()
        {
            title = "Unknown Title";
            artist = "Unknown Artist";
            album = "Unknown Album";
            albumArtist = "Unknown Album Artist";
            duration = TimeSpan.Zero;
            path = "";
            rating = 0;
            songId = -1;
            composer = "";
            disc = 1;
            trackNumber = 0;
            year = 0;
            dateAdded = new DateTime();
            genres = "Unknown Genres";
            lastPlayed = new DateTime();
            playCount = 0;
            isPlaying = false;
            sourceType = MusicSource.LocalFile;
            coverPath = AppConstants.AlbumCover;
            isAlbumArtSet = true;
        }

        public SongItem(SongsTable table)
        {
            album = table.Album;
            albumArtist = table.AlbumArtist;
            artist = table.Artists;
            composer = table.Composers;
            duration = table.Duration;
            path = table.Path;
            rating = (int)table.Rating;
            title = table.Title;
            disc = (table.Disc == 0) ? 1 : table.Disc;
            trackNumber = table.Track;
            year = table.Year;
            songId = table.SongId;
            dateAdded = table.DateAdded;
            genres = table.Genres;
            lastPlayed = table.LastPlayed;
            playCount = (int)table.PlayCount;
            isPlaying = false;
            sourceType = (table.IsAvailable > 0) ? MusicSource.LocalFile : MusicSource.LocalNotMusicLibrary;
            if (String.IsNullOrEmpty(table.AlbumArt))
            {
                isAlbumArtSet = false;
                coverPath = AppConstants.AlbumCover;
            }
            else
            {
                isAlbumArtSet = true;
                coverPath = table.AlbumArt;
            }
        }

        public const int MaxId = 100000;
        public void GenerateID()
        {
            songId = path.GetHashCode();
            if (songId < MaxId) songId += MaxId;//approximate max songId in DB
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override string GetParameter()
        {
            return MusicItemTypes.song + separator + songId.ToString();
        }
    }
}
