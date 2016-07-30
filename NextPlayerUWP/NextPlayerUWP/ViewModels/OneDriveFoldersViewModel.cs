using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
using NextPlayerUWPDataLayer.OneDrive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NextPlayerUWP.ViewModels
{
    public class OneDriveFoldersViewModel : MusicViewModelBase
    {
        private string folderName = "";
        public string FolderName
        {
            get { return folderName; }
            set { Set(ref folderName, value); }
        }

        private ObservableCollection<MusicItem> items = new ObservableCollection<MusicItem>();
        public ObservableCollection<MusicItem> Items
        {
            get { return items; }
            set { Set(ref items, value); }
        }

        private OneDriveFolder currentFolder;

        private bool loading = false;
        public bool Loading
        {
            get { return loading; }
            set { Set(ref loading, value); }
        }

        string id = "";

        protected override async Task LoadData()
        {
            Loading = true;
            currentFolder = null;
            if (id == "")
            {
                currentFolder = await OneDriveManager.Instance.GetMusicFolder();
                id = currentFolder.Id;
            }
            else
            {
                foreach(var item in items)
                {
                    if (item.GetType() == typeof(OneDriveFolder))
                    {
                        if (((OneDriveFolder)item).Id == id)
                        {
                            currentFolder = (OneDriveFolder)item;
                            break;
                        }
                    }
                }
            }
            if (currentFolder == null)
            {
                currentFolder = await OneDriveManager.Instance.GetFolder(id);
            }

            FolderName = currentFolder?.Folder ?? "";

            var folders = await OneDriveManager.Instance.GetSubFoldersFromItem(id);
            var songs = await OneDriveManager.Instance.GetSongItemsFromItem(id);
            Items.Clear();
            foreach (var folder in folders)
            {
                Items.Add(folder);
            }
            foreach (var song in songs)
            {
                Items.Add(song);
            }

            Loading = false;
        }

        
        public override void ChildOnNavigatedTo(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            id = parameter as string ?? "";
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            items = new ObservableCollection<MusicItem>();
            if (args.NavigationMode == NavigationMode.Back && FolderName != "OneDrive Music")//zmienic na spr parentid
            {
                //FolderName.Substring(0,FolderName.LastIndexOf('\\'));
            }
            await base.OnNavigatingFromAsync(args);
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            if (typeof(SongItem) == e.ClickedItem.GetType())
            {
                await SongClicked(((SongItem)e.ClickedItem).SongId);
            }
            else if (typeof(OneDriveFolder) == e.ClickedItem.GetType())
            {
                var folder = ((OneDriveFolder)e.ClickedItem);
                FolderName += @"\" + folder.Folder;
                NavigationService.Navigate(App.Pages.OneDriveFolders, folder.Id);
            }
        }

        private async Task SongClicked(int songid)
        {
            int index = 0;
            int i = 0;
            List<SongItem> songs = new List<SongItem>();
            foreach (var item in items.Where(s => s.GetType() == typeof(SongItem)))
            {
                if (typeof(SongItem) == item.GetType())
                {
                    songs.Add((SongItem)item);
                    if (((SongItem)item).SongId == songid) index = i;
                    i++;
                }
            }
            await NowPlayingPlaylistManager.Current.NewPlaylist(songs);
            ApplicationSettingsHelper.SaveSongIndex(index);
            App.PlaybackManager.PlayNew();
        }
    }
}
