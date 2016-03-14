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
            connectionAsync = new SQLiteAsyncConnection(Path.Combine(ApplicationData.Current.LocalFolder.Path, AppConstants.DBFileName), true);
            connection = new SQLiteConnection(Path.Combine(ApplicationData.Current.LocalFolder.Path, AppConstants.DBFileName), true);
            connection.BusyTimeout = TimeSpan.FromSeconds(2);
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
            DateTime lastAdded = DateTime.MinValue;
            foreach (var item in newSongs)
            {
                if (lastAdded.Ticks < item.DateAdded.Ticks) lastAdded = item.DateAdded;
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
                    LastAdded = lastAdded,
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
            var groupedAlbums = songsList.GroupBy(a => a.Album);
            var aList = await connectionAsync.Table<AlbumsTable>().ToListAsync();
            var oldAlbums = aList.ToDictionary(l => l.Album);
            foreach (var group in groupedAlbums)
            {
                TimeSpan duration = TimeSpan.Zero;
                string albumArtist = "";
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
                    if (song.AlbumArtist != "")
                    {
                        albumArtist = song.AlbumArtist;
                    }
                }
                AlbumsTable oldAlbum;
                if (oldAlbums.TryGetValue(group.FirstOrDefault().Album, out oldAlbum))
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
                        Album = group.FirstOrDefault().Album,
                        AlbumArtist = albumArtist,
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
            var albums = await connectionAsync.Table<AlbumsTable>().Where(a => a.SongsNumber.Equals(0)).ToListAsync();
            var artists = await connectionAsync.Table<ArtistsTable>().Where(a => a.SongsNumber.Equals(0)).ToListAsync();
            var genres = await connectionAsync.Table<GenresTable>().Where(g => g.SongsNumber.Equals(0)).ToListAsync();
            foreach (var album in albums)
            {
                await connectionAsync.DeleteAsync(album);
            }
            foreach(var artist in artists)
            {
                await connectionAsync.DeleteAsync(artist);
            }
            foreach(var genre in genres)
            {
                await connectionAsync.DeleteAsync(genre);
            }
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
            s.Title = npSong.Title;
            return s;
        }

        #region Get

        public string GetLyrics(int id)
        {
            var list = connection.Table<SongsTable>().Where(s => s.SongId.Equals(id)).ToList();
            if (list.Count == 0) return "";
            else return list.FirstOrDefault().Lyrics;
        }

        public SongData GetSongData(int songId)
        {
            var q = connection.Table<SongsTable>().Where(e => e.SongId.Equals(songId)).FirstOrDefault();
            SongData s = CreateSongData(q);
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

        public ObservableCollection<SongItem> GetSongItemsFromNowPlaying()
        {
            var query = connection.Table<NowPlayingTable>().OrderBy(e => e.Position).ToList();
            ObservableCollection<SongItem> list = new ObservableCollection<SongItem>();
            foreach (var e in query)
            {
                var query2 = songsConnection.Where(x => x.SongId.Equals(e.SongId)).FirstOrDefault();
                if (query2 != null)
                {
                    list.Add(new SongItem(query2));
                }
            }
            return list;
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

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromAlbumAsync(string album)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();

            var result = await songsConnectionAsync.Where(a=>a.Album.Equals(album)).ToListAsync();
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

        public async Task<ObservableCollection<SongItem>> GetSongItemsFromFolderAsync(string directory)
        {
            ObservableCollection<SongItem> songs = new ObservableCollection<SongItem>();
            var result = await songsConnectionAsync.Where(f=>f.DirectoryName.Equals(directory)).OrderBy(s => s.Title).ToListAsync();
            foreach (var item in result)
            {
                songs.Add(new SongItem(item));
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

        public async Task<AlbumItem> GetAlbumItemAsync(string album)
        {
            var result = await connectionAsync.Table<AlbumsTable>().Where(a => a.Album.Equals(album)).ToListAsync();
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
            foreach (var item in query1)
            {
                if (ids.TryGetValue(item.SmartPlaylistId, out name))
                {
                    playlists.Add(new PlaylistItem(item.SmartPlaylistId, true, name));
                }
                else
                {
                    //collection.Add(new PlaylistItem(item.SmartPlaylistId, true, item.Name));
                }

            }
            var query2 = query1.OrderBy(p => p.Name);
            foreach (var item in query2)
            {
                if (ids.TryGetValue(item.SmartPlaylistId, out name))
                {
                    //playlists.Add(new PlaylistItem(item.SmartPlaylistId, true, loader.GetString(name)));
                }
                else
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
            return new PlaylistItem(id, true, p.Name);
        }

        #endregion

        public async Task AddToPlaylist(int playlistId, System.Linq.Expressions.Expression<Func<SongsTable, bool>> condition, Func<SongsTable,object> sort)
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
                    ImagePath = "",
                    Position = i,
                    SongId = item.SongId,
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

        public async Task InsertSmartPlaylistEntry(int _playlistId, string _item, string _comparison, string _value, string _operator)
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

        #endregion

        #region Delete

        public void DeleteSmartPlaylistEntry(int primaryId)//! async?
        {
            connection.Delete<SmartPlaylistEntryTable>(primaryId);
        }

        public async Task DeleteSmartPlaylistAsync(int id)
        {
            var items = await connectionAsync.Table<SmartPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(id)).ToListAsync();
            foreach (var item in items)
            {
                DeleteSmartPlaylistEntry(item.Id);
            }
            var list =  await connectionAsync.Table<SmartPlaylistsTable>().Where(p => p.SmartPlaylistId.Equals(id)).ToListAsync();
            var playlist = list.FirstOrDefault();
            await connectionAsync.DeleteAsync(playlist);
        }

        public void DeletePlainPlaylistEntry(int primaryId)
        {
            connection.Delete<PlainPlaylistEntryTable>(primaryId);

        }
        public async Task DeletePlainPlaylistAsync(int playlistId)
        {
            var items = await connectionAsync.Table<PlainPlaylistEntryTable>().Where(e => e.PlaylistId.Equals(playlistId)).ToListAsync();
            foreach (var item in items)
            {
                DeletePlainPlaylistEntry(item.Id);
            }
            var list = await connectionAsync.Table<PlainPlaylistsTable>().Where(p => p.PlainPlaylistId.Equals(playlistId)).ToListAsync();
            var playlist = list.FirstOrDefault();
            await connectionAsync.DeleteAsync(playlist);
        }

        

        #endregion

        public async Task UpdateAlbumItem(AlbumItem album)
        {
            await connectionAsync.ExecuteAsync("UPDATE AlbumsTable SET ImagePath = ? WHERE AlbumId = ?", album.ImagePath, album.AlbumId);
        }

        public async Task UpdateArtistItem(ArtistItem artist)
        {
            await connectionAsync.UpdateAsync(artist);
        }

        public async Task UpdateGenreItem(GenreItem genre)
        {
            await connectionAsync.UpdateAsync(genre);
        }

        public async Task UpdateSongData(SongData songData)
        {
            var song = CreateSongsTable(songData);
            await connectionAsync.UpdateAsync(song);
        }

        public async Task UpdateNowPlayingSong(SongData song)
        {
            await connectionAsync.ExecuteAsync("Update NowPlayingTable SET Title = ?, Artist = ?, Album = ? WHERE SongId = ?", song.Tag.Title, song.Tag.Artists, song.Tag.Album, song.SongId);
        }

        public async Task UpdateLyricsAsync(int id, string lyrics)
        {
            await connectionAsync.ExecuteAsync("UPDATE SongsTable SET Lyrics = ? WHERE SongId = ?", lyrics, id);
        }

        public async Task UpdatePlaylistName(int id, string name)
        {
            await connectionAsync.ExecuteAsync("UPDATE PlainPlaylistsTable SET Name = ? WHERE PlainPlaylistId = ?", name, id);
        }

        public string GetAlbumArt(string album)
        {
            var q = connection.Table<AlbumsTable>().Where(a => a.Album.Equals(album)).ToList();
            string imagepath = AppConstants.AssetDefaultAlbumCover;
            if (q.Count > 0)
            {
                imagepath = q.FirstOrDefault().ImagePath;
            }
            return imagepath;
        }

        private static SongsTable CreateCopy(SongsTable s)
        {
            return new SongsTable()
            {
                Album = s.Album,
                AlbumArtist = s.AlbumArtist,
                Artists = s.Artists,
                Bitrate = s.Bitrate,
                Comment = s.Comment,
                Composers = s.Composers,
                Conductor = s.Conductor,
                DateAdded = s.DateAdded,
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
                Bitrate = q.Bitrate,
                DateAdded = q.DateAdded,
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
    }
}
