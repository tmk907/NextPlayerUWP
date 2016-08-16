using NextPlayerUWP.Common;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Constants;
using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NowPlayingView : Page
    {
        NowPlayingViewModel ViewModel;
        public NowPlayingView()
        {
            this.InitializeComponent();
            ViewModel = (NowPlayingViewModel)DataContext;
            this.Loaded += LoadSlider;
            //this.Loaded += delegate { ((NowPlayingView)DataContext).OnLoaded(FoldersListView); };
            
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
            ViewModel.sliderpressed = true;
        }

        void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            ViewModel.sliderpressed = false;
            PlaybackService.Instance.Position = TimeSpan.FromSeconds(timeslider.Value);
            //viewModel.SendMessage(AppConstants.Position, TimeSpan.FromSeconds(timeslider.Value));
        }

        void progressbar_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!ViewModel.sliderpressed)
            {
                PlaybackService.Instance.Position = TimeSpan.FromSeconds(e.NewValue);
                //viewModel.SendMessage(AppConstants.Position, TimeSpan.FromSeconds(e.NewValue));
            }
        }
        #endregion
    }

    public class SizeNotifyPanel : ContentPresenter
    {
        public static DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(Size), typeof(SizeNotifyPanel), null);

        public Size Size
        {
            get { return (Size)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public SizeNotifyPanel()
        {
            SizeChanged += (s, e) => Size = e.NewSize;
        }
    }
}
