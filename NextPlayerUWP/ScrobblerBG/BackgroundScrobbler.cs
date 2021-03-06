﻿using NextPlayerUWPDataLayer.Services;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace ScrobblerBG
{
    public sealed class BackgroundScrobbler : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;
        IBackgroundTaskInstance _taskInstance = null;
        private bool completed = false;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral =  taskInstance.GetDeferral();
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            _taskInstance = taskInstance;
            await SendScrobbles();
            System.Diagnostics.Debug.WriteLine("ScrobblerBG run");
            _deferral.Complete();
        }

        private async void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (!completed)
            {
                NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteMessage("BackgroundScrobbler scrobbling interrupted", NextPlayerUWPDataLayer.Diagnostics.Logger2.Level.WarningError);
                await NextPlayerUWPDataLayer.Diagnostics.Logger2.Current.WriteToFile();
            }
            _deferral.Complete();
        }

        private async Task SendScrobbles()
        {
            LastFmManager lfm = new LastFmManager();
            await lfm.SendCachedScrobbles();
            completed = true;
        }
    }
}
