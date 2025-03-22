using NextPlayerWinUI.Common;
using NextPlayerWinUIDataLayer.Model;
using NextPlayerWinUIDataLayer.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace NextPlayerWinUI.ViewModels
{
    public class LyricsPanelViewModel : ViewModelBase
    {
        public LyricsPanelViewModel()
        {
            ViewModelLocator vml = new ViewModelLocator();
            //lyricsExtensions = new MyLyricsExtensionsClient();//todo
            cts = new CancellationTokenSource();
        }

        //MyLyricsExtensionsClient lyricsExtensions;
        CancellationTokenSource cts;

        private string artist = "Artist";
        public string Artist
        {
            get { return artist; }
            set { SetProperty(ref artist, value); }
        }

        private string title = "Title";
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private string lyrics = "Lyrics";
        public string Lyrics
        {
            get { return lyrics; }
            set { SetProperty(ref lyrics, value); }
        }

        private Uri lyricsSource;
        public Uri LyricsSource
        {
            get { return lyricsSource; }
            set { SetProperty(ref lyricsSource, value); }
        }

        private bool lyricsSourceVisibility = false;
        public bool LyricsSourceVisibility
        {
            get { return lyricsSourceVisibility; }
            set { SetProperty(ref lyricsSourceVisibility, value); }
        }

        private string statusText = "";
        public string StatusText
        {
            get { return statusText; }
            set { SetProperty(ref statusText, value); }
        }

        private bool statusVisibility = false;
        public bool StatusVisibility
        {
            get { return statusVisibility; }
            set { SetProperty(ref statusVisibility, value); }
        }

        private bool showProgressBar = false;
        public bool ShowProgressBar
        {
            get { return showProgressBar; }
            set { SetProperty(ref showProgressBar, value); }
        }

        

        public async Task SearchLyrics(string artist, string title)
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
            try
            {
                ShowProgressBar = true;
                Title = artist;
                Artist = title;
                Lyrics = "";
                StatusText = "";
                StatusVisibility = false;
                LyricsSourceVisibility = false;
                Lyrics = await LoadLyrics(artist, title, cts.Token);
                ShowProgressBar = false;
                if (String.IsNullOrEmpty(Lyrics))
                {
                    StatusText = "No lyrics found";
                    StatusVisibility = true;
                }
                System.Diagnostics.Debug.WriteLine("LyricsViewModel ChangeLyrics() finished");
                TelemetryAdapter.TrackEvent("SearchLyrics()");
            }
            catch (OperationCanceledException)
            {

            }
        }

        public async Task ChangeLyrics(SongItem song)
        {
            System.Diagnostics.Debug.WriteLine("LyricsViewModel ChangeLyrics()");

            cts.Cancel();
            cts = new CancellationTokenSource();
            try
            {
                ShowProgressBar = true;
                Title = song.Title;
                Artist = song.Artist;
                Lyrics = "";
                StatusText = "";
                StatusVisibility = false;
                LyricsSourceVisibility = false;
                Lyrics = await LoadLyrics(song, cts.Token);
                ShowProgressBar = false;
                if (String.IsNullOrEmpty(Lyrics))
                {
                    StatusText = "No lyrics found";
                    StatusVisibility = true;
                }
                System.Diagnostics.Debug.WriteLine("LyricsViewModel ChangeLyrics() finished");
                TelemetryAdapter.TrackEvent("ChangeLyrics()");
            }
            catch (OperationCanceledException)
            {
                
            }
        }

        private async Task<string> LoadLyrics(SongItem song, CancellationToken token)
        {
            string dbLyrics = await DatabaseManager.Current.GetLyricsAsync(song.SongId);
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(dbLyrics))
            {
                //var extLyrics = await lyricsExtensions.GetLyrics(song.Album, song.Artist, song.Title, token);
                //token.ThrowIfCancellationRequested();
                //if (!String.IsNullOrEmpty(extLyrics.Url))
                //{
                //    try
                //    {
                //        LyricsSource = new Uri(extLyrics.Url);
                //        LyricsSourceVisibility = true;
                //    }
                //    catch (Exception) { }
                //}
                //return extLyrics.Lyrics;
                return "";
            }
            else
            {
                return dbLyrics;
            }
        }

        private async Task<string> LoadLyrics(string artist, string title, CancellationToken token)
        {
            //var extLyrics = await lyricsExtensions.GetLyrics("", artist, title, token);
            //token.ThrowIfCancellationRequested();
            //if (!String.IsNullOrEmpty(extLyrics.Url))
            //{
            //    try
            //    {
            //        LyricsSource = new Uri(extLyrics.Url);
            //        LyricsSourceVisibility = true;
            //    }
            //    catch (Exception) { }
            //}
            //return extLyrics.Lyrics;
            return "";
        }

    }
}
