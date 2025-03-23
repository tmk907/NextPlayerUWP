using NextPlayerWinUI.ViewModels.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace NextPlayerWinUI.Views.Settings
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
