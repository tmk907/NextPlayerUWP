using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.ViewModels
{
    public class NowPlayingPlaylistViewModel : MusicViewModelBase
    {
        private ObservableCollection<SongItem> songs;
        public ObservableCollection<SongItem> Songs
        {
            get { return songs; }
            set { Set(ref songs, value); }
        }

        protected override async Task LoadData()
        {
            if (Songs.Count == 0)
            {
                //Songs = ;
            }
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            //NavigationService.Navigate(App.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }

        public void EditTags(object sender, RoutedEventArgs e)
        {
            SelectedItem = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            NavigationService.Navigate(App.Pages.TagsEditor, ((SongItem)SelectedItem).SongId);
        }

        public void ShowDetails(object sender, RoutedEventArgs e)
        {
            SelectedItem = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            NavigationService.Navigate(App.Pages.FileInfo, ((SongItem)SelectedItem).SongId);
        }
    }
}
