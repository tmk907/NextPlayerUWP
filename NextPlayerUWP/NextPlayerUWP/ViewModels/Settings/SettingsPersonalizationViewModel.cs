using NextPlayerUWP.AppColors;
using NextPlayerUWP.Common;
using NextPlayerUWP.Messages;
using NextPlayerUWP.Messages.Hub;
using NextPlayerUWPDataLayer.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NextPlayerUWP.ViewModels.Settings
{
    public class LanguageItem
    {
        public string Name { get; set; }
        public string Code { get; set; }
    }

    public class SettingsPersonalizationViewModel : Template10.Mvvm.ViewModelBase, ISettingsViewModel
    {
        public SettingsPersonalizationViewModel()
        {
            isLoaded = false;
        }

        public void Load()
        {
            OnLoaded();
        }

        private bool isLoaded;
        private void OnLoaded()
        {
            if (isLoaded) return;
            Languages = new ObservableCollection<LanguageItem>();
            foreach (var code in Windows.Globalization.ApplicationLanguages.ManifestLanguages)
            {
                string name = "unknown";
                string langMain = code.Substring(0, 2);
                if (langMain == "pt")
                {
                    if (code == "pt-BR")
                    {
                        langMain = "pt-BR";
                    }
                    else
                    {
                        langMain = "pt-PT";
                    }
                }
                languageDescriptions.TryGetValue(langMain, out name);
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
            ThemeNr = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.AppTheme) ? 1 : 2;
            if (accentColors.Count == 0)
            {
                AppAccentColors ch = new AppAccentColors();
                var sc = AppAccentColors.GetSavedAppAccentColor();
                foreach (var c in ch.GetWin10AccentColors())
                {
                    AccentColors.Add(new SolidColorBrush(c));
                }
            }
            AccentFromAlbumArt = ApplicationSettingsHelper.ReadSettingsValue<bool>(SettingsKeys.AccentFromAlbumArt);
            AlbumArtInBackground = ApplicationSettingsHelper.ReadSettingsValue<bool>(SettingsKeys.AlbumArtInBackground);

            PrepareStartPages();

            if (menuButtons.Count == 0)
            {
                GetMenuButtons();
            }

            isLoaded = true;
        }

        DisplayRequestHelper displayRequestHelper;

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
            if (isLoaded)
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
            if (isLoaded)
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
                    if (isLoaded)
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
                    if (isLoaded)
                    {
                        ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.IncludeSubFolders, value);
                        TelemetryAdapter.TrackEvent("IncludeSubFolders " + includeSubFolders);
                    }
                }
                Set(ref includeSubFolders, value);
            }
        }

        private int themeNr = default(int);
        public int ThemeNr
        {
            get { return themeNr; }
            set
            {
                Set(ref themeNr, value);
                RaisePropertyChanged("IsLightThemeOn");
                RaisePropertyChanged("IsDarkThemeOn");
            }
        }

        private bool isLightThemeOn = true;
        public bool IsLightThemeOn
        {
            get { return themeNr.Equals(1); }
            set
            {
                System.Diagnostics.Debug.WriteLine("Settings IsLightThemeOn {0}", value);
                ThemeNr = 1;
                ChangeAppTheme(); //only one radiobutton is true, so ChangeAppTheme is executed once
            }
        }

        private bool isDarkThemeOn = true;
        public bool IsDarkThemeOn
        {
            get { return themeNr.Equals(2); }
            set
            {
                System.Diagnostics.Debug.WriteLine("Settings IsDarkThemeOn {0}", value);
                ThemeNr = 2;
                ChangeAppTheme();
            }
        }
        
        public void ChangeAppTheme()
        {
            if (isLoaded)
            {
                System.Diagnostics.Debug.WriteLine("Settings ChangeAppTheme");
                App.IsLightThemeOn = IsLightThemeOn;
                ThemeHelper.ApplyAppTheme(IsLightThemeOn);

                ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AppTheme, IsLightThemeOn);

                MessageHub.Instance.Publish<ThemeChange>(new ThemeChange() { IsLightTheme = IsLightThemeOn });
            }
        }

        private ObservableCollection<SolidColorBrush> accentColors = new ObservableCollection<SolidColorBrush>();
        public ObservableCollection<SolidColorBrush> AccentColors
        {
            get { return accentColors; }
            set { Set(ref accentColors, value); }
        }

        public void ChangeAccentColor(object sender, RoutedEventArgs e)
        {
            var grid = (GridView)sender;
            var brush = grid.SelectedItem as SolidColorBrush;
            if (brush == null) return;
            AppAccentColors.ChangeAppAccentColor(brush.Color, brush.Color);
            AppAccentColors.SaveAppAccentColor(brush.Color);
            AccentFromAlbumArt = false;
        }

        private bool accentFromAlbumArt = false;
        public bool AccentFromAlbumArt
        {
            get { return accentFromAlbumArt; }
            set
            {
                if (isLoaded && value != accentFromAlbumArt)
                {
                    MessageHub.Instance.Publish(new AppColorAccent() { UseAlbumArtAccent = value });
                    if (!value)
                    {
                        AppAccentColors.RestoreAppAccentColors();
                    }
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AccentFromAlbumArt, value);
                }
                Set(ref accentFromAlbumArt, value);
            }
        }

        private bool albumArtInBackground = false;
        public bool AlbumArtInBackground
        {
            get { return albumArtInBackground; }
            set
            {
                Set(ref albumArtInBackground, value);
                if (isLoaded)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.AlbumArtInBackground, value);
                }
            }
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
            {"pt-PT","Português" },
            {"pt-BR","Português brasileiro" },
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
            foreach (var item in menuButtons)
            {
                newOrder.Add(item);
            }
            var diff = previous.Where(p => !newOrder.Any(n => n.PageType == p.PageType));
            foreach (var item in diff)
            {
                item.ShowButton = false;
                newOrder.Add(item);
            }
            ApplicationSettingsHelper.SaveData(SettingsKeys.MenuEntries, newOrder);
            App.OnMenuItemVisibilityChange();
        }

        protected ObservableCollection<ComboBoxItemValue> startPages = new ObservableCollection<ComboBoxItemValue>();
        public ObservableCollection<ComboBoxItemValue> StartPages
        {
            get { return startPages; }
            set
            {
                startPages = value;
            }
        }

        protected ComboBoxItemValue selectedStartPage;
        public ComboBoxItemValue SelectedStartPage
        {
            get { return selectedStartPage; }
            set
            {
                bool diff = selectedStartPage != value;
                Set(ref selectedStartPage, value);
                if (value != null && diff)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.StartPage, value.Option);
                }
            }
        }

        private void PrepareStartPages()
        {
            var list = new ObservableCollection<ComboBoxItemValue>();
            TranslationHelper tr = new TranslationHelper();
            var deeplink = new DeepLinkService();

            list.Add(new ComboBoxItemValue(
                    deeplink.CreateDeepLink(AppPages.Pages.Albums,new Dictionary<string, string>()),
                    tr.GetTranslation("TBAlbums/Text")));
            list.Add(new ComboBoxItemValue(
                    deeplink.CreateDeepLink(AppPages.Pages.Artists, new Dictionary<string, string>()),
                    tr.GetTranslation("TBArtists/Text")));
            list.Add(new ComboBoxItemValue(
                    deeplink.CreateDeepLink(AppPages.Pages.AlbumArtists, new Dictionary<string, string>()),
                    tr.GetTranslation("TBAlbumArtists/Text")));
            list.Add(new ComboBoxItemValue(
                    deeplink.CreateDeepLink(AppPages.Pages.FoldersRoot, new Dictionary<string, string>()),
                    tr.GetTranslation("TBFolders/Text")));
            list.Add(new ComboBoxItemValue(
                    deeplink.CreateDeepLink(AppPages.Pages.Genres, new Dictionary<string, string>()),
                    tr.GetTranslation("TBGenres/Text")));
            list.Add(new ComboBoxItemValue(
                    deeplink.CreateDeepLink(AppPages.Pages.Playlists, new Dictionary<string, string>()),
                    tr.GetTranslation("TBPlaylists/Text")));
            list.Add(new ComboBoxItemValue(
                    deeplink.CreateDeepLink(AppPages.Pages.Songs, new Dictionary<string, string>()),
                    tr.GetTranslation("TBSongs/Text")));
            list.Add(new ComboBoxItemValue(
                    deeplink.CreateDeepLink(AppPages.Pages.NowPlaying, new Dictionary<string, string>()),
                    tr.GetTranslation("TBNowPlaying/Text")));
            list.Add(new ComboBoxItemValue(
                    deeplink.CreateDeepLink(AppPages.Pages.Radios, new Dictionary<string, string>()),
                    tr.GetTranslation("TBRadio/Text")));
            StartPages = list;
            var savedOption = ApplicationSettingsHelper.ReadSettingsValue<string>(SettingsKeys.StartPage);
            SelectedStartPage = StartPages.FirstOrDefault(p => p.Option.Equals(savedOption));
        }
    }
}
