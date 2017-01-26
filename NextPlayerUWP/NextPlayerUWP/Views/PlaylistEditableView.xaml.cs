using Microsoft.Toolkit.Uwp.UI.Controls;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Model;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaylistEditableView : Page
    {
        public PlaylistViewModel ViewModel;
        public PlaylistEditableView()
        {
            this.InitializeComponent();
            this.Loaded += View_Loaded;
            //this.Unloaded += View_Unloaded;
            ViewModel = (PlaylistViewModel)DataContext;
        }
        //~PlaylistEditableView()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}
        private void View_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnUnloaded();
            ViewModel = null;
            DataContext = null;
            this.Loaded -= View_Loaded;
            this.Unloaded -= View_Unloaded;
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnLoaded(PlaylistListView);
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
            var song = (sender as SlidableListItem).DataContext as SongItem;
            await ViewModel.SlidableListItemLeftCommandRequested(song);
        }

        private async void SlidableListItem_RightCommandRequested(object sender, EventArgs e)
        {
            var song = (sender as SlidableListItem).DataContext as SongItem;
            await ViewModel.SlidableListItemRightCommandRequested(song);
        }

        private async void PlayNowMultiple(object sender, RoutedEventArgs e)
        {
            var items = PlaylistListView.GetSelectedItems<MusicItem>();
            if (items.Count > 0) await ViewModel.PlayNowMany(items);
        }

        private async void PlayNextMultiple(object sender, RoutedEventArgs e)
        {
            var items = PlaylistListView.GetSelectedItems<MusicItem>();
            if (items.Count > 0) await ViewModel.PlayNextMany(items);
        }

        private async void AddToNowPlayingMultiple(object sender, RoutedEventArgs e)
        {
            var items = PlaylistListView.GetSelectedItems<MusicItem>();
            if (items.Count > 0) await ViewModel.AddToNowPlayingMany(items);
        }

        private void AddToPlaylistMultiple(object sender, RoutedEventArgs e)
        {
            var items = PlaylistListView.GetSelectedItems<MusicItem>();
            if (items.Count > 0) ViewModel.AddToPlaylistMany(items);
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            PlaylistListView.SelectAll();
        }
    }
}
