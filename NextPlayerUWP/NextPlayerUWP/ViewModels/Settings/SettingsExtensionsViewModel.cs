using NextPlayerUWP.Extensions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NextPlayerUWP.ViewModels.Settings
{
    public class SettingsExtensionsViewModel : Template10.Mvvm.ViewModelBase, ISettingsViewModel
    {
        LyricsExtensions extHelper;
        public SettingsExtensionsViewModel()
        {
            isLoaded = false;
            ViewModelLocator vml = new ViewModelLocator();
            extHelper = vml.LyricsExtensionsService;
        }

        public void Load()
        {
            OnLoaded();
        }

        private bool isLoaded;
        private async Task OnLoaded()
        {
            var list = await extHelper.GetExtensionsInfo();
            LyricsExtensions = new ObservableCollection<AppExtensionInfo>(list.OrderBy(e=>e.Priority));

            if (isLoaded) return;

            isLoaded = true;
        }

        private ObservableCollection<AppExtensionInfo> lyricsExtensions = new ObservableCollection<AppExtensionInfo>();
        public ObservableCollection<AppExtensionInfo> LyricsExtensions
        {
            get { return lyricsExtensions; }
            set { Set(ref lyricsExtensions, value); }
        }

        public void ApplyLyricsExtensionChanges()
        {
            extHelper.UpdatePrioritiesAndSave(LyricsExtensions.ToList());
        }
    }
}
