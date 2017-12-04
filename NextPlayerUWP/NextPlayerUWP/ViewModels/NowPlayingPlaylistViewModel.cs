using NextPlayerUWP.Commands;
using NextPlayerUWP.Commands.Navigation;
using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
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
            selectedComboBoxItem = sortingHelper.SelectedSortOption;
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
                scrollerHelper.ScrollToIndex(index);
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
            var command = new DeleteFromNowPlayingCommand((SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void Share(object sender, RoutedEventArgs e)
        {
            var command = new ShareCommand((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        //public async void ShareMany(IEnumerable<SongItem> items)
        //{
        //    ShareHelper sh = new ShareHelper();
        //    await sh.Share(items);
        //    DisableMultipleSelection();
        //}

        public async void Pin(object sender, RoutedEventArgs e)
        {
            var command = new PinCommand((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void EditTags(object sender, RoutedEventArgs e)
        {
            var command = new EditTagsCommand((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void ShowDetails(object sender, RoutedEventArgs e)
        {
            var command = new ShowDetailsCommand((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void AddToPlaylist(object sender, RoutedEventArgs e)
        {
            var command = new AddToPlaylistCommand((MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void GoToArtist(object sender, RoutedEventArgs e)
        {
            var command = new GoToArtistCommand((SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
        }

        public async void GoToAlbum(object sender, RoutedEventArgs e)
        {
            var command = new GoToAlbumCommand((SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter);
            await command.Excecute();
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
            var command = new PlayNowCommand(items);
            await command.Excecute();
        }

        public async Task PlayNextMany(IEnumerable<MusicItem> items)
        {
            var command = new PlayNextCommand(items);
            await command.Excecute();
        }

        public async Task AddToNowPlayingMany(IEnumerable<MusicItem> items)
        {
            var command = new AddToNowPlayingCommand(items);
            await command.Excecute();
        }

        public async void AddToPlaylistMany(IEnumerable<MusicItem> items)
        {
            var command = new AddToPlaylistCommand(items);
            await command.Excecute();
        }
        #endregion

        public void SavePlaylist()
        {
            NowPlayingListItem item = new NowPlayingListItem();
            NavigationService.Navigate(AppPages.Pages.AddToPlaylist, item.GetParameter());
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

        public void ScrollToCurrentPlaying()
        {
            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            int index = 0;
            foreach (var s in QueueVM.Songs)
            {
                if (s.SongId == song.SongId) break;
                index++;
            }
            if (index == QueueVM.Songs.Count) return;
            scrollerHelper.ScrollToIndex(index);
        }

        public async void SortMusicItems()
        {
            sortingHelper.SelectedSortOption = selectedComboBoxItem;
            await NowPlayingPlaylistManager.Current.SortPlaylist();
        }
    }
}
