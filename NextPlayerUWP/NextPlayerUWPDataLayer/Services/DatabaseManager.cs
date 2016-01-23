using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLitePCL;
using SQLite;
using System.IO;
using NextPlayerUWPDataLayer.Constants;
using Windows.Storage;
using NextPlayerUWPDataLayer.Tables;
using NextPlayerUWPDataLayer.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace NextPlayerUWPDataLayer.Services
{
    public class DatabaseManager
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
            connectionAsync = new SQLiteAsyncConnection(Path.Combine(ApplicationData.Current.LocalFolder.Path, AppConstants.DBFileName), true);
            connection = new SQLiteConnection(Path.Combine(ApplicationData.Current.LocalFolder.Path, AppConstants.DBFileName), true);
        }

        private SQLiteAsyncConnection connectionAsync;
        private SQLiteConnection connection;
        
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

        public void CreateDatabase()
        {
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

        public void UpdateFolder(List<SongData> newSongs, List<int> available, string directoryName)
        {
            var old = connection.Table<SongsTable>().Where(s => s.DirectoryName.Equals(directoryName)).ToList();

            var notAvailable = old.Select(s => s.SongId).Except(available);
            foreach(var id in notAvailable)
            {
                connection.Execute("UPDATE SongsTable SET IsAvailable = 0 WHERE SongId = ?", id);
            }

            var query = connection.Table<SongsTable>().Where(s => notAvailable.Contains(s.SongId));
            TimeSpan removedDuration = TimeSpan.Zero;
            foreach(var item in query)
            {
                removedDuration += item.Duration;
            }
            TimeSpan newDuration = TimeSpan.Zero;
            foreach (var item in newSongs)
            {
                newDuration += item.Duration;
            }

            var query2 = connection.Table<FoldersTable>().Where(f => f.Directory.Equals(directoryName)).ToList();
            if (query2.Count == 1)
            {
                connection.Execute("UPDATE FoldersTable SET SongsNumber = ?, Duration = ? WHERE FolderId = ?", newSongs.Count + available.Count, query2.FirstOrDefault().Duration - removedDuration + newDuration, query2.FirstOrDefault().FolderId);
            }
            else
            {
                connection.Insert(new FoldersTable()
                {
                    Directory = directoryName,
                    Duration = newDuration,
                    Folder = Path.GetFileName(directoryName),
                    SongsNumber = newSongs.Count,
                });
            }

            List<SongsTable> list = new List<SongsTable>();
            foreach (var item in newSongs)
            {
                list.Add(CreateSongsTable(item));
            }
            connection.InsertAll(list);
        }

        //Albums, Artists, Genres
        public async Task UpdateTables()
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            var query = songsConnection.ToList();
            s.Stop();
            var l = s.ElapsedMilliseconds;
            Debug.WriteLine("songs to list " + l);
            s.Restart();

            List<SongsTable> asongs = new List<SongsTable>();
            foreach(var song in query)
            {
                if (song.Artists.Contains(";"))
                {
                    string[] artists = song.Artists.Split(new char[] { ';' },StringSplitOptions.RemoveEmptyEntries);
                    var tempSong = song;
                    foreach (var a in artists)
                    {
                        tempSong.Artists = a;
                        asongs.Add(tempSong);
                    }
                    if (song.Artists.Equals(song.FirstArtist)) Debug.WriteLine("1 noteq eq" + song.Artists + " /" + song.FirstArtist);
                }
                else
                {
                    asongs.Add(song);
                    if (!song.Artists.Equals(song.FirstArtist)) Debug.WriteLine("2 eq noteq " + song.Artists + "/" + song.FirstArtist);
                }
            }
            var groupedArtists = query.GroupBy(a => a.Artists);
            connection.DeleteAll<ArtistsTable>();
            List<ArtistsTable> artistsTable = new List<ArtistsTable>();
            foreach(var item in groupedArtists)
            {
                int albumsNumber = item.GroupBy(a => a.Album).Count();
                TimeSpan duration = TimeSpan.Zero;
                int songsNumber = 0;
                foreach(var song in item)
                {
                    duration += song.Duration;
                    songsNumber++;
                }
                artistsTable.Add(new ArtistsTable()
                {
                    AlbumsNumber = albumsNumber,
                    Artist = item.FirstOrDefault().Artists,
                    Duration = TimeSpan.Zero,
                    SongsNumber = songsNumber,
                    
                });
            }
            await connectionAsync.InsertAllAsync(artistsTable);
            s.Stop();
            l = s.ElapsedMilliseconds;
            Debug.WriteLine("artists " + l);
            s.Restart();
            var groupedAlbums = query.GroupBy(a => a.Album);
            connection.DeleteAll<AlbumsTable>();
            List<AlbumsTable> albumsTable = new List<AlbumsTable>();
            foreach(var item in groupedAlbums)
            {
                string albumArtist = "";
                foreach(var song in item)
                {
                    if (song.AlbumArtist != "")
                    {
                        albumArtist = song.AlbumArtist;
                        break;
                    }
                }
                albumsTable.Add(new AlbumsTable()
                {
                    Album = item.FirstOrDefault().Album,
                    AlbumArtist = albumArtist,
                    Duration = TimeSpan.Zero,
                    SongsNumber = item.Count(),
                });
            }
            await connectionAsync.InsertAllAsync(albumsTable);
            s.Stop();
            l = s.ElapsedMilliseconds;
            Debug.WriteLine("albums " + l);
            s.Restart();
            var groupedGenres = query.GroupBy(g => g.Genre);
            List<GenresTable> genresTable = new List<GenresTable>();
            connection.DeleteAll<GenresTable>();
            foreach (var item in groupedGenres)
            {
                genresTable.Add(new GenresTable()
                {
                    Duration = TimeSpan.Zero,
                    Genre = item.FirstOrDefault().Genre,
                    SongsSumber = item.Count(),
                });
            }
            await connectionAsync.InsertAllAsync(genresTable);
            s.Stop();
            l = s.ElapsedMilliseconds;
            Debug.WriteLine("update tables " + l);
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
            string dir = Path.GetDirectoryName(song.Path);
            string name = Path.GetFileName(dir);
            if (name == "") name = dir;
            return new SongsTable()
            {
                Album = song.Tag.Album,
                AlbumArtist = song.Tag.AlbumArtist,
                Artists = song.Tag.Artists,
                Bitrate = song.Bitrate,
                Comment = song.Tag.Comment,
                Composers = song.Tag.Composers,
                Conductor = song.Tag.Conductor,
                DateAdded = song.DateAdded,
                DirectoryName = dir,
                Duration = song.Duration,
                Disc = song.Tag.Disc,
                DiscCount = song.Tag.DiscCount,
                Filename = song.Filename,
                FileSize = (long)song.FileSize,
                FirstArtist = song.Tag.FirstArtist,
                FirstComposer = song.Tag.FirstComposer,
                FolderName = name,
                Genre = song.Tag.Genre,
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
            s.Path = npSong.Path;
            s.Position = npSong.Position;
            s.SongId = npSong.SongId;
            s.Title = npSong.Title;
            return s;
        }

        #region Get

        

        public async Task<ObservableCollection<SongItem>> GetSongItemsAsync()
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var result = await songsConnectionAsync.OrderBy(s => s.Title).ToListAsync();
            foreach (var item in result)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromAlbumAsync(string album)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var result = await songsConnectionAsync.Where(a=>a.Album.Equals(album)).OrderBy(s => s.Title).ToListAsync();
            foreach (var item in result)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromArtistAsync(string artist)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var result = await songsConnectionAsync.Where(
                a=>
                    a.Artists.Equals(artist) || 
                    a.Artists.StartsWith(artist+";") || 
                    a.Artists.Contains(";"+artist+";") || 
                    a.Artists.EndsWith(";"+artist) || 
                    a.Artists.EndsWith(";"+artist+";")
                ).OrderBy(s => s.Title).ToListAsync();
            foreach (var item in result)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromFolderAsync(string directory)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var result = await songsConnectionAsync.Where(f=>f.DirectoryName.Equals(directory)).OrderBy(s => s.Title).ToListAsync();
            foreach (var item in result)
            {
                songs.Add(CreateSongItem(item));
            }
            return songs;
        }

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromGenreAsync(string genre)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();

            try
            {
                var result = await songsConnectionAsync.Where(
                        g =>
                            g.Genre.Equals(genre) ||
                            g.Genre.StartsWith(genre + ";") ||
                            g.Genre.Contains(";" + genre + ";") ||
                            g.Genre.EndsWith(";" + genre) ||
                            g.Genre.EndsWith(";" + genre + ";")
                        ).OrderBy(s => s.Title).ToListAsync();
                foreach (var item in result)
                {
                    songs.Add(CreateSongItem(item));
                }
            }
            catch (Exception ex)
            {

                
            }
            return songs;
        }

        public int PrepPlain()
        {
            var newplaylist = new PlainPlaylistsTable
            {
                Name = "nowa playlista",
            };

            connection.Insert(newplaylist);

           
            return newplaylist.PlainPlaylistId;
        }

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromPlainPlaylistAsync(int id)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var query = await connectionAsync.QueryAsync<SongsTable>("SELECT * FROM PlainPlaylistEntryTable INNER JOIN SongsTable ON PlainPlaylistEntryTable.SongId = SongsTable.SongId WHERE PlaylistId = ? AND SongsTable.IsAvailable = 1 ORDER BY PlainPlaylistEntryTable.Place", id);
            var result = query.ToList();
            foreach (var item in result)
            {
                songs.Add(CreateSongItem(item));
            }
            
            return songs;
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

        //todo
        public async Task<ObservableCollection<PlaylistItem>> GetPlaylistItemsAsync()
        {
            ObservableCollection<PlaylistItem> playlists = new ObservableCollection<PlaylistItem>();

            return playlists;
        }
        #endregion

        #region Insert

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

            await connectionAsync.InsertAsync(newEntry);
        }

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

        

        #endregion

        private static SongItem CreateSongItem(SongsTable q)
        {
            return new SongItem(q.Album, q.Artists, q.Composers, q.Duration, q.Path, (int)q.Rating, q.SongId, q.Title, (int)q.Track, q.Year);
        }
    }
}
