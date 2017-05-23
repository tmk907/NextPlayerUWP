using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Common;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class BottomPlayerViewModel : Template10.Mvvm.ViewModelBase
    {
        public BottomPlayerViewModel()
        {
            Logger2.DebugWrite("BottomPlayerViewModel()", "");
            _timer = new DispatcherTimer();
            SetupTimer();
            App.Current.Resuming += Current_Resuming;
            App.Current.Suspending += Current_Suspending;
            NowPlayingPlaylistManager.Current.SetDispatcher(WindowWrapper.Current().Dispatcher);
            //PlaybackService.Instance.Initialize().ConfigureAwait(false);
            ViewModelLocator vml = new ViewModelLocator();
            PlayerVM = vml.PlayerVM;
            QueueVM = vml.QueueVM;
            PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
            App.IsBottomPlayerVMCreated = true;

            var window = CoreApplication.GetCurrentView()?.CoreWindow;
            if (window != null)
            {
                window.SizeChanged += OnCoreWindowOnSizeChanged;
            }
            if (window.Bounds.Width >= normal)
            {
                
            }
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            Logger2.DebugWrite("BottomPlayerViewModel()", "Suspending");
            //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("BottomPlayerVM Suspending");
            //NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
            PlaybackService.MediaPlayerMediaOpened -= PlaybackService_MediaPlayerMediaOpened;
            System.Diagnostics.Debug.WriteLine("BottomPlayerVM Suspending");
            StopTimer();
            
        }

        private void Current_Resuming(object sender, object e)
        {
            Logger2.DebugWrite("BottomPlayerViewModel()", "Resuming");
            //NextPlayerUWPDataLayer.Diagnostics.Logger.Save("BottomPlayerVM Resuming");
            //NextPlayerUWPDataLayer.Diagnostics.Logger.SaveToFile();
            PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
            System.Diagnostics.Debug.WriteLine("BottomPlayerVM Resuming");
            StartTimer();
        }

        public PlayerViewModelBase PlayerVM { get; set; }
        public QueueViewModelBase QueueVM { get; set; }

        private bool bottomPlayerVisibility = true;
        public bool BottomPlayerVisibility
        {
            get { return bottomPlayerVisibility; }
            set { Set(ref bottomPlayerVisibility, value); }
        }

        private bool isNowPlayingDesktopViewActive = false;
        public bool IsNowPlayingDesktopViewActive
        {
            get { return isNowPlayingDesktopViewActive; }
            set { Set(ref isNowPlayingDesktopViewActive, value); }
        }

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

        private string showHideSliderButtonContent = "\uE972";
        public string ShowHideSliderButtonContent
        {
            get { return showHideSliderButtonContent; }
            set { Set(ref showHideSliderButtonContent, value); }
        }

        private bool isSliderVisible = false;
        public bool IsSliderVisible
        {
            get { return isSliderVisible; }
            set
            {
                Set(ref isSliderVisible, value);
                ShowHideSliderButtonContent = value ? "\uE972" : "\uE971";
            }
        }
        #endregion

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

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            Logger2.DebugWrite("BottomPlayerViewModel()", "OnNavigatedToAsync");
            PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
            StartTimer();
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            Logger2.DebugWrite("BottomPlayerViewModel()", "OnNavigatedFromAsync");
            PlaybackService.MediaPlayerMediaOpened -= PlaybackService_MediaPlayerMediaOpened;
            StopTimer();
            
            await Task.CompletedTask;
        }

        public void ShowHideSlider()
        {
            IsSliderVisible = !IsSliderVisible;
        }

        int compact = 0;
        int narrow = 500;
        int normal = 720;
        int wide = 1008;


        private void OnCoreWindowOnSizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            if (args.Size.Width >= wide)
            {
                IsSliderVisible = false;
            }
            else if (args.Size.Width >= normal)
            {
                IsSliderVisible = false;
            }
            else if (args.Size.Width >= narrow)
            {
            }
            else if (args.Size.Width >= compact)
            {
            }
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
