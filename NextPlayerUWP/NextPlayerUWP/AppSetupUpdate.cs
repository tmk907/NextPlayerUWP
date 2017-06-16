using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Template10.Common;

namespace NextPlayerUWP
{
    sealed partial class App : BootStrapper
    {
        private const int dbVersion = 13;

        private void FirstRunSetup()
        {
            DatabaseManager.Current.CreateNewDatabase();
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, dbVersion);
            CreateDefaultSmartPlaylists();

            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.TimerOn, false);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.TimerTime, 0);

            //ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AppTheme, true);
            var color = Windows.UI.Color.FromArgb(255, 0, 120, 215);
            ColorsHelper ch = new ColorsHelper();
            ch.SaveAppAccentColor(color);
            //ch.SetAccentColorShades(color);

            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.ActionAfterDropItem, SettingsKeys.ActionAddToNowPlaying);

            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LfmRateSongs, true);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LfmLove, 4);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LfmSendNP, false);

            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DisableLockscreen, false);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.HideStatusBar, false);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.IncludeSubFolders, false);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.PlaylistsFolder, "");
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AutoSavePlaylists, true);

            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.FlipViewSelectedIndex, 0);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.ActionAfterSwipeRightCommand, SettingsKeys.SwipeActionDelete);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.ActionAfterSwipeLeftCommand, SettingsKeys.SwipeActionAddToNowPlaying);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.EnableLiveTileWithImage, true);

            ApplicationSettingsHelper.SaveSettingsValue("ImportPlaylistsAfterAppUpdate9", true);

            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LibraryUpdatedAt, DateTimeOffset.Now);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LibraryUpdateFrequency, TimeSpan.FromDays(1));

            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.IgnoreArticles, false);
            ApplicationSettingsHelper.SaveData(SettingsKeys.IgnoredArticlesList, new List<string>() { "a ", "an ", "the " });
            
            var menuEntries = new List<MenuButtonItem>()
            {
                new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Albums },
                new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.AlbumArtists },
                new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Artists },
                new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Folders },
                new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Genres },
                new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Playlists },
                new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Songs },
                new MenuButtonItem() { ShowButton = !IsDesktop, PageType = MenuItemType.NowPlayingPlaylist },
                new MenuButtonItem() { ShowButton = IsDesktop, PageType = MenuItemType.NowPlayingSong },
                new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Online },
            };
            ApplicationSettingsHelper.SaveData(SettingsKeys.MenuEntries, menuEntries);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AccentFromAlbumArt, false);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AlbumArtInBackground, true);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.MediaScanUseIndexer, true);
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.SongDurationType, SettingsKeys.SongDurationTotal);

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

        private void PerformUpdate()
        {
            UpdateDB();
            UpdateApp();
        }

        private void UpdateDB()
        {
            //ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, 8);
            object version = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.DBVersion);
            if (version.ToString() == "1")
            {
                DatabaseManager.Current.UpdateToVersion2();
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, 2);
                version = "2";
            }
            if (version.ToString() == "2")
            {
                bool recreate = DatabaseManager.Current.DBCorrection();
                if (recreate)
                {
                    CreateDefaultSmartPlaylists();
                }
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, 3);
                version = "3";
            }
            if (version.ToString() == "3")
            {
                DatabaseManager.Current.UpdateToVersion4();
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, 4);
                version = "4";
            }
            if (version.ToString() == "4")
            {
                DatabaseManager.Current.UpdateToVersion5();
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, 5);
                version = "5";
            }
            if (version.ToString() == "5")
            {
                DatabaseManager.Current.UpdateToVersion6();
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, 6);
                version = "6";
            }
            if (version.ToString() == "6")
            {
                DatabaseManager.Current.UpdateToVersion7();
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, 7);
                version = "7";
            }
            if (version.ToString() == "7")
            {
                DatabaseManager.Current.UpdateToVersion8();
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, 8);
                version = "8";
            }
            if (version.ToString() == "8")
            {
                DatabaseManager.Current.UpdateToVersion9();
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, 9);
                version = "9";
            }
            if (version.ToString() == "9")
            {
                DatabaseManager.Current.UpdateToVersion10();
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, 10);
                version = "10";
            }
            if (version.ToString() == "10")
            {
                DatabaseManager.Current.UpdateToVersion11();
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, 11);
                version = "11";
            }
            if (version.ToString() == "11")
            {
                DatabaseManager.Current.UpdateToVersion12();
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, 12);
                version = "12";
            }
            if (version.ToString() == "12")
            {
                DatabaseManager.Current.UpdateToVersion13();
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DBVersion, 13);
                version = "13";
            }
            // change  private const int dbVersion
        }

        private void UpdateApp()
        {
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.DisableLockscreen) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DisableLockscreen, false);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.HideStatusBar) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.HideStatusBar, false);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.IncludeSubFolders) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.IncludeSubFolders, false);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.PlaylistsFolder) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.PlaylistsFolder, "");
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.AutoSavePlaylists) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AutoSavePlaylists, true);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.ActionAfterSwipeRightCommand) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.ActionAfterSwipeRightCommand, SettingsKeys.SwipeActionDelete);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.ActionAfterSwipeLeftCommand) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.ActionAfterSwipeLeftCommand, SettingsKeys.SwipeActionAddToNowPlaying);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.FlipViewSelectedIndex) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.FlipViewSelectedIndex, 0);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.EnableLiveTileWithImage) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.EnableLiveTileWithImage, true);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LibraryUpdatedAt) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LibraryUpdatedAt, DateTimeOffset.Now);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LibraryUpdateFrequency) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LibraryUpdateFrequency, TimeSpan.FromDays(1));
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.IgnoreArticles) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.IgnoreArticles, false);
                ApplicationSettingsHelper.SaveData(SettingsKeys.IgnoredArticlesList, new List<string>() { "a ", "an ", "the " });
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.MenuEntries) == null)
            {
                var menuEntries = new List<MenuButtonItem>()
                {
                    new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Albums },
                    new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.AlbumArtists },
                    new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Artists },
                    new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Folders },
                    new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Genres },
                    new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Playlists },
                    new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Songs },
                    new MenuButtonItem() { ShowButton = !IsDesktop, PageType = MenuItemType.NowPlayingPlaylist },
                    new MenuButtonItem() { ShowButton = IsDesktop, PageType = MenuItemType.NowPlayingSong },
                    new MenuButtonItem() { ShowButton = true, PageType = MenuItemType.Online },
                };
                ApplicationSettingsHelper.SaveData(SettingsKeys.MenuEntries, menuEntries);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.AccentFromAlbumArt) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AlbumArtInBackground, false);
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AccentFromAlbumArt, false);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.MediaScanUseIndexer) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.MediaScanUseIndexer, true);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.SongDurationType) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.SongDurationType, SettingsKeys.SongDurationTotal);
            }
        }

        private bool IsDesktop
        {
            get
            {
                return (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop");
            }
        }
    }
}
