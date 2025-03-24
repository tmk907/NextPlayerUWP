using System.Linq;
using NextPlayerWinUI.AppColors;
using NextPlayerWinUI.Common;
using NextPlayerWinUIDataLayer.Helpers;
using NextPlayerWinUIDataLayer.Model;
using System.Diagnostics;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Template10.Common;

namespace NextPlayerWinUI
{
    public partial class App
    {
        public async Task OnStartAsync(StartKind startKind, Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Debug.WriteLine("OnStartAsync " + startKind + " " + args.Arguments);

            var activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();

            if (isFirstRun)
            {
                isFirstRun = false;
                await NavigationService.NavigateAsync(AppPages.Pages.Settings);
            }
            else
            {
                if (activatedEventArgs.Kind == Microsoft.Windows.AppLifecycle.ExtendedActivationKind.File)
                {
                    if (DeviceFamilyHelper.IsDesktop())
                    {
                        await NavigationService.NavigateAsync(AppPages.Pages.NowPlayingDesktop);
                    }
                    else
                    {
                        await NavigationService.NavigateAsync(AppPages.Pages.NowPlayingPlaylist);
                    }
                }
                else
                {
                    switch (DetermineStartCause(args, activatedEventArgs))
                    {
                        case AdditionalKinds.SecondaryTile:
                            //if (!eventArgs.TileId.Contains(SettingsKeys.TileId))
                            //{
                            //    Debug.WriteLine("OnStartAsync event arg doesn't contain tileid");
                            //}
                            AppPages.Pages page = AppPages.Pages.Playlists;
                            string parameter = "";// eventArgs.Arguments;
                            //MusicItemTypes type = MusicItem.ParseType(parameter);
                            MusicItemTypes type = MusicItemTypes.plainplaylist;
                            // dodac wybor w ustawieniach, czy przejsc do widoku, czy zaczac odtwarzanie i przejsc do teraz odtwarzane
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
                                    page = AppPages.Pages.Playlist;
                                    break;
                                case MusicItemTypes.genre:
                                    page = AppPages.Pages.Playlist;
                                    break;
                                case MusicItemTypes.plainplaylist:
                                    page = AppPages.Pages.Playlist;
                                    break;
                                case MusicItemTypes.smartplaylist:
                                    page = AppPages.Pages.Playlist;
                                    break;
                                case MusicItemTypes.song:
                                    //page = AppPages.Pages.NowPlaying; ?
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
                            if (true)
                            {
                                Debug.WriteLine("OnStartAsync Primary closed");
                                var link = ApplicationSettingsHelper.ReadSettingsValue<string>(SettingsKeys.StartPage);
                                var deepLink = new DeepLinkService();
                                var parser = deepLink.ParseDeepLink(new Uri(link));
                                string startPage = parser[DeepLinkService.AvailableParameters.Page];
                                var t = AppPages.PagesToKeys.FirstOrDefault(kv => kv.Value.Equals(startPage)).Key;
                                if (t == AppPages.Pages.NowPlaying && DeviceFamilyHelper.IsDesktop())
                                {
                                    t = AppPages.Pages.NowPlayingDesktop;
                                }
                                await NavigationService.NavigateAsync(t);
                            }
                            else
                            {
                                Debug.WriteLine("OnStartAsync Primary not closed");
                            }
                            break;
                        case AdditionalKinds.Toast:
                            Debug.WriteLine("OnStartAsync Toast");
                            var toastargs = activatedEventArgs.Data as ToastNotificationActivatedEventArgs;
                            //StoreServicesEngagementManager manager = StoreServicesEngagementManager.GetDefault();
                            //string originalArgs = manager.ParseArgumentsAndTrackAppLaunch(toastargs.Argument);
                            //if (originalArgs == "extensions")
                            //{
                            //    NavigationService.Navigate(typeof(SettingsDetailsView), nameof(ViewModels.Settings.SettingsExtensionsViewModel));
                            //}
                            //else
                            //{
                            await NavigationService.NavigateAsync(AppPages.Pages.Settings);
                            //}
                            break;
                        default:
                            Debug.WriteLine("OnStartAsync default");
                            await NavigationService.NavigateAsync(AppPages.Pages.Playlists);
                            break;
                    }
                }
            }
            OnStartAsyncFinished?.Invoke(ApplicationExecutionState.NotRunning, activatedEventArgs.Data as FileActivatedEventArgs);
        }

        public async Task OnInitializeAsync(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Debug.WriteLine("OnInitializeAsync");
            AppAccentColors.RestoreAppAccentColors();
            TranslationHelper tr = new TranslationHelper();
            tr.ChangeSlideableItemDescription();

            //if (ApplicationExecutionState.Terminated == args.PreviousExecutionState)
            //{
            //    await SongCoverManager.Instance.Initialize(true);
            //}
            //else
            //{
                await SongCoverManager.Instance.Initialize();
            //}


            await ChangeStatusBarVisibility();
            ThemeHelper.ApplyAppTheme(ThemeHelper.IsLightTheme);

            var keys = PageKeys<AppPages.Pages>();
            foreach (var kv in AppPages.PagesToTypes)
            {
                if (!keys.ContainsKey(kv.Key))
                {
                    keys.Add(kv.Key, kv.Value);
                }
            }

            //try
            //{
            //    if (args.PreviousExecutionState == ApplicationExecutionState.Terminated ||
            //        args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
            //        args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
            //    {
            //        DisplayRequestHelper drh = new DisplayRequestHelper();
            //        drh.ActivateIfEnabled();
            //    }
            //}
            //catch (Exception ex)
            //{
            //}
        }
    }
}
