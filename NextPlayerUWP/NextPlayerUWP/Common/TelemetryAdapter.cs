using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.HockeyApp;
using Microsoft.Services.Store.Engagement;
using NextPlayerUWPDataLayer.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public static void TrackEvent(string eventName, IDictionary<string,string> properties, IDictionary<string,double> metrics)
        {
            HockeyClient.Current.TrackEvent(eventName, properties, metrics);
        }

        public static void TrackLibraryUpdate(int songsCount, int playlistsCount, double durationSeconds)
        {
            var metrics = new Dictionary<string, double> {
                { "updateDuration", durationSeconds },
                { "songsCount", songsCount },
                { "playlistsCount", playlistsCount }
            };
            var properties = new Dictionary<string, string> {
                { "libraryUpdateP", "libraryUpdated" }
            };
            HockeyClient.Current.TrackEvent("libraryUpdate", properties, metrics);
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
            //const string dayBetweenKeys = "daybetweenkeys";

            string Day0 = "Day0";
            string Day1 = "Day1";
            string Day2 = "Day2";
            string Day3 = "Day3";
            string Day4 = "Day4";
            string Day5 = "Day5";
            string Day6 = "Day6";
            string Day7 = "Day7";
            string Day14 = "Day14";
            string Day21 = "Day21";
            string Day30 = "Day30";
            string Day60 = "Day60";

            Dictionary<string, int> days = new Dictionary<string, int>()
            {
                { "" , -1 },
                { Day0, 0 },
                { Day1, 1 },
                { Day2, 2 },
                { Day3, 3 },
                { Day4, 4 },
                { Day5, 5 },
                { Day6, 6 },
                { Day7, 7 },
                { Day14, 14 },
                { Day21, 21 },
                { Day30, 30 },
                { Day60, 60 },
            };

            Package package = Package.Current;
            TimeSpan period =  DateTime.Now - package.InstalledDate;
            int daysPastSinceInstall = period.Days;

            int appLaunchCount = (int)(ApplicationSettingsHelper.ReadSettingsValue(appLaunchCountKey) ?? 0);
            appLaunchCount++;
            ApplicationSettingsHelper.SaveSettingsValue(appLaunchCountKey, appLaunchCount);

            //bool dayBetween = (bool)(ApplicationSettingsHelper.ReadSettingsValue(dayBetweenKeys) ?? false);

            string lastEventTracked = (string)(ApplicationSettingsHelper.ReadSettingsValue(dayOfUseKey) ?? "");
            int lastEventTrackedDaysPast = days[lastEventTracked];

            if (daysPastSinceInstall > lastEventTrackedDaysPast)
            {
                string nextLabel = lastEventTracked;
                foreach(var k in days.OrderBy(a=>a.Value))
                {
                    nextLabel = k.Key;
                    if (daysPastSinceInstall <= k.Value) break;
                }
                if (days[nextLabel] == daysPastSinceInstall)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(dayOfUseKey, nextLabel);
                    TrackEvent(nextLabel);
                    //ApplicationSettingsHelper.SaveSettingsValue(dayBetweenKeys, false);
                }
                //else
                //{
                //    bool between = true;
                //    foreach(var k in days)
                //    {
                //        if (k.Value == daysPastSinceInstall)
                //        {
                //            between = false;
                //            break;
                //        }
                //    }
                //    ApplicationSettingsHelper.SaveSettingsValue(dayBetweenKeys, between);
                //}
            }                      
        }
    }
}
