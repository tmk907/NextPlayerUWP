using NextPlayerUWP.Common;
using NextPlayerUWP.Extensions;
using NextPlayerUWP.Messages;
using NextPlayerUWP.Messages.Hub;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Template10.Common;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.ViewModels
{
    public class RightPanelViewModel : Template10.Mvvm.ViewModelBase
    {
        ListViewScrollerHelper scrollerHelper;
        CancellationTokenSource cts;

        public RightPanelViewModel()
        {
            Logger2.DebugWrite("RightPanelViewModel()", "");
            System.Diagnostics.Stopwatch s1 = new System.Diagnostics.Stopwatch();
            s1.Start();
            ViewModelLocator vml = new ViewModelLocator();
            QueueVM = vml.QueueVM;
            lyricsPanelVM = vml.LyricsPanelVM;
            scrollerHelper = new ListViewScrollerHelper();
            sortingHelper = NowPlayingPlaylistManager.Current.SortingHelper;
            ComboBoxItemValues = sortingHelper.ComboBoxItemValues;
            SelectedComboBoxItem = sortingHelper.SelectedSortOption;
            loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            isInitialized = false;
            Init();
            App.Current.EnteredBackground += Current_EnteredBackground;
            App.Current.LeavingBackground += Current_LeavingBackground;
            
            MessageHub.Instance.Subscribe<ShowLyrics>(OnShowLyricsMessage);
            MessageHub.Instance.Subscribe<ShowNowPlayingList>(OnShowNowPlayingListMessage);

            cts = new CancellationTokenSource();

            s1.Stop();
            System.Diagnostics.Debug.WriteLine("RightPanelViewModel() End {0}ms", s1.ElapsedMilliseconds);
        }

        private bool isInitialized;
        private void Init()
        {
            if (isInitialized) return;
            PlaybackService.MediaPlayerTrackChanged += TrackChanged;
            isInitialized = true;
        }
        
        public void OnShowLyricsMessage(ShowLyrics msg)
        {
            SelectedPivotIndex = 1;
        }

        public void OnShowNowPlayingListMessage(ShowNowPlayingList msg)
        {
            SelectedPivotIndex = 0;
        }

        private void Current_LeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            Init();
        }

        private void Current_EnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
            PlaybackService.MediaPlayerTrackChanged -= TrackChanged;
            isInitialized = false;
        }

        public QueueViewModelBase QueueVM { get; set; }
        private LyricsPanelViewModel lyricsPanelVM;

        private int selectedPivotIndex = 0;
        public int SelectedPivotIndex
        {
            get { return selectedPivotIndex; }
            set { Set(ref selectedPivotIndex, value); }
        }

        private async void TrackChanged(int index)
        {
            Logger2.DebugWrite("RightPanelViewModel", $"TrackChanged {index}");
            await WindowWrapper.Current().Dispatcher.DispatchAsync(async () =>
            {
                if (QueueVM.SongsCount == 0 || index > QueueVM.SongsCount - 1 || index < 0) return;
                scrollerHelper.ScrollAfterTrackChanged(index);
                if (SelectedPivotIndex == 0)
                {
                    lyricsLoaded = false;
                    cachedIndex = index;
                }
                else
                {
                    var song = QueueVM.Songs[index];
                    await lyricsPanelVM.ChangeLyrics(song);
                }
            });
        }

        public void DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Properties.ContainsKey(typeof(AlbumItem).ToString()) ||
                e.DataView.Properties.ContainsKey(typeof(ArtistItem).ToString()) ||
                e.DataView.Properties.ContainsKey(typeof(FolderItem).ToString()) ||
                e.DataView.Properties.ContainsKey(typeof(GenreItem).ToString()) ||
                e.DataView.Properties.ContainsKey(typeof(PlaylistItem).ToString()) ||
                e.DataView.Properties.ContainsKey(typeof(SongItem).ToString()))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
            else if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                e.AcceptedOperation = DataPackageOperation.Copy;
            }
        }

        public async void DropItem(object sender, DragEventArgs e)
        {
            object item;
            string action = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.ActionAfterDropItem) as string;

            if (e.DataView.Properties.TryGetValue(typeof(AlbumItem).ToString(), out item))
            {

            }
            else if (e.DataView.Properties.TryGetValue(typeof(ArtistItem).ToString(), out item))
            {

            }
            else if (e.DataView.Properties.TryGetValue(typeof(FolderItem).ToString(), out item))
            {

            }
            else if (e.DataView.Properties.TryGetValue(typeof(GenreItem).ToString(), out item))
            {

            }
            else if (e.DataView.Properties.TryGetValue(typeof(PlaylistItem).ToString(), out item))
            {

            }
            else if (e.DataView.Properties.TryGetValue(typeof(SongItem).ToString(), out item))
            {

            }
            else if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    MediaImport mi = new MediaImport(App.FileFormatsHelper);
                    bool first = true;
                    foreach (var file in items)
                    {
                        if (typeof(Windows.Storage.StorageFile) == file.GetType())
                        {
                            var storageFile = file as Windows.Storage.StorageFile;
                            string type = storageFile.FileType.ToLower();
                            if (App.FileFormatsHelper.IsFormatSupported(type))
                            {
                                SongItem newSong = await mi.OpenSingleFileAsync(storageFile);

                                if (action.Equals(SettingsKeys.ActionAddToNowPlaying))
                                {
                                    await NowPlayingPlaylistManager.Current.Add(newSong);
                                }
                                else if (action.Equals(SettingsKeys.ActionPlayNext))
                                {
                                    await NowPlayingPlaylistManager.Current.AddNext(newSong);
                                }
                                else if (action.Equals(SettingsKeys.ActionPlayNow))//!
                                {
                                    if (first)
                                    {
                                        await NowPlayingPlaylistManager.Current.NewPlaylist(newSong);
                                        await PlaybackService.Instance.PlayNewList(0);
                                    }
                                    else
                                    {
                                        await NowPlayingPlaylistManager.Current.Add(newSong);
                                    }
                                }
                                first = false;
                            }
                            else if (App.FileFormatsHelper.IsPlaylistSupportedType(type))
                            {
                                //TODO
                            }
                        }
                    }
                }
            }
            if (item != null)
            {
                if (action.Equals(SettingsKeys.ActionAddToNowPlaying))
                {
                    await NowPlayingPlaylistManager.Current.Add((MusicItem)item);
                }
                else if (action.Equals(SettingsKeys.ActionPlayNext))
                {
                    await NowPlayingPlaylistManager.Current.AddNext((MusicItem)item);
                }
                else if (action.Equals(SettingsKeys.ActionPlayNow))
                {
                    await NowPlayingPlaylistManager.Current.NewPlaylist((MusicItem)item);
                    await PlaybackService.Instance.PlayNewList(0);
                }
            }
        }

        #region NowPlaying

        public void OnLoaded(ListView p)
        {
            Logger2.DebugWrite("RightPanelViewModel", "OnLoaded");

            scrollerHelper.listView = (ListView)p;
            scrollerHelper.ScrollAfterTrackChanged(PlaybackService.Instance.CurrentSongIndex);
        }

        public void OnUnLoaded()
        {
            Logger2.DebugWrite("RightPanelViewModel", "OnUnLoaded");
            //scrollerHelper.GetPositionKey();
            //scrollerHelper.GetFirstVisibleIndex();
            scrollerHelper.listView = null;
        }

        #endregion

        #region Commands

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            int index = 0;
            int id = ((SongItem)e.ClickedItem).SongId;
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

        public void SavePlaylist()
        {
            NowPlayingListItem item = new NowPlayingListItem();
            App.Current.NavigationService.Navigate(App.Pages.AddToPlaylist, item.GetParameter());
        }

        public void ShowAudioSettings()
        {
            App.Current.NavigationService.Navigate(App.Pages.AudioSettings);
        }

        SortingHelperForSongItemsInPlaylist sortingHelper;
        private Windows.ApplicationModel.Resources.ResourceLoader loader;
        private bool lyricsLoaded = false;
        private int cachedIndex = 0;

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

        public async void PivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((Pivot)sender).SelectedIndex == 1)
            {
                if (!lyricsLoaded)
                {
                    var song = QueueVM.Songs[cachedIndex];
                    await lyricsPanelVM.ChangeLyrics(song);
                    lyricsLoaded = true;
                }
            }
        }

        public async void SortMusicItems()
        {
            sortingHelper.SelectedSortOption = selectedComboBoxItem;
            if (QueueVM.Songs != null)
            {
                await NowPlayingPlaylistManager.Current.SortPlaylist();
            }
        }
    }
}
