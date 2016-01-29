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
            if (!isBack || Playlist.Count == 0)
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

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (!isBack)
            {
                Playlist = new ObservableCollection<SongItem>();
            }
            if (parameter != null)
            {
                type = MusicItem.ParseType(parameter as string);
                firstParam = MusicItem.ParseParameter(parameter as string)[1];
            }
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

        public void EditTags(object sender, RoutedEventArgs e)
        {
            SelectedItem = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            NavigationService.Navigate(App.Pages.TagsEditor, ((SongItem)SelectedItem).SongId);
        }

        public void ShowDetails(object sender, RoutedEventArgs e)
        {
            SelectedItem = (MusicItem)((MenuFlyoutItem)e.OriginalSource).CommandParameter;
            NavigationService.Navigate(App.Pages.FileInfo, ((SongItem)SelectedItem).SongId);
        }
    }
}
