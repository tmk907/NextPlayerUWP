using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.ViewModels
{
    public class AlbumArtistsViewModel : MusicViewModelBase, IGroupedItemsList
    {
        SortingHelperForAlbumArtistItems sortingHelper;

        public AlbumArtistsViewModel()
        {
            sortingHelper = new SortingHelperForAlbumArtistItems("AlbumArtists");
            ComboBoxItemValues = sortingHelper.ComboBoxItemValues;
            SelectedComboBoxItem = sortingHelper.SelectedSortOption;
            App.SongUpdated += App_SongUpdated;
            MediaImport.MediaImported += MediaImport_MediaImported;
        }

        private async void MediaImport_MediaImported(string s)
        {
            await Dispatcher.DispatchAsync(() => ReloadData());
        }

        private async void App_SongUpdated(int id)
        {
            await Dispatcher.DispatchAsync(() => ReloadData());
        }

        private ObservableCollection<AlbumArtistItem> albumArtists = new ObservableCollection<AlbumArtistItem>();
        public ObservableCollection<AlbumArtistItem> AlbumArtists
        {
            get { return albumArtists; }
            set { Set(ref albumArtists, value); }
        }

        private ObservableCollection<GroupList> groupedAlbumArtists = new ObservableCollection<GroupList>();
        public ObservableCollection<GroupList> GroupedAlbumArtists
        {
            get { return groupedAlbumArtists; }
            set { Set(ref groupedAlbumArtists, value); }
        }

        private async Task ReloadData()
        {
            AlbumArtists = await DatabaseManager.Current.GetAlbumArtistItemsAsync();
            SortMusicItems();
        }

        protected override async Task LoadData()
        {
            if (albumArtists.Count == 0)
            {
                AlbumArtists = await DatabaseManager.Current.GetAlbumArtistItemsAsync();
            }
            if (groupedAlbumArtists.Count == 0)
            {
                var query = from item in albumArtists
                            orderby item.DisplayAlbumArtist.ToLower()
                            group item by item.DisplayAlbumArtist.FirstOrDefault().ToString().ToLower() into g
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
                    GroupedAlbumArtists.Add(group);
                }
                SortMusicItems();
            }
        }

        public override void FreeResources()
        {
            groupedAlbumArtists = null;
            albumArtists = null;
            groupedAlbumArtists = new ObservableCollection<GroupList>();
            albumArtists = new ObservableCollection<AlbumArtistItem>();
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.AlbumArtist, ((AlbumArtistItem)e.ClickedItem).AlbumArtistId);
        }

        protected override void SortMusicItems()
        {
            sortingHelper.SelectedSortOption = selectedComboBoxItem;
            var orderSelector = sortingHelper.GetOrderBySelector();
            var groupSelector = sortingHelper.GetGroupBySelector();
            string propertyName = sortingHelper.GetPropertyName();
            string format = sortingHelper.GetFormat();

            var query = albumArtists.OrderBy(orderSelector).
                GroupBy(groupSelector).
                OrderBy(g => g.Key).
                Select(group => new {
                    GroupName = (format != "duration") ? group.Key.ToString().ToUpper()
                : (((TimeSpan)group.Key).Hours == 0) ? ((TimeSpan)group.Key).ToString(@"m\:ss")
                : (((TimeSpan)group.Key).Days == 0) ? ((TimeSpan)group.Key).ToString(@"h\:mm\:ss")
                : ((TimeSpan)group.Key).ToString(@"d\.hh\:mm\:ss"),
                    Items = group
                });
            int i = 0;
            string s;
            GroupedAlbumArtists.Clear();
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
                GroupedAlbumArtists.Add(group);
            }
        }

        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = sender.Text.ToLower();
                var m1 = albumArtists.Where(s => s.DisplayAlbumArtist.ToLower().StartsWith(query));
                var m2 = albumArtists.Where(s => s.DisplayAlbumArtist.ToLower().Contains(query));
                var result = m1.Concat(m2).Distinct();
                sender.ItemsSource = result.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string albumArtist;
            if (args.ChosenSuggestion != null)
            {
                albumArtist = ((AlbumArtistItem)args.ChosenSuggestion).DisplayAlbumArtist;
            }
            else
            {
                var list = albumArtists.Where(s => s.DisplayAlbumArtist.ToLower().StartsWith(args.QueryText.ToLower())).OrderBy(s => s.DisplayAlbumArtist).ToList();
                if (list.Count == 0) return;
                albumArtist = list.FirstOrDefault().DisplayAlbumArtist;
            }
            int index = 0;
            bool find = false;
            foreach (var group in groupedAlbumArtists)
            {
                foreach (AlbumArtistItem item in group)
                {
                    if (item.DisplayAlbumArtist == albumArtist)
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
            var item = args.SelectedItem as AlbumArtistItem;
            sender.Text = item.DisplayAlbumArtist;
        }

        public int GetIndexFromGroup(object item)
        {
            return GroupedAlbumArtists.FirstOrDefault(g => g.Contains(item)).IndexOf(item);
        }
    }
}
