using NextPlayerUWP.Extensions;
using NextPlayerUWPDataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPlayerUWP.ViewModels.Settings
{
    public class SettingsExtensionsViewModel : Template10.Mvvm.ViewModelBase, ISettingsViewModel
    {
        LyricsExtensions extHelper;
        public SettingsExtensionsViewModel()
        {
            isLoaded = false;
            extHelper = new LyricsExtensions();
        }

        public void Load()
        {
            OnLoaded();
        }

        private bool isLoaded;
        private async Task OnLoaded()
        {
            if (isLoaded) return;
            var list = await extHelper.GetExtensionsInfo();
            LyricsExtensions = new ObservableCollection<AppExtensionInfo>(list);
            isLoaded = true;
        }

        private ObservableCollection<AppExtensionInfo> lyricsExtensions = new ObservableCollection<AppExtensionInfo>();
        public ObservableCollection<AppExtensionInfo> LyricsExtensions
        {
            get { return lyricsExtensions; }
            set { Set(ref lyricsExtensions, value); }
        }

        public void ApplyLyricsExtensionOrder()
        {
            LyricsExtensions extHelper = new LyricsExtensions();
            extHelper.UpdatePriorities(LyricsExtensions.ToList());
        }
    }
}
