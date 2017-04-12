using NextPlayerUWP.ViewModels;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LyricsView : Page
    {
        LyricsViewModel ViewModel;
        private WebView webView;

        public LyricsView()
        {
            this.InitializeComponent();
            this.Loaded += LyricsView_Loaded;
            this.Unloaded += LyricsView_Unloaded;
            ViewModel = (LyricsViewModel)DataContext;
        }
        //~LyricsView()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}
        private void LyricsView_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            webView = ViewModel.GetWebView();
            WebGrid.Children.Add(webView);
        }

        private void LyricsView_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            WebGrid.Children.Remove(webView);
            webView = null;
        }
    }
}
