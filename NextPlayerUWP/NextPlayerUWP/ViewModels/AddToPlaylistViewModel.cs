using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Services.NavigationService;
using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.CloudStorage;
using NextPlayerUWPDataLayer.Enums;

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
            App.ChangeBottomPlayerVisibility(true);
            Playlists = await DatabaseManager.Current.GetPlainPlaylistsAsync();
            type = MusicItem.ParseType((string)parameter);
            values = MusicItem.SplitParameter((string)parameter);
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
            switch (type)
            {
                case MusicItemTypes.album:
                    string albArt = values[2];
                    await DatabaseManager.Current.AddToPlaylist(p.Id, a => (a.Album.Equals(value) && a.AlbumArtist.Equals(albArt)),s=>s.Track);
                    break;
                case MusicItemTypes.artist:
                    await DatabaseManager.Current.AddToPlaylist(p.Id, a => a.Artists.Equals(value), s => s.Title);
                    break;
                case MusicItemTypes.folder:
                    bool subFolders = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IncludeSubFolders);
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
                    await DatabaseManager.Current.AddToPlaylist(p.Id, a => a.Genres.Equals(value), s => s.Title);
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
                default:
                    break;
            }
            PlaylistExporter pe = new PlaylistExporter();
            await pe.AutoSavePlaylistAsync(p);
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
