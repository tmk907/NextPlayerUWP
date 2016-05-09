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
using NextPlayerUWPDataLayer.SpotifyAPI.Web;

namespace NextPlayerUWP.ViewModels
{
    public class AlbumViewModel : MusicViewModelBase
    {
        int albumId;

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

        private AlbumItem editedAlbum = new AlbumItem();
        public AlbumItem EditedAlbum
        {
            get { return editedAlbum; }
            set { Set(ref editedAlbum, value); }
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
                Album = await DatabaseManager.Current.GetAlbumItemAsync(albumId);
                songs = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(album.AlbumParam, album.AlbumArtist);
                Songs = new ObservableCollection<SongItem>(songs.OrderBy(s => s.Disc).ThenBy(t=>t.TrackNumber));
                if (!album.IsImageSet)
                {
                    string path = await ImagesManager.GetAlbumCoverPath(album);
                    Album.ImagePath = path;
                    Album.ImageUri = new Uri(path);
                    await DatabaseManager.Current.UpdateAlbumImagePath(album);
                }
            }
        }

        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter != null)
            {
                try
                {
                    albumId = Int32.Parse(parameter.ToString());
                }
                catch (Exception ex)
                {

                }
            }
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
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
            args.Cancel = false;
            await Task.CompletedTask;
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

        public void EditAlbum()
        {
            EditedAlbum = album;
        }

        public async void SaveAlbum()
        {
            Album = editedAlbum;
            var a2 = await DatabaseManager.Current.GetAlbumItemAsync(editedAlbum.Album, editedAlbum.AlbumArtist);
            if (a2.AlbumId > 0)//merge albums
            {
                album.LastAdded = (album.LastAdded > a2.LastAdded) ? album.LastAdded : a2.LastAdded;
                if (!album.IsImageSet && a2.IsImageSet)
                {
                    album.ImagePath = a2.ImagePath;
                    album.ImageUri = a2.ImageUri;
                }
                album.Duration += a2.Duration;
                album.SongsNumber += a2.SongsNumber;
                
                await DatabaseManager.Current.DeleteAlbumAsync(editedAlbum.Album, editedAlbum.AlbumArtist);
            }
            await DatabaseManager.Current.UpdateAlbumItem(album);
            foreach(var song in songs)
            {
                song.Album = album.Album;
                song.AlbumArtist = album.AlbumArtist;
                song.Year = album.Year;
                await DatabaseManager.Current.UpdateSongAlbumData(song);
            }
            songs = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(editedAlbum.Album, editedAlbum.AlbumArtist);
            Songs = new ObservableCollection<SongItem>(songs.OrderBy(s => s.Disc).ThenBy(t => t.TrackNumber));
            App.OnSongUpdated(songs.FirstOrDefault().SongId);   
        }

        public async void PlayAlbum()
        {
            int index = 0;
            await NowPlayingPlaylistManager.Current.NewPlaylist(songs);
            ApplicationSettingsHelper.SaveSongIndex(index);
            PlaybackManager.Current.PlayNew();
        }

        public async void PlayAlbumNext()
        {
            await NowPlayingPlaylistManager.Current.AddNext(album);
        }

        public async void AddAlbumToNowPlaying()
        {
            //FindYear();
            await NowPlayingPlaylistManager.Current.Add(album);
        }

        public void AddAlbumToPlaylist()
        {
            NavigationService.Navigate(App.Pages.AddToPlaylist, album.GetParameter());
        }

        public async void FindYear()
        {
            SpotifyWebAPI s = new SpotifyWebAPI();
            //Uri.EscapeDataString()
            string q = "album%3A%22" + System.Net.WebUtility.UrlEncode(album.Album)+ "%22";
            var i = await s.SearchItemsAsync(q, NextPlayerUWPDataLayer.SpotifyAPI.Web.Enums.SearchType.Album);
            string id = i.Albums.Items.FirstOrDefault().Id;
            var a = await s.GetAlbumAsync(id);
            string y = a.ReleaseDate.Substring(0, 4);
        }
    }
}
