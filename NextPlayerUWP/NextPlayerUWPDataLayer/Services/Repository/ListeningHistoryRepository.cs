using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Tables;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace NextPlayerUWPDataLayer.Services.Repository
{
    public class ListeningHistoryRepository : IRepository<HistTrack>
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
                connection.GetTableInfo(nameof(HistoryTable));
            }
            catch (Exception ex)
            {
                connection.CreateTable<HistoryTable>();
            }
        }

        public async Task Add(HistTrack entity)
        {
            System.Diagnostics.Debug.WriteLine("History {0} {1} {2}", entity.SongId, entity.PlaybackDuration.ToString(), entity.DatePlayed.ToString());
            await connectionAsync.InsertAsync(entity);
        }

        public async Task Delete(HistTrack entity)
        {
            await connectionAsync.DeleteAsync(entity);
        }

        public async Task Update(HistTrack entity)
        {
            await connectionAsync.InsertAsync(entity);
        }

        public async Task<HistTrack> GetById(int id)
        {
            var h = await connectionAsync.Table<HistoryTable>().Where(e => e.HistId == id).FirstOrDefaultAsync();
            return new HistTrack()
            {
                DatePlayed = h.DatePlayed,
                histId = h.HistId,
                PlaybackDuration = h.PlaybackDuration,
                SongId = h.SongId
            };
        }

        public async Task<IEnumerable<HistTrack>> GetAll()
        {
            var list = await connectionAsync.Table<HistoryTable>().ToListAsync();
            var result = new List<HistTrack>();
            foreach(var item in list)
            {
                result.Add(new HistTrack()
                {
                    DatePlayed = item.DatePlayed,
                    histId = item.HistId,
                    PlaybackDuration=item.PlaybackDuration,
                    SongId = item.SongId
                });
            }
            return result;
        }

        public async Task<IEnumerable<HistTrack>> Get(Expression<Func<HistTrack, bool>> predicate)
        {
            var list = await connectionAsync.Table<HistoryTable>().ToListAsync();
            var result = new List<HistTrack>();
            //foreach(var item in list.Where<HistoryTable>(predicate))
            //{
            //    result.Add(item);
            //}
            return result;
        }
    }
}
