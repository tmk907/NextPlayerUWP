using NextPlayerUWPDataLayer.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace NextPlayerUWP.Extensions
{
    public abstract class BaseExtensionsHelper
    {
        protected string appExtensionName;

        protected void ApplyPriorities(List<MyAppExtensionInfo> extensions)
        {
            var list = ApplicationSettingsHelper.ReadData<List<MyAppExtensionInfo>>(appExtensionName);
            if (list == null) list = new List<MyAppExtensionInfo>();
            int max = 0;
            foreach (var ext in extensions)
            {
                ext.Priority = list.FirstOrDefault(e => e.Id == ext.Id)?.Priority ?? -1;
                ext.Enabled = list.FirstOrDefault(e => e.Id == ext.Id)?.Enabled ?? true;
                if (ext.Priority > max) max = ext.Priority;
            }
            foreach (var ext in extensions.Where(e => e.Priority == -1))
            {
                max++;
                ext.Priority = max;
            }
            ApplicationSettingsHelper.SaveData(appExtensionName, extensions);
        }

        public void UpdatePrioritiesAndSave(List<MyAppExtensionInfo> reorderedList)
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
