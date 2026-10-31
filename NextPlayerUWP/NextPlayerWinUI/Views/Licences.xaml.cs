using Newtonsoft.Json;
using NextPlayerWinUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerWinUI.Views
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
            string assets = Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets");

            string text = await File.ReadAllTextAsync(Path.Combine(assets, "Licenses.json"));
            MITLicense = await File.ReadAllTextAsync(Path.Combine(assets, "MITLicense.txt"));
            ApacheLicense = await File.ReadAllTextAsync(Path.Combine(assets, "ApacheLicense.txt"));
            TagLibLicenseAddon = await File.ReadAllTextAsync(Path.Combine(assets, "TagLibLicenseAddon.txt"));

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
