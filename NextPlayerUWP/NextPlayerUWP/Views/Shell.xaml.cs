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

        public Shell()
        {
            Logger2.DebugWrite("Shell()", "");
            Instance = this;
            InitializeComponent();
            this.DataContext = ShellVM;
            this.Loaded += Shell_Loaded;
            this.Unloaded += Shell_Unloaded;
            ShellVM.RefreshMenuButtons();

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
            ApplyTheme(ThemeHelper.IsLightTheme);

            LoadControls();

            System.Diagnostics.Debug.WriteLine("Shell.Loaded() End");
        }

        private async Task LoadControls()
        {
            System.Diagnostics.Debug.WriteLine("Shell.LoadControls()");
            //await App.PlayerInitializer.InitMain();
            await Task.Delay(800);
            FindName(nameof(BottomPlayerWrapper));
            if (DeviceFamilyHelper.IsDesktop())
            {
                FindName(nameof(RightPanelWrapper));
            }
            System.Diagnostics.Debug.WriteLine("Shell.LoadControls() End");
        }

        private void Shell_Unloaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Shell UnLoaded()");
            MessageHub.Instance.UnSubscribe(tokenMenu);
            MessageHub.Instance.UnSubscribe(tokenNotification);
            MessageHub.Instance.UnSubscribe(tokenTheme);
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
//                Menu.NavigationService.Navigate(App.Pages.NowPlaying);
//#endif
//            }
//            else
//            {
//                Menu.NavigationService.Navigate(App.Pages.NowPlaying);
//            }
//        }

    }
}
