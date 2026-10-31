using NextPlayerWinUI.Common;
using NextPlayerWinUIDataLayer.Helpers;
using NextPlayerWinUIDataLayer.Model;
using NextPlayerWinUIDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NextPlayerWinUI
{
    public delegate void SongUpdatedHandler(int id);
    public delegate void MenuItemVisibilityChangedHandler();

    public partial class App
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

        public static bool IsLightThemeOn = false;

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

        private static AppPages appPages;
        public static AppPages AppPages
        {
            get
            {
                if (appPages == null)
                {
                    appPages = new AppPages();
                }
                return appPages;
            }
        }

        public static bool mobileNowPlaying = false;
        public static void OnNavigatedToNewView(bool bottomPlayerVisible, bool isNowPlayingDesktopActive = false)
        {
            ViewModels.ViewModelLocator vml = new ViewModels.ViewModelLocator();
            vml.BottomPlayerVM.BottomPlayerVisibility = bottomPlayerVisible;
            vml.BottomPlayerVM.IsNowPlayingDesktopViewActive = isNowPlayingDesktopActive;
            mobileNowPlaying = !isNowPlayingDesktopActive && !bottomPlayerVisible;
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
