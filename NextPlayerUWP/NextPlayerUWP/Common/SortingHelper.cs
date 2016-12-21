﻿using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace NextPlayerUWP.Common
{
    public class ComboBoxItemValue
    {
        public string Option { get; set; }
        public string Label { get; set; }
        public ComboBoxItemValue(string option, string label)
        {
            Option = option;
            Label = label;
        }
    }

    public interface ISortingHelper<T>
    {
        Func<T, object> GetOrderBySelector();
        Func<T, object> GetGroupBySelector();
        string GetPropertyName();
        string GetFormat();
    }

    public class SortNames
    {
        public const string Title = "Title";
        public const string Artist = "Artist";
        public const string Album = "Album";
        public const string AlbumArtist = "AlbumArtist";
        public const string Duration = "Duration";
        public const string Year = "Year";
        public const string Genre = "Genre";
        public const string PlayCount = "PlayCount";
        public const string SongCount = "SongCount";
        public const string LastAdded = "LastAdded";
        public const string LastPlayed = "LastPlayed";
        public const string Rating = "Rating";
        public const string FolderName = "FolderName";
        public const string Directory = "Directory";
        public const string Name = "Name";
        public const string Composer = "Composer";
        public const string TrackNumber = "TrackNumber";
        public const string FileName = "FileName";
        public const string Default = "Default";
    }

    public abstract class BaseSortingHelper<T> : ISortingHelper<T>
    {
        private string collectionName;

        private string sortOption;
        protected string SortOption
        {
            get
            {
                if (sortOption == null)
                {
                    sortOption = ApplicationSettingsHelper.ReadSettingsValue(collectionName) as string;
                }
                return sortOption;
            }
            set
            {
                if (value != sortOption)
                {
                    sortOption = value;
                    ApplicationSettingsHelper.SaveSettingsValue(collectionName, value);
                }
            }
        }

        public ObservableCollection<ComboBoxItemValue> ComboBoxItemValues { get; }
        public ComboBoxItemValue SelectedSortOption
        {
            get
            {
                var a = ComboBoxItemValues.FirstOrDefault(s => s.Option.Equals(SortOption));
                return ComboBoxItemValues.FirstOrDefault(s => s.Option.Equals(SortOption));
            }
            set
            {
                if (value != null)
                {
                    SortOption = value.Option;
                }
            }
        }

        public BaseSortingHelper(string collectionName)
        {
            this.collectionName = collectionName;
            ComboBoxItemValues = GetComboBoxValues();
            if (SortOption == null) SortOption = ComboBoxItemValues.FirstOrDefault().Option;
        }

        protected abstract ObservableCollection<ComboBoxItemValue> GetComboBoxValues();

        public string GetFormat()
        {
            string format = "no";
            if (SortOption == SortNames.LastAdded || SortOption == SortNames.LastPlayed)
            {
                format = "date";
            }
            else if (SortOption == SortNames.Duration)
            {
                format = "duration";
            }
            return format;
        }

        public abstract Func<T, object> GetGroupBySelector();
        public abstract Func<T, object> GetOrderBySelector();
        public abstract string GetPropertyName();
    }

    public class SortingHelperForSongItems : BaseSortingHelper<SongItem>
    {
        public SortingHelperForSongItems(string collectionName) : base(collectionName)
        {
        }

        public override Func<SongItem, object> GetGroupBySelector()
        {
            switch (SortOption)
            {
                case SortNames.Title:
                    return t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower();
                case SortNames.Album:
                    return t => (t.Album == "") ? "" : t.Album[0].ToString().ToLower();
                case SortNames.Artist:
                    return t => (t.Artist == "") ? "" : t.Artist[0].ToString().ToLower();
                case SortNames.AlbumArtist:
                    return t => (t.AlbumArtist == "") ? "" : t.AlbumArtist[0].ToString().ToLower();
                case SortNames.Year:
                    return t => t.Year;
                case SortNames.Duration:
                    return t => new TimeSpan(t.Duration.Hours, t.Duration.Minutes, t.Duration.Seconds);
                case SortNames.Rating:
                    return t => t.Rating;
                case SortNames.Composer:
                    return t => (t.Composer == "") ? "" : t.Composer[0].ToString().ToLower();
                case SortNames.LastAdded:
                    return t => String.Format("{0:d}", t.DateAdded);
                case SortNames.LastPlayed:
                    return t => String.Format("{0:d}", t.LastPlayed);
                case SortNames.PlayCount:
                    return t => t.PlayCount;
                case SortNames.TrackNumber:
                    return t => t.TrackNumber;
                case SortNames.FileName:
                    return t => t.FileName[0].ToString().ToLower();
                default:
                    return t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower();
            }
        }

        public override Func<SongItem, object> GetOrderBySelector()
        {
            switch (SortOption)
            {
                case SortNames.Title:
                    return s => s.Title;
                case SortNames.Album:
                    return s => s.Album;
                case SortNames.Artist:
                    return s => s.Artist;
                case SortNames.AlbumArtist:
                    return s => s.AlbumArtist;
                case SortNames.Year:
                    return s => s.Year;
                case SortNames.Duration:
                    return s => s.Duration.TotalSeconds;
                case SortNames.Rating:
                    return s => s.Rating;
                case SortNames.Composer:
                    return s => s.Composer;
                case SortNames.LastAdded:
                    return s => s.DateAdded.Ticks;
                case SortNames.LastPlayed:
                    return s => s.LastPlayed.Ticks;
                case SortNames.PlayCount:
                    return s => s.PlayCount;
                case SortNames.TrackNumber:
                    return s => s.TrackNumber;
                case SortNames.FileName:
                    return s => s.FileName;
                default:
                    return s => s.Title;
            }
        }

        public override string GetPropertyName()
        {
            switch (SortOption)
            {
                case SortNames.Title:
                    return "Title";
                case SortNames.Album:
                    return "Album";
                case SortNames.Artist:
                    return "Artist";
                case SortNames.AlbumArtist:
                    return "AlbumArtist";
                case SortNames.Year:
                    return "Year";
                case SortNames.Duration:
                    return "Duration";
                case SortNames.Rating:
                    return "Rating";
                case SortNames.Composer:
                    return "Composer";
                case SortNames.LastAdded:
                    return "DateAdded";
                case SortNames.LastPlayed:
                    return "LastPlayed";
                case SortNames.PlayCount:
                    return "PlayCount";
                case SortNames.TrackNumber:
                    return "TrackNumber";
                case SortNames.FileName:
                    return "FileName";
                default:
                    return "Title";
            }
        }

        protected override ObservableCollection<ComboBoxItemValue> GetComboBoxValues()
        {
            ResourceLoader loader = new ResourceLoader();
            ObservableCollection<ComboBoxItemValue> comboboxItems = new ObservableCollection<ComboBoxItemValue>();
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Title, loader.GetString(SortNames.Title)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Album, loader.GetString(SortNames.Album)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Artist, loader.GetString(SortNames.Artist)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.AlbumArtist, loader.GetString(SortNames.AlbumArtist)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Rating, loader.GetString(SortNames.Rating)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Year, loader.GetString(SortNames.Year)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.TrackNumber, loader.GetString(SortNames.TrackNumber)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Duration, loader.GetString(SortNames.Duration)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Composer, loader.GetString(SortNames.Composer)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.LastAdded, loader.GetString(SortNames.LastAdded)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.LastPlayed, loader.GetString(SortNames.LastPlayed)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.PlayCount, loader.GetString(SortNames.PlayCount)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.FileName, loader.GetString(SortNames.FileName)));
            return comboboxItems;
        }
    }

    public class SortingHelperForSongItemsInPlaylist : BaseSortingHelper<SongItem>
    {
        public SortingHelperForSongItemsInPlaylist(string collectionName) : base(collectionName)
        {
        }

        public override Func<SongItem, object> GetGroupBySelector()
        {
            switch (SortOption)
            {
                case SortNames.Default:
                    return t => t.Index;
                case SortNames.Title:
                    return t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower();
                case SortNames.Album:
                    return t => (t.Album == "") ? "" : t.Album[0].ToString().ToLower();
                case SortNames.Artist:
                    return t => (t.Artist == "") ? "" : t.Artist[0].ToString().ToLower();
                case SortNames.AlbumArtist:
                    return t => (t.AlbumArtist == "") ? "" : t.AlbumArtist[0].ToString().ToLower();
                case SortNames.Year:
                    return t => t.Year;
                case SortNames.Duration:
                    return t => new TimeSpan(t.Duration.Hours, t.Duration.Minutes, t.Duration.Seconds);
                case SortNames.Rating:
                    return t => t.Rating;
                case SortNames.Composer:
                    return t => (t.Composer == "") ? "" : t.Composer[0].ToString().ToLower();
                case SortNames.LastAdded:
                    return t => String.Format("{0:d}", t.DateAdded);
                case SortNames.LastPlayed:
                    return t => String.Format("{0:d}", t.LastPlayed);
                case SortNames.PlayCount:
                    return t => t.PlayCount;
                case SortNames.TrackNumber:
                    return t => t.TrackNumber;
                case SortNames.FileName:
                    return t => t.FileName[0].ToString().ToLower();
                default:
                    return t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower();
            }
        }

        public override Func<SongItem, object> GetOrderBySelector()
        {
            switch (SortOption)
            {
                case SortNames.Default:
                    return s => s.Index;
                case SortNames.Title:
                    return s => s.Title;
                case SortNames.Album:
                    return s => s.Album;
                case SortNames.Artist:
                    return s => s.Artist;
                case SortNames.AlbumArtist:
                    return s => s.AlbumArtist;
                case SortNames.Year:
                    return s => s.Year;
                case SortNames.Duration:
                    return s => s.Duration.TotalSeconds;
                case SortNames.Rating:
                    return s => s.Rating;
                case SortNames.Composer:
                    return s => s.Composer;
                case SortNames.LastAdded:
                    return s => s.DateAdded.Ticks;
                case SortNames.LastPlayed:
                    return s => s.LastPlayed.Ticks;
                case SortNames.PlayCount:
                    return s => s.PlayCount;
                case SortNames.TrackNumber:
                    return s => s.TrackNumber;
                case SortNames.FileName:
                    return s => s.FileName;
                default:
                    return s => s.Title;
            }
        }

        public override string GetPropertyName()
        {
            switch (SortOption)
            {
                case SortNames.Default:
                    return "Default";
                case SortNames.Title:
                    return "Title";
                case SortNames.Album:
                    return "Album";
                case SortNames.Artist:
                    return "Artist";
                case SortNames.AlbumArtist:
                    return "AlbumArtist";
                case SortNames.Year:
                    return "Year";
                case SortNames.Duration:
                    return "Duration";
                case SortNames.Rating:
                    return "Rating";
                case SortNames.Composer:
                    return "Composer";
                case SortNames.LastAdded:
                    return "DateAdded";
                case SortNames.LastPlayed:
                    return "LastPlayed";
                case SortNames.PlayCount:
                    return "PlayCount";
                case SortNames.TrackNumber:
                    return "TrackNumber";
                case SortNames.FileName:
                    return "FileName";
                default:
                    return "Title";
            }
        }

        protected override ObservableCollection<ComboBoxItemValue> GetComboBoxValues()
        {
            ResourceLoader loader = new ResourceLoader();
            ObservableCollection<ComboBoxItemValue> comboboxItems = new ObservableCollection<ComboBoxItemValue>();
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Default, loader.GetString(SortNames.Default)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Title, loader.GetString(SortNames.Title)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Album, loader.GetString(SortNames.Album)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Artist, loader.GetString(SortNames.Artist)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.AlbumArtist, loader.GetString(SortNames.AlbumArtist)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Rating, loader.GetString(SortNames.Rating)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Year, loader.GetString(SortNames.Year)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.TrackNumber, loader.GetString(SortNames.TrackNumber)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Duration, loader.GetString(SortNames.Duration)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Composer, loader.GetString(SortNames.Composer)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.LastAdded, loader.GetString(SortNames.LastAdded)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.LastPlayed, loader.GetString(SortNames.LastPlayed)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.PlayCount, loader.GetString(SortNames.PlayCount)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.FileName, loader.GetString(SortNames.FileName)));
            return comboboxItems;
        }
    }

    public class SortingHelperForAlbumItems : BaseSortingHelper<AlbumItem>
    {
        public SortingHelperForAlbumItems(string collectionName) : base(collectionName)
        {
        }

        public override Func<AlbumItem, object> GetGroupBySelector()
        {
            switch (SortOption)
            {
                case SortNames.Album:
                    return t => (t.Album == "") ? "" : t.Album[0].ToString().ToLower();
                case SortNames.AlbumArtist:
                    return t => (t.AlbumArtist == "") ? "" : t.AlbumArtist[0].ToString().ToLower();
                case SortNames.Year:
                    return t => t.Year;
                case SortNames.Duration:
                    return t => new TimeSpan(t.Duration.Hours, t.Duration.Minutes, t.Duration.Seconds);
                case SortNames.SongCount:
                    return t => t.SongsNumber;
                case SortNames.LastAdded:
                    return t => String.Format("{0:d}", t.LastAdded);
                default:
                    return t => (t.Album == "") ? "" : t.Album[0].ToString().ToLower();
            }
        }

        public override Func<AlbumItem, object> GetOrderBySelector()
        {
            switch (SortOption)
            {
                case SortNames.Album:
                    return s => s.Album;
                case SortNames.AlbumArtist:
                    return s => s.AlbumArtist;
                case SortNames.Year:
                    return s => s.Year;
                case SortNames.Duration:
                    return s => s.Duration.TotalSeconds;
                case SortNames.SongCount:
                    return s => s.SongsNumber;
                case SortNames.LastAdded:
                    return s => s.LastAdded.Ticks;
                default:
                    return s => s.Album;
            }
        }

        public override string GetPropertyName()
        {
            switch (SortOption)
            {
                case SortNames.Album:
                    return "Album";
                case SortNames.AlbumArtist:
                    return "AlbumArtist";
                case SortNames.Year:
                    return "Year";
                case SortNames.Duration:
                    return "Duration";
                case SortNames.SongCount:
                    return "SongsNumber";
                case SortNames.LastAdded:
                    return "LastAdded";
                default:
                    return "Album";
            }
        }

        protected override ObservableCollection<ComboBoxItemValue> GetComboBoxValues()
        {
            ResourceLoader loader = new ResourceLoader();
            ObservableCollection<ComboBoxItemValue> comboboxItems = new ObservableCollection<ComboBoxItemValue>();
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Album, loader.GetString(SortNames.Album)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.AlbumArtist, loader.GetString(SortNames.AlbumArtist)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Year, loader.GetString(SortNames.Year)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Duration, loader.GetString(SortNames.Duration)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.SongCount, loader.GetString(SortNames.SongCount)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.LastAdded, loader.GetString(SortNames.LastAdded)));
            return comboboxItems;
        }
    }

    public class SortingHelperForArtistItems : BaseSortingHelper<ArtistItem>
    {
        public SortingHelperForArtistItems(string collectionName) : base(collectionName)
        {
        }

        public override Func<ArtistItem, object> GetGroupBySelector()
        {
            switch (SortOption)
            {
                case SortNames.Artist:
                    return t => (t.Artist == "") ? "" : t.Artist[0].ToString().ToLower();
                case SortNames.Duration:
                    return t => new TimeSpan(t.Duration.Hours, t.Duration.Minutes, t.Duration.Seconds);
                case SortNames.SongCount:
                    return t => t.SongsNumber;
                case SortNames.LastAdded:
                    return t => String.Format("{0:d}", t.LastAdded);
                default:
                    return t => (t.Artist == "") ? "" : t.Artist[0].ToString().ToLower();
            }
        }

        public override Func<ArtistItem, object> GetOrderBySelector()
        {
            switch (SortOption)
            {
                case SortNames.Artist:
                    return s => s.Artist;
                case SortNames.Duration:
                    return s => s.Duration.TotalSeconds;
                case SortNames.SongCount:
                    return s => s.SongsNumber;
                case SortNames.LastAdded:
                    return s => s.LastAdded.Ticks;
                default:
                    return s => s.Artist;
            }
        }

        public override string GetPropertyName()
        {
            switch (SortOption)
            {
                case SortNames.Artist:
                    return "Artist";
                case SortNames.Duration:
                    return "Duration";
                case SortNames.SongCount:
                    return "SongsNumber";
                case SortNames.LastAdded:
                    return "LastAdded";
                default:
                    return "Artist";
            }
        }

        protected override ObservableCollection<ComboBoxItemValue> GetComboBoxValues()
        {
            ResourceLoader loader = new ResourceLoader();
            ObservableCollection<ComboBoxItemValue> comboboxItems = new ObservableCollection<ComboBoxItemValue>();
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Artist, loader.GetString(SortNames.Artist)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Duration, loader.GetString(SortNames.Duration)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.SongCount, loader.GetString(SortNames.SongCount)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.LastAdded, loader.GetString(SortNames.LastAdded)));
            return comboboxItems;
        }
    }

    public class SortingHelperForGenreItems : BaseSortingHelper<GenreItem>
    {
        public SortingHelperForGenreItems(string collectionName) : base(collectionName)
        {
        }

        public override Func<GenreItem, object> GetGroupBySelector()
        {
            switch (SortOption)
            {
                case SortNames.Genre:
                    return t => (t.Genre == "") ? "" : t.Genre[0].ToString().ToLower();
                case SortNames.Duration:
                    return t => new TimeSpan(t.Duration.Hours, t.Duration.Minutes, t.Duration.Seconds);
                case SortNames.SongCount:
                    return t => t.SongsNumber;
                case SortNames.LastAdded:
                    return t => String.Format("{0:d}", t.LastAdded);
                default:
                    return t => (t.Genre == "") ? "" : t.Genre[0].ToString().ToLower();
            }
        }

        public override Func<GenreItem, object> GetOrderBySelector()
        {
            switch (SortOption)
            {
                case SortNames.Genre:
                    return s => s.Genre;
                case SortNames.Duration:
                    return s => s.Duration.TotalSeconds;
                case SortNames.SongCount:
                    return s => s.SongsNumber;
                case SortNames.LastAdded:
                    return s => s.LastAdded.Ticks * -1;
                default:
                    return s => s.Genre;
            }
        }

        public override string GetPropertyName()
        {
            switch (SortOption)
            {
                case SortNames.Genre:
                    return "Genre";
                case SortNames.Duration:
                    return "Duration";
                case SortNames.SongCount:
                    return "SongsNumber";
                case SortNames.LastAdded:
                    return "LastAdded";
                default:
                    return "Genre";
            }
        }

        protected override ObservableCollection<ComboBoxItemValue> GetComboBoxValues()
        {
            ResourceLoader loader = new ResourceLoader();
            ObservableCollection<ComboBoxItemValue> comboboxItems = new ObservableCollection<ComboBoxItemValue>();
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Genre, loader.GetString(SortNames.Genre)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Duration, loader.GetString(SortNames.Duration)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.SongCount, loader.GetString(SortNames.SongCount)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.LastAdded, loader.GetString(SortNames.LastAdded)));
            return comboboxItems;
        }
    }

    public class SortingHelperForFolderItems : BaseSortingHelper<FolderItem>
    {
        public SortingHelperForFolderItems(string collectionName) : base(collectionName)
        {
        }

        public override Func<FolderItem, object> GetGroupBySelector()
        {
            throw new NotImplementedException();
        }

        public override Func<FolderItem, object> GetOrderBySelector()
        {
            throw new NotImplementedException();
        }

        public override string GetPropertyName()
        {
            throw new NotImplementedException();
        }

        protected override ObservableCollection<ComboBoxItemValue> GetComboBoxValues()
        {
            ResourceLoader loader = new ResourceLoader();
            ObservableCollection<ComboBoxItemValue> comboboxItems = new ObservableCollection<ComboBoxItemValue>();
            comboboxItems.Add(new ComboBoxItemValue(SortNames.Directory, loader.GetString(SortNames.Directory)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.FolderName, loader.GetString(SortNames.FolderName)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.SongCount, loader.GetString(SortNames.SongCount)));
            //comboboxItems.Add(new ComboBoxItemValue(Duration, loader.GetString(Duration)));
            comboboxItems.Add(new ComboBoxItemValue(SortNames.LastAdded, loader.GetString(SortNames.LastAdded)));
            return comboboxItems;
        }
    }
}