using NextPlayerUWPDataLayer.Constants;
using System;
using System.ComponentModel;

namespace NextPlayerUWPDataLayer.Model
{
    public class TrackStream: INotifyPropertyChanged
    {
        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                if (value != title)
                {
                    title = value;
                    onPropertyChanged(this, "Title");
                }
            }
        }

        private string artist;
        public string Artist
        {
            get { return artist; }
            set
            {
                if(value!=artist)
                {
                    artist = value;
                    onPropertyChanged(this, "Artist");
                }
            }
        }

        private string album;
        public string Album
        {
            get { return album; }
            set
            {
                if (value != album)
                {
                    album = value;
                    onPropertyChanged(this, "Album");
                }
            }
        }

        private string coverUri;
        public string CoverUri
        {
            get { return coverUri; }
            set
            {
                if (value != coverUri)
                {
                    coverUri = value;
                    onPropertyChanged(this, "CoverUri");
                }
            }
        }

        public string Url { get; set; }

        public int RemainingSeconds { get; set; }

        public TrackStream()
        {
            title = "";
            artist = "";
            album = "";
            coverUri = AppConstants.AlbumCover;
            Url = "";
            RemainingSeconds = 24 * 60 * 60;
        }

        public TrackStream(string title, string artist, string album, string coverUri,  string url, int seconds)
        {
            this.title = title;
            this.artist = artist;
            this.album = album;
            this.coverUri = coverUri;
            this.Url = url;
            this.RemainingSeconds = seconds;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
