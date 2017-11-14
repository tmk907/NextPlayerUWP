using NextPlayerUWP.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Controls;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWP.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumView : Page
    {
        public AlbumViewModel ViewModel;
        private ButtonsForMultipleSelection selectionButtons;

        public AlbumView()
        {
            this.InitializeComponent();
            this.Loaded += View_Loaded;
            this.Unloaded += View_Unloaded;
            ViewModel = (AlbumViewModel)DataContext;
            selectionButtons = new ButtonsForMultipleSelection();
            selectionButtons.ShowShareButton = true;
        }
        //~AlbumView()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}
        private void View_Unloaded(object sender, RoutedEventArgs e)
        {
            selectionButtons.OnUnloaded();
            ShuffleAppBarButton.Click -= ShuffleAppBarButton_Click;
            ViewModel.OnUnloaded();
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            ShuffleAppBarButton.Click += ShuffleAppBarButton_Click;
            ViewModel.OnLoaded(AlbumSongsListView);
            selectionButtons.OnLoaded(ViewModel, PageHeader, AlbumSongsListView);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ConnectedAnimation imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("albumImageAnimation");
            if (imageAnimation != null)
            {
                imageAnimation.TryStart(AlbumCoverImage);
            }
            else
            {
                imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("AlbumCoverFromArtist");
                if (imageAnimation != null)
                {
                    imageAnimation.TryStart(AlbumCoverImage);
                }
                else
                {
                    imageAnimation = ConnectedAnimationService.GetForCurrentView().GetAnimation("AlbumCoverFromAlbumArtist");
                    if (imageAnimation != null)
                    {
                        imageAnimation.TryStart(AlbumCoverImage);
                    }
                }
            }
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

        private async void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        {
            var song = (sender as SlidableListItem).DataContext as MusicItem;
            await ViewModel.SlidableListItemLeftCommandRequested(song);
        }

        private async void EditAlbum_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EditAlbum();
            await ContentDialogEditAlbum.ShowAsync();
        }

        private void AlbumCoverImageBG_ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = (Image)sender;
            image.Fade(1, 700, 0).Start();
        }

        private void AlbumCoverImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = (Image)sender;
            image.Fade(1, 800, 0).Start();
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
            AlbumSongsListView.SelectAll();
        }
    }
}
