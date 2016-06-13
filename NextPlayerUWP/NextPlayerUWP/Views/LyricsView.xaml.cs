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
        public LyricsView()
        {
            this.InitializeComponent();
            WebView web = new WebView();
            WebGrid.Children.Add(web);
            this.Loaded += delegate { ((LyricsViewModel)DataContext).OnLoaded(web); };
            ViewModel = (LyricsViewModel)DataContext;
        }
    }
}
