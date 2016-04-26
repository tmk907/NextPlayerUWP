using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            DeleteAllCached();
        }

        private async void PlaybackManager_MediaPlayerTrackChanged(int index)
        {
            var song = NowPlayingPlaylistManager.Current.GetSongItem(index);
            var newUri = await PrepareCover(song);
            OnCoverUriPrepared(newUri);
        }

        private const int cacheCapacity = 40;
        private string coverPath = "";

        private Dictionary<int, Uri> cachedUris;

        private string currentSongPath = "";

        private const string basePath = "ms-appdata:///local/CachedCovers/";

        public async Task<Uri> PrepareCover(SongItem song)
        {
            if (cachedUris.ContainsKey(song.SongId))
            {
                return cachedUris[song.SongId];
            }
            currentSongPath = song.Path;
            var image = await ImagesManager.CreateBitmap(currentSongPath);
            if (image.PixelWidth == 1)
            {
                coverPath = AppConstants.SongCoverBig;
            }
            else
            {
                coverPath = await ImagesManager.SaveCover("cover" + song.SongId.ToString(), "CachedCovers", image);
            }

            Uri newUri = new Uri(coverPath);

            if (cachedUris.Count == cacheCapacity)
            {
                int key = cachedUris.FirstOrDefault().Key;
                //delete from disk
                cachedUris.Remove(key);
            }
            cachedUris.Add(song.SongId, newUri);

            return newUri;
        }

        private async void DeleteAllCached()
        {
            await ApplicationData.Current.LocalFolder.CreateFolderAsync("CachedCovers", CreationCollisionOption.ReplaceExisting);
        }
    }
}
