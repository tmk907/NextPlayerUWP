using NextPlayerUWP.Common;
using NextPlayerUWP.Extensions;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml.Controls;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class LyricsViewModel : ViewModelBase
    {
        LyricsExtensionsClient lyricsExtensions;
        public LyricsViewModel()
        {
            song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            translationHelper = new TranslationHelper();
            ViewModelLocator vml = new ViewModelLocator();
            lyricsExtensions = new LyricsExtensionsClient(vml.LyricsExtensionsService);
            lyricsWebview = new WebView();
            lyricsWebview.ContentLoading += webView1_ContentLoading;
            lyricsWebview.NavigationStarting += webView1_NavigationStarting;
            lyricsWebview.DOMContentLoaded += webView1_DOMContentLoaded;
            lyricsWebview.NavigationCompleted += LyricsWebview_NavigationCompleted;
        }

        private SongItem song;

        private async void TrackChanged(int index)
        {
            int prevId = song.SongId;
            song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            await Dispatcher.DispatchAsync(async () =>
            {
                if (song.SongId != prevId) await ChangeLyrics();
            });
        }

        public WebView GetWebView()
        {
            return lyricsWebview;
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            System.Diagnostics.Debug.WriteLine("LyricsViewModel OnNavTo()");
            App.OnNavigatedToNewView(true);
            PlaybackService.MediaPlayerTrackChanged += TrackChanged;
            if (suspensionState.ContainsKey(nameof(song.SongId)))
            {
                int id = (int)suspensionState[nameof(song.SongId)];
                var s = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                if (s.SongId != id)
                {
                    song = s;
                    await ChangeLyrics();
                }
                else
                {
                    if (suspensionState.ContainsKey(nameof(Lyrics)))
                    {
                        Lyrics = suspensionState[nameof(Lyrics)]?.ToString();
                    }
                    else
                    {
                        song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                        await ChangeLyrics();
                    }
                }
            }
            else
            {
                song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                await ChangeLyrics();
            }
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            PlaybackService.MediaPlayerTrackChanged -= TrackChanged;
            if (suspending)
            {
                suspensionState[nameof(song.SongId)] = song.SongId;
                suspensionState[nameof(Lyrics)] = Lyrics;
            }

            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
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

        private WebView lyricsWebview;
        //public WebView LyricsWebView
        //{
        //    get { return lyricsWebview; }
        //    set { Set(ref lyricsWebview, value); }
        //}
        private string address;
        private bool original = true;
        private TranslationHelper translationHelper;
        private bool lyricsNotLoaded = false;
        private int cachedIndex = 0;

        private bool changelyrics = false;

        //TODO sprawdzac z ustawien
        private bool autoLoadFromWeb = true;

        private async Task ChangeLyrics()
        {
            System.Diagnostics.Debug.WriteLine("LyricsViewModel ChangeLyrics()");
            original = true;

            WebVisibility = false;
            StatusVisibility = false;

            //appBarSave.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            Title = song.Title;
            Artist = song.Artist;
            Lyrics = "";
            string dbLyrics = await DatabaseManager.Current.GetLyricsAsync(song.SongId);
            
            if (string.IsNullOrEmpty(dbLyrics))
            {
                ShowProgressBar = true;
                var extLyrics = await lyricsExtensions.GetLyrics(song.Album, song.Artist, song.Title);
                ShowProgressBar = false;

                if (extLyrics.Lyrics == "")
                {
                    if (extLyrics.Url != "")
                    {
                        try
                        {
                            ShowProgressBar = true;
                            lyricsWebview.Navigate(new Uri(extLyrics.Url));
                        }
                        catch (Exception ex)
                        {
                            ShowProgressBar = false;
                        }
                    }
                    else
                    {
                        if (autoLoadFromWeb)
                        {
                            try
                            {
                                lyricsWebview.Stop();
                            }
                            catch (InvalidOperationException ex)
                            {

                            }
                            await LoadLyricsFromWebsite();
                        }
                    }
                }
                else
                {
                    original = false;
                    Lyrics = extLyrics.Lyrics;
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
                StatusText = translationHelper.GetTranslation(TranslationHelper.ConnectionError);
                ShowProgressBar = false;
                return;
            }
            JsonValue jsonList;
            bool isJson = JsonValue.TryParse(result, out jsonList);
            if (isJson)
            {
                string l = jsonList.GetObject().GetNamedString("lyrics");
                if (l == "Not found")
                {
                    StatusText = translationHelper.GetTranslation(TranslationHelper.CantFindLyrics);
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
                        StatusText = translationHelper.GetTranslation(TranslationHelper.ConnectionError);
                        ShowProgressBar = false;
                    }
                }
            }
            else
            {
                StatusText = translationHelper.GetTranslation(TranslationHelper.ConnectionError);
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

        public void webView1_NavigationStarting(object sender, WebViewNavigationStartingEventArgs args)
        {
            if (!IsAllowedUri(args.Uri))
                args.Cancel = true;
        }

        public void webView1_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            if (args.Uri != null)
            {
                //StatusText = "Loading content for " + args.Uri.ToString();
            }
        }

        public void webView1_DOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            StatusVisibility = false;
            WebVisibility = true;

            if (original)
            {
                //await ParseLyrics();
                original = false;
            }
        }

        public void LyricsWebview_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
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
                NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage("Lyrics ParseLyrics() " + "\n" + address + " " + artist + " " + title + "\n" + ex.Message);
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
