using NextPlayerUWP.Extensions;
using NextPlayerUWP.ViewModels.Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NextPlayerUWP.Views.Settings
{
    public sealed partial class SettingsExtensionsControl : UserControl
    {
        SettingsExtensionsViewModel ViewModel { get; set; }
        public SettingsExtensionsControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            ViewModel = (SettingsExtensionsViewModel)args.NewValue;
            ViewModel.Load();
        }

        private void InstallExtension_Click(object sender, RoutedEventArgs e)
        {
            MyAvailableExtension ext = (MyAvailableExtension)((Button)sender).Tag;
            ViewModel.InstallExtension(ext);
        }
    }
}
