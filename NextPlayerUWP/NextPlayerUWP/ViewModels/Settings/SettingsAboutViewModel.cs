using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Template10.Common;
using Windows.ApplicationModel;
using Windows.System;

namespace NextPlayerUWP.ViewModels.Settings
{
    public class TranslationEntry
    {
        public string Language { get; set; }
        public List<string> Translators { get; set; }
    }

    public class SettingsAboutViewModel : Template10.Mvvm.ViewModelBase, ISettingsViewModel
    {
        public SettingsAboutViewModel()
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
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            AppVersion = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
#if DEBUG
            AppVersion = AppVersion + " Debug";
#endif
            if (Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
            {
                FeedbackVisibility = true;
            }
            ShowTranslators();
            isLoaded = true;
        }

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

        private ObservableCollection<TranslationEntry> translations = new ObservableCollection<TranslationEntry>();
        public ObservableCollection<TranslationEntry> Translations
        {
            get { return translations; }
            set { Set(ref translations, value); }
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
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
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
            var nav = WindowWrapper.Current().NavigationServices.FirstOrDefault();
            nav.Navigate(AppPages.Pages.Licenses);
        }

        public async void GoToFacebook()
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://www.facebook.com/Next-Player-Music-Player-1719236521668808"));
        }

        public async void TranslateApp()
        {
            await Launcher.LaunchUriAsync(new Uri(@"https://github.com/tmk907/Next-Player_Translations"));
        }

        public void ShowTranslators()
        {
            if (translations.Count != 0) return;
            Translations = new ObservableCollection<TranslationEntry>()
            {
                new TranslationEntry()
                {
                    Language = "Arabic",
                    Translators = new List<string>(){ "JamalM" }
                },
                new TranslationEntry()
                {
                    Language = "French",
                    Translators = new List<string>(){ "Shinshia" }
                },
                new TranslationEntry()
                {
                    Language = "German",
                    Translators = new List<string>(){ "brullsker", "buagseitei", "Sebster" }
                },
                new TranslationEntry()
                {
                    Language = "Indonesian",
                    Translators = new List<string>(){ "aires", "Tzaa" }
                },
                new TranslationEntry()
                {
                    Language = "Italian",
                    Translators = new List<string>(){ "Salvo85" }
                },
                new TranslationEntry()
                {
                    Language = "Russian",
                    Translators = new List<string>(){ "interpreter", "LaFee", "Mascia", "NastyaR" }
                },
                new TranslationEntry()
                {
                    Language = "Portuguese",
                    Translators = new List<string>(){ "David Baptista da Silva" }
                },
                new TranslationEntry()
                {
                    Language = "Brazilian Portuguese",
                    Translators = new List<string>(){ "elcioebel", "Gis" }
                },
                new TranslationEntry()
                {
                    Language = "Spanish",
                    Translators = new List<string>(){ "jeac76", "jsnt", "VictorCas" }
                },
            };
        }
    }
}
