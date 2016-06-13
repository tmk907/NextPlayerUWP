﻿using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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
        public LyricsViewModel()
        {
            song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
        }

        private SongItem song;

        private void TrackChanged(int index)
        {
            int prevId = song.SongId;
            song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            if (song.SongId != prevId) ChangeLyrics();
        }

        public void OnLoaded(WebView webView)
        {
            lyricsWebview = webView;
            lyricsWebview.ContentLoading += webView1_ContentLoading;
            lyricsWebview.NavigationStarting += webView1_NavigationStarting;
            lyricsWebview.DOMContentLoaded += webView1_DOMContentLoaded;
            lyricsWebview.NavigationCompleted += LyricsWebview_NavigationCompleted;
        }


        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            PlaybackManager.MediaPlayerTrackChanged += TrackChanged;
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
            PlaybackManager.MediaPlayerTrackChanged -= TrackChanged;
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
        private string address;
        private bool original = true;
        private Windows.ApplicationModel.Resources.ResourceLoader loader;
        private bool lyricsNotLoaded = false;
        private int cachedIndex = 0;

        //TODO sprawdzac z ustawien
        private bool autoLoadFromWeb = true;

        private async Task ChangeLyrics()
        {
            original = true;

            //appBarSave.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            
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
                if (l == "Not found")
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