using SQLite;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("CloudAccountsTable")]
    public class CloudAccountsTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int Type { get; set; }
        public string Token { get; set; }
    }
}