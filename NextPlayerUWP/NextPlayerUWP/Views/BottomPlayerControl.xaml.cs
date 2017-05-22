using NextPlayerUWP.Common;
using NextPlayerUWP.ViewModels;
using System;
using System.Linq;
using Template10.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NextPlayerUWP.Views
{
    public sealed partial class BottomPlayerControl : UserControl
    {
        BottomPlayerViewModel ViewModel;
        PointerEventHandler pointerpressedhandler;
        PointerEventHandler pointerreleasedhandler;

        public BottomPlayerControl()
        {
            System.Diagnostics.Debug.WriteLine("BottomPlayerControl()");
            this.InitializeComponent();

            pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);

            ViewModel = (BottomPlayerViewModel)DataContext;
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

        #region Slider 

        private void BottomSlider_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSliderEvents(durationSliderBottom);
        }

        private void BottomSlider_Unloaded(object sender, RoutedEventArgs e)
        {
            UnloadSliderEvents(durationSliderBottom);
        }

        private void SliderGrid_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSliderEvents(timeslider);
        }

        private void SliderGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            UnloadSliderEvents(timeslider);
        }

        private void LoadSliderEvents(Slider slider)
        {
            slider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);
            slider.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);
        }

        private void UnloadSliderEvents(Slider slider)
        {
            slider.RemoveHandler(Control.PointerPressedEvent, pointerpressedhandler);
            slider.RemoveHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler);
        }

        private void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ViewModel.sliderpressed = true;
        }

        private void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            ViewModel.sliderpressed = false;
            PlaybackService.Instance.Position = TimeSpan.FromSeconds(((Slider)sender).Value);
        }

        #endregion




        private void GoToNowPlaying(object sender, TappedRoutedEventArgs e)
        {
            if (IsDesktop())
            {
#if DEBUG
                var nav = WindowWrapper.Current().NavigationServices.FirstOrDefault();
                nav.Navigate(App.Pages.NowPlayingPlaylist);
                //nav.Navigate(App.Pages.NowPlaying);
#endif
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
