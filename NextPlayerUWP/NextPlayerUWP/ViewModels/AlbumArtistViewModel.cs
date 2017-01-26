using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Services.NavigationService;
using Windows.UI.Xaml;

namespace NextPlayerUWP.ViewModels
{
    public class AlbumArtistViewModel : MusicViewModelBase
    {
        private int albumArtistId;

        private AlbumArtistItem albumArtist;
        public AlbumArtistItem AlbumArtist
        {
            get { return albumArtist; }
            set { Set(ref albumArtist, value); }
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
                    albumArtistId = Int32.Parse(parameter.ToString());
                }
                catch (Exception ex) { }
            }
        }

        protected override async Task LoadData()
        {
            if (albums.Count == 0)
            {
                AlbumArtist = await DatabaseManager.Current.GetAlbumArtistItemAsync(albumArtistId);
                var albums = await DatabaseManager.Current.GetAlbumItemsFromAlbumArtistAsync(albumArtist.AlbumArtist);
                int i = 0;
                foreach (var album in albums)
                {
                    var songs = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(album.AlbumParam, album.AlbumArtist);
                    GroupList group = new GroupList();
                    group.Key = album.AlbumParam;
                    var header = new ArtistItemHeader();
                    if (album.AlbumParam == "")
                    {
                        var helper = new TranslationHelper();
                        header.Album = helper.GetTranslation(TranslationHelper.UnknownAlbum);
                    }
                    else
                    {
                        header.Album = album.Album;
                    }
                    if (album.AlbumArtist == "")
                    {
                        var helper = new TranslationHelper();
                        header.AlbumArtist = helper.GetTranslation(TranslationHelper.UnknownAlbumArtist);
                    }
                    else
                    {
                        header.AlbumArtist = album.AlbumArtist;
                    }
                    header.Year = album.Year;
                    header.ImageUri = album.ImageUri;
                    group.Header = header;

                    foreach (var item in songs)
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
                albumArtist = new AlbumArtistItem();
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
                foreach (SongItem song in album)
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
            //NavigationService.Navigate(App.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }

        public async void ShuffleAllSongs()
        {
            if (albums.Count == 0) return;
            List<SongItem> list = new List<SongItem>();
            foreach (var group in albums)
            {
                foreach(SongItem song in group)
                {
                    list.Add(song);
                }
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
            foreach (SongItem song in group)
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
            NavigationService.Navigate(App.Pages.AddToPlaylist, item.GetParameter());
        }
    }
}
