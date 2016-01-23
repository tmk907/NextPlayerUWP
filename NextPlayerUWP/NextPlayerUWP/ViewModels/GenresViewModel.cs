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
    public class GenresViewModel : MusicViewModelBase
    {
        private ObservableCollection<GenreItem> genres = new ObservableCollection<GenreItem>();
        public ObservableCollection<GenreItem> Genres
        {
            get { return genres; }
            set { Set(ref genres, value); }
        }

        protected override async Task LoadData()
        {
            if (Genres.Count == 0)
            {
                Genres = await DatabaseManager.Current.GetGenreItemsAsync();
            }
        }

        private RelayCommand<GenreItem> itemClicked;
        public RelayCommand<GenreItem> ItemClicked
        {
            get
            {
                return itemClicked
                    ?? (itemClicked = new RelayCommand<GenreItem>(
                    itemClicked =>
                    {
                        NavigationService.Navigate(App.Pages.Playlist, itemClicked.GetParameter());
                    }));
            }
        }

    }
}
