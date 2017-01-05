using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Toolkit.Uwp.UI.Animations;
using NextPlayerUWP.Common;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Constants;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.Graphics.Effects;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NowPlayingView : Page
    {
        Compositor _compositor;
        CompositionEffectBrush _crossFadeBrushPrimary;
        CompositionSurfaceBrush _previousSurfaceBrushPrimary;
        CompositionScopedBatch _crossFadeBatchPrimary;

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
            LoadSliderEvents(sender, e);
            Init();
            if (ViewModel.CoverUri != null)
            {
                if (first)
                {
                    imageUri = GetAlbumUri();
                    PrimaryImage.Source = imageUri;
                    first = false;
                }
            }
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void Init()
        {
            first = true;
            _compositor = ElementCompositionPreview.GetElementVisual(PortraitGrid).Compositor;
            PrimaryImage.ImageOpened += PrimaryImage_FirstOpened;
            PrimaryImage.PlaceholderDelay = TimeSpan.FromMilliseconds(-1);
            PrimaryImage.PlaceholderDelay = TimeSpan.FromMilliseconds(-1);
            PrimaryImage.SharedSurface = true;

            // Create a crossfade brush to animate image transitions
            IGraphicsEffect graphicsEffect = new ArithmeticCompositeEffect()
            {
                Name = "CrossFade",
                Source1Amount = 0,
                Source2Amount = 1,
                MultiplyAmount = 0,
                Source1 = new CompositionEffectSourceParameter("ImageSource"),
                Source2 = new CompositionEffectSourceParameter("ImageSource2"),
            };
            CompositionEffectFactory factory = _compositor.CreateEffectFactory(graphicsEffect, new[] { "CrossFade.Source1Amount", "CrossFade.Source2Amount" });
            _crossFadeBrushPrimary = factory.CreateBrush();
        }

        private void NowPlayingView_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            PrimaryImage.ImageOpened -= PrimaryImage_FirstOpened;
            PrimaryImage = null;
            _compositor = null;
            if (_crossFadeBatchPrimary != null)
            {
                _crossFadeBatchPrimary.Dispose();
                _crossFadeBatchPrimary = null;

            }
            if (_crossFadeBrushPrimary != null)
            {
                _crossFadeBrushPrimary.Dispose();
                _crossFadeBrushPrimary = null;
            }
            if (_previousSurfaceBrushPrimary != null)
            {
                _previousSurfaceBrushPrimary.Dispose();
                _previousSurfaceBrushPrimary = null;
            }
            UnloadSliderEvents();
        }

        #region Slider 
        private void LoadSliderEvents(object sender, RoutedEventArgs e)
        {
            timeslider.AddHandler(Control.PointerPressedEvent, pointerpressedhandler, true);
            timeslider.AddHandler(Control.PointerCaptureLostEvent, pointerreleasedhandler, true);            
        }

        private void UnloadSliderEvents()
        {
            timeslider.RemoveHandler(Control.PointerPressedEvent, pointerpressedhandler);
            timeslider.RemoveHandler(Control.PointerPressedEvent, pointerreleasedhandler);
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
            if (ViewModel.CoverUri == new Uri(AppConstants.SongCoverBig))
            {
                ColorsHelper ch = new ColorsHelper();
                return new Uri(ch.GetAlbumCoverAssetWithCurrentAccentColor());
            }
            else
            {
                return ViewModel.CoverUri;
            }
        }

        bool first;
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.CoverUri))
            {
                imageUri = GetAlbumUri();
                if (first)
                {
                    PrimaryImage.Source = imageUri;
                    first = false;
                }
                else
                {
                    StartAnimations();
                }
            }
        }

        private void StartAnimations()
        {
            if (_previousSurfaceBrushPrimary == null) return;
            if (_crossFadeBatchPrimary == null)
            {
                // Save the previous image for a cross-fade
                _previousSurfaceBrushPrimary.Surface = PrimaryImage.SurfaceBrush.Surface;
                _previousSurfaceBrushPrimary.CenterPoint = PrimaryImage.SurfaceBrush.CenterPoint;
                _previousSurfaceBrushPrimary.Stretch = PrimaryImage.SurfaceBrush.Stretch;

                // Load the new background image
                PrimaryImage.ImageOpened += PrimaryImage_ImageChanged;
            }

            // Update the images
            PrimaryImage.Source = imageUri;
        }

        private void PrimaryImage_ImageChanged(object sender, RoutedEventArgs e)
        {
            if (_crossFadeBatchPrimary == null)
            {
                TimeSpan duration = TimeSpan.FromMilliseconds(1000);

                // Create the animations for cross-fading
                ScalarKeyFrameAnimation fadeInAnimation = _compositor.CreateScalarKeyFrameAnimation();
                fadeInAnimation.InsertKeyFrame(0, 0);
                fadeInAnimation.InsertKeyFrame(1, 1);
                fadeInAnimation.Duration = duration;

                ScalarKeyFrameAnimation fadeOutAnimation = _compositor.CreateScalarKeyFrameAnimation();
                fadeOutAnimation.InsertKeyFrame(0, 1);
                fadeOutAnimation.InsertKeyFrame(1, 0);
                fadeOutAnimation.Duration = duration;

                // Create a batch object so we can cleanup when the cross-fade completes.
                _crossFadeBatchPrimary = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

                // Set the sources
                _crossFadeBrushPrimary.SetSourceParameter("ImageSource", PrimaryImage.SurfaceBrush);
                _crossFadeBrushPrimary.SetSourceParameter("ImageSource2", _previousSurfaceBrushPrimary);

                // Animate the source amounts to fade between
                _crossFadeBrushPrimary.StartAnimation("CrossFade.Source1Amount", fadeInAnimation);
                _crossFadeBrushPrimary.StartAnimation("CrossFade.Source2Amount", fadeOutAnimation);

                // Update the image to use the cross fade brush
                PrimaryImage.Brush = _crossFadeBrushPrimary;

                _crossFadeBatchPrimary.Completed += PrimaryBatch_CrossFadeCompleted;
                _crossFadeBatchPrimary.End();
            }

            // Unhook the handler
            PrimaryImage.ImageOpened -= PrimaryImage_ImageChanged;
        }

        private void PrimaryBatch_CrossFadeCompleted(object sender, CompositionBatchCompletedEventArgs args)
        {
            PrimaryImage.Brush = PrimaryImage.SurfaceBrush;
            // Dispose the image
            ((CompositionDrawingSurface)_previousSurfaceBrushPrimary.Surface).Dispose();
            _previousSurfaceBrushPrimary.Surface = null;

            // Clear out the batch
            _crossFadeBatchPrimary = null;
        }

        private void PrimaryImage_FirstOpened(object sender, RoutedEventArgs e)
        {
            ScalarKeyFrameAnimation fadeInAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeInAnimation.InsertKeyFrame(0, 0);
            fadeInAnimation.InsertKeyFrame(1, 1);
            fadeInAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            PrimaryImage.SpriteVisual.StartAnimation("Opacity", fadeInAnimation);

            CompositionDrawingSurface surface = (CompositionDrawingSurface)PrimaryImage.SurfaceBrush.Surface;
            PrimaryImage.SurfaceBrush.CenterPoint = new Vector2((float)surface.Size.Width, (float)surface.Size.Height) * .5f;

            _previousSurfaceBrushPrimary = _compositor.CreateSurfaceBrush();

            PrimaryImage.ImageOpened -= PrimaryImage_FirstOpened;
        }

        //private void CoverImage_ImageOpened(object sender, RoutedEventArgs e)
        //{
        //    var anim1 = CoverImage.Fade(1, 400, 0);
        //    var anim2 = CoverImage2.Fade(0, 500, 0);
        //    anim2.Completed += Anim2_Completed;
        //    anim1.Completed += Anim1_Completed;
        //    anim1.Start();
        //    anim2.Start();
        //}

        //private void Anim1_Completed(object sender, EventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("Anim1_Completed");
        //}

        //private void Anim2_Completed(object sender, EventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("Anim2_Completed");

        //    BitmapImage source1 = CoverImage.Source as BitmapImage;
        //    BitmapImage bmp = new BitmapImage(source1.UriSource);
        //    bmp.DecodePixelHeight = source1.DecodePixelHeight;
        //    bmp.DecodePixelWidth = source1.DecodePixelWidth;
        //    CoverImage2.Source = bmp;
        //    CoverImage2.Opacity = 1;
        //    CoverImage.Opacity = 0;
        //}
    }    
}
