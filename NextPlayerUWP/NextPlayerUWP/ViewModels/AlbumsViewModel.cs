using GalaSoft.MvvmLight.Command;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWP.ViewModels
{
    public class AlbumsViewModel : MusicViewModelBase
    {
        private ObservableCollection<AlbumItem> albums = new ObservableCollection<AlbumItem>();
        public ObservableCollection<AlbumItem> Albums
        {
            get { return albums; }
            set { Set(ref albums, value); }
        }

        protected override async Task LoadData()
        {
            if (Albums.Count == 0)
            {
                Albums = await DatabaseManager.Current.GetAlbumItemsAsync();
            }
        }

        private RelayCommand<AlbumItem> itemClicked;
        public RelayCommand<AlbumItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<AlbumItem>(
                    item =>
                    {
                        NavigationService.Navigate(App.Pages.Album, item.GetParameter());
                    }));
            }
        }
    }
}
