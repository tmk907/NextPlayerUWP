using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
    }
}
