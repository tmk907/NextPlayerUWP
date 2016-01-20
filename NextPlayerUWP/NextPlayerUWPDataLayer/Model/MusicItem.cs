using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWPDataLayer.Model
{
    public abstract class MusicItem
    {
        public abstract string GetParameter();
        protected const string separator = "!@#$%";
    }
}
