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
            }
            args.Cancel = false;
            await Task.CompletedTask;
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            PlaylistItem p = (PlaylistItem)e.ClickedItem;
            string value = values[1];
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
                    await DatabaseManager.Current.AddToPlaylist(p.Id, a => a.DirectoryName.Equals(value), s => s.Title);
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
                default:
                    break;
            }
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
