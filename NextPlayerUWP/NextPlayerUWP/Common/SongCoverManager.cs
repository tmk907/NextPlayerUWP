using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWP.Common
{
    public delegate void CoverUriPreparedHandler(Uri newUri);

    public sealed class SongCoverManager
    {
        private Dictionary<int, Uri> cachedUris;
        private const string basePath = "ms-appdata:///local/CachedCovers/";
        public const string DefaultCover = AppConstants.SongCoverBig;
        private bool initialized;

        //public static event CoverUriPreparedHandler CoverUriPrepared;
        //public void OnCoverUriPrepared(Uri newUri)
        //{
        //    CoverUriPrepared?.Invoke(newUri);
        //}

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
            System.Diagnostics.Debug.WriteLine("SongCoverManager()");
            cachedUris = new Dictionary<int, Uri>(2);
            PlaybackService.MediaPlayerTrackChanged += PlaybackService_MediaPlayerTrackChanged;
            initialized = false;
        }

        public async Task Initialize(bool terminated = false)
        {
            System.Diagnostics.Debug.WriteLine("SCM Initialize start");
            if (initialized) return;
            if (!terminated)
            {
                await DeleteAllCached();
            }
            initialized = true;
            System.Diagnostics.Debug.WriteLine("SCM Initialize end");
        }
        private void PlaybackService_MediaPlayerTrackChanged(int index)
        {
            var song = NowPlayingPlaylistManager.Current.GetSongItem(index);
            if (!song.IsAlbumArtSet)
            {
                var uri = PrepareCover(song);
                //OnCoverUriPrepared(uri);
            }
        }

        public Uri GetCurrent()
        {
            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            if (song.IsAlbumArtSet)
            {
                return song.AlbumArtUri;
            }
            else
            {
                return new Uri(DefaultCover);
            }
        }

        private Uri PrepareCover(SongItem song)
        {
            Uri newUri = new Uri(DefaultCover);
            return newUri;
        }

        private async Task DeleteAllCached()
        {
            await ApplicationData.Current.LocalFolder.CreateFolderAsync("CachedCovers", CreationCollisionOption.ReplaceExisting);
        }
    }
}
