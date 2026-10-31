using NextPlayerWinUIDataLayer.Helpers;
using System.Runtime.InteropServices;

namespace NextPlayerWinUI.Common
{
    /// <summary>
    /// Keeps the display on while the user's "disable lock screen" setting is enabled.
    /// DisplayRequest is not available to Windows App SDK apps, so this uses the Win32 execution state.
    /// Call it on the UI thread: the state belongs to the calling thread.
    /// </summary>
    public class DisplayRequestHelper
    {
        private const string settingsName = "RequestCount";
        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_DISPLAY_REQUIRED = 0x00000002;

        [DllImport("kernel32.dll")]
        private static extern uint SetThreadExecutionState(uint esFlags);

        public int ActivateDisplay()
        {
            int requestCount = (int)(ApplicationSettingsHelper.ReadSettingsValue(settingsName) ?? 0) + 1;
            SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED);
            ApplicationSettingsHelper.SaveSettingsValue(settingsName, requestCount);
            return requestCount;
        }

        public int ReleaseDisplay()
        {
            int requestCount = (int)(ApplicationSettingsHelper.ReadSettingsValue(settingsName) ?? 0);
            if (requestCount == 0)
            {
                return 0;
            }

            requestCount--;
            if (requestCount == 0)
            {
                SetThreadExecutionState(ES_CONTINUOUS);
            }
            ApplicationSettingsHelper.SaveSettingsValue(settingsName, requestCount);
            return requestCount;
        }

        public void ActivateIfEnabled()
        {
            // The system forgets the request when the app exits, so the persisted count starts over.
            ApplicationSettingsHelper.SaveSettingsValue(settingsName, 0);

            bool disable = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.DisableLockscreen);
            if (disable)
            {
                ActivateDisplay();
            }
        }
    }
}
