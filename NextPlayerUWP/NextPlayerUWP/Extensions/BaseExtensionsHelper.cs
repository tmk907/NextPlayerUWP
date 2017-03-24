using NextPlayerUWPDataLayer.Helpers;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.AppExtensions;

namespace NextPlayerUWP.Extensions
{
    public abstract class BaseExtensionsHelper
    {
        protected string appExtensionName;
        protected AppExtensionCatalog catalog;
        protected List<AppExtensionInfo> extensions;

        protected void ApplyPriorities(List<AppExtensionInfo> extensions)
        {
            var list = ApplicationSettingsHelper.ReadData<List<AppExtensionInfo>>(appExtensionName);
            if (list == null) list = new List<AppExtensionInfo>();
            int max = 0;
            foreach (var ext in extensions)
            {
                ext.Priority = list.FirstOrDefault(e => e.Id == ext.Id)?.Priority ?? -1;
                if (ext.Priority > max) max = ext.Priority;
            }
            foreach (var ext in extensions.Where(e => e.Priority == -1))
            {
                max++;
                ext.Priority = max;
            }
            ApplicationSettingsHelper.SaveData(appExtensionName, extensions);
        }

        public void UpdatePriorities(List<AppExtensionInfo> reorderedList)
        {
            int i = 0;
            foreach (var item in reorderedList)
            {
                item.Priority = i;
                i++;
            }
            ApplicationSettingsHelper.SaveData(appExtensionName, reorderedList);
        }       
    }
}
