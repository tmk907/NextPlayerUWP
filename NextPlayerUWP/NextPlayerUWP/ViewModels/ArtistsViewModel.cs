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
            MediaImport.MediaImported += MediaImport_MediaImported;
        }

        private async void MediaImport_MediaImported(string s)
        {
            await Dispatcher.DispatchAsync(() => ReloadData());
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
            NavigationService.Navigate(App.Pages.Artist, ((ArtistItem)e.ClickedItem).ArtistId);
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
                    Sort(s => s.Duration.TotalSeconds, t => new TimeSpan(t.Duration.Hours, t.Duration.Minutes, t.Duration.Seconds), "Artist", "duration");
                    break;
                case SortNames.SongCount:
                    Sort(s => s.SongsNumber, t => t.SongsNumber, "Artist");
                    break;
                case SortNames.LastAdded:
                    Sort(s => s.LastAdded.Ticks, t => String.Format("{0:d}", t.LastAdded), "Artist", "date");
                    break;
                default:
                    Sort(s => s.Artist, t => (t.Artist == "") ? "" : t.Artist[0].ToString().ToLower(), "Artist");
                    break;
            }
        }

        private void Sort(Func<ArtistItem, object> orderSelector, Func<ArtistItem, object> groupSelector, string propertyName, string format = "no")
        {
            var query = artists.OrderBy(orderSelector).
                GroupBy(groupSelector).
                OrderBy(g => g.Key).
                Select(group => new { GroupName = (format != "duration") ? group.Key.ToString().ToUpper() 
                : (((TimeSpan)group.Key).Hours == 0) ? ((TimeSpan)group.Key).ToString(@"m\:ss") 
                : (((TimeSpan)group.Key).Days == 0) ? ((TimeSpan)group.Key).ToString(@"h\:mm\:ss") 
                : ((TimeSpan)group.Key).ToString(@"d\.hh\:mm\:ss"), Items = group });
            int i = 0;
            string s;
            GroupedArtists.Clear();
            foreach (var g in query)
            {
                i = 0;
                s = "";
                GroupList group = new GroupList();
                group.Key = g.GroupName;
                group.Header = format;
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
                string query = sender.Text.ToLower();
                var m1 = artists.Where(s => s.Artist.ToLower().StartsWith(query));
                var m2 = artists.Where(s => s.Artist.ToLower().Contains(query));
                var result = m1.Concat(m2).Distinct();
                sender.ItemsSource = result.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string artist;
            if (args.ChosenSuggestion != null)
            {
                artist = ((ArtistItem)args.ChosenSuggestion).Artist;
            }
            else
            {
                var list = artists.Where(s => s.Artist.ToLower().StartsWith(args.QueryText.ToLower())).OrderBy(s => s.Artist).ToList();
                if (list.Count == 0) return;
                artist = list.FirstOrDefault().Artist;
            }
            int index = 0;
            bool find = false;
            foreach (var group in groupedArtists)
            {
                foreach (ArtistItem item in group)
                {
                    if (item.Artist == artist)
                    {
                        find = true;
                        break;
                    }
                    index++;
                }
                if (find) break;
            }
            listView.ScrollIntoView(listView.Items[index], ScrollIntoViewAlignment.Leading);
        }

        public void AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var item = args.SelectedItem as ArtistItem;
            sender.Text = item.Artist;
        }
    }
}
