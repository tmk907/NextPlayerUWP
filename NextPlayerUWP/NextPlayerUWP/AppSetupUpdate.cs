using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Common;

namespace NextPlayerUWP
{
    sealed partial class App : BootStrapper
    {
        private void FirstRunSetup()
        {
            DatabaseManager.Current.CreateNewDatabase();
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, dbVersion);
            CreateDefaultSmartPlaylists();

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, false);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerTime, 0);

            //ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppTheme, true);
            var color = Windows.UI.Color.FromArgb(255, 0, 120, 215);
            ColorsHelper ch = new ColorsHelper();
            ch.SaveUserAccentColor(color);
            //ch.SetAccentColorShades(color);

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterDropItem, AppConstants.ActionAddToNowPlaying);

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmRateSongs, true);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmLove, 4);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmSendNP, false);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmLogin, "");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmPassword, "");

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DisableLockscreen, false);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.HideStatusBar, false);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IncludeSubFolders, false);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.PlaylistsFolder, "");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AutoSavePlaylists, true);

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.FlipViewSelectedIndex, 0);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterSwipeRightCommand, AppConstants.SwipeActionDelete);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterSwipeLeftCommand, AppConstants.SwipeActionAddToNowPlaying);
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.EnableLiveTileWithImage, true);

            ApplicationSettingsHelper.SaveSettingsValue("ImportPlaylistsAfterAppUpdate9", true);

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LibraryUpdatedAt, DateTime.Now - TimeSpan.FromDays(10));
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LibraryUpdateFrequency, TimeSpan.FromDays(3));

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
            //ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 8);
            object version = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.DBVersion);
            if (version.ToString() == "1")
            {
                DatabaseManager.Current.UpdateToVersion2();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 2);
                version = "2";
            }
            if (version.ToString() == "2")
            {
                bool recreate = DatabaseManager.Current.DBCorrection();
                if (recreate)
                {
                    CreateDefaultSmartPlaylists();
                }
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 3);
                version = "3";
            }
            if (version.ToString() == "3")
            {
                DatabaseManager.Current.UpdateToVersion4();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 4);
                version = "4";
            }
            if (version.ToString() == "4")
            {
                DatabaseManager.Current.UpdateToVersion5();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 5);
                version = "5";
            }
            if (version.ToString() == "5")
            {
                DatabaseManager.Current.UpdateToVersion6();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 6);
                version = "6";
            }
            if (version.ToString() == "6")
            {
                DatabaseManager.Current.UpdateToVersion7();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 7);
                version = "7";
            }
            if (version.ToString() == "7")
            {
                DatabaseManager.Current.UpdateToVersion8();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 8);
                version = "8";
            }
            if (version.ToString() == "8")
            {
                DatabaseManager.Current.UpdateToVersion9();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 9);
                version = "9";
            }
            if (version.ToString() == "9")
            {
                DatabaseManager.Current.UpdateToVersion10();
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DBVersion, 10);
                version = "10";
            }
            // change  private const int dbVersion
        }

        private void UpdateApp()
        {
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.DisableLockscreen) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DisableLockscreen, false);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.HideStatusBar) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.HideStatusBar, false);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IncludeSubFolders) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IncludeSubFolders, false);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.PlaylistsFolder) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.PlaylistsFolder, "");
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AutoSavePlaylists) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AutoSavePlaylists, true);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.ActionAfterSwipeRightCommand) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterSwipeRightCommand, AppConstants.SwipeActionDelete);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.ActionAfterSwipeLeftCommand) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterSwipeLeftCommand, AppConstants.SwipeActionAddToNowPlaying);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.FlipViewSelectedIndex) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.FlipViewSelectedIndex, 0);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.EnableLiveTileWithImage) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.EnableLiveTileWithImage, true);
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LibraryUpdatedAt) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LibraryUpdatedAt, DateTime.Now - TimeSpan.FromDays(10));
            }
            if (ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LibraryUpdateFrequency) == null)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LibraryUpdateFrequency, TimeSpan.FromDays(3));
            }
        }
    }
}
