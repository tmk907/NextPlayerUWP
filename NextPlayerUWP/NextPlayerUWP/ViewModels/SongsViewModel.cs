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
            if (Songs.Count == 0)
            {
                Songs = await DatabaseManager.Current.GetSongItemsAsync();
            }
            if (groupedSongs.Count == 0)
            {
                var query = from item in songs
                            group item by item.Title[0] into g
                            orderby g.Key
                            select new { GroupName = g.Key, Items = g };
                foreach(var g in query)
                {
                    GroupList group = new GroupList();
                    group.Key = g.GroupName;
                    foreach (var item in g.Items)
                    {
                        group.Add(item);
                    }
                    GroupedSongs.Add(group);
                }
            }
        }

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!isBack)
            {
                Songs = new ObservableCollection<SongItem>();
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
    }
}
