using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerUWP.Extensions
{
    public class MyAppExtensionInfo : UWPMusicPlayerExtensions.AppExtensionInfo
    {
        public int Priority { get; set; }
        public bool Enabled { get; set; }
    }

    public interface IAppExtensionsHelper
    {
        Task<List<MyAppExtensionInfo>> GetExtensionsInfo();
    }
}
