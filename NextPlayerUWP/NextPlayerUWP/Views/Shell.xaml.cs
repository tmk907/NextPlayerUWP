using NextPlayerUWP.Common;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Diagnostics;
using System;
using System.Threading.Tasks;
using Template10.Controls;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        BottomPlayerViewModel BPViewModel;

        public static Shell Instance { get; set; }
        public static HamburgerMenu HamburgerMenu => Instance.Menu;

        ShellViewModel ShellVM = new ShellViewModel();

        public Shell()
        {
            Instance = this;
            InitializeComponent();
            this.DataContext = ShellVM;
            this.Loaded += LoadSlider;
            //HamburgerMenu.HamburgerButtonVisibility = Visibility.Visible;
            //SetNavigationService(navigationService);
            App.AppThemeChanged += App_AppThemeChanged;
            BPViewModel = (BottomPlayerViewModel)BottomPlayerGrid.DataContext;
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop")
            {
                ((RightPanelControl)(RightPanel ?? FindName("RightPanel"))).Visibility = Visibility.Visible;
            }           
            ReviewReminder();
            SendLogs();
        }

        public Shell(INavigationService navigationService) : this()
        {
            SetNavigationService(navigationService);
        }

        public void SetNavigationService(INavigationService navigationService)
        {
            Menu.NavigationService = navigationService;
            if (App.Current.RequestedTheme == ApplicationTheme.Light)
            {
                HamburgerMenu.RequestedTheme = ElementTheme.Light;
            }
            else
            {
                HamburgerMenu.RequestedTheme = ElementTheme.Dark;
            }
            HamburgerMenu.RefreshStyles(App.Current.RequestedTheme);
            HamburgerMenu.IsFullScreen = false;
            HamburgerMenu.HamburgerButtonVisibility = Visibility.Visible;
        }

        private void App_AppThemeChanged(bool isLight)
        {
            if (isLight)
            {
                this.RequestedTheme = ElementTheme.Light;
                HamburgerMenu.RequestedTheme = ElementTheme.Light;
                HamburgerMenu.RefreshStyles(ApplicationTheme.Light);
            }
            else
            {
                this.RequestedTheme = ElementTheme.Dark;
                HamburgerMenu.RequestedTheme = ElementTheme.Dark;
                HamburgerMenu.RefreshStyles(ApplicationTheme.Dark);
            }
        }

        public void ChangeRightPanelVisibility(bool visible)
        {
            if (visible)
            {
                RightPanel.Visibility = Visibility.Visible;
            }
            else
            {
                RightPanel.Visibility = Visibility.Collapsed;
            }
        }

        public void ChangeBottomPlayerVisibility(bool visible)
        {
            if (visible)
            {
                BottomPlayerGrid.Visibility = Visibility.Visible;
            }
            else
            {
                BottomPlayerGrid.Visibility = Visibility.Collapsed;
            }
        }

        public void OnDesktopViewActiveChange(bool isActive)
        {
            ShellVM.IsNowPlayingDesktopViewActive = isActive;
        }

        private bool IsDesktop
        {
            get
            {
                return (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop");
            }
        }

        private async Task SendLogs()
        {            
            await Logger2.Current.SendLogs();
        }

        #region Slider 
        private void LoadSlider(object sender, RoutedEventArgs e)
        {
            PointerEventHandler pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            timeslider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
            timeslider.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }

        private void BottomSlider_Loaded(object sender, RoutedEventArgs e)
        {
            PointerEventHandler pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            durationSliderBottom.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
            durationSliderBottom.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }

        void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            BPViewModel.sliderpressed = true;
        }

        void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            BPViewModel.sliderpressed = false;
            PlaybackService.Instance.Position = TimeSpan.FromSeconds(((Slider)sender).Value);
        }
        
        #endregion

        private async Task ReviewReminder()
        {
            await Task.Delay(4000);
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (!settings.Values.ContainsKey(AppConstants.IsReviewed))
            {
                settings.Values.Add(AppConstants.IsReviewed, 0);
                settings.Values.Add(AppConstants.LastReviewRemind, DateTime.Today.Ticks);
            }
            else
            {
                int isReviewed = Convert.ToInt32(settings.Values[AppConstants.IsReviewed]);
                long dateticks = (long)(settings.Values[AppConstants.LastReviewRemind]);
                TimeSpan elapsed = TimeSpan.FromTicks(DateTime.Today.Ticks - dateticks);
                if (isReviewed >= 0 && isReviewed < 8 && TimeSpan.FromDays(7) <= elapsed)//!!!!!!!!! <=
                {
                    settings.Values[AppConstants.LastReviewRemind] = DateTime.Today.Ticks;
                    settings.Values[AppConstants.IsReviewed] = isReviewed++;
                    ResourceLoader loader = new ResourceLoader();

                    MessageDialog dialog = new MessageDialog(loader.GetString("RateAppMsg"));
                    dialog.Title = loader.GetString("RateAppTitle");
                    dialog.Commands.Add(new UICommand(loader.GetString("Yes")) { Id = 0 });
                    dialog.Commands.Add(new UICommand(loader.GetString("Later")) { Id = 1 });
                    dialog.DefaultCommandIndex = 0;
                    dialog.CancelCommandIndex = 1;

                    await dialog.ShowAsync();
                }
            }
        }

        private void GoToNowPlaying(object sender, TappedRoutedEventArgs e)
        {
            if (IsDesktop)
            {
#if DEBUG
                Menu.NavigationService.Navigate(App.Pages.NowPlaying);
                //Menu.NavigationService.Navigate(App.Pages.NowPlayingPlaylist);
#endif
            }
            else
            {
                Menu.NavigationService.Navigate(App.Pages.NowPlaying);
            }
        }

        private void NPButton_Tapped(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("npgo");

            if (IsDesktop)
            {
                Menu.NavigationService.Navigate(App.Pages.NowPlayingDesktop);
            }
            else
            {
                Menu.NavigationService.Navigate(App.Pages.NowPlayingPlaylist);
            }
        }
    }
}
