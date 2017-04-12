using NextPlayerUWP.ViewModels.Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.Views.Settings
{
    public sealed partial class SettingsPersonalizationControl : UserControl
    {
        SettingsPersonalizationViewModel ViewModel { get; set; }
        public SettingsPersonalizationControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            ViewModel = (SettingsPersonalizationViewModel)args.NewValue;
            ViewModel.Load();
        }
    }
}
