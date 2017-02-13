using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using System;

namespace NextPlayerUWP.ViewModels.Settings
{
    public class SettingsToolsViewModel : Template10.Mvvm.ViewModelBase, ISettingsViewModel
    {
        public SettingsToolsViewModel()
        {
            isLoaded = false;
        }

        public void Load()
        {
            OnLoaded();
        }

        private bool isLoaded;
        private void OnLoaded()
        {
            if (isLoaded) return;

            var tt = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.TimerTime);
            var IsTimerOn = (bool)(ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.TimerOn) ?? false);
            Time = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            if (isTimerOn)
            {
                if (tt != null)
                {
                    Time = TimeSpan.FromTicks((long)tt);
                }
                else
                {
                    IsTimerOn = false;
                }
            }

            isLoaded = true;
        }

        private bool isTimerOn = false;
        public bool IsTimerOn
        {
            get { return isTimerOn; }
            set
            {
                if (value != isTimerOn)
                {
                    ChangeTimer(value, time);
                }
                Set(ref isTimerOn, value);
            }
        }

        private TimeSpan time = TimeSpan.Zero;
        public TimeSpan Time
        {
            get { return time; }
            set
            {
                if (value != time)
                {
                    ChangeTimer(true, value);
                }
                Set(ref time, value);
            }
        }

        public void ChangeTimer(bool isOn, TimeSpan time)
        {
            if (isLoaded)
            {
                if (isOn)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.TimerOn, true);
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.TimerTime, time.Ticks);
                    PlaybackService.Instance.SetPlaybackStopTimer();
                    TelemetryAdapter.TrackEvent("Timer on");
                }
                else
                {
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.TimerOn, false);
                    PlaybackService.Instance.CancelPlaybackStopTimer();
                }
            }
        }
    }
}
