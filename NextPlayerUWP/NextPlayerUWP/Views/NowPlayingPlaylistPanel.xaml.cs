using NextPlayerUWP.ViewModels;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace NextPlayerUWP.Views
{
    public sealed partial class NowPlayingPlaylistPanel : UserControl
    {
        NowPlayingPlaylistPanelViewModel ViewModel;

        private const double compactWidth = 0.0;
        //private const double narrowWidth = 500.0;
        private const double normalWidth = 600.0;
        private const double wideWidth = 900.0;

        private double previousState = -1;

        public NowPlayingPlaylistPanel()
        {
            this.InitializeComponent();
            ViewModelLocator vml = new ViewModelLocator();
            ViewModel = vml.NowPlayingPlaylistPanelVM;
            this.DataContext = this;
            this.Loaded += NowPlayingPlaylistPanel_Loaded;
            this.Unloaded += NowPlayingPlaylistPanel_Unloaded;
            PanelRootGrid.SizeChanged += PanelRootGrid_SizeChanged;
        }

        private void PanelRootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > wideWidth)
            {
                if (previousState != wideWidth)
                {
                    previousState = wideWidth;
                    VisualStateManager.GoToState(this, nameof(WideState), false);
                }
            }
            else if (e.NewSize.Width > normalWidth)
            {
                if (previousState != normalWidth)
                {
                    previousState = normalWidth;
                    VisualStateManager.GoToState(this, nameof(NormalState), false);
                }
            }
            //else if (e.NewSize.Width > narrowWidth)
            //{
            //    if (previousState != narrowWidth)
            //    {
            //        previousState = narrowWidth;
            //        VisualStateManager.GoToState(this, nameof(NarrowState), false);
            //    }
            //}
            else if (e.NewSize.Width > compactWidth)
            {
                if (previousState != compactWidth)
                {
                    previousState = compactWidth;
                    VisualStateManager.GoToState(this, nameof(CompactState), false);
                }
            }
        }

        private void NowPlayingPlaylistPanel_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnLoaded(NowPlayingPlaylistListView);
        }

        private void NowPlayingPlaylistPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.OnUnloaded();
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["ContextMenu"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        public Brush EvenRowBackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(EvenRowBackgroundBrushProperty); }
            set
            {
                SetValue(EvenRowBackgroundBrushProperty, value);
                NowPlayingPlaylistListView.EvenRowBackground = value;
            }
        }

        public static readonly DependencyProperty EvenRowBackgroundBrushProperty =
            DependencyProperty.Register("EvenRowBackgroundBrush", typeof(Brush), typeof(NowPlayingPlaylistPanel), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        public Brush OddRowBackgroundBrush
        {
            get { return (SolidColorBrush)GetValue(OddRowBackgroundBrushProperty); }
            set
            {
                SetValue(OddRowBackgroundBrushProperty, value);
                NowPlayingPlaylistListView.OddRowBackground = value;
            }
        }

        public static readonly DependencyProperty OddRowBackgroundBrushProperty =
            DependencyProperty.Register("OddRowBackgroundBrush", typeof(Brush), typeof(NowPlayingPlaylistPanel), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));

    }
}
