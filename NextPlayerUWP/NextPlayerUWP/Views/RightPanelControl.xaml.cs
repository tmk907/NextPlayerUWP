using Microsoft.Advertising.WinRT.UI;
using NextPlayerUWP.ViewModels;
using System.Threading.Tasks;
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
            ViewModel = (RightPanelViewModel)DataContext;
            this.Loaded += delegate { ViewModel.OnLoaded(NowPlayingPlaylistListView, web); };
            //this.Unloaded += (sender, e) =>
            //{
            //    ViewModel = null;
            //    Bindings.StopTracking();
            //};
            //LoadAdControl();
        }

        //        private async Task LoadAdControl()
        //        {
        //            await Task.Delay(1000);
        //            var adControl = new AdControl();
        //            adControl.Width = 300;
        //            adControl.Height = 250;
        //            adControl.VerticalAlignment = VerticalAlignment.Top;
        //            adControl.HorizontalAlignment = HorizontalAlignment.Left;
        //            adControl.AdRefreshed += AdControl_AdRefreshed;
        //            adControl.ErrorOccurred += AdControl_ErrorOccurred;
        //#if DEBUG
        //            adControl.ApplicationId = "3f83fe91-d6be-434d-a0ae-7351c5a997f1";
        //            adControl.AdUnitId = "10865270";
        //#else
        //            adControl.ApplicationId = "bc203ea3-080a-4a87-bd1d-fdf2aab1740d";
        //            adControl.AdUnitId = "11647976";
        //#endif
        //            GridAdControlRightPanel.Children.Add(adControl);
        //        }

        ~RightPanelControl()
        {
            System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["ContextMenu"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        //private void AdControl_ErrorOccurred(object sender, AdErrorEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("AdControl error (" + ((AdControl)sender).Name + "): " + e.ErrorMessage + " ErrorCode: " + e.ErrorCode.ToString());
        //}

        //private void AdControl_AdRefreshed(object sender, RoutedEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("AdControl refreshed (" + ((AdControl)sender).Name + ") has ad:" + ((AdControl)sender).HasAd.ToString());
        //}


        private void AdControl_AdLoadingError(object sender, AdDuplex.Common.Models.AdLoadingErrorEventArgs e)
        {
            //NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage(e.Error.Message, NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.Debug);
        }
    }
}
