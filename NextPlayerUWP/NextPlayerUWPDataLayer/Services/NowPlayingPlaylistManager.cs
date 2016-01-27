using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;

namespace NextPlayerUWPDataLayer.Services
{
    public delegate void NPListChangedHandler();
    public class NowPlayingPlaylistManager
    {
        private static readonly NowPlayingPlaylistManager current = new NowPlayingPlaylistManager();
        public static NowPlayingPlaylistManager Current
        {
            get { return current; }
        }
        static NowPlayingPlaylistManager() { }
        public NowPlayingPlaylistManager()
        {
            songs = DatabaseManager.Current.GetSongItemsFromNowPlaying();
        }

        public static event NPListChangedHandler NPListChanged;

        public static void OnNPChanged()
        {
            if (NPListChanged != null)
            {
                NPListChanged();
            }
        }

        public ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();

        public async Task Add(MusicItem item)
        {
            ObservableCollection<SongItem> list = new ObservableCollection<SongItem>();
            switch (MusicItem.ParseType(item.GetParameter()))
            {
                case MusicItemTypes.album:
                    list = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(((AlbumItem)item).AlbumParam);
                    break;
                case MusicItemTypes.artist:
                    list = await DatabaseManager.Current.GetSongItemsFromArtistAsync(((ArtistItem)item).ArtistParam);
                    break;
                case MusicItemTypes.folder:
                    list = await DatabaseManager.Current.GetSongItemsFromFolderAsync(((FolderItem)item).Directory);
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
                    songs.Add((SongItem)item);
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
            ObservableCollection<SongItem> list = new ObservableCollection<SongItem>();
            switch (MusicItem.ParseType(item.GetParameter()))
            {
                case MusicItemTypes.album:
                    list = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(((AlbumItem)item).AlbumParam);
                    break;
                case MusicItemTypes.artist:
                    list = await DatabaseManager.Current.GetSongItemsFromArtistAsync(((ArtistItem)item).ArtistParam);
                    break;
                case MusicItemTypes.folder:
                    list = await DatabaseManager.Current.GetSongItemsFromFolderAsync(((FolderItem)item).Directory);
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
                    list.Add((SongItem)item);
                    break;
            }
            int index = ApplicationSettingsHelper.ReadSongIndex();
            ObservableCollection<SongItem> newsongs = new ObservableCollection<SongItem>(songs.Take(index + 1));
            foreach(var s in list)
            {
                newsongs.Add(s);
            }
            foreach(var s in songs.Skip(index+1))
            {
                newsongs.Add(s);
            }
            songs = newsongs;

            await NotifyChange();
        }

        public async Task AddNext(IEnumerable<SongItem> songs)
        {
            await SaveNowPlayingInDB();

        }

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
            await NotifyChange();
        }

        public async Task New(MusicItem item)
        {
            switch (MusicItem.ParseType(item.GetParameter()))
            {
                case MusicItemTypes.album:
                    songs = await DatabaseManager.Current.GetSongItemsFromAlbumAsync(((AlbumItem)item).AlbumParam);
                    break;
                case MusicItemTypes.artist:
                    songs = await DatabaseManager.Current.GetSongItemsFromArtistAsync(((ArtistItem)item).ArtistParam);
                    break;
                case MusicItemTypes.folder:
                    songs = await DatabaseManager.Current.GetSongItemsFromFolderAsync(((FolderItem)item).Directory);
                    break;
                case MusicItemTypes.genre:
                    songs = await DatabaseManager.Current.GetSongItemsFromGenreAsync(((GenreItem)item).Genre);
                    break;
                case MusicItemTypes.plainplaylist:
                    songs = await DatabaseManager.Current.GetSongItemsFromPlainPlaylistAsync(((PlaylistItem)item).Id);
                    break;
                case MusicItemTypes.smartplaylist:
                    songs = await DatabaseManager.Current.GetSongItemsFromSmartPlaylistAsync(((PlaylistItem)item).Id);
                    break;
                case MusicItemTypes.song:
                    songs.Clear();
                    songs.Add((SongItem)item);
                    break;
            }
            await NotifyChange();
            //Play
        }

        private async Task SaveNowPlayingInDB()
        {
            await DatabaseManager.Current.InsertNewNowPlayingPlaylistAsync(songs);
        }

        private async Task NotifyChange()
        {
            OnNPChanged();
            await SaveNowPlayingInDB();
            SendMessage(AppConstants.NowPlayingListChanged);
        }

        private bool IsMyBackgroundTaskRunning
        {
            get
            {
                object value = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.BackgroundTaskState);
                if (value == null)
                {
                    return false;
                }
                else
                {
                    bool a = ((String)value).Equals(AppConstants.BackgroundTaskRunning);
                    return a;
                }
            }
        }

        private void SendMessage(string message)
        {
            if (IsMyBackgroundTaskRunning)
            {
                var value = new ValueSet();
                value.Add(message, "");
                BackgroundMediaPlayer.SendMessageToBackground(value);
            }
        }
    }
}
