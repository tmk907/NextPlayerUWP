using Microsoft.HockeyApp;
using NextPlayerUWP.Common;
using NextPlayerUWP.Views;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Diagnostics;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Controls;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;

namespace NextPlayerUWP
{
    public delegate void SongUpdatedHandler(int id);
    public delegate void AppThemeChangedHandler(bool isLight);

    sealed partial class App : BootStrapper
    {
        public static event SongUpdatedHandler SongUpdated;
        public static void OnSongUpdated(int id)
        {
            SongUpdated?.Invoke(id);
        }
        public static event AppThemeChangedHandler AppThemeChanged;
        public static void OnAppThemeChanged(bool isLight)
        {
            AppThemeChanged?.Invoke(isLight);
        }

        public static event EventHandler MemoryUsageReduced;
        public static void OnMemoryUsageReduced()
        {
            MemoryUsageReduced?.Invoke(null, null);
        }

        private const int dbVersion = 10;

        public static bool IsLightThemeOn = false;

        private static AlbumArtFinder albumArtFinder;
        public static AlbumArtFinder AlbumArtFinder
        {
            get
            {
                if (albumArtFinder == null) albumArtFinder = new AlbumArtFinder();
                return albumArtFinder;
            }
        }
        public static FileFormatsHelper FileFormatsHelper;

        private bool isFirstRun = false;

        bool _isInBackgroundMode = false;        

        public App()
        {
            InitializeComponent();
            this.EnteredBackground += App_EnteredBackground;
            this.LeavingBackground += App_LeavingBackground;
            MemoryManager.AppMemoryUsageLimitChanging += MemoryManager_AppMemoryUsageLimitChanging;
            MemoryManager.AppMemoryUsageIncreased += MemoryManager_AppMemoryUsageIncreased;
            HockeyClient.Current.Configure(AppConstants.HockeyAppId);

            object o = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.FirstRun);
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
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.FirstRun, false);
            }

            var t = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppTheme);
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
                RequestedTheme = ApplicationTheme.Light;
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppTheme, true);
            }

            SplashFactory = (e) => new Views.Splash(e);
#if DEBUG
            try
            {
                Logger.SaveFromSettingsToFile();
            }
            catch (Exception ex)
            {

            }
#else
            Logger.ClearSettingsLogs();
#endif

            //if (IsXbox())
            //{
            //    Application.Current.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
            //}

            //DatabaseManager.Current.resetdb();
            FileFormatsHelper = new FileFormatsHelper(false);
            this.UnhandledException += App_UnhandledException;

            //var gcTimer = new DispatcherTimer();
            //gcTimer.Tick += (sender, e) => { GC.Collect(); };
            //gcTimer.Interval = TimeSpan.FromSeconds(1);
            //gcTimer.Start();
        }

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
                    var a = WindowWrapper.Current().NavigationServices.Count;
                }
                else
                {
                    //?
                }
                Window.Current.Content = null;
                GC.Collect();
            }
        }

        private void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            Debug.WriteLine("App App_EnteredBackground");
            Debug.WriteLine("Memory Usage={0} level={1} limit={2}", MemoryManager.AppMemoryUsage, MemoryManager.AppMemoryUsageLevel, MemoryManager.AppMemoryUsageLimit);

            var deferral = e.GetDeferral();
            _isInBackgroundMode = true;
            try
            {
#if DEBUG
                //If we are in debug mode free memory here because the memory limits are turned off
                //In release builds defer the actual reduction of memory to the limit changing event so we don't 
                //unnecessarily throw away the UI

                //ReduceMemoryUsage(0);
#endif
            }
            finally
            {
                deferral.Complete();
            }
        }

        bool isBackgroundLeavedFirstTime = true;
        private async void App_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            Debug.WriteLine("App App_LeavingBackground");
            _isInBackgroundMode = false;

            // Reastore view content if it was previously unloaded.
            if (!isBackgroundLeavedFirstTime && Window.Current != null && Window.Current.Content == null)
            {
                var deferral = e.GetDeferral();
                var service = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
                await service.RestoreSavedNavigationAsync();
                Window.Current.Content = new ModalDialog
                {
                    DisableBackButtonWhenModal = true,
                    Content = new Views.Shell(service),
                    ModalContent = new Views.Busy(),
                };
                deferral.Complete();
            }
            isBackgroundLeavedFirstTime = false;
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.SaveInSettings("App_UnhandledException " + e.Exception);
            HockeyClient.Current.TrackEvent("App_UnhandledException " + e.Exception.Message);
        }

        public enum Pages
        {
            AddToPlaylist,
            Albums,
            Album,
            Artists,
            Artist,
            AudioSettings,
            CloudStorageFolders,
            Genres,
            FileInfo,
            Folders,
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

            if (!isFirstRun && args.PreviousExecutionState != ApplicationExecutionState.Running && args.PreviousExecutionState != ApplicationExecutionState.Suspended)
            {
                PerformUpdate();
            }

            if (ApplicationExecutionState.Terminated == args.PreviousExecutionState)
            {
                await SongCoverManager.Instance.Initialize(true);
            }
            else
            {
                await SongCoverManager.Instance.Initialize();
            }

            ColorsHelper ch = new ColorsHelper();
            ch.RestoreUserAccentColors();
            TranslationHelper tr = new TranslationHelper();
            tr.ChangeSlideableItemDescription();

            await ChangeStatusBarVisibility();
            bool isLightTheme = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppTheme);
            ThemeHelper.ApplyThemeToStatusBar(isLightTheme);
            ThemeHelper.ApplyThemeToTitleBar(isLightTheme);

            #region AddPageKeys
            var keys = PageKeys<Pages>();
            if (!keys.ContainsKey(Pages.AddToPlaylist))
                keys.Add(Pages.AddToPlaylist, typeof(AddToPlaylistView));
            if (!keys.ContainsKey(Pages.Albums))
                keys.Add(Pages.Albums, typeof(AlbumsView));
            if (!keys.ContainsKey(Pages.Album))
                keys.Add(Pages.Album, typeof(AlbumView));
            if (!keys.ContainsKey(Pages.Artists))
                keys.Add(Pages.Artists, typeof(ArtistsView));
            if (!keys.ContainsKey(Pages.Artist))
                keys.Add(Pages.Artist, typeof(ArtistView));
            if (!keys.ContainsKey(Pages.AudioSettings))
                keys.Add(Pages.AudioSettings, typeof(AudioSettingsView));
            if (!keys.ContainsKey(Pages.CloudStorageFolders))
                keys.Add(Pages.CloudStorageFolders, typeof(CloudStorageFoldersView));
            if (!keys.ContainsKey(Pages.FileInfo))
                keys.Add(Pages.FileInfo, typeof(FileInfoView));
            if (!keys.ContainsKey(Pages.Folders))
                keys.Add(Pages.Folders, typeof(FoldersView));
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
                keys.Add(Pages.Settings, typeof(SettingsView));
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
                //Logger.SaveInSettings("OnInitializeAsync DisplayRequestHelper " + ex);
            }
            //await MediaImport.CheckChanges();
            if (!isFirstRun)
            {
                AlbumArtFinder.StartLooking().ConfigureAwait(false);
            }
        }   

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            Debug.WriteLine("OnStartAsync " + startKind + " " + args.PreviousExecutionState + " " + DetermineStartCause(args));

            if (startKind == StartKind.Launch)
            {
                TelemetryAdapter.TrackAppLaunch();
            }

            if (isFirstRun)
            {
                isFirstRun = false;
                await NavigationService.NavigateAsync(Pages.Settings);
                return;
            }
            else
            {
                if (PlaybackService.Instance.PlayerState != Windows.Media.Playback.MediaPlaybackState.Playing) Common.Tiles.TileUpdateHelper.ClearTile();//!!
            }

            var fileArgs = args as FileActivatedEventArgs;
            if (fileArgs != null && fileArgs.Files.Any())
            {
                var file = fileArgs.Files.First() as StorageFile;
                await OpenFileAndPlay(file);

                if (DeviceFamilyHelper.IsDesktop())
                {
                    await NavigationService.NavigateAsync(Pages.NowPlayingDesktop);
                }
                else
                {
                    await NavigationService.NavigateAsync(Pages.NowPlayingPlaylist);
                }
                if (fileArgs.Files.Count > 1)
                {
                    await OpenFilesAndAddToNowPlaying(fileArgs.Files.Skip(1));
                }
            }
            else
            {
                switch (DetermineStartCause(args))
                {
                    case AdditionalKinds.SecondaryTile:
                        LaunchActivatedEventArgs eventArgs = args as LaunchActivatedEventArgs;
                        if (!eventArgs.TileId.Contains(AppConstants.TileId))
                        {
                            //Logger.Save("event arg doesn't contain tileid " + Environment.NewLine + eventArgs.TileId + Environment.NewLine + eventArgs.Arguments);
                            //Logger.SaveToFile();
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
                            //Logger.SaveInSettings("Onstart from Primary " + args.PreviousExecutionState);
                            //await NavigationService.NavigateAsync(Pages.Playlists);
                        }
                        break;
                    case AdditionalKinds.Toast:
                        Debug.WriteLine("OnStartAsync Toast");
                        var toastargs = args as ToastNotificationActivatedEventArgs;
                        await NavigationService.NavigateAsync(Pages.Settings);
                        break;
                    default:
                        Debug.WriteLine("OnStartAsync default");
                        //Logger.Save("OnStart default");
                        //Logger.SaveToFile();
                        //if (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
                        //    args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
                        //{
                            await NavigationService.NavigateAsync(Pages.Playlists);
                        //}
                        break;
                }
            }

            
            if (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
                args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
            {
                await RegisterBGScrobbler();
                await TileManager.ManageSecondaryTileImages();               
            }
        }
        
        public override Task OnSuspendingAsync(object s, SuspendingEventArgs e, bool prelaunch)
        {
            if (OnNewTilePinned != null)
            {
                OnNewTilePinned();
                OnNewTilePinned = null;
            }
            return base.OnSuspendingAsync(s, e, prelaunch);
        }

        //public override void OnResuming(object s, object e, AppExecutionState previousExecutionState)
        //{
        //    base.OnResuming(s, e, previousExecutionState);
        //}

        //public override Task OnPrelaunchAsync(IActivatedEventArgs args, out bool runOnStartAsync)
        //{
        //    runOnStartAsync = true;
            
        //    object o = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.FirstRun);
        //    if (o == null) return Task.CompletedTask;

        //    var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();

        //    return Task.CompletedTask;
        //}

        #endregion

        public static Action OnNewTilePinned { get; set; }

        #region Setup and update

        private void FirstRunSetup()
        {
            DatabaseManager.Current.CreateNewDatabase();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, dbVersion);
            CreateDefaultSmartPlaylists();

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, false);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerTime, 0);

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppTheme, true);
            var color = Windows.UI.Color.FromArgb(255, 0, 120, 215);
            ColorsHelper ch = new ColorsHelper();
            ch.SaveUserAccentColor(color);
            //ch.SetAccentColorShades(color);

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterDropItem, AppConstants.ActionAddToNowPlaying);

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmRateSongs, true);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmLove, 4);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmSendNP, false);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmLogin, "");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmPassword, "");

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DisableLockscreen, false);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.HideStatusBar, false);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IncludeSubFolders, false);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.PlaylistsFolder, "");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AutoSavePlaylists, true);

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.FlipViewSelectedIndex, 0);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterSwipeRightCommand, AppConstants.SwipeActionDelete);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterSwipeLeftCommand, AppConstants.SwipeActionAddToNowPlaying);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.EnableLiveTileWithImage, true);

            ApplicationSettingsHelper.SaveSettingsValue("ImportPlaylistsAfterAppUpdate9", true);
            Debug.WriteLine("FirstRunSetup finished");
        }

        private void CreateDefaultSmartPlaylists()
        {
            int i;
            i = DatabaseManager.Current.InsertSmartPlaylist("Ostatnio dodane", 100, SPUtility.SortBy.MostRecentlyAdded);
            DatabaseManager.Current.InsertSmartPlaylistEntry(i, SPUtility.Item.DateAdded, SPUtility.Comparison.IsGreater, DateTime.Now.Subtract(TimeSpan.FromDays(14)).Ticks.ToString(), SPUtility.Operator.Or);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.OstatnioDodane, i);
            i = DatabaseManager.Current.InsertSmartPlaylist("Ostatnio odtwarzane", 100, SPUtility.SortBy.MostRecentlyPlayed);
            DatabaseManager.Current.InsertSmartPlaylistEntry(i, SPUtility.Item.LastPlayed, SPUtility.Comparison.IsGreater, DateTime.MinValue.Ticks.ToString(), SPUtility.Operator.Or);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.OstatnioOdtwarzane, i);
            i = DatabaseManager.Current.InsertSmartPlaylist("Najczęściej odtwarzane", 100, SPUtility.SortBy.MostOftenPlayed);
            DatabaseManager.Current.InsertSmartPlaylistEntry(i, SPUtility.Item.PlayCount, SPUtility.Comparison.IsGreater, "0", SPUtility.Operator.Or);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajczesciejOdtwarzane, i);
            i = DatabaseManager.Current.InsertSmartPlaylist("Najlepiej oceniane", 100, SPUtility.SortBy.HighestRating);
            DatabaseManager.Current.InsertSmartPlaylistEntry(i, SPUtility.Item.Rating, SPUtility.Comparison.IsGreater, "3", SPUtility.Operator.Or);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajlepiejOceniane, i);
            i = DatabaseManager.Current.InsertSmartPlaylist("Najrzadziej odtwarzane", 100, SPUtility.SortBy.LeastOftenPlayed);
            DatabaseManager.Current.InsertSmartPlaylistEntry(i, SPUtility.Item.PlayCount, SPUtility.Comparison.IsGreater, "-1", SPUtility.Operator.Or);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajrzadziejOdtwarzane, i);
            i = DatabaseManager.Current.InsertSmartPlaylist("Najgorzej oceniane", 100, SPUtility.SortBy.LowestRating);
            DatabaseManager.Current.InsertSmartPlaylistEntry(i, SPUtility.Item.Rating, SPUtility.Comparison.IsLess, "4", SPUtility.Operator.Or);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajgorzejOceniane, i);
        }

        private void PerformUpdate()
        {
            UpdateDB();
            UpdateApp();
        }

        private void UpdateDB()
        {
            //ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 8);
            object version = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.DBVersion);
            if (version.ToString() == "1")
            {
                DatabaseManager.Current.UpdateToVersion2();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 2);
                version = "2";
            }
            if (version.ToString() == "2")
            {
                bool recreate = DatabaseManager.Current.DBCorrection();
                if (recreate)
                {
                    CreateDefaultSmartPlaylists();
                }
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 3);
                version = "3";
            }
            if (version.ToString() == "3")
            {
                DatabaseManager.Current.UpdateToVersion4();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 4);
                version = "4";
            }
            if (version.ToString() == "4")
            {
                DatabaseManager.Current.UpdateToVersion5();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 5);
                version = "5";
            }
            if (version.ToString() == "5")
            {
                DatabaseManager.Current.UpdateToVersion6();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 6);
                version = "6";
            }
            if (version.ToString() == "6")
            {
                DatabaseManager.Current.UpdateToVersion7();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 7);
                version = "7";
            }
            if (version.ToString() == "7")
            {
                DatabaseManager.Current.UpdateToVersion8();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 8);
                version = "8";
            }
            if (version.ToString() == "8")
            {
                DatabaseManager.Current.UpdateToVersion9();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 9);
                version = "9";
            }
            if (version.ToString() == "9")
            {
                DatabaseManager.Current.UpdateToVersion10();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 10);
                version = "10";
            }
            // change  private const int dbVersion
        }

        private void UpdateApp()
        {
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.DisableLockscreen) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DisableLockscreen, false);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.HideStatusBar) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.HideStatusBar, false);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IncludeSubFolders) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IncludeSubFolders, false);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.PlaylistsFolder) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.PlaylistsFolder, "");
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AutoSavePlaylists) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AutoSavePlaylists, true);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.ActionAfterSwipeRightCommand) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterSwipeRightCommand, AppConstants.SwipeActionDelete);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.ActionAfterSwipeLeftCommand) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterSwipeLeftCommand, AppConstants.SwipeActionAddToNowPlaying);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.FlipViewSelectedIndex) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.FlipViewSelectedIndex, 0);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.EnableLiveTileWithImage) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.EnableLiveTileWithImage, true);
            }
        }

        #endregion

        private async Task SendLogs()
        {
            try
            {
                string log = await Logger.Read();
                if (!string.IsNullOrEmpty(log))
                {
                    
                }
                log = await Logger.ReadBG();
                if (!string.IsNullOrEmpty(log))
                {
                    
                }
                await Logger.ClearAll();
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackEvent("Cant send logs: " + ex);
            }
        }

        private void Resetdb()
        {
            DatabaseManager.Current.resetdb();
        }

        public static void ChangeRightPanelVisibility(bool visible)
        {
            if (Window.Current.Content == null) return;
            ((Shell)((ModalDialog)Window.Current.Content).Content).ChangeRightPanelVisibility(visible);
        }

        public static void OnNavigatedToNewView(bool visible, bool isNowPlayingDesktopActive = false)
        {
            if (Window.Current.Content == null) return;
            ((Shell)((ModalDialog)Window.Current.Content).Content).ChangeBottomPlayerVisibility(visible);
            ((Shell)((ModalDialog)Window.Current.Content).Content).OnDesktopViewActiveChange(isNowPlayingDesktopActive);
        }

        public static async Task ChangeStatusBarVisibility()
        {
            bool hide = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.HideStatusBar);
            await ChangeStatusBarVisibility(hide);
        }

        public static async Task ChangeStatusBarVisibility(bool hide)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                if (hide)
                {
                    await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
                }
                else
                {
                    await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ShowAsync();
                }
            }
        }

        private async Task RegisterBGScrobbler()
        {
            await BackgroundTaskHelper.CheckAppVersion();
            await BackgroundTaskHelper.RegisterBackgroundTasks();
        }

        private async Task OpenFileAndPlay(StorageFile file)
        {
            MediaImport mi = new MediaImport(FileFormatsHelper);
            string type = file.FileType.ToLower();
            if (FileFormatsHelper.IsFormatSupported(type))
            {
                SongItem si = await mi.OpenSingleFileAsync(file);
                await NowPlayingPlaylistManager.Current.NewPlaylist(si);
                await PlaybackService.Instance.PlayNewList(0);
            }
            else if (FileFormatsHelper.IsPlaylistSupportedType(type))
            {
                var playlist = await mi.OpenPlaylistFileAsync(file);
                if (playlist != null)
                {
                    await NowPlayingPlaylistManager.Current.NewPlaylist(playlist);
                    await PlaybackService.Instance.PlayNewList(0);
                }
            }
        }

        private async Task OpenFilesAndAddToNowPlaying(IEnumerable<IStorageItem> files)
        {
            MediaImport mi = new MediaImport(FileFormatsHelper);
            List<SongItem> list = new List<SongItem>();
            int i = 0;
            const int size = 4;
            foreach(var file in files)
            {
                var si = await mi.OpenSingleFileAsync(file as StorageFile);
                list.Add(si);
                if (i == size)
                {
                    await NowPlayingPlaylistManager.Current.Add(list);
                    list.Clear();
                    i = 0;
                }
                else
                {
                    i++;
                }
            }
            if (list.Count > 0)
            {
                await NowPlayingPlaylistManager.Current.Add(list);
            }
        }

        private static List<SongItem> temporaryList = new List<SongItem>();
        public static void AddToCache(List<SongItem> songs)
        {
            temporaryList = new List<SongItem>();
            foreach(var song in songs)
            {
                temporaryList.Add(song);
            }
        }
        public static List<SongItem> GetFromCache()
        {
            return temporaryList;
        }
        public static void ClearCache()
        {
            temporaryList = new List<SongItem>();
        }
    }
}
