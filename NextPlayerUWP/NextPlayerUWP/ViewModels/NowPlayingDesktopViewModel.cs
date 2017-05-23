﻿using NextPlayerUWP.Common;
using NextPlayerUWP.Messages;
using NextPlayerUWP.Messages.Hub;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class NowPlayingDesktopViewModel : Template10.Mvvm.ViewModelBase
    {
        public NowPlayingDesktopViewModel()
        {
            ViewModelLocator vml = new ViewModelLocator();
            QueueVM = vml.QueueVM;
        }

        public QueueViewModelBase QueueVM { get; set; }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> pageState, bool suspending)
        {
            //App.ChangeRightPanelVisibility(true);
            MessageHub.Instance.Publish<PageNavigated>(new PageNavigated() { NavigatedTo = false, PageType = PageNavigatedType.NowPlayingDesktop });
            await Task.CompletedTask;
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            //App.ChangeRightPanelVisibility(false);
            App.OnNavigatedToNewView(true, true);
            MessageHub.Instance.Publish<PageNavigated>(new PageNavigated() { NavigatedTo = true, PageType = PageNavigatedType.NowPlayingDesktop });
            if (mode == NavigationMode.New || mode == NavigationMode.Forward)
            {
                TelemetryAdapter.TrackPageView(this.GetType().ToString());
            }
            await Task.CompletedTask;
        }
        
        public async void RateSong(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int rating = Int32.Parse(button.Tag.ToString());
            await QueueVM.RateSong(rating);
        }
    }
}
