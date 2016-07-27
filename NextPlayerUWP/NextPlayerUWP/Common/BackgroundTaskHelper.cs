using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;

namespace NextPlayerUWP.Common
{
    public class BackgroundTaskHelper
    {
        public static async Task CheckAppVersion()
        {
            var version = Package.Current.Id.Version;
            var appVersion = string.Format("{0}.{1}.{2}.{3}",
                    version.Major, version.Minor, version.Build, version.Revision);

            if ((Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppVersionBGTask"] ?? "").ToString() != appVersion)
            {
                // App has been updated
                Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppVersionBGTask"] = appVersion;

                BackgroundExecutionManager.RemoveAccess();
                BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();
            }
        }

        public static async Task RegisterBackgroundTasks()
        {
            //BackgroundScrobbler
            string name = "NextPlayerBackgroundScrobbler";
            string entryPoint = "ScrobblerBG.BackgroundScrobbler";
            TimeTrigger timeTrigger = new TimeTrigger(30, false);
            SystemCondition internetCondition = new SystemCondition(SystemConditionType.InternetAvailable);

            try
            {
                await RegisterBackgroundTask(entryPoint, name, timeTrigger, internetCondition);
            }
            catch (Exception ex)
            {
                TelemetryAdapter.TrackEvent("BGScrobbler registration failed");
            }
        }

        public static async Task RegisterBackgroundTask(string taskEntryPoint, string taskName, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            var taskRegistered = false;

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    taskRegistered = true;
                    break;
                }
            }

            if (!taskRegistered)
            {
                var status = await BackgroundExecutionManager.RequestAccessAsync();

                var builder = new BackgroundTaskBuilder();
                builder.Name = taskName;
                builder.TaskEntryPoint = taskEntryPoint;
                builder.SetTrigger(trigger);
                if (condition != null)
                {
                    builder.AddCondition(condition);
                }

                BackgroundTaskRegistration task = builder.Register();
            }
        }
    }
}
