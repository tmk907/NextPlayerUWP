using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
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
    public class SongsViewModel : MusicViewModelBase
    {
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
            int index = 0;
            foreach(var s in songs)
            {
                if (s.SongId == ((SongItem)e.ClickedItem).SongId) break;
                index++;
            }
            await NowPlayingPlaylistManager.Current.NewPlaylist(songs);
            ApplicationSettingsHelper.SaveSongIndex(index);
            PlaybackManager.Current.PlayNew();
            //NavigationService.Navigate(App.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }

        public void SortItems(object sender, SelectionChangedEventArgs e)
        {
            //string option = e.AddedItems.FirstOrDefault() as string;
            Sort(s => s.Album, t => (t.Album == "") ? "" : t.Album[0].ToString().ToLower());
            //var a1 = e.AddedItems.FirstOrDefault();
            //SortEnums sortby = SortEnums.Title;
            //if (a1.ToString() == "Album") sortby = SortEnums.Album;
            //if (a1.ToString() == "Artist") sortby = SortEnums.Artist;

            //switch (sortby)
            //{
            //    case SortEnums.Album:
            //        Sort(s => s.Album, t => (t.Album=="")?"":t.Album[0].ToString().ToLower());
            //        break;
            //    case SortEnums.Artist:
            //        Sort(s => s.Artist, t => (t.Artist == "") ? "" : t.Artist[0].ToString().ToLower());
            //        break;
            //    case SortEnums.Title:
            //        Sort(s => s.Title, t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower());
            //        break;
            //    default:
            //        Sort(s => s.Title, t => (t.Title == "") ? "" : t.Title[0].ToString().ToLower());
            //        break;
            //}
        }

        private void Sort(Func<SongItem, object> orderSelector, Func<SongItem,string> groupSelector)
        {
            var query = songs.OrderBy(orderSelector).ThenBy(s => s.Title).GroupBy(groupSelector).OrderBy(g => g.Key).Select(group => new { GroupName = group.Key.ToUpper(), Items = group });
            //var query = from item in songs
            //             orderby orderSelector
            //             group item by groupSelector into g
            //             orderby g.Key
            //             select new { GroupName = g.Key, Items = g };
            int i = 0;
            GroupedSongs.Clear();
            foreach (var g in query)
            {
                i = 0;
                string s = "";
                GroupList group = new GroupList();
                group.Key = g.GroupName;
                foreach (var item in g.Items)
                {
                    if (group.Count != 0 && item.Album != s) i++;
                    item.Index = i;
                    s = item.Album;
                    group.Add(item);
                }
                GroupedSongs.Add(group);
            }
        }
    }
}
