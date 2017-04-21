using NextPlayerUWP.Controls;
using NextPlayerUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Template10.Common;
using Template10.Services.SerializationService;
using Windows.UI.Xaml.Controls;
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

        //~SettingsView2()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.OnNavigatedToNewView(true);

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
                    nav.Navigate(typeof(SettingsDetailsView), name);
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
}
