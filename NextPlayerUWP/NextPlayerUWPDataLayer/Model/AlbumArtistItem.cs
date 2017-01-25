using NextPlayerUWPDataLayer.Tables;
using System;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;

namespace NextPlayerUWPDataLayer.Model
{
    public class AlbumArtistItem : MusicItem, INotifyPropertyChanged
    {
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
                    onPropertyChanged(this, nameof(AlbumArtist));
                }
            }
        }

        private string displayAlbumArtist;
        public string DisplayAlbumArtist
        {
            get
            {
                return displayAlbumArtist;
            }
            set
            {
                if (value != displayAlbumArtist)
                {
                    displayAlbumArtist = value;
                    onPropertyChanged(this, nameof(DisplayAlbumArtist));
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
                    onPropertyChanged(this, nameof(AlbumsNumber));
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
                    onPropertyChanged(this, nameof(SongsNumber));
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
                    onPropertyChanged(this, nameof(LastAdded));
                }
            }
        }

        private int albumArtistId;
        public int AlbumArtistId { get { return albumArtistId; } }

        public AlbumArtistItem()
        {
            albumArtist = "";
            displayAlbumArtist = "Unknown";
            albumArtistId = -1;
            duration = TimeSpan.Zero;
            songsNumber = 0;
            albumsNumber = 0;
            lastAdded = DateTime.MinValue;
        }

        public AlbumArtistItem(AlbumArtistsTable table)
        {
            albumsNumber = table.AlbumsNumber;
            songsNumber = table.SongsNumber;
            duration = table.Duration;
            albumArtistId = table.AlbumArtistId;
            albumArtist = table.AlbumArtist;
            if (albumArtist == "")
            {
                ResourceLoader loader = new ResourceLoader();
                displayAlbumArtist = loader.GetString("UnknownAlbumArtist");
            }
            else
            {
                displayAlbumArtist = albumArtist;
            }
            lastAdded = table.LastAdded;
        }

        public override string ToString()
        {
            return albumArtist;
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
            return MusicItemTypes.albumartist + separator + albumArtist;
        }
    }
}
