using NextPlayerWinUI.ViewModels;
using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using NextPlayerWinUIDataLayer.Constants;
using NextPlayerWinUI.Common;
using System.Threading.Tasks;
using NextPlayerWinUI.AppColors;
using Microsoft.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerWinUI.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NowPlayingDesktopView : Page
    {
        private Uri imageUri;
        NowPlayingDesktopViewModel ViewModel;

        public NowPlayingDesktopView()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(340, 500));
            ViewModel = (NowPlayingDesktopViewModel)DataContext;
            this.Loaded += NowPlayingDesktopView_Loaded;
            this.Unloaded += NowPlayingDesktopView_Unloaded;
        }
        //~NowPlayingDesktopView()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}
        private void NowPlayingDesktopView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.QueueVM.CoverUri != null)
            {
                imageUri = GetAlbumUri();
                PrimaryImage.Source = new BitmapImage(imageUri);
                ShowPrimaryImage.Begin();
            }
            ViewModel.QueueVM.PropertyChanged += ViewModel_PropertyChanged;
            ShowAlbumArtInBackground();
        }

        private void NowPlayingDesktopView_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.QueueVM.PropertyChanged -= ViewModel_PropertyChanged;
            imageUri = null;
            DataContext = null;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.QueueVM.CoverUri))
            {
                imageUri = GetAlbumUri();
                PrimaryImage.Source = new BitmapImage(imageUri);
            }
        }

        private async Task ShowAlbumArtInBackground()
        {
            //FindName(nameof(BackDropControl));
            await Task.Delay(300);
            FindName(nameof(BackgroundImage));
            ShowBGImage.Begin();
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

        private void PrimaryImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double s = e.NewSize.Height < e.NewSize.Width ? e.NewSize.Height : e.NewSize.Width;
            SongDescriptionGrid.MinWidth = s;
        }
    }
}
