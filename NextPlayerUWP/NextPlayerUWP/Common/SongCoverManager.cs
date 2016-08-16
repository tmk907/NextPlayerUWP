﻿using NextPlayerUWPDataLayer.Constants;
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
        private const int cacheCapacity = 50;

        private string coverPath = "";

        private Dictionary<int, Uri> cachedUris;

        private const string basePath = "ms-appdata:///local/CachedCovers/";

        public const string DefaultCover = AppConstants.SongCoverBig;

        private bool initialized;

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
            System.Diagnostics.Debug.WriteLine("SongCoverManager()");
            cachedUris = new Dictionary<int, Uri>(cacheCapacity);
            PlaybackService.MediaPlayerTrackChanged += PlaybackService_MediaPlayerTrackChanged;
            PlaybackService.StreamUpdated += PlaybackService_StreamUpdated;
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
            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            if (!song.IsAlbumArtSet)
            {
                Uri uri = await PrepareCover(song);
                OnCoverUriPrepared(uri);
            }
            initialized = true;
            System.Diagnostics.Debug.WriteLine("SCM Initialize end");
        }

        private void PlaybackService_StreamUpdated(NowPlayingSong song)
        {
            Uri uri = PrepareCoverUri(song.ImagePath);
            int id = song.SongId * 10 + (int)song.SourceType;
            AddUriToCachedUri(id, uri);
            OnCoverUriPrepared(uri);
        }

        private async void PlaybackService_MediaPlayerTrackChanged(int index)
        {
            var song = NowPlayingPlaylistManager.Current.GetSongItem(index);
            if (!song.IsAlbumArtSet)
            {
                var uri = await PrepareCover(song);
                OnCoverUriPrepared(uri);
            }
        }

        public Uri GetCurrent()
        {
            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            if (song.IsAlbumArtSet)
            {
                return song.AlbumArtUri;
            }
            int id = song.SongId * 10 + (int)song.SourceType;
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
            int id = song.SongId * 10 + (int)song.SourceType;

            if (cachedUris.ContainsKey(id))
            {
                //System.Diagnostics.Debug.WriteLine(ticks + " 2 Prepare cover id=" + songId + " count=" + cachedUris.Keys.Count);
                return cachedUris[id];
            }

            Uri newUri;
            if (song.SourceType == NextPlayerUWPDataLayer.Enums.MusicSource.LocalFile)
            {
                newUri = await CopyCoverFromSongFileToCacheFolder(song.Path, id);               
            }
            else if(song.SourceType == NextPlayerUWPDataLayer.Enums.MusicSource.RadioJamendo)
            {
                newUri = PrepareCoverUri(song.CoverPath);
            }
            else
            {
                newUri = new Uri(DefaultCover);
            }

            AddUriToCachedUri(id, newUri);

            return newUri;
        }

        private void AddUriToCachedUri(int id, Uri newUri)
        {
            if (cachedUris.Count == cacheCapacity)
            {
                for (int i = 0; i < 5; i++)
                {
                    int key = cachedUris.FirstOrDefault().Key;
                    //delete from disk
                    cachedUris.Remove(key);
                }
            }
            if (cachedUris.ContainsKey(id))
            {
                cachedUris[id] = newUri;
            }
            else
            {
                cachedUris.Add(id, newUri);
            }
        }

        private async Task<Uri> CopyCoverFromSongFileToCacheFolder(string path, int id)
        {
            var image = await ImagesManager.GetAlbumArtBitmap(path);
            if (image == null)
            {
                coverPath = DefaultCover;
            }
            else
            {
                coverPath = await ImagesManager.SaveCover("cover" + id.ToString(), "CachedCovers", image);
            }

            return new Uri(coverPath);
        }

        private Uri PrepareCoverUri(string coverPath)
        {
            Uri newUri;
            try
            {
                if (!String.IsNullOrEmpty(coverPath))
                {
                    newUri = new Uri(coverPath);
                }
                else
                {
                    newUri = new Uri(DefaultCover);
                }
            }
            catch
            {
                newUri = new Uri(DefaultCover);
            }
            return newUri;
        }

        private async Task DeleteAllCached()
        {
            await ApplicationData.Current.LocalFolder.CreateFolderAsync("CachedCovers", CreationCollisionOption.ReplaceExisting);
        }
    }
}
