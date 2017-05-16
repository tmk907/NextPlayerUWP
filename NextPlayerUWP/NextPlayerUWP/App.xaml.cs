using Microsoft.HockeyApp;
using NextPlayerUWP.Common;
using NextPlayerUWP.Views;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Controls;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.Xaml;

namespace NextPlayerUWP
{    
    sealed partial class App : BootStrapper
    {
        public static event EventHandler MemoryUsageReduced;
        public static void OnMemoryUsageReduced()
        {
            MemoryUsageReduced?.Invoke(null, null);
        }

        private bool isFirstRun = false;

        bool _isInBackgroundMode = false;
        private AppKeyboardShortcuts appShortcuts;
        bool isBackgroundLeavedFirstTime = true;
        private bool resumeAlbumArtFinder = false;

        Stopwatch s1;
        public App()
        {
            InitializeComponent();
            s1 = new Stopwatch();
            s1.Start();
            this.EnteredBackground += App_EnteredBackground;
            this.LeavingBackground += App_LeavingBackground;
            MemoryManager.AppMemoryUsageLimitChanging += MemoryManager_AppMemoryUsageLimitChanging;
            MemoryManager.AppMemoryUsageIncreased += MemoryManager_AppMemoryUsageIncreased;
            HockeyClient.Current.Configure(AppConstants.HockeyAppId);
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

            SplashFactory = (e) => new Views.Splash(e);

            appShortcuts = new AppKeyboardShortcuts();
            OnStartAsyncFinished += InitAfterOnStart;

            this.UnhandledException += App_UnhandledException;
            s1.Stop();
            Debug.WriteLine("Time: {0}ms", s1.ElapsedMilliseconds);
            s1.Start();
            //var gcTimer = new DispatcherTimer();
            //gcTimer.Tick += (sender, e) => { GC.Collect(); };
            //gcTimer.Interval = TimeSpan.FromSeconds(1);
            //gcTimer.Start();
        }

        #region Events

        private void MemoryManager_AppMemoryUsageIncreased(object sender, object e)
        {
            var level = MemoryManager.AppMemoryUsageLevel;
            Debug.WriteLine("App MemoryManager_AppMemoryUsageIncreased {0}", level);
            if (level == AppMemoryUsageLevel.OverLimit || level == AppMemoryUsageLevel.High)
            {
                ReduceMemoryUsage(MemoryManager.AppMemoryUsageLimit);
            }
        }

        private void MemoryManager_AppMemoryUsageLimitChanging(object sender, AppMemoryUsageLimitChangingEventArgs e)
        {
            Debug.WriteLine("App MemoryManager_AppMemoryUsageLimitChanging " + (e.OldLimit) + " to " + (e.NewLimit));
            if (MemoryManager.AppMemoryUsage >= e.NewLimit)
            {
                ReduceMemoryUsage(e.NewLimit);
            }
        }

        public async void ReduceMemoryUsage(ulong limit)
        {
            Debug.WriteLine("App ReduceMemoryUsage {0} {1}", _isInBackgroundMode, limit);
            TelemetryAdapter.TrackEvent("ReduceMemoryUsage");
            OnMemoryUsageReduced();
            if (_isInBackgroundMode && Window.Current != null && Window.Current.Content != null)
            {
                if (NavigationService != null)
                {
                    await OnReduceMemoryUsage();
                    await NavigationService.SaveNavigationAsync();
                    WindowWrapper.Current().NavigationServices.Clear();//.Remove(NavigationService);
                    //var a = WindowWrapper.Current().NavigationServices.Count;
                }
                else
                {
                    //?
                }
                Window.Current.Content = null;
            }
            Debug.WriteLine("App ReduceMemoryUsage GC.Collect()");
            GC.Collect();
        }

        private void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            Debug.WriteLine("App App_EnteredBackground");
            Debug.WriteLine("Memory Usage={0} level={1} limit={2}", MemoryManager.AppMemoryUsage, MemoryManager.AppMemoryUsageLevel, MemoryManager.AppMemoryUsageLimit);

            var deferral = e.GetDeferral();
            _isInBackgroundMode = true;
            
            appShortcuts.DeregisterShortcuts();
            if (OnNewTilePinned != null)
            {
                OnNewTilePinned();
                OnNewTilePinned = null;
            }
            try
            {
#if DEBUG
                //If we are in debug mode free memory here because the memory limits are turned off
                //In release builds defer the actual reduction of memory to the limit changing event so we don't 
                //unnecessarily throw away the UI
                resumeAlbumArtFinder = AlbumArtFinder.Cancel();
                ReduceMemoryUsage(0);
#endif
            }
            finally
            {
                deferral.Complete();
            }
        }

        private async void App_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            Debug.WriteLine("App App_LeavingBackground");
            _isInBackgroundMode = false;

            // Reastore view content if it was previously unloaded.
            if (!isBackgroundLeavedFirstTime && Window.Current != null && Window.Current.Content == null)
            {
                var deferral = e.GetDeferral();
                var service = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
                service.Frame.RequestedTheme = ThemeHelper.IsLightTheme ? ElementTheme.Light : ElementTheme.Dark;
                await service.RestoreSavedNavigationAsync();               
                Window.Current.Content = new ModalDialog
                {
                    DisableBackButtonWhenModal = true,
                    Content = new Views.Shell(service),
                    ModalContent = new Views.Busy(),
                };
                

                if (resumeAlbumArtFinder)
                {
                    AlbumArtFinder.StartLooking().ConfigureAwait(false);
                }

                deferral.Complete();
            }
            isBackgroundLeavedFirstTime = false;

            appShortcuts.RegisterShortcuts();
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //Logger.SaveInSettings("App_UnhandledException " + e.Exception);
            Logger2.Current.LogAppUnhadledException(e);
        }

        #endregion

        public enum Pages
        {
            AddToPlaylist,
            Albums,
            Album,
            AlbumArtists,
            AlbumArtist,
            Artists,
            Artist,
            AudioSettings,
            CloudStorageFolders,
            CuteRadio,
            Genres,
            FileInfo,
            Folders,
            FoldersRoot,
            Lyrics,
            Licenses,
            NewSmartPlaylist,
            NowPlaying,
            NowPlayingDesktop,
            NowPlayingPlaylist,
            Playlists,
            Playlist,
            PlaylistEditable,
            Radios,
            Settings,
            Songs,
            TagsEditor
        }

#region Template10 overrides

        public override UIElement CreateRootElement(IActivatedEventArgs e)
        {
            s1.Stop();
            Debug.WriteLine("Time CreateRootElement Start: {0}ms", s1.ElapsedMilliseconds);
            s1.Start();
            var service = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
            return new ModalDialog
            {
                DisableBackButtonWhenModal = true,
                Content = new Views.Shell(service),
                ModalContent = new Views.Busy(),
            };            
        }

        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            Debug.WriteLine("OnInitializeAsync " + args.PreviousExecutionState + " " + DetermineStartCause(args));
            s1.Stop();
            Debug.WriteLine("Time: {0}ms", s1.ElapsedMilliseconds);
            s1.Start();
            if (ApplicationExecutionState.Terminated == args.PreviousExecutionState)
            {
                await SongCoverManager.Instance.Initialize(true);
            }
            else
            {
                await SongCoverManager.Instance.Initialize();
            }
           
            ColorsHelper ch = new ColorsHelper();
            ch.RestoreAppAccentColors();
            TranslationHelper tr = new TranslationHelper();
            tr.ChangeSlideableItemDescription();

            await ChangeStatusBarVisibility();
            ThemeHelper.ApplyAppTheme(ThemeHelper.IsLightTheme);

            #region AddPageKeys
            var keys = PageKeys<Pages>();
            if (!keys.ContainsKey(Pages.AddToPlaylist))
                keys.Add(Pages.AddToPlaylist, typeof(AddToPlaylistView));
            if (!keys.ContainsKey(Pages.Albums))
                keys.Add(Pages.Albums, typeof(AlbumsView));
            if (!keys.ContainsKey(Pages.Album))
                keys.Add(Pages.Album, typeof(AlbumView));
            if (!keys.ContainsKey(Pages.AlbumArtists))
                keys.Add(Pages.AlbumArtists, typeof(AlbumArtistsView));
            if (!keys.ContainsKey(Pages.AlbumArtist))
                keys.Add(Pages.AlbumArtist, typeof(AlbumArtistView));
            if (!keys.ContainsKey(Pages.Artists))
                keys.Add(Pages.Artists, typeof(ArtistsView));
            if (!keys.ContainsKey(Pages.Artist))
                keys.Add(Pages.Artist, typeof(ArtistView));
            if (!keys.ContainsKey(Pages.AudioSettings))
                keys.Add(Pages.AudioSettings, typeof(AudioSettingsView));
            if (!keys.ContainsKey(Pages.CloudStorageFolders))
                keys.Add(Pages.CloudStorageFolders, typeof(CloudStorageFoldersView));
            if (!keys.ContainsKey(Pages.CuteRadio))
                keys.Add(Pages.CuteRadio, typeof(CuteRadioView));
            if (!keys.ContainsKey(Pages.FileInfo))
                keys.Add(Pages.FileInfo, typeof(FileInfoView));
            if (!keys.ContainsKey(Pages.Folders))
                keys.Add(Pages.Folders, typeof(FoldersView));
            if (!keys.ContainsKey(Pages.FoldersRoot))
                keys.Add(Pages.FoldersRoot, typeof(FoldersRootView));
            if (!keys.ContainsKey(Pages.Genres))
                keys.Add(Pages.Genres, typeof(GenresView));
            if (!keys.ContainsKey(Pages.Licenses))
                keys.Add(Pages.Licenses, typeof(Licences));
            if (!keys.ContainsKey(Pages.Lyrics))
                keys.Add(Pages.Lyrics, typeof(LyricsView));
            if (!keys.ContainsKey(Pages.NewSmartPlaylist))
                keys.Add(Pages.NewSmartPlaylist, typeof(NewSmartPlaylistView));
            if (!keys.ContainsKey(Pages.NowPlaying))
                keys.Add(Pages.NowPlaying, typeof(NowPlayingView));
            if (!keys.ContainsKey(Pages.NowPlayingDesktop))
                keys.Add(Pages.NowPlayingDesktop, typeof(NowPlayingDesktopView));
            if (!keys.ContainsKey(Pages.NowPlayingPlaylist))
                keys.Add(Pages.NowPlayingPlaylist, typeof(NowPlayingPlaylistView));
            if (!keys.ContainsKey(Pages.Playlists))
                keys.Add(Pages.Playlists, typeof(PlaylistsView));
            if (!keys.ContainsKey(Pages.Playlist))
                keys.Add(Pages.Playlist, typeof(PlaylistView));
            if (!keys.ContainsKey(Pages.PlaylistEditable))
                keys.Add(Pages.PlaylistEditable, typeof(PlaylistEditableView));
            if (!keys.ContainsKey(Pages.Radios))
                keys.Add(Pages.Radios, typeof(RadiosView));
            if (!keys.ContainsKey(Pages.Settings))
                keys.Add(Pages.Settings, typeof(SettingsView2));
            if (!keys.ContainsKey(Pages.Songs))
                keys.Add(Pages.Songs, typeof(SongsView));
            if (!keys.ContainsKey(Pages.TagsEditor))
                keys.Add(Pages.TagsEditor, typeof(TagsEditor));
#endregion

            try
            {
                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated ||
                    args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
                    args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
                {
                    DisplayRequestHelper drh = new DisplayRequestHelper();
                    drh.ActivateIfEnabled();
                }
            }
            catch (Exception ex)
            {
            }

            
            s1.Stop();
            Debug.WriteLine("Time OnInitializeAsync End: {0}ms", s1.ElapsedMilliseconds);
            s1.Start();
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            Debug.WriteLine("OnStartAsync " + startKind + " " + args.PreviousExecutionState + " " + DetermineStartCause(args));
            s1.Stop();
            Debug.WriteLine("Time OnStartAsync Start: {0}ms", s1.ElapsedMilliseconds);
            s1.Start();
            if (startKind == StartKind.Launch)
            {
                //TelemetryAdapter.TrackAppLaunch();
            }

            if (isFirstRun)
            {
                isFirstRun = false;
                await NavigationService.NavigateAsync(Pages.Settings);
                return;
            }
            var fileArgs = args as FileActivatedEventArgs;
            if (fileArgs != null && fileArgs.Files.Any())
            {
                if (DeviceFamilyHelper.IsDesktop())
                {
                    await NavigationService.NavigateAsync(Pages.NowPlayingDesktop);
                }
                else
                {
                    await NavigationService.NavigateAsync(Pages.NowPlayingPlaylist);
                }
            }
            else
            {
                switch (DetermineStartCause(args))
                {
                    case AdditionalKinds.SecondaryTile:
                        LaunchActivatedEventArgs eventArgs = args as LaunchActivatedEventArgs;
                        if (!eventArgs.TileId.Contains(SettingsKeys.TileId))
                        {
                            Debug.WriteLine("OnStartAsync event arg doesn't contain tileid");
                            TelemetryAdapter.TrackEventException("event arg doesn't contain tileid");
                        }
                        Pages page = Pages.Playlists;
                        string parameter = eventArgs.Arguments;
                        MusicItemTypes type = MusicItem.ParseType(parameter);
                        // dodac wybor w ustawieniach, czy przejsc do widoku, czy zaczac odtwarzanie i przejsc do teraz odtwarzane
                        switch (type)
                        {
                            case MusicItemTypes.album:
                                page = Pages.Album;
                                parameter = MusicItem.SplitParameter(parameter)[1];
                                break;
                            case MusicItemTypes.albumartist:
                                page = Pages.AlbumArtist;
                                parameter = MusicItem.SplitParameter(parameter)[1];
                                break;
                            case MusicItemTypes.artist:
                                page = Pages.Artist;
                                parameter = MusicItem.SplitParameter(parameter)[1];
                                break;
                            case MusicItemTypes.folder:
                                page = Pages.Playlist;
                                break;
                            case MusicItemTypes.genre:
                                page = Pages.Playlist;
                                break;
                            case MusicItemTypes.plainplaylist:
                                page = Pages.Playlist;
                                break;
                            case MusicItemTypes.smartplaylist:
                                page = Pages.Playlist;
                                break;
                            case MusicItemTypes.song:
                                //page = Pages.NowPlaying; ?
                                break;
                            case MusicItemTypes.unknown:
                                TelemetryAdapter.TrackEventException("MusicItemTypes.unknown");
                                break;
                            default:
                                break;
                        }
                        TelemetryAdapter.TrackEvent("LaunchFromSecondaryTile " + type.ToString());
                        await NavigationService.NavigateAsync(page, parameter);
                        break;
                    case AdditionalKinds.Primary:
                        if (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
                            args.PreviousExecutionState == ApplicationExecutionState.NotRunning || 
                            args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                        {
                            Debug.WriteLine("OnStartAsync Primary closed");
                            await NavigationService.NavigateAsync(Pages.Playlists);
                        }
                        else
                        {
                            Debug.WriteLine("OnStartAsync Primary not closed");
                        }
                        break;
                    case AdditionalKinds.Toast:
                        Debug.WriteLine("OnStartAsync Toast");
                        var toastargs = args as ToastNotificationActivatedEventArgs;
                        await NavigationService.NavigateAsync(Pages.Settings);
                        break;
                    default:
                        Debug.WriteLine("OnStartAsync default");
                        await NavigationService.NavigateAsync(Pages.Playlists);
                        break;
                }
            }
            
            OnStartAsyncFinished?.Invoke(args.PreviousExecutionState, fileArgs);
            s1.Stop();
            Debug.WriteLine("Time OnStartAsync End: {0}ms", s1.ElapsedMilliseconds);
            s1.Start();
        }

        private delegate void OnStartAsyncFinishedHandler(ApplicationExecutionState state, FileActivatedEventArgs fileArgs);
        private event OnStartAsyncFinishedHandler OnStartAsyncFinished;

        public static FileActivatedEventArgs FileArgs;

        private async void InitAfterOnStart(ApplicationExecutionState state, FileActivatedEventArgs fileArgs)
        {
            s1.Stop();
            Debug.WriteLine("InitAfterOnStart: {0}ms", s1.ElapsedMilliseconds);
            s1.Start();
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
                    await TileManager.ManageSecondaryTileImages();
                }
                catch (Exception ex)
                {

                }
            }
            s1.Stop();
            Debug.WriteLine("InitAfterOnStart: End {0}ms", s1.ElapsedMilliseconds);
        }

        public override async Task OnSuspendingAsync(object s, SuspendingEventArgs e, bool prelaunch)
        {            
            Logger2.Current.WriteMessage("OnSuspendingAsync");
            await Logger2.Current.WriteToFile();
            await base.OnSuspendingAsync(s, e, prelaunch);
        }

#endregion

        private void Resetdb()
        {
            DatabaseManager.Current.resetdb();
        }

        private async Task RegisterBGScrobbler()
        {
            await BackgroundTaskHelper.CheckAppVersion();
            await BackgroundTaskHelper.RegisterBackgroundTasks();
        }
    }
}
