using Microsoft.Windows.AppLifecycle;
using NextPlayerWinUI.Common;
using NextPlayerWinUI.Views;
using NextPlayerWinUIDataLayer.Helpers;
using NextPlayerWinUIDataLayer.Model;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;

namespace NextPlayerWinUI
{
    public partial class App
    {
        private const string DefaultTileId = "App";

        private async Task NavigateForActivationAsync(AppActivationArguments activation, IFileActivatedEventArgs fileArgs, bool isAppStarting)
        {
            if (isFirstRun)
            {
                isFirstRun = false;
                await NavigationService.NavigateAsync(AppPages.Pages.Settings);
                return;
            }

            if (fileArgs != null && fileArgs.Files.Any())
            {
                await NavigationService.NavigateAsync(AppPages.Pages.NowPlayingDesktop);
                return;
            }

            switch (activation?.Kind ?? ExtendedActivationKind.Launch)
            {
                case ExtendedActivationKind.Launch:
                    await NavigateForLaunchAsync(activation?.Data as ILaunchActivatedEventArgs, isAppStarting);
                    break;
                case ExtendedActivationKind.ToastNotification:
                    await NavigationService.NavigateAsync(AppPages.Pages.Settings);
                    break;
                default:
                    if (isAppStarting)
                    {
                        await NavigationService.NavigateAsync(AppPages.Pages.Playlists);
                    }
                    break;
            }
        }

        private async Task NavigateForLaunchAsync(ILaunchActivatedEventArgs args, bool isAppStarting)
        {
            string tileId = args?.TileId;
            if (!string.IsNullOrEmpty(tileId) && tileId != DefaultTileId)
            {
                await NavigateToSecondaryTileTargetAsync(args.Arguments);
            }
            else if (isAppStarting)
            {
                await NavigateToStartPageAsync();
            }
        }

        private async Task NavigateToStartPageAsync()
        {
            var link = ApplicationSettingsHelper.ReadSettingsValue<string>(SettingsKeys.StartPage);
            var parser = new DeepLinkService().ParseDeepLink(new Uri(link));
            string startPage = parser[DeepLinkService.AvailableParameters.Page];
            var page = AppPages.PagesToKeys.FirstOrDefault(kv => kv.Value.Equals(startPage)).Key;
            if (page == AppPages.Pages.NowPlaying)
            {
                page = AppPages.Pages.NowPlayingDesktop;
            }
            await NavigationService.NavigateAsync(page);
        }

        private async Task NavigateToSecondaryTileTargetAsync(string parameter)
        {
            var page = AppPages.Pages.Playlists;
            MusicItemTypes type = MusicItem.ParseType(parameter);
            switch (type)
            {
                case MusicItemTypes.album:
                    page = AppPages.Pages.Album;
                    parameter = MusicItem.SplitParameter(parameter)[1];
                    break;
                case MusicItemTypes.albumartist:
                    page = AppPages.Pages.AlbumArtist;
                    parameter = MusicItem.SplitParameter(parameter)[1];
                    break;
                case MusicItemTypes.artist:
                    page = AppPages.Pages.Artist;
                    parameter = MusicItem.SplitParameter(parameter)[1];
                    break;
                case MusicItemTypes.folder:
                case MusicItemTypes.genre:
                case MusicItemTypes.plainplaylist:
                case MusicItemTypes.smartplaylist:
                    page = AppPages.Pages.Playlist;
                    break;
                case MusicItemTypes.unknown:
                    TelemetryAdapter.TrackEventException("MusicItemTypes.unknown");
                    break;
            }
            TelemetryAdapter.TrackEvent("LaunchFromSecondaryTile " + type);
            await NavigationService.NavigateAsync(page, parameter);
        }
    }
}
