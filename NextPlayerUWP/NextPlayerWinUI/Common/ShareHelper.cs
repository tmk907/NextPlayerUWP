using NextPlayerWinUIDataLayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace NextPlayerWinUI.Common
{
    public class ShareHelper
    {
        private List<StorageFile> filesToShare = new List<StorageFile>();
        private string title = "";

        public async Task Share(MusicItem item)
        {
            filesToShare.Clear();
            if (item is SongItem song)
            {
                var file = await StorageFile.GetFileFromPathAsync(song.Path);
                filesToShare.Add(file);
                title = song.Title;
                ShowShareUI();
            }
        }

        public async Task Share(IEnumerable<MusicItem> items)
        {
            filesToShare.Clear();
            List<SongItem> songs = new List<SongItem>();

            if (items.OfType<SongItem>().Count() == items.Count())
            {
                songs = items.OfType<SongItem>().ToList();
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

            foreach (SongItem song in songs)
            {
                var file = await StorageFile.GetFileFromPathAsync(song.Path);
                filesToShare.Add(file);
            }
            var songTitle = songs.FirstOrDefault().Title;
            int max = 32;
            if (songs.Count() == 1)
            {
                title = songTitle;
            }
            else
            {
                if (songTitle.Length > max)
                {
                    songTitle = songTitle.Remove(max);
                    songTitle += "...";
                }
                title = $"{songTitle} and {songs.Count() - 1} more";
            }
            ShowShareUI();
        }

        private void ShowShareUI()
        {
            var manager = DataTransferManagerInterop.GetForWindow(WindowHelper.Handle);
            manager.DataRequested -= DataTransferManager_DataRequested;
            manager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManagerInterop.ShowShareUIForWindow(WindowHelper.Handle);
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = String.IsNullOrEmpty(title) ? "Next-Player" : title;
            request.Data.Properties.ApplicationName = "Next-Player";
            IEnumerable<IStorageItem> list = new List<IStorageItem>(filesToShare);
            request.Data.SetStorageItems(list);
        }
    }
}
