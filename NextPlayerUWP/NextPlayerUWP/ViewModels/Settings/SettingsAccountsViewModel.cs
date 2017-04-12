using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.CloudStorage;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NextPlayerUWP.ViewModels.Settings
{
    public class SettingsAccountsViewModel : Template10.Mvvm.ViewModelBase, ISettingsViewModel
    {
        public SettingsAccountsViewModel()
        {
            isLoaded = false;
            //OnLoaded();
        }


        public void Load()
        {
            OnLoaded();
        }

        private bool isLoaded;

        private void OnLoaded()
        {
            LastFmLogin = LastFmManager.GetUsername();
            IsLastFmLoggedIn = LastFmManager.IsUserLoggedIn();
            LastFmRateSongs = (bool)ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.LfmRateSongs);
            LastFmShowError = false;

            OneDriveAccounts = new ObservableCollection<CloudAccount>(CloudAccounts.Instance.GetAccountsByType(CloudStorageType.OneDrive));
            DropboxAccounts = new ObservableCollection<CloudAccount>(CloudAccounts.Instance.GetAccountsByType(CloudStorageType.Dropbox));
            PCloudAccounts = new ObservableCollection<CloudAccount>(CloudAccounts.Instance.GetAccountsByType(CloudStorageType.pCloud));

            isLoaded = true;
        }

        LastFmManager lastFmManager = null;
        LastFmManager LastFmManager
        {
            get
            {
                if (lastFmManager == null) lastFmManager = new LastFmManager();
                return lastFmManager;
            }
        }

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
            if (isLoaded)
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

            LastFmManager.Logout();

            IsLastFmLoggedIn = false;

            TelemetryAdapter.TrackEvent("LastFm log out");
        }



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
    }
}
