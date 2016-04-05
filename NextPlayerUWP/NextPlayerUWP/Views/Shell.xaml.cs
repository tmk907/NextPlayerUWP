﻿using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Constants;
using System;
using System.Threading.Tasks;
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
        public Shell(INavigationService  navigationService)
        {
            this.InitializeComponent();
            this.Loaded += LoadSlider;
            Menu.NavigationService = navigationService;
            App.AppThemeChanged += App_AppThemeChanged;
            ReviewReminder();
        }

        private void App_AppThemeChanged(bool isLight)
        {
            if (isLight)
            {
                this.RequestedTheme = ElementTheme.Light;
            }
            else
            {
                this.RequestedTheme = ElementTheme.Dark;
            }
        }

        #region Slider 
        private void LoadSlider(object sender, RoutedEventArgs e)
        {
            PointerEventHandler pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            timeslider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
            timeslider.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }

        void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            NowPlayingViewModel viewModel = (NowPlayingViewModel)BottomPlayerGrid.DataContext;
            viewModel.sliderpressed = true;
        }

        void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            NowPlayingViewModel viewModel = (NowPlayingViewModel)BottomPlayerGrid.DataContext;
            viewModel.sliderpressed = false;
            Common.PlaybackManager.Current.SendMessage(AppConstants.Position, TimeSpan.FromSeconds(timeslider.Value));
            //viewModel.SendMessage(AppConstants.Position, TimeSpan.FromSeconds(timeslider.Value));
        }

        void progressbar_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            NowPlayingViewModel viewModel = (NowPlayingViewModel)BottomPlayerGrid.DataContext;
            if (!viewModel.sliderpressed)
            {
                Common.PlaybackManager.Current.SendMessage(AppConstants.Position, TimeSpan.FromSeconds(e.NewValue));
                //viewModel.SendMessage(AppConstants.Position, TimeSpan.FromSeconds(e.NewValue));
            }
        }
        #endregion

        private async Task ReviewReminder()
        {
            await Task.Delay(2000);
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
                if (isReviewed >= 0 && isReviewed < 8 && TimeSpan.FromDays(1) <= elapsed)//!!!!!!!!! <=
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
            }}
        }
    }
}
