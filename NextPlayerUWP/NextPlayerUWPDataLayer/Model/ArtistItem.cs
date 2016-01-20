using System;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;

namespace NextPlayerUWPDataLayer.Model
{
    public class ArtistItem : MusicItem, INotifyPropertyChanged
    {
        private string artistParam;
        public string ArtistParam { get { return artistParam; } }
        private string artist;
        public string Artist
        {
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
        public TimeSpan Duration { get { return duration; } }
        private int albumsNumber;
        public int AlbumsNumber
        {
            get
            {
                return albumsNumber;
            }
            set
            {
                if (value != albumsNumber)
                {
                    albumsNumber = value;
                    onPropertyChanged(this, "AlbumsNumber");
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

        public ArtistItem()
        {
            artistParam = "";
            artist = "Unknown Artist";
            duration = TimeSpan.Zero;
            songsNumber = 0;
            albumsNumber = 0;
        }

        public ArtistItem(int albumsnumber, string artistParam, TimeSpan duration, int songsnumber)
        {
            this.albumsNumber = albumsnumber;
            this.artistParam = artistParam;
            if (artistParam == "")
            {
                ResourceLoader loader = new ResourceLoader();
                this.artist = loader.GetString("UnknownArtist");
            }
            else
            {
                this.artist = artistParam;
            }
            this.duration = duration;
            this.songsNumber = songsnumber;
        }

        public override string ToString()
        {
            return artist;
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
            return "artist" + separator + artistParam;
        }
    }
}
