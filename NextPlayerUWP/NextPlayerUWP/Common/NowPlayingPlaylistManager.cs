using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;

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
            songs = DatabaseManager.Current.GetSongItemsFromNowPlaying();
            PlaybackManager.MediaPlayerTrackChanged += PlaybackManager_MediaPlayerTrackChanged;
        }

        private void PlaybackManager_MediaPlayerTrackChanged(int index)
        {
            int i = 0;
            foreach(var song in songs)
            {
                if (i == index) song.IsPlaying = true;
                else
                {
                    song.IsPlaying = false;
                }
                i++;
            }
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
            //ObservableCollection<SongItem> newsongs = new ObservableCollection<SongItem>(songs.Take(index + 1));
            //foreach(var s in list)
            //{
            //    newsongs.Add(s);
            //}
            //foreach(var s in songs.Skip(index+1))
            //{
            //    newsongs.Add(s);
            //}
            //songs.Insert = newsongs;
            foreach(var n in list)
            {
                index++;
                songs.Insert(index, n);
            }
            await NotifyChange();
        }

        public async Task AddNext(IEnumerable<SongItem> songs)
        {
            //await SaveNowPlayingInDB();

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

        public async Task NewPlaylist(MusicItem item)
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
            songs.Clear();
            foreach(var song in list)
            {
                songs.Add(song);
            }
            await NotifyChange();
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
                    SendMessage(AppConstants.NowPlayingListRefresh);
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
            await NotifyChange();
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
            int index = ApplicationSettingsHelper.ReadSongIndex();
            if (index < 0 || index > songs.Count - 1) return new SongItem();
            return songs[index];
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
                    var state = EnumHelper.Parse<BackgroundTaskState>(value as string);
                    bool isRunning = state == BackgroundTaskState.Running;
                    return isRunning;
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
