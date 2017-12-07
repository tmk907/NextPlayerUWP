using NextPlayerUWP.Common;
using NextPlayerUWP.Messages;
using NextPlayerUWP.Messages.Hub;
using NextPlayerUWP.Playback;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
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
            //App.Current.LeavingBackground += Current_LeavingBackground;
            //App.Current.EnteredBackground += Current_EnteredBackground;
            ViewModelLocator vml = new ViewModelLocator();
            PlayerVM = vml.PlayerVM;
            QueueVM = vml.QueueVM;
            lyricsPanelVM = vml.LyricsPanelVM;
            song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            songDurationType = ApplicationSettingsHelper.ReadSettingsValue<string>(SettingsKeys.SongDurationType);
        }

        //private void Current_EnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("NowPlayingVM EnteringBG");
        //    PlaybackService.MediaPlayerMediaOpened -= PlaybackService_MediaPlayerMediaOpened;
        //    PlaybackService.MediaPlayerTrackChanged -= TrackChanged;
        //    StopTimer();
        //}

        //private void Current_LeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("NowPlayingVM LeavingBG");
        //    leavedBG = true;
        //    PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
        //    PlaybackService.MediaPlayerTrackChanged += TrackChanged;
        //    StartTimer();            
        //}

        public PlayerViewModelBase PlayerVM { get; set; }
        public QueueViewModelBase QueueVM { get; set; }
        LyricsPanelViewModel lyricsPanelVM;
        SongItem song;

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

        private string songDurationType;
        private TimeSpan songDuration = TimeSpan.Zero;
        private TimeSpan playlistDuration = TimeSpan.Zero;

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

        private int pivotSelectedIndex = 0;
        public int PivotSelectedIndex
        {
            get { return pivotSelectedIndex; }
            set
            {
                Set(ref pivotSelectedIndex, value);
                if (value == 2)
                {
                    ShowNowPlayingListButtons = true;
                }
                else
                {
                    ShowNowPlayingListButtons = false;
                }
            }
        }

        private bool showNowPlayingListButtons = false;
        public bool ShowNowPlayingListButtons
        {
            get { return showNowPlayingListButtons; }
            set { Set(ref showNowPlayingListButtons, value); }
        }

        private bool showAlbumArtInBackground = false;
        public bool ShowAlbumArtInBackground
        {
            get { return showAlbumArtInBackground; }
            set { Set(ref showAlbumArtInBackground, value); }
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

        #endregion

        #region Image
        private double x, y;
        private bool isPressed = false;

        public void Image_Pressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            isPressed = true;
            var a = e.GetCurrentPoint(null);
            x = a.Position.X;
            y = a.Position.Y;
        }

        public void Image_Released(object sender, PointerRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Image_Released");
            e.Handled = true;
            isPressed = false;
        }

        public void Image_Exited(object sender, PointerRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Image_Exited");
            e.Handled = true;
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
            e.Handled = true;
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

        private async void TrackChanged(int index)
        {
            int prevId = song.SongId;
            song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            await WindowWrapper.Current().Dispatcher.DispatchAsync(async () =>
            {
                if (song.SongId != prevId)
                {
                    await lyricsPanelVM.ChangeLyrics(song);
                    ArtistSearch = song.Artist;
                    TitleSearch = song.Title;
                };
            });
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

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            System.Diagnostics.Debug.WriteLine("NowPlayingVM OnNavigatedToAsync");

            App.OnNavigatedToNewView(false);
            MessageHub.Instance.Publish<PageNavigated>(new PageNavigated() { NavigatedTo = true, PageType = PageNavigatedType.NowPlaying });
            PlaybackService.MediaPlayerMediaOpened += PlaybackService_MediaPlayerMediaOpened;
            PlaybackService.MediaPlayerTrackChanged += TrackChanged;
            FlipViewSelectedIndex = (int)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.FlipViewSelectedIndex);
            StartTimer();
            song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            TimeEnd = song.Duration;
            SliderValue = 0.0;
            SliderMaxValue = (int)Math.Round(song.Duration.TotalSeconds - 0.5, MidpointRounding.AwayFromZero);
            if (mode == NavigationMode.New || mode == NavigationMode.Forward)
            {
                TelemetryAdapter.TrackPageView(this.GetType().ToString());
            }
            //ShowAlbumArtInBackground = ApplicationSettingsHelper.ReadData<bool>(SettingsKeys.AlbumArtInBackground);
            if (mode != NavigationMode.Back)
            {
                PivotSelectedIndex = 0;
            }
            else
            {
                ShowNowPlayingListButtons = (pivotSelectedIndex == 2);
            }
            ArtistSearch = song.Artist;
            TitleSearch = song.Title;
            await lyricsPanelVM.ChangeLyrics(song);
            //await Task.CompletedTask;
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            System.Diagnostics.Debug.WriteLine("NowPlayingVM OnNavigatedFromAsync");

            MessageHub.Instance.Publish<PageNavigated>(new PageNavigated() { NavigatedTo = false, PageType = PageNavigatedType.NowPlaying });
            //App.ChangeBottomPlayerVisibility(true);
            PlaybackService.MediaPlayerMediaOpened -= PlaybackService_MediaPlayerMediaOpened;
            PlaybackService.MediaPlayerTrackChanged -= TrackChanged;
            StopTimer();
            
            await Task.CompletedTask;
        }

        public void GoToNowPlayingPlaylist()
        {
            NavigationService.Navigate(AppPages.Pages.NowPlayingPlaylist);
        }

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
    }
}
