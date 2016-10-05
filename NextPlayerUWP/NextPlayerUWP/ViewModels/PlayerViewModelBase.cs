using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;

namespace NextPlayerUWP.ViewModels
{
    public class PlayerViewModelBase : Template10.Mvvm.ViewModelBase
    {
        public PlayerViewModelBase()
        {
            RepeatMode = Repeat.CurrentState();
            ShuffleMode = Shuffle.CurrentState();
            Volume = (int)(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Volume) ?? 100);
            PlaybackRate = PlaybackService.Instance.PlaybackRatePercent;
            AudioBalance = PlaybackService.Instance.AudioBalance;
        }

        private bool isMuted = false;
        private int prevVolume = 100;

        private int volume = 100;
        public int Volume
        {
            get { return volume; }
            set
            {
                if (volume != value)
                {
                    if (value == 0) isMuted = true;
                    else isMuted = false;
                    PlaybackService.Instance.ChangeVolume(value);
                }
                Set(ref volume, value);
            }
        }

        public void MuteVolume()
        {
            if (isMuted)
            {
                Volume = prevVolume;
            }
            else
            {
                prevVolume = volume;
                Volume = 0;
            }
        }


        private int playbackRate = 100;
        public int PlaybackRate
        {
            get { return playbackRate; }
            set
            {
                if (value != PlaybackService.Instance.PlaybackRatePercent)
                {
                    PlaybackService.Instance.PlaybackRatePercent = value;
                }
                Set(ref playbackRate, value);
            }
        }

        public void ResetPlaybackRate()
        {
            PlaybackRate = 100;
        }

        private double audioBalance = 100;
        public double AudioBalance
        {
            get { return audioBalance; }
            set
            {
                if (value != PlaybackService.Instance.AudioBalance)
                {
                    PlaybackService.Instance.AudioBalance = value;
                }
                Set(ref audioBalance, value);
            }
        }

        public void ResetAudioBalance()
        {
            PlaybackService.Instance.ResetBalance();
        }

        private bool shuffleMode = false;
        public bool ShuffleMode
        {
            get { return shuffleMode; }
            set { Set(ref shuffleMode, value); }
        }

        public async void ShuffleCommand()
        {
            ShuffleMode = Shuffle.Change();
            await PlaybackService.Instance.ChangeShuffle();
        }

        private RepeatEnum repeatMode = RepeatEnum.NoRepeat;
        public RepeatEnum RepeatMode
        {
            get { return repeatMode; }
            set { Set(ref repeatMode, value); }
        }

        public void RepeatCommand()
        {
            RepeatMode = Repeat.Change();
            PlaybackService.Instance.ApplyRepeatState();
        }

        public void Play()
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
    }
}
