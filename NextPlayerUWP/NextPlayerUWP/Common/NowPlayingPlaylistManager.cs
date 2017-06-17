using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.CloudStorage;
using Template10.Common;

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
            Logger2.DebugWrite("NowPlayingPlaylistManager()","");
            PlaybackService.MediaPlayerTrackChanged += PlaybackService_MediaPlayerTrackChanged;
            PlaybackService.StreamUpdated += PlaybackService_StreamUpdated;
            App.SongUpdated += App_SongUpdated;
            SortingHelper = new SortingHelperForSongItemsInPlaylist("nowplaying");
            SortingHelper.SelectedSortOption = SortingHelper.ComboBoxItemValues.FirstOrDefault();
        }

        public async Task Init()
        {
            System.Diagnostics.Debug.WriteLine("NowPlayingPlaylistManager.Init()");
            songs = await DatabaseManager.Current.GetSongItemsFromNowPlayingAsync();
            songs.CollectionChanged += Songs_CollectionChanged;
            OnNPChanged();
            currentIndex = ApplicationSettingsHelper.ReadSongIndex();
        }

        public async Task Init(MusicItem item)
        {
            System.Diagnostics.Debug.WriteLine("NowPlayingPlaylistManager.Init(MusicItem )");
            await NewPlaylist(item);
            songs.CollectionChanged += Songs_CollectionChanged;
            OnNPChanged();
            currentIndex = 0;
        }

        public SortingHelperForSongItemsInPlaylist SortingHelper;

        private DispatcherWrapper dispatcher;
        public void SetDispatcher(DispatcherWrapper dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        private int currentIndex = 0;

        public static event NPListChangedHandler NPListChanged;
        public static void OnNPChanged()
        {
            NPListChanged?.Invoke();
        }

        private void PlaybackService_StreamUpdated(NowPlayingSong updatedSong)
        {
            dispatcher?.Dispatch(() =>
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
            });
        }

        private async void App_SongUpdated(int id)
        {
            await dispatcher?.DispatchAsync(async () =>
            {
                SongItem si = songs.Where(s => s.SongId.Equals(id)).FirstOrDefault();
                if (si != null)
                {
                    int i = songs.IndexOf(si);
                    songs[i] = await DatabaseManager.Current.GetSongItemAsync(id);
                    await NotifyChange();
                }
            });
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
            if (currentIndex != index)
            {
                currentIndex = index;
                int i = 0;
                if (dispatcher == null)
                {
                    if (WindowWrapper.Current().Dispatcher == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Dispatcher null");
                        return;
                    }
                    else
                    {
                        dispatcher = WindowWrapper.Current().Dispatcher;
                    }
                }
                foreach (var song in songs)
                {
                    if (i != index && song.IsPlaying)
                    {
                        dispatcher.Dispatch(() =>
                        {
                            song.IsPlaying = false;
                        });
                    }
                    else if (i == index)
                    {
                        dispatcher.Dispatch(() =>
                        {
                            song.IsPlaying = true;
                        });
                    }
                    i++;
                }
            }
        }

        public async Task Add(MusicItem item)
        {
            if (songs.Count == 0)
            {
                await NewPlaylist(item);
                await PlaybackService.Instance.PlayNewList(0, false);
                return;
            }
            IEnumerable<SongItem> list = new ObservableCollection<SongItem>();
            switch (MusicItem.ParseType(item.GetParameter()))
            {
                case MusicItemTypes.album:
                    list = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(((AlbumItem)item).AlbumParam, ((AlbumItem)item).AlbumArtist);
                    break;
                case MusicItemTypes.albumartist:
                    list = await DatabaseManager.Current.GetSongItemsFromAlbumArtistAsync(((AlbumArtistItem)item).AlbumArtist);
                    break;
                case MusicItemTypes.artist:
                    list = await DatabaseManager.Current.GetSongItemsFromArtistAsync(((ArtistItem)item).ArtistParam);
                    break;
                case MusicItemTypes.folder:
                    bool subFolders = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.IncludeSubFolders);
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
                case MusicItemTypes.pcloudfolder:
                    if (typeof(CloudRootFolder) == item.GetType())
                    {
                        var folder = (CloudRootFolder)item;
                        var factory = new CloudStorageServiceFactory();
                        var service = factory.GetService(folder.CloudType, folder.UserId);
                        try
                        {
                            var id = await service.GetRootFolderId();
                            list = await service.GetSongItems(id);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        var folder = (CloudFolder)item;
                        var factory = new CloudStorageServiceFactory();
                        var service = factory.GetService(folder.CloudType, folder.UserId);
                        try
                        {
                            list = await service.GetSongItems(folder.Id);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    break;
            }
            foreach (var song in list)
            {
                songs.Add(song);
            }
            await NotifyChange();
        }

        public async Task Add(IEnumerable<MusicItem> items)
        {
            if (songs.Count == 0)
            {
                await NewPlaylist(items);
                await PlaybackService.Instance.PlayNewList(0, false);
                return;
            }
            if (items.OfType<SongItem>().Count() == items.Count())
            {
                foreach (SongItem song in items)
                {
                    songs.Add(song);
                }
            }
            else
            {
                SongItemsFactory factory = new SongItemsFactory();
                foreach(var item in items)
                {
                    var list = await factory.GetSongItems(item);
                    foreach(var song in list)
                    {
                        songs.Add(song);
                    }
                }
            }
            await NotifyChange();
        }

        public async Task AddNext(MusicItem item)
        {
            if (songs.Count == 0)
            {
                await NewPlaylist(item);
                await PlaybackService.Instance.PlayNewList(0, false);
                return;
            }
            IEnumerable<SongItem> list = new List<SongItem>();
            switch (MusicItem.ParseType(item.GetParameter()))
            {
                case MusicItemTypes.album:
                    list = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(((AlbumItem)item).AlbumParam, ((AlbumItem)item).AlbumArtist);
                    break;
                case MusicItemTypes.albumartist:
                    var temp = await DatabaseManager.Current.GetSongItemsFromAlbumArtistAsync(((AlbumArtistItem)item).AlbumArtist);
                    list = new ObservableCollection<SongItem>(temp.OrderBy(a => a.Album).ThenBy(b => b.TrackNumber));
                    break;
                case MusicItemTypes.artist:
                    list = await DatabaseManager.Current.GetSongItemsFromArtistAsync(((ArtistItem)item).ArtistParam);
                    break;
                case MusicItemTypes.folder:
                    bool subFolders = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.IncludeSubFolders);
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
                case MusicItemTypes.pcloudfolder:
                    if (typeof (CloudRootFolder) == item.GetType())
                    {
                        var folder = (CloudRootFolder)item;
                        var factory = new CloudStorageServiceFactory();
                        var service = factory.GetService(folder.CloudType, folder.UserId);
                        var id = await service.GetRootFolderId();
                        list = await service.GetSongItems(id);
                    }
                    else
                    {
                        var folder = (CloudFolder)item;
                        var factory = new CloudStorageServiceFactory();
                        var service = factory.GetService(folder.CloudType, folder.UserId);
                        list = await service.GetSongItems(folder.Id);
                    }
                    break;
            }
            int index = ApplicationSettingsHelper.ReadSongIndex();
            foreach (var n in list)
            {
                index++;
                songs.Insert(index, n);
            }
            await NotifyChange();
        }

        public async Task AddNext(IEnumerable<MusicItem> items)
        {
            if (songs.Count == 0)
            {
                await NewPlaylist(items);
                await PlaybackService.Instance.PlayNewList(0, false);
                return;
            }
            int index = ApplicationSettingsHelper.ReadSongIndex();
            if (items.OfType<SongItem>().Count() == items.Count())
            {
                foreach (SongItem n in items)
                {
                    index++;
                    songs.Insert(index, n);
                }
            }
            else
            {
                SongItemsFactory factory = new SongItemsFactory();
                foreach (var item in items)
                {
                    var list = await factory.GetSongItems(item);
                    foreach (var song in list)
                    {
                        index++;
                        songs.Insert(index, song);
                    }
                }
            }
            await NotifyChange();
        }

        public async Task Delete(SongItem song)
        {
            songs.Remove(song);
            await NotifyChange();
        }

        public async Task Delete(int songId)
        {
            if (songs.Count == 1 || GetCurrentPlaying().SongId == songId) return;

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
                case MusicItemTypes.albumartist:
                    var temp = await DatabaseManager.Current.GetSongItemsFromAlbumArtistAsync(((AlbumArtistItem)item).AlbumArtist);
                    list = new ObservableCollection<SongItem>(temp.OrderBy(a => a.Album).ThenBy(b => b.TrackNumber));
                    break;
                case MusicItemTypes.artist:
                    list = await DatabaseManager.Current.GetSongItemsFromArtistAsync(((ArtistItem)item).ArtistParam);
                    break;
                case MusicItemTypes.folder:
                    bool subFolders = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.IncludeSubFolders);
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
                case MusicItemTypes.pcloudfolder:
                    if (typeof(CloudRootFolder) == item.GetType())
                    {
                        var folder = (CloudRootFolder)item;
                        var factory = new CloudStorageServiceFactory();
                        var service = factory.GetService(folder.CloudType, folder.UserId);
                        var id = await service.GetRootFolderId();
                        list = await service.GetSongItems(id);
                    }
                    else
                    {
                        var folder = (CloudFolder)item;
                        var factory = new CloudStorageServiceFactory();
                        var service = factory.GetService(folder.CloudType, folder.UserId);
                        list = await service.GetSongItems(folder.Id);
                    }
                    break;
            }
            songs.Clear();
            foreach(var song in list)
            {
                songs.Add(song);
            }
            await NotifyChange(true);
        }

        public async Task NewPlaylist(IEnumerable<MusicItem> items)
        {
            songs.Clear();

            if (items.OfType<SongItem>().Count() == items.Count())
            {
                foreach (SongItem song in items)
                {
                    songs.Add(song);
                }
            }
            else
            {
                SongItemsFactory factory = new SongItemsFactory();
                foreach (var item in items)
                {
                    var list = await factory.GetSongItems(item);
                    foreach (var song in list)
                    {
                        songs.Add(song);
                    }
                }
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
        public async Task<int> ShufflePlaylist(int startIndex)
        {
            ar = new int[songs.Count];
            int n = songs.Count;
            int newCurrentIndex = startIndex;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                SongItem value = songs[k];
                songs[k] = songs[n];
                songs[n] = value;
                ar[n] = k;
                if (k == newCurrentIndex) newCurrentIndex = n;
                else if (n == newCurrentIndex) newCurrentIndex = k;
            }
            OnNPChanged();
            await SaveNowPlayingInDB();
            return newCurrentIndex;
        }

        public async Task<int> UnShufflePlaylist()
        {
            int newCurrentIndex = currentIndex;
            if (ar !=null && ar.Length == songs.Count)
            {
                int n = 0;
                while (n < songs.Count - 1)
                {
                    n++;
                    int k = ar[n];
                    SongItem value = songs[k];
                    songs[k] = songs[n];
                    songs[n] = value;
                    if (k == newCurrentIndex) newCurrentIndex = n;
                    else if (n == newCurrentIndex) newCurrentIndex = k;
                }
                OnNPChanged();
                await SaveNowPlayingInDB();
            }
            return newCurrentIndex;
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
            await DatabaseManager.Current.InsertNewNowPlayingPlaylistAsync(new List<SongItem>(songs));
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
            if (index < 0 || index > songs.Count - 1 || songs.Count == 1) return null;
            index++;
            if (index == songs.Count) index = 0;
            return songs[index];
        }

        public SongItem GetPreviousSong()
        {
            int index = ApplicationSettingsHelper.ReadSongIndex();
            if (index < 0 || index > songs.Count - 1 || songs.Count == 1) return null;
            index--;
            if (index == -1) index = songs.Count - 1;
            return songs[index];
        }

        private async Task NotifyChange(bool newPlaylist = false)
        {
            Logger2.DebugWrite("NowPlayingPlaylistManager()", "NotifyChange");
            OnNPChanged();
            if (!newPlaylist)
            {
                await PlaybackService.Instance.UpdateMediaListWithoutPausing();
            }
            await SaveNowPlayingInDB();
            TelemetryAdapter.TrackMetrics("nowPlayingLength", songs.Count);
            //SendMessage(AppConstants.NowPlayingListChanged);
            //await PlaybackService.Instance.NewPlaylists(songs);
        }

        public async Task SortPlaylist()
        {
            if (songs.Count == 0) return;
            var songid = GetCurrentPlaying().SongId;
            var orderSelector = SortingHelper.GetOrderBySelector();
            var list = songs.OrderBy(orderSelector).ToList();
            songs.Clear();
            int newIndex = 0;
            foreach(var song in list)
            {
                if (song.SongId == songid)
                {
                    ApplicationSettingsHelper.SaveSongIndex(newIndex);
                    currentIndex = newIndex;
                }
                songs.Add(song);
                newIndex++;
            }
            await NotifyChange();
        }

    }
}
