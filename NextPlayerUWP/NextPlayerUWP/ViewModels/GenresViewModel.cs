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
using NextPlayerUWP.Common;

namespace NextPlayerUWP.ViewModels
{
    public class GenresViewModel : MusicViewModelBase
    {
        public GenresViewModel()
        {
            SortNames si = new SortNames(MusicItemTypes.genre);
            ComboBoxItemValues = si.GetSortNames();
            SelectedComboBoxItem = ComboBoxItemValues.FirstOrDefault();
            App.SongUpdated += App_SongUpdated;
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

        private async void App_SongUpdated(int id)
        {
            await Dispatcher.DispatchAsync(() => ReloadData());
        }

        private async Task ReloadData()
        {
            Genres = await DatabaseManager.Current.GetGenreItemsAsync();
            SortItems(null, null);
        }

        public void SortItems(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItemValue value = SelectedComboBoxItem;
            switch (value.Option)
            {
                case SortNames.Genre:
                    Sort(s => s.Genre, t => (t.Genre == "") ? "" : t.Genre[0].ToString().ToLower(), "Genre");
                    break;
                //case SortNames.Duration:
                //    Sort(s => s.Duration.TotalSeconds, t => new TimeSpan(t.Duration.Hours, t.Duration.Minutes, t.Duration.Seconds), "AlbumId");
                //    break;
                case SortNames.SongCount:
                    Sort(s => s.SongsNumber, t => t.SongsNumber, "Genre");
                    break;
                default:
                    Sort(s => s.Genre, t => (t.Genre == "") ? "" : t.Genre[0].ToString().ToLower(), "Genre");
                    break;
            }
        }

        private void Sort(Func<GenreItem, object> orderSelector, Func<GenreItem, object> groupSelector, string propertyName)
        {
            var query = genres.OrderBy(orderSelector);
            Genres = new ObservableCollection<GenreItem>(query);
        }

        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var query = genres.Where(s => s.Genre.ToLower().Contains(sender.Text)).OrderBy(s => s.Genre);
                sender.ItemsSource = query.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                NavigationService.Navigate(App.Pages.Album, ((GenreItem)args.ChosenSuggestion).GetParameter());
            }
            else
            {
                var list = genres.Where(s => s.Genre.ToLower().Contains(sender.Text)).OrderBy(s => s.Genre).ToList();
                //if (list.Count > 0)
                //{
                //    await SongClicked(list.FirstOrDefault().SongId);
                //}
                sender.ItemsSource = list;
            }
        }

        public void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var item = args.SelectedItem as GenreItem;
            sender.Text = item.Genre;
        }
    }
}
