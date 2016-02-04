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
using Template10.Services.NavigationService;

namespace NextPlayerUWP.ViewModels
{
    public class PlaylistViewModel : MusicViewModelBase
    {
        private MusicItemTypes type;
        string firstParam;

        private ObservableCollection<SongItem> playlist = new ObservableCollection<SongItem>();
        public ObservableCollection<SongItem> Playlist
        {
            get { return playlist; }
            set { Set(ref playlist, value); }
        }

        protected override async Task LoadData()
        {
            if (Playlist.Count == 0)
            {
                switch (type)
                {
                    case MusicItemTypes.folder:
                        Playlist = await DatabaseManager.Current.GetSongItemsFromFolderAsync(firstParam);
                        PageTitle = "Folder";
                        break;
                    case MusicItemTypes.genre:
                        Playlist = await DatabaseManager.Current.GetSongItemsFromGenreAsync(firstParam);
                        PageTitle = "Genre";
                        break;
                    case MusicItemTypes.plainplaylist:
                        Playlist = await DatabaseManager.Current.GetSongItemsFromPlainPlaylistAsync(Int32.Parse(firstParam));
                        PageTitle = "Playlist";
                        break;
                    case MusicItemTypes.smartplaylist:
                        //Playlist = await DatabaseManager.Current.GetSongItemsFromSmartPlaylistAsync(Int32.Parse(firstParam));
                        PageTitle = "Playlist";
                        break;
                }
            }
        }

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter != null)
            {
                type = MusicItem.ParseType(parameter as string);
                firstParam = MusicItem.ParseParameter(parameter as string)[1];
            }
        }

        public override Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            if (args.NavigationMode == NavigationMode.Back || args.NavigationMode == NavigationMode.New)
            {
                playlist = new ObservableCollection<SongItem>();
                pageTitle = "";
            }
            return base.OnNavigatingFromAsync(args);
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            int index = 0;
            foreach (var s in playlist)
            {
                if (s.SongId == ((SongItem)e.ClickedItem).SongId) break;
                index++;
            }
            await NowPlayingPlaylistManager.Current.NewPlaylist(playlist);
            ApplicationSettingsHelper.SaveSongIndex(index);
            PlaybackManager.Current.PlayNew();
            //NavigationService.Navigate(App.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }
    }
}
