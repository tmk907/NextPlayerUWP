using Microsoft.HockeyApp;
using NextPlayerUWP.Common;
using NextPlayerUWP.Views;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Diagnostics;
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

        public App()
        {
            InitializeComponent();
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
                    //var a = WindowWrapper.Current().NavigationServices.Count;
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
            
            appShortcuts.DeregisterShortcuts();
            try
            {
#if DEBUG
                //If we are in debug mode free memory here because the memory limits are turned off
                //In release builds defer the actual reduction of memory to the limit changing event so we don't 
                //unnecessarily throw away the UI

                ReduceMemoryUsage(0);
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

            appShortcuts.RegisterShortcuts();
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //Logger.SaveInSettings("App_UnhandledException " + e.Exception);
            Logger2.Current.LogAppUnhadledException(e);
        }

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
            bool isLightTheme = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.AppTheme);
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

            try
            {
                AdDuplex.AdDuplexClient.Initialize("bfe9d689-7cf7-4add-84fe-444dc72e6f36");
            }
            catch (Exception ex)
            {
                Logger2.Current.WriteMessage("Adduplex initialize fail", Logger2.Level.WarningError);
            }

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
                        if (!eventArgs.TileId.Contains(SettingsKeys.TileId))
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
        
        public override async Task OnSuspendingAsync(object s, SuspendingEventArgs e, bool prelaunch)
        {
            if (OnNewTilePinned != null)
            {
                OnNewTilePinned();
                OnNewTilePinned = null;
            }
            Logger2.Current.WriteMessage("suspending");
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
    }
}
