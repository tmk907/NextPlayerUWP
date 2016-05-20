﻿using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Enums;
using System;
using System.ComponentModel;

namespace NextPlayerUWPDataLayer.Model
{
    public class RadioItem: MusicItem, INotifyPropertyChanged
    {
        private int broadcastId;
        public int BroadcastId { get { return broadcastId; } }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    onPropertyChanged(this, "Name");
                }
            }
        }

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

        private string playingNowTitle;
        public string PlayingNowTitle
        {
            get { return playingNowTitle; }
            set
            {
                if (value != playingNowTitle)
                {
                    playingNowTitle = value;
                    onPropertyChanged(this, "PlayingNowTitle");
                    onPropertyChanged(this, "PlayingNowArtistTitle");
                }
            }
        }

        private string playingNowArtist;
        public string PlayingNowArtist
        {
            get { return playingNowArtist; }
            set
            {
                if (value != playingNowArtist)
                {
                    playingNowArtist = value;
                    onPropertyChanged(this, "PlayingNowArtist");
                    onPropertyChanged(this, "PlayingNowArtistTitle");
                }
            }
        }

        private string playingNowAlbum;
        public string PlayingNowAlbum
        {
            get { return playingNowAlbum; }
            set
            {
                if (value != playingNowAlbum)
                {
                    playingNowAlbum = value;
                    onPropertyChanged(this, "PlayingNowAlbum");
                }
            }
        }

        private string playingNowImagePath;
        public string PlayingNowImagePath
        {
            get { return playingNowImagePath; }
            set
            {
                if (value != playingNowImagePath)
                {
                    playingNowImagePath = value;
                    onPropertyChanged(this, "PlayingNowImagePath");                  
                }
            }
        }

        public string PlayingNowArtistTitle { get { return playingNowArtist + " - " + playingNowTitle; } }

        private RadioType type;
        public RadioType Type { get { return type; } }

        private string streamUrl;
        public string StreamUrl {
            get { return streamUrl; }
            set { streamUrl = value; }
        }

        private int remainingTime; //miliseconds
        public int RemainingTime
        {
            get { return remainingTime; }
            set { remainingTime = value; }
        }

        private DateTime streamUpdatedAt; //miliseconds
        public DateTime StreamUpdatedAt
        {
            get { return streamUpdatedAt; }
            set { streamUpdatedAt = value; }
        }

        public RadioItem()
        {
            broadcastId = 0;//?
            type = RadioType.Unknown;
            streamUrl = "";
            name = "Unknown radio";
            imagePath = AppConstants.RadioCover;
            playingNowAlbum = "Album";
            playingNowArtist = "Artist";
            playingNowTitle = "Title";
            playingNowImagePath = "";
            remainingTime = 1000 * 60 * 60 * 24;
            streamUpdatedAt = DateTime.Now;
        }

        public RadioItem(int id, RadioType type, string stream)
        {
            this.broadcastId = id;
            this.type = type;
            this.streamUrl = stream;

            name = "Unknown radio";
            imagePath = AppConstants.RadioCover;
            playingNowAlbum = "";
            playingNowArtist = "";
            playingNowTitle = "";
            playingNowImagePath = AppConstants.RadioCover;
            remainingTime = 1000 * 60 * 60 * 24;
            streamUpdatedAt = DateTime.Now;
        }



        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public SongItem ToSongItem()
        {
            SongItem song = new SongItem();
            song.Title = name;
            song.Artist = PlayingNowArtistTitle;
            song.Album = playingNowAlbum;
            //song.Year = DateTime.Now.Year;
            song.SongId = broadcastId;
            song.Path = streamUrl;
            song.SourceType = MusicSource.RadioJamendo;
            song.CoverPath = playingNowImagePath;
            song.Duration = TimeSpan.Zero;
            return song;
        }

        public override string GetParameter()
        {
            return MusicItemTypes.radio + separator + streamUrl;
        }
    }
}
