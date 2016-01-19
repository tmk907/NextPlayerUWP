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
        }

        public void DeleteDatabase()
        {
            connection.DropTable<SongsTable>();
            connection.DropTable<PlainPlaylistEntryTable>();
            connection.DropTable<PlainPlaylistsTable>();
            connection.DropTable<SmartPlaylistEntryTable>();
            connection.DropTable<SmartPlaylistsTable>();
            connection.DropTable<NowPlayingTable>();
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

        public void UpdateFolder(IEnumerable<SongData> newSongs, List<int> available, string directoryName)
        {
            var old = connection.Table<SongsTable>().Where(s => s.DirectoryName.Equals(directoryName)).ToList();
            foreach(var song in old)
            {
                connection.Execute("UPDATE SongsTable SET IsAvailable = 0 WHERE SongId = ?", song.SongId);
            }
            foreach(var id in available)
            {
                connection.Execute("UPDATE SongsTable SET IsAvailable = 0 WHERE SongId = ?", id);
            }
            List<SongsTable> list = new List<SongsTable>();
            foreach (var item in newSongs)
            {
                list.Add(CreateSongsTable(item));
            }
            connection.InsertAll(list);
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
                Duration = song.Duration,
                Disc = song.Tag.Disc,
                DiscCount = song.Tag.DiscCount,
                Filename = song.Filename,
                FileSize = (long)song.FileSize,
                FirstArtist = song.Tag.FirstArtist,
                FirstComposer = song.Tag.FirstComposer,
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
    }
}
