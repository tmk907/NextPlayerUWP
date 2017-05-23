using System.Collections.Generic;

namespace NextPlayerUWPDataLayer.Model
{
    public class GroupList : List<object>
    {
        public object Key { get; set; }
        public object Header { get; set; } 
    }
}
