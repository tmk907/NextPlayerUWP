﻿using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NextPlayerUWPDataLayer.CloudStorage;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWP.Messages;

namespace NextPlayerUWP.ViewModels
{
    public class LanguageItem
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class SettingsViewModel : Template10.Mvvm.ViewModelBase
    {
        public SettingsViewModel()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            AppVersion = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
#if DEBUG
            AppVersion = AppVersion + " Debug";
#endif
            Languages = new ObservableCollection<LanguageItem>();
            foreach (var code in Windows.Globalization.ApplicationLanguages.ManifestLanguages)
            {
                string name = "unknown";
                languageDescriptions.TryGetValue(code.Substring(0, 2), out name);
                Languages.Add(new LanguageItem() { Code = code, Name = name });
            }
            string primaryCode = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
            foreach (var l in languages)
            {
                if (l.Code == primaryCode)
                {
                    SelectedLanguage = l;
                }
            }
            displayRequestHelper = new DisplayRequestHelper();
        }

        DisplayRequestHelper displayRequestHelper;
        LastFmManager lastFmManager = null;
        LastFmManager LastFmManager
        {
            get
            {
                if (lastFmManager == null) lastFmManager = new LastFmManager();
                return lastFmManager;
            }
        }

        private bool initialization = false;

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            initialization = true;
            App.OnNavigatedToNewView(true);
            // Tools
            var tt = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.TimerTime);
            var IsTimerOn = (bool)(ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.TimerOn)??false);
            Time = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            if (isTimerOn)
            {
                if (tt != null)
                {
                    Time = TimeSpan.FromTicks((long)tt);
                }
                else
                {
                    IsTimerOn = false;
                }
            }
            
            string action = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.ActionAfterDropItem) as string;
            if (action.Equals(SettingsKeys.ActionPlayNow))
            {
                ActionNr = 1;
            }
            else if (action.Equals(SettingsKeys.ActionPlayNext))
            {
                ActionNr = 2;
            }
            else
            {
                ActionNr = 3;
            }

            string swipeAction = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.ActionAfterSwipeLeftCommand) as string;
            switch (swipeAction)
            {
                case SettingsKeys.SwipeActionPlayNow:
                    SwipeActionNr = 1;
                    break;
                case SettingsKeys.SwipeActionPlayNext:
                    SwipeActionNr = 2;
                    break;
                case SettingsKeys.SwipeActionAddToNowPlaying:
                    SwipeActionNr = 3;
                    break;
                case SettingsKeys.SwipeActionAddToPlaylist:
                    SwipeActionNr = 4;
                    break;
                default:
                    break;
            }

            PreventScreenLock = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.DisableLockscreen);
            HideStatusBar = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.HideStatusBar);
            LiveTileWithAlbumArt = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.EnableLiveTileWithImage);
            IncludeSubFolders = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.IncludeSubFolders);

            //Personalization
            IsLightThemeOn = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.AppTheme);

            if (accentColors.Count == 0)
            {
                ColorsHelper ch = new ColorsHelper();
                var sc = ch.GetSavedUserAccentColor();
                foreach (var c in ch.GetWin10Colors())
                {
                    AccentColors.Add(new SolidColorBrush(c));
                }
            }

            if (menuButtons.Count == 0)
            {
                GetMenuButtons();
            }

            //Library

            if (!isUpdating)
            {
                UpdateProgressText = "";
                ScannedFolder = "";
                UpdateProgressTextVisibility = false;
            }
            if (musicLibraryFolders.Count == 0)
            {
                var list = await GetMusicFolders();
                MusicLibraryFolders = new ObservableCollection<SdCardFolder>(list);
            }
            if (sdCardFolders.Count == 0)
            {
                var list = await GetSdCardFolders();
                SdCardFolders = new ObservableCollection<SdCardFolder>(list);
            }
            LastLibraryUpdate = ApplicationSettingsHelper.ReadSettingsValue<DateTimeOffset>(SettingsKeys.LibraryUpdatedAt).Date;
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.MediaScan) != null)
            {
                IsUpdating = true;
            }

            IgnoreArticles = ApplicationSettingsHelper.ReadSettingsValue<bool>(SettingsKeys.IgnoreArticles);
            var ignoredList = ApplicationSettingsHelper.ReadData<List<string>>(SettingsKeys.IgnoredArticlesList);
            IgnoredArticles = "";
            foreach(var item in ignoredList)
            {
                IgnoredArticles += item;
                IgnoredArticles += " ,";
            }

            //Last.fm
            string login = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LfmLogin) as string;
            LastFmLogin = login;
            string session = ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LfmSessionKey) as string;
            if (!String.IsNullOrEmpty(session))
            {
                IsLastFmLoggedIn = true;
            }
            LastFmRateSongs = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LfmRateSongs);
            LastFmShowError = false;

            OneDriveAccounts = new ObservableCollection<CloudAccount>(CloudAccounts.Instance.GetAccountsByType(CloudStorageType.OneDrive));
            DropboxAccounts = new ObservableCollection<CloudAccount>(CloudAccounts.Instance.GetAccountsByType(CloudStorageType.Dropbox));
            PCloudAccounts = new ObservableCollection<CloudAccount>(CloudAccounts.Instance.GetAccountsByType(CloudStorageType.pCloud));

            //About
            if (Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
            {
                FeedbackVisibility = true;
            }
            initialization = false;
            if (mode == NavigationMode.New || mode == NavigationMode.Forward)
            {
                TelemetryAdapter.TrackPageView(this.GetType().ToString());
            }
        }

        #region Library

        private string updateProgressText = "";
        public string UpdateProgressText
        {
            get { return updateProgressText; }
            set { Set(ref updateProgressText, value); }
        }

        private string scannedFolder = "";
        public string ScannedFolder
        {
            get { return scannedFolder; }
            set { Set(ref scannedFolder, value); }
        }

        private bool updateProgressTextVisibility = false;
        public bool UpdateProgressTextVisibility
        {
            get { return updateProgressTextVisibility; }
            set { Set(ref updateProgressTextVisibility, value); }
        }

        private bool isUpdating = false;
        public bool IsUpdating
        {
            get { return isUpdating; }
            set { Set(ref isUpdating, value); }
        }

        private DateTime lastLibraryUpdate;
        public DateTime LastLibraryUpdate
        {
            get { return lastLibraryUpdate; }
            set { Set(ref lastLibraryUpdate, value); }
        }

        private ObservableCollection<SdCardFolder> musicLibraryFolders = new ObservableCollection<SdCardFolder>();
        public ObservableCollection<SdCardFolder> MusicLibraryFolders
        {
            get { return musicLibraryFolders; }
            set { Set(ref musicLibraryFolders, value); }
        }

        private ObservableCollection<SdCardFolder> sdCardFolders = new ObservableCollection<SdCardFolder>();
        public ObservableCollection<SdCardFolder> SdCardFolders
        {
            get { return sdCardFolders; }
            set { Set(ref sdCardFolders, value); }
        }

        public async void UpdateLibrary()
        {
            MediaImport m = new MediaImport(App.FileFormatsHelper);
            UpdateProgressTextVisibility = true;
            Progress<string> progress = new Progress<string>(
                data =>
                {
                    var array = data.Split('|');
                    ScannedFolder = array[0];
                    UpdateProgressText = array[1].ToString();
                }
            );
            IsUpdating = true;
            await Task.Run(() => m.UpdateDatabaseAsync(progress));
            IsUpdating = false;
            LastLibraryUpdate = DateTime.Now;
            ScannedFolder = "";
            TelemetryAdapter.TrackEvent("Library updated");
        }

        public async void AddFolder()
        {
            var musicLibrary = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
            Windows.Storage.StorageFolder newFolder = await musicLibrary.RequestAddFolderAsync();
            if (newFolder != null)
            {
                MusicLibraryFolders.Add(new SdCardFolder() { Name = newFolder.DisplayName, Path = newFolder.Path, IncludeSubFolders=true });
            }
        }

        public async void RemoveFolder(SdCardFolder musicFolder)
        {
            try
            {
                var musicLibrary = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
                var folder = musicLibrary.Folders.Where(f => f.Path.Equals(musicFolder.Path)).FirstOrDefault();
                bool confirmDeletion = await musicLibrary.RequestRemoveFolderAsync(folder);
                if (confirmDeletion)
                {
                    MusicLibraryFolders.Remove(musicFolder);
                    await AfterFolderDelete(musicFolder.Path);
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async void AddSdCardFolder()//error element not found
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                MessageDialogHelper helper = new MessageDialogHelper();
                bool includeSubFolders = await helper.ShowIncludeAllSubFoldersQuestion();
                SdCardFolders.Add(new SdCardFolder()
                {
                    Name = folder.Name,
                    Path = folder.Path,
                    IncludeSubFolders = includeSubFolders,
                });
                var ff = sdCardFolders.Where(f => !f.Path.ToLower().StartsWith("c:"));
                var list = new List<SdCardFolder>(ff);
                await ApplicationSettingsHelper.SaveSdCardFoldersToScan(list);
            }
        }

        public async void RemoveSdCardFolder(SdCardFolder musicFolder)
        {
            if (musicFolder.Path.ToLower().StartsWith("c:")) return;

            SdCardFolders.Remove(musicFolder);
            await AfterFolderDelete(musicFolder.Path);
        }

        private async Task AfterFolderDelete(string folderPath)
        {
            await DatabaseManager.Current.DeleteFolderAndSubFoldersAsync(folderPath);
            MediaImport.OnMediaImported("FolderRemoved");
        }

        private async Task<List<SdCardFolder>> GetSdCardFolders()
        {
            var list = await ApplicationSettingsHelper.GetSdCardFoldersToScan();
            var folder = new SdCardFolder()
            {
                Name = "Music",
                Path = @"C:\",
                IncludeSubFolders = true,
            };
            list.Insert(0, folder);
            return list;
        }

        private async Task<List<SdCardFolder>> GetMusicFolders()
        {
            List<SdCardFolder> list = new List<SdCardFolder>();
            var lib = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
            foreach (var f in lib.Folders)
            {
                list.Add(new SdCardFolder { Name = f.DisplayName, Path = f.Path, IncludeSubFolders = true });
            }
            return list;
        }

        private bool ignoreArticles = false;
        public bool IgnoreArticles
        {
            get { return ignoreArticles; }
            set
            {
                Set(ref ignoreArticles, value);
                if (!initialization) ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.IgnoreArticles, value);
            }
        }

        private string ignoredArticles = "";
        public string IgnoredArticles
        {
            get { return ignoredArticles; }
            set
            {
                Set(ref ignoredArticles, value);
                SaveIgnoredArticles(value);
            }
        }

        private void SaveIgnoredArticles(string value)
        {
            if (!initialization)
            {
                List<string> ignored = new List<string>();

                if (!String.IsNullOrEmpty(value))
                {
                    var array = value.Split(new char[] { ';', ',', '/', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in array)
                    {
                        string toIgnore = item.Trim();
                        if (toIgnore != "") ignored.Add(toIgnore + " ");
                    }
                }
                ApplicationSettingsHelper.SaveData(SettingsKeys.IgnoredArticlesList, ignored);
            }
        }

        #endregion

        #region Tools

        private int actionNr = default(int);
        public int ActionNr
        {
            get { return actionNr; }
            set
            {
                Set(ref actionNr, value);
                RaisePropertyChanged("ActionNr1");
                RaisePropertyChanged("ActionNr2");
                RaisePropertyChanged("ActionNr3");
            }
        }

        public bool ActionNr1
        {
            get { return actionNr.Equals(1); }
            set
            {
                ActionNr = 1;
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.ActionAfterDropItem, SettingsKeys.ActionPlayNow);
                TelemetryAdapter.TrackEvent("After Drop Item " + SettingsKeys.ActionPlayNow);
            }
        }

        public bool ActionNr2
        {
            get { return actionNr.Equals(2); }
            set
            {
                ActionNr = 2;
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.ActionAfterDropItem, SettingsKeys.ActionPlayNext);
                TelemetryAdapter.TrackEvent("After Drop Item " + SettingsKeys.ActionPlayNext);
            }
        }

        public bool ActionNr3
        {
            get { return actionNr.Equals(3); }
            set
            {
                ActionNr = 3;
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.ActionAfterDropItem, SettingsKeys.ActionAddToNowPlaying);
                TelemetryAdapter.TrackEvent("After Drop Item " + SettingsKeys.ActionAddToNowPlaying);
            }
        }


        private int swipeActionNr = default(int);
        public int SwipeActionNr
        {
            get { return swipeActionNr; }
            set
            {
                Set(ref swipeActionNr, value);
                RaisePropertyChanged("SwipeActionNr1");
                RaisePropertyChanged("SwipeActionNr2");
                RaisePropertyChanged("SwipeActionNr3");
                RaisePropertyChanged("SwipeActionNr4");
            }
        }

        public bool SwipeActionNr1
        {
            get { return swipeActionNr.Equals(1); }
            set
            {
                SwipeActionNr = 1;
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.ActionAfterSwipeLeftCommand, SettingsKeys.SwipeActionPlayNow);
                TranslationHelper tr = new TranslationHelper();
                tr.ChangeSlideableItemDescription();
                TelemetryAdapter.TrackEvent("After swipe " + SettingsKeys.SwipeActionPlayNow);
            }
        }

        public bool SwipeActionNr2
        {
            get { return swipeActionNr.Equals(2); }
            set
            {
                SwipeActionNr = 2;
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.ActionAfterSwipeLeftCommand, SettingsKeys.SwipeActionPlayNext);
                TranslationHelper tr = new TranslationHelper();
                tr.ChangeSlideableItemDescription();
                TelemetryAdapter.TrackEvent("After swipe " + SettingsKeys.SwipeActionPlayNext);
            }
        }

        public bool SwipeActionNr3
        {
            get { return swipeActionNr.Equals(3); }
            set
            {
                SwipeActionNr = 3;
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.ActionAfterSwipeLeftCommand, SettingsKeys.SwipeActionAddToNowPlaying);
                TranslationHelper tr = new TranslationHelper();
                tr.ChangeSlideableItemDescription();
                TelemetryAdapter.TrackEvent("After swipe " + SettingsKeys.SwipeActionAddToNowPlaying);
            }
        }

        public bool SwipeActionNr4
        {
            get { return swipeActionNr.Equals(4); }
            set
            {
                SwipeActionNr = 4;
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.ActionAfterSwipeLeftCommand, SettingsKeys.SwipeActionAddToPlaylist);
                TranslationHelper tr = new TranslationHelper();
                tr.ChangeSlideableItemDescription();
                TelemetryAdapter.TrackEvent("After swipe " + SettingsKeys.SwipeActionAddToPlaylist);
            }
        }

        private bool isTimerOn = false;
        public bool IsTimerOn
        {
            get { return isTimerOn; }
            set
            {
                if (value != isTimerOn)
                {
                    ChangeTimer(value, time);
                }
                Set(ref isTimerOn, value);
            }
        }

        private TimeSpan time = TimeSpan.Zero;
        public TimeSpan Time
        {
            get { return time; }
            set
            {
                if (value != time)
                {
                    ChangeTimer(true, value);
                }
                Set(ref time, value);
            }
        }

        public void ChangeTimer(bool isOn, TimeSpan time)
        {
            if (!initialization)
            {
                if (isOn)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.TimerOn, true);
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.TimerTime, time.Ticks);
                    PlaybackService.Instance.SetPlaybackStopTimer();
                    TelemetryAdapter.TrackEvent("Timer on");
                }
                else
                {
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.TimerOn, false);
                    PlaybackService.Instance.CancelPlaybackStopTimer();
                }
            }
        }

        private bool preventScreenLock = false;
        public bool PreventScreenLock
        {
            get { return preventScreenLock; }
            set
            {
                if (value != preventScreenLock)
                {
                    ChangeScreenLock(value);
                }
                Set(ref preventScreenLock, value);
            }
        }

        private void ChangeScreenLock(bool isOn)
        {
            if (!initialization)
            {
                if (isOn)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DisableLockscreen, true);
                    displayRequestHelper.ActivateDisplay();
                    TelemetryAdapter.TrackEvent("Prevent screen dimming on");
                }
                else
                {
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.DisableLockscreen, false);
                    displayRequestHelper.ReleaseDisplay();
                    TelemetryAdapter.TrackEvent("Prevent screen dimming off");
                }
            }
        }

        private bool hideStatusBar = false;
        public bool HideStatusBar
        {
            get { return hideStatusBar; }
            set
            {
                if (value != hideStatusBar)
                {
                    ChangeStatusBarVisibility(value);
                }
                Set(ref hideStatusBar, value);
            }
        }

        private async Task ChangeStatusBarVisibility(bool hide)
        {
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.HideStatusBar, hide);
            await App.ChangeStatusBarVisibility(hide);
            if (!initialization)
            {
                TelemetryAdapter.TrackEvent("Hide status bar " + ((hide) ? "on" : "off"));
            }
        }

        private bool liveTileWithAlbumArt = false;
        public bool LiveTileWithAlbumArt
        {
            get { return liveTileWithAlbumArt; }
            set
            {
                if (value != liveTileWithAlbumArt)
                {
                    if (!initialization)
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.EnableLiveTileWithImage, value);
                        TelemetryAdapter.TrackEvent("LiveImage " + ((value) ? "on" : "off"));
                        PlaybackService.Instance.UpdateLiveTile(true);
                    }
                }
                Set(ref liveTileWithAlbumArt, value);
            }
        }

        private bool includeSubFolders = false;
        public bool IncludeSubFolders
        {
            get { return includeSubFolders; }
            set
            {
                if (value != includeSubFolders)
                {
                    if (!initialization)
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.IncludeSubFolders, value);
                        TelemetryAdapter.TrackEvent("IncludeSubFolders " + includeSubFolders);
                    }
                }
                Set(ref includeSubFolders, value);
            }
        }

        #endregion

        #region Personalize

        private bool isLightThemeOn = true;
        public bool IsLightThemeOn
        {
            get { return isLightThemeOn; }
            set {
                System.Diagnostics.Debug.WriteLine("Settings IsLightThemeOn {0}", value);
                Set(ref isLightThemeOn, value);
                if (value) ChangeAppTheme(); //only one radiobutton is true, so ChangeAppTheme is executed once
            }
        }

        private bool isDarkThemeOn = true;
        public bool IsDarkThemeOn
        {
            get { return isDarkThemeOn; }
            set {
                System.Diagnostics.Debug.WriteLine("Settings IsDarkThemeOn {0}", value);
                Set(ref isDarkThemeOn, value);
                if (value) ChangeAppTheme();
            }
        }

        private ObservableCollection<SolidColorBrush> accentColors = new ObservableCollection<SolidColorBrush>();
        public ObservableCollection<SolidColorBrush> AccentColors
        {
            get { return accentColors; }
            set { Set(ref accentColors, value); }
        }

        public void ChangeAppTheme()
        {
            if (initialization) return;
            System.Diagnostics.Debug.WriteLine("Settings ChangeAppTheme");
            App.IsLightThemeOn = isLightThemeOn;
            ThemeHelper.ApplyAppTheme(isLightThemeOn);

            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AppTheme, isLightThemeOn);

            AppMessenger.ChangeTheme(isLightThemeOn);
        }

        public void ChangeAccentColor(object sender, RoutedEventArgs e)
        {
            var grid = (GridView)sender;
            var brush = grid.SelectedItem as SolidColorBrush;
            if (brush == null) return;
            ColorsHelper ch = new ColorsHelper();
            ch.ChangeCurrentAccentColor(brush.Color);
            ch.SaveUserAccentColor(brush.Color);
        }

        private Dictionary<string, string> languageDescriptions = new Dictionary<string, string>()
        {
            {"ar","العربية" },
            {"cs","čeština" },
            {"de","Deutsch" },
            {"en","English" },
            {"es","Español" },
            {"fr","français" },
            {"id","Bahasa Indonesia" },
            {"it","Italiano" },
            {"pl","Polski" },
            {"pt","Português" },
            {"ru","русский" },
        };

        private ObservableCollection<LanguageItem> languages = new ObservableCollection<LanguageItem>();
        public ObservableCollection<LanguageItem> Languages
        {
            get { return languages; }
            set { Set(ref languages, value); }
        }

        private LanguageItem selectedLanguage = new LanguageItem();
        public LanguageItem SelectedLanguage
        {
            get { return selectedLanguage; }
            set { Set(ref selectedLanguage, value); }
        }

        public void ChangeLanguage(object sender, SelectionChangedEventArgs e)
        {
            if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride != selectedLanguage.Code)
            {
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = selectedLanguage.Code;
            }
        }

        private ObservableCollection<MenuButtonItem> menuButtons = new ObservableCollection<MenuButtonItem>();
        public ObservableCollection<MenuButtonItem> MenuButtons
        {
            get { return menuButtons; }
            set { Set(ref menuButtons, value); }
        }

        private void GetMenuButtons()
        {
            var menuList = ApplicationSettingsHelper.ReadData<List<MenuButtonItem>>(SettingsKeys.MenuEntries);
            HamburgerMenuBuilder builder = new HamburgerMenuBuilder();
            foreach (var item in menuList)
            {
                if (item.PageType == MenuItemType.NowPlayingPlaylist && DeviceFamilyHelper.IsDesktop())
                {
                    continue;
                }
                if (item.PageType == MenuItemType.NowPlayingSong && DeviceFamilyHelper.IsMobile())
                {
                    continue;
                }
                item.Name = builder.GetMenuItemText(item.PageType);
                MenuButtons.Add(item);
            }
        }

        public void SaveMenuItems()
        {
            var previous = ApplicationSettingsHelper.ReadData<List<MenuButtonItem>>(SettingsKeys.MenuEntries);
            var newOrder = new List<MenuButtonItem>();
            foreach(var item in menuButtons)
            {
                newOrder.Add(item);
            }
            var diff = previous.Where(p => !newOrder.Any(n => n.PageType == p.PageType));
            foreach(var item in diff)
            {
                item.ShowButton = false;
                newOrder.Add(item);
            }
            ApplicationSettingsHelper.SaveData(SettingsKeys.MenuEntries, newOrder);
            App.OnMenuItemVisibilityChange();
        }

        #endregion

        #region About
        private string appVersion = "";
        public string AppVersion
        {
            get { return appVersion; }
            set { Set(ref appVersion, value); }
        }

        private bool feedbackVisibility = false;
        public bool FeedbackVisibility
        {
            get { return feedbackVisibility; }
            set { Set(ref feedbackVisibility, value); }
        }

        public async void RateApp()
        {
            TelemetryAdapter.TrackEvent("Rate app button");
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.IsReviewed, -1);
            var uri = new Uri("ms-windows-store://review/?ProductId=" + AppConstants.ProductId);
            await Launcher.LaunchUriAsync(uri);
        }

        public async void LeaveFeedback()
        {
            TelemetryAdapter.TrackEvent("Leave feedback button");
            var launcher =  Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        public async void ContactWithSupport()
        {
            TelemetryAdapter.TrackEvent("Email support");
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.Subject = AppConstants.AppName;
            emailMessage.Body = "Next-Player";
            emailMessage.To.Add(new Windows.ApplicationModel.Email.EmailRecipient(AppConstants.DeveloperEmail));
            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
        }

        public void Licenses()
        {
            TelemetryAdapter.TrackEvent("View Licenses");
            NavigationService.Navigate(App.Pages.Licenses);
        }

        #endregion

        #region LastFm

        private string lastFmLogin = "";
        public string LastFmLogin
        {
            get { return lastFmLogin; }
            set { Set(ref lastFmLogin, value); }
        }

        private string lastFmPassword = "";
        public string LastFmPassword
        {
            get { return lastFmPassword; }
            set { Set(ref lastFmPassword, value); }
        }

        private bool isLoginButtonEnabled = true;
        public bool IsLoginButtonEnabled
        {
            get { return isLoginButtonEnabled; }
            set { Set(ref isLoginButtonEnabled, value); }
        }

        private bool isLastFmLoggedIn = false;
        public bool IsLastFmLoggedIn
        {
            get { return isLastFmLoggedIn; }
            set { Set(ref isLastFmLoggedIn, value); }
        }

        private bool lastFmShowError = false;
        public bool LastFmShowError
        {
            get { return lastFmShowError; }
            set { Set(ref lastFmShowError, value); }
        }

        private bool lastFmRateSongs = false;
        public bool LastFmRateSongs
        {
            get { return lastFmRateSongs; }
            set
            {
                if (value != lastFmRateSongs)
                {
                    ChangeLastFmRateSongs(value);
                }
                Set(ref lastFmRateSongs, value);
            }
        }

        private void ChangeLastFmRateSongs(bool isOn)
        {
            if (!initialization)
            {
                if (isOn)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LfmRateSongs, true);
                    TelemetryAdapter.TrackEvent("Last.fm rate songs on");
                }
                else
                {
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LfmRateSongs, false);
                    TelemetryAdapter.TrackEvent("Lat.fm rate songs off");
                }
            }
        }

        public async void LastFmLogIn()
        {
            IsLoginButtonEnabled = false;
            IsLastFmLoggedIn = await LastFmManager.Login(lastFmLogin, lastFmPassword);
            if (isLastFmLoggedIn)
            {
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LfmLogin, lastFmLogin);
                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LfmPassword, lastFmPassword);

                LastFmShowError = false;
                LastFmPassword = "";
                TelemetryAdapter.TrackEvent("LastFm log in");
            }
            else
            {
                LastFmShowError = true;
                LastFmPassword = "";
            }
            IsLoginButtonEnabled = true;
        }

        public void LastFmLogOut()
        {
            LastFmLogin = "";
            LastFmPassword = "";

            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LfmLogin, "");
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LfmPassword, "");
            ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.LfmSessionKey, "");

            LastFmManager.Logout();

            IsLastFmLoggedIn = false;

            TelemetryAdapter.TrackEvent("LastFm log out");
        }

        #endregion


        public async void CloudStorageLogout(object sender, RoutedEventArgs e)
        {
            CloudAccount account = (CloudAccount)((Button)sender).Tag;
            var cf = new CloudStorageServiceFactory();
            var service = cf.GetService(account.Type, account.UserId);
            await service.LoginSilently();
            await service.Logout();
            switch (account.Type)
            {
                case CloudStorageType.Dropbox:
                    DropboxAccounts.Remove(account);
                    break;
                case CloudStorageType.OneDrive:
                    OneDriveAccounts.Remove(account);
                    break;
                default:
                    break;
            }
        }

        #region OneDrive

        private ObservableCollection<CloudAccount> oneDriveAccounts = new ObservableCollection<CloudAccount>();
        public ObservableCollection<CloudAccount> OneDriveAccounts
        {
            get { return oneDriveAccounts; }
            set { Set(ref oneDriveAccounts, value); }
        }

        private bool isOneDriveLoginEnabled = true;
        public bool IsOneDriveLoginEnabled
        {
            get { return isOneDriveLoginEnabled; }
            set { Set(ref isOneDriveLoginEnabled, value); }
        }

        public async void AddOneDriveAccount()
        {
            IsOneDriveLoginEnabled = false;
            var cf = new CloudStorageServiceFactory();
            var service = cf.GetService(CloudStorageType.OneDrive);
            var isLoggedIn = await service.Login();
            if (isLoggedIn)
            {
                var info = await service.GetAccountInfo();
                if (info != null) OneDriveAccounts.Add(info);
                TelemetryAdapter.TrackEvent("OneDrive Login");
            }
            IsOneDriveLoginEnabled = true;
        }

        #endregion

        #region Dropbox

        private ObservableCollection<CloudAccount> dropboxAccounts = new ObservableCollection<CloudAccount>();
        public ObservableCollection<CloudAccount> DropboxAccounts
        {
            get { return dropboxAccounts; }
            set { Set(ref dropboxAccounts, value); }
        }

        private bool isDropboxLoginEnabled = true;
        public bool IsDropboxLoginEnabled
        {
            get { return isDropboxLoginEnabled; }
            set { Set(ref isDropboxLoginEnabled, value); }
        }

        public async void AddDropboxAccount()
        {
            IsDropboxLoginEnabled = false;
            var cf = new CloudStorageServiceFactory();
            var service = cf.GetService(CloudStorageType.Dropbox);
            var isLoggedIn = await service.Login();
            if (isLoggedIn)
            {
                var info = await service.GetAccountInfo();
                if (info != null) DropboxAccounts.Add(info);
                TelemetryAdapter.TrackEvent("Dropbox Login");
            }
            IsDropboxLoginEnabled = true;
        }

        #endregion

        #region pCloud

        private ObservableCollection<CloudAccount> pCloudAccounts = new ObservableCollection<CloudAccount>();
        public ObservableCollection<CloudAccount> PCloudAccounts
        {
            get { return pCloudAccounts; }
            set { Set(ref pCloudAccounts, value); }
        }

        private bool isPCloudLoginEnabled = true;
        public bool IsPCloudLoginEnabled
        {
            get { return isPCloudLoginEnabled; }
            set { Set(ref isPCloudLoginEnabled, value); }
        }

        public async void AddPCloudAccount()
        {
            IsPCloudLoginEnabled = false;
            var cf = new CloudStorageServiceFactory();
            var service = cf.GetService(CloudStorageType.pCloud);
            var isLoggedIn = await service.Login();
            if (isLoggedIn)
            {
                var info = await service.GetAccountInfo();
                if (info != null) PCloudAccounts.Add(info);
                TelemetryAdapter.TrackEvent("pCloud Login");
            }
            IsPCloudLoginEnabled = true;
        }

        #endregion

        #region GoogleDrive

        private bool isGoogleDriveLoggedIn = false;
        public bool IsGoogleDriveLoggedIn
        {
            get { return isGoogleDriveLoggedIn; }
            set { Set(ref isGoogleDriveLoggedIn, value); }
        }

        private bool isGoogleDriveLoginEnabled = true;
        public bool IsGoogleDriveLoginEnabled
        {
            get { return isGoogleDriveLoginEnabled; }
            set { Set(ref isGoogleDriveLoginEnabled, value); }
        }

        public async void GoogleDriveLogin()
        {
            IsGoogleDriveLoginEnabled = false;
            //await GoogleDriveService.Instance.Login();
            IsGoogleDriveLoginEnabled = true;
        }

        public async void GoogleDriveLogout()
        {
            //await GoogleDriveService.Instance.Logout();
            IsGoogleDriveLoggedIn = false;
        }

        #endregion

        //public SettingsAboutViewModel SettingsAboutVM { get; } = new SettingsAboutViewModel();
        private bool aboutVisibility = false;
        public bool AboutVisibility
        {
            get { return aboutVisibility; }
            set
            {
                Set(ref aboutVisibility, value);
            }
        }
        public void Expand()
        {
            AboutVisibility = true;
        }
        //public SettingsAccountsViewModel SettingsAccountsVM { get; } = new SettingsAccountsViewModel();
    }

    

    
}
