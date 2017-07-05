using NextPlayerUWP.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NextPlayerUWP.Views
{
    public sealed partial class RightPanelControl : UserControl
    {
        private RightPanelViewModel ViewModel;

        public RightPanelControl()
        {
            this.InitializeComponent();
            this.Loaded += RightPanelControl_Loaded;
            this.Unloaded += RightPanelControl_Unloaded;
            ViewModel = (RightPanelViewModel)DataContext;
        }

        private void RightPanelControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnLoaded(NowPlayingPlaylistListView);
        }

        private void RightPanelControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnUnLoaded();
        }

        private async void SearchButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await ContentDialogSearchLyrics.ShowAsync();
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["ContextMenu"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        private void ContentDialogSearchLyrics_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.ArtistSearch = ArtistSearch.Text;
            ViewModel.TitleSearch = TitleSearch.Text;
            ViewModel.SearchLyrics();
        }
    }
}
