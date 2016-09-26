using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Diagnostics;
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
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class BottomPlayerViewModel : Template10.Mvvm.ViewModelBase
    {
        public BottomPlayerViewModel()
        {
            Logger.DebugWrite("BottomPlayerViewModel()", "");
            RepeatMode = Repeat.CurrentState();
            ShuffleMode = Shuffle.CurrentState();
            _timer = new DispatcherTimer();
            SetupTimer();
            ChangePlayButtonContent(PlaybackService.Instance.PlayerState);
            PlaybackService.MediaPlayerStateChanged += ChangePlayButtonContent;
            PlaybackService.MediaPlayerTrackChanged += ChangeSong;
            PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
            SongCoverManager.CoverUriPrepared += ChangeCoverUri;
            Song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            if (!song.IsAlbumArtSet)
            {
                CoverUri = SongCoverManager.Instance.GetCurrent();
            }
            else
            {
                CoverUri = song.AlbumArtUri;
            }
            Volume = (int)(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Volume) ?? 100);
            App.Current.Resuming += Current_Resuming;
            App.Current.Suspending += Current_Suspending;
            NowPlayingPlaylistManager.Current.SetDispatcher(WindowWrapper.Current().Dispatcher);
            PlaybackService.Instance.Initialize().ConfigureAwait(false);
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            Logger.DebugWrite("BottomPlayerViewModel()", "Suspending");
            //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("BottomPlayerVM Suspending");
            //NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
            PlaybackService.MediaPlayerStateChanged -= ChangePlayButtonContent;
            PlaybackService.MediaPlayerTrackChanged -= ChangeSong;
            PlaybackService.MediaPlayerMediaOpened -= PlaybackService_MediaPlayerMediaOpened;
            SongCoverManager.CoverUriPrepared -= ChangeCoverUri;
            System.Diagnostics.Debug.WriteLine("BottomPlayerVM Suspending");
            StopTimer();
            SongCoverManager.CoverUriPrepared -= ChangeCoverUri;
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.Volume, volume);
        }

        private void Current_Resuming(object sender, object e)
        {
            Logger.DebugWrite("BottomPlayerViewModel()", "Resuming");
            //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("BottomPlayerVM Resuming");
            //NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
            PlaybackService.MediaPlayerStateChanged += ChangePlayButtonContent;
            PlaybackService.MediaPlayerTrackChanged += ChangeSong;
            PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
            SongCoverManager.CoverUriPrepared += ChangeCoverUri;
            System.Diagnostics.Debug.WriteLine("BottomPlayerVM Resuming");
            StartTimer();
            SongCoverManager.CoverUriPrepared += ChangeCoverUri;
            Volume = (int)(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Volume) ?? 100);
        }

        #region Properties
        private SongItem song = new SongItem();
        public SongItem Song
        {
            get {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    song = new SongItem();
                }
                return song;
            }
            set {
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
                    if (value == 0) isMuted = true;
                    else isMuted = false;
                    PlaybackService.Instance.ChangeVolume(value);
                }
                Set(ref volume, value);
            }
        }

        private bool isMuted = false;
        private int prevVolume = 100;

        #endregion

        #region Commands

        public void Play()
        {
            PlaybackService.Instance.TogglePlayPause();
        }

        public void Previous()
        {
            PlaybackService.Instance.Previous();
        }

        public void Next()
        {
            PlaybackService.Instance.Next();
        }

        public async void ShuffleCommand()
        {
            ShuffleMode = Shuffle.Change();
            await PlaybackService.Instance.ChangeShuffle();
        }

        public void RepeatCommand()
        {
            RepeatMode = Repeat.Change();
            PlaybackService.Instance.ApplyRepeatState();
        }
        
        public void MuteVolume()
        {
            if (isMuted)
            {
                Volume = prevVolume;
            }
            else
            {
                prevVolume = volume;
                Volume = 0;
            }
        }

        #endregion

        private void ChangePlayButtonContent(MediaPlaybackState state)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                if (state == MediaPlaybackState.Playing || state == MediaPlaybackState.Buffering)
                {
                    PlayButtonContent = "\uE769";
                }
                else if (state == MediaPlaybackState.Paused)
                {
                    PlayButtonContent = "\uE768";
                }
                else
                {
                    PlayButtonContent = "\uE768";
                }
            });
        }

        private void ChangeSong(int index)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
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

        private async void PlaybackService_MediaPlayerMediaOpened()
        {
            await Task.Delay(400);
            await WindowWrapper.Current().Dispatcher.DispatchAsync(async () =>
            {
                var duration = PlaybackService.Instance.Duration;
                if (duration == TimeSpan.MaxValue)
                {
                    duration = TimeSpan.Zero;
                }
                if (!_timer.IsEnabled)
                {
                    StartTimer();
                }
                CurrentTime = TimeSpan.Zero;
                TimeEnd = duration;
                SliderValue = 0.0;
                SliderMaxValue = (int)Math.Round(duration.TotalSeconds - 0.5, MidpointRounding.AwayFromZero);
                if (song.Duration == TimeSpan.Zero && song.SourceType == MusicSource.LocalFile || song.SourceType == MusicSource.Dropbox || song.SourceType == MusicSource.OneDrive || song.SourceType == MusicSource.PCloud)
                {
                    song.Duration = timeEnd;
                    await DatabaseManager.Current.UpdateSongDurationAsync(song.SongId, timeEnd);//.ConfigureAwait(false);
                }
            });
        }

        private void PlaybackService_MediaPlayerMediaClosed()
        {
            StopTimer();
        }

        public void ChangeCoverUri(Uri cacheUri)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                CoverUri = cacheUri;
            });
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            Logger.DebugWrite("BottomPlayerViewModel()", "OnNavigatedToAsync");
            NextPlayerUWPDataLayer.Diagnostics.Logger.Save("BottomPlayerVM OnNavigatedToAsync");
            NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
            PlaybackService.MediaPlayerStateChanged += ChangePlayButtonContent;
            PlaybackService.MediaPlayerTrackChanged += ChangeSong;
            PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
            StartTimer();
            Song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            CoverUri = SongCoverManager.Instance.GetCurrent();
            ChangePlayButtonContent(PlaybackService.Instance.PlayerState);
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Logger.DebugWrite("BottomPlayerViewModel()", "OnNavigatedFromAsync");
            NextPlayerUWPDataLayer.Diagnostics.Logger.Save("BottomPlayerVM OnNavigatedFromAsync");
            NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
            PlaybackService.MediaPlayerStateChanged -= ChangePlayButtonContent;
            PlaybackService.MediaPlayerTrackChanged -= ChangeSong;
            PlaybackService.MediaPlayerMediaOpened -= PlaybackService_MediaPlayerMediaOpened;
            StopTimer();
            if (suspending)
            {

            }
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.Volume, volume);
            await Task.CompletedTask;
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

    }
}
