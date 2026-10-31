using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using NextPlayerWinUIDataLayer.Diagnostics;
using WinRT;

namespace NextPlayerWinUI
{
    /// <summary>
    /// Custom entry point so the app stays single-instance like the UWP version: a second launch
    /// (for example opening a song from Explorer) is handed to the running instance.
    /// </summary>
    public static class Program
    {
        private const string InstanceKey = "main";

        [STAThread]
        private static int Main(string[] args)
        {
            ComWrappersSupport.InitializeComWrappers();

            if (RedirectToRunningInstance())
            {
                return 0;
            }

            Application.Start(callbackParams =>
            {
                var context = new CrashLoggingSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                SynchronizationContext.SetSynchronizationContext(context);
                new App();
            });

            return 0;
        }

        private static bool RedirectToRunningInstance()
        {
            var activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            var mainInstance = AppInstance.FindOrRegisterForKey(InstanceKey);

            if (mainInstance.IsCurrent)
            {
                mainInstance.Activated += (_, e) => App.OnRedirectedActivation(e);
                return false;
            }

            // RedirectActivationToAsync must not block the STA thread directly.
            Task.Run(() => mainInstance.RedirectActivationToAsync(activationArgs).AsTask()).GetAwaiter().GetResult();
            return true;
        }

        /// <summary>
        /// An exception from an async void method is re-thrown through the synchronization context and ends the
        /// process without raising Application.UnhandledException. Log it first, so the crash leaves a trace.
        /// </summary>
        private sealed class CrashLoggingSynchronizationContext : DispatcherQueueSynchronizationContext
        {
            public CrashLoggingSynchronizationContext(DispatcherQueue dispatcherQueue) : base(dispatcherQueue)
            {
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                base.Post(s =>
                {
                    try
                    {
                        d(s);
                    }
                    catch (Exception ex)
                    {
                        Logger2.Current.LogFatalException(ex);
                        throw;
                    }
                }, state);
            }

            public override SynchronizationContext CreateCopy() => this;
        }
    }
}
