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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Services.NavigationService;

namespace NextPlayerUWP.ViewModels
{
    public class ArtistItemHeader
    {
        public string Album { get; set; }
        public int Year { get; set; }
        public Uri ImageUri { get; set; }
    }

    public class ArtistViewModel : MusicViewModelBase
    {
        private string artistParam;

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
                    var s = parameter.ToString().Split(new string[] { MusicItem.separator }, StringSplitOptions.None);
                    artistParam = s[1];
                }
                catch (Exception ex) { }
            }
        }

        protected override async Task LoadData()
        {
            if (songs.Count == 0)
            {
                Artist = await DatabaseManager.Current.GetArtistItemAsync(artistParam);
                Songs = await DatabaseManager.Current.GetSongItemsFromArtistAsync(artistParam);
                var query = songs.OrderBy(s => s.Album).ThenBy(t => t.TrackNumber).
                    GroupBy(u => u.Album).
                    OrderBy(g => g.Key).
                    Select(group => new { GroupName = group.Key, Items = group });
                int i = 0;
                foreach (var g in query)
                {
                    i = 0;
                    GroupList group = new GroupList();
                    group.Key = g.GroupName;

                    var album = await DatabaseManager.Current.GetAlbumItemAsync(g.GroupName);

                    var header = new ArtistItemHeader();
                    if (g.GroupName == "")
                    {
                        var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                        header.Album = loader.GetString("UnknownAlbum");
                    }
                    else
                    {
                        header.Album = g.GroupName;
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

        public override Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back || args.NavigationMode == NavigationMode.New)
            {
                songs = new ObservableCollection<SongItem>();
                artist = new ArtistItem();
                albums = new ObservableCollection<GroupList>();
            }
            return base.OnNavigatingFromAsync(args);
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
            ApplicationSettingsHelper.SaveSongIndex(index);
            PlaybackManager.Current.PlayNew();
            //NavigationService.Navigate(App.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }

    }
}
