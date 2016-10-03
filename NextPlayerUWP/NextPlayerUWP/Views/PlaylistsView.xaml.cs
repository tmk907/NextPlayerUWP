using Microsoft.Toolkit.Uwp.UI.Controls;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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
    public sealed partial class PlaylistsView : Page
    {
        public PlaylistsViewModel ViewModel;
        public PlaylistsView()
        {
            this.InitializeComponent();
            this.Loaded += delegate { ((PlaylistsViewModel)DataContext).OnLoaded(PlaylistsListView); };
            ViewModel = (PlaylistsViewModel)DataContext;
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["ContextMenu"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        private async void newPlainPlaylist_Click(object sender, RoutedEventArgs e)
        {
            await ContentDialogNewPlaylist.ShowAsync();
        }

        private async void MenuFlyoutItemDelete_Click(object sender, RoutedEventArgs e)
        {
            ResourceLoader loader = new ResourceLoader();
            string content = loader.GetString("DeletePlaylistConfirmation");
            MessageDialog dialog = new MessageDialog(content);
            dialog.Commands.Add(new UICommand(loader.GetString("Delete"), (command) =>
            {
                ViewModel.ConfirmDelete(((MenuFlyoutItem)sender).CommandParameter);
            }));
            dialog.Commands.Add(new UICommand(loader.GetString("Cancel"), (command) => 
            {

            }));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            await dialog.ShowAsync();
        }

        private async void MFIEditName_Click(object sender, RoutedEventArgs e)
        {
            PlaylistItem selected = (PlaylistItem)((MenuFlyoutItem)sender).CommandParameter;
            ViewModel.EditPlaylist = new PlaylistItem(selected.Id, false, selected.Name);
            await ContentDialogEditName.ShowAsync();
        }

        private async void MFIExportChoosePathKind(object sender, RoutedEventArgs e)
        {
            PlaylistItem selected = (PlaylistItem)((MenuFlyoutItem)sender).CommandParameter;
            ViewModel.EditPlaylist = new PlaylistItem(selected.Id, selected.IsSmart, selected.Name);
            await ContentDialogChoosePathKind.ShowAsync();
        }

        private async void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        {
            var song = (sender as SlidableListItem).DataContext as SongItem;
            await ViewModel.SlidableListItemLeftCommandRequested(song);
        }

    }
}
