using NextPlayerUWP.Common;
using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsView : Page
    {
        SettingsViewModel ViewModel;
        public SettingsView()
        {
            this.InitializeComponent();
            ViewModel = (SettingsViewModel)DataContext;
        }
        
        private void RemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            MusicFolder folder = (MusicFolder)((Button)sender).Tag;
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
