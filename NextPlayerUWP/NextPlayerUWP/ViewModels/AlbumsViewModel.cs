using GalaSoft.MvvmLight.Command;
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

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);
            if (!isBack)
            {
                Albums = new ObservableCollection<AlbumItem>();
            }
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.Album, ((AlbumItem)e.ClickedItem).GetParameter());
        }
    }
}
