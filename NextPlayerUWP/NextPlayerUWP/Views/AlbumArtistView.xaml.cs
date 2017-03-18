using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Model;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Animations;
using NextPlayerUWP.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumArtistView : Page
    {
        public AlbumArtistViewModel ViewModel;
        private ButtonsForMultipleSelection selectionButtons;

        public AlbumArtistView()
        {
            this.InitializeComponent();
            this.Loaded += View_Loaded;
            this.Unloaded += View_Unloaded;
            ViewModel = (AlbumArtistViewModel)DataContext;
            selectionButtons = new ButtonsForMultipleSelection();
        }
        ~AlbumArtistView()
        {
            System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        }
        private void View_Unloaded(object sender, RoutedEventArgs e)
        {
            selectionButtons.OnUnloaded();
            ShuffleAppBarButton.Click -= ShuffleAppBarButton_Click;
            ViewModel = null;
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            ShuffleAppBarButton.Click += ShuffleAppBarButton_Click;
            ViewModel.OnLoaded(AlbumArtistSongsListView);
            selectionButtons.OnLoaded(ViewModel, PageHeader, AlbumArtistSongsListView);
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (!ViewModel.IsMultiSelection)
            {
                FrameworkElement senderElement = sender as FrameworkElement;
                var menu = this.Resources["ContextMenu"] as MenuFlyout;
                var position = e.GetPosition(senderElement);
                menu.ShowAt(senderElement, position);
            }
        }

        private void AlbumGroupHeader_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["AlbumContextMenu"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        private async void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        {
            var song = (sender as SlidableListItem).DataContext as MusicItem;
            await ViewModel.SlidableListItemLeftCommandRequested(song);
        }

        private void AlbumCoverImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = (Image)sender;
            image.Fade(1, 500, 0).Start();
        }

        private void AlbumCoverImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var image = sender as Image;
            int id = (int)image.Tag;
            App.Current.NavigationService.Navigate(App.Pages.Album, id);
        }

        private void ShuffleAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ShuffleAllSongs();
        }

        private void EnableMultipleSelection(object sender, RoutedEventArgs e)
        {
            ViewModel.EnableMultipleSelection();
            selectionButtons.ShowMultipleSelectionButtons();
        }

        private void DisableMultipleSelection(object sender, RoutedEventArgs e)
        {
            ViewModel.DisableMultipleSelection();
            selectionButtons.HideMultipleSelectionButtons();
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            AlbumArtistSongsListView.SelectAll();
        }
    }
}
