using GalaSoft.MvvmLight.Command;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Services.NavigationService;

namespace NextPlayerUWP.ViewModels
{
    public class GenresViewModel : MusicViewModelBase
    {
        public GenresViewModel()
        {
            Debug.WriteLine("genresvm constructor");
        }

        private ObservableCollection<GenreItem> genres = new ObservableCollection<GenreItem>();
        public ObservableCollection<GenreItem> Genres
        {
            get { return genres; }
            set { Set(ref genres, value); }
        }

        protected override async Task LoadData()
        {
            if (genres.Count == 0)
            {
                Genres = await DatabaseManager.Current.GetGenreItemsAsync();
            }
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.Playlist, ((GenreItem)e.ClickedItem).GetParameter());
        }
        
    }
}
