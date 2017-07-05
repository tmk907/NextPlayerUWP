using NextPlayerUWP.AppColors;
using NextPlayerUWP.Common;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using System;
using System.Threading.Tasks;
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
        NowPlayingPlaylistPanelViewModel PanelVM;
        PointerEventHandler pointerpressedhandler;
        PointerEventHandler pointerreleasedhandler;

        public NowPlayingView()
        {
            this.InitializeComponent();
            ViewModel = (NowPlayingViewModel)DataContext;
            ViewModelLocator vml = new ViewModelLocator();
            PanelVM = vml.NowPlayingPlaylistPanelVM;
            this.Loaded += NowPlayingView_Loaded;
            this.Unloaded += NowPlayingView_Unloaded;
            pointerpressedhandler = new PointerEventHandler(slider_PointerEntered);
            pointerreleasedhandler = new PointerEventHandler(slider_PointerCaptureLost);
        }

        //~NowPlayingView()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}

        private void NowPlayingView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSliderEvents();
            if (ViewModel.QueueVM.CoverUri != null)
            {
                imageUri = GetAlbumUri();
                PrimaryImage.ImageUri = imageUri;
                ShowPrimaryImage.Begin();
            }
            ViewModel.QueueVM.PropertyChanged += ViewModel_PropertyChanged;
            ShowAlbumArtInBackground();
        }

        private Uri GetAlbumUri()
        {
            if (ViewModel.QueueVM.CoverUri == AlbumArtColors.DefaultAlbumArtUri)
            {
                return AlbumArtColors.GetAlbumCoverAssetWithCurrentAccentColor();
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

        private async Task ShowAlbumArtInBackground()
        {
            bool show = ApplicationSettingsHelper.ReadSettingsValue<bool>(SettingsKeys.AlbumArtInBackground);
            if (show)
            {
                FindName(nameof(BackDropControl));
                await Task.Delay(300);
                FindName(nameof(BackgroundImage));
                ShowBGImage.Begin();
            }
        }

        private void NowPlayingView_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.QueueVM.PropertyChanged -= ViewModel_PropertyChanged;
            UnloadSliderEvents();
            imageUri = null;
            //DataContext = null;
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

        private async void SearchButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await ContentDialogSearchLyrics.ShowAsync();
        }

        private async void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await ContentDialogAudioSettings.ShowAsync();
        }

        private void ImageGrid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void ImageGrid_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void ImageGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;   
        }

        private void ContentDialogSearchLyrics_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ViewModel.ArtistSearch = ArtistSearch.Text;
            ViewModel.TitleSearch = TitleSearch.Text;
            ViewModel.SearchLyrics();
            ViewModel.PivotSelectedIndex = 1;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivot = sender as Pivot;
            if (pivot.SelectedIndex == 2)
            {
                FindName(nameof(NowPlayingPlaylistPanel));
            }
        }
    }    
}
