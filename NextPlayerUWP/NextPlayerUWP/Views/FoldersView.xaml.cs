using Microsoft.Toolkit.Uwp.UI.Controls;
using NextPlayerUWP.Controls;
using NextPlayerUWP.Messages;
using NextPlayerUWP.Messages.Hub;
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
    public sealed partial class FoldersView : Page
    {
        public FoldersViewModel ViewModel;
        private ButtonsForMultipleSelection selectionButtons;
        private Guid token;

        public FoldersView()
        {
            this.InitializeComponent();
            this.Loaded += View_Loaded;
            this.Unloaded += View_Unloaded;
            ViewModel = (FoldersViewModel)DataContext;
            selectionButtons = new ButtonsForMultipleSelection();
        }
        ~FoldersView()
        {
            System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        }
        private void View_Unloaded(object sender, RoutedEventArgs e)
        {
            selectionButtons.OnUnloaded();
            MessageHub.Instance.UnSubscribe(token);
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnLoaded(FoldersListView);
            token = MessageHub.Instance.Subscribe<EnableSearching>(OnSearchMessage);
            selectionButtons.OnLoaded(ViewModel, PageHeader, FoldersListView);
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (!ViewModel.IsMultiSelection)
            {
                FrameworkElement senderElement = sender as FrameworkElement;
                var menu = this.Resources["ContextMenuFolder"] as MenuFlyout;
                var position = e.GetPosition(senderElement);
                menu.ShowAt(senderElement, position);
            }
        }

        private void ListViewItem2_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["ContextMenuFile"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        private async void SlidableListItem_LeftCommandRequested(object sender, EventArgs e)
        {
            var item = (sender as SlidableListItem).DataContext as MusicItem;
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
            FoldersListView.SelectAll();
        }

        public void OnSearchMessage(EnableSearching msg)
        {
            SearchBox.Focus(FocusState.Programmatic);
        }
    }
}
