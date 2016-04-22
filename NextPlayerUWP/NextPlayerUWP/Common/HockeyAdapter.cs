using Microsoft.HockeyApp;

namespace NextPlayerUWP.Common
{
    public class HockeyProxy
    {
        public static void TrackEvent(string eventName)
        {
            HockeyClient.Current.TrackEvent(eventName);
        }
    }
}
