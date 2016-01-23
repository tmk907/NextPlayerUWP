using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

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
                        break;
                    case MusicItemTypes.genre:
                        Playlist = await DatabaseManager.Current.GetSongItemsFromGenreAsync(firstParam);
                        break;
                    case MusicItemTypes.plainplaylist:
                        Playlist = await DatabaseManager.Current.GetSongItemsFromPlainPlaylistAsync(Int32.Parse(firstParam));
                        break;
                    case MusicItemTypes.smartplaylist:
                        //Playlist = await DatabaseManager.Current.GetSongItemsFromSmartPlaylistAsync(Int32.Parse(firstParam));
                        break;
                }
            }
        }

        public override void OnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            base.OnNavigatedTo(parameter, mode, state);
            if (parameter != null)
            {
                type = MusicItem.ParseType(parameter as string);
                firstParam = MusicItem.ParseParameter(parameter as string)[1];
            }
        }
    }
}
