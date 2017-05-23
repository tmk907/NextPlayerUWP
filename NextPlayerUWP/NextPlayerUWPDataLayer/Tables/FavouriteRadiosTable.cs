using SQLite;
using System;

namespace NextPlayerUWPDataLayer.Tables
{
    [Table("RadiosTable")]
    public class RadiosTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int RadioId { get; set; }
        public int RadioType { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public bool IsFavourite { get; set; }
    }
}
