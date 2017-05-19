using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.ViewModels
{
    public class SongsViewModel : MusicViewModelBase, IGroupedItemsList
    {
        public SongsViewModel()
        {
            sortingHelper = new SortingHelperForSongItems("Songs");
            ComboBoxItemValues = sortingHelper.ComboBoxItemValues;
            SelectedComboBoxItem = sortingHelper.SelectedSortOption;
            App.SongUpdated += App_SongUpdated;
            MediaImport.MediaImported += MediaImport_MediaImported;
        }

        SortingHelperForSongItems sortingHelper;

        private async void MediaImport_MediaImported(string s)
        {
            Template10.Common.IDispatcherWrapper d = Dispatcher;
            if (d == null)
            {
                d = Template10.Common.WindowWrapper.Current().Dispatcher;
            }
            if (d == null)
            {
                NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage("SongsViewModel Dispatcher null", NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);
                TelemetryAdapter.TrackEvent("Dispatcher null");
                return;
            }
            await d.DispatchAsync(() => ReloadData());
        }

        private async void App_SongUpdated(int id)
        {
            Template10.Common.IDispatcherWrapper d = Dispatcher;
            if (d == null)
            {
                d = Template10.Common.WindowWrapper.Current().Dispatcher;
            }
            if (d == null)
            {
                NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage("SongsViewModel Dispatcher null", NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);
                TelemetryAdapter.TrackEvent("Dispatcher null");
                return;
            }
            await d.DispatchAsync(() => ReloadData());
        }

        private ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Songs
        {
            get
            {
                //if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                //{
                //    for(int i = 0; i < 10; i++)
                //    {
                //        Songs.Add(new SongItem());
                //    }
                //}
                return songs; }
            set { Set(ref songs, value); }
        }

        private ObservableCollection<GroupList> groupedSongs = new ObservableCollection<GroupList>();
        public ObservableCollection<GroupList> GroupedSongs
        {
            get
            {
                //if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                //{
                //    string[] t = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "K" };
                //    for (int i = 0; i < 10; i++)
                //    {
                //        GroupList group = new GroupList();
                //        group.Key = t[i];
                //        groupedSongs.Add(group);
                //    }
                //}
                return groupedSongs;
            }
            set { Set(ref groupedSongs, value); }
        }

        protected override async Task LoadData()
        {               
            if (songs.Count == 0)
            {
                Songs = await DatabaseManager.Current.GetLocalSongItemsAsync();
            }
            if (groupedSongs.Count == 0)
            {
                var query = from item in songs
                            orderby item.Title.ToLower()
                            group item by item.Title.FirstOrDefault().ToString().ToLower() into g
                            orderby g.Key
                            select new { GroupName = g.Key.ToUpper(), Items = g };
                int i = 0;
                foreach (var g in query)
                {
                    i = 0;
                    GroupList group = new GroupList();
                    group.Key = g.GroupName;
                    foreach (var item in g.Items)
                    {
                        item.Index = i;
                        i++;
                        group.Add(item);
                    }
                    GroupedSongs.Add(group);
                }
                SortMusicItems();
                //var characterGroupings = new Windows.Globalization.Collation.CharacterGroupings();
                //foreach (var c in characterGroupings)
                //{
                //    GroupedSongs.Add(new GroupList(c.Label));
                //}
                //foreach (var item in songs)
                //{
                //    string a = characterGroupings.Lookup(item.Title);
                //    GroupedSongs.FirstOrDefault(e => e.Key.Equals(a)).Add(item);
                //}
            }
        }

        public override void FreeResources()
        {
            groupedSongs = null;
            songs = null;
            groupedSongs = new ObservableCollection<GroupList>();
            songs = new ObservableCollection<SongItem>();
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            await SongClicked(((SongItem)e.ClickedItem).SongId);
        }

        private async Task SongClicked(int songid)
        {
            int index = 0;
            int i = 0;
            foreach(var group in groupedSongs)
            {
                foreach(SongItem song in group)
                {
                    if (song.SongId == songid) index = i;
                    i++;
                }
            }
            await NowPlayingPlaylistManager.Current.NewPlaylist(groupedSongs);
            await PlaybackService.Instance.PlayNewList(index);
            //NavigationService.Navigate(App.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }

        public async void ShuffleAllSongs()
        {
            if (songs.Count == 0) return;
            List<SongItem> list = new List<SongItem>();
            foreach(var s in songs)
            {
                list.Add(s);
            }
            Random rnd = new Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                SongItem value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            await NowPlayingPlaylistManager.Current.NewPlaylist(list);
            await PlaybackService.Instance.PlayNewList(0);
        }

        private async Task ReloadData()
        {
            Songs = await DatabaseManager.Current.GetLocalSongItemsAsync();
            SortMusicItems();
        }

        protected override void SortMusicItems()
        {
            sortingHelper.SelectedSortOption = selectedComboBoxItem;
            var orderSelector = sortingHelper.GetOrderBySelector();
            var groupSelector = sortingHelper.GetGroupBySelector();
            string propertyName = sortingHelper.GetPropertyName();
            string format = sortingHelper.GetFormat();

            var query = songs.OrderBy(orderSelector).ThenBy(a => a.Title).
                GroupBy(groupSelector).
                Select(group => new {
                    GroupName = (format != "duration") ? group.Key.ToString().ToUpper()
                    : (((TimeSpan)group.Key).Hours == 0) ? ((TimeSpan)group.Key).ToString(@"m\:ss")
                    : (((TimeSpan)group.Key).Days == 0) ? ((TimeSpan)group.Key).ToString(@"h\:mm\:ss")
                    : ((TimeSpan)group.Key).ToString(@"d\.hh\:mm\:ss"),
                    Items = group
                });

            int i = 0;
            string s;

            GroupedSongs.Clear();
            foreach (var g in query)
            {
                i = 0;
                s = "";
                GroupList group = new GroupList();
                group.Key = g.GroupName;
                group.Header = format;
                //(t.Duration.TotalMinutes < 1) ? "0" + t.Duration.ToString(@"\:ss") :
                //        (t.Duration.Hours == 0) ? t.Duration.ToString(@"m\:ss") :
                //        (t.Duration.Days == 0) ? t.Duration.ToString(@"h\:mm\:ss") : t.Duration.ToString(@"d\.hh\:mm\:ss"),
                foreach (var item in g.Items)
                {
                    string prop = item.GetType().GetProperty(propertyName).GetValue(item, null).ToString();
                    if (group.Count != 0 && prop != s) i++;
                    item.Index = i;
                    s = prop;
                    group.Add(item);
                }
                GroupedSongs.Add(group);
            }
        }

        public async void DeleteSong(object sender, RoutedEventArgs e)
        {
            var item = (SongItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            MessageDialogHelper msg = new MessageDialogHelper();
            bool delete = await msg.ShowDeleteSongConfirmationDialog();
            if (delete)
            {
                Songs.Remove(item);
                await DatabaseManager.Current.DeleteSong(item.SongId);
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(item.Path);
                    await file.DeleteAsync();
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = sender.Text.ToLower();
                var matchingSongs = songs.Where(s => s.Title.ToLower().StartsWith(query));
                var m2 = songs.Where(s => s.Title.ToLower().Contains(query));
                var m3 = songs.Where(s => (s.Album.ToLower().Contains(query) || s.Artist.ToLower().Contains(query)));
                var m4 = matchingSongs.Concat(m2).Concat(m3).Distinct();
                sender.ItemsSource = m4.ToList();
            }
        }

        public void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            int id;
            if (args.ChosenSuggestion != null)
            {
                id = ((SongItem)args.ChosenSuggestion).SongId;               
            }
            else
            {
                var list = songs.Where(s => s.Title.ToLower().StartsWith(args.QueryText.ToLower())).OrderBy(s => s.Title).ToList();
                if (list.Count == 0) return;
                id = list.FirstOrDefault().SongId;
            }
            int index = 0;
            bool find = false;
            foreach (var group in groupedSongs)
            {
                foreach (SongItem item in group)
                {
                    if (item.SongId == id)
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
            var song = args.SelectedItem as SongItem;
            sender.Text = song.Title;
        }

        public int GetIndexFromGroup(object item)
        {
            return GroupedSongs.FirstOrDefault(g => g.Contains(item)).IndexOf(item);
        }
    }
}
