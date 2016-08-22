using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.CloudStorage;

namespace NextPlayerUWP.Common
{
    public delegate void NPListChangedHandler();
    public sealed class NowPlayingPlaylistManager
    {
        private static readonly NowPlayingPlaylistManager current = new NowPlayingPlaylistManager();
        public static NowPlayingPlaylistManager Current
        {
            get { return current; }
        }

        static NowPlayingPlaylistManager() { }

        private NowPlayingPlaylistManager()
        {
            Logger.DebugWrite("NowPlayingPlaylistManager()", "");
            songs = DatabaseManager.Current.GetSongItemsFromNowPlaying();
            songs.CollectionChanged += Songs_CollectionChanged;
            PlaybackService.MediaPlayerTrackChanged += PlaybackService_MediaPlayerTrackChanged;
            PlaybackService.StreamUpdated += PlaybackService_StreamUpdated;
            App.SongUpdated += App_SongUpdated;
            currentIndex = ApplicationSettingsHelper.ReadSongIndex();
        }

        private int currentIndex;

        public static event NPListChangedHandler NPListChanged;
        public static void OnNPChanged()
        {
            NPListChanged?.Invoke();
        }

        private void PlaybackService_StreamUpdated(NowPlayingSong updatedSong)
        {
            SongItem si = songs.Where(s => (s.SongId.Equals(updatedSong.SongId) && s.SourceType.Equals(updatedSong.SourceType))).FirstOrDefault();
            if (si != null)
            {
                int i = songs.IndexOf(si);
                var temp = songs[i];
                temp.Album = updatedSong.Album;
                temp.Artist = updatedSong.Artist;
                temp.Title = updatedSong.Title;
                temp.CoverPath = updatedSong.ImagePath;
                songs[i] = temp;
                OnNPChanged();
            }
        }

        private async void App_SongUpdated(int id)
        {
            SongItem si = songs.Where(s => s.SongId.Equals(id)).FirstOrDefault();
            if (si != null)
            {
                int i = songs.IndexOf(si);
                songs[i] = await DatabaseManager.Current.GetSongItemAsync(id);
                await NotifyChange();
            }
        }

        public ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();

        private int removeIndex = 0;
        private int addIndex = 0;
        private DateTime removedTime = DateTime.Now;

        private void Songs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    removeIndex = e.OldStartingIndex;
                    removedTime = DateTime.Now;
                    break;
                case NotifyCollectionChangedAction.Add:
                    if (songs.Count == 1) return;
                    if (DateTime.Now - removedTime > TimeSpan.FromSeconds(1))
                        return;
                    addIndex = e.NewStartingIndex;
                    HandleReorder();
                    break;
            }
        }

        private async Task HandleReorder()
        {
            int index = ApplicationSettingsHelper.ReadSongIndex();
            if (removeIndex == index)
            {
                index = addIndex;
                ApplicationSettingsHelper.SaveSongIndex(index);
            }
            else if (removeIndex < index && addIndex >= index)
            {
                index--;
                ApplicationSettingsHelper.SaveSongIndex(index);
            }
            else if (removeIndex > index && addIndex <= index)
            {
                index++;
                ApplicationSettingsHelper.SaveSongIndex(index);
            }
            else
            {
                
            }
            await NotifyChange();
        }

        private void PlaybackService_MediaPlayerTrackChanged(int index)
        {
            currentIndex = index;
            //var dispatcher = Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().CoreWindow.Dispatcher;
            //if (dispatcher != null)
            //{
            //    await dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            //    {
            //        
            //        int i = 0;
            //        foreach (var song in songs)
            //        {
            //            if (i != index)
            //            {
            //                song.IsPlaying = false;
            //            }
            //            else
            //            {
            //                song.IsPlaying = true;
            //            }
            //            i++;
            //        }
            //    });
            //}
        }

        public async Task Add(MusicItem item)
        {
            IEnumerable<SongItem> list = new ObservableCollection<SongItem>();
            switch (MusicItem.ParseType(item.GetParameter()))
            {
                case MusicItemTypes.album:
                    list = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(((AlbumItem)item).AlbumParam, ((AlbumItem)item).AlbumArtist);
                    break;
                case MusicItemTypes.artist:
                    var c = await DatabaseManager.Current.GetSongItemsFromArtistAsync(((ArtistItem)item).ArtistParam);
                    list = new ObservableCollection<SongItem>(c.OrderBy(a => a.Album).ThenBy(b => b.TrackNumber));
                    break;
                case MusicItemTypes.folder:
                    bool subFolders = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IncludeSubFolders);
                    list = await DatabaseManager.Current.GetSongItemsFromFolderAsync(((FolderItem)item).Directory, subFolders);
                    break;
                case MusicItemTypes.genre:
                    list = await DatabaseManager.Current.GetSongItemsFromGenreAsync(((GenreItem)item).GenreParam);
                    break;
                case MusicItemTypes.plainplaylist:
                    list = await DatabaseManager.Current.GetSongItemsFromPlainPlaylistAsync(((PlaylistItem)item).Id);
                    break;
                case MusicItemTypes.smartplaylist:
                    list = await DatabaseManager.Current.GetSongItemsFromSmartPlaylistAsync(((PlaylistItem)item).Id);
                    break;
                case MusicItemTypes.song:
                    songs.Add((SongItem)item);
                    break;
                case MusicItemTypes.radio:
                    songs.Add(((RadioItem)item).ToSongItem());
                    break;
                case MusicItemTypes.onedrivefolder:
                case MusicItemTypes.dropboxfolder:
                    var folder = (CloudFolder)item;
                    var factory = new CloudStorageServiceFactory();
                    var service = factory.GetService(folder.CloudType, folder.UserId);
                    list = await service.GetSongItems(folder.Id);
                    break;
            }
            foreach (var song in list)
            {
                songs.Add(song);
            }
            await NotifyChange();
        }

        public async Task Add(IEnumerable<SongItem> newSongs)
        {
            foreach(var s in newSongs)
            {
                songs.Add(s);
            }
            await NotifyChange();
        }

        public async Task AddNext(MusicItem item)
        {
            IEnumerable<SongItem> list = new List<SongItem>();
            switch (MusicItem.ParseType(item.GetParameter()))
            {
                case MusicItemTypes.album:
                    list = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(((AlbumItem)item).AlbumParam, ((AlbumItem)item).AlbumArtist);
                    break;
                case MusicItemTypes.artist:
                    var c = await DatabaseManager.Current.GetSongItemsFromArtistAsync(((ArtistItem)item).ArtistParam);
                    list = new ObservableCollection<SongItem>(c.OrderBy(a => a.Album).ThenBy(b => b.TrackNumber));
                    break;
                case MusicItemTypes.folder:
                    bool subFolders = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IncludeSubFolders);
                    list = await DatabaseManager.Current.GetSongItemsFromFolderAsync(((FolderItem)item).Directory, subFolders);
                    break;
                case MusicItemTypes.genre:
                    list = await DatabaseManager.Current.GetSongItemsFromGenreAsync(((GenreItem)item).Genre);
                    break;
                case MusicItemTypes.plainplaylist:
                    list = await DatabaseManager.Current.GetSongItemsFromPlainPlaylistAsync(((PlaylistItem)item).Id);
                    break;
                case MusicItemTypes.smartplaylist:
                    list = await DatabaseManager.Current.GetSongItemsFromSmartPlaylistAsync(((PlaylistItem)item).Id);
                    break;
                case MusicItemTypes.song:
                    list = new List<SongItem>() { (SongItem)item };
                    break;
                case MusicItemTypes.radio:
                    list = new List<SongItem>() { ((RadioItem)item).ToSongItem() };
                    break;
                case MusicItemTypes.onedrivefolder:
                case MusicItemTypes.dropboxfolder:
                    var folder = (CloudFolder)item;
                    var factory = new CloudStorageServiceFactory();
                    var service = factory.GetService(folder.CloudType, folder.UserId);
                    list = await service.GetSongItems(folder.Id);
                    break;
            }
            int index = ApplicationSettingsHelper.ReadSongIndex();
            foreach(var n in list)
            {
                index++;
                songs.Insert(index, n);
            }
            await NotifyChange();
        }

        //public async Task AddNext(IEnumerable<SongItem> songs)
        //{
        //    //await SaveNowPlayingInDB();

        //}

        public async Task Delete(SongItem song)
        {
            songs.Remove(song);
            await NotifyChange();
        }

        public async Task Delete(int songId)
        {
            int i = 0;
            foreach(var s in songs)
            {
                if (s.SongId == songId) break;
                i++;
            }
            songs.RemoveAt(i);
            int index = ApplicationSettingsHelper.ReadSongIndex();
            if (i < index)
            {
                ApplicationSettingsHelper.SaveSongIndex(index - 1);
            }
            await NotifyChange();
        }

        public async Task NewPlaylist(MusicItem item)
        {
            IEnumerable<SongItem> list = new List<SongItem>();
            switch (MusicItem.ParseType(item.GetParameter()))
            {
                case MusicItemTypes.album:
                    list = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(((AlbumItem)item).AlbumParam, ((AlbumItem)item).AlbumArtist);
                    break;
                case MusicItemTypes.artist:
                    var c = await DatabaseManager.Current.GetSongItemsFromArtistAsync(((ArtistItem)item).ArtistParam);
                    list = new ObservableCollection<SongItem>(c.OrderBy(a => a.Album).ThenBy(b => b.TrackNumber));
                    break;
                case MusicItemTypes.folder:
                    bool subFolders = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IncludeSubFolders);
                    list = await DatabaseManager.Current.GetSongItemsFromFolderAsync(((FolderItem)item).Directory, subFolders);
                    break;
                case MusicItemTypes.genre:
                    list = await DatabaseManager.Current.GetSongItemsFromGenreAsync(((GenreItem)item).Genre);
                    break;
                case MusicItemTypes.plainplaylist:
                    list = await DatabaseManager.Current.GetSongItemsFromPlainPlaylistAsync(((PlaylistItem)item).Id);
                    break;
                case MusicItemTypes.smartplaylist:
                    list = await DatabaseManager.Current.GetSongItemsFromSmartPlaylistAsync(((PlaylistItem)item).Id);
                    break;
                case MusicItemTypes.song:
                    list = new List<SongItem>() { (SongItem)item };
                    break;
                case MusicItemTypes.radio:
                    list = new List<SongItem>() { ((RadioItem)item).ToSongItem() };
                    break;
                case MusicItemTypes.onedrivefolder:
                case MusicItemTypes.dropboxfolder:
                    var folder = (CloudFolder)item;
                    var factory = new CloudStorageServiceFactory();
                    var service = factory.GetService(folder.CloudType, folder.UserId);
                    list = await service.GetSongItems(folder.Id);
                    break;
            }
            songs.Clear();
            foreach(var song in list)
            {
                songs.Add(song);
            }
            await NotifyChange(true);
        }

        public async Task UpdateSong(SongData updatedSong)
        {
            foreach(var song in songs)
            {
                if (song.SongId == updatedSong.SongId)
                {
                    song.Album = updatedSong.Tag.Album;
                    song.AlbumArtist = updatedSong.Tag.AlbumArtist;
                    song.Artist = updatedSong.Tag.Artists;
                    song.Composer = updatedSong.Tag.Composers;
                    song.Rating = (int)updatedSong.Tag.Rating;
                    song.Title = updatedSong.Tag.Title;
                    song.TrackNumber = updatedSong.Tag.Track;
                    song.Year = updatedSong.Tag.Year;
                    song.Genres = updatedSong.Tag.Genres;

                    await DatabaseManager.Current.UpdateNowPlayingSong(updatedSong);
                    //!!SendMessage(AppConstants.NowPlayingListRefresh);
                    break;
                }
            }
        }

        public async Task NewPlaylist(IEnumerable<SongItem> playlist)
        {
            songs.Clear();
            foreach (var song in playlist)
            {
                songs.Add(song);
            }
            await NotifyChange(true);
        }

        private static Random rng = new Random();
        private int[] ar;
        public async Task ShufflePlaylist()
        {
            ar = new int[songs.Count];
            int n = songs.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                SongItem value = songs[k];
                songs[k] = songs[n];
                songs[n] = value;
                ar[n] = k;
            }
            await NotifyChange();

        }

        public async Task UnShufflePlaylist()
        {
            int n = 0;
            while (n < songs.Count - 1)
            {
                n++;
                int k = ar[n];
                SongItem value = songs[k];
                songs[k] = songs[n];
                songs[n] = value;
            }
            await NotifyChange();
        }

        public async Task NewPlaylist(ObservableCollection<GroupList> grouped)
        {
            songs.Clear();
            foreach(GroupList group in grouped)
            {
                foreach(SongItem song in group)
                {
                    songs.Add(song);
                }
            }
            await NotifyChange(true);
        }

        private async Task SaveNowPlayingInDB()
        {
            await DatabaseManager.Current.InsertNewNowPlayingPlaylistAsync(songs);
        }

        public SongItem GetSongItem(int index)
        {
            if (index < 0 || index > songs.Count - 1) return new SongItem();
            return songs[index];
        }

        public SongItem GetCurrentPlaying()
        {
            //int index = ApplicationSettingsHelper.ReadSongIndex();
            if (currentIndex < 0 || currentIndex > songs.Count - 1) return new SongItem();
            return songs[currentIndex];
        }

        public SongItem GetNextSong()
        {
            int index = ApplicationSettingsHelper.ReadSongIndex();
            if (index < 0 || index > songs.Count - 1) return new SongItem();
            index++;
            if (index == songs.Count) index = 0;
            return songs[index];
        }

        private async Task NotifyChange(bool newPlaylist = false)
        {
            Logger.DebugWrite("NowPlayingPlaylistManager()", "NotifyChange");
            OnNPChanged();
            if (!newPlaylist)
            {
                await PlaybackService.Instance.UpdateMediaListWithoutPausing();
            }
            await SaveNowPlayingInDB();
            //SendMessage(AppConstants.NowPlayingListChanged);
            //await PlaybackService.Instance.NewPlaylists(songs);
        }
    }
}
