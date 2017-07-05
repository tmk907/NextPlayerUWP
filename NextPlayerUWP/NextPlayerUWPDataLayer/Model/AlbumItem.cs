using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Tables;
using System;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;

namespace NextPlayerUWPDataLayer.Model
{
    public class AlbumItem : MusicItem, INotifyPropertyChanged
    {
        private string albumParam;
        public string AlbumParam { get { return albumParam; } set { albumParam = value; } }
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
        //private string artistParam;
        //public string ArtistParam { get { return artistParam; } }
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
        public TimeSpan Duration { get { return duration; } set { duration = value; } }
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
        private Uri imageUri;
        public Uri ImageUri
        {
            get { return imageUri; }
            set
            {
                if (value != imageUri)
                {
                    imageUri = value;
                    onPropertyChanged(this, "ImageUri");
                }
            }
        }
        private bool isImageSet;
        public bool IsImageSet
        {
            get { return isImageSet; }
            set
            {
                if (value != isImageSet)
                {
                    isImageSet = value;
                    onPropertyChanged(this, "IsImageSet");
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
        private int albumId;
        public int AlbumId { get { return albumId; } }
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
      
        public AlbumItem()
        {
            albumId = -1;
            albumParam = "";
            album = "Unknown Album";
            //artistParam = "Unknown Artist";
            albumArtist = "Unknown Album artist";
            songsNumber = 0;
            duration = TimeSpan.Zero;
            year = 2020;
            imagePath = AppConstants.AlbumCover;
            IsImageSet = true;
            imageUri = new Uri(imagePath);
            lastAdded = DateTime.MinValue;
            //SetAlbumArtColor();
        }

        public AlbumItem(AlbumsTable table)
        {
            albumId = table.AlbumId;
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
            //artistParam = null;
            year = table.Year;
            if (table.ImagePath == "")
            {
                IsImageSet = false;
                imagePath = AppConstants.AlbumCover;
            }
            else
            {
                IsImageSet = true;
                imagePath = table.ImagePath;
            }
            imageUri = new Uri(imagePath);
            lastAdded = table.LastAdded;
            //SetAlbumArtColor();
        }

        public override string ToString()
        {
            return "album|" + album + " "+ albumArtist;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(object sender, string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void SetParameter(string album ,string albumartist)
        {
            albumParam = album;
            albumArtist = albumartist;
        }

        public override string GetParameter()
        {
            return MusicItemTypes.album + separator + albumParam + separator + albumArtist;
        }

        //private double? albumArtColorSort = null;
        //public double AlbumArtColorSort
        //{
        //    get
        //    {
        //        if (albumArtColorSort == null)
        //        {
        //            if (albumArtColor == Int32.MaxValue)
        //            {
        //                albumArtColorSort = albumArtColor;
        //            }
        //            else
        //            {

        //                albumArtColorSort = Math.Sqrt(.241 * Images.PaletteUWP.ColorHelpers.Red(albumArtColor)
        //                    + .691 * Images.PaletteUWP.ColorHelpers.Green(albumArtColor)
        //                    + .068 * Images.PaletteUWP.ColorHelpers.Blue(albumArtColor));

        //                //float[] hsl = new float[3];
        //                //Images.PaletteUWP.ColorHelpers.ColorToHSL(albumArtColor, hsl);
        //                //albumArtColorSort = (int)(hsl[0] * 10000000);
        //            }
        //        }
        //        return albumArtColorSort ?? Int32.MaxValue;
        //    }
        //}


        //private int albumArtColor;
        //public string AlbumArtColor { get { return albumArtColor.ToString(); } }

        //private void SetAlbumArtColor()
        //{
        //    if (imagePath == AppConstants.AlbumCover || imagePath == "")
        //    {
        //        albumArtColor = Int32.MaxValue;
        //    }
        //    else
        //    {
        //        var parts = imagePath.ToString().Split('+');
        //        if (parts.Length == 2 && !String.IsNullOrWhiteSpace(parts[1]))
        //        {
        //            string i = parts[1].Replace(".jpg", "");
        //            int c = 0;
        //            if (Int32.TryParse(i, out c))
        //            {
        //                albumArtColor = c;
        //            }
        //            else
        //            {
        //                albumArtColor = Int32.MaxValue;
        //            }
        //        }
        //        else
        //        {
        //            albumArtColor = Int32.MaxValue;
        //        }
        //    }
        //}
    }
}
