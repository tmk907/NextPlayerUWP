using NextPlayerUWP.Common;
using NextPlayerUWP.Messages;
using NextPlayerUWP.Messages.Hub;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Threading.Tasks;
using Template10.Controls;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        public static Shell Instance { get; set; }
        public static HamburgerMenu HamburgerMenu => Instance.Menu;

        ShellViewModel ShellVM = new ShellViewModel();

        private Guid tokenMenu;
        private Guid tokenTheme;
        private Guid tokenNotification;
        private Guid tokenPageNavigated;

        private PlaybackTimer AdTimer;
        private TimeSpan AdVisibleDuration = TimeSpan.FromSeconds(70);

        public Shell()
        {
            Logger2.DebugWrite("Shell()", "");
            Instance = this;
            InitializeComponent();
            this.DataContext = ShellVM;
            this.Loaded += Shell_Loaded;
            this.Unloaded += Shell_Unloaded;
            ShellVM.RefreshMenuButtons();

            AdTimer = new PlaybackTimer();

            MigrateCredentialsAsync();
            ReviewReminder();
        }

        //~Shell()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}

        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Shell.Loaded() Start");
            tokenMenu = MessageHub.Instance.Subscribe<MenuButtonSelected>(OnMenuButtonMessage);
            tokenNotification = MessageHub.Instance.Subscribe<InAppNotification>(OnInAppNotificationMessage);
            tokenTheme = MessageHub.Instance.Subscribe<ThemeChange>(OnThemeChangeMessage);
            tokenPageNavigated = MessageHub.Instance.Subscribe<PageNavigated>(OnPageNavigatedMessage);
            ApplyTheme(ThemeHelper.IsLightTheme);

            if (App.CanLoadShellControls)
            {
                LoadControls();
            }

            System.Diagnostics.Debug.WriteLine("Shell.Loaded() End");
        }

        public void LoadControls()
        {
            System.Diagnostics.Debug.WriteLine("Shell.LoadControls()");
            FindName(nameof(BottomPlayerWrapper));
            if (DeviceFamilyHelper.IsDesktop())
            {
                FindName(nameof(RightPanelWrapper));
            }

            DateTime july1 = new DateTime(2017, 7, 1, 0, 0, 0);
            DateTime july31 = new DateTime(2017, 7, 31, 23, 59, 59);
            if (DateTime.Now.Ticks > july1.Ticks && DateTime.Now.Ticks < july31.Ticks)
            {

            }
            else
            {
                LoadAd(TimeSpan.FromSeconds(5));
            }
            System.Diagnostics.Debug.WriteLine("Shell.LoadControls() End");
        }

        private void Shell_Unloaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Shell UnLoaded()");
            MessageHub.Instance.UnSubscribe(tokenMenu);
            MessageHub.Instance.UnSubscribe(tokenNotification);
            MessageHub.Instance.UnSubscribe(tokenTheme);
            MessageHub.Instance.UnSubscribe(tokenPageNavigated);
            UnloadAd(false);
            AdTimer.TimerCancel();
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

        private void OnMenuButtonMessage(MenuButtonSelected msg)
        {
            PressMenuButton(msg.Nr);
        }

        private void OnThemeChangeMessage(ThemeChange msg)
        {
            ApplyTheme(msg.IsLightTheme);
        }

        private void OnInAppNotificationMessage(InAppNotification msg)
        {
            PopupNotification.ShowNotification(msg.FirstTextLine, msg.SecondTextLine);
        }

        private void PressMenuButton(int nr)
        {
            System.Diagnostics.Debug.WriteLine("PressMenuButton {0}", nr);
            if (nr == 0)
            {
                HamburgerMenu.Selected = HamburgerMenu.SecondaryButtons[0];
            }
            else
            {
                nr--;
                if (nr >= 0 && nr < HamburgerMenu.PrimaryButtons.Count)
                {
                    HamburgerMenu.Selected = HamburgerMenu.PrimaryButtons[nr];
                }
            }
        }

        private void ApplyTheme(bool isLight)
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

        #region Ads

        private async void LoadAd(TimeSpan delay)
        {
            if (!App.ShowAd)
            {
                UnloadAd(true);
                return;
            }
            if (delay != TimeSpan.Zero)
            {
                await Task.Delay(delay);
            }
            if (Microsoft.Toolkit.Uwp.NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable && !hideAd)
            {
                if (DeviceFamilyHelper.IsDesktop())
                {
                    FindName(nameof(AdWrapperDesktop));
                    if (FindName(nameof(DesktopAd)) == null) return;
#if DEBUG
                    //DesktopAd.ApplicationId = "9nblggh67n4f";
                    //DesktopAd.AdUnitId = "11684323";
                    DesktopAd.ApplicationId = "3f83fe91-d6be-434d-a0ae-7351c5a997f1";
                    DesktopAd.AdUnitId = "test";
#else
                    DesktopAd.ApplicationId = "9nblggh67n4f";
                    DesktopAd.AdUnitId = "11684323";
#endif
                    AdWrapperDesktop.Visibility = Visibility.Visible;
                    DesktopAd.Visibility = Visibility.Visible;
                }
                else
                {
                    FindName(nameof(AdWrapperMobile));
                    if (FindName(nameof(MobileAd)) == null) return;
#if DEBUG
                    //MobileAd.ApplicationId = "9nblggh67n4f";
                    //MobileAd.AdUnitId = "11684325";
                    MobileAd.ApplicationId = "3f83fe91-d6be-434d-a0ae-7351c5a997f1";
                    MobileAd.AdUnitId = "test";
#else
                    MobileAd.ApplicationId = "9nblggh67n4f";
                    MobileAd.AdUnitId = "11684325";
#endif
                    AdWrapperMobile.Visibility = Visibility.Visible;
                    MobileAd.Visibility = Visibility.Visible;
                }
                AdTimer.SetTimerWithAction(AdVisibleDuration, () =>
                {
                    UnloadAd(true);
                });
            }
        }

        private bool AdWasDisplayed = false;
        private bool hideAd = false;
        private void UnloadAd(bool fromTimer)
        {
            Template10.Common.WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                if (AdWrapperMobile != null)
                {
                    if (FindName(nameof(MobileAd)) != null)
                    {
                        MobileAd.Dispose();
                        AdWrapperMobile.Children.Remove(MobileAd);
                        MobileAd = null;
                    }
                    AdWrapperMobile.Visibility = Visibility.Collapsed;
                }
                if (AdWrapperDesktop != null)
                {
                    if (FindName(nameof(DesktopAd)) != null)
                    {
                        DesktopAd.Dispose();
                        AdWrapperDesktop.Children.Remove(DesktopAd);
                        DesktopAd = null;
                    }
                    AdWrapperDesktop.Visibility = Visibility.Collapsed;
                }
                if (fromTimer)
                {
                    AdWasDisplayed = true;
                    App.ShowAd = false;
                }
            });
        }

        private void DesktopAd_AdRefreshed(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Ad refreshed");
            TelemetryAdapter.TrackEvent("AdRefreshedDesktop");
        }

        private void DesktopAd_ErrorOccurred(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Ad error {0} {1}", e.ErrorCode.ToString(), e.ErrorMessage);
            TelemetryAdapter.TrackEvent("AdErrorDesktop" + e.ErrorMessage);
            if (e.ErrorMessage == "NoAdAvailable")
            {
                UnloadAd(true);
                AdTimer.TimerCancel();
            }
        }

        private void DesktopAd_IsEngagedChanged(object sender, RoutedEventArgs e)
        {
            UnloadAd(true);
            AdTimer.TimerCancel();
            TelemetryAdapter.TrackEvent("AdClickedDesktop");
        }

        private void MobileAd_IsEngagedChanged(object sender, RoutedEventArgs e)
        {
            UnloadAd(true);
            AdTimer.TimerCancel();
            TelemetryAdapter.TrackEvent("AdClickedMobile");
        }

        private void MobileAd_AdRefreshed(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Ad refreshed");
            TelemetryAdapter.TrackEvent("AdRefreshedMobile");
        }

        private void MobileAd_ErrorOccurred(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Ad error {0} {1}", e.ErrorCode.ToString(), e.ErrorMessage);
            TelemetryAdapter.TrackEvent("AdErrorMobile" + e.ErrorMessage);
            if (e.ErrorMessage == "NoAdAvailable")
            {
                UnloadAd(true);
                AdTimer.TimerCancel();
            }
        }

        private void OnPageNavigatedMessage(PageNavigated message)
        {
            if (message.NavigatedTo)
            {
                if (message.PageType == PageNavigatedType.NowPlayingDesktop ||
                    message.PageType == PageNavigatedType.NowPlaying ||
                    message.PageType == PageNavigatedType.TagsEditor)
                {
                    hideAd = true;
                    UnloadAd(false);
                }
            }
            else
            {
                if (message.PageType == PageNavigatedType.NowPlayingDesktop ||
                    message.PageType == PageNavigatedType.NowPlaying ||
                    message.PageType == PageNavigatedType.TagsEditor)
                {
                    if (!AdWasDisplayed)
                    {
                        hideAd = false;
                        LoadAd(TimeSpan.Zero);
                    }
                }
            }
        }

        #endregion

        private async Task MigrateCredentialsAsync()
        {
            if (!String.IsNullOrEmpty(ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LfmLogin) as string))
            {
                var accounts = await DatabaseManager.Current.GetAllCloudAccountsAsync();

                CredentialLockerService dropbox = new CredentialLockerService(CredentialLockerService.DropboxVault);
                CredentialLockerService pcloud = new CredentialLockerService(CredentialLockerService.PCloudVault);

                foreach (var account in accounts)
                {
                    string token = await DatabaseManager.Current.GetCloudAccountTokenAsync(account.UserId);
                    switch (account.Type)
                    {
                        case NextPlayerUWPDataLayer.CloudStorage.CloudStorageType.Dropbox:
                            dropbox.AddCredentials(account.UserId, token);
                            break;
                        case NextPlayerUWPDataLayer.CloudStorage.CloudStorageType.OneDrive:
                            break;
                        case NextPlayerUWPDataLayer.CloudStorage.CloudStorageType.pCloud:
                            pcloud.AddCredentials(account.UserId, token);
                            break;
                        default:
                            break;
                    }
                    await DatabaseManager.Current.SaveCloudAccountTokenAsync(account.UserId, "");
                }

                string login = ApplicationSettingsHelper.ReadResetSettingsValue(SettingsKeys.LfmLogin) as string;
                string password = ApplicationSettingsHelper.ReadResetSettingsValue(SettingsKeys.LfmPassword) as string;
                string session = ApplicationSettingsHelper.ReadResetSettingsValue(SettingsKeys.LfmSessionKey) as string;
                CredentialLockerService locker = new CredentialLockerService(CredentialLockerService.LastFmVault);
                locker.AddCredentials(login, password);
                locker.AddCredentials(SettingsKeys.LfmSessionKey, session);
            }
        }

        private async Task ReviewReminder()
        {
            await Task.Delay(4000);
            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (!settings.Values.ContainsKey(SettingsKeys.IsReviewed))
            {
                settings.Values.Add(SettingsKeys.IsReviewed, 0);
                settings.Values.Add(SettingsKeys.LastReviewRemind, DateTime.Today.Ticks);
            }
            else
            {
                int isReviewed = Convert.ToInt32(settings.Values[SettingsKeys.IsReviewed]);
                long dateticks = (long)(settings.Values[SettingsKeys.LastReviewRemind]);
                TimeSpan elapsed = TimeSpan.FromTicks(DateTime.Today.Ticks - dateticks);
                if (isReviewed >= 0 && isReviewed < 8 && TimeSpan.FromDays(7) <= elapsed)//!!!!!!!!! <=
                {
                    settings.Values[SettingsKeys.LastReviewRemind] = DateTime.Today.Ticks;
                    settings.Values[SettingsKeys.IsReviewed] = isReviewed++;
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

       

        //        private void GoToNowPlaying(object sender, TappedRoutedEventArgs e)
        //        {
        //            if (DeviceFamilyHelper.IsDesktop())
        //            {
        //#if DEBUG
        //                Menu.NavigationService.Navigate(AppPages.Pages.NowPlaying);
        //#endif
        //            }
        //            else
        //            {
        //                Menu.NavigationService.Navigate(AppPages.Pages.NowPlaying);
        //            }
        //        }

    }
}
