using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Common;
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
            App.Current.LeavingBackground += Current_LeavingBackground;
            App.Current.EnteredBackground += Current_EnteredBackground;
            ViewModelLocator vml = new ViewModelLocator();
            PlayerVM = vml.PlayerVM;
            QueueVM = vml.QueueVM;
        }

        private void Current_EnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
            PlaybackService.MediaPlayerMediaOpened -= PlaybackService_MediaPlayerMediaOpened;
            StopTimer();
        }

        private void Current_LeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
            StartTimer();
        }

        public PlayerViewModelBase PlayerVM { get; set; }
        public QueueViewModelBase QueueVM { get; set; }

        #region Properties

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

        private bool isVolumeControlVisible = false;
        public bool IsVolumeControlVisible
        {
            get { return isVolumeControlVisible; }
            set { Set(ref isVolumeControlVisible, value); }
        }

        private int flipViewSelectedIndex = 0;
        public int FlipViewSelectedIndex
        {
            get{ return flipViewSelectedIndex; }
            set
            {
                if (flipViewSelectedIndex != value)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.FlipViewSelectedIndex, value);
                }
                Set(ref flipViewSelectedIndex, value);
            }
        }

        #endregion

        #region Commands

        public async void RateSong(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int rating = Int32.Parse(button.Tag.ToString());
            await QueueVM.RateSong(rating);
        }

        public void ShowVolumeControl()
        {
            IsVolumeControlVisible = !IsVolumeControlVisible;
        }

        #endregion

        #region Image
        private double x, y;
        private bool isPressed = false;

        public void Image_Pressed(object sender, PointerRoutedEventArgs e)
        {
            isPressed = true;
            var a = e.GetCurrentPoint(null);
            x = a.Position.X;
            y = a.Position.Y;
        }

        public void Image_Released(object sender, PointerRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Image_Released");
            isPressed = false;
        }

        public void Image_Exited(object sender, PointerRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Image_Exited");
            var a = e.GetCurrentPoint(null);
            if (Math.Abs(x - a.Position.X) > 50)
            {
                if (x - a.Position.X > 0) PlayerVM.Next();
                else PlayerVM.Previous();
            }
            isPressed = false;
        }

        public void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            PlayerVM.Play();
        }
        private double iMGX;
        public double IMGX
        {
            get { return iMGX; }
            set { Set(ref iMGX, value); }
        }
        private double iMGY;
        public double IMGY
        {
            get { return iMGY; }
            set { Set(ref iMGY, value); }
        }
        public void CoverImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var img = sender as Image;
            
            if (isPressed)
            {
                var a = e.GetCurrentPoint(null);
                var delta = a.Position.X - x;
                if (Math.Abs(delta) < 100)
                {
                    IMGX = delta;
                }
                else
                {
                    isPressed = false;
                    if (delta > 0)
                    {
                        PlayerVM.Next();
                    }
                    else
                    {
                        PlayerVM.Previous();
                    }
                }
                //IMGY = a.Position.Y;

                e.Handled = true;
            }
        }
        #endregion

        private void PlaybackService_MediaPlayerPositionChanged(TimeSpan position, TimeSpan duration)
        {
            CurrentTime = position;
            SliderValue = position.TotalSeconds;
            //TimeEnd = duration;
        }

        private async void PlaybackService_MediaPlayerMediaOpened()
        {
            await Task.Delay(400);
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
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
            });
        }

        private void PlaybackService_MediaPlayerMediaClosed()
        {
            StopTimer();
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

            App.OnNavigatedToNewView(false);
            PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
            FlipViewSelectedIndex = (int)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.FlipViewSelectedIndex);
            StartTimer();
            
            TimeEnd = QueueVM.CurrentSong.Duration;
            SliderValue = 0.0;
            SliderMaxValue = (int)Math.Round(QueueVM.CurrentSong.Duration.TotalSeconds - 0.5, MidpointRounding.AwayFromZero);
            if (mode == NavigationMode.New || mode == NavigationMode.Forward)
            {
                TelemetryAdapter.TrackPageView(this.GetType().ToString());
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            System.Diagnostics.Debug.WriteLine("NowPlayingVM OnNavigatedFromAsync");

            //App.ChangeBottomPlayerVisibility(true);
            PlaybackService.MediaPlayerMediaOpened -= PlaybackService_MediaPlayerMediaOpened;
            StopTimer();
            
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
