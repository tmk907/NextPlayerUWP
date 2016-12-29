using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.HockeyApp;
using Microsoft.Services.Store.Engagement;
using NextPlayerUWPDataLayer.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
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
            HockeyClient.Current.TrackEvent(eventName);

            System.Diagnostics.Debug.WriteLine("TrackEvent: " + eventName);
        }

        public static void TrackMetrics(string name, double value, IDictionary<string,string> properties = null)
        {
            HockeyClient.Current.TrackMetric(name, value, properties);
        }

        public static void TrackEventException(string exception)
        {
            //Microsoft.HockeyApp.HockeyClient.Current.TrackEvent("Exception: " + exception);
            System.Diagnostics.Debug.WriteLine("TrackEventException: " + exception);
        }

        public static void TrackPageView(string name)
        {
            System.Diagnostics.Debug.WriteLine("TrackPageView " + name);
            HockeyClient.Current.TrackPageView(name);
        }

        public static void TrackAppLaunch()
        {
            const string dayOfUseKey = "dayofuse";
            const string appLaunchCountKey = "applaunchcount";

            Package package = Package.Current;
            TimeSpan period =  DateTime.Now - package.InstalledDate;
            int daysPast = period.Days;

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
                if (daysPast == 2)
                {
                    if (lastEventTracked != "D2")
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(dayOfUseKey, "D2");
                        TrackEvent("D2");
                    }
                }
                if (daysPast == 3)
                {
                    if (lastEventTracked != "D3")
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(dayOfUseKey, "D3");
                        TrackEvent("D3");
                    }
                }
                else if (daysPast > 3 && daysPast <= 7)
                {
                    if (lastEventTracked != "D7")
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(dayOfUseKey, "D7");
                        TrackEvent("D7");
                    }
                }
                else if (daysPast > 7 && daysPast <= 14)
                {
                    if (lastEventTracked != "D14")
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(dayOfUseKey, "D14");
                        TrackEvent("D14");
                    }
                }
                else if (daysPast > 14 && daysPast <= 30)
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
