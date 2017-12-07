using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Services.NavigationService;
using Windows.UI.Xaml;
using NextPlayerUWP.Models;
using NextPlayerUWP.Playback;

namespace NextPlayerUWP.ViewModels
{
    public class ArtistViewModel : MusicViewModelBase, IGroupedItemsList
    {
        private int artistId;

        public ArtistViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Artist = new ArtistItem();
                Albums = new ObservableCollection<GroupList>();
                GroupList g = new GroupList();
                g.Key = "Nowy album";
                g.Add(new SongItem());
                g.Add(new SongItem());
                g.Add(new SongItem());
                Albums.Add(g);
            }
        }

        private ArtistItem artist = new ArtistItem();
        public ArtistItem Artist
        {
            get { return artist; }
            set { Set(ref artist, value); }
        }

        private ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Songs
        {
            get { return songs; }
            set { Set(ref songs, value); }
        }

        private ObservableCollection<GroupList> albums = new ObservableCollection<GroupList>();
        public ObservableCollection<GroupList> Albums
        {
            get { return albums; }
            set { Set(ref albums, value); }
        }

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter != null)
            {
                try
                {
                    artistId = Int32.Parse(parameter.ToString());
                }
                catch (Exception ex) { }
            }
        }

        protected override async Task LoadData()
        {
            if (songs.Count == 0)
            {
                Artist = await DatabaseManager.Current.GetArtistItemAsync(artistId);
                Songs = await DatabaseManager.Current.GetSongItemsFromArtistAsync(artist.ArtistParam);
                var query = songs.
                    GroupBy(u => new { u.Album, u.AlbumArtist }).
                    OrderBy(g => g.Key.Album).
                    Select(group => new { GroupName = group.Key, Items = group });
                int i = 0;
                foreach (var g in query)
                {
                    i = 0;
                    GroupList group = new GroupList();
                    group.Key = g.GroupName;

                    var album = await DatabaseManager.Current.GetAlbumItemAsync(g.GroupName.Album, g.GroupName.AlbumArtist);

                    var header = new ArtistItemHeader();
                    header.AlbumId = album.AlbumId;
                    if (g.GroupName.Album == "")
                    {
                        var helper = new TranslationHelper();
                        header.Album = helper.GetTranslation(TranslationHelper.UnknownAlbum);
                    }
                    else
                    {
                        header.Album = g.GroupName.Album;
                    }
                    if (g.GroupName.AlbumArtist == "")
                    {
                        var helper = new TranslationHelper();
                        header.AlbumArtist = helper.GetTranslation(TranslationHelper.UnknownAlbumArtist);
                    }
                    else
                    {
                        header.AlbumArtist = g.GroupName.AlbumArtist;
                    }
                    header.Year = album.Year;
                    header.ImageUri = album.ImageUri;

                    group.Header = header;

                    foreach (var item in g.Items)
                    {
                        item.Index = i;
                        i++;
                        group.Add(item);
                    }
                    Albums.Add(group);
                }
            }
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back || args.NavigationMode == NavigationMode.New)
            {
                songs = new ObservableCollection<SongItem>();
                artist = new ArtistItem();
                albums = new ObservableCollection<GroupList>();
            }
            await base.OnNavigatingFromAsync(args);
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            int index = 0;
            bool found = false;
            foreach (var album in albums)
            {
                foreach(SongItem song in album)
                {
                    if (song.SongId == ((SongItem)e.ClickedItem).SongId)
                    {
                        found = true;
                        break;
                    }
                    index++;
                }
                if (found) break;
            }
            await NowPlayingPlaylistManager.Current.NewPlaylist(albums);
            await PlaybackService.Instance.PlayNewList(index);
            //NavigationService.Navigate(AppPages.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }

        public async void ShuffleAllSongs()
        {
            if (songs.Count == 0) return;
            List<SongItem> list = new List<SongItem>();
            foreach (var s in songs)
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

        public async void PlayNowAlbum(object sender, RoutedEventArgs e)
        {
            var group = (GroupList)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            List<SongItem> list = new List<SongItem>();
            foreach(SongItem song in group)
            {
                list.Add(song);
            }
            await NowPlayingPlaylistManager.Current.NewPlaylist(list);
            await PlaybackService.Instance.PlayNewList(0);
        }

        public async void PlayNextAlbum(object sender, RoutedEventArgs e)
        {
            var group = (GroupList)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            List<SongItem> list = new List<SongItem>();
            foreach (SongItem song in group)
            {
                list.Add(song);
            }
            await NowPlayingPlaylistManager.Current.AddNext(list);
        }

        public async void AddToNowPlayingAlbum(object sender, RoutedEventArgs e)
        {
            var group = (GroupList)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            List<SongItem> list = new List<SongItem>();
            foreach (SongItem song in group)
            {
                list.Add(song);
            }
            await NowPlayingPlaylistManager.Current.Add(list);
        }

        public void AddToPlaylistAlbum(object sender, RoutedEventArgs e)
        {
            var group = (GroupList)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            List<SongItem> list = new List<SongItem>();
            foreach (SongItem song in group)
            {
                list.Add(song);
            }
            App.AddToCache(list);
            var item = new ListOfMusicItems();
            NavigationService.Navigate(AppPages.Pages.AddToPlaylist, item.GetParameter());
        }

        public int GetIndexFromGroup(object item)
        {
            return Albums.FirstOrDefault(g => g.Contains(item)).IndexOf(item);
        }
    }
}
