using NextPlayerUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Robmikh.CompositionSurfaceFactory;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Graphics.Effects;
using Microsoft.Graphics.Canvas;
using Windows.Graphics.DirectX;
using Microsoft.Graphics.Canvas.UI.Composition;
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
        Compositor _compositor;
        CompositionEffectBrush _crossFadeBrushBackground;
        CompositionSurfaceBrush _previousSurfaceBrushBackground;
        CompositionScopedBatch _crossFadeBatchBackground;

        CompositionEffectBrush _crossFadeBrushPrimary;
        CompositionSurfaceBrush _previousSurfaceBrushPrimary;
        CompositionScopedBatch _crossFadeBatchPrimary;

        private Uri imageUri;

        NowPlayingDesktopViewModel ViewModel;
        public NowPlayingDesktopView()
        {
            this.InitializeComponent();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(340, 500));
            System.Diagnostics.Debug.WriteLine("np1a");
            NavigationCacheMode = NavigationCacheMode.Required;
            ViewModel = (NowPlayingDesktopViewModel)DataContext;
            first = true;
            this.Loaded += NowPlayingDesktopView_Loaded;
            this.Unloaded += NowPlayingDesktopView_Unloaded;
            _compositor = ElementCompositionPreview.GetElementVisual(ContentGrid).Compositor;
            System.Diagnostics.Debug.WriteLine("np1b");
            BackgroundImage.ImageOpened += BackgroundImage_FirstOpened;
            PrimaryImage.ImageOpened += PrimaryImage_FirstOpened;

            // Disable the placeholder as we'll be using a transition
            BackgroundImage.PlaceholderDelay = TimeSpan.FromMilliseconds(-1);
            BackgroundImage.PlaceholderDelay = TimeSpan.FromMilliseconds(-1);
            BackgroundImage.SharedSurface = true;

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
            System.Diagnostics.Debug.WriteLine("np1c");
            CompositionEffectFactory factory = _compositor.CreateEffectFactory(graphicsEffect, new[] { "CrossFade.Source1Amount", "CrossFade.Source2Amount" });
            _crossFadeBrushBackground = factory.CreateBrush();
            _crossFadeBrushPrimary = factory.CreateBrush();
            InitializedFrostedGlass(GlassHost);
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            System.Diagnostics.Debug.WriteLine("np2");

        }

        private void NowPlayingDesktopView_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CoverUri != null)
            {
                if (first)
                {
                    imageUri = GetAlbumUri();
                    PrimaryImage.Source = imageUri;
                    BackgroundImage.Source = imageUri;
                    first = false;
                }
            }
            //ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            System.Diagnostics.Debug.WriteLine("np3");

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
                    BackgroundImage.Source = imageUri;
                    first = false;
                }
                else
                {
                    StartAnimations();
                }
            }
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

        private void NowPlayingDesktopView_Unloaded(object sender, RoutedEventArgs e)
        {
            //ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        private void PrimaryImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double s = e.NewSize.Height < e.NewSize.Width ? e.NewSize.Height : e.NewSize.Width;
            SongDescriptionGrid.MinWidth = s;
        }

        //private void Init()
        //{
        //    BlendEffectMode blendmode = BlendEffectMode.LighterColor;

        //    // Create a chained effect graph using a BlendEffect, blending color and blur
        //    var graphicsEffect = new BlendEffect
        //    {
        //        Mode = blendmode,
        //        Background = new ColorSourceEffect()
        //        {
        //            Name = "Tint",
        //            Color = Colors.Red,
        //        },

        //        Foreground = new GaussianBlurEffect()
        //        {
        //            Name = "Blur",
        //            Source = new CompositionEffectSourceParameter("Backdrop"),
        //            BlurAmount = (float)5,
        //            BorderMode = EffectBorderMode.Hard,
        //        }
        //    };

        //    var blurEffectFactory = _compositor.CreateEffectFactory(graphicsEffect,
        //        new[] { "Blur.BlurAmount", "Tint.Color" });

        //    // Create EffectBrush, BackdropBrush and SpriteVisual
        //    _brush = blurEffectFactory.CreateBrush();

        //    // If the animation is running, restart it on the new brush
        //    //if (AnimateToggle.IsOn)
        //    //{
        //    //    StartBlurAnimation();
        //    //}

        //    var destinationBrush = _compositor.CreateBackdropBrush();
        //    _brush.SetSourceParameter("Backdrop", destinationBrush);

        //    var blurSprite = _compositor.CreateSpriteVisual();
        //    blurSprite.Size = new Vector2((float)BackgroundImage.ActualWidth, (float)BackgroundImage.ActualHeight);
        //    blurSprite.Brush = _brush;

        //    ElementCompositionPreview.SetElementChildVisual(BackgroundImage, blurSprite);
        //}

        private void StartAnimations()
        {
            if (_previousSurfaceBrushPrimary == null || _previousSurfaceBrushBackground == null) return;
            if (_crossFadeBatchBackground == null)
            {
                // Save the previous image for a cross-fade
                _previousSurfaceBrushBackground.Surface = BackgroundImage.SurfaceBrush.Surface;
                _previousSurfaceBrushBackground.CenterPoint = BackgroundImage.SurfaceBrush.CenterPoint;
                _previousSurfaceBrushBackground.Stretch = BackgroundImage.SurfaceBrush.Stretch;

                // Load the new background image
                BackgroundImage.ImageOpened += BackgroundImage_ImageChanged;
            }

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
            BackgroundImage.Source = imageUri;
            PrimaryImage.Source = imageUri;
        }

        private void BackgroundImage_ImageChanged(object sender, RoutedEventArgs e)
        {
            if (_crossFadeBatchBackground == null)
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
                _crossFadeBatchBackground = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

                // Set the sources
                _crossFadeBrushBackground.SetSourceParameter("ImageSource", BackgroundImage.SurfaceBrush);
                _crossFadeBrushBackground.SetSourceParameter("ImageSource2", _previousSurfaceBrushBackground);

                // Animate the source amounts to fade between
                _crossFadeBrushBackground.StartAnimation("CrossFade.Source1Amount", fadeInAnimation);
                _crossFadeBrushBackground.StartAnimation("CrossFade.Source2Amount", fadeOutAnimation);

                // Update the image to use the cross fade brush
                BackgroundImage.Brush = _crossFadeBrushBackground;

                _crossFadeBatchBackground.Completed += BackgroundBatch_CrossFadeCompleted;
                _crossFadeBatchBackground.End();
            }

            // Unhook the handler
            BackgroundImage.ImageOpened -= BackgroundImage_ImageChanged;
        }

        private void BackgroundBatch_CrossFadeCompleted(object sender, CompositionBatchCompletedEventArgs args)
        {
            BackgroundImage.Brush = BackgroundImage.SurfaceBrush;

            // Dispose the image
            ((CompositionDrawingSurface)_previousSurfaceBrushBackground.Surface).Dispose();
            _previousSurfaceBrushBackground.Surface = null;

            // Clear out the batch
            _crossFadeBatchBackground = null;
        }

        private void BackgroundImage_FirstOpened(object sender, RoutedEventArgs e)
        {
            // Image loaded, let's show the content
            //this.Opacity = 1;

            // Show the content now that we should have something.
            ScalarKeyFrameAnimation fadeInAnimation = _compositor.CreateScalarKeyFrameAnimation();
            fadeInAnimation.InsertKeyFrame(0, 0);
            fadeInAnimation.InsertKeyFrame(1, 1);
            fadeInAnimation.Duration = TimeSpan.FromMilliseconds(1000);
            BackgroundImage.SpriteVisual.StartAnimation("Opacity", fadeInAnimation);
            //ElementCompositionPreview.GetElementVisual(ImageList).StartAnimation("Opacity", fadeInAnimation);

            //// Start a slow UV scale to create movement in the background image

            CompositionDrawingSurface surface = (CompositionDrawingSurface)BackgroundImage.SurfaceBrush.Surface;
            BackgroundImage.SurfaceBrush.CenterPoint = new Vector2((float)surface.Size.Width, (float)surface.Size.Height) * .5f;
            //BackgroundImage.SurfaceBrush.StartAnimation("Scale", scaleAnimation);

            // Start the animation of the cross-fade brush so they're in sync
            _previousSurfaceBrushBackground = _compositor.CreateSurfaceBrush();
            //_previousSurfaceBrush.StartAnimation("Scale", scaleAnimation);

            BackgroundImage.ImageOpened -= BackgroundImage_FirstOpened;
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
