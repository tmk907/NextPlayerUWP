using NextPlayerWinUI.ViewModels.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace NextPlayerWinUI.Views.Settings
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
