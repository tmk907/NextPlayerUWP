﻿using NextPlayerUWP.Common;
using NextPlayerUWP.Extensions;
using NextPlayerUWP.Messages;
using NextPlayerUWP.Messages.Hub;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Template10.Common;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.ViewModels
{
    public class RightPanelViewModel : Template10.Mvvm.ViewModelBase
    {
        ListViewScrollerHelper scrollerHelper;
        LyricsExtensionsClient lyricsExtensions;

        public RightPanelViewModel()
        {
            Logger2.DebugWrite("RightPanelViewModel()", "");

            ViewModelLocator vml = new ViewModelLocator();
            QueueVM = vml.QueueVM;
            scrollerHelper = new ListViewScrollerHelper();
            sortingHelper = NowPlayingPlaylistManager.Current.SortingHelper;
            ComboBoxItemValues = sortingHelper.ComboBoxItemValues;
            SelectedComboBoxItem = sortingHelper.SelectedSortOption;
            loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            var helper = new LyricsExtensions();
            lyricsExtensions = new LyricsExtensionsClient(helper);
            Init();
            App.Current.EnteredBackground += Current_EnteredBackground;
            App.Current.LeavingBackground += Current_LeavingBackground;
            //AppMessenger2.RegisterShowLyrics(this);
            //AppMessenger2.RegisterShowNowPlayingList(this);
            //AppMessenger.Instance.Subscribe<ShowLyrics>(OnShowLyricsMessage);
            //AppMessenger.Instance.Subscribe<ShowNowPlayingList>(OnShowNowPlayingListMessage);
            MessageHub.Instance.Subscribe<ShowLyrics>(OnShowLyricsMessage);
            MessageHub.Instance.Subscribe<ShowNowPlayingList>(OnShowNowPlayingListMessage);
        }
        
        private void Init()
        {
            PlaybackService.MediaPlayerTrackChanged += TrackChanged;
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
        }

        public QueueViewModelBase QueueVM { get; set; }

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
                await ChangeLyrics(index);
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

        public void OnLoaded(ListView p, WebView webView)
        {
            Logger2.DebugWrite("RightPanelViewModel", "OnLoaded");

            lyricsWebview = webView;
            lyricsWebview.ContentLoading += webView1_ContentLoading;
            lyricsWebview.NavigationStarting += webView1_NavigationStarting;
            lyricsWebview.DOMContentLoaded += webView1_DOMContentLoaded;
            lyricsWebview.NavigationCompleted += LyricsWebview_NavigationCompleted;

            scrollerHelper.listView = (ListView)p;
            scrollerHelper.ScrollAfterTrackChanged(PlaybackService.Instance.CurrentSongIndex);
        }

        public void OnUnLoaded()
        {
            Logger2.DebugWrite("RightPanelViewModel", "OnUnLoaded");
            scrollerHelper.GetPositionKey();
            scrollerHelper.GetFirstVisibleIndex();
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
            if (QueueVM.Songs != null)
            {
                await NowPlayingPlaylistManager.Current.SortPlaylist();
            }
        }

        #region Lyrics

        private string artist = "Artist";
        public string Artist
        {
            get { return artist; }
            set { Set(ref artist, value); }
        }

        private string title = "Title";
        public string Title
        {
            get { return title; }
            set { Set(ref title, value); }
        }

        private string lyrics = "Lyrics";
        public string Lyrics
        {
            get { return lyrics; }
            set { Set(ref lyrics, value); }
        }

        private string statusText = "";
        public string StatusText
        {
            get { return statusText; }
            set { Set(ref statusText, value); }
        }

        private bool statusVisibility = false;
        public bool StatusVisibility
        {
            get { return statusVisibility; }
            set { Set(ref statusVisibility, value); }
        }

        private bool webVisibility = false;
        public bool WebVisibility
        {
            get { return webVisibility; }
            set { Set(ref webVisibility, value); }
        }

        private bool showProgressBar = false;
        public bool ShowProgressBar
        {
            get { return showProgressBar; }
            set { Set(ref showProgressBar, value); }
        }

        public async void PivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((Pivot)sender).SelectedIndex == 1)
            {
                if (lyricsNotLoaded)
                {
                    await ChangeLyrics(cachedIndex);
                    lyricsNotLoaded = false;
                }
            }
        }

        private WebView lyricsWebview;
        private string address;
        private bool original = true;
        private Windows.ApplicationModel.Resources.ResourceLoader loader;
        private bool lyricsNotLoaded = false;
        private int cachedIndex = 0;

        //TODO sprawdzac z ustawien
        private bool autoLoadFromWeb = true;

        private async Task ChangeLyrics(int index)
        {
            if (SelectedPivotIndex == 0)
            {
                lyricsNotLoaded = true;
                cachedIndex = index;
                return;
            }
            original = true;

            //appBarSave.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            var song = QueueVM.Songs[index];
            Title = song.Title;
            Artist = song.Artist;
            Lyrics = "";
            string dbLyrics = await DatabaseManager.Current.GetLyricsAsync(song.SongId);
            WebVisibility = false;
            if (string.IsNullOrEmpty(dbLyrics))
            {
                string extLyrics = await lyricsExtensions.GetLyrics(song.Album, song.Artist, song.Title);
                if (extLyrics == "")
                {
                    if (autoLoadFromWeb)
                    {
                        lyricsWebview.Stop();
                        await LoadLyricsFromWebsite();
                    }
                }
                else
                {
                    original = false;
                    Lyrics = extLyrics;
                }
            }
            else
            {
                original = false;
                Lyrics = dbLyrics;
            }
            TelemetryAdapter.TrackEvent("ChangeLyrics()");
        }

        private async Task LoadLyricsFromWebsite()
        {
            StatusText = "";

            ShowProgressBar = true;
            StatusVisibility = true;
            WebVisibility = false;

            //StatusText = loader.GetString("Connecting") + "...";

            string result = await ReadDataFromWeb("http://lyrics.wikia.com/api.php?action=lyrics&artist=" + artist + "&song=" + title + "&fmt=realjson");
            if (result == null || result == "")
            {
                StatusText = loader.GetString("ConnectionError");
                ShowProgressBar = false;
                return;
            }
            JsonValue jsonList;
            bool isJson = JsonValue.TryParse(result, out jsonList);
            if (isJson)
            {
                string l = jsonList.GetObject().GetNamedString("lyrics");
                if (l== "Not found")
                {
                    StatusText = loader.GetString("CantFindLyrics");
                    ShowProgressBar = false;
                }
                else
                {
                    address = jsonList.GetObject().GetNamedString("url");
                    address += "?useskin=wikiamobile";
                    try
                    {
                        Uri a = new Uri(address);
                        lyricsWebview.Navigate(a);

                    }
                    catch (FormatException e)
                    {
                        StatusText = loader.GetString("ConnectionError");
                        ShowProgressBar = false;
                    }
                }
            }
            else
            {
                StatusText = loader.GetString("ConnectionError");
                ShowProgressBar = false;
            }
        }

        private async Task<string> ReadDataFromWeb(string a)
        {
            var client = new HttpClient();
            string result = "";
            try
            {
                var response = await client.GetAsync(new Uri(a));
                result = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        private bool IsAllowedUri(Uri uri)
        {
            return uri.ToString().Contains("lyrics.wikia.com");
        }

        private void webView1_NavigationStarting(object sender, WebViewNavigationStartingEventArgs args)
        {
            if (!IsAllowedUri(args.Uri))
                args.Cancel = true;
        }

        private void webView1_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            if (args.Uri != null)
            {
                //StatusText = "Loading content for " + args.Uri.ToString();
            }
        }

        private void webView1_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            StatusVisibility = false;
            WebVisibility = true;

            if (original)
            {
                //await ParseLyrics();
                original = false;
            }
        }


        private void LyricsWebview_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            ShowProgressBar = false;
        }


        private async Task ParseLyrics()
        {
            try
            {
                string html = await lyricsWebview.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });
                int i0 = html.IndexOf("Tap what's hiding behind");
                if (i0 < 0)
                {
                    int i1 = html.IndexOf("div class=\"lyricbox");
                    if (i1 > 0)
                    {
                        int i2 = html.IndexOf("</script>", i1) + "</script>".Length;
                        int i3 = html.IndexOf("<script>", i2);

                        string text = html.Substring(i2, i3 - i2);
                        lyrics = text.Replace("<br>", "\n").Replace("<b>", "").Replace("</b>", "").Replace("&amp;", "&").Replace("<i>", "(").Replace("</i>", ")");
                        Artist = artist;
                        Title = title;
                        Lyrics = lyrics;

                        await SaveLyrics();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger2.Current.WriteMessage("Lyrics ParseLyrics() " + "\n" + address + " " + artist + " " + title + "\n" + ex.Message);
            }
        }

        private async Task SaveLyrics()
        {
            int songId = NowPlayingPlaylistManager.Current.GetCurrentPlaying().SongId;
            await DatabaseManager.Current.UpdateLyricsAsync(songId, lyrics);
            //!SaveLater.Current.SaveLyricsLater(songId, lyrics);
        }

        #endregion
    }
}
