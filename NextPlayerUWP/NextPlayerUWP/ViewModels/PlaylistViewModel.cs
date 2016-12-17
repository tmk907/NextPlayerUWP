using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Services.NavigationService;
using System.IO;
using Windows.UI.Xaml;
using NextPlayerUWPDataLayer.Playlists;

namespace NextPlayerUWP.ViewModels
{
    public class PlaylistViewModel : MusicViewModelBase
    {
        public PlaylistViewModel()
        {
            sortingHelper = new SortingHelperForSongItemsInPlaylist("init");
            ComboBoxItemValues = sortingHelper.ComboBoxItemValues;
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                playlist = new ObservableCollection<SongItem>();
                playlist.Add(new SongItem());
                playlist.Add(new SongItem());
                playlist.Add(new SongItem());
                playlist.Add(new SongItem());
                playlist.Add(new SongItem());
                playlist.Add(new SongItem());
                playlist.Add(new SongItem());
            }
        }

        BaseSortingHelper<SongItem> sortingHelper;
        private MusicItemTypes type;
        string firstParam;

        private ObservableCollection<SongItem> playlist = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Playlist
        {
            get { return playlist; }
            set { Set(ref playlist, value); }
        }

        private string pageSubTitle = "";
        public string PageSubTitle
        {
            get { return pageSubTitle; }
            set { Set(ref pageSubTitle, value); }
        }

        private bool isPlainPlaylist = false;
        public bool IsPlainPlaylist
        {
            get { return isPlainPlaylist; }
            set { Set(ref isPlainPlaylist, value); }
        }

        protected override async Task LoadData()
        {
            if (playlist.Count == 0)
            {
                PlaylistItem p = new PlaylistItem(-1, false, "Playlist");
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                IsPlainPlaylist = false;
                int i = 0;
                switch (type)
                {
                    case MusicItemTypes.folder:
                        PageTitle = loader.GetString("Folder");
                        PageSubTitle = Path.GetFileName(firstParam);
                        IsPlainPlaylist = false;
                        Playlist = await DatabaseManager.Current.GetSongItemsFromFolderAsync(firstParam);
                        break;
                    case MusicItemTypes.genre:
                        sortingHelper = new SortingHelperForSongItems("GenrePlaylist");
                        SelectedComboBoxItem = ComboBoxItemValues.FirstOrDefault(a=>a.Option.Equals(sortingHelper.SelectedSortOption.Option));
                        PageTitle = loader.GetString("Genre");
                        PageSubTitle = firstParam;
                        IsPlainPlaylist = false;
                        Playlist = await DatabaseManager.Current.GetSongItemsFromGenreAsync(firstParam);
                        break;
                    case MusicItemTypes.plainplaylist:
                        sortingHelper = new SortingHelperForSongItemsInPlaylist("Playlist");
                        SelectedComboBoxItem = ComboBoxItemValues.FirstOrDefault(a => a.Option.Equals(sortingHelper.SelectedSortOption.Option));
                        PageTitle = loader.GetString("Playlist");
                        p = await DatabaseManager.Current.GetPlainPlaylistAsync(Int32.Parse(firstParam));
                        PageSubTitle = p.Name;
                        IsPlainPlaylist = true;
                        Playlist = await DatabaseManager.Current.GetSongItemsFromPlainPlaylistAsync(Int32.Parse(firstParam));
                        foreach(var song in playlist)
                        {
                            song.Index = i;
                            i++;
                        }
                        break;
                    case MusicItemTypes.smartplaylist:
                        sortingHelper = new SortingHelperForSongItemsInPlaylist("SmartPlaylist");
                        SelectedComboBoxItem = ComboBoxItemValues.FirstOrDefault(a => a.Option.Equals(sortingHelper.SelectedSortOption.Option));
                        PageTitle = loader.GetString("Playlist");
                        p = await DatabaseManager.Current.GetSmartPlaylistAsync(Int32.Parse(firstParam));
                        PageSubTitle = p.Name;
                        IsPlainPlaylist = false;
                        Playlist = await DatabaseManager.Current.GetSongItemsFromSmartPlaylistAsync(Int32.Parse(firstParam));
                        foreach (var song in playlist)
                        {
                            song.Index = i;
                            i++;
                        }
                        break;
                }
            }
            SortMusicItems();
        }

        public override void FreeResources()
        {
            playlist = null;
            playlist = new ObservableCollection<SongItem>();
        }

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter != null)
            {
                type = MusicItem.ParseType(parameter as string);
                firstParam = MusicItem.SplitParameter(parameter as string)[1];
            }
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back || args.NavigationMode == NavigationMode.New)
            {
                playlist = new ObservableCollection<SongItem>();
                pageTitle = "";
            }
            await base.OnNavigatingFromAsync(args);
            
        }

        public async Task SlidableListItemRightCommandRequested(SongItem song)
        {
            await DeleteFromPlaylist(song);
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
            await PlaybackService.Instance.PlayNewList(index);
            //NavigationService.Navigate(App.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }

        public async void ShuffleAllSongs()
        {
            if (playlist.Count == 0) return;
            List<SongItem> list = new List<SongItem>();
            foreach (var s in playlist)
            {
                list.Add(s);
            }
            Random rnd = new Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                SongItem value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            await NowPlayingPlaylistManager.Current.NewPlaylist(list);
            await PlaybackService.Instance.PlayNewList(0);
        }

        public async void DeleteFromPlaylistClick(object sender, RoutedEventArgs e)
        {
            var item = (SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await DeleteFromPlaylist(item);
        }

        private async Task DeleteFromPlaylist(SongItem song)
        {
            int i = 0;
            foreach (var s in playlist)
            {
                if (s.SongId == song.SongId) break;
                i++;
            }
            Playlist.RemoveAt(i);
            var p = await DatabaseManager.Current.GetPlainPlaylistAsync(Int32.Parse(firstParam));
            await DatabaseManager.Current.DeletePlainPlaylistEntryAsync(song.SongId, p.Id);
            PlaylistHelper ph = new PlaylistHelper();
            await ph.UpdatePlaylistFile(p).ConfigureAwait(false);
        }

        protected override void SortMusicItems()
        {
            sortingHelper.SelectedSortOption = selectedComboBoxItem;
            var orderSelector = sortingHelper.GetOrderBySelector();
            var query = playlist.OrderBy(orderSelector);
            Playlist = new ObservableCollection<SongItem>(query);
        }

        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = sender.Text.ToLower();
                var matchingSongs = playlist.Where(s => s.Title.ToLower().StartsWith(query));
                var m2 = playlist.Where(s => s.Title.ToLower().Contains(query));
                var m3 = playlist.Where(s => (s.Album.ToLower().Contains(query) || s.Artist.ToLower().Contains(query)));
                var result = matchingSongs.Concat(m2).Concat(m3).Distinct();
                sender.ItemsSource = result.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            int index;
            if (args.ChosenSuggestion != null)
            {
                index = playlist.IndexOf((SongItem)args.ChosenSuggestion);
            }
            else
            {
                var list = playlist.Where(s => s.Title.ToLower().StartsWith(sender.Text)).OrderBy(s => s.Title).ToList();
                if (list.Count == 0) return;
                index = 0;
                bool find = false;
                foreach (var g in playlist)
                {
                    if (g.Title.Equals(list.FirstOrDefault().Title))
                    {
                        find = true;
                        break;
                    }
                    index++;
                }
                if (!find) return;
            }
            listView.ScrollIntoView(listView.Items[index], ScrollIntoViewAlignment.Leading);
        }

        public void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var song = args.SelectedItem as SongItem;
            sender.Text = song.Title;
        }
    }
}
