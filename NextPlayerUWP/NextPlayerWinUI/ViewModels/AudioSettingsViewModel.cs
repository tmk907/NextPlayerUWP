namespace NextPlayerWinUI.ViewModels
{
    public class AudioSettingsViewModel : NextPlayerWinUI.Navigation.ViewModelBase
    {
        public AudioSettingsViewModel()
        {
            ViewModelLocator vml = new ViewModelLocator();
            PlayerVM = vml.PlayerVM;
        }

        public PlayerViewModelBase PlayerVM { get; set; }
    }
}
