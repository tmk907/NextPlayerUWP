using NextPlayerUWP.ViewModels.Settings;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.Views.Settings
{
    public sealed partial class SettingsAboutControl : UserControl
    {
        SettingsAboutViewModel ViewModel { get; set; }
        public SettingsAboutControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            ViewModel = (SettingsAboutViewModel)args.NewValue;
            ViewModel.Load();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await ContentDialogTranslations.ShowAsync();
        }
    }
}
