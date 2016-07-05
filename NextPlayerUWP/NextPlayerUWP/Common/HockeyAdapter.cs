using Microsoft.HockeyApp;

namespace NextPlayerUWP.Common
{
    public class TelemetryAdapter
    {
        public static void TrackEvent(string eventName)
        {
            HockeyClient.Current.TrackEvent(eventName);
            System.Diagnostics.Debug.WriteLine(eventName);
        }

        public static void TrackEventException(string exception)
        {
            HockeyClient.Current.TrackEvent("Exception: "+exception);
        }

        public static void TrackPageView(string name)
        {
            System.Diagnostics.Debug.WriteLine("Page " + name);
            HockeyClient.Current.TrackPageView(name);
        }
    }
}
