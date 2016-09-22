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

        private MusicItemTypes type;
        private string[] values;

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
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
                    folderId = (values.Length == 2) ? values[2] : "";
                    await DatabaseManager.Current.AddToPlaylist(p.Id, a => a.DirectoryName.Equals(folderId) && a.CloudUserId.Equals(userId) && a.MusicSourceType.Equals((int)MusicItemTypes.dropboxfolder), s => s.Title);
                    break;
                case MusicItemTypes.onedrivefolder:
                    userId = values[1];
                    folderId = (values.Length == 2) ? values[2] : null;
                    if (folderId == null)
                    {
                        CloudStorageServiceFactory cssf = new CloudStorageServiceFactory();
                        var service = cssf.GetService(CloudStorageType.OneDrive, userId);
                        folderId = await service.GetRootFolderId();
                    }
                    await DatabaseManager.Current.AddToPlaylist(p.Id, a => a.DirectoryName.Equals(folderId) && a.CloudUserId.Equals(userId) && a.MusicSourceType.Equals((int)MusicItemTypes.onedrivefolder), s => s.Title);

                    break;
                case MusicItemTypes.pcloudfolder:
                    userId = values[1];
                    folderId = (values.Length == 2) ? values[2] : "0";
                    await DatabaseManager.Current.AddToPlaylist(p.Id, a => a.DirectoryName.Equals(folderId) && a.CloudUserId.Equals(userId) && a.MusicSourceType.Equals((int)MusicItemTypes.pcloudfolder), s => s.Title);
                    break;
                default:
                    break;
            }
            PlaylistExporter pe = new PlaylistExporter();
            await pe.AutoSavePlaylist(p);
            NavigationService.GoBack();
        }

        public void Save()
        {
            int id = DatabaseManager.Current.InsertPlainPlaylist(name);
            Playlists.Add(new PlaylistItem(id, false, name));
            Name = "";
        }
    }
}
