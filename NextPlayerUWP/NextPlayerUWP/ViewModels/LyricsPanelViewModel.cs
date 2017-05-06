using NextPlayerUWP.Common;
using NextPlayerUWP.Extensions;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Template10.Mvvm;

namespace NextPlayerUWP.ViewModels
{
    public class LyricsPanelViewModel : ViewModelBase
    {
        public LyricsPanelViewModel()
        {
            ViewModelLocator vml = new ViewModelLocator();
            lyricsExtensions = new LyricsExtensionsClient(vml.LyricsExtensionsService);
            cts = new CancellationTokenSource();
        }

        LyricsExtensionsClient lyricsExtensions;
        CancellationTokenSource cts;

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

        private Uri lyricsSource;
        public Uri LyricsSource
        {
            get { return lyricsSource; }
            set { Set(ref lyricsSource, value); }
        }

        private bool lyricsSourceVisibility = false;
        public bool LyricsSourceVisibility
        {
            get { return lyricsSourceVisibility; }
            set { Set(ref lyricsSourceVisibility, value); }
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

        private bool showProgressBar = false;
        public bool ShowProgressBar
        {
            get { return showProgressBar; }
            set { Set(ref showProgressBar, value); }
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
                LyricsSourceVisibility = false;
                Lyrics = await LoadLyrics(song, cts.Token);
                ShowProgressBar = false;
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
                var extLyrics = await lyricsExtensions.GetLyrics(song.Album, song.Artist, song.Title, token);
                token.ThrowIfCancellationRequested();
                if (!String.IsNullOrEmpty(extLyrics.Url))
                {
                    try
                    {
                        LyricsSource = new Uri(extLyrics.Url);
                        LyricsSourceVisibility = true;
                    }
                    catch (Exception) { }
                }
                return extLyrics.Lyrics;
            }
            else
            {
                return dbLyrics;
            }
        }

    }
}
