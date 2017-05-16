using System.Collections.Generic;
using System.Threading.Tasks;
using UWPMusicPlayerExtensions.Client;
using UWPMusicPlayerExtensions.Enums;

namespace NextPlayerUWP.Extensions
{
    public class LyricsExtensions : BaseExtensionsHelper, IAppExtensionsHelper
    {
        private ExtensionClientHelper helper;
        public LyricsExtensions(ExtensionClientHelper helper)
        {
            this.helper = helper;
            appExtensionName = UWPMusicPlayerExtensions.ExtensionsNames.Lyrics;
        }

        public async Task<List<MyAppExtensionInfo>> GetExtensionsInfo()
        {
            var available = await helper.GetAvailableExtensions();
            List<MyAppExtensionInfo> list = new List<MyAppExtensionInfo>();

            foreach(var item in available)
            {
                list.Add(new MyAppExtensionInfo()
                {
                    DisplayName = item.DisplayName,
                    Enabled = true,
                    Id = item.Id,
                    PackageName = item.PackageName,
                    Priority = -1,
                    ServiceName = item.ServiceName,
                    Type = ExtensionTypes.Lyrics
                });
            }

            ApplyPriorities(list);
            return list;
        }
    }
}
