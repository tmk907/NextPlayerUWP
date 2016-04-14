using Microsoft.HockeyApp;

namespace NextPlayerUWP.Common
{
    public class HockeyAdapter
    {
        public static void TrackEvent(string eventName)
        {
            HockeyClient.Current.TrackEvent(eventName);
        }
    }
}
