using Newtonsoft.Json;
using NextPlayerUWP.Models;
using System;
using System.Collections.Generic;
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
        public List<LicenseModel> Licenses { get; set; }
        private string MITLicense;
        private string ApacheLicense;
        private string TagLibLicenseAddon;

        public Licences()
        {
            this.InitializeComponent();
            OpenFile();
        }

        private async void OpenFile()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
            var fileLicenses = await assets.GetFileAsync("Licenses.json");
            var fileMIT = await assets.GetFileAsync("MITLicense.txt");
            var fileApache = await assets.GetFileAsync("ApacheLicense.txt");
            var fileTagLib = await assets.GetFileAsync("TagLibLicenseAddon.txt");

            string text = await FileIO.ReadTextAsync(fileLicenses);
            MITLicense = await FileIO.ReadTextAsync(fileMIT);
            ApacheLicense = await FileIO.ReadTextAsync(fileApache);
            TagLibLicenseAddon = await FileIO.ReadTextAsync(fileTagLib);

            var allLicenses = JsonConvert.DeserializeObject<LicensesModel>(text);
            Licenses = allLicenses.Licenses;
            foreach(var license in Licenses)
            {
                if (license.License.Type.Contains("MIT"))
                {
                    license.License.Content = MITLicense;
                    if (license.Name.Contains("TagLib"))
                    {
                        license.License.Content += Environment.NewLine;
                        license.License.Content += Environment.NewLine;
                        license.License.Content += TagLibLicenseAddon;
                    }
                }
                else if (license.License.Type.Contains("Apache"))
                {
                    license.License.Content = ApacheLicense;
                }
            }

            LicensesListView.ItemsSource = Licenses;
        }
    }
}
