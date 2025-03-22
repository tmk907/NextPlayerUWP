using SQLite;

namespace NextPlayerWinUIDataLayer.Tables
{
    [Table("FutureAccessTokensTable")]
    public class FutureAccessTokensTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string FilePath { get; set; }
        public string Token { get; set; }
        public bool IsFile { get; set; }
    }
}
