using NextPlayerUWPDataLayer.Model;
using System.Collections.ObjectModel;
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

        private MusicItemTypes type;
        public MusicItemTypes Type { get { return type; } }

        public SortNames(MusicItemTypes itemType)
        {
            type = itemType;
        }

        public ObservableCollection<ComboBoxItemValue> GetSortNames()
        {
            ObservableCollection<ComboBoxItemValue> comboboxItems = new ObservableCollection<ComboBoxItemValue>();
            ResourceLoader loader = new ResourceLoader();
            switch (type)
            {
                case MusicItemTypes.album:
                    comboboxItems.Add(new ComboBoxItemValue(Album, loader.GetString(Album)));
                    comboboxItems.Add(new ComboBoxItemValue(AlbumArtist, loader.GetString(AlbumArtist)));
                    comboboxItems.Add(new ComboBoxItemValue(Year, loader.GetString(Year)));
                    comboboxItems.Add(new ComboBoxItemValue(Duration, loader.GetString(Duration)));
                    comboboxItems.Add(new ComboBoxItemValue(SongCount, loader.GetString(SongCount)));
                    comboboxItems.Add(new ComboBoxItemValue(LastAdded, loader.GetString(LastAdded)));
                    break;
                case MusicItemTypes.artist:
                    comboboxItems.Add(new ComboBoxItemValue(Artist, loader.GetString(Artist)));
                    comboboxItems.Add(new ComboBoxItemValue(Duration, loader.GetString(Duration)));
                    comboboxItems.Add(new ComboBoxItemValue(SongCount, loader.GetString(SongCount)));
                    comboboxItems.Add(new ComboBoxItemValue(LastAdded, loader.GetString(LastAdded)));
                    break;
                case MusicItemTypes.folder:
                    comboboxItems.Add(new ComboBoxItemValue(Directory, loader.GetString(Directory)));
                    comboboxItems.Add(new ComboBoxItemValue(FolderName, loader.GetString(FolderName)));
                    comboboxItems.Add(new ComboBoxItemValue(SongCount, loader.GetString(SongCount)));
                    //comboboxItems.Add(new ComboBoxItemValue(Duration, loader.GetString(Duration)));
                    comboboxItems.Add(new ComboBoxItemValue(LastAdded, loader.GetString(LastAdded)));
                    break;
                case MusicItemTypes.genre:
                    comboboxItems.Add(new ComboBoxItemValue(Genre, loader.GetString(Genre)));
                    comboboxItems.Add(new ComboBoxItemValue(Duration, loader.GetString(Duration)));
                    comboboxItems.Add(new ComboBoxItemValue(SongCount, loader.GetString(SongCount)));
                    comboboxItems.Add(new ComboBoxItemValue(LastAdded, loader.GetString(LastAdded)));
                    break;
                case MusicItemTypes.song:
                    comboboxItems.Add(new ComboBoxItemValue(Title, loader.GetString(Title)));
                    comboboxItems.Add(new ComboBoxItemValue(Album, loader.GetString(Album)));
                    comboboxItems.Add(new ComboBoxItemValue(Artist, loader.GetString(Artist)));
                    comboboxItems.Add(new ComboBoxItemValue(AlbumArtist, loader.GetString(AlbumArtist)));
                    comboboxItems.Add(new ComboBoxItemValue(Rating, loader.GetString(Rating)));
                    comboboxItems.Add(new ComboBoxItemValue(Year, loader.GetString(Year)));
                    comboboxItems.Add(new ComboBoxItemValue(Duration, loader.GetString(Duration)));
                    comboboxItems.Add(new ComboBoxItemValue(Composer, loader.GetString(Composer)));
                    comboboxItems.Add(new ComboBoxItemValue(TrackNumber, loader.GetString(TrackNumber)));
                    comboboxItems.Add(new ComboBoxItemValue(LastAdded, loader.GetString(LastAdded)));
                    comboboxItems.Add(new ComboBoxItemValue(LastPlayed, loader.GetString(LastPlayed)));
                    comboboxItems.Add(new ComboBoxItemValue(PlayCount, loader.GetString(PlayCount)));
                    break;
            }
            return comboboxItems;
        }
    }
}
