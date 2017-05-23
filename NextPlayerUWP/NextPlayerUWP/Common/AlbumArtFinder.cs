using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Constants;
using System.Diagnostics;
using System.Collections.Generic;
using NextPlayerUWPDataLayer.Tables;
using NextPlayerUWPDataLayer.Model;
using System.Threading;

namespace NextPlayerUWP.Common
{
    public delegate void AlbumArtUpdatedHandler(int albumId, string albumArtPath);

    public class AlbumArtFinder
    {
        public static event AlbumArtUpdatedHandler AlbumArtUpdatedEvent;
        public static void OnAlbumArtUpdated(int albumId, string albumArtPath)
        {
            AlbumArtUpdatedEvent?.Invoke(albumId, albumArtPath);
        }

        private static bool isRunning = false;

        private List<AlbumsTable> albums = new List<AlbumsTable>();
        private List<SongsTable> songs = new List<SongsTable>();

        private CancellationTokenSource cts;

        public AlbumArtFinder()
        {
            MediaImport.MediaImported += MediaImport_MediaImported;
            cts = new CancellationTokenSource();
        }

        private async void MediaImport_MediaImported(string s)
        {
            await StartLooking();
        }

        public async Task StartLooking()
        {
            Logger2.DebugWrite("AlbumArtFinder", "StartLooking");

            if (isRunning) return;
            albums = await DatabaseManager.Current.GetAlbumsTable();
            songs = await DatabaseManager.Current.GetSongsWithoutAlbumArtAsync();
            if (songs.Count == 0) return;
            isRunning = true;
            cts = new CancellationTokenSource();
            try
            {
                await FindAlbumArtOfEverySong(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                songs.Clear();
                albums.Clear();
                isRunning = false;
                return;
            }
            //await Task.Run(() => FindAlbumArtOfEverySong());
            songs.Clear();
            await UpdateAlbumsWithNoArt().ConfigureAwait(false);
            //await Task.Run(() => UpdateAlbumsWithNoArt());
            Template10.Common.DispatcherWrapper.Current().Dispatch(() =>
            {
                MediaImport.OnMediaImported("AlbumsArt");
            });
            albums.Clear();
            isRunning = false;

            Logger2.DebugWrite("AlbumArtFinder", "StartLooking finished");
        }
        
        /// <summary>
        /// return true if task was running
        /// </summary>
        /// <returns></returns>
        public bool Cancel()
        {
            bool a = isRunning;
            cts.Cancel();
            cts = new CancellationTokenSource();
            return a;
        }

        private async Task FindAlbumArtOfEverySong(CancellationToken token)
        {
            Debug.WriteLine("AlbumArtFinder FindAlbumArtOfEverySong()");

            //First find album arts of albums, where Album or AlbumArtist is known. These album arts are shown first in AlbumsView list.
            foreach (var group in songs.Where(a => !(a.Album == "" && a.AlbumArtist == "")).OrderBy(a => a.Album).GroupBy(s => new { s.Album, s.AlbumArtist }))
            {
                var album = albums.FirstOrDefault(a => a.Album.Equals(group.Key.Album) && a.AlbumArtist.Equals(group.Key.AlbumArtist));
                await UpdateAlbumUsingSoftwareBitmap(group, album).ConfigureAwait(false);
                if (album != null)
                {
                    OnAlbumArtUpdated(album.AlbumId, album.ImagePath);
                }
                token.ThrowIfCancellationRequested();
            }
            var groupUnknown = songs.Where(a => a.Album == "" && a.AlbumArtist == "").GroupBy(s => new { s.Album, s.AlbumArtist }).FirstOrDefault();
            if (groupUnknown != null)
            {
                var albumUnknown = albums.FirstOrDefault(a => a.Album.Equals("") && a.AlbumArtist.Equals(""));
                await UpdateAlbumUsingSoftwareBitmap(groupUnknown, albumUnknown).ConfigureAwait(false);
                if (albumUnknown != null)
                {
                    OnAlbumArtUpdated(albumUnknown.AlbumId, albumUnknown.ImagePath);
                }
                token.ThrowIfCancellationRequested();
            }
        }

        private static async Task UpdateAlbumUsingSoftwareBitmap(IEnumerable<SongsTable> group, AlbumsTable album)
        {
            //Debug.WriteLine("AlbumArtFinder UpdateAlbum2() AlbumsTable: {0} {1}", album.Album, album.AlbumArtist);
            string path = AppConstants.AlbumCover;
            foreach (var song in group)
            {
                var songAlbumArt = await ImagesManager.GetAlbumArtSoftwareBitmap(song.Path, true).ConfigureAwait(false);
                if (songAlbumArt == null)
                {
                    song.AlbumArt = AppConstants.AlbumCover;
                }
                else
                {
                    string savedPath = await ImagesManager.SaveCover(song.SongId.ToString(), "Songs", songAlbumArt).ConfigureAwait(false);
                    song.AlbumArt = savedPath;
                }
                songAlbumArt = null;

                if (song.AlbumArt != AppConstants.AlbumCover)
                {
                    path = song.AlbumArt;
                }
            }
            if (album != null)
            {
                if (album.Album == "")
                {
                    album.ImagePath = AppConstants.AlbumCover;
                }
                else
                {
                    if (album.ImagePath == "" || album.ImagePath == AppConstants.AlbumCover || album.ImagePath.Contains("Albums"))
                    {
                        album.ImagePath = path;
                    }
                }
            }
            await DatabaseManager.Current.UpdateSongsImagePath(group).ConfigureAwait(false);
            await DatabaseManager.Current.UpdateAlbumTableItemAsync(album).ConfigureAwait(false);
        }

        private async Task UpdateAlbumsWithNoArt()
        {
            Debug.WriteLine("AlbumArtFinder UpdateAlbumsWithNoArt()");
            albums = await DatabaseManager.Current.GetAlbumsTable();
            if (albums.Exists(a => a.ImagePath == ""))
            {
                var songs = await DatabaseManager.Current.GetAllSongItemsAsync();
                var groups = songs.GroupBy(s => new { s.Album, s.AlbumArtist });
                foreach (var album in albums.Where(a => a.ImagePath == ""))
                {
                    if (album.Album == "") album.ImagePath = AppConstants.AlbumCover;
                    else
                    {
                        foreach (var song in groups.FirstOrDefault(g => g.Key.Album == album.Album && g.Key.AlbumArtist == album.AlbumArtist))
                        {
                            if (song.CoverPath != AppConstants.AlbumCover)
                            {
                                album.ImagePath = song.CoverPath;
                                break;
                            }
                        }
                        if (album.ImagePath == "") album.ImagePath = AppConstants.AlbumCover;
                    }
                }
            }
            await DatabaseManager.Current.UpdateAlbumsTable(albums);
        }

        public static async Task UpdateSingleAlbumArt(AlbumItem album)
        {
            Debug.WriteLine("AlbumArtFinder UpdateAlbumArt() {0} {1}", album.AlbumId, album.Album);
            if (!album.IsImageSet)
            {
                var songs = await DatabaseManager.Current.GetSongsFromAlbumItemAsync(album);
                var song = songs.FirstOrDefault(s => s.AlbumArt != AppConstants.AlbumCover && s.AlbumArt != "");
                if (song == null)
                {
                    await UpdateSingleAlbumUsingSoftwareBitmap(songs, album);
                }
                else
                {
                    album.ImagePath = song.AlbumArt;
                }
                OnAlbumArtUpdated(album.AlbumId, album.ImagePath);
            }
            else
            {
                album.ImagePath = album.ImagePath;
            }

            album.ImageUri = new Uri(album.ImagePath);
        }

        private static async Task UpdateSingleAlbumUsingSoftwareBitmap(IEnumerable<SongsTable> group, AlbumItem album)
        {
            Debug.WriteLine("AlbumArtFinder UpdateAlbum() AlbumItem: {0} {1}", album.Album, album.AlbumArtist);
            string path = AppConstants.AlbumCover;
            foreach (var song in group)
            {
                var songAlbumArt = await ImagesManager.GetAlbumArtSoftwareBitmap(song.Path, true);
                if (songAlbumArt == null)
                {
                    song.AlbumArt = AppConstants.AlbumCover;
                }
                else
                {
                    string savedPath = await ImagesManager.SaveCover(song.SongId.ToString(), "Songs", songAlbumArt);
                    song.AlbumArt = savedPath;
                }

                if (song.AlbumArt != AppConstants.AlbumCover)
                {
                    path = song.AlbumArt;
                }
            }
            if (album != null)
            {
                if (album.Album == "")
                {
                    album.ImagePath = AppConstants.AlbumCover;
                }
                else
                {
                    if (album.ImagePath == "" || album.ImagePath == AppConstants.AlbumCover || album.ImagePath.Contains("Albums"))
                    {
                        album.ImagePath = path;
                    }
                }
            }
            await DatabaseManager.Current.UpdateSongsImagePath(group);
            await DatabaseManager.Current.UpdateAlbumImagePath(album);
        }
    }
}
