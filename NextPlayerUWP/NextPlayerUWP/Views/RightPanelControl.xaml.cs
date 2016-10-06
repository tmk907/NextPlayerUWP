using Microsoft.Advertising.WinRT.UI;
using NextPlayerUWP.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NextPlayerUWP.Views
{
    public sealed partial class RightPanelControl : UserControl
    {
        RightPanelViewModel ViewModel;

        public RightPanelControl()
        {
            this.InitializeComponent();
            WebView web = new WebView(WebViewExecutionMode.SeparateThread);
            WebGrid.Children.Add(web);
            this.Loaded += delegate { ((RightPanelViewModel)DataContext).OnLoaded(NowPlayingPlaylistListView, web); };
            ViewModel = (RightPanelViewModel)DataContext;
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["ContextMenu"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        private void AdControl_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("AdControl error (" + ((AdControl)sender).Name + "): " + e.ErrorMessage + " ErrorCode: " + e.ErrorCode.ToString());
        }

        private void AdControl_AdRefreshed(object sender, RoutedEventArgs e)
        {

        }
    }
}
