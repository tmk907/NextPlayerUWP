using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class SongsViewModel : MusicViewModelBase
    {
        public SongsViewModel()
        {
            SortNames si = new SortNames(MusicItemTypes.song);
            ComboBoxItemValues = si.GetSortNames();
            SelectedComboBoxItem = ComboBoxItemValues.FirstOrDefault();
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

        private ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Songs
        {
            get
            {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    for(int i = 0; i < 10; i++)
                    {
                        Songs.Add(new SongItem());
                    }
                }
                return songs; }
            set { Set(ref songs, value); }
        }

        private ObservableCollection<GroupList> groupedSongs = new ObservableCollection<GroupList>();
        public ObservableCollection<GroupList> GroupedSongs
        {
            get
            {
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    string[] t = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "K" };
                    for (int i = 0; i < 10; i++)
                    {
                        GroupList group = new GroupList();
                        group.Key = t[i];
                        groupedSongs.Add(group);
                    }
                }
                return groupedSongs;
            }
            set { Set(ref groupedSongs, value); }
        }

        protected override async Task LoadData()
        {               
            if (songs.Count == 0)
            {
                Songs = await DatabaseManager.Current.GetSongItemsAsync();
            }
            if (groupedSongs.Count == 0)
            {
                var query = from item in songs
                            orderby item.Title.ToLower()
                            group item by item.Title[0].ToString().ToLower() into g
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
            ApplicationSettingsHelper.SaveSongIndex(index);
            App.PlaybackManager.PlayNew();
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
            ApplicationSettingsHelper.SaveSongIndex(0);
            App.PlaybackManager.PlayNew();
        }

        private async Task ReloadData()
        {
            Songs = await DatabaseManager.Current.GetSongItemsAsync();
            SortMusicItems();
        }

        protected override void SortMusicItems()
        {
            string option = selectedComboBoxItem.Option;
            switch (option)
            {
                case SortNames.Title:
                    Sort(s => s.Title, t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower(), "SongId");
                    break;
                case SortNames.Album:
                    Sort(s => s.Album, t => (t.Album == "") ? "" : t.Album[0].ToString().ToLower(), "Album");
                    break;
                case SortNames.Artist:
                    Sort(s => s.Artist, t => (t.Artist == "") ? "" : t.Artist[0].ToString().ToLower(), "Artist");
                    break;
                case SortNames.AlbumArtist:
                    Sort(s => s.AlbumArtist, t => (t.AlbumArtist == "") ? "" : t.AlbumArtist[0].ToString().ToLower(), "AlbumArtist");
                    break;
                case SortNames.Year:
                    Sort(s => s.Year, t => t.Year, "SongId", "year");
                    break;
                case SortNames.Duration:
                    Sort(s => s.Duration.TotalSeconds, t => new TimeSpan(t.Duration.Hours, t.Duration.Minutes, t.Duration.Seconds), "SongId", "duration");
                    break;
                case SortNames.Rating:
                    Sort(s => s.Rating, t => t.Rating, "SongId");
                    break;
                case SortNames.Composer:
                    Sort(s => s.Composer, t => (t.Composer == "") ? "" : t.Composer[0].ToString().ToLower(), "Composer");
                    break;
                case SortNames.LastAdded:
                    Sort(s => s.DateAdded.Ticks, t => String.Format("{0:d}", t.DateAdded), "SongId", "date");
                    break;
                case SortNames.LastPlayed:
                    Sort(s => s.LastPlayed.Ticks, t => String.Format("{0:d}", t.LastPlayed), "SongId", "date");
                    break;
                case SortNames.PlayCount:
                    Sort(s => s.PlayCount, t => t.PlayCount, "SongId");
                    break;
                case SortNames.TrackNumber:
                    Sort(s => s.TrackNumber, s => s.TrackNumber, "SongId");
                    break;
                default:
                    Sort(s => s.Title, t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower(), "SongId");
                    break;
            }
        }

        private void Sort(Func<SongItem, object> orderSelector, Func<SongItem,object> groupSelector, string propertyName, string format = "no")
        {
            var query = songs.OrderBy(orderSelector).ThenBy(a => a.Title).
                GroupBy(groupSelector).
                OrderBy(g => g.Key).
                Select(group => new { GroupName = (format != "duration") ? group.Key.ToString().ToUpper() 
                : (((TimeSpan)group.Key).Hours == 0) ? ((TimeSpan)group.Key).ToString(@"m\:ss") 
                : (((TimeSpan)group.Key).Days == 0) ? ((TimeSpan)group.Key).ToString(@"h\:mm\:ss") 
                : ((TimeSpan)group.Key).ToString(@"d\.hh\:mm\:ss"), Items = group });
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
    }
}
