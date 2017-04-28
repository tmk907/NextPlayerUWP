using Microsoft.Toolkit.Uwp.UI.Controls;
using NextPlayerUWP.Common;
using NextPlayerUWP.Controls;
using NextPlayerUWP.Messages;
using NextPlayerUWP.Messages.Hub;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Model;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaylistsView : Page
    {
        public PlaylistsViewModel ViewModel;
        private ButtonsForMultipleSelection selectionButtons;
        private Guid token;

        public PlaylistsView()
        {
            System.Diagnostics.Debug.WriteLine("PlaylistsView()");
            this.InitializeComponent();
            //NavigationCacheMode = NavigationCacheMode.Required;
            this.Loaded += View_Loaded;
            this.Unloaded += View_Unloaded;
            ViewModel = (PlaylistsViewModel)DataContext;
            selectionButtons = new ButtonsForMultipleSelection();
        }

        //~PlaylistsView()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PlaylistsView.Loaded()");
            ViewModel.OnLoaded(PlaylistsListView);
            token = MessageHub.Instance.Subscribe<EnableSearching>(OnSearchMessage);
            selectionButtons.OnLoaded(ViewModel, PageHeader, PlaylistsListView);
        }

        private void View_Unloaded(object sender, RoutedEventArgs e)
        {
            selectionButtons.OnUnloaded();
            MessageHub.Instance.UnSubscribe(token);
            ViewModel.OnUnloaded();
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

        private async void newPlainPlaylist_Click(object sender, RoutedEventArgs e)
        {
            await ContentDialogNewPlaylist.ShowAsync();
        }

        private async void MenuFlyoutItemDelete_Click(object sender, RoutedEventArgs e)
        {
            TranslationHelper helper = new  TranslationHelper();
            string content = helper.GetTranslation("DeletePlaylistConfirmation");
            MessageDialog dialog = new MessageDialog(content);
            dialog.Commands.Add(new UICommand(helper.GetTranslation(TranslationHelper.Delete), (command) =>
            {
                ViewModel.ConfirmDelete(((MenuFlyoutItem)sender).CommandParameter);
            }));
            dialog.Commands.Add(new UICommand(helper.GetTranslation(TranslationHelper.Cancel), (command) => 
            {

            }));
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;
            await dialog.ShowAsync();
        }

        private async void MenuFlyoutItemShowDetails_Click(object sender, RoutedEventArgs e)
        {
            PlaylistItem selected = (PlaylistItem)((MenuFlyoutItem)sender).CommandParameter;
            ViewModel.EditPlaylist = selected;
            await ContentDialogShowDetails.ShowAsync();
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
            ViewModel.EditPlaylist = selected;
            await ContentDialogChoosePathKind.ShowAsync();
        }

        private async void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        {
            var item = (sender as SlidableListItem).DataContext as PlaylistItem;
            await ViewModel.SlidableListItemLeftCommandRequested(item);
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
            PlaylistsListView.SelectAll();
        }

        public void OnSearchMessage(EnableSearching msg)
        {
            SearchBox.Focus(FocusState.Programmatic);
        }
    }
}
