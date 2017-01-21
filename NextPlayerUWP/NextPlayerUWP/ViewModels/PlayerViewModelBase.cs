using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using Windows.Media.Playback;

namespace NextPlayerUWP.ViewModels
{
    public class PlayerViewModelBase : Template10.Mvvm.BindableBase
    {
        public PlayerViewModelBase()
        {
            Init();
            App.Current.EnteredBackground += Current_EnteredBackground;
            App.Current.LeavingBackground += Current_LeavingBackground;
        }

        private void Current_LeavingBackground(object sender, Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            Init();
        }

        private void Current_EnteredBackground(object sender, Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
            PlaybackService.MediaPlayerStateChanged -= ChangePlayButtonContent;
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.Volume, Volume);
        }

        private void Init()
        {
            RepeatMode = Repeat.CurrentState();
            ShuffleMode = Shuffle.CurrentState();
            Volume = (int)(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.Volume) ?? 100);
            PlaybackRate = PlaybackService.Instance.PlaybackRatePercent;
            AudioBalance = PlaybackService.Instance.AudioBalance;

            seekButtonsHelper = new SeekButtonsHelper();

            ChangePlayButtonContent(PlaybackService.Instance.PlayerState);
            PlaybackService.MediaPlayerStateChanged += ChangePlayButtonContent;
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
                    PlaybackService.Instance.Volume = value;
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

        public async void ToggleShuffle()
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

        public void ToggleRepeat()
        {
            RepeatMode = Repeat.Change();
            PlaybackService.Instance.ApplyRepeatState();
        }


        private string playButtonContent = "\uE768";
        public string PlayButtonContent
        {
            get { return playButtonContent; }
            set { Set(ref playButtonContent, value); }
        }

        private void ChangePlayButtonContent(MediaPlaybackState state)
        {
            var d = Template10.Common.WindowWrapper.Current().Dispatcher;
            if (d == null)
            {
                return;
            }
            d.Dispatch(() =>
            {
                if (state == MediaPlaybackState.Playing || state == MediaPlaybackState.Buffering)
                {
                    PlayButtonContent = "\uE769";
                }
                else if (state == MediaPlaybackState.Paused)
                {
                    PlayButtonContent = "\uE768";
                }
                else
                {
                    PlayButtonContent = "\uE768";
                }
            });
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

        private SeekButtonsHelper seekButtonsHelper;

        public int RepeatButtonInterval
        {
            get { return seekButtonsHelper.RepeatButtonInterval; }
        }

        public void PreviousOrSeek()
        {
            seekButtonsHelper.Previous();
        }

        public void NextOrSeek()
        {
            seekButtonsHelper.Next();
        }
    }
}
