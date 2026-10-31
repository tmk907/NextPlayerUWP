using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace NextPlayerWinUI
{
    public sealed partial class MainWindow : Window
    {
        private const int MinimumWidth = 500;
        private const int MinimumHeight = 500;

        public MainWindow()
        {
            InitializeComponent();

            if (AppWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.PreferredMinimumWidth = MinimumWidth;
                presenter.PreferredMinimumHeight = MinimumHeight;
            }
        }
    }
}
