using NextPlayerUWPDataLayer.Model;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;

namespace NextPlayerUWP.Common
{
    public class ComboBoxItemValues
    {
        public string Option { get; set; }
        public string Label { get; set; }
        public ComboBoxItemValues(string option, string label)
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
        public const string AlbumArtist = "Album Artist";
        public const string Duration = "Duration";
        public const string Year = "Year";
        public const string Genre = "Genre";
        public const string PlayCount = "Play Count";
        public const string SongCount = "Song Count";
        public const string LastAdded = "Last Added";
        public const string Rating = "Rating";
        public const string FolderName = "Folder Name";
        public const string Name = "Name";

        private MusicItemTypes type;
        public MusicItemTypes Type { get { return type; } }

        public SortNames(MusicItem item)
        {
            type = MusicItem.ParseType(item.GetParameter());
        }

        public ObservableCollection<ComboBoxItemValues> GetSortNames()
        {
            ObservableCollection<ComboBoxItemValues> comboboxItems = new ObservableCollection<ComboBoxItemValues>();
            ResourceLoader loader = new ResourceLoader();
            switch (type)
            {
                case MusicItemTypes.album:
                    comboboxItems.Add(new ComboBoxItemValues(Album, loader.GetString(Album)));
                    comboboxItems.Add(new ComboBoxItemValues(Artist, loader.GetString(Artist)));
                    comboboxItems.Add(new ComboBoxItemValues(AlbumArtist, loader.GetString(AlbumArtist)));
                    comboboxItems.Add(new ComboBoxItemValues(Year, loader.GetString(Year)));
                    comboboxItems.Add(new ComboBoxItemValues(SongCount, loader.GetString(SongCount)));
                    comboboxItems.Add(new ComboBoxItemValues(Duration, loader.GetString(Duration)));
                    comboboxItems.Add(new ComboBoxItemValues(LastAdded, loader.GetString(LastAdded)));
                    break;
                case MusicItemTypes.artist:
                    comboboxItems.Add(new ComboBoxItemValues(Artist, loader.GetString(Artist)));
                    comboboxItems.Add(new ComboBoxItemValues(SongCount, loader.GetString(SongCount)));
                    comboboxItems.Add(new ComboBoxItemValues(Duration, loader.GetString(Duration)));
                    comboboxItems.Add(new ComboBoxItemValues(LastAdded, loader.GetString(LastAdded)));
                    break;
                case MusicItemTypes.folder:
                    comboboxItems.Add(new ComboBoxItemValues(FolderName, loader.GetString(FolderName)));
                    comboboxItems.Add(new ComboBoxItemValues(SongCount, loader.GetString(SongCount)));
                    comboboxItems.Add(new ComboBoxItemValues(Duration, loader.GetString(Duration)));
                    comboboxItems.Add(new ComboBoxItemValues(LastAdded, loader.GetString(LastAdded)));
                    break;
                case MusicItemTypes.genre:
                    comboboxItems.Add(new ComboBoxItemValues(Genre, loader.GetString(Genre)));
                    comboboxItems.Add(new ComboBoxItemValues(SongCount, loader.GetString(SongCount)));
                    comboboxItems.Add(new ComboBoxItemValues(Duration, loader.GetString(Duration)));
                    comboboxItems.Add(new ComboBoxItemValues(LastAdded, loader.GetString(LastAdded)));
                    break;
                case MusicItemTypes.song:
                    comboboxItems.Add(new ComboBoxItemValues(Title, loader.GetString(Title)));
                    comboboxItems.Add(new ComboBoxItemValues(Album, loader.GetString(Album)));
                    comboboxItems.Add(new ComboBoxItemValues(Artist, loader.GetString(Artist)));
                    comboboxItems.Add(new ComboBoxItemValues(AlbumArtist, loader.GetString(AlbumArtist)));
                    comboboxItems.Add(new ComboBoxItemValues(Year, loader.GetString(Year)));
                    comboboxItems.Add(new ComboBoxItemValues(Duration, loader.GetString(Duration)));
                    comboboxItems.Add(new ComboBoxItemValues(Rating, loader.GetString(Rating)));
                    comboboxItems.Add(new ComboBoxItemValues(LastAdded, loader.GetString(LastAdded)));
                    break;
            }
            return comboboxItems;
        }
    }
}
