using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerUWP.Extensions
{
    public class MyAppExtensionInfo : UWPMusicPlayerExtensions.Client.AppExtensionInfo
    {
        public int Priority { get; set; }
        public bool Enabled { get; set; }
        public string Url { get; set; }
    }

    public class MyAvailableExtension : UWPMusicPlayerExtensions.Store.AvailableExtension
    {

    }

    public interface IAppExtensionsHelper
    {
        Task<List<MyAppExtensionInfo>> GetExtensionsInfo();
    }
}
