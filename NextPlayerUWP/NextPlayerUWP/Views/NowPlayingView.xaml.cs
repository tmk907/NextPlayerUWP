using NextPlayerUWP.Common;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Constants;
using System;
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
        Uri imageUri;
        NowPlayingViewModel ViewModel;
        PointerEventHandler pointerpressedhandler;
        PointerEventHandler pointerreleasedhandler;

        public NowPlayingView()
        {
            this.InitializeComponent();
            ViewModel = (NowPlayingViewModel)DataContext;
            this.Loaded += NowPlayingView_Loaded;
            this.Unloaded += NowPlayingView_Unloaded;
            pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
        }

        private void NowPlayingView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSliderEvents();
            Init();
            if (ViewModel.QueueVM.CoverUri != null)
            {
                imageUri = GetAlbumUri();
                PrimaryImage.ImageUri = imageUri;
            }
            ViewModel.QueueVM.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void Init()
        {
        }

        private void NowPlayingView_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.QueueVM.PropertyChanged -= ViewModel_PropertyChanged;
            UnloadSliderEvents();
        }

        #region Slider 
        private void LoadSliderEvents()
        {
            timeslider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);
            timeslider.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);            
        }

        private void UnloadSliderEvents()
        {
            timeslider.RemoveHandler(Control.PointerPressedEvent, pointerpressedhandler);
            timeslider.RemoveHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler);
        }

        void slider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ViewModel.sliderpressed = true;
        }

        void slider_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            ViewModel.sliderpressed = false;
            PlaybackService.Instance.Position = TimeSpan.FromSeconds(timeslider.Value);
        }

        void progressbar_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!ViewModel.sliderpressed)
            {
                PlaybackService.Instance.Position = TimeSpan.FromSeconds(e.NewValue);
            }
        }
        #endregion

        private async void ButtonShowAudioSettings_Click(object sender, RoutedEventArgs e)
        {
            await ContentDialogAudioSettings.ShowAsync();
        }

        private Uri GetAlbumUri()
        {
            if (ViewModel.QueueVM.CoverUri == new Uri(AppConstants.SongCoverBig))
            {
                ColorsHelper ch = new ColorsHelper();
                return new Uri(ch.GetAlbumCoverAssetWithCurrentAccentColor());
            }
            else
            {
                return ViewModel.QueueVM.CoverUri;
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.QueueVM.CoverUri))
            {
                imageUri = GetAlbumUri();
                PrimaryImage.ImageUri = imageUri;
            }
        }

        private async void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await ContentDialogAudioSettings.ShowAsync();
        }
    }    
}
