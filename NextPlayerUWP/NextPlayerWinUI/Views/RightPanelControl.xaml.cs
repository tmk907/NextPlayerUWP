using NextPlayerWinUI.ViewModels;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NextPlayerWinUI.Views
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

        private async void SearchButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ContentDialogSearchLyrics.XamlRoot = XamlRoot;
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
