using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Services.NavigationService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shell : Page
    {
        public Shell(INavigationService  navigationService)
        {
            this.InitializeComponent();
            this.Loaded += LoadSlider;
            Menu.NavigationService = navigationService;
        }

        #region Slider 
        private void LoadSlider(object sender, RoutedEventArgs e)
        {
            PointerEventHandler pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            timeslider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);

            PointerEventHandler pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
            timeslider.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }

        void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            NowPlayingViewModel viewModel = (NowPlayingViewModel)BottomPlayerGrid.DataContext;
            viewModel.sliderpressed = true;
        }

        void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            NowPlayingViewModel viewModel = (NowPlayingViewModel)BottomPlayerGrid.DataContext;
            viewModel.sliderpressed = false;
            Common.PlaybackManager.Current.SendMessage(AppConstants.Position, TimeSpan.FromSeconds(timeslider.Value));
            //viewModel.SendMessage(AppConstants.Position, TimeSpan.FromSeconds(timeslider.Value));
        }

        void progressbar_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            NowPlayingViewModel viewModel = (NowPlayingViewModel)BottomPlayerGrid.DataContext;
            if (!viewModel.sliderpressed)
            {
                Common.PlaybackManager.Current.SendMessage(AppConstants.Position, TimeSpan.FromSeconds(e.NewValue));
                //viewModel.SendMessage(AppConstants.Position, TimeSpan.FromSeconds(e.NewValue));
            }
        }
        #endregion
    }
}
