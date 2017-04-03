using Microsoft.Advertising.WinRT.UI;
using NextPlayerUWP.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NextPlayerUWP.Views
{
    public sealed partial class RightPanelControl : UserControl
    {
        private RightPanelViewModel ViewModel;
        private WebView webView;
        //private AdDuplex.AdControl controlAdDuplex;
        //private AdControl controlMicrosoftAd;

        public RightPanelControl()
        {
            this.InitializeComponent();
            this.Loaded += RightPanelControl_Loaded;
            this.Unloaded += RightPanelControl_Unloaded;
            ViewModel = (RightPanelViewModel)DataContext;
        }

        private void RightPanelControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnLoaded(NowPlayingPlaylistListView);
            webView = ViewModel.GetWebView();
            WebGrid.Children.Add(webView);
            AddAdControl();
        }

        private void RightPanelControl_Unloaded(object sender, RoutedEventArgs e)
        {
            WebGrid.Children.Remove(webView);
            webView = null;
            ViewModel.OnUnLoaded();
            RemoveAdControl();
        }

        private void AddAdControl()
        {
            if (Microsoft.Toolkit.Uwp.ConnectionHelper.IsInternetAvailable)
            {
                //controlAdDuplex = new AdDuplex.AdControl()
                //{
                //    AdUnitId = "202211",
                //    AppKey = "bfe9d689-7cf7-4add-84fe-444dc72e6f36",
                //    CollapseOnError = true,
                //};
                ////controlAdDuplex.AdLoadingError += AdControl_AdLoadingError;

                //GridAdControlRightPanel1.Children.Add(controlAdDuplex);


                //controlMicrosoftAd = new AdControl()
                //{
                //    AdUnitId = "11647976",
                //    ApplicationId = "bc203ea3-080a-4a87-bd1d-fdf2aab1740d",
                //    Width=300,
                //    Height = 50
                //};
                //controlMicrosoftAd.ErrorOccurred += AdControl_ErrorOccurred;
                //controlMicrosoftAd.AdRefreshed += AdControl_AdRefreshed;
                //GridAdControlRightPanel1.Children.Add(controlMicrosoftAd);
            }
        }


        private void RemoveAdControl()
        {
            //if (controlAdDuplex != null)
            //{
            //    //GridAdControlRightPanel1.Children.Remove(controlAdDuplex);
            //    ////controlAdDuplex.AdLoadingError -= AdControl_AdLoadingError;
            //    //controlAdDuplex = null;

            //    //GridAdControlRightPanel1.Children.Remove(controlMicrosoftAd);
            //    //controlMicrosoftAd = null;
            //}
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

        //~RightPanelControl()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}

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
            System.Diagnostics.Debug.WriteLine("AdControl refreshed (" + ((AdControl)sender).Name + ") has ad:" + ((AdControl)sender).HasAd.ToString());
        }


        private void AdControl_AdLoadingError(object sender, AdDuplex.Common.Models.AdLoadingErrorEventArgs e)
        {
            //NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage(e.Error.Message, NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.Debug);
        }
    }
}
