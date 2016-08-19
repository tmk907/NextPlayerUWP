﻿using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Common;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class NowPlayingViewModel : Template10.Mvvm.ViewModelBase
    {
        public NowPlayingViewModel()
        {
            _timer = new DispatcherTimer();
            SetupTimer();

            App.Current.Resuming += Current_Resuming;
            App.Current.Suspending += Current_Suspending;
            lastFmCache = new LastFmCache();
        }

        private LastFmCache lastFmCache;

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            SongCoverManager.CoverUriPrepared -= ChangeCoverUri;
            PlaybackService.MediaPlayerStateChanged -= ChangePlayButtonContent;
            PlaybackService.MediaPlayerTrackChanged -= ChangeSong;
            PlaybackService.MediaPlayerMediaOpened -= PlaybackService_MediaPlayerMediaOpened;
            StopTimer();
        }

        private void Current_Resuming(object sender, object e)
        {
            SongCoverManager.CoverUriPrepared += ChangeCoverUri;
            PlaybackService.MediaPlayerStateChanged += ChangePlayButtonContent;
            PlaybackService.MediaPlayerTrackChanged += ChangeSong;
            PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
            StartTimer();
            ChangePlayButtonContent(PlaybackService.Instance.PlayerState);
        }

        #region Properties
        private SongItem song = new SongItem();
        public SongItem Song
        {
            get
            {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    song = new SongItem();
                }
                return song;
            }
            set
            {
                Set(ref song, value);
            }
        }

        private double sliderMaxValue = 0.0;
        public double SliderMaxValue
        {
            get { return sliderMaxValue; }
            set { Set(ref sliderMaxValue, value); }
        }

        private double sliderValue = 0.0;
        public double SliderValue
        {
            get { return sliderValue; }
            set { Set(ref sliderValue, value); }
        }

        private TimeSpan currentTime = TimeSpan.Zero;
        public TimeSpan CurrentTime
        {
            get { return currentTime; }
            set { Set(ref currentTime, value); }
        }

        private TimeSpan timeEnd = TimeSpan.Zero;
        public TimeSpan TimeEnd
        {
            get { return timeEnd; }
            set { Set(ref timeEnd, value); }
        }

        private string playButtonContent = "\uE768";
        public string PlayButtonContent
        {
            get { return playButtonContent; }
            set { Set(ref playButtonContent, value); }
        }

        private bool shuffleMode = false;
        public bool ShuffleMode
        {
            get { return shuffleMode; }
            set { Set(ref shuffleMode, value); }
        }

        private RepeatEnum repeatMode = RepeatEnum.NoRepeat;
        public RepeatEnum RepeatMode
        {
            get { return repeatMode; }
            set { Set(ref repeatMode, value); }
        }

        private Uri coverUri;
        public Uri CoverUri
        {
            get { return coverUri; }
            set { Set(ref coverUri, value); }
        }

        private int volume = 100;
        public int Volume
        {
            get { return volume; }
            set
            {
                if (volume != value)
                {
                    //if (value == 0) isMuted = true;
                    //else isMuted = false;
                    PlaybackService.Instance.ChangeVolume(value);
                }
                Set(ref volume, value);
            }
        }

        private bool isVolumeControlVisible = false;
        public bool IsVolumeControlVisible
        {
            get { return isVolumeControlVisible; }
            set { Set(ref isVolumeControlVisible, value); }
        }
        #endregion

        #region Commands

        public void Play()
        {
            PlaybackService.Instance.TogglePlayPause();
        }

        public async void Previous()
        {
            await PlaybackService.Instance.Previous();
        }

        public async void Next()
        {
            await PlaybackService.Instance.Next();
        }

        public void ShuffleCommand()
        {
            ShuffleMode = Shuffle.Change();
            PlaybackService.Instance.ChangeShuffle();
        }

        public void RepeatCommand()
        {
            RepeatMode = Repeat.Change();
            PlaybackService.Instance.ChangeRepeat();
        }

        public async void RateSong(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (song.SourceType == MusicSource.LocalFile || song.SourceType == MusicSource.LocalNotMusicLibrary)
            {
                int rating = Int32.Parse(button.Tag.ToString());
                Song.Rating = rating;
                await lastFmCache.CacheTrackLove(song.Artist, song.Title, rating);
                await DatabaseManager.Current.UpdateRatingAsync(song.SongId, song.Rating).ConfigureAwait(false);
            }
        }

        public void ShowVolumeControl()
        {
            IsVolumeControlVisible = !IsVolumeControlVisible;
        }

        #endregion

        #region Image
        private double x, y;

        public void Image_Pressed(object sender, PointerRoutedEventArgs e)
        {
            var a = e.GetCurrentPoint(null);
            x = a.Position.X;
            y = a.Position.Y;
        }

        public void Image_Released(object sender, PointerRoutedEventArgs e)
        {

        }

        public void Image_Exited(object sender, PointerRoutedEventArgs e)
        {
            var a = e.GetCurrentPoint(null);
            if (Math.Abs(x - a.Position.X) > 50)
            {
                if (x - a.Position.X > 0) Next();
                else Previous();
            }
        }

        public void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            Play();
        }
        #endregion

        private void ChangePlayButtonContent(MediaPlaybackState state)
        {
            Dispatcher.Dispatch(() => 
            {
                if (state == MediaPlaybackState.Playing)
                {
                    PlayButtonContent = "\uE769";
                }
                else
                {
                    PlayButtonContent = "\uE768";
                }
            });
        }

        private void ChangeSong(int index)
        {
            Dispatcher.Dispatch(() =>
            {
                Song = NowPlayingPlaylistManager.Current.GetSongItem(index);
                if (!song.IsAlbumArtSet)
                {

                }
                else
                {
                    CoverUri = song.AlbumArtUri;
                }
            });
        }

        private void PlaybackService_MediaPlayerPositionChanged(TimeSpan position, TimeSpan duration)
        {
            CurrentTime = position;
            SliderValue = position.TotalSeconds;
            //TimeEnd = duration;
        }

        private async void PlaybackService_MediaPlayerMediaOpened()
        {
            await Task.Delay(200);
            var duration = PlaybackService.Instance.Duration;
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                if (!_timer.IsEnabled)
                {
                    StartTimer();
                }
                CurrentTime = TimeSpan.Zero;
                TimeEnd = duration;
                SliderValue = 0.0;
                SliderMaxValue = (int)Math.Round(duration.TotalSeconds - 0.5, MidpointRounding.AwayFromZero);
            });
        }

        private void PlaybackService_MediaPlayerMediaClosed()
        {
            StopTimer();
        }

        public void ChangeCoverUri(Uri cacheUri)
        {
            CoverUri = cacheUri;
        }

        #region Slider Timer

        public bool sliderpressed = false;
        private DispatcherTimer _timer;

        private void SetupTimer()
        {
            _timer.Interval = TimeSpan.FromSeconds(0.5);
            //_timer.Tick += _timer_Tick;
        }

        private TimeSpan position = TimeSpan.Zero;

        private void _timer_Tick(object sender, object e)
        {
            if (!sliderpressed)
            {
                position = PlaybackService.Instance.Position;
                SliderValue = position.TotalSeconds;
                CurrentTime = position;
            }
            else
            {
                CurrentTime = TimeSpan.FromSeconds(SliderValue);
            }
        }

        private void StartTimer()
        {
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
            _timer.Tick -= _timer_Tick;
        }

        private void videoMediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // get HRESULT from event args 
            string hr = GetHresultFromErrorMessage(e);
            // Handle media failed event appropriately 
        }

        private string GetHresultFromErrorMessage(ExceptionRoutedEventArgs e)
        {
            String hr = String.Empty;
            String token = "HRESULT - ";
            const int hrLength = 10;     // eg "0xFFFFFFFF"

            int tokenPos = e.ErrorMessage.IndexOf(token, StringComparison.Ordinal);
            if (tokenPos != -1)
            {
                hr = e.ErrorMessage.Substring(tokenPos + token.Length, hrLength);
            }

            return hr;
        }

        #endregion

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            System.Diagnostics.Debug.WriteLine("NowPlayingVM OnNavigatedToAsync");
            App.ChangeBottomPlayerVisibility(false);
            CoverUri = SongCoverManager.Instance.GetCurrent();
            SongCoverManager.CoverUriPrepared += ChangeCoverUri;
            PlaybackService.MediaPlayerStateChanged += ChangePlayButtonContent;
            PlaybackService.MediaPlayerTrackChanged += ChangeSong;
            PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
            StartTimer();
            ChangePlayButtonContent(PlaybackService.Instance.PlayerState);
            

            Song =  NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            
            RepeatMode = Repeat.CurrentState();
            ShuffleMode = Shuffle.CurrentState();

            TimeEnd = song.Duration;
            SliderValue = 0.0;
            SliderMaxValue = (int)Math.Round(song.Duration.TotalSeconds - 0.5, MidpointRounding.AwayFromZero);

            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            System.Diagnostics.Debug.WriteLine("NowPlayingVM OnNavigatedFromAsync");
            App.ChangeBottomPlayerVisibility(true);
            SongCoverManager.CoverUriPrepared -= ChangeCoverUri;
            PlaybackService.MediaPlayerStateChanged -= ChangePlayButtonContent;
            PlaybackService.MediaPlayerTrackChanged -= ChangeSong;
            PlaybackService.MediaPlayerMediaOpened -= PlaybackService_MediaPlayerMediaOpened;
            StopTimer();
            if (suspending)
            {
                //state[nameof(ShuffleMode)] = ShuffleMode;
                //state[nameof(RepeatMode)] = RepeatMode;
                state[nameof(CoverUri)] = CoverUri.ToString();
            }
            await Task.CompletedTask;
        }

        public void GoToNowPlayingPlaylist()
        {
            NavigationService.Navigate(App.Pages.NowPlayingPlaylist);
        }

        public void GoToLyrics()
        {
            NavigationService.Navigate(App.Pages.Lyrics);
        }
    }
}
