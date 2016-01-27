using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class AlbumViewModel : MusicViewModelBase
    {
        private string albumParam;

        private AlbumItem album = new AlbumItem();
        public AlbumItem Album
        {
            get { return album; }
            set { Set(ref album, value); }
        }

        private ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Songs
        {
            get { return songs; }
            set { Set(ref songs, value); }
        }

        protected override async Task LoadData()
        {
            Songs = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(albumParam);
            Album = await DatabaseManager.Current.GetAlbumItemAsync(albumParam);
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);
            if (!isBack)
            {
                Songs = new ObservableCollection<SongItem>();
            }
            if (parameter != null)
            {
                try
                {
                    albumParam = (MusicItem.ParseParameter(parameter as string))[1];
                }
                catch (Exception ex) { }
            }

        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }
    }
}
