using NextPlayerUWP.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CloudStorageFoldersView : Page
    {
        public CloudStorageFoldersViewModel ViewModel;
        public CloudStorageFoldersView()
        {
            this.InitializeComponent();
            this.Loaded += delegate { ((CloudStorageFoldersViewModel)DataContext).OnLoaded(FoldersListView); };
            ViewModel = (CloudStorageFoldersViewModel)DataContext;
        }

        private void ListViewFolderItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["ContextMenuFolder"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        private void ListViewSongItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["ContextMenuFile"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }
    }
}
