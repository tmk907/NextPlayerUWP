﻿using Microsoft.Services.Store.Engagement;
using NextPlayerUWPDataLayer.Helpers;
using System;
using Windows.ApplicationModel;

namespace NextPlayerUWP.Common
{
    public class TelemetryAdapter
    {
        public static void TrackEvent(string eventName)
        {
#if DEBUG
#else
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log(eventName);
#endif
            //Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(eventName);
            System.Diagnostics.Debug.WriteLine("TrackEvent: " + eventName);
        }

        public static void TrackEventException(string exception)
        {
            //Microsoft.HockeyApp.HockeyClient.Current.TrackEvent("Exception: " + exception);
            System.Diagnostics.Debug.WriteLine("TrackEventException: " + exception);
        }

        public static void TrackPageView(string name)
        {
            System.Diagnostics.Debug.WriteLine("TrackPageView " + name);
            //Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(name);
        }

        public static void TrackAppLaunch()
        {
            const string dayOfUseKey = "dayofuse";
            const string appLaunchCountKey = "applaunchcount";

            Package package = Package.Current;
            TimeSpan period =  DateTime.Now - package.InstalledDate;

            int appLaunchCount = (int)(ApplicationSettingsHelper.ReadSettingsValue(appLaunchCountKey) ?? 0);
            ApplicationSettingsHelper.SaveSettingsValue(appLaunchCountKey, appLaunchCount + 1);

            string lastEventTracked = (string)(ApplicationSettingsHelper.ReadSettingsValue(dayOfUseKey) ?? "D0");
            if (lastEventTracked == "D0")//first time app open
            {
                ApplicationSettingsHelper.SaveSettingsValue(dayOfUseKey, "D1");
                TrackEvent("D1");
            }
            else
            {
                if (period.TotalDays == 2)
                {
                    if (lastEventTracked != "D2")
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(dayOfUseKey, "D2");
                        TrackEvent("D2");
                    }
                }
                if (period.TotalDays == 3)
                {
                    if (lastEventTracked != "D3")
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(dayOfUseKey, "D3");
                        TrackEvent("D3");
                    }
                }
                else if (period.TotalDays > 3 && period.TotalDays <= 7)
                {
                    if (lastEventTracked != "D7")
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(dayOfUseKey, "D7");
                        TrackEvent("D7");
                    }
                }
                else if (period.TotalDays > 7 && period.TotalDays <= 14)
                {
                    if (lastEventTracked != "D14")
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(dayOfUseKey, "D14");
                        TrackEvent("D14");
                    }
                }
                else if (period.TotalDays > 14 && period.TotalDays <= 30)
                {
                    if (lastEventTracked != "D30")
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(dayOfUseKey, "D30");
                        TrackEvent("D30");
                    }
                }
            }
        }
    }
}
