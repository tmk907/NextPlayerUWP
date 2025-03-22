using System;
using Microsoft.UI.Xaml;

namespace NextPlayerWinUI.Common
{
    public class SeekButtonsHelper
    {
        public SeekButtonsHelper()
        {
            seekTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(seekTimerInterval) };
            seekTimer.Tick += seekTimerTick;
        }

        private DispatcherTimer seekTimer;
        private const int seekTimerInterval = 600;//must be greater than interval in XAML
        private bool? isFirstClickPrevious = null;
        private bool? isFirstClickNext = null;

        public int RepeatButtonInterval
        {
            get { return seekTimerInterval - 100; }
        }

        private void seekTimerTick(object sender, object e)
        {
            if (isFirstClickPrevious == true)
            {
                PlaybackService.Instance.Previous();
            }
            if (isFirstClickNext == true)
            {
                PlaybackService.Instance.Next();
            }
            seekTimer.Stop();
            isFirstClickPrevious = null;
            isFirstClickNext = null;
        }

        private void seekTimerReSetProperty()
        {
            seekTimer.Stop();
            seekTimer.Start();
        }

        public void Previous()
        {
            //System.Diagnostics.Debug.WriteLine("SeekButtonsHelper Previous");
            if (isFirstClickPrevious == null)
            {
                seekTimerReSetProperty();
                isFirstClickPrevious = true;
            }
            else
            {
                seekTimerReSetProperty();
                isFirstClickPrevious = false;
                PlaybackService.Instance.Rewind();
            }
        }

        public void Next()
        {
            //System.Diagnostics.Debug.WriteLine("SeekButtonsHelper Next");
            if (isFirstClickNext == null)
            {
                seekTimerReSetProperty();
                isFirstClickNext = true;
            }
            else
            {
                seekTimerReSetProperty();
                isFirstClickNext = false;
                PlaybackService.Instance.FastForward();
            }
        }
    }
}
