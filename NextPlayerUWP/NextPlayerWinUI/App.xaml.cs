using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppLifecycle;
using NextPlayerWinUI.Common;
using NextPlayerWinUI.Navigation;
using NextPlayerWinUI.Views;
using NextPlayerWinUIDataLayer.Diagnostics;
using NextPlayerWinUIDataLayer.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace NextPlayerWinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private static MainWindow? m_window;

        private readonly ScrobbleSender scrobbleSender = new ScrobbleSender();
        private readonly AppKeyboardShortcuts appShortcuts = new AppKeyboardShortcuts();
        private bool isFirstRun;

        public static bool CanLoadShellControls = false;

        public static Window? Window => m_window;
        public static DispatcherWrapper Dispatcher { get; private set; }
        public static INavigationService NavigationService { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            Logger2.Current.CreateErrorFile();
#if DEBUG
            Logger2.Current.SetLevel(Logger2.Level.Debug);
#else
            Logger2.Current.SetLevel(Logger2.Level.DontLog);
#endif
            isFirstRun = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.FirstRun) == null;

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
                IsLightThemeOn = (bool)t;
                RequestedTheme = IsLightThemeOn ? ApplicationTheme.Light : ApplicationTheme.Dark;
            }
            else
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AppTheme, RequestedTheme == ApplicationTheme.Light);
            }

            ApplicationSettingsHelper.ReadResetSettingsValue(SettingsKeys.MediaScan);

            UnhandledException += App_UnhandledException;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Dispatcher = new DispatcherWrapper(DispatcherQueue.GetForCurrentThread());
            NavigationService = new NavigationService(new Frame());

            m_window = new MainWindow();
            m_window.Content = new Shell(NavigationService);
            m_window.Closed += OnWindowClosed;
            appShortcuts.RegisterShortcuts(m_window.Content);
            m_window.Activate();

            await OnInitializeAsync();
            await OnActivatedAsync(AppInstance.GetCurrent().GetActivatedEventArgs(), isAppStarting: true);
        }

        /// <summary>
        /// Another launch of the app (for example a file opened from Explorer) was redirected to this instance.
        /// Raised on a background thread.
        /// </summary>
        public static void OnRedirectedActivation(AppActivationArguments args)
        {
            Dispatcher?.Dispatch(async () =>
            {
                m_window?.Activate();
                await ((App)Current).OnActivatedAsync(args, isAppStarting: false);
            });
        }

        private async Task OnInitializeAsync()
        {
            AppColors.AppAccentColors.RestoreAppAccentColors();
            new TranslationHelper().ChangeSlideableItemDescription();

            await SongCoverManager.Instance.Initialize();

            ThemeHelper.ApplyAppTheme(ThemeHelper.IsLightTheme);

            new DisplayRequestHelper().ActivateIfEnabled();
        }

        private async Task OnActivatedAsync(AppActivationArguments activation, bool isAppStarting)
        {
            var fileArgs = activation.Data as IFileActivatedEventArgs;

            await NavigateForActivationAsync(activation, fileArgs, isAppStarting);

            if (fileArgs != null)
            {
                PlayerInitializer.SetFiles(fileArgs.Files);
            }
            await PlayerInitializer.InitMain();

            if (isAppStarting)
            {
                scrobbleSender.Start();
                _ = TileManager.ManageSecondaryTileImages();
            }
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Logger2.Current.LogAppUnhadledException(e);
        }

        private async void OnWindowClosed(object sender, WindowEventArgs args)
        {
            scrobbleSender.Stop();
            Logger2.Current.WriteMessage("OnWindowClosed");
            await Logger2.Current.WriteToFile();
        }
    }
}
