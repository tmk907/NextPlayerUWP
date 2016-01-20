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

        public AlbumItem()
        {
            albumParam = "";
            album = "Unknown Album";
            artistParam = "Unknown Artist";
            albumArtist = "Unknown Album artist";
            songsNumber = 0;
            duration = TimeSpan.Zero;
        }

        public AlbumItem(string albumParam, string artistParam, string albumArtist, TimeSpan duration, int songsnumber)
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
            return "album" + separator + albumParam + separator + artistParam;
        }
    }
}
