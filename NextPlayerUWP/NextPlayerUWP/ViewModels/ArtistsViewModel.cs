using GalaSoft.MvvmLight.Command;
using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class ArtistsViewModel : MusicViewModelBase
    {
        public ArtistsViewModel()
        {
            SortNames si = new SortNames(MusicItemTypes.artist);
            ComboBoxItemValues = si.GetSortNames();
            SelectedComboBoxItem = ComboBoxItemValues.FirstOrDefault();
            App.SongUpdated += App_SongUpdated;
        }

        private ObservableCollection<ArtistItem> artists = new ObservableCollection<ArtistItem>();
        public ObservableCollection<ArtistItem> Artists
        {
            get { return artists; }
            set { Set(ref artists, value); }
        }

        private ObservableCollection<GroupList> groupedArtists = new ObservableCollection<GroupList>();
        public ObservableCollection<GroupList> GroupedArtists
        {
            get { return groupedArtists; }
            set { Set(ref groupedArtists, value); }
        }

        protected override async Task LoadData()
        {
            if (artists.Count == 0)
            {
                Artists = await DatabaseManager.Current.GetArtistItemsAsync();
            }
            if (groupedArtists.Count == 0)
            {
                var query = from item in artists
                            orderby item.Artist.ToLower()
                            group item by item.Artist[0].ToString().ToLower() into g
                            orderby g.Key
                            select new { GroupName = g.Key.ToUpper(), Items = g };
                foreach (var g in query)
                {
                    GroupList group = new GroupList();
                    group.Key = g.GroupName;
                    foreach (var item in g.Items)
                    {
                        group.Add(item);
                    }
                    GroupedArtists.Add(group);
                }
            }
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.Artist, ((ArtistItem)e.ClickedItem).GetParameter());
        }

        private async void App_SongUpdated(int id)
        {
            await Dispatcher.DispatchAsync(() => ReloadData());
        }

        private async Task ReloadData()
        {
            Artists = await DatabaseManager.Current.GetArtistItemsAsync();
            SortItems(null, null);
        }

        public void SortItems(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItemValue value = SelectedComboBoxItem;
            switch (value.Option)
            {
                case SortNames.Artist:
                    Sort(s => s.Artist, t => (t.Artist == "") ? "" : t.Artist[0].ToString().ToLower(), "Artist");
                    break;
                case SortNames.Duration:
                    Sort(s => s.Duration.TotalSeconds, t => new TimeSpan(t.Duration.Hours, t.Duration.Minutes, t.Duration.Seconds), "Artist");
                    break;
                case SortNames.SongCount:
                    Sort(s => s.SongsNumber, t => t.SongsNumber, "Artist");
                    break;
                case SortNames.LastAdded:
                    Sort(s => s.LastAdded, t => String.Format("{0:d}", t.LastAdded), "Artist");
                    break;
                default:
                    Sort(s => s.Artist, t => (t.Artist == "") ? "" : t.Artist[0].ToString().ToLower(), "Artist");
                    break;
            }
        }

        private void Sort(Func<ArtistItem, object> orderSelector, Func<ArtistItem, object> groupSelector, string propertyName)
        {
            var query = artists.OrderBy(orderSelector).
                GroupBy(groupSelector).
                OrderBy(g => g.Key).
                Select(group => new { GroupName = group.Key.ToString().ToUpper(), Items = group });
            int i = 0;
            string s;
            GroupedArtists.Clear();
            foreach (var g in query)
            {
                i = 0;
                s = "";
                GroupList group = new GroupList();
                group.Key = g.GroupName;
                foreach (var item in g.Items)
                {
                    string prop = item.GetType().GetProperty(propertyName).GetValue(item, null).ToString();
                    if (group.Count != 0 && prop != s) i++;
                    item.Index = i;
                    s = prop;
                    group.Add(item);
                }
                GroupedArtists.Add(group);
            }
        }

        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var matchingAlbums = artists.Where(s => s.Artist.ToLower().Contains(sender.Text)).OrderBy(s => s.Artist);
                sender.ItemsSource = matchingAlbums.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                NavigationService.Navigate(App.Pages.Album, ((ArtistItem)args.ChosenSuggestion).GetParameter());
            }
            else
            {
                var list = artists.Where(s => s.Artist.ToLower().Contains(sender.Text)).OrderBy(s => s.Artist).ToList();
                //if (list.Count > 0)
                //{
                //    await SongClicked(list.FirstOrDefault().SongId);
                //}
                sender.ItemsSource = list;
            }
        }

        public void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var item = args.SelectedItem as ArtistItem;
            sender.Text = item.Artist;
        }
    }
}
