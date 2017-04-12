using NextPlayerUWP.ViewModels;

namespace NextPlayerUWP.Common
{
    public class PlayerCommands
    {
        public void Play()
        {
            PlaybackService.Instance.Play();
        }

        public void Pause()
        {
            PlaybackService.Instance.Pause();
        }

        public void TogglePlayPause()
        {
            PlaybackService.Instance.TogglePlayPause();
        }

        public void Previous()
        {
            PlaybackService.Instance.Previous();
        }

        public void Next()
        {
            PlaybackService.Instance.Next();
        }

        public void ToggleShuffle()
        {
            ViewModelLocator vml = new ViewModelLocator();
            PlayerViewModelBase playerVM = vml.PlayerVM;
            playerVM.ToggleShuffle();
        }

        public void ToggleRepeat()
        {
            ViewModelLocator vml = new ViewModelLocator();
            PlayerViewModelBase playerVM = vml.PlayerVM;
            playerVM.ToggleRepeat();
        }

        public void Volume(int volume)
        {
            ViewModelLocator vml = new ViewModelLocator();
            PlayerViewModelBase playerVM = vml.PlayerVM;
            playerVM.Volume = volume;
        }
    }
}
