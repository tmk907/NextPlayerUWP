using NextPlayerUWP.Common;
using NextPlayerUWP.ViewModels.Settings;
using NextPlayerUWPDataLayer.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.Views.Settings
{
    public sealed partial class SettingsLibraryControl : UserControl
    {
        SettingsLibraryViewModel ViewModel { get; set; }
        public SettingsLibraryControl()
        {
            this.InitializeComponent();
            this.DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            ViewModel = (SettingsLibraryViewModel)args.NewValue;
            ViewModel.Load();
        }

        private void RemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            SdCardFolder folder = (SdCardFolder)((Button)sender).Tag;
            ViewModel.RemoveFolder(folder);
        }

        private async void RemoveSdCardFolder_Click(object sender, RoutedEventArgs e)
        {
            SdCardFolder folder = (SdCardFolder)((Button)sender).Tag;
            MessageDialogHelper msg = new MessageDialogHelper();
            bool remove = await msg.ShowExcludeFolderConfirmation();
            if (remove)
            {
                ViewModel.RemoveSdCardFolder(folder);
            }
        }
    }
}
