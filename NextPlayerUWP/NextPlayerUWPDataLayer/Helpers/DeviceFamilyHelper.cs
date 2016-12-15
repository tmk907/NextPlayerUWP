using Windows.System.Profile;

namespace NextPlayerUWPDataLayer.Helpers
{
    public class DeviceFamilyHelper
    {
        static string deviceFamily;
        public static bool IsXbox()
        {
            return AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Xbox";
        }
        public static bool IsDesktop()
        {
            return AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop";
        }
        public static bool IsMobile()
        {
            //if (deviceFamily == null)
            //    deviceFamily = AnalyticsInfo.VersionInfo.DeviceFamily;
            return AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile";
        }
    }
}
