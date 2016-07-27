﻿using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using NextPlayerUWPDataLayer.Constants;
using System.Diagnostics;
using System.Collections.Generic;
using NextPlayerUWPDataLayer.Tables;
using NextPlayerUWPDataLayer.Model;

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
                songs = await DatabaseManager.Current.GetSongsWithoutAlbumArtAsync();
            });
            isRunning = true;
            await Task.Run(() => FindSongsAlbumArt());
            songs.Clear();
            await Task.Run(() => UpdateAlbumsWithNoArt());
            albums.Clear();
            isRunning = false;

            Logger.DebugWrite("AlbumArtFinder", "StartLooking finished");
        }

        private async Task FindSongsAlbumArt()
        {
            //Logger.DebugWrite("AlbumArtFinder", "FindSongsAlbumArt start");
            //Stopwatch st = new Stopwatch();
            //st.Start();

            foreach (var group in songs.GroupBy(s => new { s.Album, s.AlbumArtist }))
            {
                var album = albums.FirstOrDefault(a => a.Album.Equals(group.Key.Album) && a.AlbumArtist.Equals(group.Key.AlbumArtist));
                await UpdateAlbum(group, album);
                if (album != null)
                {
                    OnAlbumArtUpdated(album.AlbumId, album.ImagePath);
                }
            }

            //st.Stop();
            //Debug.WriteLine("FindSongsAlbumArt {0}ms", st.ElapsedMilliseconds);
            //Logger.DebugWrite("AlbumArtFinder", "FindSongsAlbumArt end");
        }

        private async Task UpdateAlbumsWithNoArt()
        {
            await Template10.Common.DispatcherWrapper.Current().DispatchAsync(async () =>
            {
                albums = await DatabaseManager.Current.GetAlbumsTable();
            });
            if (albums.Exists(a => a.ImagePath == ""))
            {
                var songs = await DatabaseManager.Current.GetSongItemsAsync();
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
            await Template10.Common.DispatcherWrapper.Current().DispatchAsync(async () =>
            {
                await DatabaseManager.Current.UpdateAlbumsTable(albums);
                MediaImport.OnMediaImported("AlbumsArt");
            });
        }

        private static async Task UpdateAlbum(IEnumerable<SongsTable> group, AlbumsTable album)
        {
            string path = AppConstants.AlbumCover;
            Dictionary<string, string> hashes = new Dictionary<string, string>();
            foreach (var song in group)
            {
                await Template10.Common.DispatcherWrapper.Current().DispatchAsync(async () =>
                {
                    var songAlbumArt = await ImagesManager.GetAlbumArtBitmap(song.Path, true);
                    if (songAlbumArt == null)
                    {
                        song.AlbumArt = AppConstants.AlbumCover;
                    }
                    else
                    {
                        var hash = ImagesManager.GetHash(songAlbumArt);
                        if (hashes.ContainsKey(hash))
                        {
                            song.AlbumArt = hashes[hash];
                        }
                        else
                        {
                            string savedPath = await ImagesManager.SaveCover(song.SongId.ToString(), "Songs", songAlbumArt);
                            song.AlbumArt = savedPath;
                            hashes.Add(hash, savedPath);
                        }
                    }
                    songAlbumArt = null;
                });

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
            await Template10.Common.DispatcherWrapper.Current().DispatchAsync(async () =>
            {
                await DatabaseManager.Current.UpdateSongsImagePath(group);
                await DatabaseManager.Current.UpdateAlbumTableItemAsync(album);
            });
        }

        private static async Task UpdateAlbum(IEnumerable<SongsTable> group, AlbumItem album)
        {
            string path = AppConstants.AlbumCover;
            Dictionary<string, string> hashes = new Dictionary<string, string>();
            foreach (var song in group)
            {
                await Template10.Common.DispatcherWrapper.Current().DispatchAsync(async () =>
                {
                    var songAlbumArt = await ImagesManager.GetAlbumArtBitmap(song.Path, true);
                    if (songAlbumArt == null)
                    {
                        song.AlbumArt = AppConstants.AlbumCover;
                    }
                    else
                    {
                        var hash = ImagesManager.GetHash(songAlbumArt);
                        if (hashes.ContainsKey(hash))
                        {
                            song.AlbumArt = hashes[hash];
                        }
                        else
                        {
                            string savedPath = await ImagesManager.SaveCover(song.SongId.ToString(), "Songs", songAlbumArt);
                            song.AlbumArt = savedPath;
                            hashes.Add(hash, savedPath);
                        }
                    }
                });

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
            await Template10.Common.DispatcherWrapper.Current().DispatchAsync(async () =>
            {
                await DatabaseManager.Current.UpdateSongsImagePath(group);
                await DatabaseManager.Current.UpdateAlbumImagePath(album);
            });
        }

        public static async Task UpdateAlbumArt(AlbumItem album)
        {
            if (!album.IsImageSet)
            {
                var songs = await DatabaseManager.Current.GetSongsFromAlbumItemAsync(album);
                var song = songs.FirstOrDefault(s => s.AlbumArt != AppConstants.AlbumCover && s.AlbumArt != "");
                if (song == null)
                {
                    await UpdateAlbum(songs, album);
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
    }
}
