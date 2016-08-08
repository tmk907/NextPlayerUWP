﻿using NextPlayerUWP.Common;
using NextPlayerUWPDataLayer.CloudStorage;
using NextPlayerUWPDataLayer.Helpers;
using NextPlayerUWPDataLayer.Model;
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
    public class CloudStorageFoldersViewModel : MusicViewModelBase
    {
        private string pageHeader = "";
        public string PageHeader
        {
            get { return pageHeader; }
            set { Set(ref pageHeader, value); }
        }

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

        private CloudFolder currentFolder;

        private bool loading = false;
        public bool Loading
        {
            get { return loading; }
            set { Set(ref loading, value); }
        }

        string folderId = "";
        ICloudStorageService service;

        protected override async Task LoadData()
        {
            Loading = true;
            currentFolder = null;
            if (folderId == null)
            {
                var loggedIn = await service.LoginSilently();
                if (!loggedIn)
                {
                    Items.Clear();
                    Loading = false;
                    return;
                }
                folderId = await service.GetRootFolderId();
                if (folderId == null)
                {
                    Items.Clear();
                    Loading = false;
                    return;
                }
            }

            currentFolder = await service.GetFolder(folderId);

            FolderName = currentFolder?.Folder ?? "";

            var folders = await service.GetSubFolders(folderId);
            var songs = await service.GetSongItems(folderId);
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
            string param = parameter as string ?? "";
            if (CloudRootFolder.IsCloudRootFolderParameter(param))
            {
                Items.Clear();
                string userId = CloudRootFolder.ParameterToUserId(param);
                var type = CloudRootFolder.ParameterToType(param);
                switch (type)
                {
                    case CloudStorageType.Dropbox:
                        PageHeader = "Dropbox";
                        break;
                    case CloudStorageType.GoogleDrive:
                        PageHeader = "Google Drive";
                        break;
                    case CloudStorageType.OneDrive:
                        PageHeader = "OneDrive";
                        break;
                    case CloudStorageType.pCloud:
                        PageHeader = "pCloud";
                        break;
                    default:
                        PageHeader = "Unknown Cloud Storage";
                        break;
                }
                CloudStorageServiceFactory factory = new CloudStorageServiceFactory();
                service = factory.GetService(type, userId);
                folderId = null;
            }
            else
            {
                folderId = param;
            }
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            items = new ObservableCollection<MusicItem>();
            //if (args.NavigationMode == NavigationMode.Back && FolderName != "OneDrive Music")//zmienic na spr parentid
            //{
            //    //FolderName.Substring(0,FolderName.LastIndexOf('\\'));
            //}
            await base.OnNavigatingFromAsync(args);
        }

        public async void ItemClicked(object sender, ItemClickEventArgs e)
        {
            if (typeof(SongItem) == e.ClickedItem.GetType())
            {
                await SongClicked(((SongItem)e.ClickedItem).SongId);
            }
            else if (typeof(CloudFolder) == e.ClickedItem.GetType())
            {
                var folder = ((CloudFolder)e.ClickedItem);
                //FolderName += @"\" + folder.Folder;
                NavigationService.Navigate(App.Pages.CloudStorageFolders, folder.Id);
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