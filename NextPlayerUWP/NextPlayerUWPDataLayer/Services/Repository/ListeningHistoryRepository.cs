using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Tables;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Services.Repository
{
    public class ListeningHistoryRepository
    {
        private SQLiteAsyncConnection connectionAsync;
        public string DBFilePath { get { return Path.Combine(ApplicationData.Current.LocalFolder.Path, AppConstants.FileNameHistoryDB); } }


        public ListeningHistoryRepository()
        {
            CreateDatabaseIfNotExist();

            connectionAsync = new SQLiteAsyncConnection(DBFilePath, true);
        }

        private void CreateDatabaseIfNotExist()
        {
            var connection = new SQLiteConnection(DBFilePath, true);
            connection.BusyTimeout = TimeSpan.FromSeconds(10);
            try
            {
                var a = connection.Table<ListeningHistoryTable>().ToList();
            }
            catch (SQLiteException ex)
            {
                connection.CreateTable<ListeningHistoryTable>();
            }
        }

        public async Task Add(ListenedSong song)
        {
            System.Diagnostics.Debug.WriteLine("History {0} {1} {2}", song.SongId, song.PlaybackDuration.ToString(), song.DatePlayed.ToString());
            await connectionAsync.InsertAsync(new ListeningHistoryTable()
            {
                DatePlayed = song.DatePlayed,
                PlaybackDuration = song.PlaybackDuration,
                SongId = song.SongId,
            });
        }

        //?
        public async Task<ListenedSong> GetById(int id)
        {
            var h = await connectionAsync.Table<ListeningHistoryTable>().Where(e => e.EventId == id).FirstOrDefaultAsync();
            return Create(h);
        }

        //?
        public async Task<ListenedSong> GetBySongId(int id)
        {
            var h = await connectionAsync.Table<ListeningHistoryTable>().Where(e => e.SongId == id).FirstOrDefaultAsync();
            return Create(h);
        }

        public async Task<IEnumerable<ListenedSong>> GetAll()
        {
            var list = await connectionAsync.Table<ListeningHistoryTable>().ToListAsync();
            var result = new List<ListenedSong>();
            foreach(var item in list)
            {
                result.Add(Create(item));
            }
            return result;
        }

        public async Task<IEnumerable<ListenedSong>> GetFromTo(DateTime from, DateTime to)
        {
            var list = await connectionAsync.Table<ListeningHistoryTable>().Where(e => e.DatePlayed >= from && e.DatePlayed <= to).OrderBy(e => e.DatePlayed).ToListAsync();
            var result = new List<ListenedSong>();
            foreach (var item in list)
            {
                result.Add(Create(item));
            }
            return result;
        }

        private static ListenedSong Create(ListeningHistoryTable table)
        {
            return new ListenedSong()
            {
                DatePlayed=table.DatePlayed,
                EventId = table.EventId,
                PlaybackDuration = table.PlaybackDuration,
                SongId = table.SongId,
            };
        }
    }
}
