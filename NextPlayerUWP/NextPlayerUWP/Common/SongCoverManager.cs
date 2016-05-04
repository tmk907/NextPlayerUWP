using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWP.Common
{
    public delegate void CoverUriPreparedHandler(Uri newUri);

    public sealed class SongCoverManager
    {
        public static event CoverUriPreparedHandler CoverUriPrepared;
        public void OnCoverUriPrepared(Uri newUri)
        {
            CoverUriPrepared?.Invoke(newUri);
        }

        private static readonly SongCoverManager instance = new SongCoverManager();
        public static SongCoverManager Instance
        {
            get
            {
                return instance;
            }
        }
        static SongCoverManager() { }
        private SongCoverManager()
        {
            cachedUris = new Dictionary<int, Uri>(cacheCapacity);
            PlaybackManager.MediaPlayerTrackChanged += PlaybackManager_MediaPlayerTrackChanged;
            initialized = false;
        }

        private async void PlaybackManager_MediaPlayerTrackChanged(int index)
        {
            var song = NowPlayingPlaylistManager.Current.GetSongItem(index);
            var newUri = await PrepareCover(song);
            OnCoverUriPrepared(newUri);
        }

        private const int cacheCapacity = 50;
        private string coverPath = "";

        private Dictionary<int, Uri> cachedUris;

        private const string basePath = "ms-appdata:///local/CachedCovers/";

        public const string DefaultCover = AppConstants.SongCoverBig;

        private bool initialized;

        public async Task Initialize()
        {
            if (initialized) return;
            await DeleteAllCached();
            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            Uri uri = await SaveFromFileToCache(song.Path, song.SongId);
            cachedUris.Add(song.SongId, uri);
            initialized = true;
        }

        public Uri GetFirst()
        {
            if (cachedUris.Count > 0) return cachedUris.FirstOrDefault().Value;
            else return new Uri(DefaultCover);
        }

        public Uri GetCurrent()
        {
            int id = NowPlayingPlaylistManager.Current.GetCurrentPlaying().SongId;
            if (cachedUris.ContainsKey(id))
            {
                return cachedUris[id];
            }
            else
            {
                return new Uri(DefaultCover);
            }
        }

        private async Task<Uri> PrepareCover(SongItem song)
        {
            //var ticks = DateTime.Now.Ticks;
            //System.Diagnostics.Debug.WriteLine(ticks+" 1 Prepare cover id=" + song.SongId);
            int songId = song.SongId;
            if (cachedUris.ContainsKey(songId))
            {
                //System.Diagnostics.Debug.WriteLine(ticks + " 2 Prepare cover id=" + songId + " count=" + cachedUris.Keys.Count);
                return cachedUris[songId];
            }

            //var nextSong = NowPlayingPlaylistManager.Current.GetNextSong();

            Uri newUri = await SaveFromFileToCache(song.Path, song.SongId);

            if (cachedUris.Count == cacheCapacity)
            {
                for(int i = 0; i < 5; i++)
                {
                    int key = cachedUris.FirstOrDefault().Key;
                    //delete from disk
                    cachedUris.Remove(key);
                }
            }
            if (cachedUris.ContainsKey(songId))
            {
                //System.Diagnostics.Debug.WriteLine(ticks + " 3 Prepare cover id=" + songId + " count=" + cachedUris.Keys.Count);
                HockeyProxy.TrackEventException("Duplicate key SongCoverManager " + cachedUris[songId] + ", " + newUri);
                cachedUris[songId] = newUri;
            }
            else
            {
                //System.Diagnostics.Debug.WriteLine(ticks + " 4 Prepare cover id=" + songId + " count=" + cachedUris.Keys.Count);
                cachedUris.Add(songId, newUri);
            }

            return newUri;
        }

        private async Task<Uri> SaveFromFileToCache(string path, int id)
        {
            var image = await ImagesManager.CreateBitmap(path);
            if (image.PixelWidth == 1)
            {
                coverPath = DefaultCover;
            }
            else
            {
                coverPath = await ImagesManager.SaveCover("cover" + id.ToString(), "CachedCovers", image);
            }

            return new Uri(coverPath);
        }

        private async Task DeleteAllCached()
        {
            await ApplicationData.Current.LocalFolder.CreateFolderAsync("CachedCovers", CreationCollisionOption.ReplaceExisting);
        }
    }
}
