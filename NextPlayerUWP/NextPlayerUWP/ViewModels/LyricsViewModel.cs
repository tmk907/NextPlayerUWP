using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            ViewModelLocator vml = new ViewModelLocator();
            lyricsPanelVM = vml.LyricsPanelVM;
        }

        private SongItem song;
        private LyricsPanelViewModel lyricsPanelVM;

        private string artistSearch = "";
        public string ArtistSearch
        {
            get { return artistSearch; }
            set { Set(ref artistSearch, value); }
        }

        private string titleSearch = "";
        public string TitleSearch
        {
            get { return titleSearch; }
            set { Set(ref titleSearch, value); }
        }

        public async void SearchLyrics()
        {
            await lyricsPanelVM.SearchLyrics(artistSearch, titleSearch);
        } 

        private async void TrackChanged(int index)
        {
            int prevId = song.SongId;
            song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            await Dispatcher.DispatchAsync(async () =>
            {
                if (song.SongId != prevId) await lyricsPanelVM.ChangeLyrics(song);
            });
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
                    await lyricsPanelVM.ChangeLyrics(song);
                }
                else
                {
                    if (suspensionState.ContainsKey(nameof(lyricsPanelVM.Lyrics)))
                    {
                        lyricsPanelVM.Lyrics = suspensionState[nameof(lyricsPanelVM.Lyrics)]?.ToString();
                    }
                    else
                    {
                        song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                        await lyricsPanelVM.ChangeLyrics(song);
                    }
                }
            }
            else
            {
                song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
                await lyricsPanelVM.ChangeLyrics(song);
            }
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            PlaybackService.MediaPlayerTrackChanged -= TrackChanged;
            if (suspending)
            {
                suspensionState[nameof(song.SongId)] = song.SongId;
                suspensionState[nameof(lyricsPanelVM.Lyrics)] = lyricsPanelVM.Lyrics;
            }

            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }
    }
}
