using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            ChangePlayButtonContent(App.PlaybackManager.PlayerState);
            PlaybackManager.MediaPlayerStateChanged += ChangePlayButtonContent;
            PlaybackManager.MediaPlayerTrackChanged += ChangeSong;
            PlaybackManager.MediaPlayerMediaOpened += PlaybackManager_MediaPlayerMediaOpened;
            PlaybackManager.MediaPlayerMediaClosed += PlaybackManager_MediaPlayerMediaClosed;
            PlaybackManager.MediaPlayerPositionChanged += PlaybackManager_MediaPlayerPositionChanged;
            SongCoverManager.CoverUriPrepared += ChangeCoverUri;
            Song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            CoverUri = SongCoverManager.Instance.GetFirst();
            Volume = (int)(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Volume) ?? 100);
            App.Current.Resuming += Current_Resuming;
            App.Current.Suspending += Current_Suspending;
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            Logger.DebugWrite("BottomPlayerViewModel()", "Suspending");
            //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("BottomPlayerVM Suspending");
            //NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
            PlaybackManager.MediaPlayerStateChanged -= ChangePlayButtonContent;
            PlaybackManager.MediaPlayerTrackChanged -= ChangeSong;
            PlaybackManager.MediaPlayerMediaOpened -= PlaybackManager_MediaPlayerMediaOpened;
            PlaybackManager.MediaPlayerMediaClosed -= PlaybackManager_MediaPlayerMediaClosed;
            PlaybackManager.MediaPlayerPositionChanged -= PlaybackManager_MediaPlayerPositionChanged;
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
            PlaybackManager.MediaPlayerStateChanged += ChangePlayButtonContent;
            PlaybackManager.MediaPlayerTrackChanged += ChangeSong;
            PlaybackManager.MediaPlayerMediaOpened += PlaybackManager_MediaPlayerMediaOpened;
            PlaybackManager.MediaPlayerMediaClosed += PlaybackManager_MediaPlayerMediaClosed;
            PlaybackManager.MediaPlayerPositionChanged += PlaybackManager_MediaPlayerPositionChanged;
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
                    App.PlaybackManager.SendMessage(AppConstants.Volume, value / 100.0);
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
            App.PlaybackManager.Play();
        }

        public void Previous()
        {
            App.PlaybackManager.Previous();
        }

        public void Next()
        {
            App.PlaybackManager.Next();
        }

        public void ShuffleCommand()
        {
            ShuffleMode = Shuffle.Change();
            App.PlaybackManager.SendMessage(AppConstants.Shuffle, "");
        }

        public void RepeatCommand()
        {
            RepeatMode = Repeat.Change();
            App.PlaybackManager.SendMessage(AppConstants.Repeat, "");
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

        private void ChangePlayButtonContent(MediaPlayerState state)
        {
            if (state== MediaPlayerState.Playing)
            {
                PlayButtonContent = "\uE769";
            }
            else
            {
                PlayButtonContent = "\uE768";
            }
        }

        private void ChangeSong(int index)
        {
            Song = NowPlayingPlaylistManager.Current.GetSongItem(index);           
        }

        private void PlaybackManager_MediaPlayerPositionChanged(TimeSpan position, TimeSpan duration)
        {
            CurrentTime = position;
            SliderValue = position.TotalSeconds;
            //TimeEnd = duration;
        }

        private async void PlaybackManager_MediaPlayerMediaOpened(TimeSpan duration)
        {
            if (!_timer.IsEnabled)
            {
                StartTimer();
            }
            CurrentTime = TimeSpan.Zero;
            TimeEnd = duration;
            SliderValue = 0.0;
            SliderMaxValue = (int)Math.Round(duration.TotalSeconds - 0.5, MidpointRounding.AwayFromZero);
            if (song.Duration == TimeSpan.Zero && song.SourceType == NextPlayerUWPDataLayer.Enums.MusicSource.LocalFile)
            {
                song.Duration = timeEnd;
                await DatabaseManager.Current.UpdateSongDurationAsync(song.SongId, timeEnd).ConfigureAwait(false);
            }
        }

        private void PlaybackManager_MediaPlayerMediaClosed()
        {
            StopTimer();
        }

        public void ChangeCoverUri(Uri cacheUri)
        {
            CoverUri = cacheUri;
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            Logger.DebugWrite("BottomPlayerViewModel()", "OnNavigatedToAsync");
            NextPlayerUWPDataLayer.Diagnostics.Logger.Save("BottomPlayerVM OnNavigatedToAsync");
            NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
            PlaybackManager.MediaPlayerStateChanged += ChangePlayButtonContent;
            PlaybackManager.MediaPlayerTrackChanged += ChangeSong;
            PlaybackManager.MediaPlayerMediaOpened += PlaybackManager_MediaPlayerMediaOpened;
            PlaybackManager.MediaPlayerMediaClosed += PlaybackManager_MediaPlayerMediaClosed;
            PlaybackManager.MediaPlayerPositionChanged += PlaybackManager_MediaPlayerPositionChanged;
            StartTimer();
            Song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            CoverUri = SongCoverManager.Instance.GetCurrent();
            ChangePlayButtonContent(App.PlaybackManager.PlayerState);
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Logger.DebugWrite("BottomPlayerViewModel()", "OnNavigatedFromAsync");
            NextPlayerUWPDataLayer.Diagnostics.Logger.Save("BottomPlayerVM OnNavigatedFromAsync");
            NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
            PlaybackManager.MediaPlayerStateChanged -= ChangePlayButtonContent;
            PlaybackManager.MediaPlayerTrackChanged -= ChangeSong;
            PlaybackManager.MediaPlayerMediaOpened -= PlaybackManager_MediaPlayerMediaOpened;
            PlaybackManager.MediaPlayerMediaClosed -= PlaybackManager_MediaPlayerMediaClosed;
            PlaybackManager.MediaPlayerPositionChanged -= PlaybackManager_MediaPlayerPositionChanged;
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
                position = App.PlaybackManager.CurrentPlayer.Position;
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
