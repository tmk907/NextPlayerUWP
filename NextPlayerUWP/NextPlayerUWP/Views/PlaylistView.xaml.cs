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
    public sealed partial class PlaylistView : Page
    {
        public PlaylistViewModel ViewModel;
        public PlaylistView()
        {          
            this.InitializeComponent();
            this.Loaded += View_Loaded;
            //this.Unloaded += View_Unloaded;
            ViewModel = (PlaylistViewModel)DataContext;
        }
        //~PlaylistView()
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

        public bool IsA = true;
        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            MenuFlyout menu;

            if (ViewModel.IsPlainPlaylist)
            {
                menu = this.Resources["ContextMenuPlain"] as MenuFlyout;
            }
            else
            {
                menu = this.Resources["ContextMenuList"] as MenuFlyout;
            }
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        private async void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        {
            var song = (sender as SlidableListItem).DataContext as SongItem;
            await ViewModel.SlidableListItemLeftCommandRequested(song);
        }
    }
}
