using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.ViewModels
{
    public class RightPanelViewModel : Template10.Mvvm.ViewModelBase
    {
        int firstVisibleIndex = 0;
        string positionKey;
        ListView listView;

        public RightPanelViewModel()
        {
            NowPlayingPlaylistManager.NPListChanged += NPListChanged;
            PlaybackManager.MediaPlayerTrackChanged += TrackChanged;
            //App.SongUpdated += App_SongUpdated;
            loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            UpdatePlaylist();
        }

        private async Task DelayedUpdatePlaylist()
        {
            await Task.Delay(200);
            UpdatePlaylist();
        }

        private int selectedPivotIndex = 0;
        public int SelectedPivotIndex
        {
            get { return selectedPivotIndex; }
            set { Set(ref selectedPivotIndex, value); }
        }

        private async void TrackChanged(int index)
        {
            if (songs.Count == 0 || index > songs.Count - 1 || index < 0) return;
            ScrollAfterTrackChanged(index);
            //await Task.Run(() => ChangeLyrics(index));
            await ChangeLyrics(index);
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
            string action = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.ActionAfterDropItem) as string;

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
                    MediaImport mi = new MediaImport();
                    bool first = true;
                    foreach (var file in items)
                    {
                        var storageFile = file as Windows.Storage.StorageFile;
                        string type = storageFile.FileType.ToLower();
                        if (MediaImport.IsAudioFile(type))
                        {
                            SongItem newSong = await mi.OpenSingleFileAsync(storageFile);

                            if (action.Equals(AppConstants.ActionAddToNowPlaying))
                            {
                                await NowPlayingPlaylistManager.Current.Add(newSong);
                            }
                            else if (action.Equals(AppConstants.ActionPlayNext))
                            {
                                await NowPlayingPlaylistManager.Current.AddNext(newSong);
                            }
                            else if (action.Equals(AppConstants.ActionPlayNow))//!
                            {
                                if (first)
                                {
                                    await NowPlayingPlaylistManager.Current.NewPlaylist(newSong);
                                    ApplicationSettingsHelper.SaveSongIndex(0);
                                    PlaybackManager.Current.PlayNew();
                                }
                                else
                                {
                                    await NowPlayingPlaylistManager.Current.Add(newSong);
                                }
                            }
                            first = false;
                        }
                    }
                }
            }
            if (item != null)
            {
                if (action.Equals(AppConstants.ActionAddToNowPlaying))
                {
                    await NowPlayingPlaylistManager.Current.Add((MusicItem)item);
                }
                else if (action.Equals(AppConstants.ActionPlayNext))
                {
                    await NowPlayingPlaylistManager.Current.AddNext((MusicItem)item);
                }
                else if (action.Equals(AppConstants.ActionPlayNow))
                {
                    await NowPlayingPlaylistManager.Current.NewPlaylist((MusicItem)item);
                    ApplicationSettingsHelper.SaveSongIndex(0);
                    PlaybackManager.Current.PlayNew();
                }
            }
        }

        #region NowPlaying

        private void NPListChanged()
        {
            UpdatePlaylist();
        }

        private ObservableCollection<SongItem> songs;
        public ObservableCollection<SongItem> Songs
        {
            get { return songs; }
            set { Set(ref songs, value); }
        }

        private void UpdatePlaylist()
        {
            Songs = NowPlayingPlaylistManager.Current.songs;
        }

        public async void OnLoaded(ListView p, WebView webView)
        {
            lyricsWebview = webView;
            lyricsWebview.ContentLoading += webView1_ContentLoading;
            lyricsWebview.NavigationStarting += webView1_NavigationStarting;
            lyricsWebview.DOMContentLoaded += webView1_DOMContentLoaded;
            lyricsWebview.NavigationCompleted += LyricsWebview_NavigationCompleted;

            bool scroll = false;
            if (listView != null)
            {
                positionKey = ListViewPersistenceHelper.GetRelativeScrollPosition(listView, ItemToKeyHandler);
                var isp = (ItemsStackPanel)listView.ItemsPanelRoot;
                firstVisibleIndex = isp.FirstVisibleIndex;
                scroll = true;
            }
            listView = (ListView)p;
            if (songs.Count == 0)
            {
                UpdatePlaylist();
            }
            if (scroll && songs.Count > 0 && firstVisibleIndex < songs.Count)
            {
                await SetScrollPosition();
            }
        }

        public void OnUnLoaded()
        {
            positionKey = ListViewPersistenceHelper.GetRelativeScrollPosition(listView, ItemToKeyHandler);
            var isp = (ItemsStackPanel)listView.ItemsPanelRoot;
            firstVisibleIndex = isp.FirstVisibleIndex;
        }

        private void ScrollAfterTrackChanged(int index)
        {
            if (listView == null)
            {
                HockeyProxy.TrackEvent("ScrollAfterTrackChanged listview == null");
                return;
            }
            var isp = (ItemsStackPanel)listView.ItemsPanelRoot;
            if (isp == null) return;
            int firstVisibleIndex = isp.FirstVisibleIndex;
            int lastVisibleIndex = isp.LastVisibleIndex;
            if (index <= lastVisibleIndex && index >= firstVisibleIndex) return;
            if (index < firstVisibleIndex)
            {
                listView.ScrollIntoView(listView.Items[index], ScrollIntoViewAlignment.Leading);
            }
            else if (index > lastVisibleIndex)
            {
                listView.ScrollIntoView(listView.Items[index], ScrollIntoViewAlignment.Default);
            }
        }

        private async Task SetScrollPosition()
        {
            listView.ScrollIntoView(listView.Items[firstVisibleIndex], ScrollIntoViewAlignment.Leading);
            if (positionKey != null)
            {
                await ListViewPersistenceHelper.SetRelativeScrollPositionAsync(listView, positionKey, KeyToItemHandler);
            }
        }

        private string ItemToKeyHandler(object item)
        {
            if (item == null) return null;
            MusicItem mi = (MusicItem)item;
            return mi.GetParameter();
        }

        private IAsyncOperation<object> KeyToItemHandler(string key)
        {
            return Task.Run(() =>
            {
                if (listView.Items.Count <= 0)
                {
                    return null;
                }
                else
                {
                    var i = listView.Items[firstVisibleIndex];
                    if (((MusicItem)i).GetParameter() == key)
                    {
                        return i;
                    }
                    foreach (var item in listView.Items)
                    {
                        if (((MusicItem)item).GetParameter() == key) return item;
                    }
                    return null;
                }
            }).AsAsyncOperation();
        }

        #endregion

        #region Commands

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            int index = 0;
            foreach (var s in songs)
            {
                if (s.SongId == ((SongItem)e.ClickedItem).SongId) break;
                index++;
            }
            ApplicationSettingsHelper.SaveSongIndex(index);
            PlaybackManager.Current.PlayNew();
        }

        public async void Delete(object sender, RoutedEventArgs e)
        {
            if (songs.Count == 1) return;
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
            NavigationService.Navigate(App.Pages.AddToPlaylist, item.GetParameter());
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

            var song = songs[index];
            Title = song.Title;
            Artist = song.Artist;
            Lyrics = "";
            string dbLyrics = await DatabaseManager.Current.GetLyricsAsync(song.SongId);
            WebVisibility = false;
            if (string.IsNullOrEmpty(dbLyrics))
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
                Lyrics = dbLyrics;
            }
            HockeyProxy.TrackEvent("ChangeLyrics()");
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
                NextPlayerUWPDataLayer.Diagnostics.Logger.Save("Lyrics ParseLyrics() " + "\n" + address + " " + artist + " " + title + "\n" + ex.Message);
                NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
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
