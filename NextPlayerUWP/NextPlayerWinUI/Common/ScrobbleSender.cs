using NextPlayerWinUIDataLayer.Diagnostics;
using NextPlayerWinUIDataLayer.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NextPlayerWinUI.Common
{
    /// <summary>
    /// Sends scrobbles cached by the player to Last.fm. Replaces the ScrobblerBG background task of the
    /// UWP app: it runs at startup and on the same 30 minute cycle, but only while the app is running.
    /// </summary>
    public sealed class ScrobbleSender
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

        private CancellationTokenSource cancellation;

        public void Start()
        {
            cancellation = new CancellationTokenSource();
            _ = Task.Run(() => RunAsync(cancellation.Token));
        }

        public void Stop() => cancellation?.Cancel();

        private static async Task RunAsync(CancellationToken token)
        {
            using var timer = new PeriodicTimer(Interval);
            do
            {
                try
                {
                    await new LastFmManager().SendCachedScrobbles();
                }
                catch (Exception ex)
                {
                    Logger2.Current.WriteMessage("ScrobbleSender " + ex.Message, Logger2.Level.WarningError);
                }
            }
            while (await WaitForNextTickAsync(timer, token));
        }

        private static async Task<bool> WaitForNextTickAsync(PeriodicTimer timer, CancellationToken token)
        {
            try
            {
                return await timer.WaitForNextTickAsync(token);
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }
    }
}
