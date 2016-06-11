using NextPlayerUWPDataLayer.Tables;
using System;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;

namespace NextPlayerUWPDataLayer.Model
{
    public class ArtistItem : MusicItem, INotifyPropertyChanged
    {
        private string artistParam;
        public string ArtistParam { get { return artistParam; } set { artistParam = value; } }
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
        private int artistId;
        public int ArtistId { get { return artistId; } }
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
        private DateTime lastAdded;
        public DateTime LastAdded
        {
            get { return lastAdded; }
            set
            {
                if (value != lastAdded)
                {
                    lastAdded = value;
                    onPropertyChanged(this, "LastAdded");
                }
            }
        }

        public ArtistItem()
        {
            artistParam = "";
            artist = "Unknown Artist";
            artistId = 0;
            duration = TimeSpan.Zero;
            songsNumber = 0;
            albumsNumber = 0;
            lastAdded = DateTime.MinValue;
        }

        public ArtistItem(ArtistsTable table)
        {
            albumsNumber = table.AlbumsNumber;
            songsNumber = table.SongsNumber;
            duration = table.Duration;
            artistParam = table.Artist;
            artistId = table.ArtistId;
            if (artistParam == "")
            {
                ResourceLoader loader = new ResourceLoader();
                artist = loader.GetString("UnknownArtist");
            }
            else
            {
                artist = artistParam;
            }
            lastAdded = table.LastAdded;
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

        public void SetParameter(string param)
        {
            artistParam = param;
        }

        public override string GetParameter()
        {
            return MusicItemTypes.artist + separator + artistParam;
        }
    }
}
