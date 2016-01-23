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

        public GenreItem()
        {
            this.duration = TimeSpan.Zero;
            this.genre = "Unknown Genre";
            this.genreParam = "";
            this.songsNumber = 0;
        }

        public GenreItem(TimeSpan duration, string genreParam, int songsnumber)
        {
            this.duration = duration;
            this.genreParam = genreParam;
            if (genreParam == "")
            {
                ResourceLoader loader = new ResourceLoader();
                this.genre = loader.GetString("UnknownGenre");
            }
            else
            {
                this.genre = genreParam;
            }
            this.songsNumber = songsnumber;
        }

        public GenreItem(GenresTable item)
        {
            duration = item.Duration;
            songsNumber = item.SongsSumber;
            genre = item.Genre;
            if (genre == "")
            {
                ResourceLoader loader = new ResourceLoader();
                genreParam = loader.GetString("UnknownGenre");
            }
            else
            {
                genreParam = genre;
            }
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
            return MusicItemTypes.genre + separator + genre;
        }
    }
}
