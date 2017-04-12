using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Services.NavigationService;
using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.CloudStorage;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Playlists;

namespace NextPlayerUWP.ViewModels
{
    public class AddToPlaylistViewModel : Template10.Mvvm.ViewModelBase
    {
        private ObservableCollection<PlaylistItem> playlists;
        public ObservableCollection<PlaylistItem> Playlists
        {
            get { return playlists; }
            set { Set(ref playlists, value); }
        }

        private string name = "";
        public string Name
        {
            get { return name; }
            set { Set(ref name, value); }
        }

        private bool loading = false;
        public bool Loading
        {
            get { return loading; }
            set { Set(ref loading, value); }
        }

        private MusicItemTypes type;
        private string[] values;

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            App.OnNavigatedToNewView(true);
            Playlists = await DatabaseManager.Current.GetPlainPlaylistsAsync();
            type = MusicItem.ParseType((string)parameter);
            values = MusicItem.SplitParameter((string)parameter);
            if (mode == NavigationMode.New || mode == NavigationMode.Forward)
            {
                TelemetryAdapter.TrackPageView(this.GetType().ToString());
            }
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back || args.NavigationMode == NavigationMode.New)
            {
                playlists = new ObservableCollection<PlaylistItem>();
                name = "";
            }
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            Loading = true;
            PlaylistItem p = (PlaylistItem)e.ClickedItem;
            string value = values[1];
            string userId;
            string folderId;
            IEnumerable<SongItem> songs = new List<SongItem>();
            switch (type)
            {
                case MusicItemTypes.album:
                    string albArt = values[2];
                    songs = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(value, albArt);
                    await DatabaseManager.Current.AddToPlaylist(p.Id, songs);
                    break;
                case MusicItemTypes.albumartist:
                    songs = await DatabaseManager.Current.GetSongItemsFromAlbumArtistAsync(value);
                    await DatabaseManager.Current.AddToPlaylist(p.Id, songs);
                    break;
                case MusicItemTypes.artist:
                    songs = await DatabaseManager.Current.GetSongItemsFromArtistAsync(value);
                    await DatabaseManager.Current.AddToPlaylist(p.Id, songs);
                    break;
                case MusicItemTypes.folder:
                    bool subFolders = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.IncludeSubFolders);
                    if (subFolders)
                    {
                        await DatabaseManager.Current.AddToPlaylist(p.Id, a => a.DirectoryName.StartsWith(value), s => s.Title);
                    }
                    else
                    {
                        await DatabaseManager.Current.AddToPlaylist(p.Id, a => a.DirectoryName.Equals(value), s => s.Title);
                    }
                    break;
                case MusicItemTypes.genre:
                    songs = await DatabaseManager.Current.GetSongItemsFromGenreAsync(value);
                    await DatabaseManager.Current.AddToPlaylist(p.Id, songs);
                    break;
                case MusicItemTypes.song:
                    await DatabaseManager.Current.AddToPlaylist(p.Id, a => a.SongId.Equals(value), s => s.Title);
                    break;
                case MusicItemTypes.nowplayinglist:
                    await DatabaseManager.Current.AddNowPlayingToPlaylist(p.Id);
                    break;
                case MusicItemTypes.radio:

                    break;
                case MusicItemTypes.dropboxfolder:
                    userId = values[1];
                    folderId = (values[2] != "") ? values[2] : "";
                    CloudStorageServiceFactory cssfd = new CloudStorageServiceFactory();
                    var serviced = cssfd.GetService(CloudStorageType.Dropbox, userId);
                    await serviced.GetSongItems(folderId);
                    await DatabaseManager.Current.AddToPlaylist(p.Id, a => a.DirectoryName.Equals(folderId) && a.CloudUserId.Equals(userId) && a.MusicSourceType.Equals((int)MusicSource.Dropbox), s => s.Title);
                    break;
                case MusicItemTypes.onedrivefolder:
                    userId = values[1];
                    folderId = (values[2] != "") ? values[2] : null;
                    CloudStorageServiceFactory cssf = new CloudStorageServiceFactory();
                    var service = cssf.GetService(CloudStorageType.OneDrive, userId);
                    if (folderId == null)
                    {
                        folderId = await service.GetRootFolderId();
                    }
                    await service.GetSongItems(folderId);
                    await DatabaseManager.Current.AddToPlaylist(p.Id, a => a.DirectoryName.Equals(folderId) && a.CloudUserId.Equals(userId) && a.MusicSourceType.Equals((int)MusicSource.OneDrive), s => s.Title);

                    break;
                case MusicItemTypes.pcloudfolder:
                    userId = values[1];
                    folderId = (values[2] != "") ? values[2] : "0";
                    CloudStorageServiceFactory cssfp = new CloudStorageServiceFactory();
                    var servicep = cssfp.GetService(CloudStorageType.pCloud, userId);
                    await servicep.GetSongItems(folderId);
                    await DatabaseManager.Current.AddToPlaylist(p.Id, a => a.DirectoryName.Equals(folderId) && a.CloudUserId.Equals(userId) && a.MusicSourceType.Equals((int)MusicSource.PCloud), s => s.Title);
                    break;
                case MusicItemTypes.listofitems:
                    var temp = App.GetFromCache();
                    SongItemsFactory factory = new SongItemsFactory();
                    songs = await factory.GetSongItems(temp);
                    await DatabaseManager.Current.AddToPlaylist(p.Id, songs); 
                    App.ClearCache();
                    break;
                default:
                    break;
            }
            PlaylistHelper ph = new PlaylistHelper();
            await ph.UpdatePlaylistFile(p);
            Loading = false;
            NavigationService.GoBack();
        }

        public async void Save()
        {
            int id = DatabaseManager.Current.InsertPlainPlaylist(name);
            var playlist = await DatabaseManager.Current.GetPlainPlaylistAsync(id);
            Playlists.Add(playlist);
            Name = "";
        }
    }
}
