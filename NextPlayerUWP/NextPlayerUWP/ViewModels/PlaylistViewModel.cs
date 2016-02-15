using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Services.NavigationService;

namespace NextPlayerUWP.ViewModels
{
    public class PlaylistViewModel : MusicViewModelBase
    {
        public PlaylistViewModel()
        {
            SortNames si = new SortNames(MusicItemTypes.song);
            ComboBoxItemValues = si.GetSortNames();
        }

        private MusicItemTypes type;
        string firstParam;

        private ObservableCollection<SongItem> playlist = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Playlist
        {
            get { return playlist; }
            set { Set(ref playlist, value); }
        }

        protected override async Task LoadData()
        {
            if (Playlist.Count == 0)
            {
                switch (type)
                {
                    case MusicItemTypes.folder:
                        Playlist = await DatabaseManager.Current.GetSongItemsFromFolderAsync(firstParam);
                        PageTitle = "Folder";
                        break;
                    case MusicItemTypes.genre:
                        Playlist = await DatabaseManager.Current.GetSongItemsFromGenreAsync(firstParam);
                        PageTitle = "Genre";
                        break;
                    case MusicItemTypes.plainplaylist:
                        Playlist = await DatabaseManager.Current.GetSongItemsFromPlainPlaylistAsync(Int32.Parse(firstParam));
                        PageTitle = "Playlist";
                        break;
                    case MusicItemTypes.smartplaylist:
                        //Playlist = await DatabaseManager.Current.GetSongItemsFromSmartPlaylistAsync(Int32.Parse(firstParam));
                        PageTitle = "Playlist";
                        break;
                }
            }
        }

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter != null)
            {
                type = MusicItem.ParseType(parameter as string);
                firstParam = MusicItem.ParseParameter(parameter as string)[1];
            }
        }

        public override Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back || args.NavigationMode == NavigationMode.New)
            {
                playlist = new ObservableCollection<SongItem>();
                pageTitle = "";
            }
            return base.OnNavigatingFromAsync(args);
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            await SongClicked(((SongItem)e.ClickedItem).SongId);
        }

        private async Task SongClicked(int songid)
        {
            int index = 0;
            foreach (var s in playlist)
            {
                if (s.SongId == songid) break;
                index++;
            }
            await NowPlayingPlaylistManager.Current.NewPlaylist(playlist);
            ApplicationSettingsHelper.SaveSongIndex(index);
            PlaybackManager.Current.PlayNew();
            //NavigationService.Navigate(App.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }

        public void SortItems(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItemValue value = (ComboBoxItemValue)e.AddedItems.FirstOrDefault();
            switch (value.Option)
            {
                case SortNames.Title:
                    Sort(s => s.Title, t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower(), "SongId");
                    break;
                case SortNames.Album:
                    Sort(s => s.Album, t => (t.Album == "") ? "" : t.Album[0].ToString().ToLower(), "Album");
                    break;
                case SortNames.Artist:
                    Sort(s => s.Artist, t => (t.Artist == "") ? "" : t.Artist[0].ToString().ToLower(), "Artist");
                    break;
                case SortNames.AlbumArtist:
                    Sort(s => s.AlbumArtist, t => (t.AlbumArtist == "") ? "" : t.AlbumArtist[0].ToString().ToLower(), "AlbumArtist");
                    break;
                case SortNames.Year:
                    Sort(s => s.Year, t => t.Year, "SongId");
                    break;
                case SortNames.Duration:
                    Sort(s => s.Duration, t => t.Duration, "SongId");
                    break;
                case SortNames.Rating:
                    Sort(s => s.Rating, t => t.Rating, "SongId");
                    break;
                case SortNames.Composer:
                    Sort(s => s.Composer, t => (t.Composer == "") ? "" : t.Composer[0].ToString().ToLower(), "Composer");
                    break;
                case SortNames.LastAdded:
                    Sort(s => s.DateAdded, t => t.DateAdded, "SongId");
                    break;
                case SortNames.LastPlayed:
                    Sort(s => s.LastPlayed, t => t.LastPlayed, "SongId");
                    break;
                case SortNames.PlayCount:
                    Sort(s => s.PlayCount, t => t.PlayCount, "SongId");
                    break;
                default:
                    Sort(s => s.Title, t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower(), "SongId");
                    break;
            }
        }

        private void Sort(Func<SongItem, object> orderSelector, Func<SongItem, object> groupSelector, string propertyName)
        {
            var query = playlist.OrderBy(orderSelector);
            Playlist = new ObservableCollection<SongItem>(query);
        }

        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var matchingSongs = playlist.Where(s => s.Title.ToLower().Contains(sender.Text)).OrderBy(s => s.Title);
                sender.ItemsSource = matchingSongs.ToList();
            }
        }

        public async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                await SongClicked(((SongItem)args.ChosenSuggestion).SongId);
            }
            else
            {
                var list = playlist.Where(s => s.Title.ToLower().Contains(args.QueryText)).OrderBy(s => s.Title).ToList();
                //if (list.Count > 0)
                //{
                //    await SongClicked(list.FirstOrDefault().SongId);
                //}
                sender.ItemsSource = list;
            }
        }

        public void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var song = args.SelectedItem as SongItem;
            sender.Text = song.Title;
        }
    }
}
