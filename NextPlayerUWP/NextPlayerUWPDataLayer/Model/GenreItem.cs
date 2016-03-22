using NextPlayerUWPDataLayer.Tables;
using System;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;

namespace NextPlayerUWPDataLayer.Model
{
    public class GenreItem : MusicItem, INotifyPropertyChanged
    {
        private string genreParam;
        public string GenreParam { get { return genreParam; } }
        private string genre;
        public string Genre { get { return genre; } }
        private int genreId;
        public int GenreId { get { return genreId; } }
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

        public GenreItem()
        {
            duration = TimeSpan.Zero;
            genre = "Unknown Genre";
            genreParam = "";
            genreId = 0;
            songsNumber = 0;
            lastAdded = DateTime.MinValue;
        }

        public GenreItem(GenresTable table)
        {
            duration = table.Duration;
            songsNumber = table.SongsNumber;
            genreParam = table.Genre;
            genreId = table.GenreId;
            if (genreParam == "")
            {
                ResourceLoader loader = new ResourceLoader();
                genre = loader.GetString("UnknownGenre");
            }
            else
            {
                genre = genreParam;
            }
            lastAdded = table.LastAdded;
        }

        public override string ToString()
        {
            return "genre|" + genre;
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
            return MusicItemTypes.genre + separator + genreParam;
        }
    }
}
