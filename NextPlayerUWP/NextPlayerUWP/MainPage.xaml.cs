using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NextPlayerUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            MediaImport m = new MediaImport();
            Progress<int> progress = new Progress<int>(
            percent=>
            {
                procent.Text = "nowe utwory: "+percent.ToString();
            });
            await Task.Run(() => m.UpdateDatabase(progress));
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            await DatabaseManager.Current.UpdateTables();
            //int id = DatabaseManager.Current.PrepPlain();
            //List<int> list = new List<int>();
            //for (int i = 0; i < 100; i++)
            //{
            //    list.Add(i);
            //}
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            //DatabaseManager.Current.InsertPlainPlaylistEntry(id, list);
            //sw.Stop();
            //long l1 = sw.ElapsedMilliseconds;
            //sw.Restart();
            //await DatabaseManager.Current.InsertPlainPlaylistEntryAsync(id, list);
            //sw.Stop();
            //long l2 = sw.ElapsedMilliseconds;
        }
    }
}
