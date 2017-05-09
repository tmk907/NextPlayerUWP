﻿using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWP.Common
{
    public class PlayerInitializer
    {
        private bool isInitialized = false;
        private bool initializationStarted = false;
        public async Task Init()
        {
            if (isInitialized) return;
            if (!isInitialized && initializationStarted) throw new System.Exception("PlayerInitializer failed");
            System.Diagnostics.Debug.WriteLine("PlayerInitializer.Init() Start");
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();
            initializationStarted = true;

            await NowPlayingPlaylistManager.Current.Init();
            await PlaybackService.Instance.Initialize();

            App.AlbumArtFinder.StartLooking().ConfigureAwait(false);
            s.Stop();
            System.Diagnostics.Debug.WriteLine("PlayerInitializer.Init() End {0}ms", s.ElapsedMilliseconds);
            isInitialized = true;
        }

        public async Task Init(IReadOnlyList<IStorageItem> files)
        {
            if (!isInitialized && initializationStarted) throw new System.Exception("PlayerInitializer failed");
            System.Diagnostics.Debug.WriteLine("PlayerInitializer.Init() Start");
            System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            s.Start();
            initializationStarted = true;

            var musicItem = await GetFirstItem(files);

            if (musicItem == null)
            {
                if (isInitialized)
                {
                    
                }
                else
                {
                    await NowPlayingPlaylistManager.Current.Init();
                    await PlaybackService.Instance.Initialize();
                }
            }
            else
            {
                if (isInitialized)
                {
                    await NowPlayingPlaylistManager.Current.NewPlaylist(musicItem);
                }
                else
                {
                    await NowPlayingPlaylistManager.Current.Init(musicItem);
                }
                await PlaybackService.Instance.PlayNewList(0);
                s.Stop();
                System.Diagnostics.Debug.WriteLine("PlayerInitializer.Init() 2 {0}ms", s.ElapsedMilliseconds);
                await OpenFilesAndAddToNowPlaying(files);
                s.Start();
            }

            if (!isInitialized)
            {
                App.AlbumArtFinder.StartLooking().ConfigureAwait(false);
            }
            s.Stop();
            System.Diagnostics.Debug.WriteLine("PlayerInitializer.Init() End {0}ms", s.ElapsedMilliseconds);
            isInitialized = true;
        }

        private async Task<MusicItem> GetFirstItem(IReadOnlyList<IStorageItem> files)
        {
            MediaImport mi = new MediaImport(App.FileFormatsHelper);
            MusicItem musicItem = null;
            foreach(var file in files.OfType<StorageFile>())
            {
                string type = file.FileType.ToLower();

                if (App.FileFormatsHelper.IsFormatSupported(type))
                {
                    musicItem = await mi.OpenSingleFileAsync(file);
                }
                else if (App.FileFormatsHelper.IsPlaylistSupportedType(type))
                {
                    musicItem = await mi.OpenPlaylistFileAsync(file);
                }
                if (musicItem != null) break;
            }
            return musicItem;
        }

        private async Task OpenFilesAndAddToNowPlaying(IReadOnlyList<IStorageItem> files)
        {
            MediaImport mi = new MediaImport(App.FileFormatsHelper);
            List<SongItem> list = new List<SongItem>();
            int i = 0;
            const int size = 4;
            foreach (var file in files.Skip(1))
            {
                var si = await mi.OpenSingleFileAsync(file as StorageFile);
                list.Add(si);
                if (i == size)
                {
                    await NowPlayingPlaylistManager.Current.Add(list);
                    list.Clear();
                    i = 0;
                }
                else
                {
                    i++;
                }
            }
            if (list.Count > 0)
            {
                await NowPlayingPlaylistManager.Current.Add(list);
            }
        }
    }
}
