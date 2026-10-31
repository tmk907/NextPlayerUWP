//using Microsoft.Toolkit.Uwp.UI.Controls;
using NextPlayerWinUI.Common;
using NextPlayerWinUI.Controls;
using NextPlayerWinUI.Messages;
using NextPlayerWinUI.Messages.Hub;
using NextPlayerWinUI.ViewModels;
using NextPlayerWinUIDataLayer.Model;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerWinUI.Views
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
            ContentDialogNewPlaylist.XamlRoot = XamlRoot;
            await ContentDialogNewPlaylist.ShowAsync();
        }

        private async void MenuFlyoutItemDelete_Click(object sender, RoutedEventArgs e)
        {
            TranslationHelper helper = new  TranslationHelper();
            string content = helper.GetTranslation("DeletePlaylistConfirmation");
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Content = content,
                PrimaryButtonText = helper.GetTranslation(TranslationHelper.Delete),
                CloseButtonText = helper.GetTranslation(TranslationHelper.Cancel),
                DefaultButton = ContentDialogButton.Primary,
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                ViewModel.ConfirmDelete(((MenuFlyoutItem)sender).CommandParameter);
            }
        }

        private async void MenuFlyoutItemShowDetails_Click(object sender, RoutedEventArgs e)
        {
            PlaylistItem selected = (PlaylistItem)((MenuFlyoutItem)sender).CommandParameter;
            ViewModel.EditPlaylist = selected;
            ContentDialogShowDetails.XamlRoot = XamlRoot;
            await ContentDialogShowDetails.ShowAsync();
        }

        private async void MFIEditName_Click(object sender, RoutedEventArgs e)
        {
            PlaylistItem selected = (PlaylistItem)((MenuFlyoutItem)sender).CommandParameter;
            ViewModel.EditPlaylist = new PlaylistItem(selected.Id, false, selected.Name);
            ViewModel.NewPlaylistName = selected.Name;
            ContentDialogEditName.XamlRoot = XamlRoot;
            await ContentDialogEditName.ShowAsync();
        }

        private async void MFIExportChoosePathKind(object sender, RoutedEventArgs e)
        {
            PlaylistItem selected = (PlaylistItem)((MenuFlyoutItem)sender).CommandParameter;
            ViewModel.EditPlaylist = selected;
            ContentDialogChoosePathKind.XamlRoot = XamlRoot;
            await ContentDialogChoosePathKind.ShowAsync();
        }

        //private async void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        //{
        //    var item = (sender as SlidableListItem).DataContext as PlaylistItem;
        //    await ViewModel.SlidableListItemLeftCommandRequested(item);
        //}

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

        private void ContentDialogNewPlaylist_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.Name = playlistNameTB.Text;
            ViewModel.Save();
        }

        private void ContentDialogEditName_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.NewPlaylistName = editplaylistNameTB.Text;
            ViewModel.SaveEditedName();
        }
    }
}
