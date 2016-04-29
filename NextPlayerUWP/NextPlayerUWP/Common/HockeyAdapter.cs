using Microsoft.HockeyApp;

namespace NextPlayerUWP.Common
{
    public class HockeyProxy
    {
        public static void TrackEvent(string eventName)
        {
            HockeyClient.Current.TrackEvent(eventName);
        }

        public static void TrackEventException(string exception)
        {
            HockeyClient.Current.TrackEvent("Exception: "+exception);
        }
    }
}
