using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Constants;
using System.Diagnostics;
using System.Collections.Generic;

namespace NextPlayerUWP.Common
{
    public delegate void AlbumArtUpdatedHandler(string album, string albumArtPath);

    public class AlbumArtFinder
    {
        public static event AlbumArtUpdatedHandler AlbumArtUpdatedEvent;
        public void OnAlbumArtUpdated(string album, string albumArtPath)
        {
            AlbumArtUpdatedEvent?.Invoke(album, albumArtPath);
        }

        private static bool isRunning = false;

        private ObservableCollection<AlbumItem> albums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();

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
            Logger.DebugWrite("AlbumArtFinder", "StartLooking");
            if (isRunning) return;
            await Template10.Common.DispatcherWrapper.Current().DispatchAsync(async () => 
            {
                albums = await DatabaseManager.Current.GetAlbumItemsAsync();
                songs = await DatabaseManager.Current.GetSongItemsAsync();
            });
            isRunning = true;
            await Task.Run(() => FindSongsAlbumArt());
            songs.Clear();
            albums.Clear();
            isRunning = false;
            Logger.DebugWrite("AlbumArtFinder", "StartLooking finished");
        }

        private async Task FindSongsAlbumArt()
        {
            Logger.DebugWrite("AlbumArtFinder", "FindSongsAlbumArt start");
            string path = AppConstants.AlbumCover;
            int i = 0;
            //ImagesManager.i1 = 0;
            //ImagesManager.i2 = 0;
            //long ist2 = 0;
            List<Tuple<int, string>> data = new List<Tuple<int, string>>();
            Stopwatch st = new Stopwatch();
            st.Start();
            foreach (var group in songs.Where(s => !s.IsAlbumArtSet).GroupBy(s => s.Album))
            {
                path = AppConstants.AlbumCover;
                foreach (var song in group)
                {
                    i++;
                    await Template10.Common.DispatcherWrapper.Current().DispatchAsync(async () =>
                    {
                        await ImagesManager.PrepareSongAlbumArt(song);
                    });
                    data.Add(new Tuple<int, string>(song.SongId, song.CoverPath));
                    
                    if (song.CoverPath != AppConstants.AlbumCover)
                    {
                        path = song.CoverPath;
                    }
                }
                if (group.Key != "") OnAlbumArtUpdated(group.FirstOrDefault().Album, path);
                else
                {

                }
            }
           
            await DatabaseManager.Current.UpdateSongImagePath(songs).ConfigureAwait(false);
           
            st.Stop();
            Debug.WriteLine("Songs {0} Total {1}ms DB {2}ms", i, st.ElapsedMilliseconds, st2.ElapsedMilliseconds);
            //Debug.WriteLine("Read {0}ms Save {1}ms", ImagesManager.i1, ImagesManager.i2);
            //await UpdateAlbumArts();
            Logger.DebugWrite("AlbumArtFinder", "FindSongsAlbumArt end");
        }

        private async Task UpdateAlbumArts()
        {
            Logger.DebugWrite("AlbumArtFinder", "UpdateAlbumArts start");
            var albumsWithoutAlbumArt = albums.Where(a => !a.IsImageSet);

            var groups = songs.GroupBy(s => new { s.Album, s.AlbumArtist });

            foreach(var album in albumsWithoutAlbumArt)
            {
                var group = groups.FirstOrDefault(g => g.Key.Album == album.AlbumParam && g.Key.AlbumArtist == album.AlbumArtist);
                var path = group.FirstOrDefault(s => s.CoverPath != AppConstants.AlbumCover)?.CoverPath;
                if (!String.IsNullOrEmpty(path) && album.AlbumParam != "")
                {
                    album.ImagePath = path;
                }
                else
                {
                    album.ImagePath = AppConstants.AlbumCover;
                }
                await DatabaseManager.Current.UpdateAlbumImagePath(album);

            }
            Logger.DebugWrite("AlbumArtFinder", "UpdateAlbumArts end");
        }
    }
}
