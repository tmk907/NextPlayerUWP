using NextPlayerWinUI.Common;
using NextPlayerWinUIDataLayer.Diagnostics;
using NextPlayerWinUIDataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;

namespace NextPlayerWinUI.ViewModels
{
    public class BottomPlayerViewModel : NextPlayerWinUI.Navigation.ViewModelBase
    {
        public BottomPlayerViewModel()
        {
            Logger2.DebugWrite("BottomPlayerViewModel()", "");
            _timer = new DispatcherTimer();
            SetupTimer();
            //App.Current.Resuming += Current_Resuming;
            //App.Current.Suspending += Current_Suspending;
            NowPlayingPlaylistManager.Current.SetDispatcher(App.Dispatcher);
            //PlaybackService.Instance.Initialize().ConfigureAwait(false);
            ViewModelLocator vml = new ViewModelLocator();
            PlayerVM = vml.PlayerVM;
            QueueVM = vml.QueueVM;
            PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
            songDurationType = ApplicationSettingsHelper.ReadSettingsValue<string>(SettingsKeys.SongDurationType);

            if (App.Window != null)
            {
                App.Window.SizeChanged += OnWindowSizeChanged;
            }
        }

        private void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            Logger2.DebugWrite("BottomPlayerViewModel()", "Suspending");
            //NextPlayerWinUIDataLayer.Diagnostics.Logger.Save("BottomPlayerVM Suspending");
            //NextPlayerWinUIDataLayer.Diagnostics.Logger.SaveToFile();
            PlaybackService.MediaPlayerMediaOpened -= PlaybackService_MediaPlayerMediaOpened;
            System.Diagnostics.Debug.WriteLine("BottomPlayerVM Suspending");
            StopTimer();
            
        }

        private void Current_Resuming(object sender, object e)
        {
            Logger2.DebugWrite("BottomPlayerViewModel()", "Resuming");
            //NextPlayerWinUIDataLayer.Diagnostics.Logger.Save("BottomPlayerVM Resuming");
            //NextPlayerWinUIDataLayer.Diagnostics.Logger.SaveToFile();
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
            set { SetProperty(ref bottomPlayerVisibility, value); }
        }

        private bool isNowPlayingDesktopViewActive = false;
        public bool IsNowPlayingDesktopViewActive
        {
            get { return isNowPlayingDesktopViewActive; }
            set { SetProperty(ref isNowPlayingDesktopViewActive, value); }
        }

        #region Properties

        private double sliderMaxValue = 0.0;
        public double SliderMaxValue
        {
            get { return sliderMaxValue; }
            set { SetProperty(ref sliderMaxValue, value); }
        }

        private double sliderValue = 0.0;
        public double SliderValue
        {
            get { return sliderValue; }
            set { SetProperty(ref sliderValue, value); }
        }

        private TimeSpan currentTime = TimeSpan.Zero;
        public TimeSpan CurrentTime
        {
            get { return currentTime; }
            set { SetProperty(ref currentTime, value); }
        }

        private string songDurationType;
        private TimeSpan songDuration = TimeSpan.Zero;
        private TimeSpan playlistDuration = TimeSpan.Zero;

        private TimeSpan timeEnd = TimeSpan.Zero;
        public TimeSpan TimeEnd
        {
            get { return timeEnd; }
            set { SetProperty(ref timeEnd, value); }
        }

        private string showHideSliderButtonContent = "\uE972";
        public string ShowHideSliderButtonContent
        {
            get { return showHideSliderButtonContent; }
            set { SetProperty(ref showHideSliderButtonContent, value); }
        }

        private bool isSliderVisible = false;
        public bool IsSliderVisible
        {
            get { return isSliderVisible; }
            set
            {
                SetProperty(ref isSliderVisible, value);
                ShowHideSliderButtonContent = value ? "\uE972" : "\uE971";
            }
        }
        #endregion

        public void ChangeTimeEndType()
        {
            if (songDurationType == SettingsKeys.SongDurationTotal)
            {
                songDurationType = SettingsKeys.SongDurationRemaining;
                TimeEnd = songDuration - currentTime;
            }
            else if (songDurationType == SettingsKeys.SongDurationRemaining)
            {
                songDurationType = SettingsKeys.SongDurationTotal;
                TimeEnd = songDuration;
            }
            else if (songDurationType == SettingsKeys.SongDurationPlaylistRemaining)
            {
                songDurationType = SettingsKeys.SongDurationTotal;
            }
        }

        private async void PlaybackService_MediaPlayerMediaOpened()
        {
            await Task.Delay(400);
            App.Dispatcher.Dispatch(() =>
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
                songDuration = TimeSpan.FromSeconds(Math.Truncate(duration.TotalSeconds));
                TimeEnd = songDuration;

                SliderValue = 0.0;
                if (duration.TotalSeconds > 1)
                {
                    SliderMaxValue = Math.Truncate(duration.TotalSeconds - 1);
                }
                else
                {
                    SliderMaxValue = 0.0;
                }
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


        private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            if (args.Size.Width >= normal)
            {
                IsSliderVisible = false;
            }
        }

        #region Slider Timer

        public bool sliderpressed = false;
        private DispatcherTimer _timer;

        private void SetupTimer()
        {
            _timer.Interval = TimeSpan.FromMilliseconds(250);
            //_timer.Tick += _timer_Tick;
        }

        private TimeSpan position = TimeSpan.Zero;

        private void _timer_Tick(object sender, object e)
        {
            if (!sliderpressed)
            {
                position = PlaybackService.Instance.Position;
                SliderValue = Math.Truncate(position.TotalSeconds);
                CurrentTime = TimeSpan.FromSeconds(sliderValue);
                switch (songDurationType)
                {
                    case SettingsKeys.SongDurationTotal:
                        break;
                    case SettingsKeys.SongDurationRemaining:
                        TimeEnd = songDuration - currentTime;
                        break;
                    case SettingsKeys.SongDurationPlaylistRemaining:
                        TimeEnd = playlistDuration - currentTime;
                        break;
                    default:
                        break;
                }
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
        
        #endregion

    }
}
