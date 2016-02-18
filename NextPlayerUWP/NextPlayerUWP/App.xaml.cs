using GalaSoft.MvvmLight.Threading;
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

            if (FirstRun())
            {
                DatabaseManager.Current.CreateDatabase();
            }
            //DatabaseManager.Current.ClearCoverPaths();
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
        
        public override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            //TileManager.ManageSecondaryTileImages();
            
            try
            {
                var keys = PageKeys<Pages>();
                keys.Add(Pages.AddToPlaylist, typeof(AddToPlaylistView));
                keys.Add(Pages.Albums, typeof(AlbumsView));
                keys.Add(Pages.Album, typeof(AlbumView));
                keys.Add(Pages.Artists, typeof(ArtistsView));
                keys.Add(Pages.Artist, typeof(ArtistView));
                keys.Add(Pages.Genres, typeof(GenresView));
                keys.Add(Pages.Folders, typeof(FoldersView));
                keys.Add(Pages.Playlists, typeof(PlaylistsView));
                keys.Add(Pages.Playlist, typeof(PlaylistView));
                keys.Add(Pages.Songs, typeof(SongsView));
                keys.Add(Pages.TagsEditor, typeof(TagsEditor));
                //keys.Add(Pages, typeof());
                //check if import was finished
                
                var nav = NavigationServiceFactory(BackButton.Attach, ExistingContent.Include);
                //s.Stop();
                //Debug.WriteLine("initialize 2 " + s.ElapsedMilliseconds);
                //s.Start();
                Window.Current.Content = new Views.Shell(nav);
                //s.Stop();
                //Debug.WriteLine("initialize 3 " + s.ElapsedMilliseconds);
                //s.Start();
            }
            catch (Exception ex)
            {
                Logger.Save("OnInitializeAsync" + Environment.NewLine+ex);
                Logger.SaveToFile();
            }
            DispatcherHelper.Initialize();
            s.Stop();
            Debug.WriteLine("initialize end" + s.ElapsedMilliseconds);
            return Task.CompletedTask;
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

        private bool FirstRun()
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

        
    }
}
