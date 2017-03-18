using NextPlayerUWP.ViewModels;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI.ViewManagement;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWP.Common;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
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
            //NavigationCacheMode = NavigationCacheMode.Required;
            ViewModel = (NowPlayingDesktopViewModel)DataContext;
            this.Loaded += NowPlayingDesktopView_Loaded;
            this.Unloaded += NowPlayingDesktopView_Unloaded;
            InitializedFrostedGlass(GlassHost);
        }
        ~NowPlayingDesktopView()
        {
            System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        }
        private void NowPlayingDesktopView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.QueueVM.CoverUri != null)
            {
                imageUri = GetAlbumUri();
                PrimaryImage.ImageUri = imageUri;
                BackgroundImage.ImageUri = imageUri;
            }
            ViewModel.QueueVM.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.QueueVM.CoverUri))
            {
                imageUri = GetAlbumUri();
                PrimaryImage.ImageUri = imageUri;
                BackgroundImage.ImageUri = imageUri;
            }
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

        private void NowPlayingDesktopView_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.QueueVM.PropertyChanged -= ViewModel_PropertyChanged;
            //ViewModel = null;
            imageUri = null;
            DataContext = null;
        }

        private void PrimaryImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double s = e.NewSize.Height < e.NewSize.Width ? e.NewSize.Height : e.NewSize.Width;
            SongDescriptionGrid.MinWidth = s;
        }

        private void InitializedFrostedGlass(UIElement glassHost)
        {
            Visual hostVisual = ElementCompositionPreview.GetElementVisual(glassHost);
            Compositor compositor = hostVisual.Compositor;

            // Create a glass effect, requires Win2D NuGet package
            var glassEffect = new GaussianBlurEffect
            {
                BlurAmount = 15.0f,
                BorderMode = EffectBorderMode.Hard,
                Source = new ArithmeticCompositeEffect
                {
                    MultiplyAmount = 0,
                    Source1Amount = 0.5f,
                    Source2Amount = 0.5f,
                    Source1 = new CompositionEffectSourceParameter("backdropBrush"),
                    Source2 = new ColorSourceEffect
                    {
                        Color = Color.FromArgb(255, 192, 192, 192)
                    }
                }
            };

            //  Create an instance of the effect and set its source to a CompositionBackdropBrush
            var effectFactory = compositor.CreateEffectFactory(glassEffect);
            var backdropBrush = compositor.CreateBackdropBrush();
            var effectBrush = effectFactory.CreateBrush();

            effectBrush.SetSourceParameter("backdropBrush", backdropBrush);

            // Create a Visual to contain the frosted glass effect
            var glassVisual = compositor.CreateSpriteVisual();
            glassVisual.Brush = effectBrush;

            // Add the blur as a child of the host in the visual tree
            ElementCompositionPreview.SetElementChildVisual(glassHost, glassVisual);

            // Make sure size of glass host and glass visual always stay in sync
            var bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
            bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);

            glassVisual.StartAnimation("Size", bindSizeAnimation);
        }
    }
}
