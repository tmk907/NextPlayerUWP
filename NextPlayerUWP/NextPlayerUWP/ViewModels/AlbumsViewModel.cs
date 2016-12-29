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
    public class AlbumsViewModel : MusicViewModelBase
    {
        private bool isRunning = false;
        SortingHelperForAlbumItems sortingHelper;

        public AlbumsViewModel()
        {
            System.Diagnostics.Debug.WriteLine("AlbumsViewModel constructor");
            sortingHelper = new SortingHelperForAlbumItems("Albums");
            ComboBoxItemValues = sortingHelper.ComboBoxItemValues;
            SelectedComboBoxItem = sortingHelper.SelectedSortOption;
            App.SongUpdated += App_SongUpdated;
            MediaImport.MediaImported += MediaImport_MediaImported;
            AlbumArtFinder.AlbumArtUpdatedEvent += AlbumArtFinder_AlbumArtUpdatedEvent;
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                albums = new ObservableCollection<AlbumItem>();
                albums.Add(new AlbumItem());
                albums.Add(new AlbumItem() { Album = "Very long album name, very long"});
                albums.Add(new AlbumItem() { AlbumArtist = "Very long albumartist name, very long"});
                albums.Add(new AlbumItem() { Album = "Very long album name, very long", AlbumArtist = "Very long albumartist name, very long" });
                albums.Add(new AlbumItem());
                albums.Add(new AlbumItem());
                albums.Add(new AlbumItem());
                albums.Add(new AlbumItem());
                albums.Add(new AlbumItem());
            }
        }



        private void AlbumArtFinder_AlbumArtUpdatedEvent(int albumId, string albumArtPath)
        {
            Template10.Common.IDispatcherWrapper d = Dispatcher;
            if (d == null)
            {
                d = Template10.Common.WindowWrapper.Current().Dispatcher;
            }
            if (d == null)
            {
                NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage("AlbumsViewModel Dispatcher null", NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.Warning);
                return;
            }
            d.Dispatch(() => 
            {
                var alb = Albums.FirstOrDefault(a => a.AlbumId == albumId);
                if (alb != null)
                {
                    alb.ImagePath = albumArtPath;
                    alb.ImageUri = new Uri(albumArtPath);
                    alb.IsImageSet = true;
                    bool stop = false;
                    foreach (var group in GroupedAlbums)
                    {
                        foreach (AlbumItem album in group)
                        {
                            if (album.AlbumId == albumId)
                            {
                                album.ImagePath = albumArtPath;
                                album.ImageUri = new Uri(albumArtPath);
                                album.IsImageSet = true;
                                album.Album = album.Album;
                                stop = true;
                                break;
                            }
                        }
                        if (stop) break;
                    }
                }
            });
        }

        private ObservableCollection<AlbumItem> albums = new ObservableCollection<AlbumItem>();
        public ObservableCollection<AlbumItem> Albums
        {
            get { return albums; }
            set { Set(ref albums, value); }
        }

        private ObservableCollection<GroupList> groupedAlbums = new ObservableCollection<GroupList>();
        public ObservableCollection<GroupList> GroupedAlbums
        {
            get { return groupedAlbums; }
            set { Set(ref groupedAlbums, value); }
        }

        protected override async Task LoadData()
        {
            if (albums.Count == 0)
            {
                Albums = await DatabaseManager.Current.GetAlbumItemsAsync();
            }
            if (groupedAlbums.Count == 0)
            {
                var query = from item in albums
                            orderby item.Album.ToLower()
                            group item by item.Album[0].ToString().ToLower() into g
                            orderby g.Key
                            select new { GroupName = g.Key.ToUpper(), Items = g };
                ObservableCollection<GroupList> gr = new ObservableCollection<GroupList>();
                foreach (var g in query)
                {
                    GroupList group = new GroupList();
                    group.Key = g.GroupName;
                    foreach (var item in g.Items)
                    {
                        group.Add(item);
                    }
                    gr.Add(group);
                }
                GroupedAlbums = gr;
                SortMusicItems();
            }
        }

        public override void FreeResources()
        {
            groupedAlbums = null;
            albums = null;
            groupedAlbums = new ObservableCollection<GroupList>();
            albums = new ObservableCollection<AlbumItem>();
        }

        private async void MediaImport_MediaImported(string s)
        {
            await Dispatcher.DispatchAsync(() => ReloadData());
        }

        private async void App_SongUpdated(int id)
        {
            await Dispatcher.DispatchAsync(() => ReloadData());
        }

        public void ItemClicked(object sender, ItemClickEventArgs e)
        {
            NavigationService.Navigate(App.Pages.Album, ((AlbumItem)e.ClickedItem).AlbumId);
        }

        private async Task ReloadData()
        {
            Albums = await DatabaseManager.Current.GetAlbumItemsAsync();
            SortMusicItems();
        }

        protected override void SortMusicItems()
        {
            sortingHelper.SelectedSortOption = selectedComboBoxItem;

            var orderSelector = sortingHelper.GetOrderBySelector();
            var groupSelector = sortingHelper.GetGroupBySelector();
            string propertyName = sortingHelper.GetPropertyName();
            string format = sortingHelper.GetFormat();

            var query = albums.OrderBy(orderSelector).
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
            GroupedAlbums.Clear();
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
                GroupedAlbums.Add(group);
            }
        }

        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = sender.Text.ToLower();
                var m1 = albums.Where(s => s.Album.ToLower().StartsWith(query));
                var m2 = albums.Where(s => s.Album.ToLower().Contains(query));
                var m3 = albums.Where(a => a.AlbumArtist.ToLower().StartsWith(query));
                var m4 = albums.Where(a => a.AlbumArtist.ToLower().Contains(query));
                var result = m1.Concat(m2).Concat(m3).Concat(m4).Distinct();
                sender.ItemsSource = result.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string album;
            if (args.ChosenSuggestion != null)
            {
                album = ((AlbumItem)args.ChosenSuggestion).Album;
            }
            else
            {
                var list = albums.Where(s => s.Album.ToLower().StartsWith(args.QueryText.ToLower())).OrderBy(s => s.Album).ToList();
                if (list.Count == 0) return;
                album = list.FirstOrDefault().Album;
            }
            int index = 0;
            bool find = false;
            foreach (var group in groupedAlbums)
            {
                foreach (AlbumItem item in group)
                {
                    if (item.Album == album)
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
            var item = args.SelectedItem as AlbumItem;
            sender.Text = item.Album;
        }
    }
}
