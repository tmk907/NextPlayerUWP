using NextPlayerUWP.ViewModels;
using NextPlayerUWPDataLayer.Model;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NextPlayerUWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TagsEditor : Page
    {
        TagsEditorViewModel ViewModel;
        public TagsEditor()
        {
            this.InitializeComponent();
            ViewModel = (TagsEditorViewModel)DataContext;
        }

        //~TagsEditor()
        //{
        //    System.Diagnostics.Debug.WriteLine("~" + GetType().Name);
        //}

        private void Image_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["ContextMenu"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        private void Image_Tapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            var menu = this.Resources["ContextMenu"] as MenuFlyout;
            var position = e.GetPosition(senderElement);
            menu.ShowAt(senderElement, position);
        }

        private async void AddFromSong_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetListView(songslist);
            var result = await ContentDialogAddFromSong.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                ViewModel.AddFromSong((SongItem)songslist.SelectedItem);
            }
        }

        private void songslist_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            ContentDialogAddFromSong.Hide();
            ViewModel.AddFromSong((SongItem)songslist.SelectedItem);
        }
    }
}
