using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using System;
using Windows.System.Display;

namespace NextPlayerUWP.Common
{
    public class DisplayRequestHelper
    {
        private const string settingsName = "RequestCount";
        private static DisplayRequest displayRequest;

        public int ActivateDisplay()
        {
            if (displayRequest == null) displayRequest = new DisplayRequest();
            int requestCount = (int)(ApplicationSettingsHelper.ReadSettingsValue(settingsName) ?? 0);

            try
            {
                displayRequest.RequestActive();
                requestCount++;
                ApplicationSettingsHelper.SaveSettingsValue(settingsName, requestCount);
            }
            catch (Exception ex)
            {

            }
            return requestCount;
        }

        public int ReleaseDisplay()
        {
            int requestCount = (int)(ApplicationSettingsHelper.ReadSettingsValue(settingsName) ?? 0);
            if (requestCount == 0) return 0;
            if (displayRequest == null) displayRequest = new DisplayRequest();
            try
            {
                displayRequest.RequestRelease();
                requestCount--;
                ApplicationSettingsHelper.SaveSettingsValue(settingsName, requestCount);
            }
            catch (Exception ex)
            {
                ApplicationSettingsHelper.SaveSettingsValue(settingsName, 0);
            }
            return requestCount;
        }

        public void ActivateIfEnabled()
        {
            bool disable = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.DisableLockscreen);
            if (disable)
            {
                ActivateDisplay();
            }
        }
    }
}
