using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class AudioSettingsViewModel : Template10.Mvvm.ViewModelBase
    {
        public AudioSettingsViewModel()
        {
            ViewModelLocator vml = new ViewModelLocator();
            PlayerVM = vml.PlayerVM;
        }

        public PlayerViewModelBase PlayerVM { get; set; }
       
    }
}
