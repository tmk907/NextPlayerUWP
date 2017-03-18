using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Model;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
using NextPlayerUWP.Messages;
using NextPlayerUWP.Controls;
using NextPlayerUWP.Messages.Hub;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumsView : Page
    {
        public AlbumsViewModel ViewModel;
        private ButtonsForMultipleSelection selectionButtons;
        private Guid token;

        public AlbumsView()
        {
            this.InitializeComponent();
            //NavigationCacheMode = NavigationCacheMode.Required;
            this.Loaded += View_Loaded;
            this.Unloaded += View_Unloaded;
            ViewModel = (AlbumsViewModel)DataContext;
            selectionButtons = new ButtonsForMultipleSelection();
        }
        ~AlbumsView()
        {
            System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        }
        private void View_Unloaded(object sender, RoutedEventArgs e)
        {
            selectionButtons.OnUnloaded();
            MessageHub.Instance.UnSubscribe(token);
            ViewModel.OnUnloaded();
        }

        private void View_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnLoaded(AlbumsListView);
            token = MessageHub.Instance.Subscribe<EnableSearching>(OnSearchMessage);
            selectionButtons.OnLoaded(ViewModel, PageHeader, AlbumsListView);
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

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            var image = (Image)sender;
            image.Fade(1, 500, 0).Start();
        }

        private List<MusicItem> GetSelectedItems()
        {
            List<MusicItem> list = new List<MusicItem>();
            if (AlbumsListView.SelectedItems != null)
            {
                foreach(var item in AlbumsListView.SelectedItems)
                {
                    list.Add((MusicItem)item);
                }
            }
            return list;
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
            AlbumsListView.SelectAll();
        }

        public void OnSearchMessage(EnableSearching msg)
        {
            SearchBox.Focus(FocusState.Programmatic);
        }
    }
}
