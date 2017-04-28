using NextPlayerUWP.Common;
using NextPlayerUWP.Views;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Controls;
using Windows.UI.Xaml;

namespace NextPlayerUWP
{
    public delegate void SongUpdatedHandler(int id);
    public delegate void MenuItemVisibilityChangedHandler();

    sealed partial class App : BootStrapper
    {
        public static event SongUpdatedHandler SongUpdated;
        public static void OnSongUpdated(int id)
        {
            SongUpdated?.Invoke(id);
        }

        public static event MenuItemVisibilityChangedHandler MenuItemVisibilityChange;
        public static void OnMenuItemVisibilityChange()
        {
            MenuItemVisibilityChange?.Invoke();
        }

        public static Action OnNewTilePinned { get; set; }

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

        private static FileFormatsHelper fileFormatsHelper;
        public static FileFormatsHelper FileFormatsHelper
        {
            get
            {
                if (fileFormatsHelper == null)
                {
                    fileFormatsHelper = new FileFormatsHelper(false);
                }
                return fileFormatsHelper;
            }
        }

        private static PlayerInitializer playerInitializer;
        public static PlayerInitializer PlayerInitializer
        {
            get
            {
                if (playerInitializer == null)
                {
                    playerInitializer = new PlayerInitializer();
                }
                return playerInitializer;
            }
        }

        public static void OnNavigatedToNewView(bool bottomPlayerVisible, bool isNowPlayingDesktopActive = false)
        {
            if (Window.Current == null || Window.Current.Content == null) return;
            //((Shell)((ModalDialog)Window.Current.Content).Content).ChangeBottomPlayerVisibility(visible);
            ViewModels.ViewModelLocator vml = new ViewModels.ViewModelLocator();
            vml.BottomPlayerVM.BottomPlayerVisibility = bottomPlayerVisible;
            ((Shell)((ModalDialog)Window.Current.Content).Content).OnDesktopViewActiveChange(isNowPlayingDesktopActive);
        }

        public static async Task ChangeStatusBarVisibility()
        {
            bool hide = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.HideStatusBar);
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

        private static IEnumerable<MusicItem> cacheList = new List<MusicItem>();
        public static void AddToCache(IEnumerable<MusicItem> items)
        {
            cacheList = items;
        }
        public static IEnumerable<MusicItem> GetFromCache()
        {
            return cacheList;
        }
        public static void ClearCache()
        {
            cacheList = new List<MusicItem>();
        }
    }
}
