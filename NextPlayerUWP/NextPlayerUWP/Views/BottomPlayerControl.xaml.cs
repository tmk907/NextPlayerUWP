using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Common;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NextPlayerUWP.Views
{
    public sealed partial class BottomPlayerControl : UserControl
    {
        public BottomPlayerControl()
        {
            this.InitializeComponent();
        }
        //~BottomPlayerControl()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}
        private bool IsDesktop()
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop") return true;
            else return false;
        }

        private void GoToNowPlaying(object sender, TappedRoutedEventArgs e)
        {
            if (IsDesktop())
            {
                //Menu.NavigationService.Navigate(App.Pages.NowPlaying);
                //Menu.NavigationService.Navigate(App.Pages.NowPlayingPlaylist);
            }
            else
            {
                var nav = WindowWrapper.Current().NavigationServices.FirstOrDefault();//.NavigationService;
                if (nav == null)
                {

                }
                else
                {
                    nav.Navigate(App.Pages.NowPlaying);
                }
                //Menu.NavigationService.Navigate(App.Pages.NowPlaying);
            }
        }
    }
}
