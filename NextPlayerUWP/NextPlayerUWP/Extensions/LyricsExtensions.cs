using System.Collections.Generic;
using System.Threading.Tasks;
using UWPMusicPlayerExtensions.Client;
using UWPMusicPlayerExtensions.Enums;
using UWPMusicPlayerExtensions.Store;

namespace NextPlayerUWP.Extensions
{
    public class LyricsExtensions : BaseExtensionsHelper, IAppExtensionsHelper
    {
        private ExtensionClientHelper helper;
        public LyricsExtensions(ExtensionClientHelper helper)
        {
            this.helper = helper;
            appExtensionName = ExtensionsNames.Lyrics;
        }

        public async Task<List<MyAppExtensionInfo>> GetExtensionsInfo()
        {
            var installed = await helper.GetInstalledExtensions();
            List<MyAppExtensionInfo> list = new List<MyAppExtensionInfo>();

            foreach(var item in installed)
            {
                list.Add(new MyAppExtensionInfo()
                {
                    DisplayName = item.DisplayName,
                    Enabled = true,
                    Id = item.Id,
                    PackageName = item.PackageName,
                    Priority = -1,
                    ServiceName = item.ServiceName,
                    Type = MusicPlayerExtensionTypes.Lyrics
                });
            }

            ApplyPriorities(list);
            return list;
        }

        public async Task<List<MyAvailableExtension>> GetAvailableExtensions()
        {
            List<MyAvailableExtension> list = new List<MyAvailableExtension>();
            AvailableExtensionsHelper ah = new AvailableExtensionsHelper();
            var available = await ah.GetAvailableExtensions();
            foreach(var item in available)
            {
                list.Add(new MyAvailableExtension()
                {
                    Description = item.Description,
                    DisplayName = item.DisplayName,
                    StoreId = item.StoreId,
                    Type = item.Type
                });
            }
            return list;
        }
    }
}
