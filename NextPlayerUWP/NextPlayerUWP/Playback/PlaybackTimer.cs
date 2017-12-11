using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using System;

namespace NextPlayerUWP.Playback
{
    public class PlaybackTimer
    {
        private ActionTimer MusicPlaybackTimer;

        public PlaybackTimer()
        {
            MusicPlaybackTimer = new ActionTimer();
        }

        public void SetPlaybackStopTimer()
        {
            var t = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.TimerTime);
            long timerTicks = 0;
            if (t != null)
            {
                timerTicks = (long)t;
            }
            TimeSpan currentTime = TimeSpan.FromHours(DateTime.Now.Hour) + TimeSpan.FromMinutes(DateTime.Now.Minute) + TimeSpan.FromSeconds(DateTime.Now.Second);

            TimeSpan delay = TimeSpan.FromTicks(timerTicks - currentTime.Ticks);
            if (delay < TimeSpan.Zero)
            {
                delay = delay + TimeSpan.FromHours(24);
            }

            MusicPlaybackTimer.SetTimerWithAction(delay, PlaybackStopTimerCallback);
        }

        private void PlaybackStopTimerCallback()
        {
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.TimerOn, false);
            PlaybackService.Instance.Pause();
        }

        public void CancelPlaybackStopTimer()
        {
            MusicPlaybackTimer.TimerCancel();
        }
    }
}
