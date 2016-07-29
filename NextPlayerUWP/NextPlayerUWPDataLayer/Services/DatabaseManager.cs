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
            connection.BusyTimeout = TimeSpan.FromSeconds(5);
        }

        private SQLiteAsyncConnection connectionAsync;
        private SQLiteConnection connection;
        
        //public string OldDBFilePath { get { return Path.Combine(ApplicationData.Current.LocalFolder.Path, AppConstants.DBFileName); } }
        public string DBFilePath { get { return Path.Combine(ApplicationData.Current.LocalFolder.Path, AppConstants.DBFileName); } }

        private TableQuery<SongsTable> songsConnection
        {
            get
            {
                return connection.Table<SongsTable>().Where(available => available.IsAvailable > 0);
            }
        }

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
            connection.CreateTable<CachedScrobble>();
            connection.CreateTable<ImportedPlaylistsTable>();
            connection.CreateTable<FutureAccessTokensTable>();
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
            connection.DropTable<ArtistsTable>();
            connection.DropTable<CachedScrobble>();
            connection.DropTable<ImportedPlaylistsTable>();
            connection.DropTable<FutureAccessTokensTable>();
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

        public async Task UpdateFolderAsync2(string directory, List<SongsTable> oldSongs, List<SongData> newSongs, IEnumerable<SongsTable> toNotAvailable, IEnumerable<SongsTable> changed)
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

        public async Task UpdateFolderAsync(List<SongData> newSongs, List<int> toAvailable, List<int> oldAvailable, string directoryName)
        {
            var old = await connectionAsync.Table<SongsTable>().Where(s => s.DirectoryName.Equals(directoryName)).ToListAsync();

            var notAvailable = old.Select(s => s.SongId).Except(toAvailable).Except(oldAvailable);
            foreach(var id in notAvailable)
            {
                connection.Execute("UPDATE SongsTable SET IsAvailable = 0 WHERE SongId = ?", id);
            }

            if (newSongs.Count == 0 && toAvailable.Count == 0 && oldAvailable.Count == 0)
            {
                //delete folder
                connection.Execute("DELETE FROM FoldersTable WHERE Directory = ?", directoryName);
                return;
            }

            if(newSongs.Count == 0 && toAvailable.Count == 0)//&& oldAvailable == 0
            {
                return;
            }

            foreach (var id in toAvailable)
            {
                connection.Execute("UPDATE SongsTable SET IsAvailable = 1 WHERE SongId = ?", id);
            }

            //Caculate duration of songs in database
            var queryOldAvailableSongs = old.Where(s => (toAvailable.Contains(s.SongId) || oldAvailable.Contains(s.SongId)));
            TimeSpan oldAvailableSongsDuration = TimeSpan.Zero;
            foreach(var item in queryOldAvailableSongs)
            {
                oldAvailableSongsDuration += item.Duration;
            }
            //Calculate duration of new songs and date of last added
            TimeSpan newDuration = TimeSpan.Zero;
            DateTime lastAdded = DateTime.MinValue;
            foreach (var item in newSongs)
            {
                if (lastAdded.Ticks < item.DateAdded.Ticks) lastAdded = item.DateAdded;
                newDuration += item.Duration;
            }

            //Update folder informations or create new entry
            var query2 = await connectionAsync.Table<FoldersTable>().Where(f => f.Directory.Equals(directoryName)).ToListAsync();
            if (query2.Count == 1)
            {
                connection.Execute("UPDATE FoldersTable SET SongsNumber = ?, Duration = ? WHERE FolderId = ?", newSongs.Count + toAvailable.Count + oldAvailable.Count, oldAvailableSongsDuration + newDuration, query2.FirstOrDefault().FolderId);
            }
            else
            { 
                if (newSongs.Count > 0)
                {
                    await connectionAsync.InsertAsync(new FoldersTable()
                    {
                        Directory = directoryName,
                        Duration = newDuration,
                        Folder = Path.GetFileName(directoryName),
                        LastAdded = lastAdded,
                        SongsNumber = newSongs.Count,
                    });
                }
            }

            //Add new songs to database
            List<SongsTable> list = new List<SongsTable>();
            foreach (var item in newSongs)
            {
                list.Add(CreateSongsTable(item));
            }
            await connectionAsync.InsertAllAsync(list);
        }

        //Albums, Artists, Genres
        public async Task UpdateTables()
        {
            var songsList = songsConnection.ToList();

            await connectionAsync.ExecuteAsync("UPDATE ArtistsTable SET SongsNumber = 0");
            await connectionAsync.ExecuteAsync("UPDATE AlbumsTable SET SongsNumber = 0");
            await connectionAsync.ExecuteAsync("UPDATE GenresTable SET SongsNumber = 0");

            #region artists
            List <SongsTable> asongs = new List<SongsTable>();
            foreach(var song in songsList)
            {
                if (song.Artists.Contains("; "))
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

        private async Task ClearEntryWithNoSongs()
        {
            //przed update tables ustawic songscount na 0 w tabelach album, artist, genre
            //nowe wiersze beda mialy songcount>0, updatowane tez
            //wiersze stare nie updatowane beda mialy songscount == 0
            //przeszukac album, artist, genre z songscount==0 i usunac
            var albums = await connectionAsync.Table<AlbumsTable>().Where(a => a.SongsNumber > 0).ToListAsync();
            var artists = await connectionAsync.Table<ArtistsTable>().Where(a => a.SongsNumber > 0).ToListAsync();
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
                connection.DeleteAll<GenresTable>();
                connection.InsertAll(genres);
            });
        }

        public Dictionary<string, Tuple<int, int>> GetFilePaths()
        {
            Dictionary<string, Tuple<int, int>> dict = new Dictionary<string, Tuple<int, int>>();
            var result = connection.Table<SongsTable>().ToList();
            foreach (var x in result)
            {

                dict.Add(x.Path, new Tuple<int, int>(x.IsAvailable, x.SongId));
            }
            return dict;
        }

        private static SongsTable CreateSongsTable(SongData song)
        {
            //if (song.Path == c:\) (root directory) GetDirectoryName == null
            string dir = Path.GetDirectoryName(song.Path) ?? song.Path; 
            string folderName = Path.GetFileName(dir);
            if (folderName == "") folderName = dir;
            return new SongsTable()
            {
                Album = song.Tag.Album,
                AlbumArtist = song.Tag.AlbumArtist,
                AlbumArt = song.AlbumArtPath ?? "",
                Artists = song.Tag.Artists,
                Bitrate = song.Bitrate,
                Comment = song.Tag.Comment,
                Composers = song.Tag.Composers,
                Conductor = song.Tag.Conductor,
                DateAdded = song.DateAdded,
                DateModified = song.DateModified,
                DirectoryName = dir,
                Duration = song.Duration,
                Disc = song.Tag.Disc,
                DiscCount = song.Tag.DiscCount,
                Filename = song.Filename,
                FileSize = (long)song.FileSize,
                FirstArtist = song.Tag.FirstArtist,
                FirstComposer = song.Tag.FirstComposer,
                FolderName = folderName,
                Genres = song.Tag.Genres,
                IsAvailable = song.IsAvailable,
                LastPlayed = song.LastPlayed,
                Lyrics = song.Tag.Lyrics,
                Path = song.Path,
                PlayCount = song.PlayCount,
                Rating = song.Tag.Rating,
                SongId = song.SongId,
                Title = song.Tag.Title,
                Track = song.Tag.Track,
                TrackCount = song.Tag.TrackCount,
                Year = song.Tag.Year
            };
        }

        private static NowPlayingSong CreateNowPlayingSong(NowPlayingTable npSong)
        {
            NowPlayingSong s = new NowPlayingSong();
            s.Artist = npSong.Artist;
            s.Album = npSong.Album;
            s.Path = npSong.Path;
            s.ImagePath = npSong.ImagePath;
            s.Position = npSong.Position;
            s.SongId = npSong.SongId;
            s.SourceType = (MusicSource)npSong.SourceType;
            s.Title = npSong.Title;
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
                Diagnostics.Logger.Save("GetSongData null id=" + songId);
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
                Diagnostics.Logger.Save("GetSongDataAsync null id=" + songId);

                s = CreateEmptySongData();//!
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

        public async Task<ObservableCollection<SongItem>> GetSongItemsAsync()
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var result = await songsConnectionAsync.OrderBy(s => s.Title).ToListAsync();
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
            var list = result.OrderBy(s => s.Track).ThenBy(t => t.Title);
            foreach (var item in list)
            {
                songs.Add(new SongItem(item));
            }
            return songs;
        }

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromArtistAsync(string artist)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            try
            {
                List<SongsTable> list = await connectionAsync.QueryAsync<SongsTable>("SELECT * FROM SongsTable WHERE Artists = ? OR Artists LIKE ? OR Artists LIKE ? OR Artists LIKE ?", artist, artist + "; %", "%; " + artist, "%; " + artist + "; %");

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
            var query = await connectionAsync.QueryAsync<SongsTable>("SELECT * FROM PlainPlaylistEntryTable INNER JOIN SongsTable ON PlainPlaylistEntryTable.SongId = SongsTable.SongId WHERE PlaylistId = ? AND SongsTable.IsAvailable = 1 ORDER BY PlainPlaylistEntryTable.Place", id);
            var result = query.ToList();
            foreach (var item in result)
            {
                songs.Add(new SongItem(item));
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

                foreach (var song in q)
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
                if (type == MusicSource.LocalFile || type == MusicSource.LocalNotMusicLibrary)
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
            string name;
            foreach (var item in query1) //default smart playlists
            {
                if (ids.ContainsKey(item.SmartPlaylistId))
                {
                    name = ids[item.SmartPlaylistId];
                    playlists.Add(new PlaylistItem(item.SmartPlaylistId, true, name));
                }
            }
            var query2 = query1.OrderBy(p => p.Name);
            foreach (var item in query2) //user smart playlists
            {
                if (!ids.ContainsKey(item.SmartPlaylistId))
                {
                    playlists.Add(new PlaylistItem(item.SmartPlaylistId, true, item.Name));

                }
            }

            var query = await connectionAsync.Table<PlainPlaylistsTable>().OrderBy(p => p.Name).ToListAsync();
            foreach (var item in query)
            {
                playlists.Add(new PlaylistItem(item.PlainPlaylistId, false, item.Name));
            }

            return playlists;
        }


        public async Task<ObservableCollection<PlaylistItem>> GetPlainPlaylistsAsync()
        {
            var query = await connectionAsync.Table<PlainPlaylistsTable>().OrderBy(p => p.Name).ToListAsync();
            ObservableCollection<PlaylistItem> list = new ObservableCollection<PlaylistItem>();
            foreach (var item in query)
            {
                list.Add(new PlaylistItem(item.PlainPlaylistId, false, item.Name));
            }
            return list;
        }

        public async Task<PlaylistItem> GetPlainPlaylistAsync(int id)
        {
            var list = await connectionAsync.Table<PlainPlaylistsTable>().Where(s => s.PlainPlaylistId.Equals(id)).ToListAsync();
            var p = list.FirstOrDefault();
            return new PlaylistItem(id, false, p.Name);
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


        public async Task<List<string>> GetImportedPlaylistPathsAsync()
        {
            List<string> list = new List<string>();
            var query = await connectionAsync.Table<ImportedPlaylistsTable>().ToListAsync();
            foreach(var q in query)
            {
                list.Add(q.Path);
            }
            return list;
        }

        public async Task<List<ImportedPlaylistsTable>> GetImportedPlaylists()
        {
            List<ImportedPlaylistsTable> list = await connectionAsync.Table<ImportedPlaylistsTable>().ToListAsync();
            return list;
        }

        #endregion

        #region Insert

        public async Task AddToPlaylist(int playlistId, System.Linq.Expressions.Expression<Func<SongsTable, bool>> condition, Func<SongsTable, object> sort)
        {
            var query = await songsConnectionAsync.Where(condition).ToListAsync();
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
            var newSong = CreateSongsTable(songData);
            await connectionAsync.InsertAsync(newSong);
            return newSong.SongId;
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
            };

            connection.Insert(newplaylist);
            return newplaylist.PlainPlaylistId;
        }

        public async Task InsertPlainPlaylistEntryAsync(int _playlistId, int _songId, int _place)
        {
            var newEntry = new PlainPlaylistEntryTable
            {
                PlaylistId = _playlistId,
                SongId = _songId,
                Place = _place,
            };

            await connectionAsync.InsertAsync(newEntry);//error
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

        public int InsertImportedPlaylist(string name, string path, int plainId)
        {
            var p = new ImportedPlaylistsTable() {
                Name = name,
                Path = path,
                PlainPlaylistId = plainId
            };
            return connection.Insert(p);
        }

        #endregion

        #region Delete

        public void DeleteSmartPlaylistEntry(int primaryId)//! async?
        {
            connection.Delete<SmartPlaylistEntryTable>(primaryId);
        }

        public async Task DeleteSmartPlaylistEntries(int playlistId)
        {
            var items = await connectionAsync.Table<SmartPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(playlistId)).ToListAsync();
            foreach (var item in items)
            {
                DeleteSmartPlaylistEntry(item.Id);
            }
        }

        public async Task DeleteSmartPlaylistAsync(int id)
        {
            await DeleteSmartPlaylistEntries(id);
            var list =  await connectionAsync.Table<SmartPlaylistsTable>().Where(p => p.SmartPlaylistId.Equals(id)).ToListAsync();
            var playlist = list.FirstOrDefault();
            await connectionAsync.DeleteAsync(playlist);
        }


        public async Task DeletePlainPlaylistEntryByIdAsync(int songId)
        {
            var item = await connectionAsync.Table<PlainPlaylistEntryTable>().Where(s => s.SongId == songId).ToListAsync();
            await connectionAsync.DeleteAsync(item.FirstOrDefault());
        }

        public void DeletePlainPlaylistEntry(int primaryId)
        {
            connection.Delete<PlainPlaylistEntryTable>(primaryId);           
        }

        public async Task DeletePlainPlaylistAsync(int playlistId)
        {
            var items = await connectionAsync.Table<PlainPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(playlistId)).ToListAsync();
            //foreach (var item in items)
            //{
            //    DeletePlainPlaylistEntry(item.Id);
            //}

            await connectionAsync.ExecuteAsync("DELETE FROM PlainPlaylistEntryTable WHERE PlaylistId = ?", playlistId);

            var list2 = connection.Table<ImportedPlaylistsTable>().Where(s => s.PlainPlaylistId == playlistId).ToList();
            if (list2.Count > 0)
            {
                foreach (var item in list2)
                {
                    connection.Delete(item);
                }
            }

            var list = await connectionAsync.Table<PlainPlaylistsTable>().Where(p => p.PlainPlaylistId.Equals(playlistId)).ToListAsync();
            var playlist = list.FirstOrDefault();
            await connectionAsync.DeleteAsync(playlist);
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

        public async Task DeleteImportedPlaylist(ImportedPlaylistsTable table)
        {
            await connectionAsync.DeleteAsync(table);
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

        public async Task UpdateImportedPlaylist(ImportedPlaylistsTable table)
        {
            await connectionAsync.UpdateAsync(table);
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

        private static SongsTable CreateCopy(SongsTable s)
        {
            return new SongsTable()
            {
                Album = s.Album,
                AlbumArtist = s.AlbumArtist,
                AlbumArt = s.AlbumArt,
                Artists = s.Artists,
                Bitrate = s.Bitrate,
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
                DateAdded = q.DateAdded,
                DateModified = q.DateModified,
                Duration = q.Duration,
                Filename = q.Filename,
                FileSize = (ulong)q.FileSize,
                IsAvailable = q.IsAvailable,
                LastPlayed = q.LastPlayed,
                Path = q.Path,
                PlayCount = q.PlayCount,
                SongId = q.SongId,
                Tag = tag
            };
            return s;
        }

        private static SongData CreateEmptySongData()
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
                DateAdded = DateTime.Now,
                DateModified = DateTime.UtcNow,
                Duration = TimeSpan.Zero,
                Filename = "",
                FileSize = (ulong)0,
                IsAvailable = 0,
                LastPlayed = DateTime.Now,
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
            connection.DropTable<ImportedPlaylistsTable>();

            connection.CreateTable<PlainPlaylistsTable>();
            connection.CreateTable<PlainPlaylistEntryTable>();
            connection.CreateTable<SongsTable>();
            connection.CreateTable<NowPlayingTable>();
            connection.CreateTable<FoldersTable>();
            connection.CreateTable<GenresTable>();
            connection.CreateTable<AlbumsTable>();
            connection.CreateTable<ArtistsTable>();
            connection.CreateTable<CachedScrobble>();
            connection.CreateTable<ImportedPlaylistsTable>();
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
