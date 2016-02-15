using NextPlayerUWP.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NextPlayerUWP.Views
{
    public sealed partial class RightPanelControl : UserControl
    {
        NowPlayingPlaylistViewModel ViewModel;
        public RightPanelControl()
        {
            this.InitializeComponent();
            this.Loaded += delegate { ((NowPlayingPlaylistViewModel)DataContext).OnLoaded(NowPlayingPlaylistListView); };
            ViewModel = (NowPlayingPlaylistViewModel)DataContext;
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["ContextMenu"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }
    }
}
