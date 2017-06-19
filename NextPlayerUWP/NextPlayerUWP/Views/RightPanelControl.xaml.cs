using Microsoft.Advertising.WinRT.UI;
using NextPlayerUWP.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NextPlayerUWP.Views
{
    public sealed partial class RightPanelControl : UserControl
    {
        private RightPanelViewModel ViewModel;
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
            //AddAdControl();
        }

        private void RightPanelControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnUnLoaded();
            //RemoveAdControl();
        }

        private async void SearchButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await ContentDialogSearchLyrics.ShowAsync();
        }

        private void AddAdControl()
        {
            if (Microsoft.Toolkit.Uwp.ConnectionHelper.IsInternetAvailable)
            {

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
            //    

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

        private void ContentDialogSearchLyrics_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.ArtistSearch = ArtistSearch.Text;
            ViewModel.TitleSearch = TitleSearch.Text;
            ViewModel.SearchLyrics();
        }
    }
}
