using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;
using NextPlayerWinUI.Common;
using NextPlayerWinUIDataLayer.Diagnostics;
using NextPlayerWinUIDataLayer.Helpers;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using CommunityToolkit.Common;
using Template10.Services.NavigationService;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace NextPlayerWinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            Logger2.Current.CreateErrorFile();
#if DEBUG
            Logger2.Current.SetLevel(Logger2.Level.Debug);
#else
            Logger2.Current.SetLevel(Logger2.Level.DontLog);
#endif
            object o = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.FirstRun);
            if (null == o)
            {
                isFirstRun = true;
            }
            else
            {
                isFirstRun = false;
            }

            if (isFirstRun)
            {
                FirstRunSetup();
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.FirstRun, false);
            }
            else
            {
                PerformUpdate();
            }

            var t = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.AppTheme);
            if (t != null)
            {
                if ((bool)t)
                {
                    IsLightThemeOn = true;
                    RequestedTheme = ApplicationTheme.Light;
                }
                else
                {
                    IsLightThemeOn = false;
                    RequestedTheme = ApplicationTheme.Dark;
                }
            }
            else
            {
                if (RequestedTheme == ApplicationTheme.Light)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AppTheme, true);
                }
                else
                {
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AppTheme, false);
                }
            }

            ApplicationSettingsHelper.ReadResetSettingsValue(SettingsKeys.MediaScan);

            appShortcuts = new AppKeyboardShortcuts();

            OnStartAsyncFinished += InitAfterOnStart;

            RegisterNotificationChannelAsync();
            this.UnhandledException += App_UnhandledException; ;

        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Logger2.Current.LogAppUnhadledException(e);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
            App.Dispatcher = new DispatcherWrapper(DispatcherQueue.GetForCurrentThread());

            OnStartAsync(StartKind.Launch,args).GetResultOrDefault();
            OnInitializeAsync(args).GetResultOrDefault();

            m_window.Closed += OnWindowClosed;
        }

        private async void OnWindowClosed(object sender, WindowEventArgs args)
        {
            Logger2.Current.WriteMessage("OnWindowClosed");
            await Logger2.Current.WriteToFile();
        }

        private static Window? m_window;
        public static Window? Window { get; }

        public static DispatcherWrapper Dispatcher { get; private set; }

        //
        //
        //

        private bool isFirstRun = false;
        private AppKeyboardShortcuts appShortcuts;
        public static bool CanLoadShellControls = false;

        private delegate void OnStartAsyncFinishedHandler(ApplicationExecutionState state, FileActivatedEventArgs fileArgs);
        private event OnStartAsyncFinishedHandler OnStartAsyncFinished;

        public static FileActivatedEventArgs FileArgs;
        public static INavigationService NavigationService;


        private async void InitAfterOnStart(ApplicationExecutionState state, FileActivatedEventArgs fileArgs)
        {
            if (fileArgs != null)
            {
                PlayerInitializer.SetFiles(fileArgs.Files);
            }
            await PlayerInitializer.InitMain();

            if (state == ApplicationExecutionState.ClosedByUser ||
                state == ApplicationExecutionState.NotRunning)
            {
                try
                {
                    await RegisterBGScrobbler();
                    //await TileManager.ManageSecondaryTileImages();
                }
                catch (Exception ex)
                {

                }
            }
        }

        private async Task RegisterBGScrobbler()
        {
            await BackgroundTaskHelper.CheckAppVersion();
            await BackgroundTaskHelper.RegisterBackgroundTasks();
        }

        private async void RegisterNotificationChannelAsync()
        {
            //StoreServicesEngagementManager manager = StoreServicesEngagementManager.GetDefault();
            //await manager.RegisterNotificationChannelAsync();
        }

    }
}
