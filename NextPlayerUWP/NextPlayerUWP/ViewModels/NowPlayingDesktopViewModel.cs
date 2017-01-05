using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class NowPlayingDesktopViewModel : Template10.Mvvm.ViewModelBase
    {
        public NowPlayingDesktopViewModel()
        {
            sortingHelper = NowPlayingPlaylistManager.Current.SortingHelper;
            UpdatePlaylist();
            NowPlayingPlaylistManager.NPListChanged += NPListChanged;
            PlaybackService.MediaPlayerTrackChanged += TrackChanged;
            lastFmCache = new LastFmCache();
        }

        SortingHelperForSongItemsInPlaylist sortingHelper;
        LastFmCache lastFmCache;

        private void NPListChanged()
        {
            UpdatePlaylist();
        }

        private void TrackChanged(int index)
        {
            Dispatcher.Dispatch(() =>
            {
                if (songs.Count == 0 || index > songs.Count - 1 || index < 0) return;
                CurrentSong = songs[index];
                if (!CurrentSong.IsAlbumArtSet)
                {

                }
                else
                {
                    CoverUri = CurrentSong.AlbumArtUri;
                }
            });
        }

        private ObservableCollection<SongItem> songs;
        public ObservableCollection<SongItem> Songs
        {
            get { return songs; }
            set { Set(ref songs, value); }
        }

        private SongItem currentSong = new SongItem();
        public SongItem CurrentSong
        {
            get { return currentSong; }
            set { Set(ref currentSong, value); }
        }

        private Uri coverUri;
        public Uri CoverUri
        {
            get { return coverUri; }
            set
            {
                Set(ref coverUri, value);
            }
        }

        private void UpdatePlaylist()
        {
            Songs = NowPlayingPlaylistManager.Current.songs;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            //App.ChangeRightPanelVisibility(true);
            SongCoverManager.CoverUriPrepared -= ChangeCoverUri;

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            //App.ChangeRightPanelVisibility(false);
            App.OnNavigatedToNewView(true, true);
            CoverUri = SongCoverManager.Instance.GetCurrent();
            SongCoverManager.CoverUriPrepared += ChangeCoverUri;
            int i = ApplicationSettingsHelper.ReadSongIndex();
            if (i < songs.Count && i >= 0)
            {
                CurrentSong = songs[i];
            }
            if (mode == NavigationMode.New || mode == NavigationMode.Forward)
            {
                TelemetryAdapter.TrackPageView(this.GetType().ToString());
            }
            await Task.CompletedTask;
        }
        
        public async void RateSong(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (currentSong.SourceType == MusicSource.LocalFile || currentSong.SourceType == MusicSource.LocalNotMusicLibrary)
            {
                int rating = Int32.Parse(button.Tag.ToString());
                currentSong.Rating = rating;
                await lastFmCache.RateSong(currentSong.Artist, currentSong.Title, rating);
                await DatabaseManager.Current.UpdateRatingAsync(currentSong.SongId, currentSong.Rating).ConfigureAwait(false);
            }
        }

        public void ChangeCoverUri(Uri cacheUri)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                CoverUri = cacheUri;
            });
        }
    }
}
