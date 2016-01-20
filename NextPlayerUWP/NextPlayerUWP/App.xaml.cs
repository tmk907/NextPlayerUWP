using NextPlayerUWP.Views;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Template10.Common.BootStrapper
    {
        public App()
        {
            InitializeComponent();
        }

        public enum Pages
        {
            MainPage,
            Albums,
            Album,
            Artists,
            Genres,
            Folders,
            Playlists,
            Playlist,
            Songs,
            Settings
        }

        public override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            //insights
            var keys = PageKeys<Pages>();
            keys.Add(Pages.MainPage, typeof(MainPage));
            keys.Add(Pages.Albums, typeof(AlbumsView));
            keys.Add(Pages.Album, typeof(AlbumView));
            keys.Add(Pages.Artists, typeof(ArtistsView));
            keys.Add(Pages.Genres, typeof(GenresView));
            keys.Add(Pages.Folders, typeof(FoldersView));
            keys.Add(Pages.Playlists, typeof(PlaylistsView));
            keys.Add(Pages.Playlist, typeof(PlaylistView));
            keys.Add(Pages.Songs, typeof(SongsView));
            //keys.Add(Pages, typeof());

            if (FirstRun())
            {
                DatabaseManager.Current.CreateDatabase();
            }

            return base.OnInitializeAsync(args);
        }

        public override Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            AdditionalKinds cause = DetermineStartCause(args);
            //if (cause == AdditionalKinds.SecondaryTile)
            //{
            //    LaunchActivatedEventArgs eventArgs = args as LaunchActivatedEventArgs;
            //    NavigationService.Navigate(typeof(DetailPage), eventArgs.Arguments);
            //}
            //else
            //{
            //    NavigationService.Navigate(typeof(MainPage));
            //}
            NavigationService.Navigate(Pages.MainPage);
            return Task.FromResult<object>(null);
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
    }
}
