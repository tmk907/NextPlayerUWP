using CommunityToolkit.Mvvm.ComponentModel;
using NextPlayerWinUI.Common;
using NextPlayerWinUIDataLayer.Helpers;
using NextPlayerWinUIDataLayer.Model;
using NextPlayerWinUIDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace NextPlayerWinUI.ViewModels.Settings
{
    public class SettingsLibraryViewModel : ObservableObject, ISettingsViewModel
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
            LastLibraryUpdate = ApplicationSettingsHelper.ReadSettingsValue<DateTimeOffset>(SettingsKeys.LibraryUpdatedAt).Date;
            if (ApplicationSettingsHelper.ReadSettingsValue(SettingsKeys.MediaScan) != null)
            {
                IsUpdating = true;
            }

            UseIndexer = ApplicationSettingsHelper.ReadSettingsValue<bool>(SettingsKeys.MediaScanUseIndexer);
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
            set { SetProperty(ref updateProgressText, value); }
        }

        private string scannedFolder = "";
        public string ScannedFolder
        {
            get { return scannedFolder; }
            set { SetProperty(ref scannedFolder, value); }
        }

        private bool updateProgressTextVisibility = false;
        public bool UpdateProgressTextVisibility
        {
            get { return updateProgressTextVisibility; }
            set { SetProperty(ref updateProgressTextVisibility, value); }
        }

        private bool isUpdating = false;
        public bool IsUpdating
        {
            get { return isUpdating; }
            set { SetProperty(ref isUpdating, value); }
        }

        private DateTime lastLibraryUpdate;
        public DateTime LastLibraryUpdate
        {
            get { return lastLibraryUpdate; }
            set { SetProperty(ref lastLibraryUpdate, value); }
        }

        private ObservableCollection<SdCardFolder> musicLibraryFolders = new ObservableCollection<SdCardFolder>();
        public ObservableCollection<SdCardFolder> MusicLibraryFolders
        {
            get { return musicLibraryFolders; }
            set { SetProperty(ref musicLibraryFolders, value); }
        }

        private bool useIndexer = false;
        public bool UseIndexer
        {
            get { return useIndexer; }
            set
            {
                SetProperty(ref useIndexer, value);
                if (isLoaded) ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.MediaScanUseIndexer, value);
            }
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

        /// <summary>
        /// Windows does not let desktop apps change the Music library, so added folders are kept in the app's own list.
        /// </summary>
        public async void AddFolder()
        {
            var folderPicker = WindowHelper.InitializeWithWindow(new Windows.Storage.Pickers.FolderPicker());
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            folderPicker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder == null || MusicLibraryFolders.Any(f => f.Path.Equals(folder.Path, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            bool includeSubFolders = await new MessageDialogHelper().ShowIncludeAllSubFoldersQuestion();
            MusicLibraryFolders.Add(new SdCardFolder()
            {
                Name = folder.Name,
                Path = folder.Path,
                IncludeSubFolders = includeSubFolders,
                IsRemovable = true,
            });
            await SaveAddedFolders();
        }

        public async Task RemoveFolder(SdCardFolder musicFolder)
        {
            MusicLibraryFolders.Remove(musicFolder);
            await SaveAddedFolders();
            await AfterFolderDelete(musicFolder.Path);
        }

        private Task SaveAddedFolders()
            => ApplicationSettingsHelper.SaveSdCardFoldersToScan(MusicLibraryFolders.Where(f => f.IsRemovable).ToList());

        private async Task AfterFolderDelete(string folderPath)
        {
            await DatabaseManager.Current.DeleteFolderAndSubFoldersAsync(folderPath);
            MediaImport.OnMediaImported("FolderRemoved");
        }

        private async Task<List<SdCardFolder>> GetMusicFolders()
        {
            List<SdCardFolder> list = new List<SdCardFolder>();
            var lib = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
            foreach (var f in lib.Folders)
            {
                list.Add(new SdCardFolder { Name = f.DisplayName, Path = f.Path, IncludeSubFolders = true });
            }
            var added = await ApplicationSettingsHelper.GetSdCardFoldersToScan() ?? new List<SdCardFolder>();
            foreach (var f in added)
            {
                f.IsRemovable = true;
                list.Add(f);
            }
            return list;
        }

        private bool ignoreArticles = false;
        public bool IgnoreArticles
        {
            get { return ignoreArticles; }
            set
            {
                SetProperty(ref ignoreArticles, value);
                if (isLoaded) ApplicationSettingsHelper.SaveSettingsValue(SettingsKeys.IgnoreArticles, value);
            }
        }

        private string ignoredArticles = "";
        public string IgnoredArticles
        {
            get { return ignoredArticles; }
            set
            {
                SetProperty(ref ignoredArticles, value);
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
