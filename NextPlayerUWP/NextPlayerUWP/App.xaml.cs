using GalaSoft.MvvmLight.Threading;
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
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace NextPlayerUWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>

    public delegate void SongUpdatedHandler(int id);

    sealed partial class App : Template10.Common.BootStrapper
    {
        public static event SongUpdatedHandler SongUpdated;
        public static void OnSongUpdated(int id)
        {
            if (SongUpdated != null)
            {
                SongUpdated(id);
            }
        }

#if DEBUG
        private bool dev = true;
#else
        private bool dev = false;
#endif

        public App()
        {
            InitializeComponent();
            App.Current.UnhandledException += App_UnhandledException;
            Logger.SaveFromSettingsToFile();
            //insights
            //Resetdb();            
            //DatabaseManager.Current.ClearCoverPaths();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterDropItem, AppConstants.ActionAddToNowPlaying);
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.SaveInSettings(e.Message+Environment.NewLine+e.Exception);
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
            Playlists,
            Playlist,
            Settings,
            Songs,
            TagsEditor
        }
        
        public override async Task OnInitializeAsync(IActivatedEventArgs args)
        {
            if (IsFirstRun())
            {
                await FirstRunSetup();
            }
            await TileManager.ManageSecondaryTileImages();
            try
            {
                var keys = PageKeys<Pages>();
                keys.Add(Pages.AddToPlaylist, typeof(AddToPlaylistView));
                keys.Add(Pages.Albums, typeof(AlbumsView));
                keys.Add(Pages.Album, typeof(AlbumView));
                keys.Add(Pages.Artists, typeof(ArtistsView));
                keys.Add(Pages.Artist, typeof(ArtistView));
                keys.Add(Pages.FileInfo, typeof(FileInfoView));
                keys.Add(Pages.Folders, typeof(FoldersView));
                keys.Add(Pages.Genres, typeof(GenresView));
                keys.Add(Pages.Playlists, typeof(PlaylistsView));
                keys.Add(Pages.Playlist, typeof(PlaylistView));
                keys.Add(Pages.Settings, typeof(SettingsView));
                keys.Add(Pages.Songs, typeof(SongsView));
                keys.Add(Pages.TagsEditor, typeof(TagsEditor));
                //keys.Add(Pages, typeof());
                //check if import was finished
                var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
                Window.Current.Content = new Views.Shell(nav);
            }
            catch (Exception ex)
            {
                Logger.Save("OnInitializeAsync" + Environment.NewLine+ex);
                Logger.SaveToFile();
            }
            DispatcherHelper.Initialize();
            //return Task.CompletedTask;
        }

        public override Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            AdditionalKinds cause = DetermineStartCause(args);
            
            if (cause == AdditionalKinds.SecondaryTile)
            {
                LaunchActivatedEventArgs eventArgs = args as LaunchActivatedEventArgs;
                if (!eventArgs.TileId.Contains(AppConstants.TileId))
                {
                    Logger.Save("event arg doesn't contain tileid " + Environment.NewLine + eventArgs.TileId + Environment.NewLine + eventArgs.Arguments);
                    Logger.SaveToFile();
                }
                Pages page = Pages.Playlists;
                string parameter = eventArgs.Arguments;
                MusicItemTypes type = MusicItem.ParseType(parameter);
                // dodac wybor w ustawieniach, czy przejsc do widoku, czy zaczac odtwarzanie i przejsc do teraz odtwarzane
                switch (type)
                {
                    case MusicItemTypes.album:
                        page = Pages.Album;
                        break;
                    case MusicItemTypes.artist:
                        page = Pages.Artist;
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
                }
                NavigationService.Navigate(page, parameter);
            }
            else
            {
                NavigationService.Navigate(Pages.Playlists);
            }
            return Task.FromResult<object>(null);
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

        private bool IsFirstRun()
        {
            object o = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.FirstRun);
            if (o == null)
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

        private async Task FirstRunSetup()
        {
            DatabaseManager.Current.CreateDatabase();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 1);
            await CreateDefaultSmartPlaylists();

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, false);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerTime, 0);

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterDropItem, AppConstants.ActionAddToNowPlaying);

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmRateSongs, true);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmLove, 5);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmUnLove, 1);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmSendNP, false);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmLogin, "");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmPassword, "");


        }

        private async Task CreateDefaultSmartPlaylists()
        {
            int i;
            i = DatabaseManager.Current.InsertSmartPlaylist("Ostatnio dodane", 50, SPUtility.SortBy.MostRecentlyAdded);
            await DatabaseManager.Current.InsertSmartPlaylistEntry(i, SPUtility.Item.DateAdded, SPUtility.Comparison.IsGreater, DateTime.Now.Subtract(TimeSpan.FromDays(14)).Ticks.ToString(), SPUtility.Operator.Or);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.OstatnioDodane, i);
            i = DatabaseManager.Current.InsertSmartPlaylist("Ostatnio odtwarzane", 50, SPUtility.SortBy.MostRecentlyPlayed);
            await DatabaseManager.Current.InsertSmartPlaylistEntry(i, SPUtility.Item.LastPlayed, SPUtility.Comparison.IsGreater, DateTime.MinValue.Ticks.ToString(), SPUtility.Operator.Or);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.OstatnioOdtwarzane, i);
            i = DatabaseManager.Current.InsertSmartPlaylist("Najczęściej odtwarzane", 50, SPUtility.SortBy.MostOftenPlayed);
            await DatabaseManager.Current.InsertSmartPlaylistEntry(i, SPUtility.Item.PlayCount, SPUtility.Comparison.IsGreater, "0", SPUtility.Operator.Or);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajczesciejOdtwarzane, i);
            i = DatabaseManager.Current.InsertSmartPlaylist("Najlepiej oceniane", 50, SPUtility.SortBy.HighestRating);
            await DatabaseManager.Current.InsertSmartPlaylistEntry(i, SPUtility.Item.Rating, SPUtility.Comparison.IsGreater, "3", SPUtility.Operator.Or);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajlepiejOceniane, i);
            i = DatabaseManager.Current.InsertSmartPlaylist("Najrzadziej odtwarzane", 50, SPUtility.SortBy.LeastOftenPlayed);
            await DatabaseManager.Current.InsertSmartPlaylistEntry(i, SPUtility.Item.PlayCount, SPUtility.Comparison.IsGreater, "-1", SPUtility.Operator.Or);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajrzadziejOdtwarzane, i);
            i = DatabaseManager.Current.InsertSmartPlaylist("Najgorzej oceniane", 50, SPUtility.SortBy.LowestRating);
            await DatabaseManager.Current.InsertSmartPlaylistEntry(i, SPUtility.Item.Rating, SPUtility.Comparison.IsLess, "4", SPUtility.Operator.Or);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.NajgorzejOceniane, i);
        }

        public void ChangeTelemetry(bool enable)
        {
            //TODO
        }

        private void Resetdb()
        {
            DatabaseManager.Current.resetdb();
        }
    }
}
