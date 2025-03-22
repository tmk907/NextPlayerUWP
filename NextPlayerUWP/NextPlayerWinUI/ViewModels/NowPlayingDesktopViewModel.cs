using NextPlayerWinUI.Common;
using NextPlayerWinUI.Messages;
using NextPlayerWinUI.Messages.Hub;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace NextPlayerWinUI.ViewModels
{
    public class NowPlayingDesktopViewModel : Template10.Mvvm.ViewModelBase
    {
        public NowPlayingDesktopViewModel()
        {
            ViewModelLocator vml = new ViewModelLocator();
            QueueVM = vml.QueueVM;
            token = MessageHub.Instance.Subscribe<RightPanelVisibilityChange>(OnRightPanelVisibilityChange);
            ShowButtons = Window.Current.Bounds.Width <= 720;
        }

        public QueueViewModelBase QueueVM { get; set; }
        private Guid token;

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
            ShowButtons = Window.Current.Bounds.Width < 720;
            await Task.CompletedTask;
        }
        
        private void OnRightPanelVisibilityChange(RightPanelVisibilityChange msg)
        {
            ShowButtons = !msg.Visible;
        }

        public async void RateSong(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int rating = Int32.Parse(button.Tag.ToString());
            await QueueVM.RateSong(rating);
        }

        private bool showButtons = false;
        public bool ShowButtons
        {
            get { return showButtons; }
            set { SetProperty(ref showButtons, value); }
        }

        public void GoToNowPlayingPlaylist()
        {
            NavigationService.Navigate(AppPages.Pages.NowPlayingPlaylist);
        }

        public void GoToLyrics()
        {
            NavigationService.Navigate(AppPages.Pages.Lyrics);
        }
    }
}
