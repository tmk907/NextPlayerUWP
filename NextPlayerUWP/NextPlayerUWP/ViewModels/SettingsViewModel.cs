using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Enums;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class MusicFolder
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

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

        private string updateProgressText = "";
        public string UpdateProgressText
        {
            get { return updateProgressText; }
            set { Set(ref updateProgressText, value); }
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

        private ObservableCollection<MusicFolder> musicLibraryFolders = new ObservableCollection<MusicFolder>();
        public ObservableCollection<MusicFolder> MusicLibraryFolders
        {
            get { return musicLibraryFolders; }
            set { Set(ref musicLibraryFolders, value); }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            initialization = true;

            // Tools
            var tt = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TimerTime);
            var IsTimerOn = (bool)(ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TimerOn)??false);
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
            
            string action = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.ActionAfterDropItem) as string;
            if (action.Equals(AppConstants.ActionPlayNow))
            {
                ActionNr = 1;
            }
            else if (action.Equals(AppConstants.ActionPlayNext))
            {
                ActionNr = 2;
            }
            else
            {
                ActionNr = 3;
            }

            PreventScreenLock = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.DisableLockscreen);
            HideStatusBar = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.HideStatusBar);
            IncludeSubFolders = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.IncludeSubFolders);

            //Personalization
            IsLightThemeOn = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppTheme);

            if (accentColors.Count == 0)
            {
                ColorsHelper ch = new ColorsHelper();
                var sc = ch.GetSavedUserAccentColor();
                foreach (var c in ch.GetWin10Colors())
                {
                    AccentColors.Add(new SolidColorBrush(c));
                }
            }

            //Library

            if (!isUpdating)
            {
                UpdateProgressText = "";
                UpdateProgressTextVisibility = false;
            }
            if (musicLibraryFolders.Count == 0)
            {
                var lib = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
                foreach (var f in lib.Folders)
                {
                    MusicLibraryFolders.Add(new MusicFolder() { Name = f.DisplayName, Path = f.Path });
                }
            }
            PlaylistsFolder = Windows.Storage.KnownFolders.Playlists.Path;
            AutoSavePlaylists = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AutoSavePlaylists);

            //Last.fm
            string login = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmLogin) as string;
            LastFmLogin = login;
            string session = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmSessionKey) as string;
            if (!String.IsNullOrEmpty(session))
            {
                IsLastFmLoggedIn = true;
            }
            LastFmRateSongs = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.LfmRateSongs);
            LastFmShowError = false;

            //About
            if (Microsoft.Services.Store.Engagement.Feedback.IsSupported)
            {
                FeedbackVisibility = true;
            }
            initialization = false;
        }

        #region Library

        public async void UpdateLibrary()
        {
            MediaImport m = new MediaImport();
            UpdateProgressTextVisibility = true;
            Progress<int> progress = new Progress<int>(
                percent =>
                {
                    UpdateProgressText = percent.ToString();
                }
            );
            IsUpdating = true;
            await Task.Run(() => m.UpdateDatabase(progress));
            IsUpdating = false;
            TelemetryAdapter.TrackEvent("Library updated");
        }

        public async void AddFolder()
        {
            var musicLibrary = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
            Windows.Storage.StorageFolder newFolder = await musicLibrary.RequestAddFolderAsync();
            if (newFolder != null)
            {
                MusicLibraryFolders.Add(new MusicFolder() { Name = newFolder.DisplayName, Path = newFolder.Path });
            }
        }

        public async void RemoveFolder(MusicFolder musicFolder)
        {
            var musicLibrary = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
            var folder = musicLibrary.Folders.Where(f => f.Path.Equals(musicFolder.Path)).FirstOrDefault();
            //var fi = await f.GetFilesAsync();
            //var fp = fi.FirstOrDefault().Properties;
            //var mp =  await fp.GetMusicPropertiesAsync();

            bool confirmDeletion = await musicLibrary.RequestRemoveFolderAsync(folder);
            if (confirmDeletion)
            {
                MusicLibraryFolders.Remove(musicFolder);
                //usun utwory z biblioteki
                await DatabaseManager.Current.DeleteFolderAndSubFoldersAsync(musicFolder.Path);
                MediaImport.OnMediaImported("FolderRemoved");
            }
        }

        private string playlistsFolder;
        public string PlaylistsFolder
        {
            get { return playlistsFolder; }
            set { Set(ref playlistsFolder, value); }
        }

        private bool autoSavePlaylists;
        public bool AutoSavePlaylists
        {
            get { return autoSavePlaylists; }
            set
            {
                if (value != autoSavePlaylists && !initialization)
                {
                    if (value) SavePlaylists();
                    else DeletePlaylists();
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AutoSavePlaylists, value);
                    TelemetryAdapter.TrackEvent("AutoSavePlaylists " + value);
                }
                Set(ref autoSavePlaylists, value);
            }
        }

        public async Task SavePlaylists()
        {
            PlaylistExporter pe = new PlaylistExporter();
            await pe.SaveAllPlaylists().ConfigureAwait(false);
        }

        public async Task DeletePlaylists()
        {
            PlaylistExporter pe = new PlaylistExporter();
            await pe.DeleteAllPlaylists().ConfigureAwait(false);
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
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterDropItem, AppConstants.ActionPlayNow);
                TelemetryAdapter.TrackEvent("After Drop Item " + AppConstants.ActionPlayNow);
            }
        }

        public bool ActionNr2
        {
            get { return actionNr.Equals(2); }
            set
            {
                ActionNr = 2;
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterDropItem, AppConstants.ActionPlayNext);
                TelemetryAdapter.TrackEvent("After Drop Item " + AppConstants.ActionPlayNext);
            }
        }

        public bool ActionNr3
        {
            get { return actionNr.Equals(3); }
            set
            {
                ActionNr = 3;
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterDropItem, AppConstants.ActionAddToNowPlaying);
                TelemetryAdapter.TrackEvent("After Drop Item " + AppConstants.ActionAddToNowPlaying);
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
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, true);
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerTime, time.Ticks);
                    SendMessage(AppConstants.SetTimer);
                    TelemetryAdapter.TrackEvent("Timer on");
                }
                else
                {
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, false);
                    SendMessage(AppConstants.CancelTimer);
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
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DisableLockscreen, true);
                    displayRequestHelper.ActivateDisplay();
                    TelemetryAdapter.TrackEvent("Prevent screen dimming on");
                }
                else
                {
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.DisableLockscreen, false);
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
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.HideStatusBar, hide);
            await App.ChangeStatusBarVisibility(hide);
            if (!initialization)
            {
                TelemetryAdapter.TrackEvent("Hide status bar " + ((hide) ? "on" : "off"));
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
                        ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IncludeSubFolders, value);
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
                Set(ref isLightThemeOn, value);
                if (value) ChangeAppTheme(); //only one radiobutton is true, so ChangeAppTheme is executed once
            }
        }

        private bool isDarkThemeOn = true;
        public bool IsDarkThemeOn
        {
            get { return isDarkThemeOn; }
            set {
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
            App.IsLightThemeOn = isLightThemeOn;
            if (isLightThemeOn)
            {               
                //ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppTheme, AppTheme.Light);
                App.Current.NavigationService.Frame.RequestedTheme = ElementTheme.Light;
                ThemeHelper.ApplyThemeToTitleBar(ElementTheme.Light);
                ThemeHelper.ApplyThemeToStatusBar(ElementTheme.Light);
            }
            else
            {
                App.Current.NavigationService.Frame.RequestedTheme = ElementTheme.Dark;
                ThemeHelper.ApplyThemeToTitleBar(ElementTheme.Dark);
                ThemeHelper.ApplyThemeToStatusBar(ElementTheme.Dark);
            }

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppTheme, isLightThemeOn);

            App.OnAppThemeChanged(isLightThemeOn);
        }

        public void ChangeAccentColor(object sender, RoutedEventArgs e)
        {
            var grid = (GridView)sender;
            var brush = grid.SelectedItem as SolidColorBrush;
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
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IsReviewed, -1);
            var uri = new Uri("ms-windows-store://review/?ProductId=" + AppConstants.ProductId);
            await Launcher.LaunchUriAsync(uri);
        }

        public async void LeaveFeedback()
        {
            TelemetryAdapter.TrackEvent("Leave feedback button");
            await Microsoft.Services.Store.Engagement.Feedback.LaunchFeedbackAsync();
        }

        public async void SendEmail()
        {
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.Subject = AppConstants.AppName;
            emailMessage.Body = "";
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
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmRateSongs, true);
                    TelemetryAdapter.TrackEvent("Last.fm rate songs on");
                }
                else
                {
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmRateSongs, false);
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
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmLogin, lastFmLogin);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmPassword, lastFmPassword);

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

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmLogin, "");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmPassword, "");
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.LfmSessionKey, "");

            LastFmManager.Logout();

            IsLastFmLoggedIn = false;

            TelemetryAdapter.TrackEvent("LastFm log out");
        }

        #endregion

        private void SendMessage(string message)
        {
            App.PlaybackManager.SendMessage(message, "");
        }
    }
}
