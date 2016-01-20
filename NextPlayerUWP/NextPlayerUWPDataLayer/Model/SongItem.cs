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
        public TimeSpan Duration { get { return duration; } }
        private string path;
        public string Path { get { return path; } }
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
        public int SongId { get { return songId; } }
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

        public SongItem()
        {
            title = "Unknown Title";
            artist = "Unknown Artist";
            album = "Unknown Album";
            duration = TimeSpan.Zero;
            this.path = "";
            rating = 0;
            songId = -1;
            composer = "";
            year = 0;
        }

        public SongItem(string album, string artist, string composer, TimeSpan duration, string path, int rating, int songid, string title, int trackumber, int year)
        {
            this.album = album;
            this.artist = artist;
            this.composer = composer;
            this.duration = duration;
            this.path = path;
            this.rating = rating;
            this.songId = songid;
            this.title = title;
            this.trackNumber = trackumber;
            this.year = year;
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
            return "song" + separator + songId.ToString();
        }
    }
}
