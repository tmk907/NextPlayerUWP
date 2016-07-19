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
        public static void OnAppThemChanged(bool isLight)
        {
            AppThemeChanged?.Invoke(isLight);
        }

        private const int dbVersion = 5;

        public static bool IsLightThemeOn = false;
        private static AlbumArtFinder albumArtFinder;

        private bool isFirstRun = false;

        private static PlaybackManager playbackManager;
        public static PlaybackManager PlaybackManager
        {
            get
            {
                if (playbackManager == null) playbackManager = new PlaybackManager();
                return playbackManager;
            }
        }

        public App()
        {
            InitializeComponent();
            HockeyClient.Current.Configure(AppConstants.HockeyAppId);

            object o = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.FirstRun);
            if (null == o)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.FirstRun, false);
                isFirstRun = true;
            }
            else
            {
                isFirstRun = false;
            }

            if (isFirstRun)
            {
                FirstRunSetup();                
            }
            else
            {
                UpdateDB();
                UpdateApp();
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
            albumArtFinder = new AlbumArtFinder();
            
            this.UnhandledException += App_UnhandledException;
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
            Genres,
            FileInfo,
            Folders,
            Lyrics,
            Licenses,
            NewSmartPlaylist,
            NowPlaying,
            NowPlayingPlaylist,
            Playlists,
            Playlist,
            Radios,
            Settings,
            Songs,
            TagsEditor
        }

        public override UIElement CreateRootElement(IActivatedEventArgs e)
        {
            var service = NavigationServiceFactory(BackButton.Attach, ExistingContent.Exclude);
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

            if (!isFirstRun)
            {
                await PerformUpdate();
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

            try
            {
                await ChangeStatusBarVisibility();
            }
            catch (Exception ex)
            {
                //Logger.SaveInSettings("OnInitializeAsync ChangeStatusBarVisibility " + ex);
                //throw;
            }

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
            if (!keys.ContainsKey(Pages.NowPlayingPlaylist))
                keys.Add(Pages.NowPlayingPlaylist, typeof(NowPlayingPlaylistView));
            if (!keys.ContainsKey(Pages.Playlists))
                keys.Add(Pages.Playlists, typeof(PlaylistsView));
            if (!keys.ContainsKey(Pages.Playlist))
                keys.Add(Pages.Playlist, typeof(PlaylistView));
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
                //throw;
            }

            if (!isFirstRun)
            {
                //Debug.WriteLine("before albumArtFinder.StartLooking");
                albumArtFinder.StartLooking();
                //Debug.WriteLine("after albumArtFinder.StartLooking");
            }
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            Debug.WriteLine("OnStartAsync " + startKind + " " + args.PreviousExecutionState + " " + DetermineStartCause(args));

            if (isFirstRun)
            {
                isFirstRun = false;
                await NavigationService.NavigateAsync(Pages.Settings);
                return;
            }

            var fileArgs = args as FileActivatedEventArgs;
            if (fileArgs != null && fileArgs.Files.Any())
            {
                var file = fileArgs.Files.First() as StorageFile;
                await OpenFileAndPlay(file);

                await NavigationService.NavigateAsync(Pages.NowPlayingPlaylist);
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
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppState, Enum.GetName(typeof(AppState), AppState.Suspended));
            
            if (OnNewTilePinned != null)
            {
                OnNewTilePinned();
                OnNewTilePinned = null;
            }
            return base.OnSuspendingAsync(s, e, prelaunch);
        }

        public override void OnResuming(object s, object e, AppExecutionState previousExecutionState)
        {
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppState, Enum.GetName(typeof(AppState), AppState.Active));
            
            base.OnResuming(s, e, previousExecutionState);
        }

        public override Task OnPrelaunchAsync(IActivatedEventArgs args, out bool runOnStartAsync)
        {
            runOnStartAsync = true;
            
            object o = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.FirstRun);
            if (o == null) return Task.CompletedTask;

            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();

            return Task.CompletedTask;
        }

        public static Action OnNewTilePinned { get; set; }

        private void FirstRunSetup()
        {
            DatabaseManager.Current.CreateDatabase();
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

        private async Task PerformUpdate()
        {
            if (null == ApplicationSettingsHelper.ReadSettingsValue("DatabaseMovedToLocalCacheFolder"))
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(AppConstants.DBFileName);
                await file.CopyAsync(ApplicationData.Current.LocalCacheFolder);
                await file.DeleteAsync();
                ApplicationSettingsHelper.SaveSettingsValue("DatabaseMovedToLocalCacheFolder",true);
            }
            UpdateDB();
            UpdateApp();
        }

        private void UpdateDB()
        {
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
        }

        private async Task SendLogs()
        {
            try
            {
                string log = await Logger.Read();
                if (!string.IsNullOrEmpty(log))
                {
                    HockeyClient.Current.TrackEvent("FG Error log:" + Environment.NewLine + log);
                }
                log = await Logger.ReadBG();
                if (!string.IsNullOrEmpty(log))
                {
                    HockeyClient.Current.TrackEvent("BG error log:" + Environment.NewLine + log);
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

        public static void ChangeBottomPlayerVisibility(bool visible)
        {
            if (Window.Current.Content == null) return;
            ((Shell)((ModalDialog)Window.Current.Content).Content).ChangeBottomPlayerVisibility(visible);
        }

        public static async Task ChangeStatusBarVisibility()
        {
            bool hide = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.HideStatusBar);
            await ChangeStatusBarVisibility(hide);
        }

        public static async Task ChangeStatusBarVisibility(bool hide)
        {
            var statusbar = "Windows.UI.ViewManagement.StatusBar";
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(statusbar))
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
            MediaImport mi = new MediaImport();
            string type = file.FileType.ToLower();
            if (MediaImport.IsAudioFile(type))
            {
                SongItem si = await mi.OpenSingleFileAsync(file);
                ApplicationSettingsHelper.SaveSongIndex(0);
                await NowPlayingPlaylistManager.Current.NewPlaylist(si);
                App.PlaybackManager.PlayNew();
            }
            else if (MediaImport.IsPlaylistFile(type))
            {
                var list = await mi.OpenPlaylistFileAsync(file);
                ApplicationSettingsHelper.SaveSongIndex(0);
                await NowPlayingPlaylistManager.Current.NewPlaylist(list);
                App.PlaybackManager.PlayNew();
            }
        }

        private async Task OpenFilesAndAddToNowPlaying(IEnumerable<IStorageItem> files)
        {
            MediaImport mi = new MediaImport();
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
    }
}
