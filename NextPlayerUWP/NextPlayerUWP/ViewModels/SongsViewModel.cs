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
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class SongsViewModel : MusicViewModelBase
    {
        private ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Songs
        {
            get { return songs; }
            set { Set(ref songs, value); }
        }

        protected override async Task LoadData()
        {
            if (Songs.Count == 0)
            {
                Songs = await DatabaseManager.Current.GetSongItemsAsync();
            }
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);
            if (!isBack)
            {
                Songs = new ObservableCollection<SongItem>();
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
