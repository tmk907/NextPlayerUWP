//using Microsoft.Advertising.WinRT.UI;
using NextPlayerWinUI.Common;
using NextPlayerWinUI.Messages;
using NextPlayerWinUI.Messages.Hub;
using NextPlayerWinUI.ViewModels;
using NextPlayerWinUIDataLayer.Diagnostics;
using NextPlayerWinUIDataLayer.Helpers;
using NextPlayerWinUIDataLayer.Services;
using System;
using System.Threading.Tasks;
using NextPlayerWinUI.Controls;
using NextPlayerWinUI.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Linq;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerWinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        public static Shell Instance { get; set; }
        /// <summary>Raised before going back so a page can handle the request itself.</summary>
        public static event EventHandler<HandledEventArgs> BackRequested;

        ShellViewModel ShellVM = new ShellViewModel();

        private Guid tokenMenu;
        private Guid tokenTheme;
        private Guid tokenNotification;
        private Guid tokenPageNavigated;

        private INavigationService navigation;
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
            //LoadAd(TimeSpan.FromSeconds(5));
            System.Diagnostics.Debug.WriteLine("Shell.LoadControls() End");
        }

        private void Shell_Unloaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Shell UnLoaded()");
            MessageHub.Instance.UnSubscribe(tokenMenu);
            MessageHub.Instance.UnSubscribe(tokenNotification);
            MessageHub.Instance.UnSubscribe(tokenTheme);
            MessageHub.Instance.UnSubscribe(tokenPageNavigated);
            //UnloadAd(false);
        }
       
        public Shell(INavigationService navigationService) : this()
        {
            SetNavigationService(navigationService);
        }

        public void SetNavigationService(INavigationService navigationService)
        {
            navigation = navigationService;
            Menu.Content = navigation.Frame;
            Menu.FooterMenuItems.Add(new HamburgerMenuBuilder().MakeSettingsButton());
            navigation.Frame.Navigated += Frame_Navigated;
        }

        private void Menu_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer?.Tag is MenuDestination destination)
            {
                NavigateTo(destination);
            }
        }

        private void NavigateTo(MenuDestination destination)
        {
            navigation.Navigate(destination.PageType);
            if (destination.ClearHistory)
            {
                navigation.ClearHistory();
                Menu.IsBackEnabled = false;
            }
        }

        private void Menu_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) => GoBack();

        private void BackAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            GoBack();
            args.Handled = true;
        }

        private void GoBack()
        {
            var request = new HandledEventArgs();
            BackRequested?.Invoke(this, request);
            if (!request.Handled)
            {
                navigation.GoBack();
            }
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            Menu.IsBackEnabled = navigation.CanGoBack;
            Menu.SelectedItem = ShellVM.PrimaryButtons.Concat(Menu.FooterMenuItems.OfType<NavigationViewItem>())
                .FirstOrDefault(item => item.Tag is MenuDestination destination && destination.PageType == e.SourcePageType);
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
            var item = nr == 0
                ? Menu.FooterMenuItems.OfType<NavigationViewItem>().FirstOrDefault()
                : ShellVM.PrimaryButtons.ElementAtOrDefault(nr - 1);
            if (item?.Tag is MenuDestination destination)
            {
                NavigateTo(destination);
            }
        }

        private void ApplyTheme(bool isLight)
        {
            RequestedTheme = isLight ? ElementTheme.Light : ElementTheme.Dark;
        }

        #region Ads

        //        private async void LoadAd(TimeSpan delay)
        //        {
        //            DateTime july1 = new DateTime(2017, 7, 1, 0, 0, 0);
        //            DateTime july31 = new DateTime(2017, 7, 31, 23, 59, 59);
        //            if (DateTime.Now.Ticks > july1.Ticks && DateTime.Now.Ticks < july31.Ticks)
        //            {
        //                return;
        //            }
        //            if (!App.ShowAd)
        //            {
        //                UnloadAd(true);
        //                return;
        //            }
        //            if (delay != TimeSpan.Zero)
        //            {
        //                await Task.Delay(delay);
        //            }
        //            hideAd = hideAd || App.mobileNowPlaying;
        //            if (Microsoft.Toolkit.Uwp.NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable && !hideAd)
        //            {
        //                if (DeviceFamilyHelper.IsDesktop())
        //                {
        //                    FindName(nameof(AdWrapperDesktop));
        //                    AdControl DesktopAd = new AdControl();
        //                    DesktopAd.Height = 90;
        //                    DesktopAd.Width = 728;
        //                    DesktopAd.HorizontalAlignment = HorizontalAlignment.Left;
        //                    DesktopAd.VerticalAlignment = VerticalAlignment.Top;
        //                    DesktopAd.AdRefreshed += DesktopAd_AdRefreshed;
        //                    DesktopAd.ErrorOccurred += DesktopAd_ErrorOccurred;
        //                    DesktopAd.IsEngagedChanged += DesktopAd_IsEngagedChanged;
        //#if DEBUG
        //                    //DesktopAd.ApplicationId = "9nblggh67n4f";
        //                    //DesktopAd.AdUnitId = "11684323";
        //                    DesktopAd.ApplicationId = "3f83fe91-d6be-434d-a0ae-7351c5a997f1";
        //                    DesktopAd.AdUnitId = "test";
        //#else
        //                    DesktopAd.ApplicationId = "9nblggh67n4f";
        //                    DesktopAd.AdUnitId = "11684323";
        //#endif
        //                    AdWrapperDesktop.Visibility = Visibility.Visible;
        //                    DesktopAd.Visibility = Visibility.Visible;
        //                }
        //                else
        //                {
        //                    FindName(nameof(AdWrapperMobile));
        //                    AdControl MobileAd = new AdControl();
        //                    MobileAd.Height = 50;
        //                    MobileAd.Width = 320;
        //                    MobileAd.HorizontalAlignment = HorizontalAlignment.Left;
        //                    MobileAd.VerticalAlignment = VerticalAlignment.Top;
        //                    MobileAd.AdRefreshed += MobileAd_AdRefreshed;
        //                    MobileAd.ErrorOccurred += MobileAd_ErrorOccurred;
        //                    MobileAd.IsEngagedChanged += MobileAd_IsEngagedChanged;
        //#if DEBUG
        //                    //MobileAd.ApplicationId = "9nblggh67n4f";
        //                    //MobileAd.AdUnitId = "11684325";
        //                    MobileAd.ApplicationId = "3f83fe91-d6be-434d-a0ae-7351c5a997f1";
        //                    MobileAd.AdUnitId = "test";
        //#else
        //                    MobileAd.ApplicationId = "9nblggh67n4f";
        //                    MobileAd.AdUnitId = "11684325";
        //#endif

        //                    AdWrapperMobile.Children.Add(MobileAd);
        //                    AdWrapperMobile.Visibility = Visibility.Visible;
        //                    MobileAd.Visibility = Visibility.Visible;
        //                }
        //                AdTimer.SetTimerWithAction(AdVisibleDuration, () =>
        //                {
        //                    UnloadAd(true);
        //                });
        //            }
        //        }

        //private bool AdWasDisplayed = false;
        //private bool hideAd = false;
        //        private void UnloadAd(bool fromTimer)
        //        {
        //            App.Dispatcher.Dispatch(() =>
        //            {
        //                AdTimer.TimerCancel();
        //                if (AdWrapperMobile != null && AdWrapperMobile.Children.Count > 0)
        //                {
        //                    var ad = AdWrapperMobile.Children[0] as AdControl;
        //                    ad.AdRefreshed -= MobileAd_AdRefreshed;
        //                    ad.ErrorOccurred -= MobileAd_ErrorOccurred;
        //                    ad.IsEngagedChanged -= MobileAd_IsEngagedChanged;
        //                    ad.Dispose();
        //                    AdWrapperMobile.Children.Remove(ad);
        //                    ad = null;
        //                    AdWrapperMobile.Visibility = Visibility.Collapsed;
        //                }
        //                if (AdWrapperDesktop != null && AdWrapperDesktop.Children.Count > 0)
        //                {
        //                    var ad = AdWrapperDesktop.Children[0] as AdControl;
        //                    ad.AdRefreshed -= DesktopAd_AdRefreshed;
        //                    ad.ErrorOccurred -= DesktopAd_ErrorOccurred;
        //                    ad.IsEngagedChanged -= DesktopAd_IsEngagedChanged;
        //                    ad.Dispose();
        //                    AdWrapperDesktop.Children.Remove(ad);
        //                    ad = null;
        //                    AdWrapperDesktop.Visibility = Visibility.Collapsed;
        //                }
        //                if (fromTimer)
        //                {
        //                    AdWasDisplayed = true;
        //                    App.ShowAd = false;
        //                }
        //            });
        //        }

        //        private void DesktopAd_AdRefreshed(object sender, RoutedEventArgs e)
        //        {
        //            System.Diagnostics.Debug.WriteLine("Ad refreshed");
        //            TelemetryAdapter.TrackEvent("AdRefreshedDesktop");
        //        }

        //        private void DesktopAd_ErrorOccurred(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        //        {
        //            System.Diagnostics.Debug.WriteLine("Ad error {0} {1}", e.ErrorCode.ToString(), e.ErrorMessage);
        //            TelemetryAdapter.TrackEvent("AdErrorDesktop" + e.ErrorMessage);
        //            if (e.ErrorMessage == "NoAdAvailable")
        //            {
        //                UnloadAd(true);
        //            }
        //        }

        //        private void DesktopAd_IsEngagedChanged(object sender, RoutedEventArgs e)
        //        {
        //            UnloadAd(true);
        //            TelemetryAdapter.TrackEvent("AdClickedDesktop");
        //        }

        //        private void MobileAd_IsEngagedChanged(object sender, RoutedEventArgs e)
        //        {
        //            UnloadAd(true);
        //            TelemetryAdapter.TrackEvent("AdClickedMobile");
        //        }

        //        private void MobileAd_AdRefreshed(object sender, RoutedEventArgs e)
        //        {
        //            System.Diagnostics.Debug.WriteLine("Ad refreshed");
        //            TelemetryAdapter.TrackEvent("AdRefreshedMobile");
        //        }

        //        private void MobileAd_ErrorOccurred(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        //        {
        //            System.Diagnostics.Debug.WriteLine("Ad error {0} {1}", e.ErrorCode.ToString(), e.ErrorMessage);
        //            TelemetryAdapter.TrackEvent("AdErrorMobile" + e.ErrorMessage);
        //            if (e.ErrorMessage == "NoAdAvailable")
        //            {
        //                UnloadAd(true);
        //            }
        //        }

        #endregion

        private void OnPageNavigatedMessage(PageNavigated message)
        {
            if (message.NavigatedTo)
            {
                if (message.PageType == PageNavigatedType.NowPlayingDesktop ||
                    message.PageType == PageNavigatedType.NowPlaying)
                {
                    //hideAd = true;
                    //UnloadAd(false);
                }
            }
            else
            {
                if (message.PageType == PageNavigatedType.NowPlayingDesktop ||
                    message.PageType == PageNavigatedType.NowPlaying)
                {
                    //if (!AdWasDisplayed)
                    //{
                    //    hideAd = false;
                    //    App.mobileNowPlaying = false;
                    //    LoadAd(TimeSpan.Zero);
                    //}
                }
            }
        }

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
                        case NextPlayerWinUIDataLayer.CloudStorage.CloudStorageType.Dropbox:
                            dropbox.AddCredentials(account.UserId, token);
                            break;
                        case NextPlayerWinUIDataLayer.CloudStorage.CloudStorageType.OneDrive:
                            break;
                        case NextPlayerWinUIDataLayer.CloudStorage.CloudStorageType.pCloud:
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
                    settings.Values[SettingsKeys.IsReviewed] = isReviewed + 1;
                    ResourceLoader loader = new ResourceLoader();

                    await new ContentDialog
                    {
                        XamlRoot = XamlRoot,
                        Title = loader.GetString("RateAppTitle"),
                        Content = loader.GetString("RateAppMsg"),
                        PrimaryButtonText = loader.GetString("Yes"),
                        CloseButtonText = loader.GetString("Later"),
                        DefaultButton = ContentDialogButton.Primary,
                    }.ShowAsync();
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
