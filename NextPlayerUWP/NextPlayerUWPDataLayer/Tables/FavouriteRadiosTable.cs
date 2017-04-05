using SQLite;
using System;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("FavouriteRadiosTable")]
    public class FavouriteRadiosTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int RadioId { get; set; }
        public int RadioType { get; set; }
        public string Name { get; set; }
    }
}
