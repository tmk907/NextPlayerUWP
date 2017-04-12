using NextPlayerUWP.ViewModels.Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.Views.Settings
{
    public sealed partial class SettingsAccountsControl : UserControl
    {
        SettingsAccountsViewModel ViewModel { get; set; }
        public SettingsAccountsControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            ViewModel = (SettingsAccountsViewModel)args.NewValue;
            ViewModel.Load();
        }
    }
}
