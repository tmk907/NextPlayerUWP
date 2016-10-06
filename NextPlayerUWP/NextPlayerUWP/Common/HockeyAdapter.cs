using Microsoft.Services.Store.Engagement;

namespace NextPlayerUWP.Common
{
    public class TelemetryAdapter
    {
        public static void TrackEvent(string eventName)
        {
            StoreServicesCustomEventLogger logger = StoreServicesCustomEventLogger.GetDefault();
            logger.Log(eventName);
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(eventName);
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
            Microsoft.HockeyApp.HockeyClient.Current.TrackPageView(name);
        }
    }
}
