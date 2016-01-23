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
    public class ArtistsViewModel : MusicViewModelBase
    {
        private ObservableCollection<ArtistItem> artists = new ObservableCollection<ArtistItem>();
        public ObservableCollection<ArtistItem> Artists
        {
            get { return artists; }
            set { Set(ref artists, value); }
        }

        protected override async Task LoadData()
        {
            if (Artists.Count == 0)
            {
                Artists = await DatabaseManager.Current.GetArtistItemsAsync();
            }
        }

        private RelayCommand<ArtistItem> itemClicked;
        public RelayCommand<ArtistItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<ArtistItem>(
                    item =>
                    {
                        NavigationService.Navigate(App.Pages.Artist, item.GetParameter());
                    }));
            }
        }
    }
}
