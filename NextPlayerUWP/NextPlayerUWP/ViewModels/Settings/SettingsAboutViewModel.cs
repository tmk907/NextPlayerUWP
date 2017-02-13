using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Constants;
using NextPlayerUWPDataLayer.Helpers;
using System;
using System.Linq;
using Template10.Common;
using Windows.ApplicationModel;
using Windows.System;

namespace NextPlayerUWP.ViewModels.Settings
{
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
            nav.Navigate(App.Pages.Licenses);
        }
    }
}
