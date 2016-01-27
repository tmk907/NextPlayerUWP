using GalaSoft.MvvmLight.Command;
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

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);
            if (!isBack)
            {
                Genres = new ObservableCollection<GenreItem>();
            }
        }
        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.Playlist, ((GenreItem)e.ClickedItem).GetParameter());
        }
        
    }
}
