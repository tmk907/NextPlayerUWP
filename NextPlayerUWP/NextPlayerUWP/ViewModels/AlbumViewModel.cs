using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Services.NavigationService;
using NextPlayerUWPDataLayer.SpotifyAPI.Web;
using NextPlayerUWPDataLayer.Constants;

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
                    if (album.AlbumParam == "")
                    {
                        Album.ImagePath = AppConstants.AlbumCover;
                        album.ImageUri = new Uri(album.ImagePath);
                    }
                    else
                    {
                        await AlbumArtFinder.UpdateAlbumArt(album);
                    }
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
            await PlaybackService.Instance.PlayNewList(index);
            //NavigationService.Navigate(App.Pages.NowPlaying, ((SongItem)e.ClickedItem).GetParameter());
        }

        public async void ShuffleAllSongs()
        {
            if (songs.Count == 0) return;
            List<SongItem> list = new List<SongItem>();
            foreach (var s in songs)
            {
                list.Add(s);
            }
            Random rnd = new Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                SongItem value = list[k];
                list[k] = list[n];
                list[n] = value;
            }

            await NowPlayingPlaylistManager.Current.NewPlaylist(list);
            await PlaybackService.Instance.PlayNewList(0);
        }

        public async void EditAlbum()
        {
            EditedAlbum = await DatabaseManager.Current.GetAlbumItemAsync(albumId);
        }

        public async void SaveAlbum()
        {
            var newAlbum = await DatabaseManager.Current.GetAlbumItemAsync(editedAlbum.AlbumParam, editedAlbum.AlbumArtist);

            if (album.SongsNumber == 1)
            {
                if (newAlbum.AlbumId > 0)
                {
                    album.LastAdded = (album.LastAdded > newAlbum.LastAdded) ? album.LastAdded : newAlbum.LastAdded;
                    album.ImagePath = (newAlbum.ImagePath == AppConstants.AlbumCover) ? album.ImagePath : 
                        (album.ImagePath == AppConstants.AlbumCover) ? newAlbum.ImagePath : album.ImagePath;
                    album.ImageUri = new Uri(album.ImagePath);
                    album.IsImageSet = album.ImagePath != "";// newAlbum.IsImageSet;
                    album.Duration += newAlbum.Duration;
                    album.SongsNumber += newAlbum.SongsNumber;
                    await DatabaseManager.Current.DeleteAlbumAsync(newAlbum.AlbumParam, newAlbum.AlbumArtist);
                }
                album.AlbumParam = editedAlbum.AlbumParam;
                album.AlbumArtist = editedAlbum.AlbumArtist;
                album.Year = editedAlbum.Year;
                await DatabaseManager.Current.UpdateAlbumItem(album);
            }
            else
            {
                if (newAlbum.AlbumId > 0 && newAlbum.AlbumId != albumId)//merge albums
                {
                    album.LastAdded = (album.LastAdded > newAlbum.LastAdded) ? album.LastAdded : newAlbum.LastAdded;
                    if (!album.IsImageSet && newAlbum.IsImageSet)
                    {
                        album.ImagePath = newAlbum.ImagePath;
                        album.ImageUri = newAlbum.ImageUri;
                        album.IsImageSet = true;
                    }
                    album.Duration += newAlbum.Duration;
                    album.SongsNumber += newAlbum.SongsNumber;

                    await DatabaseManager.Current.DeleteAlbumAsync(editedAlbum.AlbumParam, editedAlbum.AlbumArtist);
                }
            }
            //Album = editedAlbum;
            Album.Album = editedAlbum.AlbumParam;
            Album.AlbumParam = EditedAlbum.AlbumParam;
            Album.AlbumArtist = editedAlbum.AlbumArtist;
            Album.Year = editedAlbum.Year;

            await DatabaseManager.Current.UpdateAlbumItem(album);
            //Album = await DatabaseManager.Current.GetAlbumItemAsync(albumId);
            foreach(var song in songs)
            {
                song.Album = album.AlbumParam;
                song.AlbumArtist = album.AlbumArtist;
                song.Year = album.Year;
                await DatabaseManager.Current.UpdateSongAlbumData(song);
            }
            songs = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(editedAlbum.AlbumParam, editedAlbum.AlbumArtist);
            Songs = new ObservableCollection<SongItem>(songs.OrderBy(s => s.Disc).ThenBy(t => t.TrackNumber));
            App.OnSongUpdated(songs.FirstOrDefault().SongId);   
        }

        public async void PlayAlbum()
        {
            int index = 0;
            await NowPlayingPlaylistManager.Current.NewPlaylist(songs);
            await PlaybackService.Instance.PlayNewList(index);
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
