using Microsoft.Toolkit.Uwp.UI.Controls;
using NextPlayerUWP.Common;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            this.Loaded += delegate { ((PlaylistViewModel)DataContext).OnLoaded(PlaylistListView); };
            ViewModel = (PlaylistViewModel)DataContext;
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            MenuFlyout menu = this.Resources["ContextMenuPlain"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        private async void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        {
            var a = (sender as SlidableListItem).DataContext as SongItem;
            await NowPlayingPlaylistManager.Current.Add(a);
        }

        private void SlidableListItem_RightCommandRequested(object sender, EventArgs e)
        {
            var a = (sender as SlidableListItem).DataContext as SongItem;
            ViewModel.Playlist.Remove(a);
        }
    }
}
