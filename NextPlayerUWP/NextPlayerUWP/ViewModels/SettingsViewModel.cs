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

            // Timer
            var tt = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TimerTime);
            var to = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.TimerOn);
            if (to == null)
            {
                IsTimerOn = false;
            }
            else
            {
                IsTimerOn = (bool)to;
            }
            Time = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            if (isTimerOn)
            {
                if (tt != null)
                {
                    Time = TimeSpan.FromTicks((long)tt);
                }
            }
            else
            {
                IsTimerOn = false;
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

            //Personalization
            bool islightthemeon = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.AppTheme);
            if (islightthemeon)
            {
                IsLightThemeOn = true;
            }
            else
            {
                IsLightThemeOn = false;
            }
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

            //if (languages.Count == 0)
            //{
            //    Languages = new ObservableCollection<LanguageItem>();
            //    foreach(var code in Windows.Globalization.ApplicationLanguages.ManifestLanguages)
            //    {
            //        string name = "unknown";
            //        languageDescriptions.TryGetValue(code.Substring(0, 2), out name);
            //        Languages.Add(new LanguageItem() { Code = code, Name = name });
            //    }
            //}
            //string primaryCode = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
            //foreach(var l in languages)
            //{
            //    if (l.Code == primaryCode)
            //    {
            //        SelectedLanguage = new LanguageItem() { Code = l.Code, Name = l.Name };
            //    }
            //}



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
                await DatabaseManager.Current.DeleteFolderAsync(musicFolder.Path);
                MediaImport.OnMediaImported("FolderRemoved");
            }
        }

        #endregion

        #region Tools

        private bool isTimerOn = false;
        public bool IsTimerOn
        {
            get { return isTimerOn; }
            set { Set(ref isTimerOn, value); }
        }

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
            }
        }

        public bool ActionNr2
        {
            get { return actionNr.Equals(2); }
            set
            {
                ActionNr = 2;
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterDropItem, AppConstants.ActionPlayNext);
            }
        }

        public bool ActionNr3
        {
            get { return actionNr.Equals(3); }
            set
            {
                ActionNr = 3;
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.ActionAfterDropItem, AppConstants.ActionAddToNowPlaying);
            }
        }

        private TimeSpan time = TimeSpan.Zero;
        public TimeSpan Time
        {
            get { return time; }
            set {
                if (!initialization)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerTime, Time.Ticks);
                    TimeSpan now = TimeSpan.FromHours(DateTime.Now.Hour) + TimeSpan.FromMinutes(DateTime.Now.Minute);
                    TimeSpan difference = TimeSpan.FromTicks(Time.Ticks - now.Ticks);
                    if (difference <= TimeSpan.Zero || !isTimerOn) return;
                    else
                    {
                        SendMessage(AppConstants.SetTimer);
                    }
                }
                Set(ref time, value);
            }
        }

        public void TimerSwitchToggled(object sender, RoutedEventArgs e)
        {
            if (((ToggleSwitch)sender).IsOn)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, true);
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerTime, Time.Ticks);
                SendMessage(AppConstants.SetTimer);
                //App.TelemetryClient.TrackEvent("Timer On");
            }
            else
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerOn, false);
                SendMessage(AppConstants.CancelTimer);
            }
        }

        public void TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (!initialization)
            {
                ApplicationSettingsHelper.SaveSettingsValue(AppConstants.TimerTime, Time.Ticks);
                TimeSpan now = TimeSpan.FromHours(DateTime.Now.Hour) + TimeSpan.FromMinutes(DateTime.Now.Minute);
                TimeSpan difference = TimeSpan.FromTicks(Time.Ticks - now.Ticks);
                if (difference <= TimeSpan.Zero) return;
                else
                {
                    SendMessage(AppConstants.SetTimer);
                }
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
            }
            else
            {
                App.Current.NavigationService.Frame.RequestedTheme = ElementTheme.Dark;
            }

            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.AppTheme, isLightThemeOn);

            App.OnAppThemChanged(isLightThemeOn);

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    if (isLightThemeOn)
                    {
                        titleBar.BackgroundColor = Colors.White;
                        titleBar.ButtonBackgroundColor = Colors.White;
                        titleBar.ButtonForegroundColor = Colors.Black;
                        titleBar.ForegroundColor = Colors.Black;
                        //titleBar.ButtonHoverBackgroundColor = 
                    }
                    else
                    {
                        titleBar.BackgroundColor = Colors.Black;
                        titleBar.ButtonBackgroundColor = Colors.Black;
                        titleBar.ButtonForegroundColor = Colors.White;
                        titleBar.ForegroundColor = Colors.White;
                    }
                }
            }
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
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IsReviewed, -1);
            var uri = new Uri("ms-windows-store://review/?ProductId=" + AppConstants.ProductId);
            await Launcher.LaunchUriAsync(uri);
        }

        public async void LeaveFeedback()
        {
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
            NavigationService.Navigate(App.Pages.Licenses);
        }

        #endregion

        private bool IsMyBackgroundTaskRunning
        {
            get
            {
                object value = ApplicationSettingsHelper.ReadSettingsValue(AppConstants.BackgroundTaskState);
                if (value == null)
                {
                    return false;
                }
                else
                {
                    var state = EnumHelper.Parse<BackgroundTaskState>(value as string);
                    bool isRunning = state == BackgroundTaskState.Running;
                    return isRunning;
                }
            }
        }

        private void SendMessage(string message)
        {
            if (IsMyBackgroundTaskRunning)
            {
                PlaybackManager.Current.SendMessage(message, "");
            }
        }
    }
}
