using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using System.IO;
using NextPlayerUWPDataLayer.Constants;
using Windows.Storage;
using NextPlayerUWPDataLayer.Tables;
using NextPlayerUWPDataLayer.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.CloudStorage;
using NextPlayerUWPDataLayer.Diagnostics;

namespace NextPlayerUWPDataLayer.Services
{
    public sealed class DatabaseManager
    {
        private static readonly DatabaseManager current = new DatabaseManager();

        static DatabaseManager() { }

        public static DatabaseManager Current
        {
            get
            {
                return current;
            }
        }

        private DatabaseManager()
        {
            connectionAsync = new SQLiteAsyncConnection(DBFilePath, true);
            connection = new SQLiteConnection(DBFilePath, true);
            connection.BusyTimeout = TimeSpan.FromSeconds(10);
        }

        private SQLiteAsyncConnection connectionAsync;
        private SQLiteConnection connection;
        
        //public string OldDBFilePath { get { return Path.Combine(ApplicationData.Current.LocalFolder.Path, AppConstants.DBFileName); } }
        public string DBFilePath { get { return Path.Combine(ApplicationData.Current.LocalFolder.Path, AppConstants.DBFileName); } }
      
        private AsyncTableQuery<SongsTable> songsConnectionAsync
        {
            get
            {
                return connectionAsync.Table<SongsTable>().Where(available => available.IsAvailable > 0);
            }
        }

        public void CreateNewDatabase()
        {
            try
            {
                DeleteDatabase();
            }
            catch (Exception) { }
            connection.CreateTable<PlainPlaylistsTable>();
            connection.CreateTable<PlainPlaylistEntryTable>();
            connection.CreateTable<SmartPlaylistsTable>();
            connection.CreateTable<SmartPlaylistEntryTable>();
            connection.CreateTable<SongsTable>();
            connection.CreateTable<NowPlayingTable>();
            connection.CreateTable<FoldersTable>();
            connection.CreateTable<GenresTable>();
            connection.CreateTable<AlbumsTable>();
            connection.CreateTable<ArtistsTable>();
            connection.CreateTable<AlbumArtistsTable>();
            connection.CreateTable<CachedScrobble>();
            connection.CreateTable<FutureAccessTokensTable>();
            connection.CreateTable<CloudAccountsTable>();
            connection.CreateTable<FavouriteRadiosTable>();
        }

        public void DeleteDatabase()
        {
            connection.DropTable<SongsTable>();
            connection.DropTable<PlainPlaylistEntryTable>();
            connection.DropTable<PlainPlaylistsTable>();
            connection.DropTable<SmartPlaylistEntryTable>();
            connection.DropTable<SmartPlaylistsTable>();
            connection.DropTable<NowPlayingTable>();
            connection.DropTable<FoldersTable>();
            connection.DropTable<GenresTable>();
            connection.DropTable<AlbumsTable>();
            connection.DropTable<AlbumArtistsTable>();
            connection.DropTable<ArtistsTable>();
            connection.DropTable<CachedScrobble>();
            connection.DropTable<FutureAccessTokensTable>();
            connection.DropTable<CloudAccountsTable>();
            connection.DropTable<FavouriteRadiosTable>();
        }

        public async Task ChangeToNotAvaialble(List<string> availableDirectories)
        {
            var allFolders = await connectionAsync.Table<FoldersTable>().ToListAsync();
            List<FoldersTable> notAvailable = new List<FoldersTable>();
            List<FoldersTable> availableFolders = new List<FoldersTable>();
            foreach (var folder in allFolders)
            {
                if (availableDirectories.Contains(folder.Directory))
                {
                    availableFolders.Add(folder);
                }
                else
                {
                    notAvailable.Add(folder);
                }
            }
            //var availableFolders = allFolders.Where(f => availableDirectories.Contains(f.Directory)).ToList();
            connection.RunInTransaction(() => 
            {
                connection.DeleteAll<FoldersTable>();
                connection.InsertAll(availableFolders);
            });
            
            const int N = 512;
            string[] array = new string[N];               // Temporary array of N items.
            int i = 0;
            foreach (var folder in notAvailable)
            {         // Just one iterator.
                array[i++] = folder.Directory;              // Store a reference to this item.
                if (i == N)
                {                // When we have N items,
                    var query = await connectionAsync.Table<SongsTable>().Where(s => array.Contains(s.DirectoryName)).ToListAsync();
                    foreach (var song in query)
                    {
                        song.IsAvailable = 0;
                    }
                    await connectionAsync.UpdateAllAsync(query);
                    i = 0;                   // and reset the array index.
                }
            }

            // remaining items
            if (i > 0)
            {
                var array2 = array.Take(i);
                var query = await connectionAsync.Table<SongsTable>().Where(s => array.Contains(s.DirectoryName)).ToListAsync();
                foreach (var song in query)
                {
                    song.IsAvailable = 0;
                }
                await connectionAsync.UpdateAllAsync(query);
            }
        }

        public async Task UpdateFolderAsync(string directory, List<SongsTable> oldSongs, List<SongData> newSongs, IEnumerable<SongsTable> toNotAvailable, IEnumerable<SongsTable> changed)
        {
            //toNotAvailable i changed sa podzbiorami oldSongs
            //Debug.WriteLine("UpdateFolderAsync2 {0} {1} {2} {3}", newSongs.Count, oldSongs.Count, toNotAvailable.Count(), changed.Count());
            var folder = await connectionAsync.Table<FoldersTable>().Where(f => f.Directory.Equals(directory)).FirstOrDefaultAsync();
            if (newSongs.Count == 0 && oldSongs.Count == 0)
            {
                if (null != folder)
                {
                    await connectionAsync.DeleteAsync(folder);
                }
                return;
            }
            
            TimeSpan duration = TimeSpan.Zero;
            DateTime lastAdded = DateTime.MinValue;
            int songsNumber = 0;
            foreach (var song in oldSongs.Where( s=> s.IsAvailable == 1))
            {
                duration += song.Duration;
                if (lastAdded < song.DateAdded) lastAdded = song.DateAdded;
                songsNumber++;
            }
            foreach (var song in newSongs)
            {
                duration += song.Duration;
                if (lastAdded < song.DateAdded) lastAdded = song.DateAdded;
                songsNumber++;
            }

            if (null == folder)
            {
                await connectionAsync.InsertAsync(new FoldersTable()
                {
                    Directory = directory,
                    Duration = duration,
                    Folder = Path.GetFileName(directory),
                    LastAdded = lastAdded,
                    SongsNumber = songsNumber,
                });
            }
            else
            {
                if (newSongs.Count == 0 && !oldSongs.Exists(s => s.IsAvailable == 1))
                {
                    await connectionAsync.DeleteAsync(folder);
                }
                else
                {
                    if (folder.Duration != duration || folder.LastAdded != lastAdded || folder.SongsNumber != songsNumber)
                    {
                        folder.Duration = duration;
                        folder.LastAdded = lastAdded;
                        folder.SongsNumber = songsNumber;
                        await connectionAsync.UpdateAsync(folder);
                    }
                }
            }

            await connectionAsync.UpdateAllAsync(toNotAvailable);
            await connectionAsync.UpdateAllAsync(changed);

            List<SongsTable> list = new List<SongsTable>();
            foreach (var item in newSongs)
            {
                list.Add(CreateSongsTable(item));
            }
            await connectionAsync.InsertAllAsync(list);
        }

        //Albums, Artists, Genres
        public async Task UpdateTables()//Error null reference
        {
            var songsList = await connectionAsync.Table<SongsTable>().Where(available => available.IsAvailable > 0).ToListAsync();

            await connectionAsync.ExecuteAsync("UPDATE ArtistsTable SET SongsNumber = 0");
            await connectionAsync.ExecuteAsync("UPDATE AlbumsTable SET SongsNumber = 0");
            await connectionAsync.ExecuteAsync("UPDATE AlbumArtistsTable SET SongsNumber = 0");
            await connectionAsync.ExecuteAsync("UPDATE GenresTable SET SongsNumber = 0");

            #region artists
            List <SongsTable> asongs = new List<SongsTable>();
            foreach(var song in songsList)
            {
                if (song.Artists.Contains("; "))//error null reference
                {
                    string[] artists = song.Artists.Split(new string[] { "; " },StringSplitOptions.RemoveEmptyEntries);
                    foreach (var a in artists)
                    {
                        SongsTable tempSong = CreateCopy(song);
                        tempSong.Artists = a;
                        asongs.Add(tempSong);
                    }
                }
                else
                {
                    asongs.Add(song);
                }
            }
            List<ArtistsTable> newArtists = new List<ArtistsTable>();
            List<ArtistsTable> updatedArtists = new List<ArtistsTable>();
            var groupedArtists = asongs.GroupBy(a => a.Artists);
            var arList = await connectionAsync.Table<ArtistsTable>().ToListAsync();
            var oldArtists = arList.ToDictionary(l => l.Artist);
            foreach(var group in groupedArtists)
            {
                int albumsNumber = group.GroupBy(a => a.Album).Count();
                TimeSpan duration = TimeSpan.Zero;
                int songsNumber = 0;
                DateTime lastAdded = DateTime.MinValue;
                foreach(var song in group)
                {
                    if (lastAdded.Ticks < song.DateAdded.Ticks) lastAdded = song.DateAdded;
                    duration += song.Duration;
                    songsNumber++;
                }
                ArtistsTable oldArtist;
                if (oldArtists.TryGetValue(group.FirstOrDefault().Artists,out oldArtist))
                {
                    oldArtist.Duration = duration;
                    oldArtist.SongsNumber = songsNumber;
                    oldArtist.LastAdded = lastAdded;
                    updatedArtists.Add(oldArtist);
                    //await connectionAsync.ExecuteAsync("UPDATE ArtistsTable SET Duration = ?, SongsNumber = ?, LastAdded = ? WHERE ArtistId = ?", 
                    //    duration, songsNumber, lastAdded.Ticks, oldArtist.ArtistId);
                }
                else
                {
                    newArtists.Add(new ArtistsTable()
                    {
                        AlbumsNumber = albumsNumber,
                        Artist = group.FirstOrDefault().Artists,
                        Duration = duration,
                        LastAdded = lastAdded,
                        SongsNumber = songsNumber,
                    });
                }
            }
            await connectionAsync.InsertAllAsync(newArtists);
            await connectionAsync.UpdateAllAsync(updatedArtists);
            #endregion            
            #region albums
            List<AlbumsTable> newAlbums = new List<AlbumsTable>();
            List<AlbumsTable> updatedAlbums = new List<AlbumsTable>();
            var groupedAlbums = songsList.GroupBy(a => new { a.Album, a.AlbumArtist });
            var aList = await connectionAsync.Table<AlbumsTable>().ToListAsync();
            var oldAlbums = aList.ToDictionary(l=> Tuple.Create(l.Album,l.AlbumArtist));
            foreach (var group in groupedAlbums)
            {
                TimeSpan duration = TimeSpan.Zero;
                //string albumArtist = group.Key.AlbumArtist;
                int year = 0;
                int count = 0;
                DateTime lastAdded = DateTime.MinValue;
                foreach (var song in group)
                {
                    if (lastAdded.Ticks < song.DateAdded.Ticks) lastAdded = song.DateAdded;
                    duration += song.Duration;
                    count++;
                    if (song.Year != 0)
                    {
                        year = song.Year;
                    }
                    //if (song.AlbumArtist != "")
                    //{
                    //    albumArtist = song.AlbumArtist;
                    //}
                }
                AlbumsTable oldAlbum;
                if (oldAlbums.TryGetValue(Tuple.Create(group.Key.Album,group.Key.AlbumArtist), out oldAlbum))
                {
                    oldAlbum.Duration = duration;
                    oldAlbum.SongsNumber = count;
                    oldAlbum.LastAdded = lastAdded;
                    updatedAlbums.Add(oldAlbum);
                    //await connectionAsync.ExecuteAsync("UPDATE AlbumsTable SET Duration = ?, SongsNumber = ?, LastAdded = ? WHERE AlbumId = ?", 
                    //    duration, count, lastAdded.Ticks, oldAlbum.AlbumId);
                }
                else
                {
                    newAlbums.Add(new AlbumsTable()
                    {
                        Album = group.Key.Album,
                        AlbumArtist = group.Key.AlbumArtist,
                        Duration = duration,
                        ImagePath = "",
                        LastAdded = lastAdded,
                        SongsNumber = count,
                        Year = year
                    });
                }
            }
            await connectionAsync.InsertAllAsync(newAlbums);
            await connectionAsync.UpdateAllAsync(updatedAlbums);
            #endregion
            #region album artists
            List<SongsTable> songs1 = new List<SongsTable>();
            foreach (var song in songsList)
            {
                if (song.AlbumArtist.Contains("; "))//error null reference
                {
                    string[] albumArtists = song.AlbumArtist.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var a in albumArtists)
                    {
                        SongsTable tempSong = CreateCopy(song);
                        tempSong.AlbumArtist = a;
                        songs1.Add(tempSong);
                    }
                }
                else
                {
                    songs1.Add(song);
                }
            }
            var newAlbumArtists = new List<AlbumArtistsTable>();
            var updatedAlbumArtists = new List<AlbumArtistsTable>();
            var groupedAlbumArtists = songs1.GroupBy(a => new { a.AlbumArtist });
            var aaList = await connectionAsync.Table<AlbumArtistsTable>().ToListAsync();
            var oldAlbumArtists = aaList.ToDictionary(l => l.AlbumArtist);
            foreach (var group in groupedAlbumArtists)
            {
                TimeSpan duration = TimeSpan.Zero;
                int count = 0;
                int albumsCount = group.GroupBy(s => s.Album).Count();
                DateTime lastAdded = DateTime.MinValue;
                foreach (var song in group)
                {
                    if (lastAdded.Ticks < song.DateAdded.Ticks) lastAdded = song.DateAdded;
                    duration += song.Duration;
                    count++;
                }
                AlbumArtistsTable oldAlbumArtist;
                if (oldAlbumArtists.TryGetValue(group.Key.AlbumArtist, out oldAlbumArtist))
                {
                    oldAlbumArtist.Duration = duration;
                    oldAlbumArtist.SongsNumber = count;
                    oldAlbumArtist.LastAdded = lastAdded;
                    oldAlbumArtist.AlbumsNumber = albumsCount;
                    updatedAlbumArtists.Add(oldAlbumArtist);
                }
                else
                {
                    newAlbumArtists.Add(new AlbumArtistsTable()
                    {
                        AlbumArtist = group.Key.AlbumArtist,
                        Duration = duration,
                        LastAdded = lastAdded,
                        SongsNumber = count,
                        AlbumsNumber = albumsCount,
                    });
                }
            }
            await connectionAsync.InsertAllAsync(newAlbumArtists);
            await connectionAsync.UpdateAllAsync(updatedAlbumArtists);
            #endregion
            #region genres
            List<SongsTable> gsongs = new List<SongsTable>();
            foreach (var song in songsList)
            {
                if (song.Genres.Contains("; "))
                {
                    string[] genres = song.Genres.Split(new string[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var a in genres)
                    {
                        SongsTable tempSong = CreateCopy(song);
                        tempSong.Genres = a;
                        gsongs.Add(tempSong);
                    }
                }
                else
                {
                    gsongs.Add(song);
                }
            }
            List<GenresTable> newGenres = new List<GenresTable>();
            List<GenresTable> updatedGenres = new List<GenresTable>();
            var groupedGenres = gsongs.GroupBy(g => g.Genres);
            var gList = await connectionAsync.Table<GenresTable>().ToListAsync();
            var oldGenres = gList.ToDictionary(l => l.Genre);
            foreach (var group in groupedGenres)
            {
                TimeSpan duration = TimeSpan.Zero;
                int count = 0;
                DateTime lastAdded = DateTime.MinValue;
                foreach(var song in group)
                {
                    if (lastAdded.Ticks < song.DateAdded.Ticks) lastAdded = song.DateAdded;
                    duration += song.Duration;
                    count++;
                }
                GenresTable oldGenre;
                if (oldGenres.TryGetValue(group.FirstOrDefault().Genres, out oldGenre))
                {
                    oldGenre.Duration = duration;
                    oldGenre.SongsNumber = count;
                    oldGenre.LastAdded = lastAdded;
                    updatedGenres.Add(oldGenre);
                    //await connectionAsync.ExecuteAsync("UPDATE GenresTable SET Duration = ?, SongsNumber = ?, LastAdded = ? WHERE AlbumId = ?", 
                    //    duration, count, lastAdded.Ticks, oldGenre.GenreId);
                }
                else
                {
                    newGenres.Add(new GenresTable()
                    {
                        Duration = duration,
                        Genre = group.FirstOrDefault().Genres,
                        LastAdded = lastAdded,
                        SongsNumber = count,
                    });
                }
            }
            await connectionAsync.InsertAllAsync(newGenres);
            await connectionAsync.UpdateAllAsync(updatedGenres);
            #endregion
            await ClearEntryWithNoSongs();
        }

        private async Task ClearEntryWithNoSongs()// error DoSavePointExecute
        {
            //przed update tables ustawic songscount na 0 w tabelach album, artist, genre
            //nowe wiersze beda mialy songcount>0, updatowane tez
            //wiersze stare nie updatowane beda mialy songscount == 0
            //przeszukac album, artist, genre z songscount==0 i usunac
            var albums = await connectionAsync.Table<AlbumsTable>().Where(a => a.SongsNumber > 0).ToListAsync();
            var artists = await connectionAsync.Table<ArtistsTable>().Where(a => a.SongsNumber > 0).ToListAsync();
            var albumArtists = await connectionAsync.Table<AlbumArtistsTable>().Where(a => a.SongsNumber > 0).ToListAsync();
            var genres = await connectionAsync.Table<GenresTable>().Where(g => g.SongsNumber > 0).ToListAsync();
            connection.RunInTransaction(() =>
            {
                connection.DeleteAll<AlbumsTable>();
                connection.InsertAll(albums);
            });
            connection.RunInTransaction(() =>
            {
                connection.DeleteAll<ArtistsTable>();
                connection.InsertAll(artists);
            });
            connection.RunInTransaction(() =>
            {
                connection.DeleteAll<AlbumArtistsTable>();
                connection.InsertAll(albumArtists);
            });
            connection.RunInTransaction(() =>
            {
                connection.DeleteAll<GenresTable>();
                connection.InsertAll(genres);
            });
        }

        private static SongsTable CreateSongsTable(SongData song)
        {
            //if (song.Path == c:\) (root directory) GetDirectoryName == null

            //string dir = (!String.IsNullOrWhiteSpace(song.Path)) ? Path.GetDirectoryName(song.Path) : "UnknownDirectory"; 
            //string folderName = Path.GetFileName(dir);
            return new SongsTable()
            {
                Album = song.Tag.Album ?? "",
                AlbumArtist = song.Tag.AlbumArtist ?? "",
                AlbumArt = song.AlbumArtPath ?? "",
                Artists = song.Tag.Artists ?? "",
                Bitrate = song.Bitrate,
                CloudUserId = song.CloudUserId ?? "",
                Comment = song.Tag.Comment ?? "",
                Composers = song.Tag.Composers ?? "",
                Conductor = song.Tag.Conductor ?? "",
                DateAdded = song.DateAdded,
                DateCreated = song.DateCreated,
                DateModified = song.DateModified,
                DirectoryName = song.DirectoryPath ?? "",
                Duration = song.Duration,
                Disc = song.Tag.Disc,
                DiscCount = song.Tag.DiscCount,
                Filename = song.Filename ?? "",
                FileSize = (long)song.FileSize,
                FirstArtist = song.Tag.FirstArtist ?? "",
                FirstComposer = song.Tag.FirstComposer ?? "",
                FolderName = song.FolderName ?? "",
                Genres = song.Tag.Genres ?? "",
                IsAvailable = song.IsAvailable,
                LastPlayed = song.LastPlayed,
                Lyrics = song.Tag.Lyrics ?? "",
                MusicSourceType = song.MusicSourceType,
                Path = song.Path ?? "",
                PlayCount = song.PlayCount,
                Rating = song.Tag.Rating,
                SongId = song.SongId,
                Title = song.Tag.Title ?? "",
                Track = song.Tag.Track,
                TrackCount = song.Tag.TrackCount,
                Year = song.Tag.Year
            };
        }

        private static NowPlayingSong CreateNowPlayingSong(NowPlayingTable song)
        {
            NowPlayingSong s = new NowPlayingSong();
            s.Artist = song.Artist;
            s.Album = song.Album;
            s.Path = song.Path;
            s.ImagePath = song.ImagePath;
            s.Position = song.Position;
            s.SongId = song.SongId;
            s.SourceType = (MusicSource)song.SourceType;
            s.Title = song.Title;
            return s;
        }

        #region Get

        public string GetAlbumArt(string album)
        {
            var q = connection.Table<AlbumsTable>().Where(a => a.Album.Equals(album)).ToList();
            string imagepath = AppConstants.AlbumCover;
            if (q.Count > 0)
            {
                imagepath = q.FirstOrDefault().ImagePath;
            }
            return imagepath;
        }

        public string GetLyrics(int id)
        {
            var list = connection.Table<SongsTable>().Where(s => s.SongId.Equals(id)).ToList();
            if (list.Count == 0) return "";
            else return list.FirstOrDefault().Lyrics;
        }

        public async Task<string> GetLyricsAsync(int id)
        {
            var list = await connectionAsync.Table<SongsTable>().Where(s => s.SongId.Equals(id)).ToListAsync();
            if (list.Count == 0) return "";
            else return list.FirstOrDefault().Lyrics;
        }

        public SongData GetSongData(int songId)
        {
            var l = connection.Table<SongsTable>().Where(e => e.SongId.Equals(songId)).ToList();
            var q = l.FirstOrDefault();
            if (q == null)
            {
                Logger2.Current.WriteMessage("GetSongData null id=" + songId, NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);
            }//!
            SongData s = CreateSongData(q);
            return s;
        }

        public async Task<SongData> GetSongDataAsync(int songId)
        {
            var l = await connectionAsync.Table<SongsTable>().Where(e => e.SongId.Equals(songId)).ToListAsync();
            var q = l.FirstOrDefault();
            SongData s;
            if (q == null)
            {
                Logger2.Current.WriteMessage("GetSongDataAsync null id=" + songId, NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);

                s = GetEmptySongData();//!
            }
            else
            {
                s = CreateSongData(q);
            }
            return s;
        }

        public List<NowPlayingSong> GetNowPlayingSongs()
        {
            List<NowPlayingSong> songs = new List<NowPlayingSong>();
            var query = connection.Table<NowPlayingTable>().OrderBy(s => s.Position).ToList();
            foreach (var e in query)
            {
                songs.Add(CreateNowPlayingSong(e));
            }
            return songs;
        }


        public NowPlayingSong GetNowPlayingSong(int songId)
        {
            var list = connection.Table<NowPlayingTable>().Where(s => s.SongId.Equals(songId)).ToList();
            if (list.Count > 0)
            {
                return CreateNowPlayingSong(list.FirstOrDefault());
            }
            return null;
        }

        public async Task<SongItem> GetSongItemAsync(int id)
        {
            var l = await connectionAsync.Table<SongsTable>().Where(s => s.SongId.Equals(id)).ToListAsync();
            var i = l.FirstOrDefault();
            return new SongItem(i);
        }

        public async Task<SongItem> GetSongItemIfExistAsync(string path)
        {
            var l = await connectionAsync.Table<SongsTable>().Where(s => s.Path.Equals(path)).ToListAsync();
            if (l.Count == 0) return null;
            else
            {
                return new SongItem(l.FirstOrDefault());
            }
        }

        public async Task<ObservableCollection<SongItem>> GetAllSongItemsAsync()
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var result = await songsConnectionAsync.OrderBy(s => s.Title).ToListAsync();
            foreach (var item in result)
            {
                songs.Add(new SongItem(item));
            }
            return songs;
        }

        public async Task<ObservableCollection<SongItem>> GetLocalSongItemsAsync()
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var result = await songsConnectionAsync.
                Where(s => s.MusicSourceType == (int)MusicSource.LocalFile || s.MusicSourceType == (int)MusicSource.LocalNotMusicLibrary).
                OrderBy(s => s.Title).ToListAsync();
            foreach (var item in result)
            {
                songs.Add(new SongItem(item));
            }
            return songs;
        }

        public async Task<List<SongsTable>> GetSongsWithoutAlbumArtAsync()
        {
           return await songsConnectionAsync.Where(s => s.AlbumArt == "").ToListAsync();
        }

        public async Task<List<SongsTable>> GetSongsFromAlbumItemAsync(AlbumItem album)
        {
            return await songsConnectionAsync.Where(s => s.Album.Equals(album.AlbumParam) && s.AlbumArtist.Equals(album.AlbumArtist)).ToListAsync();
        }

        public async Task<List<SongItem>> GetSongItemsForPlaylistAsync(IEnumerable<string> paths)
        {
            List<SongItem> songs = new List<SongItem>();
            //http://stackoverflow.com/questions/12832483/is-there-another-way-to-take-n-at-a-time-than-a-for-loop

            const int N = 512;
            string[] array = new string[N];               // Temporary array of N items.
            int i = 0;
            foreach (var path in paths)
            {         // Just one iterator.
                array[i++] = path;              // Store a reference to this item.
                if (i == N)
                {                // When we have N items,
                    var query = await connectionAsync.Table<SongsTable>().Where(s => array.Contains(s.Path)).ToListAsync();
                    foreach (var songPath in array)
                    {
                        var song = query.FirstOrDefault(s => s.Path.Equals(songPath));
                        if (song!=null) songs.Add(new SongItem(song));
                    }

                    i = 0;                   // and reset the array index.
                }
            }

            // remaining items
            if (i > 0)
            {
                var array2 = array.Take(i);
                var query = await connectionAsync.Table<SongsTable>().Where(s => array2.Contains(s.Path)).ToListAsync();
                foreach (var path in array2)
                {
                    var song = query.FirstOrDefault(s => s.Path.Equals(path));
                    if (song != null) songs.Add(new SongItem(song));
                }
            }

            return songs;
        }

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromAlbumAsync(string album, string albumArtist)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();

            var result = await songsConnectionAsync.Where(a => (a.Album.Equals(album) && a.AlbumArtist.Equals(albumArtist))).ToListAsync();
            var list = result.OrderBy(s => s.Disc).ThenBy(t => t.Track).ThenBy(u => u.Title);
            
            foreach (var item in list)
            {
                songs.Add(new SongItem(item));
            }
            return songs;
        }

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromAlbumArtistAsync(string albumArtist)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            List<SongsTable> list = await connectionAsync.QueryAsync<SongsTable>("SELECT * FROM SongsTable WHERE AlbumArtist = ? OR AlbumArtist LIKE ? OR AlbumArtist LIKE ? OR AlbumArtist LIKE ?", albumArtist, albumArtist + "; %", "%; " + albumArtist, "%; " + albumArtist + "; %");

            foreach (var item in list)
            {
                if (item.IsAvailable == 1)
                {
                    songs.Add(new SongItem(item));
                }
            }
            return songs;
        }

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromArtistAsync(string artist)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            try
            {
                var result = await connectionAsync.QueryAsync<SongsTable>("SELECT * FROM SongsTable WHERE Artists = ? OR Artists LIKE ? OR Artists LIKE ? OR Artists LIKE ?", artist, artist + "; %", "%; " + artist, "%; " + artist + "; %");
                
                foreach (var item in result.OrderBy(s => s.Album).ThenBy(t => t.Track))
                {
                    if (item.IsAvailable == 1)
                    {
                        songs.Add(new SongItem(item));
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return songs;
        }

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromFolderAsync(string directory, bool includeSubFolders = false)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            if (includeSubFolders)
            {
                var result = await songsConnectionAsync.Where(f => f.DirectoryName.StartsWith(directory)).ToListAsync();
                foreach (var item in result.OrderBy(i => i.DirectoryName).ThenBy(j => j.Title))
                {
                    songs.Add(new SongItem(item));
                }
            }
            else
            {
                var result = await songsConnectionAsync.Where(f => f.DirectoryName.Equals(directory)).OrderBy(s => s.Title).ToListAsync();
                foreach (var item in result)
                {
                    songs.Add(new SongItem(item));
                }
            }
            
            return songs;
        }

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromGenreAsync(string genre)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            try
            {
                List<SongsTable> list = await connectionAsync.QueryAsync<SongsTable>("SELECT * FROM SongsTable WHERE Genres = ? OR Genres LIKE ? OR Genres LIKE ? OR Genres LIKE ?", genre, genre + "; %", "%; " + genre, "%; " + genre + "; %");
                foreach (var item in list)
                {
                    if (item.IsAvailable == 1)
                    {
                        songs.Add(new SongItem(item));
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return songs;
        }

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromPlainPlaylistAsync(int id)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var playlistEntries = await connectionAsync.Table<PlainPlaylistEntryTable>().Where(p => p.PlaylistId.Equals(id)).ToListAsync();
            var songsList = await connectionAsync.QueryAsync<SongsTable>(@"SELECT * FROM SongsTable
                WHERE SongId IN 
                (SELECT PlainPlaylistEntryTable.SongId FROM PlainPlaylistEntryTable WHERE PlaylistId = ? ORDER BY PlainPlaylistEntryTable.Place)", id, id);
            var result = songsList.Where(s => s.IsAvailable == 1);
            foreach (var item in playlistEntries)    
            {
                var song = result.FirstOrDefault(s => s.SongId == item.SongId);
                if (song != null) songs.Add(new SongItem(song));
            }
            
            return songs;
        }

        public async Task <ObservableCollection<SongItem>> GetSongItemsFromSmartPlaylistAsync(int id)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
       
            List<int> list = new List<int>();
            var q2 = await connectionAsync.Table<SmartPlaylistsTable>().Where(p => p.SmartPlaylistId.Equals(id)).ToListAsync();
            var smartPlaylist = q2.FirstOrDefault();
            string name = smartPlaylist.Name;
            int maxNumber = smartPlaylist.SongsNumber;
            string sorting = SPUtility.SPsorting[smartPlaylist.SortBy];

            string less = null;
            string greater = null;
            bool ORcondition;

            var rules = await connectionAsync.Table<SmartPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(id)).ToListAsync();
            if (rules.Count != 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("SELECT * FROM SongsTable WHERE IsAvailable > 0 AND ");
                int i = 1;
                foreach (var rule in rules) //x = condition
                {
                    ORcondition = false;
                    string boolOperator = rule.Operator;
                    string comparison = SPUtility.SPConditionComparison[rule.Comparison];
                    string item = SPUtility.SPConditionItem[rule.Item];
                    string value = rule.Value;

                    if (rule.Comparison.Equals(SPUtility.Comparison.Contains))
                    {
                        value = "'%" + value + "%'";
                    }
                    else if (rule.Comparison.Equals(SPUtility.Comparison.DoesNotContain))
                    {
                        value = "'%" + value + "%'";
                    }
                    else if (rule.Comparison.Equals(SPUtility.Comparison.StartsWith))
                    {
                        value = "'%" + value + "'";
                    }
                    else if (rule.Comparison.Equals(SPUtility.Comparison.EndsWith))
                    {
                        value = "'" + value + "%'";
                    }
                    else if (rule.Comparison.Equals(SPUtility.Comparison.Is))
                    {
                        value = "'" + value + "'";
                    }
                    else if (rule.Comparison.Equals(SPUtility.Comparison.IsNot))
                    {
                        value = "'" + value + "'";
                    }
                    else if (rule.Comparison.Equals(SPUtility.Comparison.IsGreater))
                    {
                        value = "'" + value + "'";
                    }
                    else if (rule.Comparison.Equals(SPUtility.Comparison.IsLess))
                    {
                        value = "'" + value + "'";
                    }
                    else if (rule.Comparison.Equals(SPUtility.ComparisonEx.IsGreaterOR))
                    {
                        greater = "'" + value + "'";
                        ORcondition = true;
                    }
                    else if (rule.Comparison.Equals(SPUtility.ComparisonEx.IsLessOR))
                    {
                        less = "'" + value + "'";
                        ORcondition = true;
                    }

                    if (greater != null && less != null)
                    {
                        builder.Append("(").Append(item).Append(" < ").Append(less).Append(" OR ").Append(item).Append(" > ").Append(greater).Append(") ");
                        greater = null;
                        less = null;
                        if (i < rules.Count)
                        {
                            builder.Append(boolOperator).Append(" ");
                        }
                    }
                    if (!ORcondition)
                    {
                        builder.Append("(").Append(item).Append(" ").Append(comparison).Append(" ").Append(value).Append(") ");
                        if (i < rules.Count)
                        {
                            builder.Append(boolOperator).Append(" ");
                        }
                    }
                    
                    i++;
                }
                
                builder.Append("order by ").Append(sorting).Append(" limit ").Append(maxNumber);

                List<SongsTable> q = await connectionAsync.QueryAsync<SongsTable>(builder.ToString());

                foreach (var song in q.Where(s=>s.MusicSourceType == (int)Enums.MusicSource.LocalFile || s.MusicSourceType == (int)Enums.MusicSource.OnlineFile))
                {
                    songs.Add(new SongItem(song));
                }
            }

            return songs;
        }

        public ObservableCollection<SongItem> GetSongItemsFromNowPlaying()
        {
            var query = connection.Table<NowPlayingTable>().OrderBy(e => e.Position).ToList();
            ObservableCollection<SongItem> list = new ObservableCollection<SongItem>();
            foreach (var e in query)
            {
                var type = (MusicSource)e.SourceType;
                if (type == MusicSource.LocalFile || type == MusicSource.LocalNotMusicLibrary || type == MusicSource.Dropbox || type == MusicSource.OneDrive || type == MusicSource.PCloud)
                {
                    var query2 = connection.Table<SongsTable>().Where(x => x.SongId.Equals(e.SongId)).FirstOrDefault();
                    if (query2 != null)
                    {
                        var newSong = new SongItem(query2);
                        newSong.SourceType = type;
                        list.Add(newSong);
                    }
                }
                else if (type == MusicSource.RadioJamendo)
                {
                    SongItem s = new SongItem();
                    s.SourceType = MusicSource.RadioJamendo;
                    s.Title = e.Title;
                    s.Artist = e.Artist;
                    s.Album = e.Album;
                    s.Path = e.Path;
                    s.CoverPath = e.ImagePath;
                    s.SongId = e.SongId;
                    list.Add(s);
                }
            }
            return list;
        }


        public async Task<ObservableCollection<AlbumItem>> GetAlbumItemsAsync()
        {
            ObservableCollection<AlbumItem> albums = new ObservableCollection<AlbumItem>();
            var query = await connectionAsync.Table<AlbumsTable>().OrderBy(a=>a.Album).ToListAsync();
            foreach(var album in query)
            {
                albums.Add(new AlbumItem(album));
            }
            return albums;
        }

        public async Task<List<AlbumsTable>> GetAlbumsTable()
        {
            return await connectionAsync.Table<AlbumsTable>().ToListAsync();
        }

        public async Task<AlbumItem> GetAlbumItemAsync(int id)
        {
            var result = await connectionAsync.Table<AlbumsTable>().Where(a => a.AlbumId.Equals(id)).ToListAsync();
            if (result.Count > 0)
            {
                return new AlbumItem(result.FirstOrDefault());
            }
            else
            {
                return new AlbumItem();
            }
        }

        public async Task<AlbumItem> GetAlbumItemAsync(string album, string albumArtist)
        {
            var result = await connectionAsync.Table<AlbumsTable>().Where(a => (a.Album.Equals(album) && a.AlbumArtist.Equals(albumArtist))).ToListAsync();
            if (result.Count > 0)
            {
                return new AlbumItem(result.FirstOrDefault());
            }
            else
            {
                return new AlbumItem();
            }
        }

        public async Task<ObservableCollection<AlbumItem>> GetAlbumItemsFromAlbumArtistAsync(string albumArtist)
        {
            List<AlbumsTable> list = await connectionAsync.QueryAsync<AlbumsTable>("SELECT * FROM AlbumsTable WHERE AlbumArtist = ? OR AlbumArtist LIKE ? OR AlbumArtist LIKE ? OR AlbumArtist LIKE ?", albumArtist, albumArtist + "; %", "%; " + albumArtist, "%; " + albumArtist + "; %");
            ObservableCollection<AlbumItem> albums = new ObservableCollection<AlbumItem>();
            foreach(var item in list)
            {
                albums.Add(new AlbumItem(item));
            }
            return albums;
        }

        public async Task<ObservableCollection<AlbumArtistItem>> GetAlbumArtistItemsAsync()
        {
            ObservableCollection<AlbumArtistItem> list = new ObservableCollection<AlbumArtistItem>();
            var query = await connectionAsync.Table<AlbumArtistsTable>().OrderBy(s => s.AlbumArtist).ToListAsync();
            foreach(var item in query)
            {
                list.Add(new AlbumArtistItem(item));
            }
            return list;
        }

        public async Task<ObservableCollection<ArtistItem>> GetArtistItemsAsync()
        {
            ObservableCollection<ArtistItem> artists = new ObservableCollection<ArtistItem>();
            var query = await connectionAsync.Table<ArtistsTable>().OrderBy(s => s.Artist).ToListAsync();
            foreach(var item in query)
            {
                artists.Add(new ArtistItem(item));
            }
            return artists;
        }

        public async Task<AlbumArtistItem> GetAlbumArtistItemAsync(int albumArtistId)
        {
            var query = await connectionAsync.Table<AlbumArtistsTable>().Where(a => a.AlbumArtistId.Equals(albumArtistId)).ToListAsync();
            if (query.Count > 0)
            {
                return new AlbumArtistItem(query.FirstOrDefault());
            }
            else
            {
                return new AlbumArtistItem();
            }
        }

        public async Task<ArtistItem> GetArtistItemAsync(int artistId)
        {
            var result = await connectionAsync.Table<ArtistsTable>().Where(a => a.ArtistId.Equals(artistId)).ToListAsync();
            if (result.Count > 0)
            {
                return new ArtistItem(result.FirstOrDefault());
            }
            else
            {
                return new ArtistItem();
            }
        }

        public async Task<ArtistItem> GetArtistItemAsync(string artist)
        {
            var result = await connectionAsync.Table<ArtistsTable>().Where(a => a.Artist.Equals(artist)).ToListAsync();
            if (result.Count > 0)
            {
                return new ArtistItem(result.FirstOrDefault());
            }
            else
            {
                return new ArtistItem();
            }
        }


        public async Task<ObservableCollection<FolderItem>> GetFolderItemsAsync()
        {
            ObservableCollection<FolderItem> folders = new ObservableCollection<FolderItem>();
            var query = await connectionAsync.Table<FoldersTable>().ToListAsync();
            var ordered = query.OrderBy(f => f.Directory.ToLower());
            foreach (var item in ordered)
            {
                folders.Add(new FolderItem(item));
            }
            return folders;
        }

        public async Task<ObservableCollection<GenreItem>> GetGenreItemsAsync()
        {
            ObservableCollection<GenreItem> genres = new ObservableCollection<GenreItem>();
            var query = await connectionAsync.Table<GenresTable>().OrderBy(g=>g.Genre).ToListAsync();
            foreach (var item2 in query)
            {
                genres.Add(new GenreItem(item2));
            }
            return genres;
        }

        public async Task<ObservableCollection<PlaylistItem>> GetPlaylistItemsAsync()
        {
            ObservableCollection<PlaylistItem> playlists = new ObservableCollection<PlaylistItem>();

            var query1 = await connectionAsync.Table<SmartPlaylistsTable>().OrderBy(p => p.SmartPlaylistId).ToListAsync();
            Dictionary<int, string> ids = Helpers.ApplicationSettingsHelper.PredefinedSmartPlaylistsId();
            foreach (var item in query1) //default smart playlists
            {
                if (ids.ContainsKey(item.SmartPlaylistId))
                {
                    playlists.Add(new PlaylistItem(item));
                }
            }
            var query2 = query1.OrderBy(p => p.Name);
            foreach (var item in query2) //user smart playlists
            {
                if (!ids.ContainsKey(item.SmartPlaylistId))
                {
                    playlists.Add(new PlaylistItem(item));

                }
            }

            var query = await connectionAsync.Table<PlainPlaylistsTable>().ToListAsync();
            foreach (var item in query.OrderBy(p => p.Name.ToLower()))
            {
                playlists.Add(new PlaylistItem(item));
            }

            return playlists;
        }


        public async Task<ObservableCollection<PlaylistItem>> GetPlainPlaylistsAsync()
        {
            var query = await connectionAsync.Table<PlainPlaylistsTable>().OrderBy(p => p.Name).ToListAsync();
            ObservableCollection<PlaylistItem> list = new ObservableCollection<PlaylistItem>();
            foreach (var item in query)
            {
                list.Add(new PlaylistItem(item));
            }
            return list;
        }

        public async Task<PlaylistItem> GetPlainPlaylistAsync(int id)
        {
            var list = await connectionAsync.Table<PlainPlaylistsTable>().Where(s => s.PlainPlaylistId.Equals(id)).ToListAsync();
            var p = list.FirstOrDefault();
            if (p == null)
            {
                return new PlaylistItem();
            }
            return new PlaylistItem(p);
        }


        public async Task<PlaylistItem> GetSmartPlaylistAsync(int id)
        {
            var list = await connectionAsync.Table<SmartPlaylistsTable>().Where(s => s.SmartPlaylistId.Equals(id)).ToListAsync();
            var p = list.FirstOrDefault();
            return new SmartPlaylistItem(id, p.Name, p.SongsNumber, p.SortBy);
        }

        public async Task<List<SmartPlaylistEntryTable>> GetSmartPlaylistEntries(int id)
        {
            List<SmartPlaylistEntryTable> list = await connectionAsync.Table<SmartPlaylistEntryTable>().Where(s => s.PlaylistId.Equals(id)).ToListAsync();
            return list;
        }


        public async Task<List<ImportedPlaylist>> GetImportedPlaylistsAsync()
        {
            List<ImportedPlaylist> list = new List<ImportedPlaylist>();
            var playlists = await connectionAsync.Table<PlainPlaylistsTable>().ToListAsync();
            var playlistsEntries = await connectionAsync.Table<PlainPlaylistEntryTable>().ToListAsync();
            foreach(var q in playlists.Where(p => !String.IsNullOrEmpty(p.Path)))
            {
                var songIds = new List<int>();
                foreach(var item in playlistsEntries.Where(p => p.PlaylistId == q.PlainPlaylistId).OrderBy(p => p.Place))
                {
                    if (item.SongId > 0)
                    {
                        songIds.Add(item.SongId);
                    }
                }
                list.Add(new ImportedPlaylist()
                {
                    DateModified = q.DateModified,
                    Name = q.Name,
                    Path = q.Path,
                    PlainPlaylistId = q.PlainPlaylistId,
                    SongIds = songIds
                });
            }
            return list;
        }

        //public async Task<List<ImportedPlaylistsTable>> GetImportedPlaylists()
        //{
        //    List<ImportedPlaylistsTable> list = await connectionAsync.Table<ImportedPlaylistsTable>().ToListAsync();
        //    return list;
        //}

        public async Task<List<SimpleRadioData>> GetRadioFavouritesAsync()
        {
            List<SimpleRadioData> list = new List<SimpleRadioData>();

            var res = await connectionAsync.Table<FavouriteRadiosTable>().ToListAsync();

            foreach(var item in res)
            {
                list.Add(new SimpleRadioData(item.Id, (RadioType)item.RadioType, item.Name));
            }

            return list;
        }

        #endregion

        #region Insert

        public async Task AddToPlaylist(int playlistId, System.Linq.Expressions.Expression<Func<SongsTable, bool>> condition, Func<SongsTable, object> sort)
        {
            var query = await connectionAsync.Table<SongsTable>().Where(condition).ToListAsync();
            List<PlainPlaylistEntryTable> list = new List<PlainPlaylistEntryTable>();
            var l = await connectionAsync.Table<PlainPlaylistEntryTable>().Where(p => p.PlaylistId == playlistId).ToListAsync();
            int lastPosition = l.Count;
            var sorted = query.OrderBy(sort);
            foreach (var item in sorted)
            {
                lastPosition++;
                var newEntry = new PlainPlaylistEntryTable()
                {
                    PlaylistId = playlistId,
                    SongId = item.SongId,
                    Place = lastPosition,
                };
                list.Add(newEntry);
            }
            await connectionAsync.InsertAllAsync(list);
        }

        public async Task AddToPlaylist(int playlistId, IEnumerable<SongItem> songs)
        {
            List<PlainPlaylistEntryTable> list = new List<PlainPlaylistEntryTable>();
            var l = await connectionAsync.Table<PlainPlaylistEntryTable>().Where(p => p.PlaylistId == playlistId).ToListAsync();
            int lastPosition = l.Count;
            foreach (var song in songs)
            {
                lastPosition++;
                var newEntry = new PlainPlaylistEntryTable()
                {
                    PlaylistId = playlistId,
                    SongId = song.SongId,
                    Place = lastPosition,
                };
                list.Add(newEntry);
            }
            await connectionAsync.InsertAllAsync(list);
        }

        public async Task AddToPlaylist(int playlistId, IEnumerable<int> songIds)
        {
            List<PlainPlaylistEntryTable> list = new List<PlainPlaylistEntryTable>();
            var l = await connectionAsync.Table<PlainPlaylistEntryTable>().Where(p => p.PlaylistId == playlistId).ToListAsync();
            int lastPosition = l.Count;
            foreach (var id in songIds)
            {
                lastPosition++;
                var newEntry = new PlainPlaylistEntryTable()
                {
                    PlaylistId = playlistId,
                    SongId = id,
                    Place = lastPosition,
                };
                list.Add(newEntry);
            }
            await connectionAsync.InsertAllAsync(list);
        }

        public async Task AddNowPlayingToPlaylist(int playlistId)
        {
            var songs = await connectionAsync.Table<NowPlayingTable>().ToListAsync();
            List<PlainPlaylistEntryTable> list = new List<PlainPlaylistEntryTable>();
            var l = await connectionAsync.Table<PlainPlaylistEntryTable>().Where(p => p.PlaylistId == playlistId).ToListAsync();
            int lastPosition = l.Count;
            foreach (var item in songs)
            {
                if ((MusicSource)item.SourceType == MusicSource.LocalFile)
                {
                    lastPosition++;
                    var newEntry = new PlainPlaylistEntryTable()
                    {
                        PlaylistId = playlistId,
                        SongId = item.SongId,
                        Place = lastPosition,
                    };
                    list.Add(newEntry);
                }
            }
            await connectionAsync.InsertAllAsync(list);
        }

        public async Task<int> InsertSongAsync(SongData songData)
        {
            var q = await connectionAsync.Table<SongsTable>().Where(s => s.Path.Equals(songData.Path)).ToListAsync();
            if (q.Count == 0)
            {
                var newSong = CreateSongsTable(songData);
                await connectionAsync.InsertAsync(newSong);
                return newSong.SongId;
            }
            else
            {
                return q.FirstOrDefault().SongId;
            }
        }

        public async Task InsertSongsAsync(IEnumerable<SongData> list)
        {
            List<SongsTable> newSongs = new List<SongsTable>();
            foreach (var item in list)
            {
                newSongs.Add(CreateSongsTable(item));
            }
            await connectionAsync.InsertAllAsync(newSongs);
        }

        public int InsertPlainPlaylist(string _name)
        {
            var newplaylist = new PlainPlaylistsTable
            {
                Name = _name,
                IsHidden = false,
                DateModified = DateTime.MinValue,
            };

            connection.Insert(newplaylist);
            return newplaylist.PlainPlaylistId;
        }

        public int InsertPlainPlaylist(PlaylistItem item)
        {
            var newplaylist = new PlainPlaylistsTable
            {
                Name = item.Name,
                DateModified = item.DateModified,
                IsHidden = item.IsHidden,
                Path = item.Path,
            };

            connection.Insert(newplaylist);
            return newplaylist.PlainPlaylistId;
        }

        /// <summary>
        /// InsertPlainPlaylistEntryAsync
        /// </summary>
        /// <param name="_playlistId"></param>
        /// <param name="list">SongId,Place</param>
        /// <returns></returns>
        public async Task InsertPlainPlaylistEntryAsync(int _playlistId, List<Tuple<int,int>> list)
        {
            List<PlainPlaylistEntryTable> l = new List<PlainPlaylistEntryTable>();
            foreach (var item in list)
            {
                l.Add(new PlainPlaylistEntryTable()
                {
                    PlaylistId = _playlistId,
                    SongId = item.Item1,
                    Place = item.Item2
                });
            }
            await connectionAsync.InsertAllAsync(l);
        }

        //error constraint https://rink.hockeyapp.net/manage/apps/308671/app_versions/56/crash_reasons/151334480
        //two position same ?
        public async Task InsertNewNowPlayingPlaylistAsync(IEnumerable<SongItem> songs)
        {
            List<NowPlayingTable> list = new List<NowPlayingTable>();
            connection.DeleteAll<NowPlayingTable>();
            int i = 0;
            foreach (var item in songs)
            {
                list.Add(new NowPlayingTable()
                {
                    Artist = item.Artist,
                    Album = item.Album,
                    Path = item.Path,
                    ImagePath = item.CoverPath,
                    Position = i,
                    SongId = item.SongId,
                    SourceType = (int)item.SourceType,
                    Title = item.Title,
                });
                i++;
            }
            await connectionAsync.InsertAllAsync(list);
        }

        public int InsertSmartPlaylist(string name, int songsNumber, string sorting)
        {
            var newplaylist = new SmartPlaylistsTable
            {
                Name = name,
                SongsNumber = songsNumber,
                SortBy = sorting,
                IsHidden = false
            };

            connection.Insert(newplaylist);
            return newplaylist.SmartPlaylistId;
        }

        public async Task<int> InsertSmartPlaylistAsync(string name, int songsNumber, string sorting)
        {
            var newplaylist = new SmartPlaylistsTable
            {
                Name = name,
                SongsNumber = songsNumber,
                SortBy = sorting,
                IsHidden = false
            };

            await connectionAsync.InsertAsync(newplaylist);
            return newplaylist.SmartPlaylistId;
        }

        public async Task InsertSmartPlaylistEntryAsync(int _playlistId, string _item, string _comparison, string _value, string _operator)
        {
            var newEntry = new SmartPlaylistEntryTable
            {
                PlaylistId = _playlistId,
                Item = _item,
                Comparison = _comparison,
                Operator = _operator,
                Value = _value,
            };

            await connectionAsync.InsertAsync(newEntry);
        }

        public void InsertSmartPlaylistEntry(int _playlistId, string _item, string _comparison, string _value, string _operator)
        {
            var newEntry = new SmartPlaylistEntryTable
            {
                PlaylistId = _playlistId,
                Item = _item,
                Comparison = _comparison,
                Operator = _operator,
                Value = _value,
            };

           connection.Insert(newEntry);
        }

        //public int InsertImportedPlaylist(string name, string path, int plainId)
        //{
        //    var p = new ImportedPlaylistsTable() {
        //        Name = name,
        //        Path = path,
        //        PlainPlaylistId = plainId
        //    };
        //    return connection.Insert(p);
        //}

        public async Task<List<SongItem>> InsertCloudItems(IEnumerable<SongData> songsdata, MusicSource type)
        {
            var indb = await connectionAsync.Table<SongsTable>().Where(s => s.MusicSourceType == (int)type).ToListAsync();
            List<SongItem> songs = new List<SongItem>();
            List<SongsTable> newSongs = new List<SongsTable>();
            List<SongsTable> updatedSongs = new List<SongsTable>();

            foreach (var song in songsdata)
            {
                var q = indb.FirstOrDefault(s => s.Path == song.Path);
                if (q == null)
                {
                    newSongs.Add(CreateSongsTable(song));
                }
                else
                {
                    if (q.DateModified != song.DateModified)
                    {

                    }
                }
            }
            if (newSongs.Count > 0)
            {
                await connectionAsync.InsertAllAsync(newSongs);
            }
            indb = await connectionAsync.Table<SongsTable>().Where(s => s.MusicSourceType == (int)type).ToListAsync();
            var paths = songsdata.Select(s => s.Path);
            var fromDatabase = indb.Where(s => paths.Contains(s.Path));
            foreach(var song in fromDatabase)
            {
                songs.Add(new SongItem(song));
            }
            return songs;
        }

        public async Task<int> InsertOrUpdateImportedPlaylist(ImportedPlaylist playlist)
        {
            int id;
            var plainPlaylists = await connectionAsync.Table<PlainPlaylistsTable>().Where(p => p.Path.Equals(playlist.Path)).ToListAsync();
            if (plainPlaylists.Count == 0)
            {
                var table = new PlainPlaylistsTable()
                {
                    DateModified = playlist.DateModified,
                    Name = playlist.Name,
                    Path = playlist.Path,
                    IsHidden = false,
                };
                connection.Insert(table);
                id = table.PlainPlaylistId;
            }
            else
            {
                var existingPlaylist = plainPlaylists.FirstOrDefault();
                id = existingPlaylist.PlainPlaylistId;
                existingPlaylist.DateModified = playlist.DateModified;
                existingPlaylist.Name = playlist.Name;
                await connectionAsync.UpdateAsync(existingPlaylist);
                await connectionAsync.ExecuteAsync("DELETE FROM PlainPlaylistEntryTable WHERE PlaylistId = ?", playlist.PlainPlaylistId);
            }

            int i = 0;
            List<PlainPlaylistEntryTable> entries = new List<PlainPlaylistEntryTable>();
            foreach (var item in playlist.SongIds)
            {
                var entry = new PlainPlaylistEntryTable()
                {
                    Place = i,
                    PlaylistId = id,
                    SongId = item,
                };
                entries.Add(entry);
                i++;
            }
            await connectionAsync.InsertAllAsync(entries);

            return id;
        }

        public async Task AddRadioToFavourites(SimpleRadioData radio)
        {
            await connectionAsync.InsertAsync(new FavouriteRadiosTable()
            {
                RadioId = radio.BroadcastId,
                RadioType = (int)radio.Type,
                Name = radio.Name
            });
        }

        #endregion

        #region Delete

        public async Task DeleteSmartPlaylistEntries(int playlistId)
        {
            var items = await connectionAsync.Table<SmartPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(playlistId)).ToListAsync();
            foreach (var item in items)
            {
                connection.Delete<SmartPlaylistEntryTable>(item.Id);
            }
        }

        public async Task DeleteSmartPlaylistAsync(int id)
        {
            await DeleteSmartPlaylistEntries(id);
            var list =  await connectionAsync.Table<SmartPlaylistsTable>().Where(p => p.SmartPlaylistId.Equals(id)).ToListAsync();
            var playlist = list.FirstOrDefault();
            await connectionAsync.DeleteAsync(playlist);
        }

        public async Task DeletePlainPlaylistEntryAsync(int songId, int playlistId)
        {
            var item = await connectionAsync.Table<PlainPlaylistEntryTable>().Where(s => s.SongId == songId && s.PlaylistId == playlistId).ToListAsync();
            await connectionAsync.DeleteAsync(item.FirstOrDefault());
        }

        public async Task DeletePlainPlaylistAsync(int playlistId)
        {
            await connectionAsync.ExecuteAsync("DELETE FROM PlainPlaylistEntryTable WHERE PlaylistId = ?", playlistId);
            //connection.Execute("DELETE FROM PlainPlaylistsTable WHERE PlainPlaylistId = ?");
            var q = await connectionAsync.Table<PlainPlaylistsTable>().Where(p => p.PlainPlaylistId == playlistId).FirstOrDefaultAsync();
            await connectionAsync.DeleteAsync(q);
            
            //var list = await connectionAsync.Table<PlainPlaylistsTable>().Where(p => p.PlainPlaylistId.Equals(playlistId)).ToListAsync();
            //var playlist = list.FirstOrDefault();
            //await connectionAsync.DeleteAsync(playlist);
        }


        public async Task DeleteAlbumAsync(string album, string albumArtist)
        {
            await connectionAsync.ExecuteAsync("DELETE FROM AlbumsTable WHERE Album = ? AND AlbumArtist = ?", album, albumArtist);
        }

        public async Task DeleteArtistAsync(string artist)
        {
            await connectionAsync.ExecuteAsync("DELETE FROM ArtistsTable WHERE Artist = ?", artist);
        }

        public async Task DeleteFolderAndSubFoldersAsync(string path)
        {
            connection.RunInTransaction(() =>
            {
                connection.Execute("UPDATE SongsTable SET IsAvailable = 0 WHERE DirectoryName LIKE ?", path + "%");
                connection.Execute("DELETE FROM FoldersTable WHERE Directory LIKE ?", path + "%");
            });
            await UpdateTables();
        }

        public async Task DeleteSong(int songId)
        {
            var q = await connectionAsync.Table<SongsTable>().Where(s => s.SongId.Equals(songId)).ToListAsync();
            if (q.Count == 1)
            {
                connection.Delete(q.FirstOrDefault());
            }
        }

        public async Task DeleteRadioFromFavourites(SimpleRadioData radio)
        {
            var r = await connectionAsync.Table<FavouriteRadiosTable>().Where(a => a.RadioId == radio.BroadcastId && a.RadioType == (int)radio.Type).ToListAsync();
            if (r.Count == 1)
            {
                connection.Delete(r.FirstOrDefault());
            }
        }

        #endregion

        #region Update

        public async Task UpdateAlbumImagePath(AlbumItem album)//error
        {
            await connectionAsync.ExecuteAsync("UPDATE AlbumsTable SET ImagePath = ? WHERE AlbumId = ?", album.ImagePath, album.AlbumId);
        }

        public async Task UpdateAlbumTableItemAsync(AlbumsTable album)
        {
            await connectionAsync.UpdateAsync(album);
        }

        public async Task UpdateAlbumsTable(IEnumerable<AlbumsTable> albums)//error
        {
            await connectionAsync.UpdateAllAsync(albums);
        }

        public void UpdateSongImagePath(SongItem song)
        {
           connection.Execute("UPDATE SongsTable SET AlbumArt = ? WHERE SongId = ?", song.CoverPath, song.SongId);
        }

        public async Task UpdateSongsImagePath(IEnumerable<SongsTable> songs)
        {
            await connectionAsync.UpdateAllAsync(songs);
        }

        public async Task UpdateSongImagePath(List<Tuple<int,string>> data)
        {
            foreach(var t in data)
            {
                await connectionAsync.ExecuteAsync("UPDATE SongsTable SET AlbumArt = ? WHERE SongId = ?", t.Item2, t.Item1);
            }
        }

        public async Task UpdateAlbumItem(AlbumItem album)
        {
            AlbumsTable t = new AlbumsTable()
            {
                Album = album.AlbumParam,
                AlbumArtist = album.AlbumArtist,
                AlbumId = album.AlbumId,
                Duration = album.Duration,
                ImagePath = album.ImagePath,
                LastAdded = album.LastAdded,
                SongsNumber = album.SongsNumber,
                Year = album.Year
            };
            await connectionAsync.UpdateAsync(t);
        }

        public async Task UpdateArtistItem(ArtistItem artist)
        {
            ArtistsTable t = new ArtistsTable()
            {
                AlbumsNumber = artist.AlbumsNumber,
                Artist = artist.ArtistParam,
                ArtistId = artist.ArtistId,
                Duration = artist.Duration,
                LastAdded = artist.LastAdded,
                SongsNumber = artist.SongsNumber
            };
            await connectionAsync.UpdateAsync(t);
        }

        public async Task UpdateGenreItem(GenreItem genre)
        {
            GenresTable t = new GenresTable()
            {
                Duration = genre.Duration,
                Genre = genre.Genre,
                GenreId = genre.GenreId,
                LastAdded = genre.LastAdded,
                SongsNumber = genre.SongsNumber
            };
            await connectionAsync.UpdateAsync(t);
        }

        public async Task UpdateSongData(SongData songData)
        {
            var song = CreateSongsTable(songData);
            await connectionAsync.UpdateAsync(song);
        }

        public async Task UpdateSongAlbumData(SongItem song)
        {
            await connectionAsync.ExecuteAsync("UPDATE SongsTable SET Album = ?, AlbumArtist = ?, Year = ? WHERE SongId = ?", song.Album, song.AlbumArtist, song.Year, song.SongId);
        }

        public async Task UpdateNowPlayingSong(SongData song)
        {
            await connectionAsync.ExecuteAsync("Update NowPlayingTable SET Title = ?, Artist = ?, Album = ? WHERE SongId = ?", song.Tag.Title, song.Tag.Artists, song.Tag.Album, song.SongId);
        }

        public async Task UpdateLyricsAsync(int id, string lyrics)
        {
            await connectionAsync.ExecuteAsync("UPDATE SongsTable SET Lyrics = ? WHERE SongId = ?", lyrics, id);
        }

        public async Task UpdateRatingAsync(int id, int rating)
        {
            await connectionAsync.ExecuteAsync("UPDATE SongsTable SET Rating = ? WHERE SongId = ?", rating, id);
        }

        public async Task UpdatePlaylistName(int id, string name)
        {
            await connectionAsync.ExecuteAsync("UPDATE PlainPlaylistsTable SET Name = ? WHERE PlainPlaylistId = ?", name, id);
        }

        public async Task UpdateSmartPlaylist(int id, string name, int songsNumber, string sorting)
        {
            var playlist = await connectionAsync.Table<SmartPlaylistsTable>().Where(p => p.SmartPlaylistId.Equals(id)).FirstOrDefaultAsync();
            if (playlist == null) return;
            playlist.Name = name;
            playlist.SongsNumber = songsNumber;
            playlist.SortBy = sorting;
            await connectionAsync.UpdateAsync(playlist);
        }

        public async Task UpdateSmartPlaylist(int id, string name, bool hide)
        {
            var playlist = await connectionAsync.Table<SmartPlaylistsTable>().Where(p => p.SmartPlaylistId.Equals(id)).FirstOrDefaultAsync();
            if (playlist == null) return;
            playlist.Name = name;
            playlist.IsHidden = hide;
            await connectionAsync.UpdateAsync(playlist);
        }

        //public async Task UpdateImportedPlaylist(ImportedPlaylistsTable table)
        //{
        //    await connectionAsync.UpdateAsync(table);
        //}

        public async Task UpdateImportedPlaylists(List<ImportedPlaylist> toDelete, List<ImportedPlaylist> newPlaylists, List<ImportedPlaylist> updatedPlaylists)
        {
            foreach(var playlist in toDelete)
            {
                await DeletePlainPlaylistAsync(playlist.PlainPlaylistId);
            }
            List<PlainPlaylistsTable> updatedPPT = new List<PlainPlaylistsTable>();            
            foreach (var playlist in updatedPlaylists)
            {
                PlainPlaylistsTable t = new PlainPlaylistsTable()
                {
                    DateModified = playlist.DateModified,
                    Name = playlist.Name,
                    Path = playlist.Path,
                    PlainPlaylistId = playlist.PlainPlaylistId,
                    IsHidden = false
                };
                updatedPPT.Add(t);
                await connectionAsync.ExecuteAsync("DELETE FROM PlainPlaylistEntryTable WHERE PlaylistId = ?", playlist.PlainPlaylistId);
                int i = 0;
                List<PlainPlaylistEntryTable> entries = new List<PlainPlaylistEntryTable>();
                foreach (var item in playlist.SongIds)
                {
                    var entry = new PlainPlaylistEntryTable()
                    {
                        Place = i,
                        PlaylistId = t.PlainPlaylistId,
                        SongId = item,
                    };
                    entries.Add(entry);
                    i++;
                }
                await connectionAsync.InsertAllAsync(entries);
            }
            await connectionAsync.UpdateAllAsync(updatedPPT);
            foreach(var playlist in newPlaylists)
            {
                PlainPlaylistsTable t = new PlainPlaylistsTable()
                {
                    DateModified = playlist.DateModified,
                    Name = playlist.Name,
                    Path = playlist.Path,
                    IsHidden = playlist.SongIds.Count == 0
                };
                connection.Insert(t);
                int i = 0;
                List<PlainPlaylistEntryTable> entries = new List<PlainPlaylistEntryTable>();
                foreach (var item in playlist.SongIds)
                {
                    var entry = new PlainPlaylistEntryTable()
                    {
                        Place = i,
                        PlaylistId = t.PlainPlaylistId,
                        SongId = item,
                    };
                    entries.Add(entry);
                    i++;
                }
                await connectionAsync.InsertAllAsync(entries);
            }
        }

        public async Task UpdatePlainPlaylistAsync(PlaylistItem playlist)
        {
            if (playlist.IsSmart) return;
            var item = new PlainPlaylistsTable()
            {
                DateModified = playlist.DateModified,
                Name = playlist.Name,
                Path = playlist.Path,
                PlainPlaylistId = playlist.Id,
                IsHidden = playlist.IsHidden
            };
            await connectionAsync.UpdateAsync(item);
        }

        /// <summary>
        /// Updates PlayCount and LastPlayed
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task UpdateSongStatistics(int id)
        {
            var list = await connectionAsync.Table<SongsTable>().Where(s => s.SongId.Equals(id)).ToListAsync();
            if (list.Count == 0) return;
            uint playCount = list.FirstOrDefault().PlayCount + 1;
            DateTime lastPlayed = DateTime.Now;
            await connectionAsync.ExecuteAsync("UPDATE SongsTable SET PlayCount = ?, LastPlayed = ? WHERE SongId = ?", playCount, lastPlayed, id);
        }

        public async Task UpdateSongDurationAsync(int songId, TimeSpan duration)//error
        {
            await connectionAsync.ExecuteAsync("UPDATE SongsTable SET Duration = ? WHERE SongId = ?", duration, songId);
        }

        #endregion

        public async Task UpdateDateCreatedAsync(List<SongsTable> songs)
        {
            await connectionAsync.UpdateAllAsync(songs);
        }

        #region Last.fm

        public async Task CacheTrackScrobbleAsync(string function, string artist, string title, string timestamp)
        {
            CachedScrobble scrobble = new CachedScrobble()
            {
                Artist = artist,
                Function = function,
                Timestamp = timestamp,
                Track = title
            };
            await connectionAsync.InsertAsync(scrobble);
        }

        public async Task CacheTrackScrobblesAsync(IEnumerable<TrackScrobble> scrobbles)
        {
            List<CachedScrobble> list = new List<CachedScrobble>();
            foreach (var scrobble in scrobbles)
            {
                CachedScrobble cs = new CachedScrobble()
                {
                    Artist = scrobble.Artist,
                    Function = "track.scrobble",
                    Timestamp = scrobble.Timestamp,
                    Track = scrobble.Track
                };
                list.Add(cs);
            }
            await connectionAsync.InsertAllAsync(list);
        }

        public async Task CacheTrackLoveAsync(string function, string artist, string title)
        {
            var list = await connectionAsync.Table<CachedScrobble>().Where(c => (c.Artist.Equals(artist) && c.Track.Equals(title) && c.Timestamp == "")).ToListAsync();
            if (list.Count == 0)
            {
                CachedScrobble scrobble = new CachedScrobble()
                {
                    Artist = artist,
                    Function = function,
                    Timestamp = "",
                    Track = title
                };
                await connectionAsync.InsertAsync(scrobble);
            }
            else
            {
                var scrobble = list.FirstOrDefault();
                scrobble.Function = function;
                await connectionAsync.UpdateAsync(scrobble);
            }
        }

        public async Task<List<CachedScrobble>> GetCachedScrobblesAsync()
        {
            var list = await connectionAsync.Table<CachedScrobble>().ToListAsync();
            return list;
        }

        public void DeleteCachedScrobble(int id)
        {
            connection.Execute("DELETE FROM CachedScrobble WHERE id = ?", id);
        }

        public async Task DeleteAllCachedScrobbles()
        {
            await connectionAsync.ExecuteAsync("DELETE FROM CachedScrobble");
        }

        public async Task DeleteCachedScrobblesLove()
        {
            await connectionAsync.ExecuteAsync("DELETE FROM CachedScrobble WHERE Function = ? OR Function = ?", "track.love", "track.unlove");
        }

        public async Task DeleteCachedScrobblesTrack()
        {
            await connectionAsync.ExecuteAsync("DELETE FROM CachedScrobble WHERE Function = ?", "track.scrobble");
        }

        #endregion

        #region FutureAccessTokens

        public async Task<string> GetAccessToken(string path)
        {
            var list = await connectionAsync.Table<FutureAccessTokensTable>().Where(f => f.FilePath.Equals(path)).ToListAsync();
            if (list.Count == 0) return null;
            else return list.FirstOrDefault().Token;
        }

        public async Task<string> DeleteAccessTokenAsync()
        {
            var list = await connectionAsync.Table<FutureAccessTokensTable>().ToListAsync();
            if (list.Count == 0) return null;
            var item = list.FirstOrDefault();
            string token = item.Token;
            await connectionAsync.DeleteAsync(item);
            return token;
        }

        public async Task SaveAccessToken(string path, string token, bool isFile)
        {
            FutureAccessTokensTable fatt = new FutureAccessTokensTable()
            {
                FilePath = path,
                IsFile = isFile,
                Token = token,
            };
            await connectionAsync.InsertAsync(fatt);
        }

        #endregion

        #region CloudStorageAccounts

        public async Task<int> AddCloudAccountAsync(string userId, CloudStorageType type, string username)
        {
            CloudAccountsTable item = new CloudAccountsTable()
            {
                Token = "",
                Type = (int)type,
                UserId = userId,
                UserName = username
            };
            int i = await connectionAsync.InsertAsync(item);
            return item.Id;
        }

        public async Task DeleteCloudAccountAsync(CloudAccount account)
        {
            var item = await connectionAsync.Table<CloudAccountsTable>().Where(a => a.Id.Equals(account.DBId)).ToListAsync();
            if (item.Count == 1)
            {
                await connectionAsync.DeleteAsync(item.FirstOrDefault());
            }
            else
            {
                //TODO error
            }
        }

        public async Task<List<CloudAccount>> GetAllCloudAccountsAsync()
        {
            var query = await connectionAsync.Table<CloudAccountsTable>().ToListAsync();
            List<CloudAccount> list = new List<CloudAccount>();
            foreach(var item in query)
            {
                list.Add(new CloudAccount(item.Id, item.UserId, (CloudStorageType)item.Type, item.UserName));
            }
            return list;
        }

        public List<CloudAccount> GetAllCloudAccounts()
        {
            var query = connection.Table<CloudAccountsTable>().ToList();
            List<CloudAccount> list = new List<CloudAccount>();
            foreach (var item in query)
            {
                list.Add(new CloudAccount(item.Id, item.UserId, (CloudStorageType)item.Type, item.UserName));
            }
            return list;
        }

        public async Task SaveCloudAccountTokenAsync(string userId, string token)
        {
            var items = await connectionAsync.Table<CloudAccountsTable>().Where(a => a.UserId.Equals(userId)).ToListAsync();
            if (items.Count == 1)
            {
                var item = items.FirstOrDefault();
                item.Token = token;
                await connectionAsync.UpdateAsync(item);
            }
            else
            {
                //TODO error
            }
        }

        public async Task<string> GetCloudAccountTokenAsync(string userId)
        {
            var items = await connectionAsync.Table<CloudAccountsTable>().Where(a => a.UserId.Equals(userId)).ToListAsync();
            if (items.Count == 1)
            {
                return items.FirstOrDefault().Token;
            }
            else
            {
                //TODO error
                return null;
            }
        }

        #endregion

        private static SongsTable CreateCopy(SongsTable s)
        {
            return new SongsTable()
            {
                Album = s.Album,
                AlbumArtist = s.AlbumArtist,
                AlbumArt = s.AlbumArt,
                Artists = s.Artists,
                Bitrate = s.Bitrate,
                CloudUserId = s.CloudUserId,
                Comment = s.Comment,
                Composers = s.Composers,
                Conductor = s.Conductor,
                DateAdded = s.DateAdded,
                DateModified = s.DateModified,
                DirectoryName = s.DirectoryName,
                Disc = s.Disc,
                DiscCount = s.DiscCount,
                Duration = s.Duration,
                Filename = s.Filename,
                FileSize = s.FileSize,
                FirstArtist = s.FirstArtist,
                FirstComposer = s.FirstComposer,
                FolderName = s.FolderName,
                Genres = s.Genres,
                IsAvailable = s.IsAvailable,
                LastPlayed = s.LastPlayed,
                Lyrics = s.Lyrics,
                MusicSourceType = s.MusicSourceType,
                Path = s.Path,
                PlayCount = s.PlayCount,
                Rating = s.Rating,
                SongId = s.SongId,
                Title = s.Title,
                Track = s.Track,
                TrackCount = s.TrackCount,
                Year = s.Year,
            };
        }

        private static SongData CreateSongData(SongsTable q)
        {
            Tags tag = new Tags()
            {
                Album = q.Album,
                AlbumArtist = q.AlbumArtist,
                Artists = q.Artists,
                Comment = q.Comment,
                Composers = q.Composers,
                Conductor = q.Conductor,
                Disc = q.Disc,
                DiscCount = q.DiscCount,
                FirstArtist = q.FirstArtist,
                FirstComposer = q.FirstComposer,
                Genres = q.Genres,
                Lyrics = q.Lyrics,
                Rating = q.Rating,
                Title = q.Title,
                Track = q.Track,
                TrackCount = q.TrackCount,
                Year = q.Year
            };
            SongData s = new SongData()
            {
                AlbumArtPath = q.AlbumArt,
                Bitrate = q.Bitrate,
                CloudUserId = q.CloudUserId,
                DateAdded = q.DateAdded,
                DirectoryPath = q.DirectoryName,
                DateModified = q.DateModified,
                DateCreated = q.DateCreated,
                Duration = q.Duration,
                Filename = q.Filename,
                FileSize = (ulong)q.FileSize,
                FolderName = q.FolderName,
                IsAvailable = q.IsAvailable,
                LastPlayed = q.LastPlayed,
                MusicSourceType = q.MusicSourceType,
                Path = q.Path,
                PlayCount = q.PlayCount,
                SongId = q.SongId,
                Tag = tag
            };
            return s;
        }

        public static SongData GetEmptySongData()
        {
            Tags tag = new Tags()
            {
                Album = "",
                AlbumArtist = "",
                Artists = "",
                Comment = "",
                Composers = "",
                Conductor = "",
                Disc = 0,
                DiscCount = 0,
                FirstArtist = "",
                FirstComposer = "",
                Genres = "",
                Lyrics = "",
                Rating = 0,
                Title = "",
                Track = 0,
                TrackCount = 0,
                Year = 0
            };
            SongData s = new SongData()
            {
                AlbumArtPath = "",
                Bitrate = 0,
                CloudUserId = "",
                DateAdded = DateTime.Now,
                DirectoryPath = "",
                DateModified = DateTime.UtcNow,
                DateCreated = DateTime.UtcNow,
                Duration = TimeSpan.Zero,
                Filename = "",
                FileSize = (ulong)0,
                FolderName = "",
                IsAvailable = 0,
                LastPlayed = DateTime.Now,
                MusicSourceType = 0,
                Path = "",
                PlayCount = 0,
                SongId = -2,
                Tag = tag
            };
            return s;
        }

        public void ClearCoverPaths()
        {
            connection.Execute("UPDATE AlbumsTable SET ImagePath = ''");
        }

        public void ResetNowPlaying()
        {
            connection.DropTable<NowPlayingTable>();
            connection.CreateTable<NowPlayingTable>();
        }
        public void update1()
        {
            connection.DropTable<SmartPlaylistEntryTable>();
            connection.DropTable<SmartPlaylistsTable>();
            connection.CreateTable<SmartPlaylistsTable>();
            connection.CreateTable<SmartPlaylistEntryTable>();
        }


        public void resetdb()
        {
            connection.DropTable<SongsTable>();
            connection.DropTable<PlainPlaylistEntryTable>();
            connection.DropTable<PlainPlaylistsTable>();
            connection.DropTable<NowPlayingTable>();
            connection.DropTable<FoldersTable>();
            connection.DropTable<GenresTable>();
            connection.DropTable<AlbumsTable>();
            connection.DropTable<ArtistsTable>();
            connection.DropTable<CachedScrobble>();

            connection.CreateTable<PlainPlaylistsTable>();
            connection.CreateTable<PlainPlaylistEntryTable>();
            connection.CreateTable<SongsTable>();
            connection.CreateTable<NowPlayingTable>();
            connection.CreateTable<FoldersTable>();
            connection.CreateTable<GenresTable>();
            connection.CreateTable<AlbumsTable>();
            connection.CreateTable<ArtistsTable>();
            connection.CreateTable<CachedScrobble>();
        }

        public void UpdateToVersion2()
        {
            connection.Execute("ALTER TABLE NowPlayingTable ADD COLUMN SourceType INTEGER");
            connection.Execute("UPDATE NowPlayingTable SET SourceType = 1");
            connection.CreateTable<ImportedPlaylistsTable>();
        }

        public bool DBCorrection()
        {
            bool recreate = false;
            try
            {
                var list = connection.Table<GenresTable>().ToList();
                if (list.Count > 0)
                {
                    connection.Execute("UPDATE GenresTable SET SongsNumber = ? WHERE GenreId = ?", list.FirstOrDefault().SongsNumber, list.FirstOrDefault().GenreId);
                }
            }
            catch (Exception ex)
            {
                recreate = true;
            }
            try
            {
                var list = connection.Table<FoldersTable>().ToList();
                if (list.Count > 0)
                {
                    connection.Execute("UPDATE FoldersTable SET SongsNumber = ? WHERE FolderId = ?", list.FirstOrDefault().SongsNumber, list.FirstOrDefault().FolderId);
                }
            }
            catch (Exception ex)
            {
                recreate = true;
            }

            if (recreate)
            {
                DeleteDatabase();
                CreateNewDatabase();
                //Diagnostics.Logger.Save("Recreate db");
                //Diagnostics.Logger.SaveToFile();
            }
            return recreate;
        }

        public void UpdateToVersion4()
        {
            connection.CreateTable<FutureAccessTokensTable>();
        }

        public void UpdateToVersion5()
        {
            connection.CreateTable<SongsTable>();
            connection.Execute("UPDATE SongsTable SET AlbumArt = ?", "");
            connection.Execute("UPDATE SongsTable SET DateModified = ?", 0);
        }

        public void UpdateToVersion6()
        {
            connection.CreateTable<CloudAccountsTable>();
        }

        public void UpdateToVersion7()
        {
            connection.CreateTable<SongsTable>();
            connection.Execute("UPDATE SongsTable SET MusicSourceType = ?", 1);
            connection.Execute("UPDATE SongsTable SET CloudUserId = ?", "");
        }

        public void UpdateToVersion8()
        {
            connection.Execute("UPDATE SongsTable SET IsAvailable = 0 WHERE Path LIKE ? OR Path LIKE ? OR Path LIKE ? OR Path LIKE ? OR Path LIKE ?", "%.ogg", "%.ape", "%.wv", "%.opus", "%.ac3");
            connection.Execute("DELETE FROM NowPlayingTable WHERE Path LIKE ? OR Path LIKE ? OR Path LIKE ? OR Path LIKE ? OR Path LIKE ?", "%.ogg", "%.ape", "%.wv", "%.opus", "%.ac3");
        }

        public void UpdateToVersion9()
        {
            connection.CreateTable<PlainPlaylistsTable>();
            connection.CreateTable<PlainPlaylistEntryTable>();
            connection.CreateTable<SmartPlaylistsTable>();
            connection.Execute("DELETE FROM PlainPlaylistEntryTable WHERE PlainPlaylistEntryTable.Id IN ( SELECT PlainPlaylistEntryTable.Id FROM PlainPlaylistEntryTable LEFT JOIN ImportedPlaylistsTable ON PlainPlaylistEntryTable.PlaylistId = ImportedPlaylistsTable.PlainPlaylistId)");
            connection.Execute("DELETE FROM PlainPlaylistsTable WHERE PlainPlaylistsTable.PlainPlaylistId IN ( SELECT PlainPlaylistsTable.PlainPlaylistId FROM PlainPlaylistsTable LEFT JOIN ImportedPlaylistsTable ON PlainPlaylistsTable.PlainPlaylistId = ImportedPlaylistsTable.PlainPlaylistId WHERE ImportedPlaylistsTable.PlainPlaylistId IS NOT NULL)");
            connection.Execute("DELETE FROM ImportedPlaylistsTable");
            connection.Execute("UPDATE PlainPlaylistsTable SET IsHidden = 0");
            connection.Execute("UPDATE SmartPlaylistsTable SET IsHidden = 0");
        }

        public void UpdateToVersion10()
        {
            connection.Execute("UPDATE SongsTable SET IsAvailable = 1 WHERE MusicSourceType = 5 OR MusicSourceType = 6 OR MusicSourceType = 8");
        }

        public void UpdateToVersion11()
        {
            connection.CreateTable<AlbumArtistsTable>();
            connection.CreateTable<SongsTable>();
            DateTime date = new DateTime(2016, 4, 26);
            connection.Execute("UPDATE SongsTable SET DateCreated = ?", date);
        }

        public void UpdateToVersion12()
        {
            connection.CreateTable<FavouriteRadiosTable>();
        }

        public async Task<List<SongsTable>> GetSongsTableAsync()
        {
            return await connectionAsync.Table<SongsTable>().ToListAsync();
        }

        public async Task<List<SongsTable>> GetSongsTableFromDirectory(string dir)
        {
            return await connectionAsync.Table<SongsTable>().Where(s => s.DirectoryName.Equals(dir)).ToListAsync();
        }

        public async Task UpdateSongsTableAsync(List<SongsTable> songs)
        {
            await connectionAsync.UpdateAllAsync(songs);
        }
    }
}
