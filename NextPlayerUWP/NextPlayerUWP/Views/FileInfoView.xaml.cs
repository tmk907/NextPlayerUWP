using NextPlayerUWP.ViewModels;
using Windows.ApplicationModel.DataTransfer;
using System;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FileInfoView : Page
    {
        FileInfoViewModel ViewModel;

        public FileInfoView()
        {
            this.InitializeComponent();
            ViewModel = (FileInfoViewModel)DataContext;
        }

        private async void CopyPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(ViewModel.FileInfo.DirectoryPath);
                await Launcher.LaunchFolderAsync(folder);
            }
            catch (Exception ex)
            {
            }

            //DataPackage dataPackage = new DataPackage();
            //dataPackage.RequestedOperation = DataPackageOperation.Copy;
            //dataPackage.SetText(FilePath_TextBlock.Text);
            //Clipboard.SetContent(dataPackage);
        }
    }
}
