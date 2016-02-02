﻿using NextPlayerUWPDataLayer.Tables;
using System;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;

namespace NextPlayerUWPDataLayer.Model
{
    public class AlbumItem : MusicItem, INotifyPropertyChanged
    {
        private string albumParam;
        public string AlbumParam { get { return albumParam; } }
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
        private string artistParam;
        public string ArtistParam { get { return artistParam; } }
        private string albumArtist;
        public string AlbumArtist
        {
            get
            {
                return albumArtist;
            }
            set
            {
                if (value != albumArtist)
                {
                    albumArtist = value;
                    onPropertyChanged(this, "AlbumArtist");
                }
            }
        }
        private int songsNumber;
        public int SongsNumber
        {
            get
            {
                return songsNumber;
            }
            set
            {
                if (value != songsNumber)
                {
                    songsNumber = value;
                    onPropertyChanged(this, "SongsNumber");
                }
            }
        }
        private TimeSpan duration;
        public TimeSpan Duration { get { return duration; } }
        private string imagePath;
        public string ImagePath
        {
            get { return imagePath; }
            set
            {
                if (value != imagePath)
                {
                    imagePath = value;
                    onPropertyChanged(this, "ImagePath");
                }
            }
        }
        private int year;
        public int Year
        {
            get { return year; }
            set
            {
                if (value != year)
                {
                    year = value;
                    onPropertyChanged(this, "Year");
                }
            }
        }

        public AlbumItem()
        {
            albumParam = "";
            album = "Unknown Album";
            artistParam = "Unknown Artist";
            albumArtist = "Unknown Album artist";
            songsNumber = 0;
            duration = TimeSpan.Zero;
            year = 2020;
            imagePath = "";
        }

        public AlbumItem(string albumParam, string artistParam, string albumArtist, TimeSpan duration, int songsnumber, int year, string imagePath)
        {
            this.albumParam = albumParam;
            this.artistParam = artistParam;
            if (albumParam == "")
            {
                ResourceLoader loader = new ResourceLoader();
                this.album = loader.GetString("UnknownAlbum");
            }
            else
            {
                this.album = albumParam;
            }
            if (albumArtist == "")
            {
                if (artistParam == null || artistParam == "")
                {
                    ResourceLoader loader = new ResourceLoader();
                    this.albumArtist = loader.GetString("UnknownArtist");
                }
                else
                {
                    this.albumArtist = artistParam;
                }
            }
            else
            {
                this.albumArtist = albumArtist;
            }
            this.duration = duration;
            this.songsNumber = songsnumber;
            this.year = year;
            this.imagePath = imagePath;
        }

        public AlbumItem(AlbumsTable table)
        {
            duration = table.Duration;
            songsNumber = table.SongsNumber;
            albumParam = table.Album;
            if (albumParam == "")
            {
                ResourceLoader loader = new ResourceLoader();
                album = loader.GetString("UnknownAlbum");
            }
            else
            {
                album = albumParam;
            }
            albumArtist = table.AlbumArtist;
            artistParam = null;
            year = table.Year;
            imagePath = table.ImagePath;
        }

        public override string ToString()
        {
            return "album|" + album;
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
            return MusicItemTypes.album + separator + albumParam + separator + artistParam;
        }
    }
}
