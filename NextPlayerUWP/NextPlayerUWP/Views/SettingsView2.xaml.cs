using NextPlayerUWP.Common;
using NextPlayerUWP.ViewModels;
using NextPlayerUWP.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Common;
using Template10.Services.NavigationService;
using Template10.Services.SerializationService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsView2 : Page
    {
        public SettingsView2()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var items = MasterListView.ItemsSource as List<SettingsMenuItem>;

            if (items == null)
            {
                ViewModelLocator vml = new ViewModelLocator();
                items = SettingsMenuItem.GetMainItems(vml.SettingsVMService);

                MasterListView.ItemsSource = items;
            }

            if (e.Parameter != null)
            {
                // Parameter is item ID
                string name = (string)e.Parameter;
                try
                {
                    name = SerializationService.Json.Deserialize(e.Parameter?.ToString()) as string;
                }
                catch (Exception ex)
                {
                    var nav = WindowWrapper.Current().NavigationServices.FirstOrDefault();
                    nav.Navigate(typeof(SettingsDetailsView), name, new DrillInNavigationTransitionInfo());
                }
            }
        }

        private void MasterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (SettingsMenuItem)e.ClickedItem;
            var nav = WindowWrapper.Current().NavigationServices.FirstOrDefault();
            nav.Navigate(typeof(SettingsDetailsView), clickedItem.TypeName);
        }
    }

    public class SettingsMenuItem
    {
        public string Icon { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public ISettingsViewModel ViewModel { get; set; }

        public static List<SettingsMenuItem> GetMainItems(SettingsVMService service)
        {
            TranslationHelper tr = new TranslationHelper();

            var items = new List<SettingsMenuItem>();
            items.Add(new SettingsMenuItem() { Icon = "\xE8F1", Name = tr.GetTranslation("TBLibrary/Text"), ViewModel = service.GetViewModelByName(nameof(SettingsLibraryViewModel)), TypeName = nameof(SettingsLibraryViewModel) });
            items.Add(new SettingsMenuItem() { Icon = "\xE771", Name = tr.GetTranslation("TBPersonalize/Text"), ViewModel = service.GetViewModelByName(nameof(SettingsPersonalizationViewModel)), TypeName = nameof(SettingsPersonalizationViewModel) });
            items.Add(new SettingsMenuItem() { Icon = "\xE90F", Name = tr.GetTranslation("TBTools/Text"), ViewModel = service.GetViewModelByName(nameof(SettingsToolsViewModel)), TypeName = nameof(SettingsToolsViewModel) });
            items.Add(new SettingsMenuItem() { Icon = "\xE716", Name = tr.GetTranslation("TBAccounts/Text"), ViewModel = service.GetViewModelByName(nameof(SettingsAccountsViewModel)), TypeName = nameof(SettingsAccountsViewModel) });
            items.Add(new SettingsMenuItem() { Icon = "\xE946", Name = tr.GetTranslation("TBAbout/Text"), ViewModel = service.GetViewModelByName(nameof(SettingsAboutViewModel)), TypeName = nameof(SettingsAboutViewModel) });
            return items;
        }

        public static List<SettingsMenuItem> GetOptionsItems()
        {
            var items = new List<SettingsMenuItem>();
            return items;
        }
    }
}
