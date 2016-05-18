using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NextPlayerUWP.Common
{
    public class AlbumArtFinder
    {
        private static bool isRunning = false;

        private ObservableCollection<AlbumItem> albums = new ObservableCollection<AlbumItem>();

        public AlbumArtFinder()
        {
            MediaImport.MediaImported += MediaImport_MediaImported;
        }

        private void MediaImport_MediaImported(string s)
        {
            StartLooking();
        }

        public async void StartLooking()
        {
            System.Diagnostics.Debug.WriteLine("start StartLooking");
            if (isRunning) return;
            await Template10.Common.DispatcherWrapper.Current().DispatchAsync(async () => 
            {
                albums = await DatabaseManager.Current.GetAlbumItemsAsync();
            });
            isRunning = true;
            await Task.Run(() => FindAlbumArts());
            isRunning = false;
            System.Diagnostics.Debug.WriteLine("finish StartLooking");
        }

        private async Task FindAlbumArts()
        {
            foreach (AlbumItem album in albums)
            {
                if (!album.IsImageSet)
                {
                    await Template10.Common.DispatcherWrapper.Current().DispatchAsync(async () => 
                    {
                        string path = await ImagesManager.GetAlbumCoverPath(album);
                        album.ImagePath = path;
                        album.ImageUri = new Uri(path);
                        album.IsImageSet = true;
                    });
                    await DatabaseManager.Current.UpdateAlbumImagePath(album).ConfigureAwait(false);
                }
            }
        }
    }
}
