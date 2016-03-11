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
    public class AlbumViewModel : MusicViewModelBase
    {
        private string albumParam;

        public AlbumViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                songs = new ObservableCollection<SongItem>();
                songs.Add(new SongItem());
                songs.Add(new SongItem() { TrackNumber = 882 });
                songs.Add(new SongItem());
                songs.Add(new SongItem());
            }
        }

        private AlbumItem album = new AlbumItem();
        public AlbumItem Album
        {
            get { return album; }
            set { Set(ref album, value); }
        }

        private ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Songs
        {
            get { return songs; }
            set { Set(ref songs, value); }
        }

        protected override async Task LoadData()
        {
            if (songs.Count == 0)
            {
                songs = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(albumParam);
                Songs = new ObservableCollection<SongItem>(songs.OrderBy(s => s.TrackNumber));
                Album = await DatabaseManager.Current.GetAlbumItemAsync(albumParam);
                if (!album.IsImageSet)
                {
                    string path = await ImagesManager.GetAlbumCoverPath(album);
                    Album.ImagePath = path;
                    Album.ImageUri = new Uri(path);
                    await DatabaseManager.Current.UpdateAlbumItem(album);
                }
            }
        }

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter != null)
            {
                try
                {
                    albumParam = (MusicItem.ParseParameter(parameter as string))[1];
                }
                catch (Exception ex) { }
            }
        }

        public override Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back || args.NavigationMode == NavigationMode.New)
            {
                songs = new ObservableCollection<SongItem>();
                album = new AlbumItem();
            }
            if (args.NavigationMode == NavigationMode.Refresh)
            {
                System.Diagnostics.Debug.WriteLine("navvigation mode refresh");
            }
            return base.OnNavigatingFromAsync(args);
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            int index = 0;
            foreach (var s in songs)
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
