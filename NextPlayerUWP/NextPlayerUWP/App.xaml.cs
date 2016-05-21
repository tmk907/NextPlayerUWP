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
using System.Diagnostics;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Controls;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Globalization;
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
        

        public static bool IsLightThemeOn = false;
        private static AlbumArtFinder albumArtFinder;

        public App()
        {
            InitializeComponent();
            
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
            }


            HockeyClient.Current.Configure(AppConstants.HockeyAppId);

            if (IsFirstRun())
            {
                FirstRunSetup();
            }
            else
            {
                UpdateDB();
            }

            SplashFactory = (e) => new Views.Splash(e);

            try
            {
                Logger.SaveFromSettingsToFile();
            }
            catch (Exception ex)
            {

            }
            albumArtFinder = new AlbumArtFinder();
            this.UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Save("App_UnhandledException " + e.Exception);
            Logger.SaveToFile();
            HockeyClient.Current.TrackEvent("App_UnhandledException " + e.Exception);
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
            NowPlaying,
            NowPlayingPlaylist,
            Playlists,
            Playlist,
            Radios,
            Settings,
            Songs,
            TagsEditor
        }

        private bool NeedsNewNavigationService(IActivatedEventArgs args)
        {
            switch (args.PreviousExecutionState)
            {
                case ApplicationExecutionState.ClosedByUser:
                case ApplicationExecutionState.NotRunning:
                case ApplicationExecutionState.Terminated:
                    return true;
                case ApplicationExecutionState.Running:
                case ApplicationExecutionState.Suspended:
                    return false;
            }
            return true;
        }

        //public override UIElement CreateRootElement(IActivatedEventArgs e)
        //{
        //    var service = NavigationServiceFactory(BackButton.Attach, ExistingContent.Exclude);
        //    return new ModalDialog
        //    {
        //        DisableBackButtonWhenModal = true,
        //        Content = new Views.Shell(service),
        //        ModalContent = new Views.Busy(),
        //    };
        //}

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
            Debug.WriteLine("OnInitializeAsync");

            //Logger.Save("OnInitializeAsync null " + args.Kind + " " + args.PreviousExecutionState);
            //Logger.SaveToFile();
            ColorsHelper ch = new ColorsHelper();
            ch.RestoreUserAccentColors();
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
            //DispatcherHelper.Initialize();

            //Logger.Save("OnInitializeAsync null " + args.Kind + " " + args.PreviousExecutionState);

            //if (Window.Current.Content as ModalDialog == null)
            //{
            //    var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
            //    // create modal root
            //    Window.Current.Content = new ModalDialog
            //    {
            //        DisableBackButtonWhenModal = true,
            //        Content = new Views.Shell(nav),
            //        ModalContent = new Views.Busy(),
            //    };
            //}

            //if (NeedsNewNavigationService(args))
            //{
            //    var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
            //    var s = BootStrapper.Current.NavigationService.FrameFacade.GetNavigationState();
            //    if (Window.Current.Content as Shell == null)
            //    {
            //        Window.Current.Content = new Views.Shell(nav);
            //    }
            //    //Logger.Save("NeedsNewNavigationService");
            //}
            //else
            //{

            //}
            //Logger.SaveToFile();

            try
            {
                //if (Window.Current.Content as Shell == null)
                //{
                //    //Logger.Save("OnInitializeAsync null " + args.Kind + " " + args.PreviousExecutionState);
                //    //Logger.SaveToFile();
                //    var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
                //    //Logger.Save("OnInitializeAsync 2 ");
                //    //Logger.SaveToFile();
                //    Window.Current.Content = new Views.Shell(nav);

                //    DispatcherHelper.Initialize();
                //}
                //else
                //{
                //    //Logger.Save("OnInitializeAsync not null " + args.Kind + " " + args.PreviousExecutionState);
                //    //Logger.SaveToFile();
                //}
                
            }
            catch (Exception ex)
            {
                Logger.Save("OnInitializeAsync Exception" + Environment.NewLine + ex);
                Logger.SaveToFile();
            }

            //await JamendoTest.Start();

            await Task.CompletedTask;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            Debug.WriteLine("OnStartAsync " + startKind + " " + args.PreviousExecutionState + " " + DetermineStartCause(args));
            Logger.Save("OnStartAsync " + startKind + " " + args.PreviousExecutionState + " " + DetermineStartCause(args));
            Logger.SaveToFile();
            await SongCoverManager.Instance.Initialize();
            if (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
                args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
            {
                await TileManager.ManageSecondaryTileImages();
                if (!IsFirstRun())
                {
                    Debug.WriteLine("before albumArtFinder.StartLooking");
                    albumArtFinder.StartLooking();
                    Debug.WriteLine("after albumArtFinder.StartLooking");
                }
            }

            switch (DetermineStartCause(args))
            {
                case AdditionalKinds.SecondaryTile:
                    LaunchActivatedEventArgs eventArgs = args as LaunchActivatedEventArgs;
                    if (!eventArgs.TileId.Contains(AppConstants.TileId))
                    {
                        Logger.Save("event arg doesn't contain tileid " + Environment.NewLine + eventArgs.TileId + Environment.NewLine + eventArgs.Arguments);
                        Logger.SaveToFile();
                        HockeyClient.Current.TrackEvent("event arg doesn't contain tileid");
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
                        default:
                            break;
                    }
                    await NavigationService.NavigateAsync(page, parameter);
                    break;
                case AdditionalKinds.Primary:
                    if (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
                        args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
                    {
                        //Logger.Save("OnStart primary navigate");
                        //Logger.SaveToFile();
                        await NavigationService.NavigateAsync(Pages.Playlists);
                    }
                    //Logger.Save("OnStart primary ");
                    //Logger.SaveToFile();
                    break;
                case AdditionalKinds.Toast:
                    var toastargs = args as ToastNotificationActivatedEventArgs;

                    await NavigationService.NavigateAsync(Pages.Playlists);
                    break;
                default:
                    //Logger.Save("OnStart default");
                    //Logger.SaveToFile();
                    if (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
                        args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
                    {
                        await NavigationService.NavigateAsync(Pages.Playlists);
                    }
                    break;
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

        public override async void OnResuming(object s, object e, AppExecutionState previousExecutionState)
        {
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppState, Enum.GetName(typeof(AppState), AppState.Active));
            if (previousExecutionState == AppExecutionState.Terminated)
            {
                await SongCoverManager.Instance.Initialize(true);
            }
            base.OnResuming(s, e, previousExecutionState);
        }

        public override Task OnPrelaunchAsync(IActivatedEventArgs args, out bool runOnStartAsync)
        {
            runOnStartAsync = true;
            Logger.Save("OnPrelaunchAsync " + DateTime.Now);
            Logger.SaveToFile();
            object o = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.FirstRun);
            if (o == null) return Task.CompletedTask;
            var song = NowPlayingPlaylistManager.Current.GetCurrentPlaying();
            try
            {
                //SongCoverManager.Instance.Initialize().Wait();
            }
            catch (Exception ex)
            {
                Logger.Save("OnPrelaunchAsync " + ex);
                Logger.SaveToFile();
            }
            return Task.CompletedTask;
        }

        private bool IsFirstRun()
        {
            object o = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.FirstRun);
            if (null == o)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.FirstRun, false);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static Action OnNewTilePinned { get; set; }

        private void FirstRunSetup()
        {
            DatabaseManager.Current.CreateDatabase();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 2);
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
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmLove, 5);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmUnLove, 1);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmSendNP, false);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmLogin, "");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmPassword, "");

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

        public void ChangeTelemetry(bool enable)
        {
            //TODO
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


        private void UpdateDB()
        {
            object version = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.DBVersion);
            if (version.ToString() == "1")
            {
                DatabaseManager.Current.UpdateToVersion2();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 2);
            }
        }
    }
}
