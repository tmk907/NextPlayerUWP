using NextPlayerWinUI.Controls;
using NextPlayerWinUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerWinUI.Views
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

                    }

        private void MasterListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = (SettingsMenuItem)e.ClickedItem;
            var nav = App.NavigationService;
            nav.Navigate(typeof(SettingsDetailsView), clickedItem.TypeName);
        }
    }    
}
