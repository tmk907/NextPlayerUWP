using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NextPlayerUWP.ViewModels.Settings
{
    public class SettingsLibraryViewModel : Template10.Mvvm.ViewModelBase, ISettingsViewModel
    {
        public SettingsLibraryViewModel()
        {
            isLoaded = false;
        }

        public void Load()
        {
            OnLoaded();
        }

        private bool isLoaded;
        private async Task OnLoaded()
        {
            if (isLoaded) return;

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
            foreach (var item in ignoredList)
            {
                IgnoredArticles += item;
                IgnoredArticles += " ,";
            }

            isLoaded = true;
        }

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
                MusicLibraryFolders.Add(new SdCardFolder() { Name = newFolder.DisplayName, Path = newFolder.Path, IncludeSubFolders = true });
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
                if (isLoaded) ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.IgnoreArticles, value);
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
            if (isLoaded)
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
    }
}
