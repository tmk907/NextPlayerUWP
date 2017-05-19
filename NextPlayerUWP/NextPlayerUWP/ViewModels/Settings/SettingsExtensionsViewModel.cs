using NextPlayerUWP.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;

namespace NextPlayerUWP.ViewModels.Settings
{
    public class SettingsExtensionsViewModel : Template10.Mvvm.ViewModelBase, ISettingsViewModel
    {
        LyricsExtensions extHelper;
        public SettingsExtensionsViewModel()
        {
            isLoaded = false;
            ViewModelLocator vml = new ViewModelLocator();
            extHelper = vml.LyricsExtensionsClient.GetHelper();
        }

        public void Load()
        {
            OnLoaded();
        }

        private bool isLoaded;
        private async Task OnLoaded()
        {
            var list = await extHelper.GetExtensionsInfo();
            LyricsExtensions = new ObservableCollection<MyAppExtensionInfo>(list.OrderBy(e=>e.Priority));
            var list2 = await extHelper.GetAvailableExtensions();
            AvailableLyricsExtensions = new ObservableCollection<MyAvailableExtension>(list2);
            if (isLoaded) return;

            isLoaded = true;
        }
        
        private ObservableCollection<MyAppExtensionInfo> lyricsExtensions = new ObservableCollection<MyAppExtensionInfo>();
        public ObservableCollection<MyAppExtensionInfo> LyricsExtensions
        {
            get { return lyricsExtensions; }
            set { Set(ref lyricsExtensions, value); }
        }

        private ObservableCollection<MyAvailableExtension> availableLyricsExtensions = new ObservableCollection<MyAvailableExtension>();
        public ObservableCollection<MyAvailableExtension> AvailableLyricsExtensions
        {
            get { return availableLyricsExtensions; }
            set { Set(ref availableLyricsExtensions, value); }
        }

        public void ApplyLyricsExtensionChanges()
        {
            extHelper.UpdatePrioritiesAndSave(LyricsExtensions.ToList());
        }

        public async Task InstallExtension(MyAvailableExtension ext)
        {
            await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://pdp/?ProductId={ext.StoreId}"));
        }
    }
}
