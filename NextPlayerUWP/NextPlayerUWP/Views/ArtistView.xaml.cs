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
using NextPlayerUWP.Common;
using NextPlayerUWP.AppColors;
using System.Linq;
using System.Collections;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ArtistView : Page
    {
        public ArtistViewModel ViewModel;
        private ButtonsForMultipleSelection selectionButtons;
        private AlbumArtColors albumArtColors;

        public ArtistView()
        {
            this.InitializeComponent();
            this.Loaded += View_Loaded;
            this.Unloaded += View_Unloaded;
            ViewModel = (ArtistViewModel)DataContext;
            selectionButtons = new ButtonsForMultipleSelection();
            selectionButtons.ShowShareButton = true;
            albumArtColors = new AlbumArtColors();
        }
        //~ArtistView()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}
        private void View_Unloaded(object sender, RoutedEventArgs e)
        {
            selectionButtons.OnUnloaded();
            ShuffleAppBarButton.Click -= ShuffleAppBarButton_Click;
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            ShuffleAppBarButton.Click += ShuffleAppBarButton_Click;
            ViewModel.OnLoaded(ArtistSongsListView);
            selectionButtons.OnLoaded(ViewModel, PageHeader, ArtistSongsListView);
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
            ArtistSongsListView.SelectAll();
        }

        private void AlbumCoverImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var image = sender as Image;
            int id = (int)image.Tag;
            App.Current.NavigationService.Navigate(AppPages.Pages.Album, id);
        }

        private async void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var grid = ((Grid)sender);
            Image image;
            if (e.OriginalSource.GetType() == typeof(Image))
            {
                image = e.OriginalSource as Image;
            }
            else
            {
                return;
            }

            var dt = image.DataContext as IList;
            var item = dt[0] as SongItem;
            var color = albumArtColors.GetDominantColorFromSavedAlbumArt(item.AlbumArtUri);
            var shadow = grid.Children.OfType<DropShadowPanel>().First();
            shadow.Color = color;
            shadow.ShadowOpacity = 0.7;
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var grid = ((Grid)sender);
            var shadow = grid.Children.OfType<DropShadowPanel>().First();
            shadow.ShadowOpacity = 0.0;
        }

        private void DropShadowPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var shadow = ((DropShadowPanel)sender);
            Image image;
            if (e.OriginalSource.GetType() == typeof(Image))
            {
                image = e.OriginalSource as Image;
            }
            else
            {
                return;
            }

            var dt = image.DataContext as IList;
            var item = dt[0] as SongItem;
            var color = albumArtColors.GetDominantColorFromSavedAlbumArt(item.AlbumArtUri);
            shadow.Color = color;
            shadow.ShadowOpacity = 0.7;
        }

        private void DropShadowPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var shadow = ((DropShadowPanel)sender);
            shadow.ShadowOpacity = 0.0;
        }
    }
}
