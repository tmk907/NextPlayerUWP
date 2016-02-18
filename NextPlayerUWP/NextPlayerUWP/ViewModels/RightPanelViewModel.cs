using NextPlayerUWP.Common;
using NextPlayerUWP.Helpers;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
            UpdatePlaylist();
            NowPlayingPlaylistManager.NPListChanged += NPListChanged;
            PlaybackManager.MediaPlayerTrackChanged += ChangeLyrics;
            loader = new Windows.ApplicationModel.Resources.ResourceLoader();
        }

        private int selectedPivotIndex = 0;
        public int SelectedPivotIndex
        {
            get { return selectedPivotIndex; }
            set { Set(ref selectedPivotIndex, value); }
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
            // App.Current.NavigationService.Navigate(App.Pages.FileInfo, ((SongItem)SelectedItem).SongId);
        }

        public void AddToPlaylist(object sender, RoutedEventArgs e)
        {
            var item = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            App.Current.NavigationService.Navigate(App.Pages.AddToPlaylist, item.GetParameter()); 
        }

        #endregion

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

        private WebView lyricsWebview;
        private string address;
        private bool original = true;
        private Windows.ApplicationModel.Resources.ResourceLoader loader;

        private bool autoLoadFromWeb = true;

        private async void ChangeLyrics(int index)
        {
            original = true;

            //appBarSave.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            var song = songs[index];
            Title = song.Title;
            Artist = song.Artist;
            Lyrics = "";
            string dbLyrics = DatabaseManager.Current.GetLyrics(song.SongId);
            if (string.IsNullOrEmpty(dbLyrics))
            {
                if (autoLoadFromWeb)
                {
                    WebVisibility = true;
                    LoadLyricsFromWebsite();
                }
            }
            else
            {
                original = false;
                WebVisibility = false;
                Lyrics = dbLyrics;
            }
        }

        private async Task LoadLyricsFromWebsite()
        {
            StatusText = loader.GetString("Connecting") + "...";
            StatusVisibility = true;
            WebVisibility = false;
            string result = await ReadDataFromWeb("http://lyrics.wikia.com/api.php?action=lyrics&artist=" + artist + "&song=" + title + "&fmt=realjson");
            if (result == null || result == "")
            {
                StatusText = loader.GetString("ConnectionError");
                StatusVisibility = true;
                return;
            }
            JsonValue jsonList;
            bool isJson = JsonValue.TryParse(result, out jsonList);
            if (isJson)
            {
                address = jsonList.GetObject().GetNamedString("url");
                address += "?useskin=wikiamobile";
                try
                {
                    Uri a = new Uri(address);
                    lyricsWebview.Navigate(a);
                    WebVisibility = true;
                    StatusVisibility = false;
                }
                catch (FormatException e)
                {
                    StatusText = loader.GetString("ConnectionError");
                    StatusVisibility = true;
                }
            }
            else
            {
                StatusText = loader.GetString("ConnectionError");
                StatusVisibility = true;
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
                StatusText = "Loading content for " + args.Uri.ToString();
            }
        }

        private async void webView1_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            if (original)
            {
                await ParseLyrics();
                original = false;
            }
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

                        SaveLyrics();
                    }
                }
            }
            catch (Exception ex)
            {
                NextPlayerUWPDataLayer.Diagnostics.Logger.Save("Lyrics ParseLyrics() " + "\n" + address + " " + artist + " " + title + "\n" + ex.Message);
                NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
            }
        }

        private async void SaveLyrics()
        {
            int songId = NowPlayingPlaylistManager.Current.GetCurrentPlaying().SongId;
            await DatabaseManager.Current.UpdateLyricsAsync(songId, lyrics);
            //!SaveLater.Current.SaveLyricsLater(songId, lyrics);
        }

        #endregion
    }
}
