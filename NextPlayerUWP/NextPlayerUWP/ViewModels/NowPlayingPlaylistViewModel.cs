using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class NowPlayingPlaylistViewModel : Template10.Mvvm.ViewModelBase
    {
        ListViewScrollerHelper scrollerHelper;

        public NowPlayingPlaylistViewModel()
        {
            ViewModelLocator vml = new ViewModelLocator();
            QueueVM = vml.QueueVM;
            scrollerHelper = new ListViewScrollerHelper();
            sortingHelper = NowPlayingPlaylistManager.Current.SortingHelper;
            ComboBoxItemValues = sortingHelper.ComboBoxItemValues;
            SelectedComboBoxItem = sortingHelper.SelectedSortOption;
            PlaybackService.MediaPlayerTrackChanged += TrackChanged;
        }

        public QueueViewModelBase QueueVM { get; set; }
        SortingHelperForSongItemsInPlaylist sortingHelper;

        private int selectedPivotIndex = 0;
        public int SelectedPivotIndex
        {
            get { return selectedPivotIndex; }
            set { Set(ref selectedPivotIndex, value); }
        }

        private void TrackChanged(int index)
        {
            Template10.Common.IDispatcherWrapper d = Dispatcher;
            if (d == null)
            {
                d = Template10.Common.WindowWrapper.Current().Dispatcher;
            }
            if (d == null)
            {
                NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage("NowPlayingPlaylistViewModel Dispatcher null", NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);
                return;
            }
            d.Dispatch(() =>
            {
                scrollerHelper.ScrollAfterTrackChanged(index);
            });
        }


        public async void OnLoaded(ListView p)
        {
            scrollerHelper.listView = p;
            scrollerHelper.firstVisibleIndex = PlaybackService.Instance.CurrentSongIndex;
            await scrollerHelper.ScrollToPosition();
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            //App.ChangeRightPanelVisibility(true);
            scrollerHelper.GetPositionKey();
            scrollerHelper.GetFirstVisibleIndex();

            if (suspending)
            {
                pageState["firstVisibleIndex"] = scrollerHelper.firstVisibleIndex;
                pageState["positionKey"] = scrollerHelper.positionKey;
            }

            await Task.CompletedTask;
        }

        public void OnUnloaded()
        {
            scrollerHelper.listView = null;
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            //App.ChangeRightPanelVisibility(false);
            App.OnNavigatedToNewView(true);
            
            if (state.ContainsKey("firstVisibleIndex"))
            {
                scrollerHelper.firstVisibleIndex = (int)state["firstVisibleIndex"];
            }
            if (state.ContainsKey("positionKey"))
            {
                scrollerHelper.positionKey = (string)state["positionKey"];
            }
            selectedComboBoxItem = ComboBoxItemValues.FirstOrDefault(c => c.Option.Equals(sortingHelper.SelectedSortOption.Option));
            if (mode == NavigationMode.New || mode == NavigationMode.Forward)
            {
                TelemetryAdapter.TrackPageView(this.GetType().ToString());
            }
            await Task.CompletedTask;
        }

        #region Commands

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            int id = ((SongItem)e.ClickedItem).SongId;
            int index = 0;
            foreach (var s in QueueVM.Songs)
            {
                if (s.SongId == id) break;
                index++;
            }
            await PlaybackService.Instance.JumpTo(index);
        }

        public async void Delete(object sender, RoutedEventArgs e)
        {
            var item = (SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            await NowPlayingPlaylistManager.Current.Delete(item.SongId);
        }

        public void Share(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            // App.Current.NavigationService.Navigate(App.Pages.BluetoothSharePage, item.GetParameter()); TODO
        }

        public async void Pin(object sender, RoutedEventArgs e)
        {
            await TileManager.CreateTile((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
        }

        public void EditTags(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            App.Current.NavigationService.Navigate(App.Pages.TagsEditor, item.GetParameter());
        }

        public void ShowDetails(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            App.Current.NavigationService.Navigate(App.Pages.FileInfo, item.GetParameter());
        }

        public void AddToPlaylist(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            App.Current.NavigationService.Navigate(App.Pages.AddToPlaylist, item.GetParameter());
        }

        public async void GoToArtist(object sender, RoutedEventArgs e)
        {
            var item = (SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            ArtistItem temp = await DatabaseManager.Current.GetArtistItemAsync(item.FirstArtist);
            App.Current.NavigationService.Navigate(App.Pages.Artist, temp.ArtistId);
        }

        public async void GoToAlbum(object sender, RoutedEventArgs e)
        {
            var item = (SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            AlbumItem temp = await DatabaseManager.Current.GetAlbumItemAsync(item.Album, item.AlbumArtist);
            App.Current.NavigationService.Navigate(App.Pages.Album, temp.AlbumId);
        }

        #endregion

        #region Multiple selection
        private bool isClickEnabled = true;
        public bool IsClickEnabled
        {
            get { return isClickEnabled; }
            set
            {
                Set(ref isClickEnabled, value);
                IsMultiSelection = !isClickEnabled && selectionMode == ListViewSelectionMode.Multiple;
            }
        }

        private ListViewSelectionMode selectionMode = ListViewSelectionMode.None;
        public ListViewSelectionMode SelectionMode
        {
            get { return selectionMode; }
            set
            {
                Set(ref selectionMode, value);
                IsMultiSelection = !isClickEnabled && selectionMode == ListViewSelectionMode.Multiple;
            }
        }

        private bool isMultiSelection = false;
        public bool IsMultiSelection
        {
            get { return isMultiSelection; }
            set { Set(ref isMultiSelection, value); }
        }

        public void EnableMultipleSelection()
        {
            IsClickEnabled = false;
            SelectionMode = ListViewSelectionMode.Multiple;
        }

        public void DisableMultipleSelection()
        {
            IsClickEnabled = true;
            SelectionMode = ListViewSelectionMode.None;
        }

        public async Task PlayNowMany(IEnumerable<MusicItem> items)
        {
            await NowPlayingPlaylistManager.Current.NewPlaylist(items);
            await PlaybackService.Instance.PlayNewList(0);
        }

        public async Task PlayNextMany(IEnumerable<MusicItem> items)
        {
            await NowPlayingPlaylistManager.Current.AddNext(items);
        }

        public async Task AddToNowPlayingMany(IEnumerable<MusicItem> items)
        {
            await NowPlayingPlaylistManager.Current.Add(items);
        }

        public void AddToPlaylistMany(IEnumerable<MusicItem> items)
        {
            App.AddToCache(items);
            NavigationService.Navigate(App.Pages.AddToPlaylist, new ListOfMusicItems().GetParameter());
        }
        #endregion

        public void SavePlaylist()
        {
            NowPlayingListItem item = new NowPlayingListItem();
            NavigationService.Navigate(App.Pages.AddToPlaylist, item.GetParameter());
        }

        protected ObservableCollection<ComboBoxItemValue> comboBoxItemValues = new ObservableCollection<ComboBoxItemValue>();
        public ObservableCollection<ComboBoxItemValue> ComboBoxItemValues
        {
            get { return comboBoxItemValues; }
            set
            {
                comboBoxItemValues = value;
            }
        }

        protected ComboBoxItemValue selectedComboBoxItem;
        public ComboBoxItemValue SelectedComboBoxItem
        {
            get { return selectedComboBoxItem; }
            set
            {
                bool diff = selectedComboBoxItem != value;
                selectedComboBoxItem = value;
                if (value != null && diff)
                {
                    SortMusicItems();
                }
            }
        }

        public async void SortMusicItems()
        {
            sortingHelper.SelectedSortOption = selectedComboBoxItem;
            await NowPlayingPlaylistManager.Current.SortPlaylist();
        }
    }
}
