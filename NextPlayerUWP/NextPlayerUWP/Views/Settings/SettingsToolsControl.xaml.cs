using NextPlayerUWP.ViewModels.Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.Views.Settings
{
    public sealed partial class SettingsToolsControl : UserControl
    {
        SettingsToolsViewModel ViewModel { get; set; }
        public SettingsToolsControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            ViewModel = (SettingsToolsViewModel)args.NewValue;
            ViewModel.Load();
        }
    }
}
