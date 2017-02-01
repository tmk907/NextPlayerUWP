using GalaSoft.MvvmLight.Messaging;
using NextPlayerUWP.Common;
using NextPlayerUWP.Messages;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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

        PointerEventHandler pointerpressedhandler;
        PointerEventHandler pointerreleasedhandler;

        public Shell()
        {
            Instance = this;
            InitializeComponent();
            this.DataContext = ShellVM;
            this.Loaded += Shell_Loaded;
            this.Unloaded += Shell_Unloaded;

            pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);

            ShellVM.RefreshMenuButtons();
            //SetNavigationService(navigationService);
            BPViewModel = (BottomPlayerViewModel)BottomPlayerGrid.DataContext;
            if (DeviceFamilyHelper.IsDesktop())
            {
                ((RightPanelControl)(RightPanel ?? FindName("RightPanel"))).Visibility = Visibility.Visible;
            }

            ReviewReminder();
            //SendLogs();
        }

        private void Shell_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSliderEvents();
            Messenger.Default.Register<NotificationMessage<InAppNotification>>(this, (message) =>
            {
                var content = message.Content;
                PopupNotification.ShowNotification(content.FirstTextLine, content.SecondTextLine);
            });
            Messenger.Default.Register<NotificationMessage<ThemeChange>>(this, (message) =>
            {
                AppThemeChanged(message.Content.IsLightTheme);
            });
            Messenger.Default.Register<NotificationMessage<MenuButtonSelected>>(this, (message) =>
            {
                PressMenuButton(message.Content.Nr);
            });
        }

        private void Shell_Unloaded(object sender, RoutedEventArgs e)
        {
            UnloadSliderEvents();
            Messenger.Default.Unregister(this);
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

        private void PressMenuButton(int nr)
        {
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

        private void AppThemeChanged(bool isLight)
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

        private async Task SendLogs()
        {            
            await Logger2.Current.SendLogs();
        }

        #region Slider 

        private void LoadSliderEvents()
        {
            timeslider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);
            timeslider.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }

        private void UnloadSliderEvents()
        {
            timeslider.RemoveHandler(Control.PointerPressedEvent, pointerpressedhandler);
            timeslider.RemoveHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler);
        }

        private void BottomSlider_Loaded(object sender, RoutedEventArgs e)
        {
            durationSliderBottom.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);
            durationSliderBottom.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }


        private void BottomSlider_Unloaded(object sender, RoutedEventArgs e)
        {
            durationSliderBottom.RemoveHandler(Control.PointerPressedEvent, pointerpressedhandler);
            durationSliderBottom.RemoveHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler);
        }

        private void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            BPViewModel.sliderpressed = true;
        }

        private void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            BPViewModel.sliderpressed = false;
            PlaybackService.Instance.Position = TimeSpan.FromSeconds(((Slider)sender).Value);
        }
        
        #endregion

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

        private void GoToNowPlaying(object sender, TappedRoutedEventArgs e)
        {
            if (DeviceFamilyHelper.IsDesktop())
            {
#if DEBUG
                Menu.NavigationService.Navigate(App.Pages.NowPlaying);
#endif
            }
            else
            {
                Menu.NavigationService.Navigate(App.Pages.NowPlaying);
            }
        }

        private async Task AutoUpdateLibrary()
        {
            await Task.Delay(TimeSpan.FromMinutes(1));
            NextPlayerUWPDataLayer.Services.MediaImport mi = new NextPlayerUWPDataLayer.Services.MediaImport(App.FileFormatsHelper);
            Progress<string> progress = new Progress<string>(
                data =>
                {
                    var array = data.Split('|');
                }
            );
            await Task.Run(() => mi.UpdateDatabaseAsync(progress));
        }
    }
}
