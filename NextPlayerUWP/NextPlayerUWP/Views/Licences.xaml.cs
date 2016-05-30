using System;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Licences : Page
    {
        public Licences()
        {
            this.InitializeComponent();
            OpenFile();
        }

        private async void OpenFile()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
            StorageFile file = await assets.GetFileAsync("Licenses.txt");
            string text = await Windows.Storage.FileIO.ReadTextAsync(file);
            LicensesTextBlock.Text = text;
        }
    }
}
