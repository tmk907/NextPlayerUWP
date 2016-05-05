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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
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
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            SongCoverManager.CoverUriPrepared -= ChangeCoverUri;
            PlaybackManager.MediaPlayerStateChanged -= ChangePlayButtonContent;
            PlaybackManager.MediaPlayerTrackChanged -= ChangeSong;
            PlaybackManager.MediaPlayerMediaOpened -= PlaybackManager_MediaPlayerMediaOpened;
            PlaybackManager.MediaPlayerPositionChanged -= PlaybackManager_MediaPlayerPositionChanged;
            StopTimer();
        }

        private void Current_Resuming(object sender, object e)
        {
            SongCoverManager.CoverUriPrepared += ChangeCoverUri;
            PlaybackManager.MediaPlayerStateChanged += ChangePlayButtonContent;
            PlaybackManager.MediaPlayerTrackChanged += ChangeSong;
            PlaybackManager.MediaPlayerMediaOpened += PlaybackManager_MediaPlayerMediaOpened;
            PlaybackManager.MediaPlayerPositionChanged += PlaybackManager_MediaPlayerPositionChanged;
            if (PlaybackManager.Current.IsBackgroundTaskRunning())
            {
                StartTimer();
            }
            ChangePlayButtonContent(PlaybackManager.Current.PlayerState);
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
        #endregion

        #region Commands

        public void Play()
        {
            PlaybackManager.Current.Play();
        }

        public void Previous()
        {
            PlaybackManager.Current.Previous();
        }

        public void Next()
        {
            PlaybackManager.Current.Next();
        }

        public void ShuffleCommand()
        {
            ShuffleMode = Shuffle.Change();
            PlaybackManager.Current.SendMessage(AppConstants.Shuffle, "");
        }

        public void RepeatCommand()
        {
            RepeatMode = Repeat.Change();
            PlaybackManager.Current.SendMessage(AppConstants.Repeat, "");
        }

        public async void RateSong(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Song.Rating = Int32.Parse(button.Tag.ToString());
            await DatabaseManager.Current.UpdateRatingAsync(song.SongId, song.Rating).ConfigureAwait(false);
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

        private void ChangePlayButtonContent(MediaPlayerState state)
        {
            if (state == MediaPlayerState.Playing)
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

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            System.Diagnostics.Debug.WriteLine("NowPlayingVM OnNavigatedToAsync");
            App.ChangeBottomPlayerVisibility(false);
            SongCoverManager.CoverUriPrepared += ChangeCoverUri;
            PlaybackManager.MediaPlayerStateChanged += ChangePlayButtonContent;
            PlaybackManager.MediaPlayerTrackChanged += ChangeSong;
            PlaybackManager.MediaPlayerMediaOpened += PlaybackManager_MediaPlayerMediaOpened;
            PlaybackManager.MediaPlayerPositionChanged += PlaybackManager_MediaPlayerPositionChanged;
            if (PlaybackManager.Current.IsBackgroundTaskRunning())
            {
                StartTimer();
                ChangePlayButtonContent(PlaybackManager.Current.PlayerState);
            }
            else
            {
                ChangePlayButtonContent(MediaPlayerState.Paused);
            }

            Song =  NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            CoverUri = SongCoverManager.Instance.GetCurrent();
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
            PlaybackManager.MediaPlayerStateChanged -= ChangePlayButtonContent;
            PlaybackManager.MediaPlayerTrackChanged -= ChangeSong;
            PlaybackManager.MediaPlayerMediaOpened -= PlaybackManager_MediaPlayerMediaOpened;
            PlaybackManager.MediaPlayerPositionChanged -= PlaybackManager_MediaPlayerPositionChanged;
            StopTimer();
            if (suspending)
            {
                //state[nameof(ShuffleMode)] = ShuffleMode;
                //state[nameof(RepeatMode)] = RepeatMode;
                state[nameof(CoverUri)] = CoverUri.ToString();
            }
            await Task.CompletedTask;
        }

        
    }
}
