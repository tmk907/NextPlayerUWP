﻿using GalaSoft.MvvmLight.Command;
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
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class MusicFolder
    {
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class SettingsViewModel : Template10.Mvvm.ViewModelBase
    {
        public SettingsViewModel()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            AppVersion = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
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
            EnableTelemetry = (bool)ApplicationSettingsHelper.ReadSettingsValue(AppConstants.EnableTelemetry);

            initialization = false;
        }

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

        #region Tools

        private bool isTimerOn = false;
        public bool IsTimerOn
        {
            get { return isTimerOn; }
            set { Set(ref isTimerOn, value); }
        }

        private TimeSpan time = TimeSpan.Zero;
        public TimeSpan Time
        {
            get { return time; }
            set { Set(ref time, value); }
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
                TimeSpan t1 = TimeSpan.FromHours(DateTime.Now.Hour) + TimeSpan.FromMinutes(DateTime.Now.Minute);
                long l1 = t1.Ticks;
                long l2 = Time.Ticks;
                TimeSpan t2 = TimeSpan.FromTicks(l2 - l1);
                if (t2 <= TimeSpan.Zero) return;
                else
                {
                    SendMessage(AppConstants.SetTimer);
                }
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

        private bool enableTelemetry = true;
        public bool EnableTelemetry
        {
            get { return enableTelemetry; }
            set { Set(ref enableTelemetry, value); }
        }

        public void TelemetrySwitchToggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.EnableTelemetry, true);
                }
                else
                {
                    ApplicationSettingsHelper.SaveSettingsValue(AppConstants.EnableTelemetry, true);
                }
            }
            //App..Current..ChangeTelemetry(toggleSwitch.IsOn);
        }

        public async void RateApp()
        {
            ApplicationSettingsHelper.SaveSettingsValue(AppConstants.IsReviewed, -1);
            var uri = new Uri("ms-windows-store://review/?ProductId=" + AppConstants.ProductId);
            await Launcher.LaunchUriAsync(uri);
        }

        public async void SendEmail()
        {
            var emailMessage = new Windows.ApplicationModel.Email.EmailMessage();
            emailMessage.Subject = AppConstants.AppName;
            emailMessage.Body = "";
            emailMessage.To.Add(new Windows.ApplicationModel.Email.EmailRecipient(AppConstants.DeveloperEmail));

            await Windows.ApplicationModel.Email.EmailManager.ShowComposeNewEmailAsync(emailMessage);
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
