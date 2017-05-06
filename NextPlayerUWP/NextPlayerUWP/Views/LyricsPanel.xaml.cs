using NextPlayerUWP.ViewModels;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace NextPlayerUWP.Views
{
    public sealed partial class LyricsPanel : UserControl
    {
        LyricsPanelViewModel ViewModel;

        public LyricsPanel()
        {
            this.InitializeComponent();
            ViewModel = (LyricsPanelViewModel)DataContext;
        }
    }
}
