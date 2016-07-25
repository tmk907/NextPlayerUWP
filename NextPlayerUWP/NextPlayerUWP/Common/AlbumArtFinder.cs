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
using Windows.UI.Xaml.Media.Imaging;
using NextPlayerUWPDataLayer.Tables;

namespace NextPlayerUWP.Common
{
    public delegate void AlbumArtUpdatedHandler(int albumId, string albumArtPath);

    public class AlbumArtFinder
    {
        public static event AlbumArtUpdatedHandler AlbumArtUpdatedEvent;
        public void OnAlbumArtUpdated(int albumId, string albumArtPath)
        {
            AlbumArtUpdatedEvent?.Invoke(albumId, albumArtPath);
        }

        private static bool isRunning = false;

        private List<AlbumsTable> albums = new List<AlbumsTable>();
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
                albums = await DatabaseManager.Current.GetAlbumsTable();
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
            List<Tuple<int, string>> data = new List<Tuple<int, string>>();
            Stopwatch st = new Stopwatch();
            st.Start();
            foreach (var group in songs.Where(s => !s.IsAlbumArtSet).GroupBy(s => new { s.Album, s.AlbumArtist }))
            {
                path = AppConstants.AlbumCover;
                Dictionary<string, string> hashes = new Dictionary<string, string>();
                foreach (var song in group)
                {
                    i++;
                    await Template10.Common.DispatcherWrapper.Current().DispatchAsync(async () =>
                    {
                        var songAlbumArt = await ImagesManager.GetAlbumArtBitmap2(song.Path, true);
                        if (songAlbumArt==null || songAlbumArt.PixelHeight == 1)
                        {
                            song.CoverPath = AppConstants.AlbumCover;
                        }
                        else
                        {
                            var hash = ImagesManager.GetHash(songAlbumArt);
                            if (hashes.ContainsKey(hash))
                            {
                                song.CoverPath = hashes[hash];
                            }
                            else
                            {
                                string savedPath = await ImagesManager.SaveCover(song.SongId.ToString(), "Songs", songAlbumArt);
                                song.CoverPath = savedPath;
                                hashes.Add(hash,savedPath);
                            }
                        }
                        song.IsAlbumArtSet = true;
                    });
                    data.Add(new Tuple<int, string>(song.SongId, song.CoverPath));
                    
                    if (song.CoverPath != AppConstants.AlbumCover)
                    {
                        path = song.CoverPath;
                    }
                }
                var album = albums.FirstOrDefault(a => a.Album.Equals(group.Key.Album) && a.AlbumArtist.Equals(group.Key.AlbumArtist));
                if (album != null)
                {
                    if (album.Album == "")
                    {
                        album.ImagePath = AppConstants.AlbumCover;
                    }
                    else
                    {
                        album.ImagePath = path;
                    }
                    OnAlbumArtUpdated(album.AlbumId, path);
                }
            }
            await Template10.Common.DispatcherWrapper.Current().DispatchAsync(async () =>
            {
                await DatabaseManager.Current.UpdateSongImagePath(songs);
                await DatabaseManager.Current.UpdateAlbumsImagePath(albums);
            });

            st.Stop();
            Debug.WriteLine("Songs {0} Total {1}ms", i, st.ElapsedMilliseconds);
            Logger.DebugWrite("AlbumArtFinder", "FindSongsAlbumArt end");
        }

        //private async Task UpdateAlbumArts()
        //{
        //    Logger.DebugWrite("AlbumArtFinder", "UpdateAlbumArts start");
        //    var albumsWithoutAlbumArt = albums.Where(a => !a.IsImageSet);

        //    var groups = songs.GroupBy(s => new { s.Album, s.AlbumArtist });

        //    foreach(var album in albumsWithoutAlbumArt)
        //    {
        //        var group = groups.FirstOrDefault(g => g.Key.Album == album.AlbumParam && g.Key.AlbumArtist == album.AlbumArtist);
        //        var path = group.FirstOrDefault(s => s.CoverPath != AppConstants.AlbumCover)?.CoverPath;
        //        if (!String.IsNullOrEmpty(path) && album.AlbumParam != "")
        //        {
        //            album.ImagePath = path;
        //        }
        //        else
        //        {
        //            album.ImagePath = AppConstants.AlbumCover;
        //        }
        //        await DatabaseManager.Current.UpdateAlbumImagePath(album);

        //    }
        //    Logger.DebugWrite("AlbumArtFinder", "UpdateAlbumArts end");
        //}
    }
}
