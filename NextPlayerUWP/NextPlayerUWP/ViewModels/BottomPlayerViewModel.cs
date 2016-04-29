using GalaSoft.MvvmLight.Command;
using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class BottomPlayerViewModel : Template10.Mvvm.ViewModelBase
    {
        public BottomPlayerViewModel()
        {
            RepeatMode = Repeat.CurrentState();
            ShuffleMode = Shuffle.CurrentState();
            _timer = new DispatcherTimer();
            SetupTimer();
            if (PlaybackManager.Current.IsBackgroundTaskRunning())
            {
                ChangePlayButtonContent(MediaPlayerState.Playing);
            }
            else
            {
                ChangePlayButtonContent(MediaPlayerState.Paused);
            }
            PlaybackManager.MediaPlayerStateChanged += ChangePlayButtonContent;
            PlaybackManager.MediaPlayerTrackChanged += ChangeSong;
            PlaybackManager.MediaPlayerMediaOpened += PlaybackManager_MediaPlayerMediaOpened;
            PlaybackManager.MediaPlayerMediaClosed += PlaybackManager_MediaPlayerMediaClosed;
            PlaybackManager.MediaPlayerPositionChanged += PlaybackManager_MediaPlayerPositionChanged;
            SongCoverManager.CoverUriPrepared += ChangeCoverUri;
            Initialize();
        }

        private async Task Initialize()
        {
            Song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            CoverUri = await SongCoverManager.Instance.PrepareCover(song);
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
        #endregion

        #region Commands
        private RelayCommand play;
        public RelayCommand Play
        {
            get
            {
                return play
                    ?? (play = new RelayCommand(
                    () =>
                    {
                        PlaybackManager.Current.Play();
                    }));
            }
        }

        private RelayCommand previous;
        public RelayCommand Previous
        {
            get
            {
                return previous
                    ?? (previous = new RelayCommand(
                    () =>
                    {
                        PlaybackManager.Current.Previous();
                    }));
            }
        }

        private RelayCommand next;
        public RelayCommand Next
        {
            get
            {
                return next
                    ?? (next = new RelayCommand(
                    () =>
                    {
                        PlaybackManager.Current.Next();
                    }));
            }
        }

        private RelayCommand shuffleCommand;
        public RelayCommand ShuffleCommand
        {
            get
            {
                return shuffleCommand
                    ?? (shuffleCommand = new RelayCommand(
                        () =>
                        {
                            ShuffleMode = Shuffle.Change();
                            PlaybackManager.Current.SendMessage(AppConstants.Shuffle, "");
                        }));
            }
        }

        private RelayCommand repeatCommand;
        public RelayCommand RepeatCommand
        {
            get
            {
                return repeatCommand
                    ?? (repeatCommand = new RelayCommand(
                        () =>
                        {
                            RepeatMode = Repeat.Change();
                            PlaybackManager.Current.SendMessage(AppConstants.Repeat, "");
                        }));
            }
        }
        #endregion

        private void ChangePlayButtonContent(MediaPlayerState state)
        {
            if (state== MediaPlayerState.Playing)
            {
                PlayButtonContent = "\uE769";
                //PlayButtonContent = Symbol.Pause;
            }
            else
            {
                PlayButtonContent = "\uE768";
                //PlayButtonContent = Symbol.Play;
            }
        }

        private void ChangeSong(int index)
        {
            Song = NowPlayingPlaylistManager.Current.GetSongItem(index);
            //ChangeCover();
        }

        private void PlaybackManager_MediaPlayerPositionChanged(TimeSpan position, TimeSpan duration)
        {
            CurrentTime = position;
            SliderValue = position.TotalSeconds;
            //TimeEnd = duration;
        }

        private void PlaybackManager_MediaPlayerMediaOpened(TimeSpan duration)
        {
            if (!_timer.IsEnabled)
            {
                StartTimer();
            }
            CurrentTime = TimeSpan.Zero;
            TimeEnd = duration;
            SliderValue = 0.0;
            SliderMaxValue = (int)Math.Round(duration.TotalSeconds - 0.5, MidpointRounding.AwayFromZero);
        }

        private void PlaybackManager_MediaPlayerMediaClosed()
        {
            StopTimer();
        }

        public void ChangeCoverUri(Uri cacheUri)
        {
            CoverUri = cacheUri;
        }

        //public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        //{
        //    PlaybackManager.MediaPlayerStateChanged += ChangePlayButtonContent;
        //    PlaybackManager.MediaPlayerTrackChanged += ChangeSong;
        //    PlaybackManager.MediaPlayerMediaOpened += PlaybackManager_MediaPlayerMediaOpened;
        //    PlaybackManager.MediaPlayerPositionChanged += PlaybackManager_MediaPlayerPositionChanged;
        //    StartTimer();
        //    Song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
        //    ChangeCover();
        //    //cover
        //    ChangePlayButtonContent(PlaybackManager.Current.PlayerState);
        //    return Task.CompletedTask;
        //}

        //public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        //{
        //    PlaybackManager.MediaPlayerStateChanged -= ChangePlayButtonContent;
        //    PlaybackManager.MediaPlayerTrackChanged -= ChangeSong;
        //    PlaybackManager.MediaPlayerMediaOpened -= PlaybackManager_MediaPlayerMediaOpened;
        //    PlaybackManager.MediaPlayerPositionChanged -= PlaybackManager_MediaPlayerPositionChanged;
        //    StopTimer();
        //    if (suspending)
        //    {

        //    }
        //    return base.OnNavigatedFromAsync(state, suspending);
        //}

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
                position = PlaybackManager.Current.CurrentPlayer.Position;
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
