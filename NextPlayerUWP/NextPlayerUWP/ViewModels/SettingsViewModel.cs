using NextPlayerUWPDataLayer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
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
        private string updateProgressText = "";
        public string UpdateProgressText
        {
            get { return updateProgressText; }
            set { Set(ref updateProgressText, value); }
        }

        private ObservableCollection<MusicFolder> musicLibraryFolders = new ObservableCollection<MusicFolder>();
        public ObservableCollection<MusicFolder> MusicLibraryFolders
        {
            get { return musicLibraryFolders; }
            set { Set(ref musicLibraryFolders, value); }
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (musicLibraryFolders.Count == 0)
            {
                var lib = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
                foreach (var f in lib.Folders)
                {
                    MusicLibraryFolders.Add(new MusicFolder() { Name = f.DisplayName, Path = f.Path });
                }
            }
            
        }

        public async void UpdateLibrary()
        {
            MediaImport m = new MediaImport();
            Progress<int> progress = new Progress<int>(
            percent =>
            {
                UpdateProgressText = percent.ToString();
            });
            await Task.Run(() => m.UpdateDatabase(progress));
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

        public async void RemoveFolder()
        {
            var musicLibrary = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Music);
            var f = musicLibrary.Folders.FirstOrDefault();
            var fi = await f.GetFilesAsync();
            var fp = fi.FirstOrDefault().Properties;
            var mp =  await fp.GetMusicPropertiesAsync();
            
            //bool result = await musicLibrary.RequestRemoveFolderAsync(musicLibrary.Folders.LastOrDefault().);
            //MusicLibraryFolders.Remove();
        }
    }
}
